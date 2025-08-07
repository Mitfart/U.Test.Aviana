using Infrastructure;
using Infrastructure.States;
using Player;
using UnityEngine;

namespace Environment
{
    [RequireComponent(typeof(Collider))]
    public class FinishLine : MonoBehaviour
    {
        private GameStateMachine _game;

        
        private void Awake() => GetComponent<Collider>().isTrigger = true;


        public FinishLine Construct(GameStateMachine game)
        {
            _game = game;
            return this;
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.TryGetComponent(out PlayerTag _)) 
                _game.Enter<EndGameState>();
        }
    }
}