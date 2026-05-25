using Microsoft.EntityFrameworkCore;
using Turnos.Api.Entities;

namespace Turnos.Api.Data;

public sealed class TurnosDbContext : DbContext
{
    public TurnosDbContext(DbContextOptions<TurnosDbContext> options) : base(options)
    {
        ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;
    }

    public DbSet<User> Users => Set<User>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
    public DbSet<Specialty> Specialties => Set<Specialty>();
    public DbSet<Doctor> Doctors => Set<Doctor>();
    public DbSet<Patient> Patients => Set<Patient>();
    public DbSet<Appointment> Appointments => Set<Appointment>();
    public DbSet<AppointmentFile> AppointmentFiles => Set<AppointmentFile>();
    public DbSet<Payment> Payments => Set<Payment>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        ConfigureUser(modelBuilder);
        ConfigureRefreshToken(modelBuilder);
        ConfigureSpecialty(modelBuilder);
        ConfigureDoctor(modelBuilder);
        ConfigurePatient(modelBuilder);
        ConfigureAppointment(modelBuilder);
        ConfigureAppointmentFile(modelBuilder);
        ConfigurePayment(modelBuilder);
    }

    private static void ConfigureUser(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id);

            entity.HasIndex(e => e.Dni).IsUnique();

            entity.Property(e => e.Dni)
                .IsRequired()
                .HasMaxLength(20);

            entity.HasIndex(e => e.Email).IsUnique();

            entity.Property(e => e.Email)
                .HasMaxLength(256);

            entity.Property(e => e.PasswordHash)
                .IsRequired();

            entity.Property(e => e.Phone)
                .HasMaxLength(30);

            entity.Property(e => e.FirstName)
                .IsRequired()
                .HasMaxLength(100);

            entity.Property(e => e.LastName)
                .IsRequired()
                .HasMaxLength(100);

            entity.Property(e => e.Role)
                .HasConversion<string>()
                .HasMaxLength(20)
                .IsRequired();

            entity.HasOne(e => e.RegisteredByUser)
                .WithMany()
                .HasForeignKey(e => e.RegisteredBy)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasMany(e => e.RefreshTokens)
                .WithOne(rt => rt.User)
                .HasForeignKey(rt => rt.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.Doctor)
                .WithOne(d => d.User)
                .HasForeignKey<Doctor>(d => d.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.Patient)
                .WithOne(p => p.User)
                .HasForeignKey<Patient>(p => p.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }

    private static void ConfigureRefreshToken(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<RefreshToken>(entity =>
        {
            entity.HasKey(e => e.Id);

            entity.Property(e => e.Token)
                .IsRequired()
                .HasMaxLength(512);

            entity.HasIndex(e => e.Token).IsUnique();
        });
    }

    private static void ConfigureSpecialty(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Specialty>(entity =>
        {
            entity.HasKey(e => e.Id);

            entity.HasIndex(e => e.Name).IsUnique();

            entity.Property(e => e.Name)
                .IsRequired()
                .HasMaxLength(150);

            entity.Property(e => e.Description)
                .HasMaxLength(500);

            entity.HasMany(e => e.Doctors)
                .WithOne(d => d.Specialty)
                .HasForeignKey(d => d.SpecialtyId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }

    private static void ConfigureDoctor(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Doctor>(entity =>
        {
            entity.HasKey(e => e.Id);

            entity.HasIndex(e => e.UserId).IsUnique();

            entity.HasIndex(e => e.LicenseNumber).IsUnique();

            entity.Property(e => e.LicenseNumber)
                .IsRequired()
                .HasMaxLength(50);

            entity.Property(e => e.ConsultationPrice)
                .HasPrecision(18, 2);

            entity.HasMany(e => e.Appointments)
                .WithOne(a => a.Doctor)
                .HasForeignKey(a => a.DoctorId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }

    private static void ConfigurePatient(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Patient>(entity =>
        {
            entity.HasKey(e => e.Id);

            entity.HasIndex(e => e.UserId).IsUnique();

            entity.HasMany(e => e.Appointments)
                .WithOne(a => a.Patient)
                .HasForeignKey(a => a.PatientId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }

    private static void ConfigureAppointment(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Appointment>(entity =>
        {
            entity.HasKey(e => e.Id);

            entity.Property(e => e.Status)
                .HasConversion<string>()
                .HasMaxLength(30)
                .IsRequired();

            entity.Property(e => e.Notes)
                .HasMaxLength(2000);

            entity.HasOne(e => e.Payment)
                .WithOne(p => p.Appointment)
                .HasForeignKey<Payment>(p => p.AppointmentId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasMany(e => e.Files)
                .WithOne(f => f.Appointment)
                .HasForeignKey(f => f.AppointmentId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }

    private static void ConfigureAppointmentFile(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<AppointmentFile>(entity =>
        {
            entity.HasKey(e => e.Id);

            entity.Property(e => e.FilePathOrUrl)
                .IsRequired()
                .HasMaxLength(500);

            entity.Property(e => e.FileName)
                .IsRequired()
                .HasMaxLength(255);

            entity.Property(e => e.FileType)
                .IsRequired()
                .HasMaxLength(50);
        });
    }

    private static void ConfigurePayment(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Payment>(entity =>
        {
            entity.HasKey(e => e.Id);

            entity.HasIndex(e => e.AppointmentId).IsUnique();

            entity.Property(e => e.Amount)
                .HasPrecision(18, 2);

            entity.Property(e => e.Method)
                .HasConversion<string>()
                .HasMaxLength(20)
                .IsRequired();

            entity.Property(e => e.Status)
                .HasConversion<string>()
                .HasMaxLength(20)
                .IsRequired();

            entity.Property(e => e.ReceiptUrl)
                .HasMaxLength(500);
        });
    }
}