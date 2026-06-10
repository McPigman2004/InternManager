using InternManager.Data;
using InternManager.DTO.task;
using InternManager.Model;
using InternManager.Model.task;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace InternManager.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
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
        public async Task<IActionResult> GetUserTask(int userID)
        {
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
        public async Task<IActionResult> CreateTask(task_regDTO newtaskRegDTO)
        {
            var isUserExists = await _db.Users
                .AnyAsync(u => u.id == newtaskRegDTO.leader_ID);

            if (!isUserExists)
            {
                return NotFound(new
                {
                    message = $"Không tìm thấy tài khoản nào có ID = {newtaskRegDTO.leader_ID} trong hệ thống."
                });
            }


            var hasValidRole = await _db.Users
                .AnyAsync(u => u.id == newtaskRegDTO.leader_ID
                           && (u.role == UserRole.leader || u.role == UserRole.admin));

            if (!hasValidRole)
            {
                return BadRequest(new
                {
                    message = "Tạo task thất bại! Tài khoản này không có quyền hạn hợp lệ (Phải là Leader hoặc Admin) để giao việc."
                });
            }

            var usercheck = await _db.Users.AnyAsync(u => u.id == newtaskRegDTO.User_ID);
            if (!usercheck)
            {
                return NotFound(new
                {
                    message = $"Không tìm thấy thực tập sinh có ID = {newtaskRegDTO.User_ID} trong hệ thống."
                });
            }

            var newTask = new task_reg
            {
                leader_ID = newtaskRegDTO.leader_ID,
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

        // TTS cập nhật tiến độ và trạng thái của task
        [HttpPut]
        public async Task<IActionResult> UpdateTask(int taskId, task_regDTO updateDTO)
        {
            if (updateDTO.progress < 0 || updateDTO.progress > 100)
            {
                return BadRequest(new { message = "Tiến độ công việc phải nằm trong khoảng từ 0% đến 100%." });
            }

            var task = await _db.Task_Regs.FirstOrDefaultAsync(t => t.id == taskId);
            if (task == null)
            {
                return NotFound(new
                {
                    message = $"Không tìm thấy task nào có ID = {taskId} trong hệ thống."
                });
            }

            if (task.statusTask == StatusTask.done)
            {
                return BadRequest(new { message = "Task này đã hoàn thành và đóng lại, không thể sửa đổi tiến độ." });
            }

            task.progress = updateDTO.progress;

            if (updateDTO.progress == 100)
            {
                task.statusTask = StatusTask.done;
            }
            else
            {
                task.statusTask = StatusTask.in_progress;
            }

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
        public async Task<IActionResult> AdminUpdateTask(int taskId, int adminId, [FromBody] task_regDTO updateDTO)
        {
            var isAdminValid = await _db.Users
                .AnyAsync(u => u.id == adminId && (u.role == UserRole.admin || u.role == UserRole.leader));

            if (!isAdminValid)
            {
                return BadRequest(new
                {
                    message = "Thao tác thất bại! Bạn không có quyền Admin hoặc Leader để chỉnh sửa các thông tin cốt lõi của task."
                });
            }

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
                message = $"Admin (ID: {adminId}) đã cập nhật toàn bộ thông tin task {taskId} thành công.",
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

        // Xem danh sách task dành cho leader
        [HttpGet("list")]
        public IActionResult GetAllTasks(int leaderID)
        {
            var tasks = _db.Task_Regs
                .Include(t => t.Users)
                .Where(t => t.leader_ID == leaderID)
                .Take(30)
                .Select(t => new
                {
                    t.id,
                    t.leader_ID,
                    t.tieu_de,
                    t.noi_dung,
                    t.progress,
                    t.statusTask,
                    t.ngay_dang_ki,
                    username = t.Users.tendangnhap
                })
                .ToList();

            if (!tasks.Any())
            {
                return Ok(new
                {
                    message = "Hiện tại chưa có task nào trong hệ thống."
                });
            }

            return Ok(new
            {
                message = "Danh sách tất cả task",
                task = tasks
            });
        }
        [HttpGet("search")]
        public IActionResult SearchTasks(string username, int leaderID)
        {
            var tasks = _db.Task_Regs
                .Include(t => t.Users)
                .Where(t => t.Users.tendangnhap == username && t.leader_ID == leaderID)
                .Select(t => new
                {
                    t.id,
                    t.tieu_de,
                    t.noi_dung,
                    t.progress,
                    t.statusTask,
                    t.ngay_dang_ki,
                    username = t.Users.tendangnhap
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

        // Admin xem danh sách task
        [HttpGet("list-all")]
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
