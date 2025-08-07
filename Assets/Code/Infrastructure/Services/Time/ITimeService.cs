namespace Infrastructure.Services.Time
{
    public interface ITimeService
    {
        float Current { get; }
        float DeltaTime { get; }
    }
}