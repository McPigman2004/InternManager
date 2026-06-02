using InternManager.Model;
using InternManager.Model.attend;
using InternManager.Model.info;
using InternManager.Model.task;
using Microsoft.EntityFrameworkCore;

namespace InternManager.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        public DbSet<users> Users { get; set; }
        public DbSet<users_info> Users_Infos { get; set; }

        public DbSet<reg_schedule_intern> Reg_Schedule_Interns { get; set; }
        public DbSet<attendance_checkin> Attendance_Checkins { get; set; }
        public DbSet<attendance_checkout> Attendance_Checkouts { get; set; }

        public DbSet<task_reg> Task_Regs { get; set; }
        public DbSet<task_review> Task_Reviews { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<users>(entity =>
            {
                entity.HasIndex(u => u.tendangnhap)
                      .IsUnique();
            });

            modelBuilder.Entity<users_info>(entity =>
            {
                entity.HasIndex(ui => ui.email_ca_nhan)
                      .IsUnique();
                
                entity.HasIndex(ui => ui.email_truong)
                      .IsUnique();

                entity.HasIndex(ui => ui.sdt)
                      .IsUnique();

                entity.HasIndex(ui => ui.cccd)
                      .IsUnique();

                entity.HasOne(ui => ui.Users)
                      .WithOne(u => u.userInfo)
                      .HasForeignKey<users_info>(ui => ui.User_ID)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<reg_schedule_intern>(entity =>
            {
                entity.HasOne(rsi => rsi.Users)
                      .WithMany(u => u.Reg_Schedule_Interns)
                      .HasForeignKey(rsi => rsi.User_ID)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(rsi => rsi.Attendance_Checkin)
                      .WithOne(ci => ci.Reg_Schedule_Intern)
                      .HasForeignKey<attendance_checkin>(ci => ci.reg_intern_ID)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(rsi => rsi.Attendance_Checkout)
                      .WithOne(co => co.Reg_Schedule_Intern)
                      .HasForeignKey<attendance_checkout>(co => co.reg_intern_ID)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<attendance_checkin>(entity =>
            {
                entity.HasOne(ci => ci.Users)
                      .WithMany(u => u.Attendance_checkin)
                      .HasForeignKey(ci => ci.User_ID)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<attendance_checkout>(entity =>
            {
                entity.HasOne(co => co.Users)
                      .WithMany(u => u.Attendance_Checkouts)
                      .HasForeignKey(co => co.User_ID)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<task_reg>(entity =>
            {
                entity.HasOne(t => t.Leaders)
                      .WithMany()
                      .HasForeignKey(t => t.leader_ID)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(t => t.Users)
                      .WithMany(u => u.Task_Regs)
                      .HasForeignKey(t => t.User_ID)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<task_review>(entity =>
            {
                entity.HasOne(tr => tr.ReviewBy)
                      .WithMany(u => u.Task_Reviews)
                      .HasForeignKey(tr => tr.review_by_ID)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(tr => tr.Task_Reg)
                      .WithMany(t => t.Task_Reviews)
                      .HasForeignKey(tr => tr.task_ID)
                      .OnDelete(DeleteBehavior.Cascade);
            });
        }
    }
}