using System.Collections.Generic;

namespace Infrastructure.Services.Tick
{
    public class TickService : ITickService
    {
        private HashSet<ITickable> _tickables = new();

        public void Tick()
        {
            foreach (var tickable in _tickables) 
                tickable.Tick();
        }

        public void Add(ITickable tickable) => _tickables.Add(tickable);
        public void Remove(ITickable tickable) => _tickables.Remove(tickable);
    }
}