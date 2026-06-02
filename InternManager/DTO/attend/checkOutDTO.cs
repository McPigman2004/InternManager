using System.ComponentModel.DataAnnotations;
using InternManager.Model.attend;

namespace InternManager.DTO.attend
{
    public class checkOutDTO
    {
        public int User_ID { get; set; }
        public int reg_intern_ID { get; set; }
        public DateTime checkout { get; set; } = DateTime.Now;
        public double kinh_do { get; set; }
        public double vi_do { get; set; }


        [EnumDataType(typeof(StatusCheckOut), ErrorMessage = "Trạng thái check-out không hợp lệ. Chỉ chấp nhận các giá trị: success, failed.")]
        public StatusCheckOut statusCheckOut { get; set; } = StatusCheckOut.success;


        /// <summary>
        /// Ghi chú lúc check in
        /// </summary>
        /// <example>Check out thành công</example>
        public String note { get; set; } = "Check out thành công";
    }
}
