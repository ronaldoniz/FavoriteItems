using System;
using System.Reflection;
using FavoriteItems.API;
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
                FavoriteItemsPlugin.Log.LogInfo("GorilaChestMod was not detected; favorites and the visual marker remain active.");
                return;
            }

            MethodInfo canMove = AccessTools.Method(quickStackType, "CanMove",
                new Type[] { typeof(Player), typeof(Inventory), typeof(ItemDrop.ItemData) });
            MethodInfo postfix = AccessTools.Method(typeof(GorilaChestIntegration), "CanMovePostfix");

            if (canMove == null || postfix == null || canMove.ReturnType != typeof(bool))
            {
                FavoriteItemsPlugin.Log.LogWarning("GorilaChestMod was detected, but QuickStack.CanMove has changed; protection was not applied.");
                return;
            }

            harmony.Patch(canMove, null, new HarmonyMethod(postfix));
            FavoriteItemsPlugin.Log.LogInfo("GorilaChestMod quick-stack protection is active.");
        }

        public static void CanMovePostfix(ItemDrop.ItemData __2, ref bool __result)
        {
            ItemDrop.ItemData item = __2;
            if (!__result || item == null)
                return;

            if (FavoriteItemsApi.ShouldPreventAutomaticMove(item))
                __result = false;
        }
    }
}
