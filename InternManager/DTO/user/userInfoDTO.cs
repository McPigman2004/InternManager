using System.ComponentModel.DataAnnotations;
using InternManager.Model;
using InternManager.Model.info;

namespace InternManager.DTO.user
{
    public class userInfoDTO
    {
        public int UserID { get; set; }
        /// <summary>
        /// Họ và tên đầy đủ
        /// </summary>
        /// <example>Nguyễn Văn A</example>
        [Required(ErrorMessage = "Họ tên không được để trống.")]
        [StringLength(255, ErrorMessage = "Họ tên tối đa 255 kí tự.")]
        public string fullname { get; set; } = "Nguyễn Văn A";

        /// <summary>
        /// Ngành học hiện tại
        /// </summary>
        /// <example>Công nghệ thông tin</example>
        [Required(ErrorMessage = "Ngành học không được để trống.")]
        [StringLength(50, ErrorMessage = "Ngành học tối đa 50 kí tự.")]
        public string study { get; set; } = "Công nghệ thông tin";

        /// <summary>
        /// Vị trí thực tập đăng ký
        /// </summary>
        /// <example>Backend Developer</example>
        [Required(ErrorMessage = "Vị trí thực tập không được để trống.")]
        [StringLength(50, ErrorMessage = "Vị trí tối đa 50 kí tự.")]
        public string postion { get; set; } = "Backend Developer";

        /// <summary>
        /// Mã số sinh viên
        /// </summary>
        /// <example>123456789</example>
        [Required(ErrorMessage = "Mã số sinh viên không được để trống.")]
        [StringLength(50, ErrorMessage = "MSSV tối đa 50 kí tự.")]
        public string studentID { get; set; } = "123456789";

        /// <summary>
        /// Trường đang theo học
        /// </summary>
        /// <example>Đại học ABC</example>
        [Required(ErrorMessage = "Tên trường không được để trống.")]
        [StringLength(255, ErrorMessage = "Tên trường tối đa 255 kí tự.")]
        public string school { get; set; } = "Đại học ABC";


        public DateOnly start_intern { get; set; } = DateOnly.FromDateTime(DateTime.Now);

        /// <summary>
        /// Thời gian thực tập (Ví dụ: 3 tháng, 1 học kỳ)
        /// </summary>
        /// <example>3 tháng</example>
        [Required(ErrorMessage = "Thời gian thực tập không được để trống.")]
        public string duration_intern { get; set; } = "3 tháng";

        /// <summary>
        /// Email do nhà trường cấp
        /// </summary>
        /// <example>anvps12345@abc.edu.vn</example>
        [Required(ErrorMessage = "Email trường không được để trống.")]
        [EmailAddress(ErrorMessage = "Email trường không đúng định dạng.")]
        [StringLength(255, ErrorMessage = "Email trường tối đa 255 kí tự.")]
        public string email_school { get; set; } = "anvps12345@abc.edu.vn";

        /// <summary>
        /// Email cá nhân
        /// </summary>
        /// <example>nguyenvana@gmail.com</example>
        [Required(ErrorMessage = "Email cá nhân không được để trống.")]
        [EmailAddress(ErrorMessage = "Email cá nhân không đúng định dạng.")]
        [StringLength(255, ErrorMessage = "Email cá nhân tối đa 255 kí tự.")]
        public string email_personal { get; set; } = "nguyenvana@gmail.com";

        [EnumDataType(typeof(GioiTinh), ErrorMessage = "Giới tính không hợp lệ. Chỉ chấp nhận các giá trị: boy, girl.")]
        public GioiTinh gioi_tinh { get; set; }

        /// <summary>
        /// Điểm trung bình tích lũy
        /// </summary>
        /// <example>3.5</example>
        [Required(ErrorMessage = "Điểm GPA không được để trống.")]
        [Range(0.0, 4.0, ErrorMessage = "GPA phải nằm trong khoảng từ 0.0 đến 4.0")]
        public decimal gpa { get; set; } = 3.5m;

        /// <summary>
        /// Trình độ tiếng Anh (Ví dụ: TOEIC 650, IELTS 6.0, B1)
        /// </summary>
        /// <example>TOEIC 650</example>
        [Required(ErrorMessage = "Trình độ tiếng Anh không được để trống.")]
        [StringLength(100, ErrorMessage = "Trình độ tiếng Anh tối đa 100 kí tự.")]
        public string english_level { get; set; } = "TOEIC 650";

        /// <summary>
        /// Giới thiệu bản thân ngắn gọn
        /// </summary>
        /// <example>Em là sinh viên năm cuối muốn tìm kiếm cơ hội cọ xát thực tế.</example>
        [Required(ErrorMessage = "Lời giới thiệu không được để trống.")]
        [StringLength(255, ErrorMessage = "Lời giới thiệu tối đa 255 kí tự.")]
        public string description { get; set; } = "Em là sinh viên năm cuối muốn tìm kiếm cơ hội cọ xát thực tế.";

        /// <summary>
        /// Địa chỉ tạm trú/thường trú hiện tại
        /// </summary>
        /// <example>Quận 12, TP. Hồ Chí Minh</example>
        [Required(ErrorMessage = "Địa chỉ không được để trống.")]
        [StringLength(255, ErrorMessage = "Địa chỉ tối đa 255 kí tự.")]
        public string location { get; set; } = "Quận 12, TP. Hồ Chí Minh";

        /// <summary>
        /// Đường dẫn Facebook cá nhân
        /// </summary>
        /// <example>https://facebook.com/nguyenvana</example>
        [Required(ErrorMessage = "Link Facebook không được để trống.")]
        [Url(ErrorMessage = "Đường dẫn Facebook không hợp lệ.")]
        [StringLength(255, ErrorMessage = "Link Facebook tối đa 255 kí tự.")]
        public string fb_url { get; set; } = "https://facebook.com/nguyenvana";

        /// <summary>
        /// Số điện thoại liên hệ (10 chữ số)
        /// </summary>
        /// <example>0987654321</example>
        [Required(ErrorMessage = "Số điện thoại không được để trống.")]
        [RegularExpression(@"^\d{10}$", ErrorMessage = "Số điện thoại phải bao gồm đúng 10 chữ số.")]
        public string sdt { get; set; } = "0987654321";

        /// <summary>
        /// Số Căn cước công dân
        /// </summary>
        /// <example>012345678901</example>
        [Required(ErrorMessage = "Số CCCD không được để trống.")]
        [StringLength(50, ErrorMessage = "Số CCCD tối đa 50 kí tự.")]
        [RegularExpression(@"^\d+$", ErrorMessage = "CCCD chỉ được chứa các chữ số.")]
        public string cccd { get; set; } = "012345678901";

        public DateOnly cccd_create { get; set; } = DateOnly.FromDateTime(DateTime.Now);

        /// <summary>
        /// Nơi cấp CCCD (Cục Cảnh sát hoặc Tỉnh/Thành phố)
        /// </summary>
        /// <example>Cục Cảnh sát Quản lý hành chính về trật tự xã hội</example>
        [Required(ErrorMessage = "Nơi cấp CCCD không được để trống.")]
        [StringLength(255, ErrorMessage = "Nơi cấp CCCD tối đa 255 kí tự.")]
        public string cccd_location { get; set; } = "Cục Cảnh sát Quản lý hành chính về trật tự xã hội";

        /// <summary>
        /// Đường dẫn tệp tin CV (PDF/Drive link)
        /// </summary>
        /// <example>https://drive.google.com/cv-nguyenvana.pdf</example>
        [Required(ErrorMessage = "Đường dẫn CV không được để trống.")]
        [StringLength(255, ErrorMessage = "Đường dẫn CV tối đa 255 kí tự.")]
        public string cv { get; set; } = "https://drive.google.com/cv-nguyenvana.pdf";
    }
}
