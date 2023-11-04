using StronglyTypedIds;

namespace Primal.Domain.Users;

[StronglyTypedId(converters: StronglyTypedIdConverter.SystemTextJson)]
public partial struct UserId
{
}
