using Microsoft.Extensions.DependencyInjection;

namespace ECommerceSample.DataAccess.EFCore
{
	public static class Extensions
	{
		public static void RegisterRepository<T>(this IServiceCollection services) where T : class
		{
			services.AddScoped<ICommandRepository<T>, CommandRepository<T>>();
			services.AddScoped<IQueryRepository<T>, QueryRepository<T>>();
		}
	}
}