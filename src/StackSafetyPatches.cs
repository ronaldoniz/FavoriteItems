using System;
using System.Collections.Generic;
using HarmonyLib;

namespace FavoriteItems
{
    // m_customData does not participate in Valheim's stack compatibility check. These patches
    // make the semantic behavior predictable: when any part of a favorite stack is merged into
    // another compatible stack, the receiving stack becomes favorite too.
    internal static class StackSafetyPatches
    {
        internal sealed class StackSnapshot
        {
            internal readonly List<StackEntry> Entries = new List<StackEntry>();
        }

        internal sealed class StackEntry
        {
            internal ItemDrop.ItemData Item;
            internal int Amount;
        }

        [HarmonyPatch(typeof(Inventory), "AddItem", new Type[] { typeof(ItemDrop.ItemData) })]
        internal static class AddItemWholePatch
        {
            private static void Prefix(Inventory __instance, ItemDrop.ItemData __0, out StackSnapshot __state)
            {
                ItemDrop.ItemData item = __0;
                __state = null;
                if (!FavoriteItemsPlugin.Enabled.Value || !FavoriteManager.IsFavorite(item) ||
                    __instance == null || item.m_shared == null)
                    return;

                StackSnapshot snapshot = new StackSnapshot();
                List<ItemDrop.ItemData> items = __instance.GetAllItems();
                for (int i = 0; i < items.Count; ++i)
                {
                    ItemDrop.ItemData existing = items[i];
                    if (existing != null && existing.m_shared != null && existing.IsSameType(item) &&
                        existing.m_stack < existing.m_shared.m_maxStackSize)
                    {
                        StackEntry entry = new StackEntry();
                        entry.Item = existing;
                        entry.Amount = existing.m_stack;
                        snapshot.Entries.Add(entry);
                    }
                }
                __state = snapshot;
            }

            private static void Postfix(StackSnapshot __state)
            {
                if (__state == null)
                    return;

                for (int i = 0; i < __state.Entries.Count; ++i)
                {
                    StackEntry entry = __state.Entries[i];
                    if (entry.Item != null && entry.Item.m_stack > entry.Amount)
                        FavoriteManager.SetFavorite(entry.Item, true);
                }
            }
        }

        [HarmonyPatch(typeof(Inventory), "AddItem",
            new Type[] { typeof(ItemDrop.ItemData), typeof(int), typeof(int), typeof(int), typeof(bool) })]
        internal static class AddItemAtPositionPatch
        {
            private static void Prefix(Inventory __instance, ItemDrop.ItemData __0, int __1, int __2, int __3)
            {
                ItemDrop.ItemData item = __0;
                int amount = __1;
                int x = __2;
                int y = __3;
                if (!FavoriteItemsPlugin.Enabled.Value || !FavoriteManager.IsFavorite(item) ||
                    __instance == null || item == null || item.m_shared == null || amount <= 0)
                    return;

                ItemDrop.ItemData target = __instance.GetItemAt(x, y);
                if (target != null && target.IsSameType(item) && target.m_shared != null &&
                    target.m_stack < target.m_shared.m_maxStackSize)
                {
                    FavoriteManager.SetFavorite(target, true);
                }
            }
        }
    }
}
