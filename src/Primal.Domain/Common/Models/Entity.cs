namespace Primal.Domain.Common.Models;

public abstract class Entity<TId> : IEquatable<Entity<TId>>
	where TId : notnull
{
	protected Entity(TId id)
	{
		this.Id = id;
	}

	public TId Id { get; init; }

	public static bool operator ==(Entity<TId> left, Entity<TId> right)
	{
		return left?.Equals(right) ?? right is null;
	}

	public static bool operator !=(Entity<TId> left, Entity<TId> right)
	{
		return !(left == right);
	}

	public bool Equals(Entity<TId> other)
	{
		return this.Id.Equals(other.Id);
	}

	public override bool Equals(object obj)
	{
		return obj is Entity<TId> entity && this.Equals(entity);
	}

	public override int GetHashCode()
	{
		return this.Id.GetHashCode();
	}
}
