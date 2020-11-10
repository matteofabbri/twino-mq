using System;
using Microsoft.EntityFrameworkCore;
using ECommerceSample.Domain;

namespace QueryCommandSample.DataAccess
{
	public class SampleContext : DbContext
	{
		public DbSet<Person> Persons { get; set; }

		protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
		{
			optionsBuilder.UseInMemoryDatabase("sampleDB");
			base.OnConfiguring(optionsBuilder);
		}
	}
}