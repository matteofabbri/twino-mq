using System.Threading.Tasks;
using Twino.MQ.Clients;
using Twino.MQ.Events;
using Twino.MQ.Queues;
using Twino.MQ.Security;
using Twino.Protocols.TMQ;

namespace Twino.MQ.Network
{
    internal class EventMessageHandler : INetworkMessageHandler
    {
        #region Fields

        /// <summary>
        /// Messaging Queue Server
        /// </summary>
        private readonly TwinoMQ _server;

        public EventMessageHandler(TwinoMQ server)
        {
            _server = server;
        }

        #endregion

        private static async Task SendResponse(MqClient client, TwinoMessage message, bool successful)
        {
            ushort contentType = successful ? (ushort) TwinoResultCode.Ok : (ushort) TwinoResultCode.Failed;
            TwinoMessage response = new TwinoMessage(MessageType.Response, client.UniqueId, contentType);
            response.SetMessageId(message.MessageId);
            await client.SendAsync(response);
        }

        public Task Handle(MqClient client, TwinoMessage message, bool fromNode)
        {
            string eventName = message.Target;
            string queueName = message.FindHeader(TwinoHeaders.QUEUE_NAME);
            bool subscribe = message.ContentType == 1;

            TwinoQueue queue = !string.IsNullOrEmpty(queueName) ? _server.FindQueue(queueName) : null;
            if (subscribe)
            {
                foreach (IClientAuthorization authorization in _server.Authorizations)
                {
                    if (!authorization.CanSubscribeEvent(client, queue))
                        return SendResponse(client, message, false);
                }
            }

            switch (eventName)
            {
                case EventNames.MessageProduced:
                    if (queue == null)
                        return SendResponse(client, message, false);

                    if (subscribe)
                        queue.OnMessageProduced.Subscribe(client);
                    else
                        queue.OnMessageProduced.Unsubscribe(client);

                    return SendResponse(client, message, true);

                case EventNames.ClientConnected:
                    if (subscribe)
                        _server.OnClientConnected.Subscribe(client);
                    else
                        _server.OnClientConnected.Unsubscribe(client);

                    return SendResponse(client, message, true);

                case EventNames.ClientDisconnected:
                    if (subscribe)
                        _server.OnClientDisconnected.Subscribe(client);
                    else
                        _server.OnClientDisconnected.Unsubscribe(client);

                    return SendResponse(client, message, true);

                case EventNames.Subscribe:
                    if (queue == null)
                        return SendResponse(client, message, false);

                    if (subscribe)
                        queue.OnConsumerSubscribed.Subscribe(client);
                    else
                        queue.OnConsumerSubscribed.Unsubscribe(client);

                    return SendResponse(client, message, true);

                case EventNames.Unsubscribe:
                    if (queue == null)
                        return SendResponse(client, message, false);

                    if (subscribe)
                        queue.OnConsumerUnsubscribed.Subscribe(client);
                    else
                        queue.OnConsumerUnsubscribed.Unsubscribe(client);

                    return SendResponse(client, message, true);

                case EventNames.QueueCreated:
                    if (subscribe)
                        _server.OnQueueCreated.Subscribe(client);
                    else
                        _server.OnQueueCreated.Unsubscribe(client);

                    return SendResponse(client, message, true);

                case EventNames.QueueUpdated:
                    if (subscribe)
                        _server.OnQueueUpdated.Subscribe(client);
                    else
                        _server.OnQueueUpdated.Unsubscribe(client);

                    return SendResponse(client, message, true);

                case EventNames.QueueRemoved:
                    if (subscribe)
                        _server.OnQueueRemoved.Subscribe(client);
                    else
                        _server.OnQueueRemoved.Unsubscribe(client);

                    return SendResponse(client, message, true);
            }

            return Task.CompletedTask;
        }
    }
}