# FavoriteItems public API

FavoriteItems 1.2.0 exposes API contract version `1` from the main `FavoriteItems.dll`. The API is
intended for calls from Unity's main thread.

## Contract

The public type is `FavoriteItems.API.FavoriteItemsApi`:

```csharp
public const int ApiVersion = 1;
public static bool IsEnabled { get; }
public static bool IsFavorite(ItemDrop.ItemData item);
public static bool ShouldPreventAutomaticMove(ItemDrop.ItemData item);
public static bool TrySetFavorite(ItemDrop.ItemData item, bool favorite);
public static bool TryToggleFavorite(ItemDrop.ItemData item, out bool isFavorite);
public static bool RegisterProtectionProvider(string ownerId, Func<ItemDrop.ItemData, bool> provider);
public static bool UnregisterProtectionProvider(string ownerId);
public static event Action<ItemDrop.ItemData, bool> FavoriteChanged;
```

`IsFavorite` reads the saved state even when FavoriteItems is disabled. Movement protection and
favorite mutations are active only while FavoriteItems is enabled. API mutations do not display
in-game notifications. `FavoriteChanged` is raised only for real favorite-state changes made
through FavoriteItems, not when Valheim replaces an item instance during load, split, or merge.

`ShouldPreventAutomaticMove` combines the manual favorite flag with every registered protection
provider. Quick-stack integrations should query it immediately before moving an item.

## Required integration

When FavoriteItems is a required dependency, reference `FavoriteItems.dll`, declare its BepInEx
dependency, and call the API directly:

```csharp
using BepInEx;
using FavoriteItems.API;

[BepInDependency("com.ronaldo.valheim.favoriteitems", BepInDependency.DependencyFlags.HardDependency)]
public sealed class MyInventoryPlugin : BaseUnityPlugin
{
    private bool CanMoveAutomatically(ItemDrop.ItemData item)
    {
        return !FavoriteItemsApi.ShouldPreventAutomaticMove(item);
    }
}
```

An additional-slot mod can register one predicate under its own BepInEx GUID:

```csharp
private void Awake()
{
    FavoriteItemsApi.RegisterProtectionProvider(PluginGuid, IsInMySpecialSlot);
}

private void OnDestroy()
{
    FavoriteItemsApi.UnregisterProtectionProvider(PluginGuid);
}

private bool IsInMySpecialSlot(ItemDrop.ItemData item)
{
    return item != null && IsSpecialCell(item.m_gridPos.x, item.m_gridPos.y);
}
```

Registering the same owner ID again replaces the previous predicate. FavoriteItems isolates and
logs provider exceptions, then continues evaluating the remaining providers.

## Optional integration

Do not place static references to FavoriteItems API types in code that must load without
FavoriteItems. Detect the plugin and resolve the API through reflection instead:

```csharp
using System;
using System.Reflection;

Type apiType = Type.GetType("FavoriteItems.API.FavoriteItemsApi, FavoriteItems", false);
MethodInfo shouldPrevent = apiType == null
    ? null
    : apiType.GetMethod(
        "ShouldPreventAutomaticMove",
        BindingFlags.Public | BindingFlags.Static,
        null,
        new Type[] { typeof(ItemDrop.ItemData) },
        null);

bool blocked = shouldPrevent != null &&
    (bool)shouldPrevent.Invoke(null, new object[] { item });
```

Cache the resolved type and methods during plugin initialization. If the type or method is absent,
continue with the integrating mod's normal behavior. Consumers may also inspect `ApiVersion` before
using newer API members.

## Compatibility guarantees

- Additive changes keep API contract version `1`.
- A breaking public-contract change increments `ApiVersion` and the FavoriteItems major version.
- The persisted favorite key remains an internal storage detail; integrations must use the API
  instead of reading or writing `m_customData` directly.
