namespace Primal.Domain.Common.Models;

public abstract class ValueObject : IEquatable<ValueObject>
{
	public static bool operator ==(ValueObject left, ValueObject right)
	{
		return left?.Equals(right) ?? right is null;
	}

	public static bool operator !=(ValueObject left, ValueObject right)
	{
		return !(left == right);
	}

	public bool Equals(ValueObject other)
	{
		return this.GetEqualityComponents().SequenceEqual(other.GetEqualityComponents());
	}

	public override bool Equals(object obj)
	{
		return obj is not null && obj.GetType() == this.GetType() && this.Equals((ValueObject)obj);
	}

	public override int GetHashCode()
	{
		return this.GetEqualityComponents()
			.Select(x => x != null ? x.GetHashCode() : 0)
			.Aggregate((x, y) => x ^ y);
	}

	protected abstract IEnumerable<object> GetEqualityComponents();
}
