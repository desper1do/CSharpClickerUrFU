using CSharpClicker.ViewModels;
using MediatR;

namespace CSharpClicker.UseCases.GetLeaderboard;

public record GetLeaderboardQuery(int Page = 1, int PageSize = 10) : IRequest<LeaderboardViewModel>;