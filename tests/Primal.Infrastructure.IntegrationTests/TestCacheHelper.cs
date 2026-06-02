using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.DependencyInjection;

namespace Primal.Infrastructure.IntegrationTests;

internal static class TestCacheHelper
{
	internal static HybridCache CreateHybridCache()
	{
		var services = new ServiceCollection();
		services.AddHybridCache();
		var provider = services.BuildServiceProvider();
		return provider.GetRequiredService<HybridCache>();
	}
}
