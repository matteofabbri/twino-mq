using System;
using System.Threading.Tasks;
using System.Windows.Input;
using Twino.MQ.Client;
using Twino.MQ.Client.Annotations;
using Twino.Protocols.TMQ;

namespace QueryCommandSample.Core
{
	[AutoAck]
	[AutoNack(NackReason.ExceptionMessage)]
	internal abstract class SampleCommandExecuter<TCommand> : IDirectConsumer<TCommand> where TCommand : ICommand
	{
		public Task Consume(TwinoMessage message, TCommand model, TmqClient client)
		{
			throw new NotImplementedException();
		}

		protected abstract Task Execute(TCommand command);
	}
}