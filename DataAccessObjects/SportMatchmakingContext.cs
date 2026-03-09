using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace BusinessObjects;

public partial class SportMatchmakingContext : DbContext
{
    public SportMatchmakingContext()
    {
    }

    public SportMatchmakingContext(DbContextOptions<SportMatchmakingContext> options)
        : base(options)
    {
    }

    public virtual DbSet<AppUser> AppUsers { get; set; }

    public virtual DbSet<ChatMessage> ChatMessages { get; set; }

    public virtual DbSet<ChatThread> ChatThreads { get; set; }

    public virtual DbSet<ChatThreadMember> ChatThreadMembers { get; set; }

    public virtual DbSet<JoinRequest> JoinRequests { get; set; }

    public virtual DbSet<MatchPost> MatchPosts { get; set; }

    public virtual DbSet<Notification> Notifications { get; set; }

    public virtual DbSet<PostParticipant> PostParticipants { get; set; }

    public virtual DbSet<Report> Reports { get; set; }

    public virtual DbSet<Role> Roles { get; set; }

    public virtual DbSet<Sport> Sports { get; set; }

    public virtual DbSet<SportImage> SportImages { get; set; }
    public virtual DbSet<EmailVerification> EmailVerifications { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        => optionsBuilder.UseSqlServer("Name=ConnectionStrings:mvc_b1Context");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<AppUser>(entity =>
        {
            entity.HasKey(e => e.UserId);

            entity.HasIndex(e => e.RoleId, "IX_AppUsers_RoleId");

            entity.HasIndex(e => e.Email, "UQ_AppUsers_Email").IsUnique();

            entity.HasIndex(e => e.UserName, "UQ_AppUsers_UserName").IsUnique();

            entity.Property(e => e.AvatarUrl).HasMaxLength(500);
            entity.Property(e => e.Bio).HasMaxLength(500);
            entity.Property(e => e.City).HasMaxLength(100);
            entity.Property(e => e.CreatedAt)
                .HasPrecision(0)
                .HasDefaultValueSql("(sysutcdatetime())");
            entity.Property(e => e.DisplayName).HasMaxLength(120);
            entity.Property(e => e.District).HasMaxLength(100);
            entity.Property(e => e.Email).HasMaxLength(256);
            entity.Property(e => e.PasswordHash).HasMaxLength(300);
            entity.Property(e => e.PhoneNumber).HasMaxLength(30);
            entity.Property(e => e.UpdatedAt).HasPrecision(0);
            entity.Property(e => e.UserName).HasMaxLength(100);

            entity.HasOne(d => d.Role).WithMany(p => p.AppUsers)
                .HasForeignKey(d => d.RoleId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_AppUsers_Roles");
        });

        modelBuilder.Entity<ChatMessage>(entity =>
        {
            entity.HasKey(e => e.MessageId);

            entity.HasIndex(e => new { e.ThreadId, e.SentAt }, "IX_ChatMessages_Thread_Time");

            entity.Property(e => e.EditedAt).HasPrecision(0);
            entity.Property(e => e.SentAt)
                .HasPrecision(0)
                .HasDefaultValueSql("(sysutcdatetime())");

            entity.HasOne(d => d.SenderUser).WithMany(p => p.ChatMessages)
                .HasForeignKey(d => d.SenderUserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ChatMessages_Sender");

            entity.HasOne(d => d.Thread).WithMany(p => p.ChatMessages)
                .HasForeignKey(d => d.ThreadId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ChatMessages_Thread");
        });

        modelBuilder.Entity<ChatThread>(entity =>
        {
            entity.HasKey(e => e.ThreadId);

            entity.Property(e => e.CreatedAt)
                .HasPrecision(0)
                .HasDefaultValueSql("(sysutcdatetime())");
            entity.Property(e => e.ThreadType).HasDefaultValue((byte)1);

            entity.HasOne(d => d.Post).WithMany(p => p.ChatThreads)
                .HasForeignKey(d => d.PostId)
                .HasConstraintName("FK_ChatThreads_Post");
        });

        modelBuilder.Entity<ChatThreadMember>(entity =>
        {
            entity.HasKey(e => new { e.ThreadId, e.UserId });

            entity.Property(e => e.JoinedAt)
                .HasPrecision(0)
                .HasDefaultValueSql("(sysutcdatetime())");

            entity.HasOne(d => d.Thread).WithMany(p => p.ChatThreadMembers)
                .HasForeignKey(d => d.ThreadId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ChatMembers_Thread");

            entity.HasOne(d => d.User).WithMany(p => p.ChatThreadMembers)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ChatMembers_User");
        });

        modelBuilder.Entity<JoinRequest>(entity =>
        {
            entity.HasKey(e => e.RequestId);

            entity.HasIndex(e => new { e.PostId, e.Status, e.CreatedAt }, "IX_JoinRequests_Post_Status");

            entity.HasIndex(e => new { e.PostId, e.RequesterUserId }, "UX_JoinRequests_Unique").IsUnique();

            entity.Property(e => e.CreatedAt)
                .HasPrecision(0)
                .HasDefaultValueSql("(sysutcdatetime())");
            entity.Property(e => e.DecidedAt).HasPrecision(0);
            entity.Property(e => e.GuestNames).HasMaxLength(300);
            entity.Property(e => e.Message).HasMaxLength(500);
            entity.Property(e => e.PartySize).HasDefaultValue(1);
            entity.Property(e => e.Status).HasDefaultValue((byte)1);

            entity.HasOne(d => d.DecidedByUser).WithMany(p => p.JoinRequestDecidedByUsers)
                .HasForeignKey(d => d.DecidedByUserId)
                .HasConstraintName("FK_JoinRequests_Decider");

            entity.HasOne(d => d.Post).WithMany(p => p.JoinRequests)
                .HasForeignKey(d => d.PostId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_JoinRequests_Post");

            entity.HasOne(d => d.RequesterUser).WithMany(p => p.JoinRequestRequesterUsers)
                .HasForeignKey(d => d.RequesterUserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_JoinRequests_Requester");
        });

        modelBuilder.Entity<MatchPost>(entity =>
        {
            entity.HasKey(e => e.PostId);

            entity.HasIndex(e => e.CreatorUserId, "IX_Posts_Creator");

            entity.HasIndex(e => new { e.City, e.District }, "IX_Posts_Location");

            entity.HasIndex(e => new { e.SportId, e.Status, e.StartTime }, "IX_Posts_Search");

            entity.Property(e => e.City).HasMaxLength(100);
            entity.Property(e => e.CreatedAt)
                .HasPrecision(0)
                .HasDefaultValueSql("(sysutcdatetime())");
            entity.Property(e => e.District).HasMaxLength(100);
            entity.Property(e => e.EndTime).HasPrecision(0);
            entity.Property(e => e.ExpiresAt).HasPrecision(0);
            entity.Property(e => e.FeePerPerson).HasColumnType("decimal(12, 2)");
            entity.Property(e => e.GoogleMapsUrl).HasMaxLength(600);
            entity.Property(e => e.LocationText).HasMaxLength(300);
            entity.Property(e => e.MatchType).HasDefaultValue((byte)4);
            entity.Property(e => e.StartTime).HasPrecision(0);
            entity.Property(e => e.Status).HasDefaultValue((byte)1);
            entity.Property(e => e.Title).HasMaxLength(150);
            entity.Property(e => e.UpdatedAt).HasPrecision(0);

            entity.HasOne(d => d.CreatorUser).WithMany(p => p.MatchPosts)
                .HasForeignKey(d => d.CreatorUserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Posts_Users");

            entity.HasOne(d => d.Sport).WithMany(p => p.MatchPosts)
                .HasForeignKey(d => d.SportId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Posts_Sports");
        });

        modelBuilder.Entity<Notification>(entity =>
        {
            entity.HasIndex(e => new { e.UserId, e.IsRead, e.CreatedAt }, "IX_Notifications_User_Read");

            entity.Property(e => e.Body).HasMaxLength(400);
            entity.Property(e => e.CreatedAt)
                .HasPrecision(0)
                .HasDefaultValueSql("(sysutcdatetime())");
            entity.Property(e => e.ReadAt).HasPrecision(0);
            entity.Property(e => e.Title).HasMaxLength(120);
            entity.Property(e => e.Type).HasMaxLength(50);

            entity.HasOne(d => d.User).WithMany(p => p.Notifications)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Notifications_User");
        });

        modelBuilder.Entity<PostParticipant>(entity =>
        {
            entity.HasKey(e => new { e.PostId, e.UserId });

            entity.Property(e => e.JoinedAt)
                .HasPrecision(0)
                .HasDefaultValueSql("(sysutcdatetime())");
            entity.Property(e => e.LeftAt).HasPrecision(0);
            entity.Property(e => e.PartySize).HasDefaultValue(1);
            entity.Property(e => e.Role).HasDefaultValue((byte)2);
            entity.Property(e => e.Status).HasDefaultValue((byte)1);

            entity.HasOne(d => d.Post).WithMany(p => p.PostParticipants)
                .HasForeignKey(d => d.PostId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Part_Post");

            entity.HasOne(d => d.User).WithMany(p => p.PostParticipants)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Part_User");
        });

        modelBuilder.Entity<Report>(entity =>
        {
            entity.Property(e => e.CreatedAt)
                .HasPrecision(0)
                .HasDefaultValueSql("(sysutcdatetime())");
            entity.Property(e => e.Details).HasMaxLength(500);
            entity.Property(e => e.Resolution).HasMaxLength(500);
            entity.Property(e => e.ReviewedAt).HasPrecision(0);
            entity.Property(e => e.Status).HasDefaultValue((byte)1);

            entity.HasOne(d => d.ReporterUser).WithMany(p => p.ReportReporterUsers)
                .HasForeignKey(d => d.ReporterUserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Reports_Reporter");

            entity.HasOne(d => d.TargetPost).WithMany(p => p.Reports)
                .HasForeignKey(d => d.TargetPostId)
                .HasConstraintName("FK_Reports_TargetPost");

            entity.HasOne(d => d.TargetUser).WithMany(p => p.ReportTargetUsers)
                .HasForeignKey(d => d.TargetUserId)
                .HasConstraintName("FK_Reports_TargetUser");
        });

        modelBuilder.Entity<Role>(entity =>
        {
            entity.HasIndex(e => e.Name, "UQ_Roles_Name").IsUnique();

            entity.Property(e => e.Description).HasMaxLength(200);
            entity.Property(e => e.Name).HasMaxLength(50);
        });

        modelBuilder.Entity<Sport>(entity =>
        {
            entity.HasIndex(e => e.ImageId, "IX_Sports_ImageId");

            entity.HasIndex(e => e.Name, "UQ_Sports_Name").IsUnique();

            entity.Property(e => e.CreatedAt)
                .HasPrecision(0)
                .HasDefaultValueSql("(sysutcdatetime())");
            entity.Property(e => e.Name).HasMaxLength(100);

            entity.HasOne(d => d.Image).WithMany(p => p.Sports)
                .HasForeignKey(d => d.ImageId)
                .HasConstraintName("FK_Sports_SportImages");
        });

        modelBuilder.Entity<SportImage>(entity =>
        {
            entity.HasKey(e => e.ImageId);

            entity.Property(e => e.ImageUrl).HasMaxLength(500);
        });

        modelBuilder.Entity<EmailVerification>(entity =>
        {
            entity.HasKey(e => e.Id);

            entity.ToTable("EmailVerification");

            entity.Property(e => e.Email)
                .HasMaxLength(255);

            entity.Property(e => e.OTP)
                .HasMaxLength(6);

            entity.Property(e => e.IsUsed)
                .HasDefaultValue(false);

            entity.Property(e => e.CreatedAt)
                .HasColumnType("datetime");

            entity.Property(e => e.ExpireTime)
                .HasColumnType("datetime");
        });

        OnModelCreatingPartial(modelBuilder);
    }



    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
