namespace Infrastructure.States
{
    public class EndGameState : BaseGameState
    {
        public override async void OnEnter()
        {
            Mediator.DisablePlayerInput();
            
            await Mediator.SetupEndGameMenu();
            Mediator.ShowEndGameMenu();
        }

        public override void OnExit()
        {
            Mediator.HideEndGameMenu();
        }
    }
}