using UnityEngine;

namespace Gameplay
{
    public class Ladder: MonoBehaviour
    {
        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Player") && other.TryGetComponent<Player.Player>(out var player))
            {
                Debug.Log("Ladder entered");
                // var player = other.GetComponent<Player.Player>();
                player.ForcedState = Player.Player.State.Climbing;
            }
        }
        
        private void OnTriggerExit(Collider other)
        {
            if (other.CompareTag("Player") && other.TryGetComponent<Player.Player>(out var player))
            {
                Debug.Log("Ladder exited");
                player.ForcedState = null;
            }
        }
    }
}