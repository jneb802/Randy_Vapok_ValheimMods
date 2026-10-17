using JetBrains.Annotations;

namespace EpicLootAPI;

public static partial class EpicLoot
{
    private static readonly Method API_RegisterEnchantableItemCategory = new(
        "RegisterEnchantableItemCategory", typeof(string), typeof(string[]), typeof(string[]), typeof(int[]));

    /// <summary>API v2: register non-equipment enchantables with an isolated effect pool.
    /// Counts run from Magic through Ancient. Returns false on unsupported API or invalid input.</summary>
    [PublicAPI]
    public static bool RegisterEnchantableItemCategory(string id, string[] prefabs, string[] effects, int[] effectCounts)
    {
        return (bool)(API_RegisterEnchantableItemCategory.Invoke(id, prefabs, effects, effectCounts)[0] ?? false);
    }
}
