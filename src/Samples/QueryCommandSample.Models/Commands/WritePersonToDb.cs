using QueryCommandSample.Domain;
using Twino.Client.TMQ.Annotations;

namespace QueryCommandSample.Models.Commands
{
	[RouterName("db-writer-router")]
	[QueueName("WRITE-PERSON-TO-DB-QUEUE")]
	public class WritePersonToDb
	{
		public Person Person { get; set; }
	}
}