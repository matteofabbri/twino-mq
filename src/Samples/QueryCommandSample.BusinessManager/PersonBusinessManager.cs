using QueryCommandSample.Domain;

namespace QueryCommandSample.BusinessManager
{
	public class PersonBusinessManager
	{
		public Person Create(string name, string lastname)
		{
			return new Person
			{
				Name = name,
				Lastname = lastname
			};
		}
	}
}