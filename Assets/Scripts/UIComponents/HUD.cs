using UnityEngine;
using UnityEngine.UIElements;

namespace UIComponents
{
    public class HUD : MonoBehaviour
    {
        [SerializeField] protected UIDocument document;
        
        private ProgressBar staminaBar;
        private ProgressBar batteryBar;
        private void OnEnable()
        {
            staminaBar = document.rootVisualElement.Q<ProgressBar>("stamina-bar");
            batteryBar = document.rootVisualElement.Q<ProgressBar>("battery-bar");
        }
        
        private void Update()
        {
            var batteryPercentage = Gameplay.Flashlight.Current.BatteryPercentage;
            var staminaPercentage = Player.Player.Current.StaminaPercentage;
            
            batteryBar.value = batteryPercentage;
            staminaBar.value = staminaPercentage;
        }
    }
}
