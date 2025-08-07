using System;

namespace StateMachine
{
    public interface IState<TState, TStateMachine> : IDisposable
        where TState : IState<TState, TStateMachine>
        where TStateMachine : StateMachine<TState, TStateMachine>
    {
        public TStateMachine StateMachine { get; }

        public void Init(TStateMachine stateMachine);

        public void OnEnter();
        public void Process();
        public void OnExit();
    }
}