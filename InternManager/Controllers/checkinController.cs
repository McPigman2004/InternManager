using System.Security.Claims;
using InternManager.Data;
using InternManager.DTO.attend;
using InternManager.Model;
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
    public class checkinController : ControllerBase
    {
        private readonly ApplicationDbContext _db;
        private readonly IConfiguration _config;

        public checkinController(ApplicationDbContext db, IConfiguration config)
        {
            _db = db;
            _config = config;
        }

        // -- CRUD ATTENDANCE (TTS)
        [HttpGet]
        public async Task<IActionResult> GetAttendUser(int userID)
        {
            var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdStr)) return Unauthorized("Token không hợp lệ.");
            userID = int.Parse(userIdStr);

            var user = await _db.Users.FirstOrDefaultAsync(u => u.id == userID);
            if (user == null)
            {
                return NotFound(new
                {
                    message = $"Không tìm thấy thực tập sinh có ID = {userID} trong hệ thống."
                });
            }

            var checkIn = await _db.Attendance_Checkins
                .Where(ci => ci.User_ID == userID)
                .Select(ci => new
                {
                    // user
                    username = ci.Users.tendangnhap,
                    // reg_schedule_intern
                    ci.id,
                    ci.User_ID,
                    ci.checkin,
                    ci.statusCheckIn,
                    ci.ghi_chu
                })
                .ToListAsync();

            if (!checkIn.Any())
            {
                return Ok(new
                {
                    message = $"Hiện tại thực tập sinh {user.tendangnhap} chưa checkIn.",
                    checkIns = checkIn
                });
            }

            return Ok(new
            {
                message = $"Danh sách checkIn của User {user.tendangnhap}",
                checkIns = checkIn
            });
        }
        [HttpPost]
        public async Task<IActionResult> CheckInGPS(checkInDTO newCheckInDTO)
        {
            var companyLat = _config.GetValue<double>("GPSSettings:Latitude");
            var companyLon = _config.GetValue<double>("GPSSettings:Longitude");
            var allowedRadius = _config.GetValue<double>("GPSSettings:AllowedRadiusInMeters");

            // Kiểm tra xem có đăng kí lịch làm không
            var today = DateOnly.FromDateTime(DateTime.Now);

            var validSchedule = await _db.Reg_Schedule_Interns
                .FirstOrDefaultAsync(s => s.id == newCheckInDTO.reg_intern_ID
                                       && s.User_ID == newCheckInDTO.User_ID
                                       && s.ngay_dang_ki == today);

            if (validSchedule == null)
            {
                return BadRequest(new { message = "Điểm danh thất bại! Bạn không có lịch đăng ký làm việc hợp lệ trong ngày hôm nay." });
            }

            if (validSchedule.status != StatusRegIntern.reg)
            {
                return BadRequest(new { message = $"Điểm danh thất bại! Lịch làm việc hôm nay của bạn đang ở trạng thái không thể điểm danh ({validSchedule.status})." });
            }

            // Kiểm tra xem đã điểm danh chưa
            var isAlreadyCheckedIn = await _db.Attendance_Checkins
                        .AnyAsync(ci => ci.reg_intern_ID == newCheckInDTO.reg_intern_ID);

            if (isAlreadyCheckedIn)
            {
                return BadRequest(new { message = "Điểm danh thất bại! Bạn đã thực hiện điểm danh cho lịch làm việc này ngày hôm nay rồi." });
            }

            // Hàm điểm danh (Kiểm tra GPS)
            double distance = GPS.CalculateDistance(newCheckInDTO.vi_do, newCheckInDTO.kinh_do, companyLat, companyLon);
            if (distance > allowedRadius)
            {
                return BadRequest(new { message = $"Điểm danh thất bại! Bạn đang ở cách công ty {Math.Round(distance, 1)}m (Vượt quá bán kính {allowedRadius}m cho phép)." });
            }

            DateTime now = DateTime.Now;
            TimeSpan currentTime = now.TimeOfDay;

            string systemNote = newCheckInDTO.note;
            bool isLate = false;

            // Tự động gán tên ca dựa trên Enum để ghi log cho đẹp
            string tenCa = validSchedule.ca_lam == CaLam.morning ? "Ca Sáng" : "Ca Chiều";

            if (validSchedule.ca_lam == CaLam.morning)
            {
                // CA SÁNG: Kiểm tra mốc đi trễ từ appsettings
                var lateTimeStringMorning = _config.GetValue<string>("Settings:LateCheckInMorning") ?? "08:30:00";
                TimeSpan lateThreshold = TimeSpan.Parse(lateTimeStringMorning);

                if (currentTime > lateThreshold)
                {
                    isLate = true;
                    if (string.IsNullOrEmpty(systemNote))
                    {
                        systemNote = $"Ca Sáng - Đi trễ (Sau {lateThreshold.ToString(@"hh\:mm")})";
                    }
                    else
                    {
                        systemNote += $" | Ca Sáng - Đi trễ (Sau {lateThreshold.ToString(@"hh\:mm")})";
                    }
                }
                else
                {
                    systemNote = "Ca Sáng - Đúng giờ | Điểm danh thành công";
                }
            }
            else if (validSchedule.ca_lam == CaLam.afternoon)
            {
                // CA CHIỀU: Không tính trễ, luôn luôn ghi nhận đúng giờ
                if (string.IsNullOrEmpty(systemNote))
                {
                    systemNote = "Ca Chiều - Đúng giờ | Điểm danh thành công";
                }
                else
                {
                    systemNote += " | Ca Chiều - Đúng giờ | Điểm danh thành công";
                }
            }

            // Lưu dữ liệu điểm danh xuống Database
            var newCheckIn = new attendance_checkin
            {
                User_ID = newCheckInDTO.User_ID,
                reg_intern_ID = newCheckInDTO.reg_intern_ID,
                checkin = now,
                vi_do = (decimal)Math.Round(newCheckInDTO.vi_do, 2),
                kinh_do = (decimal)Math.Round(newCheckInDTO.kinh_do, 2),
                statusCheckIn = StatusCheckIn.success,
                ghi_chu = systemNote
            };

            _db.Attendance_Checkins.Add(newCheckIn);
            await _db.SaveChangesAsync();

            // Phản hồi kết quả về Client
            if (isLate)
            {
                return Ok(new
                {
                    message = $"Điểm danh {tenCa} thành công lúc {now.ToString("HH:mm:ss")} nhưng bạn bị tính đi trễ.",
                    khoangCach = Math.Round(distance, 1),
                    thoiGianThuong = "Late"
                });
            }

            return Ok(new
            {
                message = $"Điểm danh {tenCa} đúng giờ thành công lúc {now.ToString("HH:mm:ss")}!",
                khoangCach = Math.Round(distance, 1),
                thoiGianThuong = "On"
            });
        }

        // -- ADMIN Hoặc Leader thấy
        [HttpGet("list")]
        public async Task<IActionResult> GetAttendAll()
        {
            var today = DateOnly.FromDateTime(DateTime.Now);
            var checkIn = await _db.Attendance_Checkins
                .Where(ci => DateOnly.FromDateTime(ci.checkin) == today)
                .Select(ci => new
                {
                    // user
                    username = ci.Users.tendangnhap,
                    // reg_schedule_intern
                    ci.id,
                    ci.User_ID,
                    ci.checkin,
                    ci.statusCheckIn,
                    ci.ghi_chu
                })
                .ToListAsync();

            if (!checkIn.Any())
            {
                return Ok(new
                {
                    message = $"Hiện tại chưa có thực tập sinh nào checkIn.",
                    checkIns = checkIn
                });
            }

            return Ok(new
            {
                message = $"Danh sách checkIn của thực tập sinh",
                checkIns = checkIn
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
            var checkIn = await _db.Attendance_Checkins
                .Where(ci => ci.Users.tendangnhap.ToLower().Contains(searchKeyword) 
                    && DateOnly.FromDateTime(ci.checkin) == today)
                .Select(ci => new
                {
                    username = ci.Users.tendangnhap,
                    ci.id,
                    ci.User_ID,
                    ci.checkin,
                    ci.statusCheckIn,
                    ci.ghi_chu
                })
                .ToListAsync();

            if (!checkIn.Any())
            {
                return Ok(new
                {
                    message = $"Không tìm thấy lịch sử checkIn nào khớp với tên đăng nhập '{username}'.",
                    checkIns = checkIn
                });
            }

            return Ok(new
            {
                message = $"Kết quả tìm kiếm checkIn cho thực tập sinh gần đúng với từ khóa '{username}':",
                checkIns = checkIn
            });
        }
        [HttpDelete]
        public async Task<IActionResult> RemoveCheckIn(int UserID)
        {
            var todayDateTime = DateTime.Today;

            var todayCheckIn = await _db.Attendance_Checkins
                .FirstOrDefaultAsync(ci => ci.User_ID == UserID
                                       && ci.checkin.Date == todayDateTime);

            if (todayCheckIn == null)
            {
                return NotFound(new
                {
                    message = $"Không tìm thấy dữ liệu điểm danh ngày hôm nay của thực tập sinh có ID = {UserID}."
                });
            }

            _db.Attendance_Checkins.Remove(todayCheckIn);
            await _db.SaveChangesAsync();

            return Ok(new
            {
                message = $"Xóa thành công dữ liệu điểm danh ngày hôm nay của thực tập sinh có ID = {UserID}."
            });
        }
    }
}
