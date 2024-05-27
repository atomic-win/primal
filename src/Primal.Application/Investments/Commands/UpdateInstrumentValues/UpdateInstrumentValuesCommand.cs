using ErrorOr;
using MediatR;

namespace Primal.Application.Investments;

public sealed record UpdateInstrumentValuesCommand() : IRequest<ErrorOr<Success>>;
