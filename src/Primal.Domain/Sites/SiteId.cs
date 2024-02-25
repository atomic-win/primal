using StronglyTypedIds;

namespace Primal.Domain.Sites;

[StronglyTypedId(converters: StronglyTypedIdConverter.SystemTextJson)]
public partial struct SiteId
{
}
