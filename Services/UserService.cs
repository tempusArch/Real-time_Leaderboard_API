using RealtimeLeaderboardAPI.Domain;
using RealtimeLeaderboardAPI.Infrastructure;
using Microsoft.EntityFrameworkCore;
using AutoMapper;

namespace RealtimeLeaderboardAPI.Application;

public class UserService {
    private readonly IMapper _mapper;
    private readonly JwtService _jwtService;
    private readonly PasswordHasher _passwordHasher;
    private readonly RealtimeLeaderboardAPIDbContext _context;
    private readonly HttpContextService _httpContextService;


    public UserService(IMapper mapper, JwtService jwtService, PasswordHasher passwordHasher, RealtimeLeaderboardAPIDbContext context, HttpContextService httpContextService) {
        _mapper = mapper;
        _jwtService = jwtService;
        _passwordHasher = passwordHasher;
        _context = context;
        _httpContextService = httpContextService;
    }

    public async Task<UserResponse> RegisterUser(RegisterUserDto dto, CancellationToken cancellationToken) {
        var isEmailExisted = await _context.UserTable
            .AnyAsync(x => x.Email == dto.Email);

        if (isEmailExisted)
            throw new InvalidOperationException("Email already existed");

        var theNewUser = _mapper.Map<User>(dto);
            
        theNewUser.PasswordHashed = _passwordHasher.HashPassword(dto.Password);

        _context.UserTable.Add(theNewUser);
        await _context.SaveChangesAsync(cancellationToken);

        return new UserResponse {
            Name = theNewUser.Name,
            Email = theNewUser.Email
        };
    }

    public async Task<string> LoginUser(LoginUserDto dto, CancellationToken cancellationToken) {
        var theUser = await _context.UserTable
            .SingleOrDefaultAsync(x => x.Email == dto.Email);

        if (theUser == null) 
            throw new UnauthorizedAccessException("User not found");

        var isValidPassword = _passwordHasher.VerifyPassword(
            dto.Password,
            theUser.PasswordHashed
        );

        if (!isValidPassword) 
            throw new UnauthorizedAccessException("Invalid password");
          
        var accessToken = _jwtService.Generate_JWT(theUser);
        var refreshToken = _jwtService.Generate_RefreshToken(theUser.Id.ToString());

        _context.RefreshTokenTable.Add(refreshToken);
        await _context.SaveChangesAsync(cancellationToken);
        
        _httpContextService.SetRefreshToken(refreshToken);

        return accessToken;
    }

    public async Task<string> RefreshUser(CancellationToken cancellationToken) {
        var valueOfRefreshToken = _httpContextService.GetRefreshToken();

        var kyuuRefreshToken = await _context.RefreshTokenTable
            .SingleOrDefaultAsync(n => n.Token == valueOfRefreshToken);

        if (kyuuRefreshToken == null || !kyuuRefreshToken.IsActive)
            throw new UnauthorizedAccessException("RefreshToken null or invalid");

        kyuuRefreshToken.RevokedAt = DateTime.UtcNow;

        var newRefreshToken = _jwtService.Generate_RefreshToken(kyuuRefreshToken.UserId);

        var um = await _context.UserTable.
            SingleOrDefaultAsync(
                n => n.Id == int.Parse(kyuuRefreshToken.UserId)
            );
        var newAccessToken = _jwtService.Generate_JWT(um);

        _context.RefreshTokenTable.Add(newRefreshToken);
        await _context.SaveChangesAsync(cancellationToken);

        _httpContextService.SetRefreshToken(newRefreshToken);

        return newAccessToken;
    }

    public async Task LogoutUser(CancellationToken cancellationToken) {
        var valueOfRefreshToken = _httpContextService.GetRefreshToken();

        if (!string.IsNullOrEmpty(valueOfRefreshToken)) {
            var kyuuRefreshToken = await _context.RefreshTokenTable
                .SingleOrDefaultAsync(n => n.Token == valueOfRefreshToken, cancellationToken);

            if (kyuuRefreshToken != null) {
                kyuuRefreshToken.RevokedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync(cancellationToken);
            }
        }

        _httpContextService.DeleteRefreshToken();

        return;
    }
}