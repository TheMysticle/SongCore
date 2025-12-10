using System;
using System.Threading.Tasks;
using HarmonyLib;

namespace SongCore.Patches.BeatmapLevelCache
{
    internal static class ReversePatches
    {
        [HarmonyPatch(typeof(global::FileSystemBeatmapLevelData))]
        public static class FileSystemBeatmapLevelData
        {
            [HarmonyPatch(nameof(global::FileSystemBeatmapLevelData.GetBeatmapStringAsync))]
            [HarmonyReversePatch]
            public static Task<string?> GetBeatmapStringAsync(global::FileSystemBeatmapLevelData instance, in BeatmapKey beatmapKey)
            {
                throw new NotImplementedException();
            }

            [HarmonyPatch(nameof(global::FileSystemBeatmapLevelData.GetLightshowStringAsync))]
            [HarmonyReversePatch]
            public static Task<string?> GetLightshowStringAsync(global::FileSystemBeatmapLevelData instance, in BeatmapKey beatmapKey)
            {
                throw new NotImplementedException();
            }

            [HarmonyPatch(nameof(global::FileSystemBeatmapLevelData.GetAudioDataStringAsync))]
            [HarmonyReversePatch]
            public static Task<string?> GetAudioDataStringAsync(global::FileSystemBeatmapLevelData instance)
            {
                throw new NotImplementedException();
            }

            [HarmonyPatch(nameof(global::FileSystemBeatmapLevelData.GetBeatmapString))]
            [HarmonyReversePatch]
            public static string? GetBeatmapString(global::FileSystemBeatmapLevelData instance, in BeatmapKey beatmapKey)
            {
                throw new NotImplementedException();
            }

            [HarmonyPatch(nameof(global::FileSystemBeatmapLevelData.GetLightshowString))]
            [HarmonyReversePatch]
            public static string? GetLightshowString(global::FileSystemBeatmapLevelData instance, in BeatmapKey beatmapKey)
            {
                throw new NotImplementedException();
            }

            [HarmonyPatch(nameof(global::FileSystemBeatmapLevelData.GetAudioDataString))]
            [HarmonyReversePatch]
            public static string? GetAudioDataString(global::FileSystemBeatmapLevelData instance)
            {
                throw new NotImplementedException();
            }
        }

        [HarmonyPatch(typeof(global::BeatmapLevelDataSO))]
        public static class BeatmapLevelDataSO
        {
            [HarmonyPatch(nameof(global::BeatmapLevelDataSO.GetBeatmapStringAsync))]
            [HarmonyReversePatch]
            public static Task<string?> GetBeatmapStringAsync(global::BeatmapLevelDataSO instance, in BeatmapKey beatmapKey)
            {
                throw new NotImplementedException();
            }

            [HarmonyPatch(nameof(global::BeatmapLevelDataSO.GetLightshowStringAsync))]
            [HarmonyReversePatch]
            public static Task<string?> GetLightshowStringAsync(global::BeatmapLevelDataSO instance, in BeatmapKey beatmapKey)
            {
                throw new NotImplementedException();
            }

            [HarmonyPatch(nameof(global::BeatmapLevelDataSO.GetAudioDataStringAsync))]
            [HarmonyReversePatch]
            public static Task<string?> GetAudioDataStringAsync(global::BeatmapLevelDataSO instance)
            {
                throw new NotImplementedException();
            }

            [HarmonyPatch(nameof(global::BeatmapLevelDataSO.GetBeatmapString))]
            [HarmonyReversePatch]
            public static string? GetBeatmapString(global::BeatmapLevelDataSO instance, in BeatmapKey beatmapKey)
            {
                throw new NotImplementedException();
            }

            [HarmonyPatch(nameof(global::BeatmapLevelDataSO.GetLightshowString))]
            [HarmonyReversePatch]
            public static string? GetLightshowString(global::BeatmapLevelDataSO instance, in BeatmapKey beatmapKey)
            {
                throw new NotImplementedException();
            }

            [HarmonyPatch(nameof(global::BeatmapLevelDataSO.GetAudioDataString))]
            [HarmonyReversePatch]
            public static string? GetAudioDataString(global::BeatmapLevelDataSO instance)
            {
                throw new NotImplementedException();
            }
        }

        [HarmonyPatch(typeof(global::BeatmapDataLoader))]
        public static class BeatmapDataLoader
        {
            [HarmonyPatch(nameof(global::BeatmapDataLoader.LoadBeatmapDataAsync))]
            [HarmonyReversePatch]
            public static Task<IReadonlyBeatmapData?> LoadBeatmapDataAsync(global::BeatmapDataLoader instance, IBeatmapLevelData beatmapLevelData, BeatmapKey beatmapKey, float startBpm, bool loadingForDesignatedEnvironment, IEnvironmentInfo? targetEnvironmentInfo, IEnvironmentInfo? originalEnvironmentInfo, BeatmapLevelDataVersion beatmapLevelDataVersion, GameplayModifiers? gameplayModifiers, PlayerSpecificSettings? playerSpecificSettings, bool enableBeatmapDataCaching)
            {
                throw new NotImplementedException();
            }
        }

        [HarmonyPatch(typeof(global::ScoreModel))]
        public static class ScoreModel
        {
            [HarmonyPatch(nameof(global::ScoreModel.ComputeMaxMultipliedScoreForBeatmap))]
            [HarmonyReversePatch]
            public static int ComputeMaxMultipliedScoreForBeatmap(IReadonlyBeatmapData beatmapData)
            {
                throw new NotImplementedException();
            }
        }
    }
}
