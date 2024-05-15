using StronglyTypedIds;

namespace Primal.Domain.Investments;

[StronglyTypedId(converters: StronglyTypedIdConverter.SystemTextJson)]
public partial struct StockId
{
}
