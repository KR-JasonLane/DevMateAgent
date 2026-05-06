# DevPilot Agent

개발자를 위한 데스크톱 AI Agent 도구. 에러 로그를 분석하고, 근본 원인을 파악하며, 수정 코드를 제안합니다.

![.NET 10](https://img.shields.io/badge/.NET-10.0-512BD4)
![WPF](https://img.shields.io/badge/WPF-Blazor_Hybrid-blue)
![Semantic Kernel](https://img.shields.io/badge/Semantic_Kernel-AI_Agent-orange)

## 주요 기능

- **7단계 에러 분석 파이프라인** — 에러 파싱 → 파일 검색 → 코드 분석 → 근본 원인 도출 → 수정 제안 → 테스트 시나리오 → PR 설명문
- **실시간 스트리밍** — SignalR을 통해 분석 진행 상황과 LLM 응답을 실시간 표시
- **대화형 인터페이스** — 에이전트와 채팅하며 분석 결과를 확인하고 수정 적용
- **마크다운 렌더링** — 분석 결과를 깔끔한 마크다운으로 표시
- **패치 적용** — 수정 제안을 원클릭으로 파일에 적용 (자동 백업)
- **분석 히스토리** — 이전 분석 결과를 저장하고 재확인
- **최근 프로젝트 기억** — 열었던 프로젝트를 기억하고 빠르게 전환

## 아키텍처

```
┌─────────────────────────────────────────────────────────┐
│  WPF Shell (MaterialDesign)                             │
│  ┌───────────────────────────────────────────────────┐  │
│  │  BlazorWebView (Chat UI + Result Panels)          │  │
│  └───────────────────────────────────────────────────┘  │
└─────────────────────┬───────────────────────────────────┘
                      │ HTTP + SignalR
┌─────────────────────▼───────────────────────────────────┐
│  ASP.NET Core API                                       │
│  ├── Controllers (REST)                                 │
│  ├── SignalR Hub (실시간 스트리밍)                        │
│  └── Semantic Kernel (7-step Agent Pipeline)            │
└─────────────────────┬───────────────────────────────────┘
                      │
              ┌───────▼───────┐
              │  LLM Backend  │
              │  (Ollama/OpenAI)│
              └───────────────┘
```

### 레이어 구조

| 프로젝트 | 역할 |
|---------|------|
| `DevPilotAgent.Domain` | 엔티티, 열거형 (순수 C# 비즈니스 로직) |
| `DevPilotAgent.Shared` | DTO, Request/Response, Constants |
| `DevPilotAgent.Application` | UseCase, 서비스 인터페이스 |
| `DevPilotAgent.Infrastructure` | EF Core, 파일시스템, Semantic Kernel 플러그인 |
| `DevPilotAgent.Api` | ASP.NET Core API, SignalR Hub |
| `DevPilotAgent.App` | WPF + Blazor 하이브리드 데스크톱 앱 |
| `DevPilotAgent.Tests` | xUnit 단위/통합 테스트 |

## 기술 스택

- **Frontend**: WPF (MaterialDesignThemes) + Blazor Hybrid (BlazorWebView)
- **Backend**: ASP.NET Core 10, SignalR
- **AI**: Semantic Kernel, Ollama / OpenAI 호환
- **DB**: SQLite (EF Core, WAL mode)
- **패턴**: Clean Architecture, MVVM (CommunityToolkit.Mvvm)
- **로깅**: Serilog (File + Debug)
- **테스트**: xUnit, Moq, FluentAssertions

## 시작하기

### 사전 요구사항

- .NET 10 SDK
- Ollama 서버 (또는 OpenAI API 키)
- Windows 10/11

### 실행

```bash
# 클론
git clone https://github.com/KR-JasonLane/DevPilotAgent.git
cd DevPilotAgent

# 빌드
dotnet build

# API 설정 (Ollama 사용 시)
# src/DevPilotAgent.Api/appsettings.Development.json 편집:
# {
#   "OpenAI": {
#     "ApiKey": "ollama",
#     "ModelId": "gemma4:31b-cloud",
#     "Endpoint": "http://localhost:11434/v1"
#   }
# }

# 앱 실행 (API 서버 자동 시작)
dotnet run --project src/DevPilotAgent.App
```

### 테스트

```bash
dotnet test
```

## 사용법

1. 앱 실행 후 **프로젝트 폴더 선택**
2. 채팅창에 **에러 로그 붙여넣기**
3. **Enter** 또는 전송 버튼 클릭
4. 실시간으로 분석 진행 확인
5. 우측 패널에서 **분석 결과** 확인:
   - 관련 파일
   - 근본 원인 분석
   - 수정 제안 (Diff + 적용 버튼)
   - 테스트 시나리오
   - PR 설명문

## 라이선스

MIT
