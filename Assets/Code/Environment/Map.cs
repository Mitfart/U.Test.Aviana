using Infrastructure;
using UnityEngine;

namespace Environment
{
    public class Map : MonoBehaviour
    {
        public Transform spawnPoint;
        public FinishLine finishLine;

        
        public Map Construct(GameStateMachine game)
        {
            finishLine.Construct(game);
            return this;
        }
    }
}