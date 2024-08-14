using HarmonyLib;
using UnityEngine;

namespace EpicLoot.MagicItemEffects
{
    [HarmonyPatch(typeof(SE_Demister), nameof(SE_Demister.UpdateStatusEffect))]
    public static class ModifyWispRange_SE_Demister_UpdateStatusEffect_Patch
    {
        private static float _originalEndRange = -1f;
        
        public static void Prefix(SE_Demister __instance)
        {
            if (__instance.m_character.IsPlayer())
            {
                var player = (Player)__instance.m_character;
                var multiplier = player.GetTotalActiveMagicEffectValue(MagicEffectType.ModifyWispRange, 0.01f);
                
                var ball = __instance.m_ballPrefab;
                if (ball != null)
                {
                    var forceField = ball.GetComponentInChildren<ParticleSystemForceField>();
                    if (forceField != null)
                    {
                        if (_originalEndRange < 0)
                        {
                            _originalEndRange = forceField.endRange;
                        }
                        
                        if (player.HasActiveMagicEffect(MagicEffectType.ModifyWispRange))
                        {
                            forceField.endRange = _originalEndRange * (1 + multiplier);
                            return;
                        }
                        forceField.endRange = _originalEndRange;
                    }
                }
            }
        }
    }
}