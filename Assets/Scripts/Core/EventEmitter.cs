using UnityEngine;
using System;
using Types;

namespace Core
{
    public class EventEmitter: MonoBehaviour
    {
        public static event Action<State> OnStateChange;
        public static event Action<string> OnPageOpen;

        public static event Action OnMakeNoise;

        public static void EmitStateChange(State state)
        {
            OnStateChange?.Invoke(state);
        }

        public static void EmitPageOpen(string id)
        {
            OnPageOpen?.Invoke(id);
        }

        public static void EmitMakeNoise()
        {
            OnMakeNoise?.Invoke();
        }
    }
}