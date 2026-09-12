using System;
using System.Collections.Generic;
using BepInEx;
using BepInEx.Configuration;
using FavoriteItems.API;
using UnityEngine;

namespace FavoriteItems.ApiTestMod
{
    [BepInPlugin(PluginGuid, PluginName, PluginVersion)]
    [BepInProcess("valheim.exe")]
    [BepInDependency(FavoriteItemsPlugin.PluginGuid, BepInDependency.DependencyFlags.HardDependency)]
    public sealed class ApiTestPlugin : BaseUnityPlugin
    {
        public const string PluginGuid = "com.ronaldo.valheim.favoriteitems.apitest";
        public const string PluginName = "FavoriteItems API Test Mod";
        public const string PluginVersion = "1.0.0";

        private const string ReplacementProviderId = PluginGuid + ".replacement";
        private const string ThrowingProviderId = PluginGuid + ".throwing";

        private ConfigEntry<string> _mockProtectedSharedName;
        private bool _mockProviderRegistered;
        private int _favoriteChangedCount;

        private void Awake()
        {
            _mockProtectedSharedName = Config.Bind("Test", "MockProtectedSharedName", "$item_wood",
                "Shared item name protected by the mock provider while it is enabled.");

            FavoriteItemsApi.FavoriteChanged += OnFavoriteChanged;
            Logger.LogInfo("FavoriteItems public API detected. API version: " + FavoriteItemsApi.ApiVersion);
            RunNullContractChecks();
            Logger.LogInfo("Press F8 in game to run API checks against the first inventory item. Press F9 to toggle the mock protection provider.");
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.F8))
                RunInventoryChecks();

            if (Input.GetKeyDown(KeyCode.F9))
                ToggleMockProvider();
        }

        private void OnDestroy()
        {
            FavoriteItemsApi.FavoriteChanged -= OnFavoriteChanged;
            FavoriteItemsApi.UnregisterProtectionProvider(PluginGuid);
            FavoriteItemsApi.UnregisterProtectionProvider(ReplacementProviderId);
            FavoriteItemsApi.UnregisterProtectionProvider(ThrowingProviderId);
        }

        private void RunNullContractChecks()
        {
            bool resultingState;
            Check(!FavoriteItemsApi.IsFavorite(null), "IsFavorite(null) returns false");
            Check(!FavoriteItemsApi.ShouldPreventAutomaticMove(null), "ShouldPreventAutomaticMove(null) returns false");
            Check(!FavoriteItemsApi.TrySetFavorite(null, true), "TrySetFavorite(null) is rejected");
            Check(!FavoriteItemsApi.TryToggleFavorite(null, out resultingState) && !resultingState,
                "TryToggleFavorite(null) is rejected safely");
            Check(!FavoriteItemsApi.RegisterProtectionProvider(null, IsMockProtected),
                "A provider without an owner ID is rejected");
            Check(!FavoriteItemsApi.RegisterProtectionProvider(PluginGuid, null),
                "A null provider is rejected");
        }

        private void RunInventoryChecks()
        {
            Player player = Player.m_localPlayer;
            if (player == null || player.GetInventory() == null)
            {
                Logger.LogWarning("API checks require a loaded local player.");
                return;
            }

            List<ItemDrop.ItemData> items = player.GetInventory().GetAllItems();
            if (items == null || items.Count == 0)
            {
                Logger.LogWarning("API checks require at least one item in the player inventory.");
                return;
            }

            ItemDrop.ItemData item = items[0];
            bool original = FavoriteItemsApi.IsFavorite(item);

            if (!FavoriteItemsApi.IsEnabled)
            {
                bool disabledState;
                Check(!FavoriteItemsApi.TrySetFavorite(item, !original), "Mutations are rejected while FavoriteItems is disabled");
                Check(!FavoriteItemsApi.TryToggleFavorite(item, out disabledState), "Toggles are rejected while FavoriteItems is disabled");
                Check(FavoriteItemsApi.IsFavorite(item) == original, "Persisted favorite state remains readable while disabled");
                Check(!FavoriteItemsApi.ShouldPreventAutomaticMove(item), "Automatic movement is not blocked while disabled");
                return;
            }

            _favoriteChangedCount = 0;
            Check(FavoriteItemsApi.TrySetFavorite(item, original), "An idempotent favorite request is accepted");
            Check(_favoriteChangedCount == 0, "An idempotent request does not raise FavoriteChanged");

            bool toggledState;
            Check(FavoriteItemsApi.TryToggleFavorite(item, out toggledState), "TryToggleFavorite succeeds");
            Check(toggledState != original && FavoriteItemsApi.IsFavorite(item) == toggledState,
                "TryToggleFavorite reports and persists the resulting state");
            Check(_favoriteChangedCount == 1, "One real toggle raises FavoriteChanged exactly once");

            Check(FavoriteItemsApi.TrySetFavorite(item, original), "The original favorite state is restored");
            Check(_favoriteChangedCount == 2, "Restoring the state raises one additional event");

            bool stateBeforeProviderChecks = FavoriteItemsApi.IsFavorite(item);
            FavoriteItemsApi.TrySetFavorite(item, false);

            Check(FavoriteItemsApi.RegisterProtectionProvider(ReplacementProviderId, delegate { return false; }),
                "A protection provider can be registered");
            Check(FavoriteItemsApi.RegisterProtectionProvider(ReplacementProviderId, delegate { return true; }),
                "Registering the same owner replaces its provider");
            Check(FavoriteItemsApi.ShouldPreventAutomaticMove(item), "A replacement provider can protect the item");
            Check(FavoriteItemsApi.UnregisterProtectionProvider(ReplacementProviderId), "A provider can be unregistered");

            Check(FavoriteItemsApi.RegisterProtectionProvider(ThrowingProviderId,
                delegate { throw new InvalidOperationException("Intentional API test failure"); }),
                "The exception-isolation provider can be registered");

            bool exceptionEscaped = false;
            try
            {
                FavoriteItemsApi.ShouldPreventAutomaticMove(item);
            }
            catch
            {
                exceptionEscaped = true;
            }

            Check(!exceptionEscaped, "Provider exceptions do not escape the API");
            Check(FavoriteItemsApi.UnregisterProtectionProvider(ThrowingProviderId),
                "The exception-isolation provider can be removed");

            FavoriteItemsApi.TrySetFavorite(item, stateBeforeProviderChecks);
            Logger.LogInfo("FavoriteItems API inventory checks completed. Review PASS/FAIL entries above.");
        }

        private void ToggleMockProvider()
        {
            if (_mockProviderRegistered)
            {
                Check(FavoriteItemsApi.UnregisterProtectionProvider(PluginGuid), "Mock protection provider disabled");
                _mockProviderRegistered = false;
                return;
            }

            _mockProviderRegistered = FavoriteItemsApi.RegisterProtectionProvider(PluginGuid, IsMockProtected);
            Check(_mockProviderRegistered,
                "Mock protection provider enabled for shared name " + _mockProtectedSharedName.Value);
        }

        private bool IsMockProtected(ItemDrop.ItemData item)
        {
            return item != null && item.m_shared != null &&
                   string.Equals(item.m_shared.m_name, _mockProtectedSharedName.Value, StringComparison.Ordinal);
        }

        private void OnFavoriteChanged(ItemDrop.ItemData item, bool favorite)
        {
            ++_favoriteChangedCount;
            Logger.LogInfo("FavoriteChanged received: " + favorite);
        }

        private void Check(bool condition, string description)
        {
            if (condition)
                Logger.LogInfo("PASS: " + description);
            else
                Logger.LogError("FAIL: " + description);
        }
    }
}
