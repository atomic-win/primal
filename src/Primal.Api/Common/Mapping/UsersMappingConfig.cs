using Mapster;
using Primal.Contracts.Users;
using Primal.Domain.Users;

namespace Primal.Api.Common.Mapping;

internal sealed class UsersMappingConfig : IRegister
{
	public void Register(TypeAdapterConfig config)
	{
		config.NewConfig<Guid, UserId>()
			.ConstructUsing((guid) => new UserId(guid));

		config.NewConfig<UserId, Guid>()
			.ConstructUsing((userId) => userId.Value);

		config.NewConfig<User, UserProfileResponse>();
	}
}
