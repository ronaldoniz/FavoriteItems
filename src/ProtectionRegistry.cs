using System;
using System.Collections.Generic;

namespace FavoriteItems
{
    internal static class ProtectionRegistry
    {
        private sealed class ProviderEntry
        {
            internal string OwnerId;
            internal Func<ItemDrop.ItemData, bool> Provider;
        }

        private static readonly object SyncRoot = new object();
        private static readonly Dictionary<string, Func<ItemDrop.ItemData, bool>> Providers =
            new Dictionary<string, Func<ItemDrop.ItemData, bool>>(StringComparer.Ordinal);
        private static readonly HashSet<string> WarnedProviders =
            new HashSet<string>(StringComparer.Ordinal);

        internal static bool Register(string ownerId, Func<ItemDrop.ItemData, bool> provider)
        {
            if (string.IsNullOrWhiteSpace(ownerId) || provider == null)
                return false;

            lock (SyncRoot)
            {
                Providers[ownerId] = provider;
                WarnedProviders.Remove(ownerId);
            }

            return true;
        }

        internal static bool Unregister(string ownerId)
        {
            if (string.IsNullOrWhiteSpace(ownerId))
                return false;

            lock (SyncRoot)
            {
                WarnedProviders.Remove(ownerId);
                return Providers.Remove(ownerId);
            }
        }

        internal static bool IsProtected(ItemDrop.ItemData item)
        {
            if (item == null)
                return false;

            ProviderEntry[] snapshot;
            lock (SyncRoot)
            {
                snapshot = new ProviderEntry[Providers.Count];
                int index = 0;
                foreach (KeyValuePair<string, Func<ItemDrop.ItemData, bool>> pair in Providers)
                {
                    ProviderEntry entry = new ProviderEntry();
                    entry.OwnerId = pair.Key;
                    entry.Provider = pair.Value;
                    snapshot[index++] = entry;
                }
            }

            for (int i = 0; i < snapshot.Length; ++i)
            {
                try
                {
                    if (snapshot[i].Provider(item))
                        return true;
                }
                catch (Exception ex)
                {
                    bool shouldLog;
                    lock (SyncRoot)
                        shouldLog = WarnedProviders.Add(snapshot[i].OwnerId);

                    if (shouldLog)
                        LogWarning("Protection provider '" + snapshot[i].OwnerId + "' failed: " + ex.Message);
                }
            }

            return false;
        }

        internal static void Clear()
        {
            lock (SyncRoot)
            {
                Providers.Clear();
                WarnedProviders.Clear();
            }
        }

        private static void LogWarning(string message)
        {
            if (FavoriteItemsPlugin.Log != null)
                FavoriteItemsPlugin.Log.LogWarning(message);
        }
    }
}
