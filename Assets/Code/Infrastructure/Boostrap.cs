using Infrastructure.States;
using UnityEngine;

namespace Infrastructure
{
    public class Boostrap : MonoBehaviour
    {
        public GameMediator mediator;
        
        private GameStateMachine _game;


        private void Awake()
        {
            _game = new GameStateMachine(mediator);
            
            mediator.Construct(_game);
        }

        private void Start() => _game.Enter<BootState>();
    }
}