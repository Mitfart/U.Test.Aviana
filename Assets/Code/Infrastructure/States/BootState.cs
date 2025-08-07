namespace Infrastructure.States
{
    public class BootState : BaseGameState
    {
        public override void OnEnter()
        {
            StateMachine.Enter<MainMenuState>();
        }
    }
}