using System;
using System.Collections.Generic;

namespace StateMachine
{
    public abstract class StateMachine<TState, TStateMachine> : IDisposable
        where TState : IState<TState, TStateMachine>
        where TStateMachine : StateMachine<TState, TStateMachine>
    {
        private readonly Dictionary<Type, TState> _states = new();

        private TState _activeState;


        public void Dispose()
        {
            foreach (var state in _states.Values)
                state.Dispose();

            _states.Clear();
        }


        protected void Add<TConcrete>(TConcrete instance) where TConcrete : TState
        {
            _states.Add(typeof(TConcrete), instance);
            instance.Init((TStateMachine)this);

            _activeState ??= instance;
        }

        public void Enter<TConcrete>() where TConcrete : TState
        {
            _activeState.OnExit();

            _activeState = _states[typeof(TConcrete)];

            _activeState.OnEnter();
        }

        public void Process()
        {
            _activeState.Process();
        }
    }
}