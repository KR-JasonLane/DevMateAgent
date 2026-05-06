namespace DevPilotAgent.App.Components.Services;

using DevPilotAgent.Shared.Constants;
using DevPilotAgent.Shared.DTOs;
using Microsoft.AspNetCore.SignalR.Client;

/// <summary>
/// SignalR Hub에 연결하여 분석 실시간 스트리밍 이벤트를 수신하는 서비스.
/// 자동 재연결 시 분석 그룹에 자동으로 재참가한다.
/// </summary>
public class AnalysisHubService : IAsyncDisposable
{
    private HubConnection? _connection;
    private Guid? _currentAnalysisId;
    private readonly string _hubUrl;

    public AnalysisHubService(string hubUrl)
    {
        _hubUrl = hubUrl;
    }

    public event Action<AgentStepMessage>? OnAgentStepUpdate;
    public event Action<StreamChunkMessage>? OnStreamChunk;
    public event Action<AnalysisCompletedMessage>? OnAnalysisCompleted;

    public async Task ConnectAsync()
    {
        if (_connection is not null) return;

        _connection = new HubConnectionBuilder()
            .WithUrl(_hubUrl)
            .WithAutomaticReconnect()
            .Build();

        _connection.On<AgentStepMessage>(HubMethods.AgentStepUpdate, msg =>
            OnAgentStepUpdate?.Invoke(msg));

        _connection.On<StreamChunkMessage>(HubMethods.StreamChunk, msg =>
            OnStreamChunk?.Invoke(msg));

        _connection.On<AnalysisCompletedMessage>(HubMethods.AnalysisCompleted, msg =>
            OnAnalysisCompleted?.Invoke(msg));

        _connection.Reconnected += async _ =>
        {
            if (_currentAnalysisId.HasValue)
                await _connection.InvokeAsync(HubMethods.JoinAnalysis, _currentAnalysisId.Value);
        };

        await _connection.StartAsync();
    }

    public async Task JoinAnalysisAsync(Guid analysisId)
    {
        _currentAnalysisId = analysisId;
        if (_connection is not null)
            await _connection.InvokeAsync(HubMethods.JoinAnalysis, analysisId);
    }

    public async Task LeaveAnalysisAsync(Guid analysisId)
    {
        _currentAnalysisId = null;
        if (_connection is not null)
            await _connection.InvokeAsync(HubMethods.LeaveAnalysis, analysisId);
    }

    public async ValueTask DisposeAsync()
    {
        if (_connection is not null)
            await _connection.DisposeAsync();
    }
}
