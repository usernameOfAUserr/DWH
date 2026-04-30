using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace DWH.Application.Services;

public class GithubFileUploadService(HttpClient httpClient)
{
    public async Task<string> UploadOrUpdateFileAsync(
        string owner,
        string repo,
        string branch,
        string githubToken,
        string localFilePath,
        string targetPathInRepo,
        string commitMessage,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(owner);
        ArgumentException.ThrowIfNullOrWhiteSpace(repo);
        ArgumentException.ThrowIfNullOrWhiteSpace(branch);
        ArgumentException.ThrowIfNullOrWhiteSpace(githubToken);
        ArgumentException.ThrowIfNullOrWhiteSpace(localFilePath);
        ArgumentException.ThrowIfNullOrWhiteSpace(targetPathInRepo);
        ArgumentException.ThrowIfNullOrWhiteSpace(commitMessage);

        if (!File.Exists(localFilePath))
        {
            throw new FileNotFoundException("Die lokale Datei wurde nicht gefunden.", localFilePath);
        }

        httpClient.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", githubToken);

        httpClient.DefaultRequestHeaders.Accept.Clear();
        httpClient.DefaultRequestHeaders.Accept.Add(
            new MediaTypeWithQualityHeaderValue("application/vnd.github+json"));

        httpClient.DefaultRequestHeaders.UserAgent.Clear();
        httpClient.DefaultRequestHeaders.UserAgent.ParseAdd("DWH-Import-Service");

        if (!httpClient.DefaultRequestHeaders.Contains("X-GitHub-Api-Version"))
        {
            httpClient.DefaultRequestHeaders.Add("X-GitHub-Api-Version", "2022-11-28");
        }

        var encodedPath = Uri.EscapeDataString(targetPathInRepo).Replace("%2F", "/");
        var contentUrl =
            $"https://api.github.com/repos/{owner}/{repo}/contents/{encodedPath}?ref={Uri.EscapeDataString(branch)}";

        var sha = await TryGetExistingShaAsync(contentUrl, cancellationToken);

        var fileBytes = await File.ReadAllBytesAsync(localFilePath, cancellationToken);
        var base64Content = Convert.ToBase64String(fileBytes);

        var requestBody = new GithubCreateOrUpdateFileRequest
        {
            Message = commitMessage,
            Content = base64Content,
            Branch = branch,
            Sha = sha,
        };

        var json = JsonSerializer.Serialize(
            requestBody,
            new JsonSerializerOptions
            {
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
            });

        using var request = new HttpRequestMessage(
            HttpMethod.Put,
            $"https://api.github.com/repos/{owner}/{repo}/contents/{encodedPath}");

        request.Content = new StringContent(json, Encoding.UTF8, "application/json");

        using var response = await httpClient.SendAsync(request, cancellationToken);
        var responseText = await response.Content.ReadAsStringAsync(cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            throw new InvalidOperationException(
                $"GitHub-Upload fehlgeschlagen. Status: {(int)response.StatusCode} {response.ReasonPhrase}. Response: {responseText}");
        }

        return BuildRawFileUrl(owner, repo, branch, targetPathInRepo);
    }

    private async Task<string?> TryGetExistingShaAsync(
        string contentUrl,
        CancellationToken cancellationToken)
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, contentUrl);
        using var response = await httpClient.SendAsync(request, cancellationToken);

        if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            return null;
        }

        var responseText = await response.Content.ReadAsStringAsync(cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            throw new InvalidOperationException(
                $"Fehler beim Lesen der bestehenden GitHub-Datei. Status: {(int)response.StatusCode} {response.ReasonPhrase}. Response: {responseText}");
        }

        var dto = JsonSerializer.Deserialize<GithubContentResponse>(responseText);

        return dto?.Sha;
    }

    private static string BuildRawFileUrl(
        string owner,
        string repo,
        string branch,
        string targetPathInRepo)
    {
        var normalizedPath = targetPathInRepo.Replace("\\", "/").TrimStart('/');

        return $"https://raw.githubusercontent.com/{owner}/{repo}/{branch}/{normalizedPath}";
    }

    private sealed class GithubCreateOrUpdateFileRequest
    {
        [JsonPropertyName("message")]
        public required string Message { get; set; }

        [JsonPropertyName("content")]
        public required string Content { get; set; }

        [JsonPropertyName("branch")]
        public required string Branch { get; set; }

        [JsonPropertyName("sha")]
        public string? Sha { get; set; }
    }

    private sealed class GithubContentResponse
    {
        [JsonPropertyName("sha")]
        public string? Sha { get; set; }
    }
}