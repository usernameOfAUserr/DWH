using System.Text.Json;
using DWH.Application.Interfaces.Repositories;
using DWH.Domain.Dtos;

namespace DWH.Infrastructure.Repositories;

public class EurostatRepository(HttpClient httpClient) : IEurostatRepository
{
    private const string BaseUrl =
        "https://ec.europa.eu/eurostat/api/dissemination/sdmx/3.0/data/dataflow/ESTAT/jvs_q_nace2/1.0/*.*.*.*.*.*?c[freq]=Q&c[s_adj]=SA&c[nace_r2]=J&c[sizeclas]=TOTAL&c[indic_em]=JOBVAC&compress=false&format=json&lang=en";

    public async Task<EurostatResponse> GetInformationCommunicationJobVacanciesAsync(
        CancellationToken cancellationToken = default)
    {
        using var response = await httpClient.GetAsync(BaseUrl, cancellationToken)
            .ConfigureAwait(false);

        response.EnsureSuccessStatusCode();

        var stream = await response.Content.ReadAsStreamAsync(cancellationToken)
            .ConfigureAwait(false);

        var document = await JsonDocument.ParseAsync(stream, cancellationToken: cancellationToken)
            .ConfigureAwait(false);

        return new EurostatResponse(document);
    }
}