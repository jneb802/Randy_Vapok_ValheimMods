using System;
using EpicLoot.Healing;
using HarmonyLib;
using UnityEngine;

namespace EpicLoot.MagicItemEffects;

public static class FireAbsorption
{
    [HarmonyPatch(typeof(Humanoid), nameof(Humanoid.BlockAttack))]
    public static class FireAbsorption_Character_Damage_Patch
    {
        public static void Postfix(HitData hit, ref bool __result)
        {
            if (!__result)
            {
                return;
            }
            
            CheckAndDoFireHeal(hit);
        }
    }
        
    public static void CheckAndDoFireHeal(HitData hit)
    {
        var player = Player.m_localPlayer;
        
        if (!player.HasActiveMagicEffect(MagicEffectType.FireAbsorption) || hit.m_damage.m_fire == 0)
        {
            Debug.Log("No fire absorb effect or fire damage is 0");
            return;
        }

        var fireHealMultiplier = 0f;
        fireHealMultiplier += player.GetTotalActiveMagicEffectValue(MagicEffectType.FireAbsorption, 0.01f);

        if (fireHealMultiplier == 0)
        {
            Debug.Log("No fire multuplier");
            return;  
        }
            
        
        var healOn = hit.m_damage.GetTotalElementalDamage() * fireHealMultiplier;
        
        Debug.Log("Total elemental Damage " + hit.m_damage.GetTotalElementalDamage());
        Debug.Log("Total blockable Damage " + hit.m_damage.GetTotalBlockableDamage());
        Debug.Log("Total fire Damage " + hit.m_damage.m_fire);
        Debug.Log("Fire heal multiplier " + fireHealMultiplier);
        Debug.Log("FireHeal " + healOn);
        var healFromQueue = false;
        if (player.IsPlayer())
        {
            var healingQueue = player.GetComponent<HealingQueueMono>();
            if (healingQueue)
            {
                healFromQueue = true;
                healingQueue.HealRequests.Add(healOn);
            }
        } 
        
        if (!healFromQueue)
        { 
            player.Heal(healOn);
        }
    }
}