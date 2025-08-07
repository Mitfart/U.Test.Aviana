using StateMachine;

namespace Infrastructure.States
{
    public abstract class BaseGameState : BaseState<BaseGameState, GameStateMachine>
    {
        public GameMediator Mediator => StateMachine.Mediator;
    }
}