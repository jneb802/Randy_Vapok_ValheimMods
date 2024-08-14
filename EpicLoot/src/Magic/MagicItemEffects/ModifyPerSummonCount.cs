using System;
using HarmonyLib;
using UnityEngine;

namespace EpicLoot.MagicItemEffects
{
    public class ModifyPerSummonCount
    {
        public static int summonCount = 0;
        public static string playerName;
        
        // Patch is used to acquire the players name
        [HarmonyPatch(typeof(Player), nameof(Player.Update))]
        public static class ModifyPerSummonCount_Player_Awake_Patch
        {
            public static void Postfix(Player __instance)
            {
                if (__instance.IsPlayer())
                {
                    playerName = __instance.GetPlayerName();
                    // Debug.Log("Player name is set");
                }
            }
        }

        // Patch is used to increment summonCount when player uses a summoning staff
        [HarmonyPatch(typeof(Attack), nameof(Attack.FireProjectileBurst))]
        public static class ModifyPerSummonCount_Attack_FireProjectileBurst_Patch
        {
            public static void Postfix(Attack __instance)
            {
                if (__instance.m_character is Player player && __instance.m_attackProjectile != null)
                {
                    var spawnProjectile = __instance.m_attackProjectile;
                    if (spawnProjectile.TryGetComponent<SpawnAbility>(out var spawnAbility))
                    {
                        if (spawnAbility != null)
                        {
                            summonCount++;
                            // Debug.Log("Summon added to count. Current summons is: " + summonCount);

                            var zdo = __instance.m_character.m_nview.GetZDO();
                            if (zdo != null)
                            {
                                zdo.Set("m_summonCount", summonCount);
                                // Debug.Log("Summon count set in the players ZDO. Current summons is: " + summonCount);
                            }
                            
                        }
                    }
                    
                }
            }
        }
        
        [HarmonyPatch(typeof(Character), nameof(Character.OnDeath))]
        public static class ModifyPerSummonCount_Character_OnDeath_Patch
        {
            public static void Prefix(Character __instance)
            {
                if (__instance == null)
                {
                    // Debug.LogError("Character instance is null");
                    return;
                }

                if (!__instance.IsPlayer())
                {
                    
                    // Debug.Log("Name of character being destroyed is: " + __instance.m_name);

                    var zdo = __instance.m_nview?.m_zdo;
                    if (zdo == null)
                    {
                        // Debug.LogError("ZDO is null");
                        return;
                    }

                    string followTargetName = zdo.GetString(ZDOVars.s_follow);
                    // Debug.Log("Follow target has name: " + followTargetName);
                    if (followTargetName != null && followTargetName == playerName)
                    {
                        summonCount--;
                        // Debug.Log("Summon removed from count. Current summons is: " + summonCount);

                        // this line is null because there is no tameableAI component
                        MonsterAI monsterAI = __instance.GetComponentInParent<MonsterAI>();
                        if (monsterAI == null)
                        {
                            // Debug.LogError("monsterAI is null");
                            return;
                        }

                        GameObject followTarget = monsterAI.GetFollowTarget();
                        if (followTarget == null)
                        {
                            // Debug.LogError("followTarget is null");
                            return;
                        }
                        
                        // Debug.Log("Follow name is: " + monsterAI.GetFollowTarget());
                        if (followTarget != null && followTarget.TryGetComponent<ZNetView>(out var zNetView))
                        {
                            var followZDO = zNetView.GetZDO();
                            if (followZDO != null)
                            {
                                followZDO.Set("m_summonCount", summonCount);
                                // Debug.Log("Summon count set in the player's ZDO. Current summons is: " + summonCount);
                            }
                            else
                            {
                                // Debug.LogError("Follow ZDO is null");
                            }
                        }
                        else
                        {
                            // Debug.LogError("Follow or ZNetView is null");
                        }
                    }
                }
            }
        }

        public static void Apply(Player player, string name, Action<string> action)
        {
            action(name);
            if (PlayerHasActiveSummon(player))
            {
                action(name + "PerSummon");
            }
        }
        
        public static void ApplyOnlyForPerSummon(Player player, string name, Action<string> action)
        {
            if (PlayerHasActiveSummon(player))
            {
                action(name + "PerSummon");
            }
        }
        
        public static int GetActiveSummons(Player player)
        {
            if (PlayerHasActiveSummon(player))
            {
                var zdo = player.m_nview.GetZDO();
                if (zdo != null)
                {
                    return zdo.GetInt("m_summonCount");
                }
            }
            return 0;
        }
        
        public static bool PlayerHasActiveSummon(Player player)
        {
            var zdo = player.m_nview.GetZDO();
            if (zdo != null)
            {
                var zdoSummonCount = zdo.GetInt("m_summonCount");
                if (zdoSummonCount > 0)
                {
                    // Debug.Log("Checking for zdo summon count. Count is currently: " + zdoSummonCount);
                    return true;
                }
                return false;
            }
            return false;
        }
    }
}