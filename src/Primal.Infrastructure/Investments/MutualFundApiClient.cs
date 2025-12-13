using System.Collections.Frozen;
using System.Collections.Immutable;
using System.Globalization;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json.Serialization;
using Primal.Application.Investments;
using Primal.Domain.Money;

namespace Primal.Infrastructure.Investments;

internal sealed class MutualFundApiClient : IMutualFundApiClient
{
	private readonly IHttpClientFactory httpClientFactory;

	public MutualFundApiClient(IHttpClientFactory httpClientFactory)
	{
		this.httpClientFactory = httpClientFactory;
	}

	public async Task<MutualFund> GetBySchemeCodeAsync(string schemeCode, CancellationToken cancellationToken)
	{
		var httpClient = this.httpClientFactory.CreateClient(nameof(MutualFundApiClient));
		var response = await httpClient.GetAsync($"/mf/{schemeCode}/latest", cancellationToken);

		if (response.StatusCode == HttpStatusCode.NotFound)
		{
			return new MutualFund(
				SchemeCode: string.Empty,
				Name: string.Empty,
				SchemeType: string.Empty,
				SchemeCategory: string.Empty,
				Currency: Currency.Unknown);
		}

		var apiResponse = await response.Content.ReadFromJsonAsync<MutualFundApiResponse>(cancellationToken);

		return new MutualFund(
			SchemeCode: schemeCode,
			Name: apiResponse.Meta.SchemeName,
			SchemeType: apiResponse.Meta.SchemeType,
			SchemeCategory: apiResponse.Meta.SchemeCategory,
			Currency: Currency.INR);
	}

	public async Task<IReadOnlyDictionary<DateOnly, decimal>> GetPriceAsync(string schemeCode, CancellationToken cancellationToken)
	{
		var httpClient = this.httpClientFactory.CreateClient(nameof(MutualFundApiClient));
		var response = await httpClient.GetAsync($"/mf/{schemeCode}", cancellationToken);

		if (response.StatusCode == HttpStatusCode.NotFound)
		{
			return ImmutableDictionary<DateOnly, decimal>.Empty;
		}

		var apiResponse = await response.Content.ReadFromJsonAsync<MutualFundApiResponse>(cancellationToken);

		return apiResponse.Data
			.ToFrozenDictionary(
				keySelector: data => DateOnly.ParseExact(data.Date, "dd-MM-yyyy", CultureInfo.InvariantCulture),
				elementSelector: data => decimal.Parse(data.Nav, CultureInfo.InvariantCulture));
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
