namespace Primal.Infrastructure.Persistence;

internal sealed class UserTableEntity : TableEntity
{
	public string Id { get; set; } = null!;

	public string Email { get; set; } = null!;

	public string FirstName { get; set; } = null!;

	public string LastName { get; set; } = null!;

	public string FullName { get; set; } = null!;

	public string PreferredCurrency { get; set; } = null!;

	public string PreferredLocale { get; set; } = null!;
}
