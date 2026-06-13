using System.Security.Claims;
using InternManager.Data;
using InternManager.DTO.task;
using InternManager.Model;
using InternManager.Model.task;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace InternManager.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class taskController : ControllerBase
    {
        private readonly ApplicationDbContext _db;

        public taskController(ApplicationDbContext db)
        {
            _db = db;
        }

        // TTS xem danh sách task của mình
        // Xem danh sách task của một user
        [HttpGet]
        [Authorize(Roles = "tts")]
        public async Task<IActionResult> GetUserTask(int userID)
        {
            var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            // Nếu không tìm thấy danh tính từ Cookie, hệ thống mới đá ra
            if (string.IsNullOrEmpty(userIdStr))
            {
                return Unauthorized(new { message = "Bạn chưa đăng nhập hoặc phiên làm việc đã hết hạn." });
            }

            userID = int.Parse(userIdStr);

            var user = await _db.Users.FirstOrDefaultAsync(u => u.id == userID);
            if (user == null)
            {
                return NotFound(new
                {
                    message = $"Không tìm thấy thực tập sinh có ID = {userID} trong hệ thống."
                });
            }

            var listTask = await _db.Task_Regs
                .Where(ci => ci.User_ID == userID)
                .OrderByDescending(ci => ci.statusTask == StatusTask.in_progress)
                .ThenByDescending(ci => ci.statusTask == StatusTask.done)
                .Select(ci => new
                {
                    // user
                    username = ci.Users.tendangnhap,
                    // reg_schedule_intern
                    ci.id,
                    ci.User_ID,
                    ci.tieu_de,
                    ci.noi_dung,
                    ci.progress,
                    ci.statusTask,
                    ci.ngay_dang_ki
                })
                .ToListAsync();

            if (!listTask.Any())
            {
                return Ok(new
                {
                    message = $"Hiện tại thực tập sinh {user.tendangnhap} chưa có task nào.",
                    listTasks = listTask
                });
            }

            return Ok(new
            {
                message = $"Danh sách task của User {user.tendangnhap}",
                listTasks = listTask
            });
        }

        // Leader hoặc admin tạo task cho TTS
        [HttpPost]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> CreateTask(task_regDTO newtaskRegDTO)
        {
            var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdStr)) return Unauthorized("Phiên làm việc không hợp lệ.");
            int adminID = int.Parse(userIdStr);

            var today = DateOnly.FromDateTime(DateTime.Now);
            if (newtaskRegDTO.ngay_dang_ki < today)
            {
                return BadRequest(new
                {
                    message = $"Tạo task thất bại! Ngày đăng ký ({newtaskRegDTO.ngay_dang_ki:dd/MM/yyyy}) không được nằm trong quá khứ so với hôm nay ({today:dd/MM/yyyy})."
                });
            }

            var usercheck = await _db.Users.AnyAsync(u => u.id == newtaskRegDTO.User_ID);
            if (!usercheck)
            {
                return NotFound(new { message = $"Không tìm thấy thực tập sinh có ID = {newtaskRegDTO.User_ID} trong hệ thống." });
            }

            var newTask = new task_reg
            {
                leader_ID = adminID,
                User_ID = newtaskRegDTO.User_ID,
                tieu_de = newtaskRegDTO.title.Trim(),
                noi_dung = newtaskRegDTO.content.Trim(),
                progress = newtaskRegDTO.progress,
                statusTask = newtaskRegDTO.statusTask,
                ngay_dang_ki = newtaskRegDTO.ngay_dang_ki
            };

            _db.Task_Regs.Add(newTask);
            await _db.SaveChangesAsync();

            var username = await _db.Users
                .Where(u => u.id == newTask.User_ID)
                .Select(u => u.tendangnhap)
                .FirstOrDefaultAsync();

            return Ok(new
            {
                message = $"Tạo task thành công cho thực tập sinh có ID = {newtaskRegDTO.User_ID}.",
                task = new
                {
                    newTask.id,
                    newTask.leader_ID,
                    tentts = username,
                    newTask.User_ID,
                    newTask.tieu_de,
                    newTask.noi_dung,
                    newTask.progress,
                    newTask.statusTask,
                    newTask.ngay_dang_ki
                }
            });
        }

        [HttpPut]
        [Authorize(Roles = "tts")]
        public async Task<IActionResult> UpdateTask(int taskId, task_regDTO updateDTO)
        {
            var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdStr)) return Unauthorized("Phiên làm việc không hợp lệ.");
            int userID = int.Parse(userIdStr);

            if (updateDTO.progress < 0 || updateDTO.progress > 100)
            {
                return BadRequest(new { message = "Tiến độ công việc phải nằm trong khoảng từ 0% đến 100%." });
            }

            var task = await _db.Task_Regs.FirstOrDefaultAsync(t => t.id == taskId && t.User_ID == userID);
            if (task == null)
            {
                return NotFound(new
                {
                    message = $"Không tìm thấy task hợp lệ nào có ID = {taskId} thuộc quyền sở hữu của bạn."
                });
            }

            if (task.statusTask == StatusTask.done)
            {
                return BadRequest(new { message = "Task này đã hoàn thành và đóng lại, không thể sửa đổi tiến độ." });
            }

            task.progress = updateDTO.progress;
            task.statusTask = (updateDTO.progress == 100) ? StatusTask.done : StatusTask.in_progress;

            _db.Task_Regs.Update(task);
            await _db.SaveChangesAsync();

            return Ok(new
            {
                message = $"Cập nhật tiến độ của task có ID = {taskId} thành công.",
                updatedTask = new
                {
                    task.id,
                    task.leader_ID,
                    task.User_ID,
                    task.tieu_de,
                    task.noi_dung,
                    task.progress,
                    task.statusTask,
                    task.ngay_dang_ki
                }
            });
        }

        [HttpPut("admin-update")]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> AdminUpdateTask(int taskId, [FromBody] task_regDTO updateDTO)
        {
            if (updateDTO.progress < 0 || updateDTO.progress > 100)
            {
                return BadRequest(new { message = "Tiến độ công việc phải nằm trong khoảng từ 0% đến 100%." });
            }

            var task = await _db.Task_Regs.FirstOrDefaultAsync(t => t.id == taskId);
            if (task == null)
            {
                return NotFound(new { message = $"Không tìm thấy task nào có ID = {taskId} trong hệ thống." });
            }

            if (task.User_ID != updateDTO.User_ID)
            {
                var isNewUserExists = await _db.Users.AnyAsync(u => u.id == updateDTO.User_ID);
                if (!isNewUserExists)
                {
                    return NotFound(new { message = $"Không tìm thấy thực tập sinh mới có ID = {updateDTO.User_ID} để giao lại task." });
                }
                task.User_ID = updateDTO.User_ID;
            }

            task.tieu_de = updateDTO.title.Trim();
            task.noi_dung = updateDTO.content.Trim();
            task.progress = updateDTO.progress;
            task.statusTask = updateDTO.statusTask;
            task.ngay_dang_ki = updateDTO.ngay_dang_ki;

            _db.Task_Regs.Update(task);
            await _db.SaveChangesAsync();

            var currentInternName = await _db.Users
                .Where(u => u.id == task.User_ID)
                .Select(u => u.tendangnhap)
                .FirstOrDefaultAsync();

            return Ok(new
            {
                message = $"Cập nhật toàn bộ thông tin task {taskId} thành công.",
                updatedTask = new
                {
                    task.id,
                    task.leader_ID,
                    tentts = currentInternName,
                    task.User_ID,
                    task.tieu_de,
                    task.noi_dung,
                    task.progress,
                    task.statusTask,
                    task.ngay_dang_ki
                }
            });
        }

        [HttpDelete]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> DeleteTask(int taskId)
        {
            var task = await _db.Task_Regs.FirstOrDefaultAsync(t => t.id == taskId);
            if (task == null)
            {
                return NotFound(new
                {
                    message = $"Không tìm thấy task nào có ID = {taskId} trong hệ thống."
                });
            }
            _db.Task_Regs.Remove(task);
            await _db.SaveChangesAsync();
            return Ok(new
            {
                message = $"Xóa task có ID = {taskId} thành công."
            });
        }

        // Admin xem danh sách task
        [HttpGet("list-all")]
        [Authorize(Roles = "admin")]
        public IActionResult GetTasksDone()
        {
            var completedTasks = _db.Task_Regs
                .Include(t => t.Users)
                .Include(t => t.Leaders)
                .OrderByDescending(t => t.ngay_dang_ki)
                .Take(30)
                .Select(t => new
                {
                    t.id,
                    t.tieu_de,
                    t.noi_dung,
                    t.progress,
                    t.statusTask,
                    t.ngay_dang_ki,
                    username = t.Users.tendangnhap,
                    leader = t.Leaders.tendangnhap
                })
                .ToList();
            if (!completedTasks.Any())
            {
                return Ok(new
                {
                    message = "Hiện tại chưa có task nào đã hoàn thành ."
                });
            }
            return Ok(new
            {
                message = "Danh sách task đã hoàn thành",
                task = completedTasks
            });
        }
        [HttpGet("admin-search")]
        [Authorize(Roles = "admin")]
        public IActionResult GetSearchTaskTTS(string username)
        {
            var tasks = _db.Task_Regs
                .Include(t => t.Users)
                .Include(t => t.Leaders)
                .Where(t => t.Users.tendangnhap == username)
                .Select(t => new
                {
                    t.id,
                    t.tieu_de,
                    t.noi_dung,
                    t.progress,
                    t.statusTask,
                    t.ngay_dang_ki,
                    username = t.Users.tendangnhap,
                    leader = t.Leaders.tendangnhap
                })
                .ToList();
            if (!tasks.Any())
            {
                return Ok(new
                {
                    message = $"Không tìm thấy task nào của thực tập sinh '{username}'."
                });
            }
            return Ok(new
            {
                message = $"Danh sách task của thực tập sinh '{username}'",
                task = tasks
            });
        }
    }
}
