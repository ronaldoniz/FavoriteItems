using System.Collections.Generic;
using HarmonyLib;
using TMPro;
using UnityEngine;

namespace FavoriteItems
{
    [HarmonyPatch(typeof(InventoryGrid), "UpdateGui")]
    internal static class FavoriteVisualPatches
    {
        private const string MarkerName = "FavoriteItems_Star";
        private static readonly List<GameObject> CreatedMarkers = new List<GameObject>();

        [HarmonyPriority(Priority.Last)]
        private static void Postfix(InventoryGrid __instance, List<InventoryElement> ___m_elements)
        {
            if (___m_elements == null)
                return;

            Inventory inventory = __instance.GetInventory();
            Player player = Player.m_localPlayer;
            if (player == null || inventory != player.GetInventory())
                return;

            for (int i = 0; i < ___m_elements.Count; ++i)
            {
                InventoryElement element = ___m_elements[i];
                if (element == null)
                    continue;

                Transform existing = element.transform.Find(MarkerName);
                if (existing != null)
                    existing.gameObject.SetActive(false);
            }

            if (!FavoriteItemsPlugin.Enabled.Value || !FavoriteItemsPlugin.ShowMarker.Value)
                return;

            int width = inventory.GetWidth();
            List<ItemDrop.ItemData> items = inventory.GetAllItems();
            for (int i = 0; i < items.Count; ++i)
            {
                ItemDrop.ItemData item = items[i];
                if (!FavoriteManager.IsFavorite(item))
                    continue;

                int index = item.m_gridPos.y * width + item.m_gridPos.x;
                if (index < 0 || index >= ___m_elements.Count)
                    continue;

                InventoryElement element = ___m_elements[index];
                if (element == null)
                    continue;

                GameObject marker = EnsureMarker(element);
                if (marker != null)
                    marker.SetActive(true);
            }
        }

        private static GameObject EnsureMarker(InventoryElement element)
        {
            Transform existing = element.transform.Find(MarkerName);
            if (existing != null)
                return existing.gameObject;

            GameObject marker = new GameObject(MarkerName, typeof(RectTransform));
            marker.transform.SetParent(element.transform, false);
            marker.transform.SetAsLastSibling();

            RectTransform rect = marker.GetComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;

            TextMeshProUGUI text = marker.AddComponent<TextMeshProUGUI>();
            text.text = "★";
            text.fontSize = 14f;
            text.fontStyle = FontStyles.Bold;
            text.alignment = TextAlignmentOptions.TopRight;
            text.margin = new Vector4(0f, 1f, 2f, 0f);
            text.color = new Color(1f, 0.78f, 0.15f, 0.95f);
            text.raycastTarget = false;

            if (element.m_amount != null)
            {
                text.font = element.m_amount.font;
                text.fontSharedMaterial = element.m_amount.fontSharedMaterial;
            }

            CreatedMarkers.Add(marker);
            return marker;
        }

        internal static void Cleanup()
        {
            for (int i = 0; i < CreatedMarkers.Count; ++i)
            {
                if (CreatedMarkers[i] != null)
                    Object.Destroy(CreatedMarkers[i]);
            }
            CreatedMarkers.Clear();
        }
    }
}
