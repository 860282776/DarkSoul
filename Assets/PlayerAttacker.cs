using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace LM
{
    public class PlayerAttacker : MonoBehaviour
    {
        AnimatorHandler animatorHandler;
        InputHandler inputHandler;
        public string lastAttack;
        private void Awake()
        {
            animatorHandler = GetComponentInChildren<AnimatorHandler>();
            inputHandler = GetComponent<InputHandler>();
        }
        public void HandleWeaponCombo(WeaponItem weapon)
        {
            if (inputHandler.comboFlag)
            {
                animatorHandler.anim.SetBool("canDoCombo", false);
                if (lastAttack == weapon.LightAttack)
                {
                    animatorHandler.PlayTargetAnimation(weapon.LightAttack2, true);
                }
            }
        }
        public void HandleLightAttack(WeaponItem weapon)
        {
            animatorHandler.PlayTargetAnimation(weapon.LightAttack, true);
            lastAttack = weapon.LightAttack;
        }
        public void HandleHeavyAttack(WeaponItem weapon)
        {
            animatorHandler.PlayTargetAnimation(weapon.HeavyAttack, true);
            lastAttack = weapon.LightAttack;
        }
    }

}
