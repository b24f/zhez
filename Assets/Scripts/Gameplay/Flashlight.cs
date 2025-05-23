using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Gameplay
{
    public class Flashlight : MonoBehaviour
    {
        [SerializeField] private Light flashlight;
        
        [SerializeField] private float maxBattery = 100f;
        [SerializeField] private float batteryDrainRate = 5f;
        [SerializeField] private float battery = 100f;
        [SerializeField] private float rechargeAmount = 2f;
        
        private bool isOn;
        private const float Intensity = 10f;
        
        public float BatteryPercentage => Mathf.RoundToInt((battery / maxBattery) * 100f);

        public static Flashlight Current;
        
        private void Awake()
        {
            Current = this;
        }

        private void OnEnable()
        {
            isOn = flashlight.enabled;
        }
        
        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.F))
            {
                ToggleFlashlight();   
            }

            if (Input.GetKeyUp(KeyCode.R))
            {
                RechargeBattery();
            }

            if (isOn)
            {
                DrainBattery();
                FlickerBattery();
            }
        }

        private void ToggleFlashlight()
        {
            isOn = !isOn;
            flashlight.enabled = isOn;
        }

        private void DrainBattery()
        {
            battery -= batteryDrainRate * Time.deltaTime;
            battery = Mathf.Max(battery, 0);
            
            flashlight.intensity = Mathf.Lerp(0, Intensity, battery / maxBattery);
        }

        private void RechargeBattery()
        {
            battery += rechargeAmount;
            battery = Mathf.Clamp(battery, 0f, maxBattery);
            
            flashlight.intensity = Mathf.Lerp(0f, Intensity, battery / maxBattery);
        }

        private void FlickerBattery()
        {
            if (battery < 10f && Random.value < 0.1f)
            {
                flashlight.enabled = !flashlight.enabled;
            }
        }
    }
}
