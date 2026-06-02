using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace InternManager.Model.task
{
    [Table("task_reg")]
    public class task_reg
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column("id")]
        public int id { get; set; }


        [Column("leader_ID")]
        public int leader_ID { get; set; }

        [ForeignKey("leader_ID")]
        public virtual users Leaders { get; set; }


        [Column("User_ID")]
        public int User_ID { get; set; }

        [ForeignKey("User_ID")]
        public virtual users Users { get; set; }


        // VD : Task 28/05 IT
        [Required]
        [StringLength(255)]
        [Column("title", TypeName = "varchar(255)")]
        public String tieu_de { get; set; } = String.Empty;


        [Required]
        [Column("content", TypeName = "text")]
        public String noi_dung { get; set; } = String.Empty;


        [Required]
        [Column("progress", TypeName = "int")]
        public int progress { get; set; } = 0;


        [Required]
        [StringLength(50)]
        [Column("status", TypeName = "varchar(50)")]
        public StatusTask statusTask { get; set; }


        [Column("create_at", TypeName = "date")]
        public DateOnly ngay_dang_ki { get; set; } = DateOnly.FromDateTime(DateTime.Now);


        public virtual ICollection<task_review> Task_Reviews { get; set; } = new List<task_review>();
    }
    public enum StatusTask
    {
        [Display(Name = "Đang thực hiện")]
        in_progress,
        [Display(Name = "Hoàn thành")]
        done,
        [Display(Name = "Đánh giá")]
        review
    }
}
