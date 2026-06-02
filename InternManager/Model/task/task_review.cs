using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace InternManager.Model.task
{
    [Table("task_review")]
    public class task_review
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column("id")]
        public int id { get; set; }


        [Column("review_by_ID")]
        public int review_by_ID { get; set; }

        [ForeignKey("review_by_ID")]
        public virtual users ReviewBy { get; set; }


        [Column("task_ID")]
        public int task_ID { get; set; }

        [ForeignKey("task_ID")]
        public virtual task_reg Task_Reg { get; set; }


        [Required]
        [StringLength(255)]
        [Column("review_content", TypeName = "varchar(255)")]
        public String noidung_danhgia { get; set; } = String.Empty;


        [Column("create_at", TypeName = "date")]
        public DateOnly ngay_dang_ki { get; set; } = DateOnly.FromDateTime(DateTime.Now);
    }
}
