using System;
using System.Reflection;
using HarmonyLib;

namespace FavoriteItems
{
    internal static class GorilaChestIntegration
    {
        internal const string PluginGuid = "dev.trentini.gorilachestmod";

        internal static void Initialize(Harmony harmony)
        {
            Type quickStackType = Type.GetType("GorilaChestMod.QuickStack, GorilaChestMod", false);
            if (quickStackType == null)
            {
                FavoriteItemsPlugin.Log.LogInfo("GorilaChestMod nao detectado; favoritos e indicador continuam ativos.");
                return;
            }

            MethodInfo canMove = AccessTools.Method(quickStackType, "CanMove",
                new Type[] { typeof(Player), typeof(Inventory), typeof(ItemDrop.ItemData) });
            MethodInfo postfix = AccessTools.Method(typeof(GorilaChestIntegration), "CanMovePostfix");

            if (canMove == null || postfix == null || canMove.ReturnType != typeof(bool))
            {
                FavoriteItemsPlugin.Log.LogWarning("GorilaChestMod detectado, mas QuickStack.CanMove mudou; protecao nao aplicada.");
                return;
            }

            harmony.Patch(canMove, null, new HarmonyMethod(postfix));
            FavoriteItemsPlugin.Log.LogInfo("Protecao do quick stack do GorilaChestMod ativa.");
        }

        public static void CanMovePostfix(ItemDrop.ItemData __2, ref bool __result)
        {
            ItemDrop.ItemData item = __2;
            if (!__result || !FavoriteItemsPlugin.Enabled.Value || item == null)
                return;

            if (FavoriteManager.IsFavorite(item) || EquipmentAndQuickSlotsIntegration.IsProtectedSlot(item))
                __result = false;
        }
    }
}
