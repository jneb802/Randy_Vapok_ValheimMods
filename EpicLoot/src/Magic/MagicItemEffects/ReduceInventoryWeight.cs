using HarmonyLib;

namespace EpicLoot.MagicItemEffects;

public class ReduceInventoryWeight
{
    [HarmonyPatch(typeof(ItemDrop.ItemData), nameof(ItemDrop.ItemData.GetWeight))]
    public static class ReduceInventoryWeight_ItemData_GetWeight_Patch
    {
        public static void Postfix(ItemDrop.ItemData __instance, ref float __result)
        {
            var player = PlayerExtensions.GetPlayerWithEquippedItem(__instance);
            if (player == null)
            {
                return;
            }
            
            if (__instance.HasMagicEffect(MagicEffectType.Weightless))
            {
                __result = 0;
            }

            if (player.HasActiveMagicEffect(MagicEffectType.ReduceInventoryWeight))
            {
                float weightReduceMultiplier =
                    player.GetTotalActiveMagicEffectValue(MagicEffectType.ReduceInventoryWeight, 0.01f);
                __result *= 1 - weightReduceMultiplier;
            }
        }
    }

    [HarmonyPatch(typeof(ItemDrop.ItemData), nameof(ItemDrop.ItemData.GetNonStackedWeight))]
    public static class ReduceInventoryWeight_ItemData_GetNonstackedWeight_Patch
    {
        public static void Postfix(ItemDrop.ItemData __instance, ref float __result)
        {
            var player = PlayerExtensions.GetPlayerWithEquippedItem(__instance);
            if (player == null)
            {
                return;
            }
            if (player.HasActiveMagicEffect(MagicEffectType.ReduceInventoryWeight))
            {
                float weightReduceMultiplier =
                    player.GetTotalActiveMagicEffectValue(MagicEffectType.ReduceInventoryWeight, 0.01f);
                __result *= 1 - weightReduceMultiplier;
            }
        }
    }

    [HarmonyPatch(typeof(Inventory), nameof(Inventory.GetTotalWeight))]
    public static class ReduceInventoryWeight_Player_Weight
    {
        public static void Postfix(Inventory __instance, ref float __result)
        {
            var player = Player.m_localPlayer;
            if (player == null)
            {
                return;
            }
            if (player.HasActiveMagicEffect(MagicEffectType.ReduceInventoryWeight))
            {
                float weightReduceMultiplier =
                    player.GetTotalActiveMagicEffectValue(MagicEffectType.ReduceInventoryWeight, 0.01f);
                __result *= 1 - weightReduceMultiplier;
            }
        }
    }
}