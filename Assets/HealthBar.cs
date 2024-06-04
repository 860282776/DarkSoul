using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace LM
{
    public class HealthBar : MonoBehaviour
    {
        public Slider slider;
        private void Start()
        {
            slider = GetComponent<Slider>();
        }
        public void SetMaxHealth(int maxHealth)
        {
            slider.maxValue = maxHealth;
            slider.value = maxHealth;
        }
        public void SetCurrentHelath(int currentHelath)
        {
            slider.value = currentHelath;
        }
    }

}
