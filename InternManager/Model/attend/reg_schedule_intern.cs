using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace InternManager.Model.attend
{
    [Table("reg_schedule_intern")]
    public class reg_schedule_intern
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column("id")]
        public int id { get; set; }


        [Column("User_ID")]
        public int User_ID { get; set; }

        [ForeignKey("User_ID")]
        public virtual users Users { get; set; }


        [Required]
        [StringLength(255)]
        [Column("day", TypeName = "varchar(50)")]
        public String thu_trong_tuan { get; set; } = String.Empty;


        [Required]
        [Column("session", TypeName = "varchar(50)")]
        public CaLam ca_lam { get; set; }


        [Column("create_at", TypeName = "date")]
        public DateOnly ngay_dang_ki { get; set; } = DateOnly.FromDateTime(DateTime.Now);


        [Required]
        [Column("status", TypeName = "varchar(50)")]
        public StatusRegIntern status { get; set; } = StatusRegIntern.reg;

        public virtual attendance_checkin? Attendance_Checkin { get; set; }
        public virtual attendance_checkout? Attendance_Checkout { get; set; }
    }

    public enum CaLam
    {
        [Display(Name = "sáng")]
        morning,
        [Display(Name = "chiều")]
        afternoon
    }

    public enum StatusRegIntern
    {
        [Display(Name = "Đăng kí")]
        reg,
        [Display(Name = "Hủy đăng kí")]
        cancel,
        [Display(Name = "Đã đi")]
        on,
        [Display(Name = "Nghĩ")]
        off
    }
}
