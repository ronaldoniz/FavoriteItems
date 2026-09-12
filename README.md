# FavoriteItems for Valheim 1.0

A lightweight client-side mod that lets you mark inventory stacks as favorites and prevents
**GorilaChestMod** from moving them into chests during quick stack.

## Usage

1. Open your inventory.
2. Hold the left or right `Alt` key.
3. Left-click an item stack.
4. A small golden star indicates that the stack is protected.

Repeat `Alt + left-click` to remove the favorite.

## Compatibility

- **GorilaChestMod 2.2.2**: applies a Harmony postfix to
  `GorilaChestMod.QuickStack.CanMove(Player, Inventory, ItemData)`. Favorite items are rejected
  before they can be moved.
- **EquipmentAndQuickSlots 3.x**: optional reflection-based integration with the public
  `EquipmentAndQuickSlots.API.IsSlotCell` API. Active equipment and quick-slot cells are
  automatically protected, even when they do not have a favorite star.
- Both mods are optional dependencies. FavoriteItems continues to load without them.

## Persistence and stack safety

Favorite state is stored only in this `ItemData.m_customData` key:

```
com.ronaldo.valheim.favoriteitems.favorite = 1
```

This preserves data created by other mods. In Valheim 1.0.7, `m_customData` is saved with the
character and copied by `ItemData.Clone()`, but it is not considered by `IsSameType` or
`FindFreeStackItem`. As a result, the favorite marker does not prevent compatible stacks from
merging.

When a favorite stack is split, both resulting stacks remain favorites. When it is merged into
another compatible stack, the destination stack becomes a favorite as well. Removing a favorite
deletes only the custom-data key shown above.

## Installation

Requirements:

- Valheim 1.0 using the Mono backend;
- BepInEx 5 (`denikson-BepInExPack_Valheim`);
- GorilaChestMod only if you want quick-stack protection;
- EquipmentAndQuickSlots only if you want automatic protection for its extra slots.

### Thunderstore Mod Manager / r2modman profile

Place `FavoriteItems.dll` at:

```
<profile>\BepInEx\plugins\ronaldoniz-FavoriteItems\FavoriteItems.dll
```

### Manual installation

Place `FavoriteItems.dll` at:

```
<Valheim>\BepInEx\plugins\ronaldoniz-FavoriteItems\FavoriteItems.dll
```

Start the game and confirm in `BepInEx\LogOutput.log` that FavoriteItems 1.0.2 was loaded. When
GorilaChestMod is installed and its supported integration is found, the log also confirms that
quick-stack protection is active.

The configuration file is created at:

```
BepInEx\config\com.ronaldo.valheim.favoriteitems.cfg
```

## Building from source

Install the .NET SDK and run this command in the project directory:

```powershell
dotnet build -c Release
```

If Valheim is installed elsewhere:

```powershell
dotnet build -c Release -p:ValheimDir="D:\SteamLibrary\steamapps\common\Valheim"
```

The compiled file is written to `bin\Release\FavoriteItems.dll`.

The project does not include Valheim assemblies. The build uses the assemblies from the local
game installation specified by `ValheimDir`.

## Test status

- Loading and usage in a single-player game: tested by the author.
- Loading and usage on a multiplayer server: tested by the author.
- Local build of the published version: verified before release.

These tests were performed by the author in their own game environment and do not constitute an
independent certification for every possible combination of mods.

## Releases and updates

Compiled versions are available from
[GitHub Releases](https://github.com/ronaldoniz/FavoriteItems/releases). Each fix receives a new
semantic version; files from previous releases are not replaced.

## Recommended in-game verification

1. Create a test profile with BepInEx, GorilaChestMod, and FavoriteItems.
2. Put two identical stacks in your inventory and the same item in a nearby chest.
3. Favorite only one stack and run GorilaChestMod quick stack.
4. Confirm that the favorite remains and the other stack is moved.
5. Split and merge the favorite stack, then confirm the star remains on the resulting stacks.
6. With EquipmentAndQuickSlots installed, place food in `Quick1` and confirm it is not moved.
7. Exit normally, load the character again, and confirm that the star persists.

## Known limitations

- The shortcut currently supports keyboard and mouse only; there is no gamepad binding.
- EAQS integration requires the `IsSlotCell` API provided by current 3.x versions. If that API
  changes, the log reports the problem and disables only automatic slot protection; manual
  favorites continue to work.
- Quick-stack protection is currently specific to GorilaChestMod.
