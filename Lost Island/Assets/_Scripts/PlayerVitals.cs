using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace StarterAssets {
  

    public class PlayerVitals : MonoBehaviour
    {
        [Header("Stamina Controller")]
        public Slider staminaSlider;
        public float normalMaxStamina;
        public float fatigueMaxStamina;
        public  float staminaFallRate;
        public float staminaFallMultiplier;
        private float staminaRegainRate;
        public float staminaRegainMultiplier;

        [Header("Character Controller")]
        public PlayerController playerController;

        void Start()
        {
            staminaSlider.maxValue = normalMaxStamina;
            staminaSlider.value = normalMaxStamina;

            staminaFallRate = 1;
            staminaRegainRate = 1;
        }

        void Update()
        { 

            #region Stamina Controller

            if (Input.GetKey(KeyCode.LeftShift)) 
            {
                staminaSlider.value -= Time.deltaTime / staminaFallRate * staminaFallMultiplier;
            }

            else 
            {
                staminaSlider.value += Time.deltaTime / staminaRegainRate * staminaRegainMultiplier;
            }

            if (staminaSlider.value >= fatigueMaxStamina) 
            {
                staminaSlider.value = fatigueMaxStamina;
            }

            else if (staminaSlider.value <= 0) 
            {
                staminaSlider.value = 0;
                playerController.runSpeed = playerController.walkSpeed;
            }

            else if (staminaSlider.value >= 0) 
            {
                playerController.runSpeed = playerController.initialRunSpeed;    
            }
            #endregion
    
        }
    }
}
