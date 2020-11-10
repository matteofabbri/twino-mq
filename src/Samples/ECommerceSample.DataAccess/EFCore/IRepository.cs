using System.Linq;
using System.Threading.Tasks;

namespace ECommerceSample.DataAccess.EFCore
{
	public interface IRepository { }

	public interface IQueryRepository<T> : IRepository where T : class
	{
		public IQueryable<T> Entities { get; }
	}

	public interface ICommandRepository<T> : IRepository where T : class
	{
		public Task Add(T entity);
		public Task Update(T entity);
		public Task Remove(T entity);
	}
}