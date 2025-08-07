namespace Infrastructure.Services.Time
{
    public class UnityTimeService : ITimeService
    {
        public float Current => UnityEngine.Time.time;
        public float DeltaTime => UnityEngine.Time.deltaTime;
    }
}