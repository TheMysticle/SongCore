using System.Threading.Tasks;
using BeatSaber.Destinations;
using ModestTree;
using UnityEngine.SceneManagement;

namespace SongCore.Patches.BeatmapLevelCache
{
    internal record BeatmapDataRequest(IBeatmapLevelData BeatmapLevelData, BeatmapKey BeatmapKey, float StartBpm, bool LoadingForDesignatedEnvironment, IEnvironmentInfo? TargetEnvironmentInfo, IEnvironmentInfo? OriginalEnvironmentInfo, BeatmapLevelDataVersion BeatmapLevelDataVersion, GameplayModifiers? GameplayModifiers, PlayerSpecificSettings? PlayerSpecificSettings, bool EnableBeatmapDataCaching)
    {
        public Task<IReadonlyBeatmapData?> Start(BeatmapDataLoader beatmapDataLoader)
        {
            Assert.That(SceneManager.GetActiveScene().name != SceneNames.kGameCoreSceneName, "Beatmap data should not be loaded in the game scene, as garbage collection is disabled.");

            return ReversePatches.BeatmapDataLoader.LoadBeatmapDataAsync(beatmapDataLoader, BeatmapLevelData, BeatmapKey, StartBpm, LoadingForDesignatedEnvironment, TargetEnvironmentInfo, OriginalEnvironmentInfo, BeatmapLevelDataVersion, GameplayModifiers, PlayerSpecificSettings, EnableBeatmapDataCaching);
        }
    }
}
