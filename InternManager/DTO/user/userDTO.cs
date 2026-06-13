using System.ComponentModel.DataAnnotations;
using InternManager.Model;

namespace InternManager.DTO.user
{
    public class userDTO
    {
        /// <summary>
        /// Tên đăng nhập của người dùng
        /// </summary>
        /// <example>nguyenvana</example>
        [MinLength(3, ErrorMessage = "Tên đăng nhập tối thiểu 3 kí tự")]
        [StringLength(50, ErrorMessage = "Tên đăng nhập tối đa 50 kí tự.")]
        [RegularExpression(@"^[a-zA-Z0-9_]+$", ErrorMessage = "Tên đăng nhập không được chứa dấu, khoảng trắng hoặc kí tự đặc biệt.")]
        public string username { get; set; } = "nguyenvana";

        /// <summary>
        /// Mật khẩu của người dùng
        /// </summary>
        /// <example>123456</example>
        [MinLength(6, ErrorMessage = "Mật khẩu phải có tối thiểu 6 kí tự.")]
        [RegularExpression(@"^\d+$", ErrorMessage = "Mật khẩu chỉ được chứa các chữ số.")]
        public string password { get; set; } = "123456";


        [EnumDataType(typeof(UserRole), ErrorMessage = "Quyền không hợp lệ. Chỉ chấp nhận các giá trị: Admin, Leader, TTS.")]
        public UserRole role { get; set; } = UserRole.tts;

        public DateOnly create_at { get; set; } = DateOnly.FromDateTime(DateTime.Now);


        [EnumDataType(typeof(UserStatus), ErrorMessage = "Quyền không hợp lệ. Chỉ chấp nhận các giá trị: active, inactive.")]
        public UserStatus status { get; set; } = UserStatus.active;
    }
}