namespace Turnos.Api.Entities;

public class AppointmentFile
{
    public Guid Id { get; set; }
    public Guid AppointmentId { get; set; }
    public string FilePathOrUrl { get; set; } = null!;
    public string FileName { get; set; } = null!;
    public string FileType { get; set; } = null!;
    public DateTime UploadedAt { get; set; } = DateTime.UtcNow;

    public Appointment Appointment { get; set; } = null!;
}