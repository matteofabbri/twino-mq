using QueryCommandSample.Domain;
using Twino.Client.TMQ.Annotations;

namespace QueryCommandSample.Models.Commands
{
	[RouterName("command-executer-router")]
	[QueueName("ADD-PERSON-COMMAND-QUEUE")]
	public class AddPersonCommand
	{
		public string Name { get; set; }
		public string Lastname { get; set; }
	}
}