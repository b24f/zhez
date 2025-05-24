using UnityEngine;

namespace Core
{
    public class InputController: MonoBehaviour
    { 
        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                EventEmitter.EmitStateChange(GameState.Current.state == Types.State.Menu ? Types.State.Play : Types.State.Menu);
            }
        }
    }
}