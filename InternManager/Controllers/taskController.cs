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
    }
}
