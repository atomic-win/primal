using StronglyTypedIds;

namespace Primal.Domain.Users;

[StronglyTypedId(backingType: StronglyTypedIdBackingType.String, converters: StronglyTypedIdConverter.SystemTextJson)]
public partial struct IdentityUserId
{
}
