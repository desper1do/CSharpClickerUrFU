using AutoMapper;
using CSharpClicker.Dtos;
using CSharpClicker.Infrastructure.Abstractions;
using CSharpClicker.ViewModels;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CSharpClicker.UseCases.GetLeaderboard;

public class GetLeaderboardQueryHandler : IRequestHandler<GetLeaderboardQuery, LeaderboardViewModel>
{
    private readonly IAppDbContext _appDbContext;
    private readonly IMapper _mapper;

    public GetLeaderboardQueryHandler(IAppDbContext appDbContext, IMapper mapper)
    {
        _appDbContext = appDbContext;
        _mapper = mapper;
    }

    public async Task<LeaderboardViewModel> Handle(GetLeaderboardQuery request, CancellationToken cancellationToken)
    {
        var query = _appDbContext.Users
            .AsNoTracking() // Для чтения быстрее без трекинга
            .OrderByDescending(u => u.CurrentScore);

        var totalUsers = await query.CountAsync(cancellationToken);

        var usersOnPage = await query
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync(cancellationToken);

        var userDtos = _mapper.Map<IEnumerable<UserInfoDto>>(usersOnPage);

        var totalPages = (int)Math.Ceiling(totalUsers / (double)request.PageSize);

        return new LeaderboardViewModel
        {
            Users = userDtos,
            CurrentPage = request.Page,
            TotalPages = totalPages
        };
    }
}