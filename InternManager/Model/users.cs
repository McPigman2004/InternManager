using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using InternManager.Model.attend;
using InternManager.Model.info;
using InternManager.Model.task;

namespace InternManager.Model
{
    [Table("user")]
    public class users
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column("id")]
        public int id { get; set; }


        [Required]
        [StringLength(50)]
        [Column("username", TypeName = "varchar(50)")]
        public String tendangnhap { get; set; } = String.Empty;


        [Required]
        [StringLength(255)]
        [Column("password", TypeName = "varchar(255)")]
        public String matkhau { get; set; } = String.Empty;


        [Required]
        [StringLength(50)]
        [Column("role", TypeName = "varchar(50)")]
        public UserRole role { get; set; }


        [Column("create_at", TypeName = "date")]
        public DateOnly ngaytao { get; set; } = DateOnly.FromDateTime(DateTime.Now);


        [Column("active", TypeName = "varchar(50)")]
        public UserStatus status { get; set; } = UserStatus.active;


        // -- user info
        public virtual users_info? userInfo { get; set; }

        // -- attend
        public virtual ICollection<reg_schedule_intern> Reg_Schedule_Interns { get; set; } = new List<reg_schedule_intern>();
        public virtual ICollection<attendance_checkin> Attendance_checkin { get; set; } = new List<attendance_checkin>();
        public virtual ICollection<attendance_checkout> Attendance_Checkouts { get; set; } = new List<attendance_checkout>();

        // -- task
        public virtual ICollection<task_reg> Task_Regs { get; set; } = new List<task_reg>();
        public virtual ICollection<task_review> Task_Reviews { get; set; } = new List<task_review>();

    }

    public enum UserRole
    {
        [Display(Name = "TTS")]
        tts,
        [Display(Name = "Leader")]
        leader,
        [Display(Name = "Admin")]
        admin,
    }

    public enum UserStatus
    {
        [Display(Name = "Hoạt động")]
        active,
        [Display(Name = "Dừng hoạt động")]
        inactive
    }
}
