using Ashsvp;
using UnityEngine;

namespace Player
{
    public class PlayerCarController : MonoBehaviour, IVehicleController
    {
        public SimcadeVehicleController car;
        
        private Controls _input;

        
        private void Awake() => car.SetController(this);

        public PlayerCarController Construct(Controls input)
        {
            _input = input;
            return this;
        }

        public float AccelerationInput => _input.Player.Move.ReadValue<Vector2>().y;
        public float SteerInput => _input.Player.Move.ReadValue<Vector2>().x;
        public bool BrakeInput => _input.Player.Jump.IsPressed();
    }
}