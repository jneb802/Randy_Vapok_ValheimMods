# Shard gear support checks

This executable compiles the current `ShardSocketManager.cs` against a built Epic Loot reference
assembly and the installed game libraries. It runs the managed placement gate without starting Unity.
The reference assembly must provide the current shard definition and classification APIs.
This permits focused checks when an unrelated file prevents building the full mod.

```sh
dotnet build tests/ShardGearSupport/ShardGearSupport.csproj \
  -p:EpicLootAssembly=/absolute/path/to/EpicLoot.dll \
  -p:GamePathManaged=/absolute/path/to/Valheim/Managed \
  -p:BepinexPath=/absolute/path/to/BepInEx
mono tests/ShardGearSupport/bin/Debug/net481/ShardGearSupport.exe
```

On Windows, run the built executable directly instead of using Mono.

The checks cover selected gear mappings, different effects by type, broad-group fallback,
specific-type priority, missing rarity values, zero-valued effects, uniform effects, invalid input,
and preserving the identity of existing unsupported socketed stones. Game objects are constructed
without Unity-native initialization. This does not test the inventory UI or multiplayer.

Before release, use the current Praetoris Season 8 test profiles to check:

1. A shard mapped only to Head and Chest accepts both types and rejects weapons and other armor.
2. Drag insertion, quick transfer, socket-to-inventory swaps, and inventory-to-socket swaps all obey
   the same rule without consuming or duplicating items.
3. The loose-shard tooltip and compendium show supported types and gear-specific exclusivity limits.
4. Existing uniform shards and runestones still follow their existing rules.
5. An unsupported stone already in an item survives opening, closing, saving, and loading its sockets.
6. Changes to mappings behave correctly after configuration synchronization and socket recomputation.
