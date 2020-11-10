using Microsoft.EntityFrameworkCore;
using ProductService.Domain;

namespace ProductService.DataAccess
{
	public class ProductEntities : DbContext
	{
		public DbSet<Product> Products { get; set; }

		public DbSet<ProductCategory> Categories { get; set; }

		protected override void OnConfiguring(DbContextOptionsBuilder builder)
		{
			builder.UseInMemoryDatabase("productEntities");
			base.OnConfiguring(builder);
		}
	}
}