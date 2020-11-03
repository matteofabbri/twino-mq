using System;
using System.Threading.Tasks;
using QueryCommandSample.DataAccess;
using QueryCommandSample.Models.Commands;
using Twino.Client.TMQ;
using Twino.Protocols.TMQ;

namespace QueryCommandSample.DatabaseWriter
{
	public class WritePersonToDbHandler : IQueueConsumer<WritePersonToDb>
	{
		public async Task Consume(TwinoMessage message, WritePersonToDb model, TmqClient client)
		{
			await using SampleContext context = new SampleContext();
			await context.Persons.AddAsync(model.Person);
			_ = await context.SaveChangesAsync();
			await client.SendResponseAsync(message, model.Person);
		}
	}
}