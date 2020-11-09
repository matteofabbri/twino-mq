using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using Twino.MQ.Client.Annotations;
using Twino.Protocols.TMQ;

namespace Twino.MQ.Client.Internal
{
    internal abstract class ConsumerExecuter
    {
        #region Properties

        protected bool SendAck { get; private set; }
        protected bool SendNack { get; private set; }
        protected NackReason NackReason { get; private set; }

        protected RetryAttribute Retry { get; private set; }

        protected string DefaultPushException { get; private set; }
        protected List<Tuple<Type, string>> PushExceptions { get; private set; }

        protected KeyValuePair<string, ushort> DefaultPublishException { get; private set; }
        protected List<Tuple<Type, KeyValuePair<string, ushort>>> PublishExceptions { get; private set; }

        #endregion

        #region Actions

        public virtual void Resolve(ModelTypeConfigurator defaultOptions = null)
        {
            if (defaultOptions == null)
                return;

            SendAck = defaultOptions.AutoAck;
            SendNack = defaultOptions.AutoNack;
            NackReason = defaultOptions.NackReason;
            Retry = defaultOptions.Retry;
            DefaultPublishException = defaultOptions.DefaultPublishException;
            PushExceptions = defaultOptions.PushExceptions;
            DefaultPublishException = defaultOptions.DefaultPublishException;
            PublishExceptions = defaultOptions.PublishExceptions;
        }

        public abstract Task Execute(TmqClient client, TwinoMessage message, object model);

        protected void ResolveAttributes(Type type, Type modelType)
        {
            if (!SendAck)
            {
                AutoAckAttribute ackAttribute = type.GetCustomAttribute<AutoAckAttribute>();
                SendAck = ackAttribute != null;
            }

            if (!SendNack)
            {
                AutoNackAttribute nackAttribute = type.GetCustomAttribute<AutoNackAttribute>();
                SendNack = nackAttribute != null;
                NackReason = nackAttribute != null ? nackAttribute.Reason : NackReason.None;
            }

            RetryAttribute retryAttr = type.GetCustomAttribute<RetryAttribute>();
            if (retryAttr != null)
                Retry = retryAttr;

            if (PushExceptions == null)
                PushExceptions = new List<Tuple<Type, string>>();
            
            IEnumerable<PushExceptionsAttribute> pushAttributes = type.GetCustomAttributes<PushExceptionsAttribute>(true);
            foreach (PushExceptionsAttribute attribute in pushAttributes)
            {
                if (attribute.ExceptionType == null)
                    DefaultPushException = attribute.QueueName;
                else
                    PushExceptions.Add(new Tuple<Type, string>(attribute.ExceptionType, attribute.QueueName));
            }

            if (PublishExceptions == null)
                PublishExceptions = new List<Tuple<Type, KeyValuePair<string, ushort>>>();

            IEnumerable<PublishExceptionsAttribute> publishAttributes = type.GetCustomAttributes<PublishExceptionsAttribute>(true);
            foreach (PublishExceptionsAttribute attribute in publishAttributes)
            {
                if (attribute.ExceptionType == null)
                    DefaultPublishException = new KeyValuePair<string, ushort>(attribute.RouterName, attribute.ContentType);
                else
                    PublishExceptions.Add(new Tuple<Type, KeyValuePair<string, ushort>>(attribute.ExceptionType,
                                                                                        new KeyValuePair<string, ushort>(attribute.RouterName, attribute.ContentType)));
            }
        }

        /// <summary>
        /// Sends negative ack
        /// </summary>
        protected Task SendNegativeAck(TwinoMessage message, TmqClient client, Exception exception)
        {
            string reason;
            switch (NackReason)
            {
                case NackReason.Error:
                    reason = TwinoHeaders.NACK_REASON_ERROR;
                    break;

                case NackReason.ExceptionType:
                    reason = exception.GetType().Name;
                    break;

                case NackReason.ExceptionMessage:
                    reason = exception.Message;
                    break;

                default:
                    reason = TwinoHeaders.NACK_REASON_NONE;
                    break;
            }

            return client.SendNegativeAck(message, reason);
        }

        protected async Task SendExceptions(TmqClient client, Exception exception)
        {
            if (PushExceptions.Count == 0 &&
                PublishExceptions.Count == 0 &&
                string.IsNullOrEmpty(DefaultPushException) &&
                string.IsNullOrEmpty(DefaultPublishException.Key))
                return;

            Type type = exception.GetType();
            string serialized = Newtonsoft.Json.JsonConvert.SerializeObject(exception);

            bool pushFound = false;
            foreach (Tuple<Type, string> tuple in PushExceptions)
            {
                if (tuple.Item1.IsAssignableFrom(type))
                {
                    await client.Queues.Push(tuple.Item2, serialized, false);
                    pushFound = true;
                }
            }

            if (!pushFound && !string.IsNullOrEmpty(DefaultPushException))
                await client.Queues.Push(DefaultPushException, serialized, false);

            bool publishFound = false;
            foreach (Tuple<Type, KeyValuePair<string, ushort>> tuple in PublishExceptions)
            {
                if (tuple.Item1.IsAssignableFrom(type))
                {
                    await client.Routers.Publish(tuple.Item2.Key, serialized, false, tuple.Item2.Value);
                    publishFound = true;
                }
            }

            if (!publishFound && !string.IsNullOrEmpty(DefaultPublishException.Key))
                await client.Routers.Publish(DefaultPublishException.Key, serialized, false, DefaultPublishException.Value);
        }

        #endregion
    }
}