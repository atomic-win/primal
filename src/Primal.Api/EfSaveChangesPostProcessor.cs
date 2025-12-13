using FastEndpoints;
using Primal.Infrastructure.Persistence;

namespace Primal.Api;

internal sealed class EfSaveChangesPostProcessor<TReq, TRes>
	: IPostProcessor<TReq, TRes>
{
	private readonly AppDbContext appDbContext;

	public EfSaveChangesPostProcessor(AppDbContext appDbContext)
	{
		this.appDbContext = appDbContext;
	}

	public async Task PostProcessAsync(
		IPostProcessorContext<TReq, TRes> ctx,
		CancellationToken ct)
	{
		// Only save if handler succeeded
		if (this.appDbContext.ChangeTracker.HasChanges())
		{
			await this.appDbContext.SaveChangesAsync(ct);
		}
	}
}
