using StronglyTypedIds;

namespace Primal.Domain.Investments;

[StronglyTypedId(backingType: StronglyTypedIdBackingType.String, converters: StronglyTypedIdConverter.SystemTextJson)]
public partial struct AccountId
{
}
