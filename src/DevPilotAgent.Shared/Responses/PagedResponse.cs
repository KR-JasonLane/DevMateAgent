namespace DevPilotAgent.Shared.Responses;

/// <summary>
/// 페이지네이션 결과를 래핑하는 제네릭 응답 DTO.
/// </summary>
/// <typeparam name="T">페이지 내 항목 타입.</typeparam>
/// <param name="Items">현재 페이지의 항목 목록.</param>
/// <param name="TotalCount">전체 항목 수.</param>
/// <param name="Page">현재 페이지 번호 (1-based).</param>
/// <param name="PageSize">페이지당 항목 수.</param>
public record PagedResponse<T>(
    List<T> Items,
    int TotalCount,
    int Page,
    int PageSize
);
