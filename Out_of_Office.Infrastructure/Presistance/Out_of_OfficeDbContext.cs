using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Out_of_Office.Domain.Entities;
using Out_of_Office.Infrastructure.Identity;


namespace Out_of_Office.Infrastructure.Presistance
{
    public class Out_of_OfficeDbContext : IdentityDbContext<ApplicationUser>
    {
        public Out_of_OfficeDbContext(DbContextOptions<Out_of_OfficeDbContext> options)
        : base(options)
        {

        }
        public DbSet<Employee> Employees { get; set; }
        public DbSet<Project> Projects { get; set; }
        public DbSet<LeaveRequest> LeaveRequests { get; set; }
        public DbSet<ApprovalRequest> ApprovalRequests { get; set; }
        public DbSet<EmployeeProject> EmployeeProjects { get; set; }
        public DbSet<LeaveBalance> LeaveBalances { get; set; }
        public DbSet<WorkCalendarDay> WorkCalendarDays { get; set; }
        public DbSet<ApplicationUser> Users { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<ApplicationUser>()
            .HasOne(u => u.Employee)
            .WithOne() 
            .HasForeignKey<ApplicationUser>(u => u.EmployeeId)
            .OnDelete(DeleteBehavior.SetNull);
            modelBuilder.Entity<Employee>(entity =>
            {

                entity.HasKey(e => e.Id);

                entity.Property(e => e.FullName)
                      .IsRequired()
                      .IsUnicode(false)
                      .HasMaxLength(100);

                entity.Property(e => e.Subdivision)
                      .IsRequired()
                      .HasMaxLength(50);

                entity.Property(e => e.Position)
                      .IsRequired()
                      .HasMaxLength(50);

                entity.Property(e => e.Status)
                      .IsRequired()
                      .HasConversion<string>();

                entity.Property(e => e.OutOfOfficeBalance)
                      .IsRequired()
                      .HasDefaultValue(0);

                entity.Property(e => e.Photo)
                      .IsRequired(false)
                      .HasColumnType("varbinary(max)");
            });
            modelBuilder.Entity<LeaveBalance>(entity =>
            {
                entity.HasKey(lb => new { lb.EmployeeId, lb.Type });

                entity.HasOne(lb => lb.Employee)
                      .WithMany(e => e.LeaveBalances)
                      .HasForeignKey(lb => lb.EmployeeId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.Property(lb => lb.Type)
                      .IsRequired()
                      .HasConversion<string>();

                entity.Property(lb => lb.DaysAvailable)
                      .IsRequired();
            });

            modelBuilder.Entity<LeaveRequest>(entity =>
            {
                entity.HasKey(e => e.ID);

                entity.HasOne(e => e.Employee)
                      .WithMany()
                      .HasForeignKey(e => e.EmployeeID)
                      .OnDelete(DeleteBehavior.Restrict)
                      .IsRequired();

                entity.Property(e => e.AbsenceReason)
                      .IsRequired();

                entity.Property(e => e.StartDate)
                      .IsRequired();

                entity.Property(e => e.EndDate)
                      .IsRequired();

                entity.Property(e => e.Comment)
                      .HasMaxLength(500);

                entity.Property(e => e.Status)
                      .IsRequired()
                      .HasConversion<string>();
            });

            modelBuilder.Entity<Project>(entity =>
            {
                entity.HasKey(e => e.ID);

                entity.Property(e => e.ProjectType)
                      .IsRequired();

                entity.Property(e => e.StartDate)
                      .IsRequired();

                entity.Property(e => e.EndDate)
                      .IsRequired(false);

                entity.HasOne(e => e.ProjectManager)
                      .WithMany()
                      .HasForeignKey(e => e.ProjectManagerID)
                      .OnDelete(DeleteBehavior.Restrict)
                      .IsRequired();

                entity.Property(e => e.Comment)
                      .HasMaxLength(500);

                entity.Property(e => e.Status)
                      .IsRequired()
                      .HasConversion<string>();  
            });

            modelBuilder.Entity<ApprovalRequest>(entity =>
            {
                entity.HasKey(e => e.ID);  

                entity.HasOne(e => e.Approver)
                      .WithMany() 
                      .HasForeignKey(e => e.ApproverID)
                      .OnDelete(DeleteBehavior.Restrict)
                      .IsRequired();

                entity.HasOne(e => e.LeaveRequest)
                      .WithMany()  
                      .HasForeignKey(e => e.LeaveRequestID) 
                      .IsRequired();

                entity.Property(e => e.Status)
                      .IsRequired()
                      .HasConversion<string>(); 
                entity.Property(e => e.Comment)
                      .HasMaxLength(1000);  
            });
            modelBuilder.Entity<EmployeeProject>(entity =>
            {
                entity.HasKey(ep => new { ep.EmployeeId, ep.ProjectId });

                entity.HasOne(ep => ep.Employee)
                      .WithMany(e => e.EmployeeProjects)
                      .HasForeignKey(ep => ep.EmployeeId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(ep => ep.Project)
                      .WithMany(p => p.EmployeeProjects)
                      .HasForeignKey(ep => ep.ProjectId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.Property(ep => ep.IsProjectManager)
                      .IsRequired();
            });
            modelBuilder.Entity<WorkCalendarDay>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Description).HasMaxLength(200);
            });
            base.OnModelCreating(modelBuilder);
        }
    }
}

