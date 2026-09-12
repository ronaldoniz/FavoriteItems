using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using FavoriteItems.API;
using HarmonyLib;

namespace FavoriteItems
{
    [BepInPlugin(PluginGuid, PluginName, PluginVersion)]
    [BepInProcess("valheim.exe")]
    [BepInDependency(GorilaChestIntegration.PluginGuid, BepInDependency.DependencyFlags.SoftDependency)]
    [BepInDependency(EquipmentAndQuickSlotsIntegration.PluginGuid, BepInDependency.DependencyFlags.SoftDependency)]
    public sealed class FavoriteItemsPlugin : BaseUnityPlugin
    {
        public const string PluginGuid = "com.ronaldo.valheim.favoriteitems";
        public const string PluginName = "FavoriteItems";
        public const string PluginVersion = "1.1.0";

        internal static ManualLogSource Log;
        internal static ConfigEntry<bool> Enabled;
        internal static ConfigEntry<bool> ShowMarker;
        internal static ConfigEntry<bool> ShowMessages;
        internal static ConfigEntry<bool> ProtectEquipmentAndQuickSlots;

        private Harmony _harmony;

        private void Awake()
        {
            Log = Logger;
            Enabled = Config.Bind("General", "Enabled", true,
                "Enables Alt-click favorite toggling, the visual marker, and quick-stack protection.");
            ShowMarker = Config.Bind("Visual", "ShowMarker", true,
                "Shows a subtle star in the corner of favorite stacks.");
            ShowMessages = Config.Bind("Visual", "ShowMessages", true,
                "Shows a short message when an item is favorited or unfavorited.");
            ProtectEquipmentAndQuickSlots = Config.Bind("Compatibility", "ProtectEquipmentAndQuickSlots", false,
                "Automatically protects items in EquipmentAndQuickSlots special slots, even when they are not favorited.");

            _harmony = new Harmony(PluginGuid);
            _harmony.PatchAll(typeof(FavoriteItemsPlugin).Assembly);

            EquipmentAndQuickSlotsIntegration.Initialize();
            GorilaChestIntegration.Initialize(_harmony);

            Log.LogInfo(PluginName + " " + PluginVersion + " loaded. Use Alt+left-click to toggle favorites.");
        }

        private void OnDestroy()
        {
            EquipmentAndQuickSlotsIntegration.Shutdown();
            ProtectionRegistry.Clear();
            FavoriteItemsApi.Shutdown();
            FavoriteVisualPatches.Cleanup();
            if (_harmony != null)
                _harmony.UnpatchSelf();
        }
    }
}
