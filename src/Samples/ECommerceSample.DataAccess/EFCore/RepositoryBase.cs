using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace ECommerceSample.DataAccess.EFCore
{
	internal class CommandRepository<T> : ICommandRepository<T> where T : class
	{
		private readonly DbContext _context;

		protected CommandRepository(DbContext context)
		{
			_context = context;
		}

		public async Task Add(T entity)
		{
			await _context.Set<T>().AddAsync(entity);
		}

		public Task Update(T entity)
		{
			_context.Set<T>().Update(entity);
			return Task.CompletedTask;
		}

		public Task Remove(T entity)
		{
			_context.Set<T>().Remove(entity);
			return Task.CompletedTask;
		}
	}

	internal class QueryRepository<T> : IQueryRepository<T> where T : class
	{
		private readonly DbContext _context;

		protected QueryRepository(DbContext context)
		{
			_context = context;
		}

		public IQueryable<T> Entities => _context.Set<T>().AsQueryable();
	}
}