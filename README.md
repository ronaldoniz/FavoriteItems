# FavoriteItems for Valheim

Keep important inventory stacks safe from automatic movement with a simple favorite marker.

## Features

- Mark or unmark a stack with `Alt + left-click`.
- See favorites at a glance with a subtle golden star.
- Keep favorite items out of supported automatic inventory actions.
- Optionally protect EquipmentAndQuickSlots equipment and quick slots without favoriting them.
- Preserve favorites when saving, moving, splitting, or merging stacks.
- Provide a public API for compatible inventory and quick-stack mods.
- Use the mod client-side without requiring it on the server.

## How to use

1. Open your inventory.
2. Hold the left or right `Alt` key.
3. Left-click an item stack.

The golden star means that the stack is protected. Repeat the same action to remove the favorite.

## Installation

### Thunderstore Mod Manager or r2modman

Install **FavoriteItems** from the Valheim community and launch the game through your mod manager.

### Manual installation

Install BepInEx 5, then place `FavoriteItems.dll` at:

```text
<Valheim>\BepInEx\plugins\ronaldoniz-FavoriteItems\FavoriteItems.dll
```

The configuration file is created at:

```text
BepInEx\config\com.ronaldo.valheim.favoriteitems.cfg
```

## Compatibility

- **GorilaChestMod 2.4.0:** favorite stacks are not moved during quick stack.
- **EquipmentAndQuickSlots 3.x:** equipment and quick-slot cells can be protected automatically,
  even without a favorite marker, when the compatibility option is enabled.
- Both integrations are optional. FavoriteItems continues to work when either mod is absent.

Favorites are currently guaranteed to block automatic movement only in the supported
GorilaChestMod integration. Other inventory and quick-stack mods require their own integration.

## Configuration

- Enable or disable the favorite system.
- Show or hide the golden star.
- Show or hide favorite notifications.
- Enable optional automatic EquipmentAndQuickSlots protection. This is disabled by default for new
  installations so that unmarked items behave predictably.

## For mod authors

FavoriteItems provides a small public API to query or change favorites, block automatic movement,
and register protected-slot providers. The [API reference](https://github.com/ronaldoniz/FavoriteItems/blob/main/docs/API.md)
includes direct-reference and optional-integration examples.

## Known limitations

- The shortcut currently supports keyboard and mouse only.
- Quick-stack protection is currently specific to GorilaChestMod.
- EquipmentAndQuickSlots protection depends on its current public `IsSlotCell` API.

## Source and support

FavoriteItems is open source under the MIT License. Source code, releases, and issue reporting are
available on [GitHub](https://github.com/ronaldoniz/FavoriteItems).

## Tip jar

Liked the mod? You can leave a small tip on [Ko-fi](https://ko-fi.com/ronaldoniz) — completely
optional, always appreciated.
