using UnityEngine;
using Types;

namespace Core
{
    public class GameState: MonoBehaviour
    {
        public static GameState Current;

        public State prevState;
        public State state;
        public State nextState;

        private void Awake()
        {
            Current = this;
        }
    }
}