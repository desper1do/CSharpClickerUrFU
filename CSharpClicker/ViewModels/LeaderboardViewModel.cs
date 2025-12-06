using CSharpClicker.Dtos;

namespace CSharpClicker.ViewModels;

public class LeaderboardViewModel
{
    public IEnumerable<UserInfoDto> Users { get; init; } = Enumerable.Empty<UserInfoDto>();
    public int CurrentPage { get; init; }
    public int TotalPages { get; init; }
    public bool HasPreviousPage => CurrentPage > 1;
    public bool HasNextPage => CurrentPage < TotalPages;
}