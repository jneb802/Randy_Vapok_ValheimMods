using System;
using System.Reflection;
using System.Runtime.Serialization;
using EpicLoot;
using EpicLoot.ShardStones;

internal static class Program {
    private static int checks;

    private static void Check(bool condition, string message) {
        checks++;
        if (!condition) throw new Exception(message);
    }

    // Avoid Unity-native constructors. These tests exercise managed socket rules, not the game UI.
    private static ItemDrop.ItemData Item(ItemDrop.ItemData.ItemType type) {
        ItemDrop.ItemData item = (ItemDrop.ItemData)FormatterServices.GetUninitializedObject(typeof(ItemDrop.ItemData));
        item.m_shared = (ItemDrop.ItemData.SharedData)FormatterServices.GetUninitializedObject(typeof(ItemDrop.ItemData.SharedData));
        item.m_shared.m_itemType = type;
        item.m_shared.m_ammoType = "";
        return item;
    }

    private static bool Placement(ItemDrop.ItemData equipment, ItemDrop.ItemData shard, out string reason) {
        object[] args = { equipment, shard, null, ShardType.None, ItemRarity.Magic, null };
        bool result = (bool)typeof(ShardSocketManager).GetMethod("ResolvePlacementEffect", BindingFlags.NonPublic | BindingFlags.Static)
            .Invoke(null, args);
        reason = (string)args[5];
        return result;
    }

    private static ShardEffectDefinition Effect(string name, float value = 5) {
        return new ShardEffectDefinition {
            EffectType = name,
            ValuesPerRarity = { [ItemRarity.Epic] = value }
        };
    }

    private static void Main() {
        ShardType color = (ShardType)123456;
        ShardDefinition definition = new ShardDefinition {
            TypeEffects = {
                [ShardSlotCategory.Head] = Effect("HeadEffect"),
                [ShardSlotCategory.Chest] = Effect("ChestEffect")
            }
        };
        Shards.ShardDefinitions.ShardEffects[color] = definition;
        ItemDrop.ItemData shard = Item(ItemDrop.ItemData.ItemType.Material);
        shard.m_shared.m_ammoType = $"{color}|Epic|ShardStone";
        ItemDrop.ItemData head = Item(ItemDrop.ItemData.ItemType.Helmet);
        ItemDrop.ItemData chest = Item(ItemDrop.ItemData.ItemType.Chest);
        ItemDrop.ItemData legs = Item(ItemDrop.ItemData.ItemType.Legs);

        Check(Placement(head, shard, out _), "head must accept its mapped effect");
        Check(Placement(chest, shard, out _), "chest must accept its different effect");
        Check(!Placement(legs, shard, out string reason), "unmapped legs must reject insertion");
        Check(reason == "$mod_epicloot_socket_shardunsupported", "unsupported placement must explain why");
        Check(ShardSocketManager.ResolveSocketedEffect(legs, shard, out MagicItemEffect effect, out ShardType storedColor, out ItemRarity rarity),
            "existing unsupported stones must remain resolvable for saves and removal");
        Check(effect == null && storedColor == color && rarity == ItemRarity.Epic, "unsupported saved socket must retain identity without an effect");
        Check(ShardSocketManager.ResolveSocketedEffect(chest, shard, out effect, out _, out _) && effect.EffectType == "ChestEffect", "chest must resolve its own effect");

        definition.TypeEffects[ShardSlotCategory.Armor] = Effect("ArmorEffect");
        Check(Placement(legs, shard, out _), "broad armor group must cover legs");
        Check(ShardSocketManager.ResolveSocketedEffect(head, shard, out effect, out _, out _) && effect.EffectType == "HeadEffect", "specific head mapping must override armor group");
        definition.TypeEffects[ShardSlotCategory.Head].ValuesPerRarity.Clear();
        Check(!Placement(head, shard, out _), "specific mapping with missing rarity must not fall back to armor");
        definition.TypeEffects[ShardSlotCategory.Head].ValuesPerRarity[ItemRarity.Epic] = 0;
        Check(Placement(head, shard, out _), "zero-valued passive effects remain valid");

        definition.UniformEffect = Effect("UniformEffect");
        Check(Placement(Item(ItemDrop.ItemData.ItemType.Utility), shard, out _), "uniform shards still support other gear");
        Check(ShardSocketManager.ResolveSocketedEffect(head, shard, out effect, out _, out _) && effect.EffectType == "UniformEffect", "uniform effect retains priority");
        shard.m_shared.m_ammoType = $"{color}|Magic|ShardStone";
        Check(!Placement(head, shard, out _), "missing uniform rarity must reject placement");
        Check(!Placement(head, Item(ItemDrop.ItemData.ItemType.Material), out reason), "ordinary material must be rejected");
        Check(reason == "$mod_epicloot_socket_invalidinput", "ordinary material must retain invalid-input message");
        Shards.ShardDefinitions.ShardEffects.Remove(color);
        System.Console.WriteLine($"Passed {checks} managed shard placement checks. Live UI validation remains required.");
    }
}
