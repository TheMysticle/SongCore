using System.Collections.Concurrent;
using System.IO;
using System.Threading;
using HarmonyLib;
using ModestTree;
using Zenject;

namespace SongCore.Patches.BeatmapLevelCache
{
    /// <summary>
    /// This forces consumers to use cached data when reading blacklisted files.
    /// </summary>
    [HarmonyPatch(typeof(FileStream))]
    internal static class IOBlacklistPatch
    {
        public static ConcurrentDictionary<string, string> FilesBlacklist { get; } = new();
        public static AsyncLocal<bool> AllowIO { get; } = new();

        [HarmonyPatch(nameof(FileStream.Read))]
        [HarmonyPatch(nameof(FileStream.ReadAsync))]
        private static void Prefix(FileStream __instance)
        {
            try
            {
                var filePath = Path.GetFullPath(__instance.Name);
                Assert.That(!FilesBlacklist.TryGetValue(filePath, out var reason) || AllowIO.Value, $"File '{filePath}' is blacklisted. Reason: {reason}");
            }
            catch (ZenjectException ex)
            {
                // Ensure we don't get silenced by try-catch blocks.
                Plugin.Log.Error(ex.ToString());
                throw;
            }
        }
    }
}
