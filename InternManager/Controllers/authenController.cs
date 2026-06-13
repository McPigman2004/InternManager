using System.Security.Claims;
using Microsoft.AspNetCore.Authentication; 
using Microsoft.AspNetCore.Authentication.Cookies; 
using InternManager.Data;
using InternManager.DTO;
using InternManager.Model;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace InternManager.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [AllowAnonymous] 
    public class authenController : ControllerBase
    {
        private readonly ApplicationDbContext _db;

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

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.id.ToString()), 
                new Claim(ClaimTypes.Name, user.tendangnhap),             
                new Claim(ClaimTypes.Role, user.role.ToString().ToLower()) 
            };

            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(claimsIdentity)
            );


            return Ok(new
            {
                message = "Đăng nhập thành công và đã khởi tạo phiên làm việc!",
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