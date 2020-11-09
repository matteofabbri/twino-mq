using System.Threading.Tasks;
using QueryCommandSample.BusinessManager;
using QueryCommandSample.Domain;
using QueryCommandSample.Models.Commands;
using Twino.MQ.Client;
using Twino.Protocols.TMQ;

namespace QueryCommandSample.CommandExecuter
{
	public class AddPersonCommandHandler : IQueueConsumer<AddPersonCommand>
	{
		public Task Consume(TwinoMessage message, AddPersonCommand model, TmqClient client)
		{
			PersonBusinessManager bm = new PersonBusinessManager();
			Person person = bm.Create(model.Name, model.Lastname);
			return Task.CompletedTask;
		}
	}
}