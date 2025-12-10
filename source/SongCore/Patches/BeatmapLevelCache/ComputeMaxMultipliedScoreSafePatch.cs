using HarmonyLib;

namespace SongCore.Patches.BeatmapLevelCache
{
    /// <summary>
    /// This fixes a potential race condition with <see cref="ScoreModel.ComputeMaxMultipliedScoreForBeatmap"/> when called concurrently.
    /// </summary>
    [HarmonyPatch(typeof(ScoreModel), nameof(ScoreModel.ComputeMaxMultipliedScoreForBeatmap))]
    internal static class ComputeMaxMultipliedScoreSafelyPatch
    {
        private static readonly object _lock = new();

        private static bool Prefix(ref int __result, IReadonlyBeatmapData beatmapData)
        {
            lock (_lock)
            {
                __result = ReversePatches.ScoreModel.ComputeMaxMultipliedScoreForBeatmap(beatmapData);
            }

            return false;
        }
    }
}
