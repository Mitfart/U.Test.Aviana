using System;

namespace Infrastructure.Services.Path
{
    public class RoundService
    {
        public event Action<int> OnRoundChange;
        
        public int Round { get; private set; }

        
        public void NextRound()
        {
            Round++;
            OnRoundChange?.Invoke(Round);
        }
    }
}