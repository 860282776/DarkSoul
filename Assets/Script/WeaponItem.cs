using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace LM
{
    [CreateAssetMenu(menuName ="Items/Weapon Item")]
    public class WeaponItem : Item
    {
        public GameObject modelPrefab;
        public bool isUnarmed;

        [Header("Idle Animations")]
        public string right_hand_idle;
        public string left_hand_idle;

        [Header("Attack Animations")]
        public string LightAttack;
        public string LightAttack2;
        //public string LightAttack3;
        //public string LightAttack4;
        public string HeavyAttack;
    }
}

