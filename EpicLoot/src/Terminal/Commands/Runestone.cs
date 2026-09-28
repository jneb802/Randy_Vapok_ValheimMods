using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using EpicLoot.Crafting;
using Jotunn.Managers;
using UnityEngine;

namespace EpicLoot;

public static partial class TerminalManager
{
    private static List<string> GetRunestoneOptions(string[] args)
    {
        return args.Length switch
        {
            2 => Enum.GetNames(typeof(ItemRarity)).Select(name => name.ToLowerInvariant()).ToList(),
            3 => MagicItemEffectDefinitions.AllDefinitions.Keys.ToList(),
            _ => []
        };
    }

    private static void SpawnRunestone(Terminal.ConsoleEventArgs args)
    {
        if (args.Length < 4 || args.Length > 5)
        {
            args.Context.AddString("> Usage: runestone <rarity> <effect> <value> [amount: 1-100]");
            return;
        }

        Player player = Player.m_localPlayer;
        if (player == null)
        {
            args.Context.AddString("> Enter a world with a player first.");
            return;
        }

        // Accept rarity names only: enum parsing also accepts undefined numbers and comma lists.
        string rarityName = Enum.GetNames(typeof(ItemRarity)).FirstOrDefault(
            name => string.Equals(name, args[1], StringComparison.OrdinalIgnoreCase));
        if (rarityName == null)
        {
            args.Context.AddString($"> Unknown rarity '{args[1]}'. Use {string.Join("/", Enum.GetNames(typeof(ItemRarity)))}.");
            return;
        }
        ItemRarity rarity = (ItemRarity)Enum.Parse(typeof(ItemRarity), rarityName);

        string effectType = args[2];
        if (!MagicItemEffectDefinitions.TryGet(effectType, out MagicItemEffectDefinition effectDefinition))
        {
            args.Context.AddString($"> Unknown effect '{effectType}'. Use Tab to select an effect ID.");
            return;
        }

        if (!float.TryParse(args[3], NumberStyles.Float, CultureInfo.InvariantCulture, out float value) ||
            float.IsNaN(value) || float.IsInfinity(value))
        {
            args.Context.AddString("> Value must be a finite number. Use a period for decimals.");
            return;
        }

        // These effects are normalized to 1 on load. Reject values we cannot preserve exactly.
        if (MagicItemEffectDefinitions.IsValuelessEffect(effectDefinition.Type) && value != MagicItemEffect.DefaultValue)
        {
            args.Context.AddString($"> Effect '{effectType}' has no numeric strength. Use value 1.");
            return;
        }

        int amount = 1;
        if (args.Length == 5 && (!int.TryParse(args[4], NumberStyles.Integer, CultureInfo.InvariantCulture, out amount) ||
            amount < 1 || amount > 100))
        {
            args.Context.AddString("> Amount must be a whole number from 1 to 100.");
            return;
        }

        string prefabName = $"EtchedRunestone{rarity}";
        GameObject prefab = PrefabManager.Instance.GetPrefab(prefabName);
        ItemDrop template = prefab != null ? prefab.GetComponent<ItemDrop>() : null;
        if (template == null || !template.m_itemData.IsRunestone())
        {
            args.Context.AddString($"> Could not find runestone prefab '{prefabName}'.");
            return;
        }

        for (int index = 0; index < amount; index++)
        {
            ItemDrop.ItemData item = template.m_itemData.Clone();
            item.m_dropPrefab = prefab;
            item.m_stack = 1;
            MagicItem magicItem = new MagicItem
            {
                Rarity = rarity,
                Effects = new List<MagicItemEffect> { new MagicItemEffect(effectType, value) }
            };
            API.WithChangeReason(API.ChangeReason.Rune, () => item.SaveMagicItem(magicItem));

            Vector3 position = player.transform.position + player.transform.forward * 2f + Vector3.up;
            ItemDrop.DropItem(item, 1, position, Quaternion.identity);
        }

        args.Context.AddString($"> Spawned {amount} {prefabName} with {effectType} = {value.ToString("R", CultureInfo.InvariantCulture)}.");
    }
}
