using System.Collections.Generic;
using HarmonyLib;
using TMPro;
using UnityEngine;

namespace EpicLoot.MagicItemEffects
{
    [HarmonyPatch(typeof(Player), nameof(Player.UpdateFood))]
    public static class ModifyFoodDuration_Player_UpdateFood_Patch
    {
        public static void Prefix(Player __instance, ref float dt)
        {
            if (__instance.IsPlayer())
            {
                float extensionFactor = 1f + __instance.GetTotalActiveMagicEffectValue(MagicEffectType.ModifyFoodDuration, 0.01f);
                
                dt /= extensionFactor;
            }
        }
    }
    
    [HarmonyPatch(typeof(Hud), nameof(Hud.UpdateFood))]
    public static class ModifyFoodDisplay_Hud_UpdateFood_Patch
    {
        public static void Prefix(Player player, ref float __state)
        {
            float extensionFactor = 1f + player.GetTotalActiveMagicEffectValue(MagicEffectType.ModifyFoodDuration, 0.01f);
            
            __state = extensionFactor;
        }

        public static void Postfix(Player player, float __state)
        {
            // Iterate through each food item in the player's food list
            List<Player.Food> foods = player.GetFoods();

            // Apply the extension factor to the remaining time for each food item
            for (int i = 0; i < foods.Count; i++)
            {
                Player.Food food = foods[i];

                // Calculate the extended remaining time
                float extendedTime = food.m_time * __state;

                // Find the corresponding TMP_Text element and update its text
                TMP_Text tmpText = Hud.instance.m_foodTime[i];
                
                if (extendedTime >= 60.0f)
                {
                    tmpText.text = Mathf.CeilToInt(extendedTime / 60f).ToString() + "m";
                }
                else
                {
                    tmpText.text = Mathf.FloorToInt(extendedTime).ToString() + "s";
                }
            }
        }
    }
}