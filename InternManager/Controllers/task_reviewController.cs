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
    [Route("api/task-reviews")]
    [ApiController]
    [Authorize]
    public class task_reviewController : ControllerBase
    {
        private readonly ApplicationDbContext _db;

        public task_reviewController(ApplicationDbContext db)
        {
            _db = db;
        }

        [HttpGet]
        [Authorize(Roles = "tts")]
        public IActionResult GetTaskReviews(int UserID)
        {
            var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            // Nếu không tìm thấy danh tính từ Cookie, hệ thống mới đá ra
            if (string.IsNullOrEmpty(userIdStr))
            {
                return Unauthorized(new { message = "Bạn chưa đăng nhập hoặc phiên làm việc đã hết hạn." });
            }

            UserID = int.Parse(userIdStr);

            var reviews = _db.Task_Reviews
                .Where(r => r.Task_Reg.User_ID == UserID)
                .OrderByDescending(r => r.ngay_dang_ki)
                .Take(30) 
                .Select(r => new TaskReviewResponseDTO
                {
                    id = r.id,
                    comment = r.noidung_danhgia,
                    ngay_danh_gia = r.ngay_dang_ki,

                    reviewer = _db.Users.Where(u => u.id == r.review_by_ID).Select(u => u.tendangnhap).FirstOrDefault() ?? "",

                    task_id = r.Task_Reg.id,
                    tieu_de = r.Task_Reg.tieu_de,
                    noi_dung = r.Task_Reg.noi_dung,
                    progress = r.Task_Reg.progress,
                    statusTask = r.Task_Reg.statusTask.ToString(),
                    username = r.Task_Reg.Users.tendangnhap,
                    leader = r.Task_Reg.Leaders.tendangnhap
                })
                .ToList();

            if (!reviews.Any())
            {
                return Ok(new
                {
                    message = $"Thực tập sinh có ID = {UserID} chưa có đánh giá task nào trong hệ thống.",
                    taskReviews = reviews 
                });
            }

            return Ok(new
            {
                message = $"Danh sách đánh giá task của thực tập sinh có ID = {UserID}",
                taskReviews = reviews
            });
        }

        [HttpGet("list-all")]
        [Authorize(Roles = "admin")]
        public IActionResult GetAllTaskReviews()
        {
            var reviews = _db.Task_Reviews
                    .Include(r => r.Task_Reg)
                        .ThenInclude(t => t.Users)
                    .Include(r => r.Task_Reg)
                        .ThenInclude(t => t.Leaders)
                    .OrderByDescending(r => r.ngay_dang_ki)
                    .Take(30)
                    .Select(r => new TaskReviewResponseDTO
                    {
                        id = r.id,
                        comment = r.noidung_danhgia,
                        ngay_danh_gia = r.ngay_dang_ki,

                        reviewer = _db.Users.Where(u => u.id == r.review_by_ID).Select(u => u.tendangnhap).FirstOrDefault() ?? "",

                        task_id = r.Task_Reg.id,
                        tieu_de = r.Task_Reg.tieu_de,
                        noi_dung = r.Task_Reg.noi_dung,
                        progress = r.Task_Reg.progress,
                        statusTask = r.Task_Reg.statusTask.ToString(),
                        username = r.Task_Reg.Users.tendangnhap,
                        leader = r.Task_Reg.Leaders.tendangnhap
                    })
                    .ToList();

            if (!reviews.Any())
            {
                return Ok(new
                {
                    message = "Hiện tại chưa có đánh giá task nào trong hệ thống."
                });
            }

            return Ok(new
            {
                message = "Danh sách tất cả đánh giá và nội dung task",
                taskReviews = reviews
            });
        }

        [HttpPost]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> ReviewTask(task_reviewDTO newReviewsTask)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdStr)) return Unauthorized("Phiên làm việc không hợp lệ.");
            int adminID = int.Parse(userIdStr);

            var task = await _db.Task_Regs.FirstOrDefaultAsync(t => t.id == newReviewsTask.taskID);
            if (task == null)
            {
                return NotFound(new
                {
                    message = $"Không tìm thấy task có ID = {newReviewsTask.taskID} để đánh giá."
                });
            }

            var isAlreadyReviewed = await _db.Task_Reviews.AnyAsync(r => r.task_ID == newReviewsTask.taskID);
            if (isAlreadyReviewed)
            {
                return BadRequest(new
                {
                    message = $"Thao tác thất bại! Task có ID = {newReviewsTask.taskID} này đã được đánh giá rồi, không thể tạo thêm review mới."
                });
            }

            if (task.statusTask != StatusTask.done)
            {
                return BadRequest(new
                {
                    message = $"Thao tác thất bại! Task này hiện đang ở trạng thái '{task.statusTask}'. Chỉ những task đã 'Hoàn thành (done)' mới được phép đánh giá."
                });
            }

            var newReview = new task_review
            {
                review_by_ID = adminID, // 🌟 Thay vì dùng DTO, đắp adminID lấy từ Cookie vào đây!
                task_ID = newReviewsTask.taskID,
                noidung_danhgia = newReviewsTask.comment.Trim(),
                ngay_dang_ki = newReviewsTask.ngay_dang_ki
            };

            _db.Task_Reviews.Add(newReview);

            task.statusTask = StatusTask.review;
            _db.Task_Regs.Update(task);

            await _db.SaveChangesAsync();

            var reviewerName = await _db.Users.Where(u => u.id == adminID).Select(u => u.tendangnhap).FirstOrDefaultAsync();
            var internName = await _db.Users.Where(u => u.id == task.User_ID).Select(u => u.tendangnhap).FirstOrDefaultAsync();

            return Ok(new
            {
                message = "Gửi nhận xét và đánh giá task thành công.",
                reviewInfo = new
                {
                    newReview.id,
                    reviewerName = reviewerName,
                    internName = internName,
                    taskTitle = task.tieu_de,
                    comment = newReview.noidung_danhgia,
                    ngayDanhGia = newReview.ngay_dang_ki.ToString("dd/MM/yyyy")
                }
            });
        }

        [HttpPut]
        [Authorize(Roles = "admin,leader")]
        public async Task<IActionResult> UpdateReviewTask(int reviewId, task_reviewDTO updateReviewDTO)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdStr)) return Unauthorized("Phiên làm việc không hợp lệ.");
            int adminID = int.Parse(userIdStr);

            var review = await _db.Task_Reviews.FirstOrDefaultAsync(r => r.id == reviewId);
            if (review == null)
            {
                return NotFound(new { message = $"Không tìm thấy bản ghi đánh giá nào có ID = {reviewId} trong hệ thống." });
            }

            review.review_by_ID = adminID;
            review.noidung_danhgia = updateReviewDTO.comment.Trim();
            review.ngay_dang_ki = updateReviewDTO.ngay_dang_ki;

            await _db.SaveChangesAsync();

            var reviewerName = await _db.Users.Where(u => u.id == adminID).Select(u => u.tendangnhap).FirstOrDefaultAsync();
            var taskTitle = await _db.Task_Regs.Where(t => t.id == review.task_ID).Select(t => t.tieu_de).FirstOrDefaultAsync();

            return Ok(new
            {
                message = "Chỉnh sửa đánh giá thành công.",
                updatedReview = new
                {
                    review.id,
                    reviewerName = reviewerName,
                    taskTitle = taskTitle,
                    comment = review.noidung_danhgia,
                    ngayCapNhat = review.ngay_dang_ki.ToString("dd/MM/yyyy")
                }
            });
        }

        [HttpDelete]
        [Authorize(Roles = "admin")]
        public IActionResult DeleteReviewTask(int reviewId)
        {
            var review = _db.Task_Reviews.FirstOrDefault(r => r.id == reviewId);
            if (review == null)
            {
                return NotFound(new
                {
                    message = $"Không tìm thấy bản ghi đánh giá nào có ID = {reviewId} trong hệ thống."
                });
            }
            _db.Task_Reviews.Remove(review);
            _db.SaveChanges();
            return Ok(new
            {
                message = $"Xóa đánh giá có ID = {reviewId} thành công."
            });
        }

        [HttpGet("search")]
        [Authorize(Roles = "admin")]
        public IActionResult GetSearchTaskReviews(string username)
        {
            var reviews = _db.Task_Reviews
                    .Include(r => r.Task_Reg)
                        .ThenInclude(t => t.Users)
                    .Include(r => r.Task_Reg)
                        .ThenInclude(t => t.Leaders)
                    .Where(r => r.Task_Reg.Users.tendangnhap == username)
                    .OrderByDescending(r => r.ngay_dang_ki)
                    .Select(r => new TaskReviewResponseDTO
                    {
                        id = r.id,
                        comment = r.noidung_danhgia,
                        ngay_danh_gia = r.ngay_dang_ki,

                        reviewer = _db.Users.Where(u => u.id == r.review_by_ID).Select(u => u.tendangnhap).FirstOrDefault() ?? "",

                        task_id = r.Task_Reg.id,
                        tieu_de = r.Task_Reg.tieu_de,
                        noi_dung = r.Task_Reg.noi_dung,
                        progress = r.Task_Reg.progress,
                        statusTask = r.Task_Reg.statusTask.ToString(),
                        username = r.Task_Reg.Users.tendangnhap,
                        leader = r.Task_Reg.Leaders.tendangnhap
                    })
                    .ToList();

            if (!reviews.Any())
            {
                return Ok(new
                {
                    message = "Hiện tại chưa có đánh giá task nào trong hệ thống."
                });
            }

            return Ok(new
            {
                message = "Danh sách tất cả đánh giá và nội dung task",
                taskReviews = reviews
            });
        }
    }
}
