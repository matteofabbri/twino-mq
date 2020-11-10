using System;
using ECommerceSample.DataAccess.EFCore;
using ECommerceSample.DataAccess.EFCore.UnitOfWork;
using Microsoft.Extensions.DependencyInjection;
using ProductService.Domain;

namespace ProductService.DataAccess.Uow
{
	internal class ProductEntitiesUow : UnitOfWork, IProductEntitiesUow
	{
		private readonly ProductEntities _context;
		private readonly IServiceProvider _services;

		public ProductEntitiesUow(ProductEntities context, IServiceProvider services) : base(context)
		{
			_context = context;
			_services = services;
		}

		public ICommandRepository<Product> ProductCommands => _services.GetService<ICommandRepository<Product>>();
		public IQueryRepository<Product> ProductQueries => _services.GetService<IQueryRepository<Product>>();
		public ICommandRepository<ProductCategory> CategoryCommands => _services.GetService<ICommandRepository<ProductCategory>>();
		public IQueryRepository<ProductCategory> CategoryQueries => _services.GetService<IQueryRepository<ProductCategory>>();
	}
}