using System.Collections.Generic;
using HarmonyLib;
using UnityEngine;

namespace EpicLoot.MagicItemEffects;

public class ModifySummonHealth
{
    
    private static readonly Dictionary<Humanoid, float> originalHealth = new Dictionary<Humanoid, float>();
    
    [HarmonyPatch(typeof(Attack), nameof(Attack.FireProjectileBurst))]
    public class ModifySummonHealth_Attack_GetAttackHealth_Patch
    {
        public static void Prefix(Attack __instance)
        {
            if (__instance.m_character is Player player 
                    && MagicEffectsHelper.HasActiveMagicEffect(player, __instance.m_weapon, MagicEffectType.ModifySummonHealth)
                    && __instance.m_attackProjectile != null)
            {
                // Debug.Log("Item with name " + __instance.m_weapon.m_shared.m_name + " has ModifySummonHealth and AttackProjectile is not null");
                
                var spawnProjectile = __instance.m_attackProjectile;
                if (spawnProjectile.TryGetComponent<SpawnAbility>(out var spawnAbility))
                {
                    // Debug.Log("Got spawnAbility with name: " + spawnAbility);
                    var spawnPrefab = spawnAbility.m_spawnPrefab[0];
                    if (spawnPrefab != null)
                    {
                        // Debug.Log("Got spawnPrefab with name: " + spawnPrefab);
                        if (spawnPrefab.TryGetComponent<Humanoid>(out var humanoid))
                        {
                            if (!originalHealth.ContainsKey(humanoid))
                            {
                                originalHealth[humanoid] = humanoid.m_health;
                            }
                            
                            // Debug.Log("Got humanoid with name:");
                            // Debug.Log("Original summon health is: " + humanoid.m_health);
                            float modifier = player.GetTotalActiveMagicEffectValue(MagicEffectType.ModifySummonHealth, 0.01f);
                            humanoid.m_health *= 1 + modifier;
                            // Debug.Log("Updated summon health is: " + humanoid.m_health);
                        }
                    }
                }
            }
        }

        public static void Postfix(Attack __instance)
        {
            if (__instance.m_attackProjectile != null)
            {
                var spawnProjectile = __instance.m_attackProjectile;
                if (spawnProjectile.TryGetComponent<SpawnAbility>(out var spawnAbility))
                {
                    var spawnPrefab = spawnAbility.m_spawnPrefab[0];
                    if (spawnPrefab != null)
                    {
                        if (spawnPrefab.TryGetComponent<Humanoid>(out var humanoid))
                        {
                            if (originalHealth.TryGetValue(humanoid, out var origHealth))
                            {
                                // Revert health back to original value
                                humanoid.m_health = origHealth;
                                originalHealth.Remove(humanoid); // Clean up the dictionary
                            }
                        }
                    }
                }
            }
        }
    }
}