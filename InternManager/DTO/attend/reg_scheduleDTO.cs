using System.ComponentModel.DataAnnotations;
using InternManager.Model.attend;

namespace InternManager.DTO.attend
{
    public class reg_scheduleDTO
    {
        public int UserID { get; set; }

        /// <summary>
        /// Thứ trong tuần (Ví dụ: Thứ 2, Thứ 3...)
        /// </summary>
        /// <example>Thứ 2</example>
        [Required(ErrorMessage = "Thứ trong tuần không được để trống.")]
        [StringLength(50, ErrorMessage = "Tên thứ không được vượt quá 20 kí tự.")]
        public string thu_trong_tuan { get; set; } = "Thứ 2";

        /// <summary>
        /// Ca làm đăng ký (Dựa trên cấu trúc Enum CaLam)
        /// </summary>
        /// <example>morning</example> 
        [Required(ErrorMessage = "Ca làm không được để trống.")]
        [EnumDataType(typeof(CaLam), ErrorMessage = "Ca làm không hợp lệ. Vui lòng chọn (morning<sáng> ,afternoon<chiều> ).")]
        public CaLam ca_lam { get; set; }

        /// <summary>
        /// Ngày đăng ký lịch làm (Định dạng: YYYY-MM-DD)
        /// </summary>
        /// <example>2026-05-29</example>
        [Required(ErrorMessage = "Ngày đăng ký không được để trống.")]
        public DateOnly ngay_dang_ki { get; set; } = DateOnly.FromDateTime(DateTime.Now);

        /// <summary>
        /// Trạng thái của lịch đăng ký (Mặc định là 'reg' - Mới đăng ký)
        /// </summary>
        /// <example>reg</example>
        [Required(ErrorMessage = "Trạng thái không được để trống.")]
        [EnumDataType(typeof(StatusRegIntern), ErrorMessage = "Trạng thái không hợp lệ. Vui lòng chọn (reg, cancel, on, off).")]
        public StatusRegIntern status { get; set; } = StatusRegIntern.reg;
    }
}