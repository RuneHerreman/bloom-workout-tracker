namespace Bloom.Domain.Entity.Logs;

public class LoggedWorkout
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public DateTime Date { get; set; }
    public decimal Volume { get; set; } = 0;
}