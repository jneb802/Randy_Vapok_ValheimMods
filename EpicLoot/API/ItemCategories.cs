using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;

namespace EpicLoot;

public static partial class API
{
    internal sealed class ItemCategory
    {
        internal string Id;
        internal HashSet<string> Prefabs;
        internal HashSet<string> Effects;
        internal int[] EffectCounts;
    }

    private static readonly List<ItemCategory> ItemCategories = new();

    /// <summary>Registers non-equipment enchantables with an isolated effect pool.
    /// Counts are indexed by rarity (Magic through Ancient). These items have no sockets,
    /// equipment set assignment, or unique legendary assignment. Normal restrictions and
    /// the non-stacking requirement still apply. Call before players open enchanting UI.</summary>
    [PublicAPI]
    public static bool RegisterEnchantableItemCategory(string id, string[] prefabs,
        string[] effects, int[] effectCounts)
    {
        if (string.IsNullOrWhiteSpace(id) || prefabs == null || prefabs.Length == 0 ||
            effects == null || effects.Length == 0 || effectCounts == null || effectCounts.Length != Rarities.Count ||
            prefabs.Concat(effects).Any(string.IsNullOrWhiteSpace) ||
            effects.Distinct().Count() != effects.Length ||
            effectCounts.Any(n => n < 1 || n > effects.Length) ||
            ItemCategories.Any(c => c.Id == id || c.Prefabs.Overlaps(prefabs) || c.Effects.Overlaps(effects)))
            return false;
        ItemCategories.Add(new ItemCategory { Id = id,
            Prefabs = new HashSet<string>(prefabs, StringComparer.Ordinal),
            Effects = new HashSet<string>(effects, StringComparer.Ordinal),
            EffectCounts = (int[])effectCounts.Clone() });
        return true;
    }

    internal static ItemCategory FindItemCategory(ItemDrop.ItemData item) =>
        item?.m_dropPrefab == null ? null : ItemCategories.Find(c => c.Prefabs.Contains(item.m_dropPrefab.name));

    internal static bool CategoryAllowsEffect(ItemDrop.ItemData item, string effect)
    {
        ItemCategory category = FindItemCategory(item);
        return category != null ? category.Effects.Contains(effect) :
            !ItemCategories.Any(c => c.Effects.Contains(effect));
    }
}
