using System;
using System.Threading.Tasks;
using System.Transactions;
using Microsoft.EntityFrameworkCore;

namespace ECommerceSample.DataAccess.EFCore.UnitOfWork
{
	public interface IUnitOfWork : IDisposable
	{
		public Task Save();
		public void Commit();
	}

	public abstract class UnitOfWork : IUnitOfWork
	{
		private readonly DbContext _context;
		private readonly TransactionScope _transactionScope;

		protected UnitOfWork(DbContext context)
		{
			_context = context;
			_transactionScope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);
		}

		public async Task Save()
		{
			await _context.SaveChangesAsync();
		}

		public void Commit()
		{
			_transactionScope.Complete();
		}

		public void Dispose()
		{
			_context?.Dispose();
		}
	}
}