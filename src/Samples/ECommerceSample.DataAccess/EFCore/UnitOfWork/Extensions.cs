using Microsoft.Extensions.DependencyInjection;

namespace ECommerceSample.DataAccess.EFCore.UnitOfWork
{
	public static class Extensions
	{
		public static void RegisterUnitOfWork<TInterface, IImplemantation>(this IServiceCollection services)
			where TInterface : class, IUnitOfWork
			where IImplemantation : UnitOfWork, TInterface
		{
			services.AddScoped<TInterface, IImplemantation>();
		}
	}
}