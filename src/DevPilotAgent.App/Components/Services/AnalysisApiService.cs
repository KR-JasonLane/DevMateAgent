namespace DevPilotAgent.App.Components.Services;

using System.Net.Http;
using System.Net.Http.Json;
using DevPilotAgent.Shared.Requests;
using DevPilotAgent.Shared.Responses;

/// <summary>
/// API 서버와 HTTP 통신을 수행하는 클라이언트 서비스.
/// Blazor 컴포넌트에서 분석 생성, 실행, 조회, 수정안 적용 등을 호출한다.
/// </summary>
public class AnalysisApiService
{
    private readonly HttpClient _httpClient;

    public AnalysisApiService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<AnalysisStartedResponse> StartAnalysisAsync(AnalysisRequest request, CancellationToken ct = default)
    {
        var response = await _httpClient.PostAsJsonAsync("api/analysis", request, ct);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<AnalysisStartedResponse>(ct)
            ?? throw new InvalidOperationException("응답 파싱 실패");
    }

    public async Task TriggerAnalysisAsync(Guid analysisId, AnalysisRequest request, CancellationToken ct = default)
    {
        var response = await _httpClient.PostAsJsonAsync($"api/analysis/{analysisId}/start", request, ct);
        response.EnsureSuccessStatusCode();
    }

    public async Task<AnalysisResponse?> GetAnalysisAsync(Guid id, CancellationToken ct = default)
    {
        return await _httpClient.GetFromJsonAsync<AnalysisResponse>($"api/analysis/{id}", ct);
    }

    public async Task<PagedResponse<AnalysisSummaryDto>> ListAnalysesAsync(int page = 1, int pageSize = 20, CancellationToken ct = default)
    {
        return await _httpClient.GetFromJsonAsync<PagedResponse<AnalysisSummaryDto>>(
            $"api/analysis?page={page}&pageSize={pageSize}", ct)
            ?? new PagedResponse<AnalysisSummaryDto>([], 0, page, pageSize);
    }

    public async Task<ApplyPatchResponse> ApplyPatchAsync(ApplyPatchRequest request, CancellationToken ct = default)
    {
        var response = await _httpClient.PostAsJsonAsync($"api/analysis/{request.AnalysisId}/apply", request, ct);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<ApplyPatchResponse>(ct)
            ?? throw new InvalidOperationException("응답 파싱 실패");
    }

    public async Task CancelAnalysisAsync(Guid analysisId, CancellationToken ct = default)
    {
        await _httpClient.DeleteAsync($"api/analysis/{analysisId}/cancel", ct);
    }
}
