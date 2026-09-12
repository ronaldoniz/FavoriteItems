using System.Collections.Generic;

namespace FavoriteItems
{
    internal static class FavoriteManager
    {
        // Stored on the ItemData so the favorite follows moves, splits, drops and save/load.
        // Valheim 1.0.7 persists and clones m_customData, while its stack matching ignores it.
        internal const string FavoriteKey = FavoriteItemsPlugin.PluginGuid + ".favorite";

        internal static bool IsFavorite(ItemDrop.ItemData item)
        {
            string value;
            return item != null && item.m_customData != null &&
                   item.m_customData.TryGetValue(FavoriteKey, out value) && value == "1";
        }

        internal static bool SetFavorite(ItemDrop.ItemData item, bool favorite)
        {
            if (item == null)
                return false;

            if (item.m_customData == null)
                item.m_customData = new Dictionary<string, string>();

            if (favorite)
                item.m_customData[FavoriteKey] = "1";
            else
                item.m_customData.Remove(FavoriteKey);

            return favorite;
        }

        internal static bool Toggle(ItemDrop.ItemData item)
        {
            return SetFavorite(item, !IsFavorite(item));
        }

        internal static void Notify(Player player, ItemDrop.ItemData item, bool favorite)
        {
            if (player == null || item == null || !FavoriteItemsPlugin.ShowMessages.Value)
                return;

            string name = item.m_shared != null ? item.m_shared.m_name : "item";
            if (Localization.instance != null)
                name = Localization.instance.Localize(name);

            player.Message(MessageHud.MessageType.Center,
                favorite ? "★ Favorite: " + name : "☆ Removed from favorites: " + name);
        }
    }
}
