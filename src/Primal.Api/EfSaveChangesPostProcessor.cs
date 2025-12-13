using FastEndpoints;
using Primal.Infrastructure.Persistence;

namespace Primal.Api;

internal sealed class EfSaveChangesPostProcessor<TReq, TRes>
	: IPostProcessor<TReq, TRes>
{
	public async Task PostProcessAsync(
		IPostProcessorContext<TReq, TRes> ctx,
		CancellationToken ct)
	{
		var appDbContext = ctx.HttpContext.RequestServices.GetRequiredService<AppDbContext>();

		// Only save if handler succeeded
		if (appDbContext.ChangeTracker.HasChanges())
		{
			await appDbContext.SaveChangesAsync(ct);
		}
	}
}
