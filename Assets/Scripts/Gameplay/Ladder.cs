using UnityEngine;

namespace Gameplay
{
    public class Ladder: MonoBehaviour
    {
        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Player") && other.TryGetComponent<Player.Player>(out var player))
            {
                player.ForcedState = Player.Player.State.Climbing;
            }
        }
        
        private void OnTriggerExit(Collider other)
        {
            if (other.CompareTag("Player") && other.TryGetComponent<Player.Player>(out var player))
            {
                player.ForcedState = null;
            }
        }
    }
}