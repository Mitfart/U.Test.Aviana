using UnityEngine;

namespace Infrastructure.Services.Tick
{
    public class UnityTickService : MonoBehaviour, ITickService
    {
        private readonly TickService _tickService = new();

        private void Update() => _tickService.Tick();

        public void Add(ITickable tickable) => _tickService.Add(tickable);
        public void Remove(ITickable tickable) => _tickService.Remove(tickable);
    }
}