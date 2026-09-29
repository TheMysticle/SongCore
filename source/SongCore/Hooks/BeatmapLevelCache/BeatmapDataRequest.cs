using System;
using System.Threading.Tasks;
using BeatSaber.Destinations;
using ModestTree;
using UnityEngine.SceneManagement;

namespace SongCore.Hooks.BeatmapLevelCache
{
    internal record BeatmapDataRequest(IBeatmapLevelData BeatmapLevelData, BeatmapKey BeatmapKey, float StartBpm, bool LoadingForDesignatedEnvironment, IEnvironmentInfo? TargetEnvironmentInfo, IEnvironmentInfo? OriginalEnvironmentInfo, BeatmapLevelDataVersion BeatmapLevelDataVersion, GameplayModifiers? GameplayModifiers, PlayerSpecificSettings? PlayerSpecificSettings)
    {
        // BeatmapDataLoader.LoadBeatmapDataAsync takes exactly these 9 parameters (confirmed by
        // decompiling DataModels.dll) -- there's no "enableBeatmapDataCaching" bool on the real
        // method. An earlier version of this file had one anyway, which made the MonoMod Hook's
        // delegate signature not match the real method at all, so the Hook's constructor threw
        // "Target method is not compatible with source method" and this whole class silently
        // never initialized (confirmed via a real-hardware log).
        public Task<IReadonlyBeatmapData?> Start(Func<BeatmapDataLoader, IBeatmapLevelData, BeatmapKey, float, bool, IEnvironmentInfo?, IEnvironmentInfo?, BeatmapLevelDataVersion, GameplayModifiers?, PlayerSpecificSettings?, Task<IReadonlyBeatmapData?>> original, BeatmapDataLoader beatmapDataLoader)
        {
            Assert.That(SceneManager.GetActiveScene().name != SceneNames.kGameCoreSceneName, "Beatmap data should not be loaded in the game scene, as garbage collection is disabled.");

            return original(beatmapDataLoader, BeatmapLevelData, BeatmapKey, StartBpm, LoadingForDesignatedEnvironment, TargetEnvironmentInfo, OriginalEnvironmentInfo, BeatmapLevelDataVersion, GameplayModifiers, PlayerSpecificSettings);
        }
    }
}
