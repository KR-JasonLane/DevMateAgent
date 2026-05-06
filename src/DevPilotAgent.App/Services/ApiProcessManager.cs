namespace DevPilotAgent.App.Services;

using System.Diagnostics;
using System.Net.Http;
using Serilog;

/// <summary>
/// API 서버 프로세스의 생명주기(시작, 헬스체크, 종료)를 관리한다.
/// WPF App.OnStartup에서 시작하고 App.OnExit에서 종료한다.
/// </summary>
public class ApiProcessManager : IDisposable
{
    private Process? _apiProcess;
    private readonly string _apiProjectPath;
    private readonly string _healthUrl;
    private bool _disposed;

    public ApiProcessManager(string apiProjectPath, string baseUrl)
    {
        _apiProjectPath = apiProjectPath;
        _healthUrl = $"{baseUrl}/health";
    }

    /// <summary>
    /// API 서버 프로세스를 시작하고 헬스체크가 성공할 때까지 대기한다.
    /// </summary>
    /// <param name="ct">취소 토큰.</param>
    /// <returns>헬스체크 성공 시 true, 타임아웃 시 false.</returns>
    public async Task<bool> StartAsync(CancellationToken ct = default)
    {
        Log.Information("API 서버 시작 중... 경로: {Path}", _apiProjectPath);

        var startInfo = new ProcessStartInfo
        {
            FileName = "dotnet",
            Arguments = $"run --project \"{_apiProjectPath}\"",
            UseShellExecute = false,
            CreateNoWindow = true,
            RedirectStandardOutput = true,
            RedirectStandardError = true
        };

        _apiProcess = Process.Start(startInfo);

        if (_apiProcess is null)
        {
            Log.Error("API 서버 프로세스 시작 실패");
            return false;
        }

        Log.Information("API 프로세스 시작됨 (PID: {Pid})", _apiProcess.Id);

        _apiProcess.OutputDataReceived += (_, e) =>
        {
            if (!string.IsNullOrEmpty(e.Data))
                Log.Debug("[API] {Output}", e.Data);
        };
        _apiProcess.ErrorDataReceived += (_, e) =>
        {
            if (!string.IsNullOrEmpty(e.Data))
                Log.Warning("[API:ERR] {Error}", e.Data);
        };
        _apiProcess.BeginOutputReadLine();
        _apiProcess.BeginErrorReadLine();

        return await WaitForHealthyAsync(TimeSpan.FromSeconds(30), ct);
    }

    /// <summary>
    /// API 서버 프로세스 트리를 종료한다.
    /// </summary>
    public void Stop()
    {
        if (_apiProcess is null || _apiProcess.HasExited)
            return;

        Log.Information("API 서버 종료 중 (PID: {Pid})", _apiProcess.Id);

        try
        {
            _apiProcess.Kill(entireProcessTree: true);
            _apiProcess.WaitForExit(5000);
            Log.Information("API 서버 종료 완료");
        }
        catch (Exception ex)
        {
            Log.Warning(ex, "API 서버 종료 중 오류");
        }
    }

    private async Task<bool> WaitForHealthyAsync(TimeSpan timeout, CancellationToken ct)
    {
        using var httpClient = new HttpClient { Timeout = TimeSpan.FromSeconds(2) };
        var deadline = DateTime.UtcNow + timeout;

        while (DateTime.UtcNow < deadline && !ct.IsCancellationRequested)
        {
            try
            {
                var response = await httpClient.GetAsync(_healthUrl, ct);
                if (response.IsSuccessStatusCode)
                {
                    Log.Information("API 서�� 헬스체크 성공");
                    return true;
                }
            }
            catch
            {
                // 아직 준비 안 됨 - 재시도
            }

            await Task.Delay(500, ct);
        }

        Log.Error("API 서버 헬스체크 타임아웃 ({Timeout}초)", timeout.TotalSeconds);
        return false;
    }

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;
        Stop();
        _apiProcess?.Dispose();
    }
}
