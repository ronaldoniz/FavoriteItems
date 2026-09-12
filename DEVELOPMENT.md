# FavoriteItems development

This document contains technical information for contributors and maintainers. Player-facing usage
and installation instructions are kept in `README.md`.

## Building from source

Install the .NET SDK and run this command in the project directory:

```powershell
dotnet build -c Release
```

If Valheim is installed elsewhere:

```powershell
dotnet build -c Release -p:ValheimDir="D:\SteamLibrary\steamapps\common\Valheim"
```

The compiled file is written to `bin\Release\FavoriteItems.dll`. The project does not distribute
Valheim assemblies; the build references the local game installation selected by `ValheimDir`.

## Packaging

After compiling a Release build, create and validate the Thunderstore ZIP with:

```powershell
.\scripts\package-thunderstore.ps1
```

The script verifies version consistency, metadata, icon dimensions, package structure, the DLL
version, and the project's English-language policy.

## Persistence and stack safety

Favorite state is stored only in this `ItemData.m_customData` key:

```text
com.ronaldo.valheim.favoriteitems.favorite = 1
```

Valheim 1.0.7 saves and clones `m_customData`, while its stack compatibility checks ignore it. A
favorite therefore follows the item without preventing compatible stacks from merging. When a
favorite stack is merged into another compatible stack, FavoriteItems transfers the favorite state
to the receiving stack. Removing a favorite deletes only the key shown above.

## Compatibility implementation

- GorilaChestMod support uses a Harmony postfix on
  `GorilaChestMod.QuickStack.CanMove(Player, Inventory, ItemData)`.
- EquipmentAndQuickSlots support discovers `EquipmentAndQuickSlots.API.IsSlotCell` through
  reflection, keeping it an optional dependency.

## Test status

- Loading and usage in a single-player game were tested by the author for version 1.0.1.
- Loading and usage on a multiplayer server were tested by the author for version 1.0.1.
- Version 1.0.2 changes English text, release metadata, and packaging checks without intentionally
  changing inventory behavior.

These are author tests, not independent certification for every combination of mods.

## Recommended release verification

1. Load a profile containing BepInEx, GorilaChestMod, EquipmentAndQuickSlots, and FavoriteItems.
2. Toggle a favorite and confirm the English notification and golden star.
3. Run quick stack with favorite and non-favorite stacks of the same item.
4. Split and merge a favorite stack and verify that its state is preserved.
5. Confirm that an EquipmentAndQuickSlots quick-slot item is not moved.
6. Restart the character and confirm persistence.
7. Check `BepInEx\LogOutput.log` for English status messages and patching errors.
