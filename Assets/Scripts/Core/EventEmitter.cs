using UnityEngine;
using System;
using Types;

namespace Core
{
    public class EventEmitter: MonoBehaviour
    {
        public static event Action<State> OnStateChange;
        public static event Action<string> OnPageOpen;

        public static void EmitStateChange(State state)
        {
            OnStateChange?.Invoke(state);
        }

        public static void EmitPageOpen(string id)
        {
            OnPageOpen?.Invoke(id);
        }
    }
}