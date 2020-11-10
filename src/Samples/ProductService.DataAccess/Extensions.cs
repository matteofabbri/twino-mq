using Microsoft.Extensions.DependencyInjection;
using ProductService.DataAccess.Uow;
using ProductService.Domain;
using ECommerceSample.DataAccess.EFCore;
using ECommerceSample.DataAccess.EFCore.UnitOfWork;

namespace ProductService.DataAccess
{
	public static class Extensions
	{
		public static void RegisterRequiredServices(this IServiceCollection services)
		{
			services.RegisterRepository<Product>();
			services.RegisterRepository<ProductCategory>();
			services.RegisterUnitOfWork<IProductEntitiesUow, ProductEntitiesUow>();
			services.AddDbContext<ProductEntities>(ServiceLifetime.Scoped);
		}
	}
}