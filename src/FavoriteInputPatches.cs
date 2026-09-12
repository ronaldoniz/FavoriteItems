using System;
using System.Reflection;
using HarmonyLib;
using UnityEngine;

namespace FavoriteItems
{
    [HarmonyPatch(typeof(InventoryGrid), "OnLeftDown")]
    internal static class FavoriteInputPatches
    {
        private static readonly MethodInfo GetButtonPosMethod =
            AccessTools.Method(typeof(InventoryGrid), "GetButtonPos", new Type[] { typeof(GameObject) });

        [HarmonyPriority(Priority.First)]
        private static bool Prefix(InventoryGrid __instance, UIInputHandler __0)
        {
            UIInputHandler element = __0;
            if (!FavoriteItemsPlugin.Enabled.Value || !AltIsPressed() || element == null)
                return true;

            Player player = Player.m_localPlayer;
            if (player == null || __instance.GetInventory() != player.GetInventory() || GetButtonPosMethod == null)
                return true;

            try
            {
                Vector2i position = (Vector2i)GetButtonPosMethod.Invoke(__instance, new object[] { element.gameObject });
                ItemDrop.ItemData item = __instance.GetInventory().GetItemAt(position.x, position.y);
                if (item == null)
                    return true;

                bool favorite = FavoriteManager.Toggle(item);
                FavoriteManager.Notify(player, item, favorite);

                // The marker is refreshed by InventoryGrid.UpdateGui. Skipping the original
                // OnLeftDown prevents the same click from starting an item drag.
                return false;
            }
            catch (Exception ex)
            {
                FavoriteItemsPlugin.Log.LogWarning("Could not toggle favorite state: " + ex.Message);
                return true;
            }
        }

        private static bool AltIsPressed()
        {
            return ZInput.GetKey(KeyCode.LeftAlt, true) || ZInput.GetKey(KeyCode.RightAlt, true);
        }
    }
}
