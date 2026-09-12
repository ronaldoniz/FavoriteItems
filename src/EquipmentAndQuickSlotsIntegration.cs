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
                FavoriteItemsPlugin.Log.LogInfo("EquipmentAndQuickSlots nao detectado; protecao por favoritos continua ativa.");
                return;
            }

            _isSlotCell = apiType.GetMethod("IsSlotCell", BindingFlags.Public | BindingFlags.Static,
                null, new Type[] { typeof(int), typeof(int), typeof(string).MakeByRefType() }, null);

            if (_isSlotCell == null)
                FavoriteItemsPlugin.Log.LogWarning("EquipmentAndQuickSlots detectado, mas a API IsSlotCell nao esta disponivel.");
            else
                FavoriteItemsPlugin.Log.LogInfo("Protecao automatica dos slots do EquipmentAndQuickSlots ativa.");
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
                    FavoriteItemsPlugin.Log.LogWarning("Falha ao consultar a API do EquipmentAndQuickSlots: " + ex.Message);
                }
                return false;
            }
        }
    }
}
