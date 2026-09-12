using System;
using System.Reflection;

namespace FavoriteItems
{
    internal static class EquipmentAndQuickSlotsIntegration
    {
        internal const string PluginGuid = "randyknapp.mods.equipmentandquickslots";

        private static MethodInfo _isSlotCell;
        private static bool _warned;

        internal static void Initialize()
        {
            Type apiType = Type.GetType("EquipmentAndQuickSlots.API, EquipmentAndQuickSlots", false);
            if (apiType == null)
            {
                FavoriteItemsPlugin.Log.LogInfo("EquipmentAndQuickSlots was not detected; favorite protection remains active.");
                return;
            }

            _isSlotCell = apiType.GetMethod("IsSlotCell", BindingFlags.Public | BindingFlags.Static,
                null, new Type[] { typeof(int), typeof(int), typeof(string).MakeByRefType() }, null);

            if (_isSlotCell == null)
                FavoriteItemsPlugin.Log.LogWarning("EquipmentAndQuickSlots was detected, but the IsSlotCell API is unavailable.");
            else
                FavoriteItemsPlugin.Log.LogInfo("Automatic EquipmentAndQuickSlots slot protection is active.");
        }

        internal static bool IsProtectedSlot(ItemDrop.ItemData item)
        {
            if (item == null || _isSlotCell == null || !FavoriteItemsPlugin.ProtectEquipmentAndQuickSlots.Value)
                return false;

            try
            {
                object[] arguments = { item.m_gridPos.x, item.m_gridPos.y, null };
                return (bool)_isSlotCell.Invoke(null, arguments);
            }
            catch (Exception ex)
            {
                if (!_warned)
                {
                    _warned = true;
                    FavoriteItemsPlugin.Log.LogWarning("Failed to query the EquipmentAndQuickSlots API: " + ex.Message);
                }
                return false;
            }
        }
    }
}
