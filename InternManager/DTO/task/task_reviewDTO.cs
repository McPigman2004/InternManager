using System.ComponentModel.DataAnnotations;

namespace InternManager.DTO.task
{
    public class task_reviewDTO
    {
        public int review_by_user { get; set; }
        public int taskID { get; set; }

        /// <summary>
        /// Lời nhắn, nhận xét hoặc góp ý của Leader dành cho thực tập sinh
        /// </summary>
        /// <example>Code rất tốt, cần tối ưu lại câu lệnh SQL một chút nhé.</example>
        [Required(ErrorMessage = "Nội dung nhận xét (comment) không được để trống.")]
        [StringLength(255, ErrorMessage = "Nội dung nhận xét tối đa là 255 kí tự.")]
        public string comment { get; set; } = string.Empty;

        /// <summary>
        /// Ngày thực hiện đánh giá, mặc định lấy ngày hôm nay
        /// </summary>
        /// <example>2026-06-08</example>
        public DateOnly ngay_dang_ki { get; set; } = DateOnly.FromDateTime(DateTime.Now);
    }
}