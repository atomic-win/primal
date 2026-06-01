using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Primal.E2ETests;

internal static class HttpResponseMessageExtensions
{
	private static readonly JsonSerializerOptions JsonOptions = new()
	{
		PropertyNameCaseInsensitive = true,
		Converters = { new JsonStringEnumConverter() },
	};

	internal static async Task<T> ReadJsonAsync<T>(this HttpResponseMessage response)
	{
		response.EnsureSuccessStatusCode();
		var result = await response.Content.ReadFromJsonAsync<T>(JsonOptions);
		return result!;
	}
}
