using AuthService.Data;
using AuthService.Entities;
using AuthService.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace AuthService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IJwtService _jwtService;
        public AuthController(AppDbContext dbContext, IJwtService jwtService)
        {
            _context = dbContext;
            _jwtService = jwtService;
        }

        //[Authorize]
        //[HttpGet("me")]
        //public async Task<IActionResult> Me()
        //{
        //    var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);

        //    if (!int.TryParse(userIdClaim, out var userId))
        //    {
        //        return Unauthorized(new ApiResponse
        //        {
        //            Code = 401,
        //            Success = false,
        //            Message = "Invalid token"
        //        });
        //    }

        //    var user = await _context.DbUserMaster
        //        .AsNoTracking()
        //        .FirstOrDefaultAsync(x =>
        //            x.login_id == userId &&
        //            x.isactive);

        //    if (user == null)
        //    {
        //        return Unauthorized(new ApiResponse
        //        {
        //            Code = 401,
        //            Success = false,
        //            Message = "User not found or disabled"
        //        });
        //    }

        //    //var productAccessList = await (
        //    //    from up in _context.DbUserProductAccess.AsNoTracking()
        //    //    join p in _context.DbProduct.AsNoTracking()
        //    //    on up.product_id equals p.product_id
        //    //    where up.login_id == user.login_id && up.isactive
        //    //    select new ProductMap
        //    //    {
        //    //        Name = p.product_name,
        //    //        Code = p.unq_code
        //    //    }).ToListAsync();

        //    var response = new VerifiedAuthResponse
        //    {
        //        LoginId = user.login_id,
        //        Email = user.email,

        //        Role = user.role.HasValue
        //            ? ((UserRole)user.role.Value).ToString()
        //            : "User",

        //        ProductsList = null,
        //    };

        //    return Ok(new ApiResponse
        //    {
        //        Code = 200,
        //        Success = true,
        //        Message = "Verified successfully",
        //        Data = response
        //    });
        //}
        [HttpPost("register")]
        public async Task<IActionResult> AddUser(RegisterRequestDto register, CancellationToken ct)
        {
            var ip = HttpContext?.Connection?.RemoteIpAddress?.ToString();
            var exists = await _context.DbUserMaster
                    .AnyAsync(x => x.email == register.email, ct);
            if (exists)
            {
                return BadRequest(new ApiResponse
                {
                    Code = 400,
                    Success = false,
                    Message = "Email already exists"
                });
            }
            var newUser = new UserMaster
            {
                user_name = register.username.Trim().ToLower(),
                email = register.email.Trim().ToLower(),
                phone = register.phone,
                password_hash = BCrypt.Net.BCrypt.HashPassword(register.password),
                role = 1,
                isactive = true,
                create_date = DateTime.UtcNow,
                create_by = 1,
                modify_date = null,
                modify_by = null,
                ip_address = ip,
            };
            await _context.DbUserMaster.AddAsync(newUser, ct);
            await _context.SaveChangesAsync(ct);

            return Ok(new ApiResponse
            {
                Code = 200,
                Success = true,
                Message = "User register successfully",

            });
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginRequestDto request)
        {
            var tenant = HttpContext.Items["Tenant"]?.ToString();
            bool isEmail = request.identifier.Contains("@");

            bool isPhone = request.identifier.All(char.IsDigit);

            UserMaster? user = null;

            if (isEmail)
            {
                user = await _context.DbUserMaster
                    .FirstOrDefaultAsync(x =>
                        x.email == request.identifier);
            }
            else if (isPhone)
            {
                user = await _context.DbUserMaster
                    .FirstOrDefaultAsync(x =>
                        x.phone == request.identifier);
            }
            else
            {
                user = await _context.DbUserMaster
                    .FirstOrDefaultAsync(x =>
                        x.user_name == request.identifier);
            }

            if (user == null)
            {
                return Unauthorized(new ApiResponse
                {
                    Code = 401,
                    Success = false,
                    Message = "Invalid credentials"
                });
            }

            bool isValidPassword =
                BCrypt.Net.BCrypt.Verify(
                    request.password,
                    user.password_hash
                );

            if (!isValidPassword)
            {
                return Unauthorized(new ApiResponse
                {
                    Code = 401,
                    Success = false,
                    Message = "Invalid credentials"
                });
            }

            string token = _jwtService.GenerateToken(user);

            string sessionCode =
                Guid.NewGuid().ToString("N");

            var newSession = new AuthSession
            {
                session_code = sessionCode,
                login_id = user.login_id,
                access_token = token,
                expires_date = DateTime.UtcNow.AddMinutes(2),
                is_used = false,
                client_name = tenant,
                create_date = DateTime.UtcNow
            };

            await _context.DbAuthSession.AddAsync(newSession);
            await _context.SaveChangesAsync();
            return Ok(new ApiResponse
            {
                Code = 200,
                Success = true,
                Message = "Login successful",

                Data = new
                {
                    session_code = sessionCode
                }
            });
        }

        [HttpPost("exchange")]
        public async Task<IActionResult> ExchangeCode(ExchangeCodeRequestDto request)
        {
            var session = await _context.DbAuthSession
                .FirstOrDefaultAsync(x => x.session_code == request.code);

            if (session == null)
            {
                return Unauthorized(new ApiResponse
                {
                    Code = 401,
                    Success = false,
                    Message = "Invalid session"
                });
            }

            if (session.is_used)
            {
                return Unauthorized(new ApiResponse
                {
                    Code = 401,
                    Success = false,
                    Message = "Session already used"
                });
            }

            if (session.expires_date < DateTime.UtcNow)
            {
                return Unauthorized(new ApiResponse
                {
                    Code = 401,
                    Success = false,
                    Message = "Session expired"
                });
            }

            var tenant = HttpContext.Items["Tenant"]?.ToString();

            if (string.IsNullOrEmpty(tenant) ||
                !string.Equals(session.client_name, tenant, StringComparison.OrdinalIgnoreCase))
            {
                return Unauthorized(new ApiResponse
                {
                    Code = 401,
                    Success = false,
                    Message = "Tenant mismatch"
                });
            }

            session.is_used = true;
            await _context.SaveChangesAsync();

            return Ok(new ApiResponse
            {
                Code = 200,
                Success = true,
                Message = "Token exchange successful",
                Data = new
                {
                    accessToken = session.access_token
                }
            });
        }
        public enum UserRole
        {
            Admin = 2,
            SupAdmin = 3,
        }
    }
}


