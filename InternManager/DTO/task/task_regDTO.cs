using System.ComponentModel.DataAnnotations;
using InternManager.Model;
using InternManager.Model.task;

namespace InternManager.DTO.task
{
    public class task_regDTO
    {
        public int leader_ID { get; set; }
        public int User_ID { get; set; }


        /// <summary>
        /// Tiêu đề
        /// </summary>
        /// <example>Task ngày 01/06</example>
        [Required(ErrorMessage = "Tiêu đề task không được để trống.")]
        [StringLength(255, ErrorMessage = "Tiêu đề task tối đa 255 kí tự.")]
        public String title { get; set; } = "Task IT ngày 01/06";


        /// <summary>
        /// Nội dung
        /// </summary>
        /// <example>Task ngày 01/06</example>
        [Required(ErrorMessage = "Nội dung task không được để trống.")]
        [StringLength(255, ErrorMessage = "Nội dung task tối đa 255 kí tự.")]
        public String content { get; set; } = "Hoàn thiện backend";


        /// <summary>
        /// Tiến độ công việc, mặc định 0% khi mới tạo task, có thể cập nhật sau khi thực tập sinh hoàn thành một phần công việc.
        /// </summary>
        /// <example>0</example>
        public int progress { get; set; } = 0;


        [EnumDataType(typeof(StatusTask), ErrorMessage = "Trạng thái task không hợp lệ. Chỉ chấp nhận các giá trị: in_progress, done, review.")]
        public StatusTask statusTask { get; set; } = StatusTask.in_progress;



        public DateOnly ngay_dang_ki { get; set; } = DateOnly.FromDateTime(DateTime.Now);
    }
}
