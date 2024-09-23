using System;
using HarmonyLib;
using UnityEngine;

namespace EpicLoot.MagicItemEffects;

public class ModifyWeaponQuality
{
    [HarmonyPatch(typeof(ItemDrop.ItemData), nameof(ItemDrop.ItemData.GetDamage))]
    public static class ModifyWeaponQuality_ItemDrop_ItemData_GetDamage_Patch
    {
        public static void Prefix(ref int quality, ItemDrop.ItemData __instance)
        {
            quality += ModifyWeaponQuality_Humanoid_EquipItem_Patch.modifyWeaponQaulityValue;
        }
    }
    
    [HarmonyPatch(typeof(ItemDrop.ItemData), nameof(ItemDrop.ItemData.GetMaxDurability))]
    public static class ModifyWeaponQuality_ItemDrop_ItemData_GetMaxDurability_Patch
    {
        public static void Prefix(ref int quality)
        {
            quality += ModifyWeaponQuality_Humanoid_EquipItem_Patch.modifyWeaponQaulityValue;
        }
    }
    
    [HarmonyPatch(typeof(Humanoid), nameof(Humanoid.EquipItem))]
    public static class ModifyWeaponQuality_Humanoid_EquipItem_Patch
    {
        public static bool hasModifyWeaponQuality = false;
        public static int modifyWeaponQaulityValue = 0;
        
        public static void Postfix(Player __instance)
        {
            if (__instance.HasActiveMagicEffect(MagicEffectType.ModifyWeaponQuality))
            {
                hasModifyWeaponQuality = true;
                modifyWeaponQaulityValue =
                    Mathf.FloorToInt(__instance.GetTotalActiveMagicEffectValue(MagicEffectType.ModifyWeaponQuality, 1f));
            }
        }
    }
    
}