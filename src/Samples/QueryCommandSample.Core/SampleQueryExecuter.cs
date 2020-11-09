using System;
using System.Threading.Tasks;
using QueryCommandSample.Core.Interfaces;
using Twino.MQ.Client;
using Twino.Protocols.TMQ;

namespace QueryCommandSample.Core
{
	internal abstract class SampleQueryExecuter<TQuery, TResult> : ITwinoRequestHandler<TQuery, TResult> where TQuery : ISampleQuery
	{
		public Task<TResult> Handle(TQuery request, TwinoMessage rawMessage, TmqClient client)
		{
			throw new NotImplementedException();
		}

		protected abstract Task Execute(TQuery query);

		public Task<ErrorResponse> OnError(Exception exception, TQuery request, TwinoMessage rawMessage, TmqClient client)
		{
			throw new NotImplementedException();
		}
	}
}