using System.Net.Http.Json;
using System.Text.Json.Serialization;
using ErrorOr;
using Primal.Application.Common.Interfaces.Investments;
using Primal.Domain.Investments;
using Primal.Domain.Money;

namespace Primal.Infrastructure.Investments;

internal sealed class MutualFundApiClient : IMutualFundApiClient
{
	private readonly HttpClient httpClient;

	public MutualFundApiClient(HttpClient httpClient)
	{
		this.httpClient = httpClient;
	}

	public async Task<ErrorOr<MutualFund>> GetBySchemeCodeAsync(int schemeCode, CancellationToken cancellationToken)
	{
		var apiResponse = await this.httpClient.GetFromJsonAsync<MutualFundApiResponse>(
			$"/mf/{schemeCode}/latest",
			cancellationToken: cancellationToken);

		if (apiResponse == null)
		{
			return Error.NotFound();
		}

		if (!string.Equals(apiResponse.Status, "SUCCESS", StringComparison.OrdinalIgnoreCase))
		{
			return Error.Unexpected();
		}

		return new MutualFund(
			new MutualFundId(Guid.Empty),
			apiResponse.Meta.SchemeName,
			apiResponse.Meta.FundHouse,
			apiResponse.Meta.SchemeType,
			apiResponse.Meta.SchemeCategory,
			apiResponse.Meta.SchemeCode,
			Currency.INR);
	}

	private sealed class MutualFundApiResponse
	{
		public Meta Meta { get; set; }

		public List<Data> Data { get; set; }

		public string Status { get; set; }
	}

	private sealed class Meta
	{
		[JsonPropertyName("fund_house")]
		public string FundHouse { get; set; }

		[JsonPropertyName("scheme_type")]
		public string SchemeType { get; set; }

		[JsonPropertyName("scheme_category")]
		public string SchemeCategory { get; set; }

		[JsonPropertyName("scheme_code")]
		public int SchemeCode { get; set; }

		[JsonPropertyName("scheme_name")]
		public string SchemeName { get; set; }
	}

	private sealed class Data
	{
		public string Date { get; set; }

		public string Nav { get; set; }
	}
}
