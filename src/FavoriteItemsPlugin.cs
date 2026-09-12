using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
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
        public const string PluginVersion = "1.0.1";

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
                "Ativa o gesto Alt+clique, o indicador e a protecao no quick stack.");
            ShowMarker = Config.Bind("Visual", "ShowMarker", true,
                "Mostra uma estrela discreta no canto dos stacks favoritos.");
            ShowMessages = Config.Bind("Visual", "ShowMessages", true,
                "Mostra uma mensagem curta ao favoritar ou desfavoritar.");
            ProtectEquipmentAndQuickSlots = Config.Bind("Compatibility", "ProtectEquipmentAndQuickSlots", true,
                "Impede que o GorilaChestMod mova itens em qualquer slot especial do EquipmentAndQuickSlots.");

            _harmony = new Harmony(PluginGuid);
            _harmony.PatchAll(typeof(FavoriteItemsPlugin).Assembly);

            EquipmentAndQuickSlotsIntegration.Initialize();
            GorilaChestIntegration.Initialize(_harmony);

            Log.LogInfo(PluginName + " " + PluginVersion + " carregado. Use Alt+clique esquerdo para favoritar.");
        }

        private void OnDestroy()
        {
            FavoriteVisualPatches.Cleanup();
            if (_harmony != null)
                _harmony.UnpatchSelf();
        }
    }
}
