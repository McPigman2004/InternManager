using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace InternManager.Model.info
{
    [Table("users_info")]
    public class users_info
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column("id")]
        public int id { get; set; }


        [Column("User_ID")]
        public int User_ID { get; set; }

        [ForeignKey("User_ID")]
        public virtual users Users { get; set; } = null!;


        [Required]
        [StringLength(255)]
        [Column("fullname", TypeName = "varchar(255)")]
        public String hoten { get; set; } = String.Empty;


        [Required]
        [StringLength(50)]
        [Column("study", TypeName = "varchar(50)")]
        public String nganh_hoc { get; set; } = String.Empty;


        [Required]
        [StringLength(50)]
        [Column("postion", TypeName = "varchar(50)")]
        public String vi_tri { get; set; } = String.Empty;


        [Required]
        [StringLength(50)]
        [Column("student_ID", TypeName = "varchar(50)")]
        public String mssv { get; set; } = String.Empty;


        [Required]
        [StringLength(255)]
        [Column("school", TypeName = "varchar(255)")]
        public String truong { get; set; } = String.Empty;


        [Column("start_time", TypeName = "date")]
        public DateOnly ngay_bat_dau { get; set; } = DateOnly.FromDateTime(DateTime.Now);


        [Required]
        [StringLength(100)]
        [Column("duration_time", TypeName = "varchar(100)")]
        public String thoi_gian_thuctap { get; set; } = String.Empty;


        [Required]
        [StringLength(255)]
        [Column("email_school", TypeName = "varchar(255)")]
        public String email_truong { get; set; } = String.Empty;


        [Required]
        [StringLength(255)]
        [Column("email_personal", TypeName = "varchar(255)")]
        public String email_ca_nhan { get; set; } = String.Empty;


        [Required]
        [StringLength(50)]
        [Column("gioi_tinh", TypeName = "varchar(50)")]
        public GioiTinh gioi_tinh { get; set; }


        [Required]
        [Column("gpa", TypeName = "decimal(10,2)")]
        public decimal gpa { get; set; }


        [Required]
        [StringLength(100)]
        [Column("english_level", TypeName = "varchar(100)")]
        public string trinh_do_tieng_anh { get; set; } = string.Empty;


        [Required]
        [StringLength(255)]
        [Column("description", TypeName = "varchar(255)")]
        public string gioi_thieu { get; set; } = string.Empty;


        [Required]
        [StringLength(255)]
        [Column("location", TypeName = "varchar(255)")]
        public string dia_chi { get; set; } = string.Empty;


        [Required]
        [StringLength(255)]
        [Column("fb", TypeName = "varchar(255)")]
        public string fb_url { get; set; } = string.Empty;

        [Required]
        [StringLength(255)]
        [Column("sdt", TypeName = "varchar(10)")]
        public string sdt { get; set; } = string.Empty;


        [Required]
        [StringLength(255)]
        [Column("cccd", TypeName = "varchar(50)")]
        public string cccd { get; set; } = string.Empty;


        [Column("cccd_create", TypeName = "date")]
        public DateOnly ngay_cap_cccd { get; set; } = DateOnly.FromDateTime(DateTime.Now);


        [Required]
        [StringLength(255)]
        [Column("cccd_location", TypeName = "varchar(255)")]
        public string noi_cap_cccd { get; set; } = string.Empty;


        [Required]
        [StringLength(255)]
        [Column("cv", TypeName = "varchar(255)")]
        public string cv { get; set; } = string.Empty;
    }

    public enum GioiTinh
    {
        [Display(Name = "Nam")]
        boy,
        [Display(Name = "Nữ")]
        girl
    }
}
