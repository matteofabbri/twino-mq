using System.Runtime.CompilerServices;
using ECommerceSample.DataAccess.EFCore;
using ECommerceSample.DataAccess.EFCore.UnitOfWork;
using ProductService.Domain;

[assembly: InternalsVisibleTo("ProductService.Client.Command.All")]
[assembly: InternalsVisibleTo("ProductService.Client.Query.All")]

namespace ProductService.DataAccess.Uow
{
	internal interface IProductEntitiesUow : IUnitOfWork
	{
		public ICommandRepository<Product> ProductCommands { get; }
		public IQueryRepository<Product> ProductQueries { get; }
		public ICommandRepository<ProductCategory> CategoryCommands { get; }
		public IQueryRepository<ProductCategory> CategoryQueries { get; }
	}
}