using System;

namespace FavoriteItems.API
{
    /// <summary>
    /// Stable integration surface for inventory, slot, and automatic-movement mods.
    /// Call this API only from Unity's main thread.
    /// </summary>
    public static class FavoriteItemsApi
    {
        /// <summary>
        /// Version of the public API contract. This changes only for breaking API changes.
        /// </summary>
        public const int ApiVersion = 1;

        /// <summary>
        /// True after FavoriteItems has loaded and its main feature is enabled.
        /// </summary>
        public static bool IsEnabled
        {
            get
            {
                return FavoriteItemsPlugin.Enabled != null && FavoriteItemsPlugin.Enabled.Value;
            }
        }

        /// <summary>
        /// Raised after the favorite flag on an item is changed through FavoriteItems.
        /// </summary>
        public static event Action<ItemDrop.ItemData, bool> FavoriteChanged;

        /// <summary>
        /// Reads the persisted favorite flag, even when FavoriteItems is disabled.
        /// </summary>
        public static bool IsFavorite(ItemDrop.ItemData item)
        {
            return FavoriteManager.IsFavorite(item);
        }

        /// <summary>
        /// Returns whether an automatic inventory operation should leave this item in place.
        /// </summary>
        public static bool ShouldPreventAutomaticMove(ItemDrop.ItemData item)
        {
            if (!IsEnabled || item == null)
                return false;

            return FavoriteManager.IsFavorite(item) || ProtectionRegistry.IsProtected(item);
        }

        /// <summary>
        /// Sets the favorite flag. Returns false only when the request cannot be accepted.
        /// </summary>
        public static bool TrySetFavorite(ItemDrop.ItemData item, bool favorite)
        {
            if (!IsEnabled || item == null)
                return false;

            bool previous = FavoriteManager.IsFavorite(item);
            if (previous == favorite)
                return true;

            FavoriteManager.SetFavorite(item, favorite);
            RaiseFavoriteChanged(item, favorite);
            return true;
        }

        /// <summary>
        /// Toggles the favorite flag and returns the resulting state through isFavorite.
        /// </summary>
        public static bool TryToggleFavorite(ItemDrop.ItemData item, out bool isFavorite)
        {
            isFavorite = FavoriteManager.IsFavorite(item);
            if (!IsEnabled || item == null)
                return false;

            isFavorite = !isFavorite;
            FavoriteManager.SetFavorite(item, isFavorite);
            RaiseFavoriteChanged(item, isFavorite);
            return true;
        }

        /// <summary>
        /// Registers or replaces an item-protection provider under a BepInEx plugin GUID.
        /// </summary>
        public static bool RegisterProtectionProvider(string ownerId, Func<ItemDrop.ItemData, bool> provider)
        {
            return ProtectionRegistry.Register(ownerId, provider);
        }

        /// <summary>
        /// Removes a previously registered protection provider.
        /// </summary>
        public static bool UnregisterProtectionProvider(string ownerId)
        {
            return ProtectionRegistry.Unregister(ownerId);
        }

        internal static void Shutdown()
        {
            FavoriteChanged = null;
        }

        private static void RaiseFavoriteChanged(ItemDrop.ItemData item, bool favorite)
        {
            Action<ItemDrop.ItemData, bool> handlers = FavoriteChanged;
            if (handlers == null)
                return;

            Delegate[] subscribers = handlers.GetInvocationList();
            for (int i = 0; i < subscribers.Length; ++i)
            {
                try
                {
                    ((Action<ItemDrop.ItemData, bool>)subscribers[i])(item, favorite);
                }
                catch (Exception ex)
                {
                    if (FavoriteItemsPlugin.Log != null)
                        FavoriteItemsPlugin.Log.LogWarning("A FavoriteChanged subscriber failed: " + ex.Message);
                }
            }
        }
    }
}
