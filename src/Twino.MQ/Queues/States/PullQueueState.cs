using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Twino.MQ.Clients;
using Twino.MQ.Delivery;
using Twino.MQ.Helpers;
using Twino.Protocols.TMQ;

namespace Twino.MQ.Queues.States
{
    internal enum ClearDecision
    {
        None,
        All,
        HighPriority,
        DefaultPriority
    }

    internal class PullQueueState : IQueueState
    {
        public QueueMessage ProcessingMessage { get; private set; }
        public bool TriggerSupported => false;

        private readonly TwinoQueue _queue;

        public PullQueueState(TwinoQueue queue)
        {
            _queue = queue;
        }

        /// <summary>
        /// Reads Count header value and returns as integer
        /// </summary>
        private static int FindCount(TwinoMessage request)
        {
            string countStr = request.FindHeader(TwinoHeaders.COUNT);

            if (string.IsNullOrEmpty(countStr))
                return 0;

            return Convert.ToInt32(countStr.Trim());
        }

        /// <summary>
        /// Reads Clear header value and returns as enum
        /// </summary>
        private static ClearDecision FindClearDecision(TwinoMessage request)
        {
            string clearStr = request.FindHeader(TwinoHeaders.CLEAR);

            if (string.IsNullOrEmpty(clearStr))
                return ClearDecision.None;

            clearStr = clearStr.Trim();

            if (clearStr.Equals("all", StringComparison.InvariantCultureIgnoreCase))
                return ClearDecision.All;

            if (clearStr.Equals("high-priority", StringComparison.InvariantCultureIgnoreCase))
                return ClearDecision.HighPriority;

            if (clearStr.Equals("default-priority", StringComparison.InvariantCultureIgnoreCase))
                return ClearDecision.DefaultPriority;

            return ClearDecision.None;
        }

        /// <summary>
        /// Reads Info header value and returns as boolean
        /// </summary>
        private static bool FindInfoRequest(TwinoMessage request)
        {
            string infoStr = request.FindHeader(TwinoHeaders.INFO);
            return !string.IsNullOrEmpty(infoStr) && infoStr.Trim().Equals("Yes", StringComparison.InvariantCultureIgnoreCase);
        }

        /// <summary>
        /// Reads Order header value and returns as true false.
        /// If true, it's fifo (as default).
        /// If false, it's lifo.
        /// </summary>
        private static bool FindOrder(TwinoMessage request)
        {
            string orderStr = request.FindHeader(TwinoHeaders.ORDER);
            if (string.IsNullOrEmpty(orderStr))
                return true;

            bool lifo = orderStr.Trim().Equals(TwinoHeaders.LIFO, StringComparison.InvariantCultureIgnoreCase);
            return !lifo;
        }

        public async Task<PullResult> Pull(QueueClient client, TwinoMessage request)
        {
            int index = 1;
            int count = FindCount(request);
            if (count < 1)
            {
                await client.Client.SendAsync(MessageBuilder.CreateNoContentPullResponse(request, TwinoHeaders.UNACCEPTABLE));
                return PullResult.Unacceptable;
            }

            ClearDecision clear = FindClearDecision(request);
            bool sendInfo = FindInfoRequest(request);
            bool fifo = FindOrder(request);

            Tuple<QueueMessage, int, int> messageTuple = await DequeueMessage(fifo, sendInfo, count == index ? clear : ClearDecision.None);

            //there is no pullable message
            if (messageTuple.Item1 == null)
            {
                await client.Client.SendAsync(MessageBuilder.CreateNoContentPullResponse(request, TwinoHeaders.EMPTY));
                return PullResult.Empty;
            }

            List<KeyValuePair<string, string>> headers = new List<KeyValuePair<string, string>>();
            KeyValuePair<string, string> requestId = new KeyValuePair<string, string>(TwinoHeaders.REQUEST_ID, request.MessageId);
            KeyValuePair<string, string> countHeader = new KeyValuePair<string, string>(TwinoHeaders.COUNT, count.ToString());

            while (index <= count)
            {
                QueueMessage message = messageTuple.Item1;
                if (message == null)
                    break;

                try
                {
                    headers.Add(requestId);
                    headers.Add(countHeader);
                    headers.Add(new KeyValuePair<string, string>(TwinoHeaders.INDEX, index.ToString()));

                    if (sendInfo)
                    {
                        headers.Add(new KeyValuePair<string, string>(TwinoHeaders.PRIORITY_MESSAGES, messageTuple.Item2.ToString()));
                        headers.Add(new KeyValuePair<string, string>(TwinoHeaders.MESSAGES, messageTuple.Item3.ToString()));
                    }

                    bool processed = await ProcessPull(client, request, message, headers);

                    if (!processed)
                        break;

                    index++;
                    messageTuple = await DequeueMessage(fifo, sendInfo, count == index ? clear : ClearDecision.None);
                    headers.Clear();
                }
                catch (Exception ex)
                {
                    _queue.Info.AddError();
                    try
                    {
                        Decision decision = await _queue.DeliveryHandler.ExceptionThrown(_queue, message, ex);
                        await _queue.ApplyDecision(decision, message);

                        if (!message.IsInQueue)
                        {
                            if (decision.PutBack == PutBackDecision.Start)
                                _queue.AddMessage(message, false);
                            else if (decision.PutBack == PutBackDecision.End)
                                _queue.AddMessage(message);
                        }
                    }
                    catch //if developer does wrong operation, we should not stop
                    {
                    }
                }
            }

            await client.Client.SendAsync(MessageBuilder.CreateNoContentPullResponse(request, TwinoHeaders.END));
            return PullResult.Success;
        }

        /// <summary>
        /// Finds and dequeues a message from the queue.
        /// If there is not message, returns null
        /// </summary>
        private async Task<Tuple<QueueMessage, int, int>> DequeueMessage(bool fifo, bool getInfo, ClearDecision clear)
        {
            QueueMessage message = null;
            int prioMessageCount = 0;
            int messageCount = 0;

            await _queue.RunInListSync(() =>
            {
                //pull from prefential messages
                if (_queue.PriorityMessagesList.Count > 0)
                {
                    if (fifo)
                    {
                        // ReSharper disable once PossibleNullReferenceException
                        message = _queue.PriorityMessagesList.First.Value;
                        _queue.PriorityMessagesList.RemoveFirst();
                    }
                    else
                    {
                        // ReSharper disable once PossibleNullReferenceException
                        message = _queue.PriorityMessagesList.Last.Value;
                        _queue.PriorityMessagesList.RemoveLast();
                    }

                    if (message != null)
                        message.IsInQueue = false;
                }

                //if there is no prefential message, pull from standard messages
                if (message == null && _queue.MessagesList.Count > 0)
                {
                    if (fifo)
                    {
                        // ReSharper disable once PossibleNullReferenceException
                        message = _queue.MessagesList.First.Value;
                        _queue.MessagesList.RemoveFirst();
                    }
                    else
                    {
                        // ReSharper disable once PossibleNullReferenceException
                        message = _queue.MessagesList.Last.Value;
                        _queue.MessagesList.RemoveLast();
                    }

                    if (message != null)
                        message.IsInQueue = false;
                }

                if (getInfo)
                {
                    prioMessageCount = _queue.PriorityMessageCount();
                    messageCount = _queue.MessageCount();
                }

                if (clear == ClearDecision.All)
                    _queue.ClearAllMessages();

                else if (clear == ClearDecision.HighPriority)
                    _queue.ClearHighPriorityMessages();

                else if (clear == ClearDecision.DefaultPriority)
                    _queue.ClearRegularMessages();
            });

            return new Tuple<QueueMessage, int, int>(message, prioMessageCount, messageCount);
        }

        /// <summary>
        /// Process pull request and sends queue message to requester as response
        /// </summary>
        private async Task<bool> ProcessPull(QueueClient requester, TwinoMessage request, QueueMessage message, IList<KeyValuePair<string, string>> headers)
        {
            //if we need acknowledge, we are sending this information to receivers that we require response
            message.Message.WaitResponse = _queue.Options.Acknowledge != QueueAckDecision.None;

            //if we need acknowledge from receiver, it has a deadline.
            DateTime? deadline = null;
            if (_queue.Options.Acknowledge != QueueAckDecision.None)
                deadline = DateTime.UtcNow.Add(_queue.Options.AcknowledgeTimeout);

            //if to process next message is requires previous message acknowledge, wait here
            if (_queue.Options.Acknowledge == QueueAckDecision.WaitForAcknowledge)
                await _queue.WaitForAcknowledge(message);

            if (message.CurrentDeliveryReceivers.Count > 0)
                message.CurrentDeliveryReceivers.Clear();

            message.Decision = await _queue.DeliveryHandler.BeginSend(_queue, message);
            if (!await _queue.ApplyDecision(message.Decision, message))
                return false;

            //call before send and check decision
            message.Decision = await _queue.DeliveryHandler.CanConsumerReceive(_queue, message, requester.Client);
            if (!await _queue.ApplyDecision(message.Decision, message))
                return false;

            //create delivery object
            MessageDelivery delivery = new MessageDelivery(message, requester, deadline);
            bool sent = await requester.Client.SendAsync(message.Message, headers);

            if (sent)
            {
                message.CurrentDeliveryReceivers.Add(requester);
                _queue.TimeKeeper.AddAcknowledgeCheck(delivery);
                delivery.MarkAsSent();

                //do after send operations for per message
                _queue.Info.AddDelivery();
                message.Decision = await _queue.DeliveryHandler.ConsumerReceived(_queue, delivery, requester.Client);

                //after all sending operations completed, calls implementation send completed method and complete the operation
                _queue.Info.AddMessageSend();

                if (!await _queue.ApplyDecision(message.Decision, message))
                    return false;
            }
            else
            {
                message.Decision = await _queue.DeliveryHandler.ConsumerReceiveFailed(_queue, delivery, requester.Client);
                if (!await _queue.ApplyDecision(message.Decision, message))
                    return false;
            }

            message.Decision = await _queue.DeliveryHandler.EndSend(_queue, message);
            await _queue.ApplyDecision(message.Decision, message);

            if (message.Decision.Allow && message.Decision.PutBack == PutBackDecision.No)
            {
                _queue.Info.AddMessageRemove();
                _ = _queue.DeliveryHandler.MessageDequeued(_queue, message);
            }

            return true;
        }

        public bool CanEnqueue(QueueMessage message)
        {
            return true;
        }

        public Task<PushResult> Push(QueueMessage message)
        {
            return Task.FromResult(PushResult.Success);
        }

        public Task<QueueStatusAction> EnterStatus(QueueStatus previousStatus)
        {
            return Task.FromResult(QueueStatusAction.Allow);
        }

        public Task<QueueStatusAction> LeaveStatus(QueueStatus nextStatus)
        {
            return Task.FromResult(QueueStatusAction.Allow);
        }
    }
}