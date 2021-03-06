using System;
using System.Threading.Tasks;
using Test.Bus.Models;
using Twino.MQ.Client;
using Twino.MQ.Client.Annotations;
using Twino.Protocols.TMQ;

namespace Test.Bus.Consumers
{
    [AutoAck]
    [AutoNack]
    [PushExceptions(typeof(ExceptionModel1))]
    [PushExceptions(typeof(ExceptionModel2), typeof(NotSupportedException))]
    [PushExceptions(typeof(ExceptionModel3), typeof(InvalidOperationException))]
    public class QueueConsumer3 : IQueueConsumer<Model3>
    {
        public int Count { get; private set; }

        public static QueueConsumer3 Instance { get; private set; }

        public QueueConsumer3()
        {
            Instance = this;
        }

        public Task Consume(TwinoMessage message, Model3 model, TmqClient client)
        {
            Count++;

            if (Count == 2)
                throw new NotSupportedException();

            if (Count == 3)
                throw new InvalidOperationException();

            throw new InvalidCastException();
        }
    }
}