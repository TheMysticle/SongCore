using System.Threading.Tasks;
using SiraUtil.Affinity;

namespace SongCore.Patches.BeatmapLevelCache
{
    /// <summary>
    /// These patches are caching beatmap JSON to avoid doing unnecessary I/O.
    /// </summary>
    internal class BeatmapJsonCachePatches : IAffinity
    {
        private readonly BeatmapLevelCache _beatmapLevelCache;

        private BeatmapJsonCachePatches(BeatmapLevelCache beatmapLevelCache)
        {
            _beatmapLevelCache = beatmapLevelCache;
        }

        [AffinityPatch(typeof(FileSystemBeatmapLevelData), nameof(FileSystemBeatmapLevelData.GetBeatmapStringAsync))]
        [AffinityPatch(typeof(BeatmapLevelDataSO), nameof(BeatmapLevelDataSO.GetBeatmapStringAsync))]
        [AffinityPrefix]
        private bool GetBeatmapDataJson(IBeatmapLevelData __instance, ref Task<string?> __result, BeatmapKey beatmapKey)
        {
            __result = _beatmapLevelCache.GetJsonDataAsync(__instance, beatmapKey, BeatmapDataType.Beatmap, () =>
            {
                return __instance is FileSystemBeatmapLevelData fileSystemBeatmapLevelData
                    ? ReversePatches.FileSystemBeatmapLevelData.GetBeatmapStringAsync(fileSystemBeatmapLevelData, beatmapKey)
                    : ReversePatches.BeatmapLevelDataSO.GetBeatmapStringAsync((BeatmapLevelDataSO)__instance, beatmapKey);
            });

            return false;
        }

        [AffinityPatch(typeof(FileSystemBeatmapLevelData), nameof(FileSystemBeatmapLevelData.GetLightshowStringAsync))]
        [AffinityPatch(typeof(BeatmapLevelDataSO), nameof(BeatmapLevelDataSO.GetLightshowStringAsync))]
        [AffinityPrefix]
        private bool GetLightshowDataJson(IBeatmapLevelData __instance, ref Task<string?> __result, BeatmapKey beatmapKey)
        {
            __result = _beatmapLevelCache.GetJsonDataAsync(__instance, beatmapKey, BeatmapDataType.Lightshow, () =>
            {
                return __instance is FileSystemBeatmapLevelData fileSystemBeatmapLevelData
                    ? ReversePatches.FileSystemBeatmapLevelData.GetLightshowStringAsync(fileSystemBeatmapLevelData, beatmapKey)
                    : ReversePatches.BeatmapLevelDataSO.GetLightshowStringAsync((BeatmapLevelDataSO)__instance, beatmapKey);
            });

            return false;
        }

        [AffinityPatch(typeof(FileSystemBeatmapLevelData), nameof(FileSystemBeatmapLevelData.GetAudioDataStringAsync))]
        [AffinityPatch(typeof(BeatmapLevelDataSO), nameof(BeatmapLevelDataSO.GetAudioDataStringAsync))]
        [AffinityPrefix]
        private bool GetAudioDataJson(IBeatmapLevelData __instance, ref Task<string?> __result)
        {
            __result = _beatmapLevelCache.GetJsonDataAsync(__instance, new BeatmapKey(), BeatmapDataType.Audio, () =>
            {
                return __instance is FileSystemBeatmapLevelData fileSystemBeatmapLevelData
                    ? ReversePatches.FileSystemBeatmapLevelData.GetAudioDataStringAsync(fileSystemBeatmapLevelData)
                    : ReversePatches.BeatmapLevelDataSO.GetAudioDataStringAsync((BeatmapLevelDataSO)__instance);
            });

            return false;
        }

        [AffinityPatch(typeof(FileSystemBeatmapLevelData), nameof(FileSystemBeatmapLevelData.GetBeatmapString))]
        [AffinityPatch(typeof(BeatmapLevelDataSO), nameof(BeatmapLevelDataSO.GetBeatmapString))]
        [AffinityPrefix]
        private bool GetBeatmapDataJson(IBeatmapLevelData __instance, ref string? __result, BeatmapKey beatmapKey)
        {
            __result = _beatmapLevelCache.GetJsonData(__instance, beatmapKey, BeatmapDataType.Beatmap, () =>
            {
                return __instance is FileSystemBeatmapLevelData fileSystemBeatmapLevelData
                    ? ReversePatches.FileSystemBeatmapLevelData.GetBeatmapString(fileSystemBeatmapLevelData, beatmapKey)
                    : ReversePatches.BeatmapLevelDataSO.GetBeatmapString((BeatmapLevelDataSO)__instance, beatmapKey);
            });

            return false;
        }

        [AffinityPatch(typeof(FileSystemBeatmapLevelData), nameof(FileSystemBeatmapLevelData.GetLightshowString))]
        [AffinityPatch(typeof(BeatmapLevelDataSO), nameof(BeatmapLevelDataSO.GetLightshowString))]
        [AffinityPrefix]
        private bool GetLightshowDataJson(IBeatmapLevelData __instance, ref string? __result, BeatmapKey beatmapKey)
        {
            __result = _beatmapLevelCache.GetJsonData(__instance, beatmapKey, BeatmapDataType.Lightshow, () =>
            {
                return __instance is FileSystemBeatmapLevelData fileSystemBeatmapLevelData
                    ? ReversePatches.FileSystemBeatmapLevelData.GetLightshowString(fileSystemBeatmapLevelData, beatmapKey)
                    : ReversePatches.BeatmapLevelDataSO.GetLightshowString((BeatmapLevelDataSO)__instance, beatmapKey);
            });

            return false;
        }

        [AffinityPatch(typeof(FileSystemBeatmapLevelData), nameof(FileSystemBeatmapLevelData.GetAudioDataString))]
        [AffinityPatch(typeof(BeatmapLevelDataSO), nameof(BeatmapLevelDataSO.GetAudioDataString))]
        [AffinityPrefix]
        private bool GetAudioDataJson(IBeatmapLevelData __instance, ref string? __result)
        {
            __result = _beatmapLevelCache.GetJsonData(__instance, new BeatmapKey(), BeatmapDataType.Audio, () =>
            {
                return __instance is FileSystemBeatmapLevelData fileSystemBeatmapLevelData
                    ? ReversePatches.FileSystemBeatmapLevelData.GetAudioDataString(fileSystemBeatmapLevelData)
                    : ReversePatches.BeatmapLevelDataSO.GetAudioDataString((BeatmapLevelDataSO)__instance);
            });

            return false;
        }
    }
}
