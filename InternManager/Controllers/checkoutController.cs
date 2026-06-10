using InternManager.Data;
using InternManager.DTO.attend;
using InternManager.Model.attend;
using InternManager.Utils;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace InternManager.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [AllowAnonymous]
    public class checkoutController : ControllerBase
    {
        private readonly ApplicationDbContext _db;
        private readonly IConfiguration _config;

        public checkoutController(ApplicationDbContext db, IConfiguration config)
        {
            _db = db;
            _config = config;
        }
        // -- CRUD ATTENDANCE (TTS)
        [HttpGet]
        public async Task<IActionResult> GetAttendUser(int userID)
        {
            var user = await _db.Users.FirstOrDefaultAsync(u => u.id == userID);
            if (user == null)
            {
                return NotFound(new
                {
                    message = $"Không tìm thấy thực tập sinh có ID = {userID} trong hệ thống."
                });
            }

            var checkOut = await _db.Attendance_Checkouts
                .Where(ci => ci.User_ID == userID)
                .Select(ci => new
                {
                    // user
                    username = ci.Users.tendangnhap,
                    // reg_schedule_intern
                    ci.id,
                    ci.User_ID,
                    ci.checkout,
                    ci.statusCheckOut,
                    ci.ghi_chu
                })
                .ToListAsync();

            if (!checkOut.Any())
            {
                return Ok(new
                {
                    message = $"Hiện tại thực tập sinh {user.tendangnhap} chưa checkout.",
                    checkOuts = checkOut
                });
            }

            return Ok(new
            {
                message = $"Danh sách checkout của User {user.tendangnhap}",
                checkOuts = checkOut
            });
        }
        [HttpPost]
        public async Task<IActionResult> CheckOutGPS(checkOutDTO newcheckOutDTO)
        {
            var today = DateOnly.FromDateTime(DateTime.Now);

            var validSchedule = await _db.Reg_Schedule_Interns
                .FirstOrDefaultAsync(s => s.id == newcheckOutDTO.reg_intern_ID
                                       && s.User_ID == newcheckOutDTO.User_ID
                                       && s.ngay_dang_ki == today);

            if (validSchedule == null)
            {
                return BadRequest(new 
                { 
                    message = "Check-out thất bại! Bạn không có lịch đăng ký làm việc hợp lệ trong ngày hôm nay." 
                });
            }

            var existingCheckIn = await _db.Attendance_Checkins
                .FirstOrDefaultAsync(ci => ci.reg_intern_ID == newcheckOutDTO.reg_intern_ID
                                       && ci.User_ID == newcheckOutDTO.User_ID);

            if (existingCheckIn == null)
            {
                return BadRequest(new 
                { 
                    message = "Check-out thất bại! Bạn chưa thực hiện điểm danh đầu ca (Check-in) cho lịch làm việc này." 
                });
            }

            var newcheckOut = new attendance_checkout
            {
                User_ID = newcheckOutDTO.User_ID,
                reg_intern_ID = newcheckOutDTO.reg_intern_ID,
                checkout = DateTime.Now,
                vi_do = (decimal)Math.Round(newcheckOutDTO.vi_do, 2),
                kinh_do = (decimal)Math.Round(newcheckOutDTO.kinh_do, 2),
                statusCheckOut = StatusCheckOut.success,
                ghi_chu = $"| {newcheckOutDTO.note} lúc {DateTime.Now.ToString("HH:mm:ss")}"
            };

            _db.Attendance_Checkouts.Add(newcheckOut);
            await _db.SaveChangesAsync();


            return Ok(new
            {
                message = $"Thực tập sinh đã Check-out thành công lúc {DateTime.Now.ToString("HH:mm:ss")}!",
            });
        }
        // -- ADMIN Hoặc Leader thấy
        [HttpGet("list")]
        public async Task<IActionResult> GetAttendAll()
        {
            var today = DateOnly.FromDateTime(DateTime.Now);
            var checkOut = await _db.Attendance_Checkouts
                .Where(ci => DateOnly.FromDateTime(ci.checkout) == today)
                .Select(ci => new
                {
                    // user
                    username = ci.Users.tendangnhap,
                    // reg_schedule_intern
                    ci.id,
                    ci.User_ID,
                    ci.checkout,
                    ci.statusCheckOut,
                    ci.ghi_chu
                })
                .ToListAsync();

            if (!checkOut.Any())
            {
                return Ok(new
                {
                    message = $"Hiện tại chưa có thực tập sinh nào checkOut.",
                    checkOuts = checkOut
                });
            }

            return Ok(new
            {
                message = $"Danh sách checkOut của thực tập sinh",
                checkOuts = checkOut
            });
        }
        [HttpGet("search")]
        public async Task<IActionResult> SearchAttendAll(string username)
        {
            if (string.IsNullOrEmpty(username))
            {
                return BadRequest(new { message = "Vui lòng nhập tên đăng nhập cần tìm kiếm." });
            }

            var searchKeyword = username.Trim().ToLower();

            var today = DateOnly.FromDateTime(DateTime.Now);
            var checkOut = await _db.Attendance_Checkouts
                .Where(ci => ci.Users.tendangnhap.ToLower().Contains(searchKeyword)
                    && DateOnly.FromDateTime(ci.checkout) == today)
                .Select(ci => new
                {
                    username = ci.Users.tendangnhap,
                    ci.id,
                    ci.User_ID,
                    ci.checkout,
                    ci.statusCheckOut,
                    ci.ghi_chu
                })
                .ToListAsync();

            if (!checkOut.Any())
            {
                return Ok(new
                {
                    message = $"Không tìm thấy lịch sử checkOut nào khớp với tên đăng nhập '{username}'.",
                    checkOuts = checkOut
                });
            }

            return Ok(new
            {
                message = $"Kết quả tìm kiếm checkOut cho thực tập sinh gần đúng với từ khóa '{username}':",
                checkOuts = checkOut
            });
        }
        [HttpDelete]
        public async Task<IActionResult> RemoveCheckOut(int UserID)
        {
            var todayDateTime = DateTime.Today;

            var todayCheckOut = await _db.Attendance_Checkouts
                .FirstOrDefaultAsync(ci => ci.User_ID == UserID
                                       && ci.checkout.Date == todayDateTime);

            if (todayCheckOut == null)
            {
                return NotFound(new
                {
                    message = $"Không tìm thấy dữ liệu điểm danh ngày hôm nay của thực tập sinh có ID = {UserID}."
                });
            }

            _db.Attendance_Checkouts.Remove(todayCheckOut);
            await _db.SaveChangesAsync();

            return Ok(new
            {
                message = $"Xóa thành công dữ liệu điểm danh ngày hôm nay của thực tập sinh có ID = {UserID}."
            });
        }
    }
}
