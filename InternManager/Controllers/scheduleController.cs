using InternManager.Data;
using InternManager.DTO.attend;
using InternManager.Model;
using InternManager.Model.attend;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace InternManager.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class scheduleController : ControllerBase
    {
        private readonly ApplicationDbContext _db;

        public scheduleController(ApplicationDbContext db)
        {
            _db = db;
        }

        // -- CRUD SCHEDULE (TTS)
        [HttpGet]
        public async Task<IActionResult> GetScheduleList(int userID)
        {
            var user = await _db.Users.FirstOrDefaultAsync(u => u.id == userID);
            if (user == null)
            {
                return NotFound(new
                {
                    message = $"Không tìm thấy thực tập sinh có ID = {userID} trong hệ thống."
                });
            }

            var schedules = await _db.Reg_Schedule_Interns
                .Where(s => s.User_ID == userID && s.status == StatusRegIntern.reg)
                .Select(s => new 
                {
                    // user
                    username = s.Users.tendangnhap,
                    // reg_schedule_intern
                    s.id,
                    s.User_ID,
                    s.thu_trong_tuan,
                    s.ngay_dang_ki,
                    s.status
                })
                .ToListAsync();

            if (!schedules.Any())
            {
                return Ok(new
                {
                    message = $"Hiện tại thực tập sinh {user.tendangnhap} chưa có đăng kí lịch làm.",
                    lichlam = schedules
                });
            }

            return Ok(new
            {
                message = $"Danh sách lịch làm của thực tập sinh {user.tendangnhap}",
                lichlam = schedules
            });
        }

        [HttpPost]
        public async Task<IActionResult> RegSchedule(List<reg_scheduleDTO> listScheduleDTO)
        {
            if (listScheduleDTO == null || !listScheduleDTO.Any())
            {
                return BadRequest("Danh sách đăng ký lịch không được để trống.");
            }

            var userIDs = listScheduleDTO.Select(s => s.UserID).Distinct().ToList();

            var existingUserCount = await _db.Users.CountAsync(u => userIDs.Contains(u.id));
            if (existingUserCount != userIDs.Count)
            {
                return NotFound("Có chứa ID thực tập sinh không tồn tại trong hệ thống.");
            }

            var dates = listScheduleDTO.Select(s => s.ngay_dang_ki).Distinct().ToList();

            var existingSchedules = await _db.Reg_Schedule_Interns
                .Where(s => userIDs.Contains(s.User_ID) && dates.Contains(s.ngay_dang_ki))
                .ToListAsync();

            var schedulesToAdd = new List<reg_schedule_intern>();
            var dynamicComplete = new List<string>();
            var dynamicFail = new List<string>();
            int updatedCount = 0;

            foreach (var item in listScheduleDTO)
            {
                bool isDuplicateLocal = schedulesToAdd.Any(s =>
                    s.User_ID == item.UserID && s.ngay_dang_ki == item.ngay_dang_ki && s.ca_lam == item.ca_lam);

                if (isDuplicateLocal)
                {
                    dynamicFail.Add($"User {item.UserID} - Ngày {item.ngay_dang_ki} ({item.ca_lam}) - Trùng lặp trong danh sách gửi lên");
                    continue;
                }

                var dbSchedule = existingSchedules.FirstOrDefault(s =>
                    s.User_ID == item.UserID && s.ngay_dang_ki == item.ngay_dang_ki && s.ca_lam == item.ca_lam);

                if (dbSchedule != null)
                {
                    if (dbSchedule.status == StatusRegIntern.reg)
                    {
                        dynamicFail.Add($"User {item.UserID} - Ngày {item.ngay_dang_ki} ({item.ca_lam}) - Lịch làm này bạn đã đăng ký rồi");
                        continue;
                    }

                    if (dbSchedule.status == StatusRegIntern.cancel)
                    {
                        dbSchedule.status = StatusRegIntern.reg;
                        dbSchedule.ngay_dang_ki = item.ngay_dang_ki;
                        updatedCount++;

                        dynamicComplete.Add($"User {item.UserID} - Ngày {item.ngay_dang_ki} ({item.ca_lam}) - Đăng ký lại lịch đã hủy thành công");
                        continue;
                    }
                }

                schedulesToAdd.Add(new reg_schedule_intern
                {
                    User_ID = item.UserID,
                    thu_trong_tuan = item.thu_trong_tuan,
                    ca_lam = item.ca_lam,
                    ngay_dang_ki = item.ngay_dang_ki,
                    status = item.status
                });

                dynamicComplete.Add($"User {item.UserID} - Ngày {item.ngay_dang_ki} ({item.ca_lam})");
            }

            if (schedulesToAdd.Any())
            {
                _db.Reg_Schedule_Interns.AddRange(schedulesToAdd);
            }

            if (schedulesToAdd.Any() || updatedCount > 0)
            {
                await _db.SaveChangesAsync();
            }

            int totalSuccess = schedulesToAdd.Count + updatedCount;

            return Ok(new
            {
                message = $"Xử lý đăng ký lịch hoàn tất! Thành công: {totalSuccess}, Thất bại: {dynamicFail.Count}",
                thanhCong = dynamicComplete,
                thatBai = dynamicFail
            });
        }
        [HttpPut]
        public async Task<IActionResult> UpdateSchedule(List<int> ids)
        {
            if (ids == null || !ids.Any())
            {
                return BadRequest(new 
                { 
                    message = "Danh sách ID lịch cần hủy không được để trống." 
                });
            }

            var schedules = await _db.Reg_Schedule_Interns
                    .Where(s => ids.Contains(s.id))
                    .ToListAsync();

            if (!schedules.Any())
            {
                return NotFound(new 
                { 
                    message = "Không tìm thấy lịch nào phù hợp với danh sách ID đã cung cấp." 
                });
            }

            foreach (var schedule in schedules)
            {
                schedule.status = StatusRegIntern.cancel;
                schedule.ngay_dang_ki = DateOnly.FromDateTime(DateTime.Now);
            }

            await _db.SaveChangesAsync();

            return Ok(new
            {
                message = $"Đã hủy thành công {schedules.Count} lịch thực tập.",
                updated_ids = schedules.Select(s => s.id)
            });
        }

        // -- Áp dụng cho admin
        [HttpGet("list-all")]
        public async Task<IActionResult> GetAllScheduleListGrouped()
        {
            var rawSchedules = await _db.Reg_Schedule_Interns
                .Where(s => s.status == StatusRegIntern.reg)
                .Select(s => new
                {
                    Username = s.Users.tendangnhap,
                    s.User_ID,
                    s.thu_trong_tuan,
                    s.ngay_dang_ki,
                    s.ca_lam
                })
                .ToListAsync();

            var groupedSchedules = rawSchedules
                .GroupBy(s => new { s.User_ID, s.ngay_dang_ki })
                .Select(g => {
                    var caLamTrongNgay = g.Select(x => x.ca_lam.ToString().ToLower()).ToList();

                    string hienThiCaLam = "";

                    if (caLamTrongNgay.Contains("morning") && caLamTrongNgay.Contains("afternoon"))
                    {
                        hienThiCaLam = "Cả ngày";
                    }
                    else if (caLamTrongNgay.Contains("morning"))
                    {
                        hienThiCaLam = "Sáng";
                    }
                    else if (caLamTrongNgay.Contains("afternoon"))
                    {
                        hienThiCaLam = "Chiều";
                    }

                    var firstItem = g.First();
                    return new
                    {
                        username = firstItem.Username,
                        user_ID = g.Key.User_ID,
                        thu_trong_tuan = firstItem.thu_trong_tuan,
                        ngay_dang_ki = g.Key.ngay_dang_ki,
                        ca_lam = hienThiCaLam
                    };
                })
                .OrderBy(s => s.ngay_dang_ki)
                .ThenBy(s => s.username)
                .ToList();

            if (!groupedSchedules.Any())
            {
                return Ok(new
                {
                    message = "Hiện tại chưa có thực tập sinh nào đăng ký lịch làm.",
                    lichlam = groupedSchedules
                });
            }

            return Ok(new
            {
                message = "Danh sách tổng hợp lịch làm của tất cả thực tập sinh",
                lichlam = groupedSchedules
            });
        }

        // -- Áp dụng cho admin
        [HttpGet("search")]
        public async Task<IActionResult> GetScheduleListGrouped(string username)
        {
            var user = await _db.Users.FirstOrDefaultAsync(u => u.tendangnhap == username);
            if (user == null)
            {
                return NotFound(new
                {
                    message = $"Không tìm thấy thực tập sinh có tên đăng nhập = {username}."
                });
            }

            var rawSchedules = await _db.Reg_Schedule_Interns
                .Where(s => s.User_ID == user.id && s.status == StatusRegIntern.reg)
                .Select(s => new
                {
                    Username = s.Users.tendangnhap,
                    s.User_ID,
                    s.thu_trong_tuan,
                    s.ngay_dang_ki,
                    s.ca_lam
                })
                .ToListAsync();

            var groupedSchedules = rawSchedules
                .GroupBy(s => s.ngay_dang_ki)
                .Select(g => {
                    var caLamTrongNgay = g.Select(x => x.ca_lam.ToString().ToLower()).ToList();

                    string hienThiCaLam = "";

                    if (caLamTrongNgay.Contains("morning") && caLamTrongNgay.Contains("afternoon"))
                    {
                        hienThiCaLam = "Cả ngày";
                    }
                    else if (caLamTrongNgay.Contains("morning"))
                    {
                        hienThiCaLam = "Sáng";
                    }
                    else if (caLamTrongNgay.Contains("afternoon"))
                    {
                        hienThiCaLam = "Chiều";
                    }

                    var firstItem = g.First();
                    return new
                    {
                        username = firstItem.Username,
                        user_ID = firstItem.User_ID,
                        thu_trong_tuan = firstItem.thu_trong_tuan,
                        ngay_dang_ki = g.Key,
                        ca_lam = hienThiCaLam
                    };
                })
                .OrderBy(s => s.ngay_dang_ki)
                .ToList();

            if (!groupedSchedules.Any())
            {
                return Ok(new
                {
                    message = $"Thực tập sinh {user.tendangnhap} chưa có đăng ký lịch làm nào.",
                    lichlam = groupedSchedules
                });
            }

            return Ok(new
            {
                message = $"Danh sách lịch làm của thực tập sinh {user.tendangnhap}",
                lichlam = groupedSchedules
            });
        }
    }
}
