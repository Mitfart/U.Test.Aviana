namespace StateMachine
{
    public abstract class BaseState<TState, TStateMachine> : IState<TState, TStateMachine>
        where TState : IState<TState, TStateMachine>
        where TStateMachine : StateMachine<TState, TStateMachine>
    {
        public TStateMachine StateMachine { get; protected set; }

        public virtual void Init(TStateMachine stateMachine)
        {
            StateMachine = stateMachine;
        }

        public virtual void Dispose()
        {
        }

        public virtual void OnEnter()
        {
        }

        public virtual void Process()
        {
        }

        public virtual void OnExit()
        {
        }
    }
}