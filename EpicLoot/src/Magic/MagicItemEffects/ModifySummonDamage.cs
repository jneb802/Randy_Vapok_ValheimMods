using System.Collections.Generic;
using HarmonyLib;
using UnityEngine;

namespace EpicLoot.MagicItemEffects;

public class ModifySummonDamage
{
    private static readonly Dictionary<Humanoid, Dictionary<ItemDrop, HitData.DamageTypes>> originalDamages = new Dictionary<Humanoid, Dictionary<ItemDrop, HitData.DamageTypes>>();
    
    [HarmonyPatch(typeof(Attack), nameof(Attack.FireProjectileBurst))]
    public class ModifySummonDamage_Attack_FireProjectileBurst_Patch
    {
        public static void Prefix(Attack __instance)
        {
            if (__instance.m_character is Player player 
                    && MagicEffectsHelper.HasActiveMagicEffect(player, __instance.m_weapon, MagicEffectType.ModifySummonDamage)
                    && __instance.m_attackProjectile != null)
            {
                // Debug.Log("Item with name " + __instance.m_weapon.m_shared.m_name + " has ModifySummonDamage and AttackProjectile is not null");
                float modifier = 1 + player.GetTotalActiveMagicEffectValue(MagicEffectType.ModifySummonDamage, 0.01f);
                
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
                            GameObject[] randomWeapons = humanoid.m_randomWeapon;
                            if (randomWeapons != null)
                            {
                                if (!originalDamages.ContainsKey(humanoid))
                                {
                                    originalDamages[humanoid] = new Dictionary<ItemDrop, HitData.DamageTypes>();
                                }
                                
                                foreach (var weapon in randomWeapons)
                                {
                                    if (weapon.TryGetComponent(out ItemDrop itemDrop))
                                    {
                                        // Debug.Log("Found ItemDrop component on: " + weapon.name);
                                        var itemDropDamages = itemDrop.m_itemData.m_shared.m_damages;
                                        
                                        if (!originalDamages[humanoid].ContainsKey(itemDrop))
                                        {
                                            originalDamages[humanoid][itemDrop] = itemDropDamages;
                                        }
                                        
                                        // Debug.Log("Item with name: " + weapon.name + "has total damage with value:" + itemDropDamages.GetTotalDamage());
                                        itemDropDamages.Modify(modifier);
                                        // Debug.Log("Item with name: " + weapon.name + "has as modified total damage with value:" + itemDropDamages.GetTotalDamage());
                                    }
                                }
                            }
                            
                            GameObject[] defaultItems = humanoid.m_defaultItems;
                            if (defaultItems != null)
                            {
                                if (!originalDamages.ContainsKey(humanoid))
                                {
                                    originalDamages[humanoid] = new Dictionary<ItemDrop, HitData.DamageTypes>();
                                }
                                
                                foreach (var weapon in defaultItems)
                                {
                                    if (weapon.TryGetComponent(out ItemDrop itemDrop))
                                    {
                                        // Debug.Log("Found ItemDrop component on: " + weapon.name);
                                        var itemDropDamages = itemDrop.m_itemData.m_shared.m_damages;
                                        
                                        if (!originalDamages[humanoid].ContainsKey(itemDrop))
                                        {
                                            originalDamages[humanoid][itemDrop] = itemDropDamages;
                                        }
                                        
                                        // Debug.Log("Item with name: " + weapon.name + "has total damage with value:" + itemDropDamages.GetTotalDamage());
                                        itemDropDamages.Modify(modifier);
                                        // Debug.Log("Item with name: " + weapon.name + "has as modified total damage with value:" + itemDropDamages.GetTotalDamage());
                                    }
                                }
                            }
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
                            if (originalDamages.TryGetValue(humanoid, out var itemDropDamages))
                            {
                                foreach (var kvp in itemDropDamages)
                                {
                                    if (kvp.Key.TryGetComponent(out ItemDrop itemDrop))
                                    {
                                        var originalDamage = kvp.Value;
                                        itemDrop.m_itemData.m_shared.m_damages = originalDamage;
                                    }
                                }

                                originalDamages.Remove(humanoid); // Clean up the dictionary
                            }
                        }
                    }
                }
            }
        }
    }
}