using System;
using System.Threading.Tasks;
using Twino.MQ.Client;
using Twino.MQ.Client.Annotations;
using Twino.Protocols.TMQ;

namespace ECommerceSample.Core
{
	[AutoAck]
	[AutoNack(NackReason.ExceptionMessage)]
	public abstract class QueueEventHandler<T> : IQueueConsumer<T>
	{
		public Task Consume(TwinoMessage message, T model, TmqClient client)
		{
			throw new NotImplementedException();
		}

		protected abstract Task Consume(T @event);
	}
}