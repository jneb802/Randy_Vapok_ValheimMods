using System.Collections.Generic;
using HarmonyLib;
using UnityEngine;

namespace EpicLoot.MagicItemEffects
{
    public static class Bloodlust_Attack_StaminaCache
    {
        // Dictionary to store the attack stamina for each attack instance
        public static Dictionary<Attack, float> StaminaCache = new Dictionary<Attack, float>();
        public static bool staminaCacheSet;
    }
    
    [HarmonyPatch(typeof(Attack), nameof(Attack.GetAttackStamina))]
    public class Bloodlust_Attack_GetAttackStamina_Patch
    {
        public static void Prefix(Attack __instance)
        {
            if (__instance.m_character is Player player &&
                MagicEffectsHelper.HasActiveMagicEffect(player, __instance.m_weapon, MagicEffectType.Bloodlust))
            {
                if (!Bloodlust_Attack_StaminaCache.staminaCacheSet)
                {
                    // Cache the stamina cost for later use
                    Bloodlust_Attack_StaminaCache.StaminaCache[__instance] = __instance.m_attackStamina;
                    Bloodlust_Attack_StaminaCache.staminaCacheSet = true;
                    Debug.Log("Stamina cache set to" + __instance.m_attackStamina);
                }
                
                // Set stamina cost to 0 when Blood Lust is active
                __instance.m_attackStamina = 0f;
                __instance.m_weapon.m_shared.m_attack.m_attackStamina = 0f;
                Debug.Log("Item with name " + __instance.m_weapon.m_shared.m_name + " has Blood Lust. Set Stamina to 0.");
            }
        }
        
    [HarmonyPatch(typeof(Attack), nameof(Attack.GetAttackHealth))]
    public class Bloodlust_Attack_GetAttackHealth_Patch
    {
        public static void Postfix(Attack __instance, ref float __result)
        {
            if (__instance.m_character is Player player && MagicEffectsHelper.HasActiveMagicEffectOnWeapon(player, __instance.m_weapon, MagicEffectType.Bloodlust))
            {
                if (Bloodlust_Attack_StaminaCache.StaminaCache.TryGetValue(__instance, out float cachedStamina))
                {
                    float newAttackHealth = cachedStamina;
                    __instance.m_weapon.m_shared.m_attack.m_attackHealth = cachedStamina;
                    float skillFactor = __instance.m_character.GetSkillFactor(__instance.m_weapon.m_shared.m_skillType);
                    __result = newAttackHealth - newAttackHealth * 0.33f * skillFactor;

                    Debug.Log("Item with name " + __instance.m_weapon.m_shared.m_name +
                              " has Blood Lust. Set new attack health.");
                }
            }
        }
    }

    
    }



}
