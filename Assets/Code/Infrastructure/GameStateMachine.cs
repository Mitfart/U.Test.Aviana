using Infrastructure.Services.Tick;
using Infrastructure.States;
using StateMachine;

namespace Infrastructure
{
    public class GameStateMachine : StateMachine<BaseGameState, GameStateMachine>, ITickable
    {
        public GameMediator Mediator { get; }


        public GameStateMachine(GameMediator mediator)
        {
            Mediator = mediator;
            
            Add(new BootState());
            Add(new MainMenuState());
            Add(new GameplayState());
            Add(new EndGameState());
        }
        
        public void Tick() => Process();
    }
}