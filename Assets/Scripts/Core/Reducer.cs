using System;
using System.Collections.Generic;
using UnityEngine;
using Types;

namespace Core
{
    public class Reducer: MonoBehaviour
    {
        private Dictionary<State, Action> stateEnterActions = new Dictionary<State, Action>();
        private Dictionary<State, Action> stateExitActions = new Dictionary<State, Action>();

        private bool _nextStateSet;
        private State _nextState;

        private void Start()
        {
            EventEmitter.OnStateChange += OnStateChange;
            stateEnterActions[State.Play] = () =>
            {
                Time.timeScale = 1;
                UIController.Current.RenderHUD();
            };
            stateExitActions[State.Play] = () =>
            {
                Time.timeScale = 0;
            };
            stateEnterActions[State.Menu] = () =>
            {
                UIController.Current.RenderMenu();
            };
            stateEnterActions[State.Examine] = () =>
            {
                UIController.Current.RenderExamine();
            };
            stateEnterActions[State.GameOver] = () =>
            {
                UIController.Current.RenderGameOver();
            };
            
            UIController.Current.RenderHUD();
        }

        private void OnStateChange(State state)
        {
            var previousState = GameState.Current.state;

            if (state == previousState) return;

            if (stateExitActions.ContainsKey(previousState))
            {
                stateExitActions[previousState]();
            }

            if (stateEnterActions.ContainsKey(state))
            {
                stateEnterActions[state]();
            }
            GameState.Current.prevState = GameState.Current.state;
            GameState.Current.state = state;
            if (_nextStateSet)
            {
                var nextState = _nextState;
                _nextState = State.None;
                _nextStateSet = false;
                OnStateChange(nextState);
            }
        }
    }
}