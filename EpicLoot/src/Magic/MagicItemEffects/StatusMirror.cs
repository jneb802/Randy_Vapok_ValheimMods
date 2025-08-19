using System.Collections.Generic;
using HarmonyLib;
using JetBrains.Annotations;

namespace EpicLoot.Magic.MagicItemEffects;

public class StatusMirror
{
    [HarmonyPatch(typeof(Character), nameof(Character.Damage))]
    public class StatusMirror_Character_Damage_Patch
    {
        public static void Prefix(Character __instance, HitData hit)
        {
            Player player = __instance as Player;
            if (__instance is not Player) return;
            if (player == null || player != Player.m_localPlayer) return;
            if (!player.HasActiveMagicEffect(MagicEffectType.StatusMirror)) return;
            if (hit.m_ranged == true) return;
           
            List<StatusEffect> statusEffects = player.GetSEMan().GetStatusEffects();
            if (statusEffects.Count == 0) return;

            Character character = hit.GetAttacker();
            if (character == null) return;
            
            foreach (StatusEffect statusEffect in statusEffects)
            {
                character.GetSEMan().AddStatusEffect(statusEffect);
            }
        }
    }
}