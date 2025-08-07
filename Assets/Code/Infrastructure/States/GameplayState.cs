namespace Infrastructure.States
{
    public class GameplayState : BaseGameState
    {
        private bool _raceStarted;
        
        public override async void OnEnter()
        {
            await Mediator.SetupPathRecorder();
            await Mediator.SetupMap();
            await Mediator.SetupRoundCounterUI();
            await Mediator.SetupRaceCountdownUI();

            if (Mediator.PlayerExist()) 
                Mediator.DestroyPlayerCar();
            await Mediator.SetupPlayerCar();
            Mediator.DisablePlayerInput();

            if (Mediator.HasRecordedPath())
            {
                await Mediator.SetupGhost();
                Mediator.DisableGhost();
            }

            Mediator.NextRound();
            
            Mediator.HideLoadingCurtain();

            Mediator.ShowRoundCounterUI();
            Mediator.ShowRaceCountdownUI();

            Mediator.StartRaceCountdown();
            Mediator.SubscribeOnRaceStart(StartRace);
        }
        
        private void StartRace()
        {
            _raceStarted = true;
            
            Mediator.EnablePlayerInput();
            
            if (Mediator.GhostExist()) Mediator.EnableGhost();
        }

        public override void Process()
        {
            if (_raceStarted)
                Mediator.RecordPlayer();
        }

        public override void OnExit()
        {
            _raceStarted = false;
            
            Mediator.UnsubscribeOnRaceStart(StartRace);
            
            Mediator.DisablePlayerInput();

            if (Mediator.GhostExist()) Mediator.DisableGhost();

            Mediator.SaveRecord();
            
            Mediator.HideRoundCounterUI();
            Mediator.HideRaceCountdownUI();
        }
    }
}