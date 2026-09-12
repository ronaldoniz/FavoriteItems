using System;
using System.Reflection;
using FavoriteItems.API;

namespace FavoriteItems
{
    internal static class ExtraSlotsIntegration
    {
        internal const string PluginGuid = "shudnal.ExtraSlots";

        private static MethodInfo _isItemInSlot;
        private static bool _warned;

        internal static void Initialize()
        {
            Type apiType = Type.GetType("ExtraSlots.API, ExtraSlots", false);
            if (apiType == null)
            {
                FavoriteItemsPlugin.Log.LogInfo("ExtraSlots was not detected; favorite protection remains active.");
                return;
            }

            _isItemInSlot = apiType.GetMethod("IsItemInSlot", BindingFlags.Public | BindingFlags.Static,
                null, new Type[] { typeof(ItemDrop.ItemData) }, null);

            if (_isItemInSlot == null || _isItemInSlot.ReturnType != typeof(bool))
            {
                FavoriteItemsPlugin.Log.LogWarning("ExtraSlots was detected, but the IsItemInSlot API is unavailable.");
                return;
            }

            FavoriteItemsApi.RegisterProtectionProvider(PluginGuid, IsProtectedSlot);
            FavoriteItemsPlugin.Log.LogInfo("Optional ExtraSlots slot protection is available.");
        }

        internal static void Shutdown()
        {
            FavoriteItemsApi.UnregisterProtectionProvider(PluginGuid);
            _isItemInSlot = null;
            _warned = false;
        }

        internal static bool IsProtectedSlot(ItemDrop.ItemData item)
        {
            if (item == null || _isItemInSlot == null || !FavoriteItemsPlugin.ProtectExtraSlots.Value)
                return false;

            try
            {
                return (bool)_isItemInSlot.Invoke(null, new object[] { item });
            }
            catch (Exception ex)
            {
                if (!_warned)
                {
                    _warned = true;
                    FavoriteItemsPlugin.Log.LogWarning("Failed to query the ExtraSlots API: " + ex.Message);
                }

                return false;
            }
        }
    }
}
