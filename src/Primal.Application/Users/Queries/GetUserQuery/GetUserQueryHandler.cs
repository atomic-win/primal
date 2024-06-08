using ErrorOr;
using MediatR;
using Primal.Application.Common.Interfaces.Persistence;
using Primal.Domain.Users;

namespace Primal.Application.Users;

internal sealed class GetUserQueryHandler : IRequestHandler<GetUserQuery, ErrorOr<User>>
{
	private readonly IUserRepository userRepository;

	public GetUserQueryHandler(IUserRepository userRepository)
	{
		this.userRepository = userRepository;
	}

	public async Task<ErrorOr<User>> Handle(GetUserQuery request, CancellationToken cancellationToken)
	{
		return await this.userRepository.GetUserAsync(request.UserId, cancellationToken);
	}
}
