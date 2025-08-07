namespace Infrastructure.States
{
    public class MainMenuState : BaseGameState
    {
        public override async void OnEnter()
        {
            if (Mediator.PlayerExist()) Mediator.DestroyPlayerCar();
            if (Mediator.GhostExist()) Mediator.DestroyGhost();
            
            Mediator.ShowLoadingCurtain();

            await Mediator.SetupMainMenu();
            Mediator.ShowMainMenu();
            
            Mediator.HideLoadingCurtain();
            
            if (Mediator.autoPlay)
                StateMachine.Enter<GameplayState>();
        }

        public override void OnExit()
        {
            Mediator.HideMainMenu();
            
            Mediator.ShowLoadingCurtain();
        }
    }
}