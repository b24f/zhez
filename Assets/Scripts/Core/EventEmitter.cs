using UnityEngine;
using System;
using Types;

namespace Core
{
    public class EventEmitter: MonoBehaviour
    {
        public static event Action<State> OnStateChange;

        public static void EmitStateChange(State state)
        {
            OnStateChange?.Invoke(state);
        }
    }
}