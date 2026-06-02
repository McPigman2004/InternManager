using System.ComponentModel.DataAnnotations;
using InternManager.Model.attend;
using InternManager.Model.task;

namespace InternManager.DTO.attend
{
    public class checkInDTO
    {
        public int User_ID { get; set; }
        public int reg_intern_ID { get; set; }
        public DateTime checkin { get; set; } = DateTime.Now;
        public double kinh_do { get; set; }
        public double vi_do { get; set; }


        [EnumDataType(typeof(StatusCheckIn), ErrorMessage = "Trạng thái check-in không hợp lệ. Chỉ chấp nhận các giá trị: success, failed.")]
        public StatusCheckIn statusCheckIn { get; set; } = StatusCheckIn.success;


        /// <summary>
        /// Ghi chú lúc check in
        /// </summary>
        /// <example>Điểm danh thành công</example>
        public String note { get; set; } = "Điểm danh thành công";
    }
}
