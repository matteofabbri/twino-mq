using System.Threading.Tasks;
using QueryCommandSample.BusinessManager;
using QueryCommandSample.Domain;
using QueryCommandSample.Models.Commands;
using Twino.Client.TMQ;
using Twino.Protocols.TMQ;

namespace QueryCommandSample.CommandExecuter
{
	public class AddPersonCommandHandler : IQueueConsumer<AddPersonCommand>
	{
		public async Task Consume(TwinoMessage message, AddPersonCommand model, TmqClient client)
		{
			PersonBusinessManager bm = new PersonBusinessManager();
			Person person = bm.Create(model.Name, model.Lastname);
			WritePersonToDb dbWriterCommand = new WritePersonToDb
			{
				Person = person
			};
			TwinoMessage requestMessage = new TwinoMessage();
			requestMessage.Serialize(dbWriterCommand, new NewtonsoftContentSerializer());
			TwinoResult result = await client.SendAsync(requestMessage);

			await client.SendResponseAsync(message, result.Message.Content);
		}
	}
}