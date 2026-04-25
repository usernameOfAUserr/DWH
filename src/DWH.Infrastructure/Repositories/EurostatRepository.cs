using System.Text.Json;
using DWH.Application.Interfaces.Repositories;
using DWH.Domain.Dtos;

namespace DWH.Infrastructure.Repositories;

public class EurostatRepository(HttpClient httpClient) : IEurostatRepository
{
    private const string BaseUrl =
        "https://ec.europa.eu/eurostat/api/dissemination/sdmx/3.0/data/dataflow/ESTAT/jvs_q_nace2/1.0/*.*.*.*.*.*";

    public async Task<EurostatResponse> GetInformationCommunicationJobVacanciesAsync(
        CancellationToken cancellationToken = default)
    {
        var query =
            "?c[freq]=Q" +
            "&c[s_adj]=SA" +
            "&c[nace_r2]=J" +
            "&c[sizeclas]=TOTAL" +
            "&c[indic_em]=JOBVAC" +
            "&c[geo]=DE" +
            "&c[TIME_PERIOD]=2025-Q3,2025-Q2,2025-Q1,2024-Q4,2024-Q3,2024-Q2,2024-Q1,2023-Q4,2023-Q3,2023-Q2" +
            "&compress=false" +
            "&format=json" +
            "&lang=en";

        using var response = await httpClient.GetAsync(BaseUrl + query, cancellationToken)
            .ConfigureAwait(false);

        response.EnsureSuccessStatusCode();

        var stream = await response.Content.ReadAsStreamAsync(cancellationToken)
            .ConfigureAwait(false);

        var document = await JsonDocument.ParseAsync(stream, cancellationToken: cancellationToken)
            .ConfigureAwait(false);

        return new EurostatResponse(document);
    }
}