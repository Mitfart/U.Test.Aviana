namespace Infrastructure.Services.Tick
{
    internal interface ITickService
    {
        void Add(ITickable tickable);
        void Remove(ITickable tickable);
    }
}