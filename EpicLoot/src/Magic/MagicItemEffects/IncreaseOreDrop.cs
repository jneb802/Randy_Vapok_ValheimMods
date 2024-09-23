using System.Collections.Generic;
using System.Linq.Expressions;
using HarmonyLib;
using UnityEngine;

namespace EpicLoot.MagicItemEffects;

public class IncreaseOreDrop
{
    [HarmonyPatch(typeof(DropTable), nameof(DropTable.GetDropList), typeof(int))]
    public static class IncreaseOreDrop_DropTable_GetDropList_Patch
    {
        private static void Prefix(ref int amount)
        {
            if (!IncreaseOreDrop_Attack_OnAttackTrigger_Patch.playerHasIncreaseOreDropEffect)
            {
                if (!IncreaseOreDrop_MineRock5_RPC_Damage_Patch.IsMining)
                {
                    return; 
                }
            }
            
            int magicEffectValue = Mathf.FloorToInt(IncreaseOreDrop_Attack_OnAttackTrigger_Patch.increaseOreDropEffectValue);
            amount += magicEffectValue;
        }
    }

    [HarmonyPatch(typeof(Attack), nameof(Attack.GetAttackStamina))]
    public static class IncreaseOreDrop_Attack_OnAttackTrigger_Patch
    {
        public static bool playerHasIncreaseOreDropEffect = false;
        public static float increaseOreDropEffectValue = 0;

        private static void Prefix(Attack __instance)
        {
            if (__instance.m_character is Player player)
            {
                if (player.HasActiveMagicEffect(MagicEffectType.IncreaseOreDrop))
                {
                    playerHasIncreaseOreDropEffect = true;
                    increaseOreDropEffectValue = MagicEffectsHelper.GetTotalActiveMagicEffectValueForWeapon(player,
                        __instance.m_weapon, MagicEffectType.IncreaseOreDrop, 1f);
                }
                
                playerHasIncreaseOreDropEffect = false;
            }
        }
    }

    [HarmonyPatch(typeof(MineRock5), nameof(MineRock5.RPC_Damage))]
    public static class IncreaseOreDrop_MineRock5_RPC_Damage_Patch
    {
        public static bool IsMining = false;

        private static bool Prefix(HitData hit)
        {
            return HandleMining(hit);
        }

        public static bool HandleMining(HitData hit)
        {
            if (hit.GetAttacker() is not Player player)
            {
                return true;
            }

            IsMining = true;
            return true;
        }

        private static void Finalizer()
        {
            IsMining = false;
        }
    }
    
    [HarmonyPatch(typeof(Destructible), nameof(Destructible.RPC_Damage))]
    public static class IncreaseOreDrop_Destructible_RPC_Damage_Patch
    {
        private static bool Prefix(Destructible __instance, HitData hit)
        {
            if (!IncreaseOreDrop_MineRock5_RPC_Damage_Patch.IsMining && __instance.m_damages.m_pickaxe != HitData.DamageModifier.Immune && __instance.m_damages.m_chop == HitData.DamageModifier.Immune && __instance.m_destructibleType != DestructibleType.Tree)
            {            
                return IncreaseOreDrop_MineRock5_RPC_Damage_Patch.HandleMining(hit);
            }
            return true;
        }

        private static void Finalizer()
        {
            IncreaseOreDrop_MineRock5_RPC_Damage_Patch.IsMining = false;
        }
    }
}