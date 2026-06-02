using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace InternManager.Model.attend
{
    [Table("attendance_checkin")]
    public class attendance_checkin
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column("id")]
        public int id { get; set; }


        [Column("User_ID")]
        public int User_ID { get; set; }

        [ForeignKey("User_ID")]
        public virtual users Users { get; set; }


        [Column("reg_intern_ID")]
        public int reg_intern_ID { get; set; }

        [ForeignKey("reg_intern_ID")]
        public virtual reg_schedule_intern Reg_Schedule_Intern { get; set; }


        [Column("check_in_time", TypeName = "date")]
        public DateTime checkin { get; set; } = DateTime.Now;


        [Column("latitude_in", TypeName = "decimal(18,12)")]
        public decimal vi_do { get; set; }


        [Column("longitude_in", TypeName = "decimal(18,12)")]
        public decimal kinh_do { get; set; }


        [Column("status", TypeName = "varchar(50)")]
        public StatusCheckIn statusCheckIn { get; set; }


        [Required]
        [StringLength(255)]
        [Column("note", TypeName = "varchar(255)")]
        public String ghi_chu { get; set; } = String.Empty;
    }

    public enum StatusCheckIn
    {
        [Display(Name = "Thành công")]
        success,
        [Display(Name = "Thất bại")]
        fail,
    }
}
