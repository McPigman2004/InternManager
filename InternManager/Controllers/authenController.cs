using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using InternManager.Data;
using InternManager.DTO;
using InternManager.Model;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace InternManager.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [AllowAnonymous]
    public class authenController : ControllerBase
    {
        private readonly ApplicationDbContext _db;
        private readonly IConfiguration _config;

        public authenController(ApplicationDbContext db)
        {
            _db = db;
        }

        [HttpPost]
        public async Task<IActionResult> loginRequest(authenDT0 request)
        {
            var user = await _db.Users
                .Select(u => new { u.id, u.tendangnhap, u.matkhau, u.role, u.status })
                .FirstOrDefaultAsync(u => u.tendangnhap == request.username && u.matkhau == request.password);


            if (user == null)
            {
                return Unauthorized(new { message = "Tên đăng nhập hoặc mật khẩu không đúng." });
            }

            if (user.status != UserStatus.active)
            {
                return Unauthorized(new { message = "Tài khoản của bạn đã bị vô hiệu hóa. Vui lòng liên hệ quản trị viên." });
            }

            HttpContext.Session.SetString("UserId", user.id.ToString());
            HttpContext.Session.SetString("Role", user.role.ToString());
            HttpContext.Session.SetString("UserName", user.tendangnhap);

            var secretKey = _config["Settings:SecretKey"]; 
            var keyBytes = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
            var creds = new SigningCredentials(keyBytes, SecurityAlgorithms.HmacSha256);


            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.id.ToString()),
                new Claim(ClaimTypes.Name, user.tendangnhap),
                new Claim(ClaimTypes.Role, user.role.ToString())
            };

            var token = new JwtSecurityToken(
                issuer: null,
                audience: null,
                claims: claims,
                expires: DateTime.Now.AddDays(1), // Token có hạn 1 ngày
                signingCredentials: creds
            );

            var tokenString = new JwtSecurityTokenHandler().WriteToken(token);

            return Ok(new
            {
                message = "Đăng nhập thành công",
                user = new
                {
                    id = user.id,
                    tendangnhap = user.tendangnhap,
                    role = user.role,
                }
            });
        }
    }
}
