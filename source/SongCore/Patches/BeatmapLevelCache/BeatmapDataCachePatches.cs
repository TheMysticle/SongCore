using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using IPA.Utilities;
using ModestTree;
using SiraUtil.Affinity;
using SongCore.Utilities;

namespace SongCore.Patches.BeatmapLevelCache
{
    /// <summary>
    /// These patches implement a way to cache beatmap data when selecting levels for later use by the game or mods.
    /// The execution flow is as follows:
    /// <list>
    ///   <item><see cref="LevelCollectionTableView.didSelectLevelEvent"/> fires.</item>
    ///   <item><see cref="HandleDidSelectLevel(LevelCollectionTableView, BeatmapLevel)"/> invokes base game handlers.</item>
    ///   <item>Base game handler fires <see cref="LevelCollectionViewController.didSelectLevelEvent"/>.</item>
    ///   <item><see cref="HandleDidSelectLevel(LevelCollectionViewController, BeatmapLevel)"/> invokes base game handlers.</item>
    ///   <item>Base game handler fires <see cref="StandardLevelDetailViewController.didChangeContentEvent"/>.</item>
    ///   <item><see cref="HandleDidChangeContent"/> invokes base game handlers. <see cref="StandardLevelDetailView.CreateBeatmapKey"/> is called here.</item>
    ///   <item><see cref="HandleDidChangeContent"/> waits for <see cref="InitializeBeatmapLevelCacheAsync"/> to be done.</item>
    ///   <item><see cref="HandleDidChangeContent"/> waits for <see cref="SetBeatmapLevelCacheBeatmapKeyAsync"/> to be done.</item>
    ///   <item><see cref="HandleDidChangeContent"/> invokes mod handlers.</item>
    ///   <item><see cref="HandleDidSelectLevel(LevelCollectionViewController, BeatmapLevel)"/> invokes mod handlers.</item>
    ///   <item><see cref="HandleDidSelectLevel(LevelCollectionTableView, BeatmapLevel)"/> invokes mod handlers.</item>
    /// </list>
    /// Ultimately, it ensures the cache is ready and prevents race conditions.
    /// </summary>
    internal class BeatmapDataCachePatches : IAffinity
    {
        private readonly CustomLevelLoader _customLevelLoader;
        private readonly BeatmapLevelsModel _beatmapLevelsModel;
        private readonly BeatmapLevelsEntitlementModel _beatmapLevelsEntitlementModel;
        private readonly BeatmapDataLoader _beatmapDataLoader;
        private readonly BeatmapLevelCache _beatmapLevelCache;
        private readonly EventProxyPatches _eventProxyPatches;

        private CancellationToken _cancellationToken;

        private BeatmapDataCachePatches(CustomLevelLoader customLevelLoader, BeatmapLevelsModel beatmapLevelsModel, BeatmapLevelsEntitlementModel beatmapLevelsEntitlementModel, BeatmapDataLoader beatmapDataLoader, BeatmapLevelCache beatmapLevelCache, EventProxyPatches eventProxyPatches)
        {
            _customLevelLoader = customLevelLoader;
            _beatmapLevelsModel = beatmapLevelsModel;
            _beatmapLevelsEntitlementModel = beatmapLevelsEntitlementModel;
            _beatmapDataLoader = beatmapDataLoader;
            _beatmapLevelCache = beatmapLevelCache;
            _eventProxyPatches = eventProxyPatches;
        }

        private async Task<IBeatmapLevelData?> InitializeBeatmapLevelCacheAsync(BeatmapLevel beatmapLevel, CancellationToken cancellationToken)
        {
            try
            {
                var beatmapLevelDataVersion = await _beatmapLevelsEntitlementModel.GetLevelDataVersionAsync(beatmapLevel.levelID, cancellationToken);
                var beatmapLevelDataResult = await _beatmapLevelsModel.LoadBeatmapLevelDataAsync(beatmapLevel.levelID, beatmapLevelDataVersion, cancellationToken);

                if (beatmapLevelDataResult.isError)
                {
                    Plugin.Log.Error($"Failed to load beatmap level data for {beatmapLevel.levelID}: {beatmapLevelDataResult.errorMessage}");

                    return null;
                }

                cancellationToken.ThrowIfCancellationRequested();

                Plugin.Log.Debug($"Initialized beatmap level cache for {beatmapLevel.levelID}");

                return _beatmapLevelCache.BeatmapLevelData = beatmapLevelDataResult.beatmapLevelData;
            }
            catch (OperationCanceledException)
            {
                Plugin.Log.Debug($"Beatmap level cache initialization was cancelled for {beatmapLevel.levelID}");

                return null;
            }
            catch (Exception ex)
            {
                Plugin.Log.Error($"An error occurred during beatmap level cache initialization.\n{ex}");

                return null;
            }
        }

        private async Task InvokeEventAsync<TSender, TArgs>(TSender sender, TArgs? eventArgs, Delegate originalDelegate, CancellationToken cancellationToken)
        {
            var baseHandlers = new List<Action<TSender, TArgs?>>();
            var modHandlers = new List<Action<TSender, TArgs?>>();

            foreach (var handler in originalDelegate.GetInvocationList().Cast<Action<TSender, TArgs?>>())
            {
                if (Directory.GetParent(handler.Method.DeclaringType!.Assembly.Location)!.Name == "Managed")
                {
                    baseHandlers.Add(handler);
                }
                else
                {
                    modHandlers.Add(handler);
                }
            }

            foreach (var action in baseHandlers)
            {
                action(sender, eventArgs);
            }

            await _beatmapLevelCache.WaitUntilReadyAsync(cancellationToken);

            foreach (var action in modHandlers)
            {
                action(sender, eventArgs);
            }
        }

        private async void HandleDidSelectLevel(LevelCollectionTableView levelCollectionTableView, BeatmapLevel beatmapLevel)
        {
            Plugin.Log.Trace("LevelCollectionTableView.didSelectLevelEvent");

            var message = $"Selected level: {beatmapLevel.levelID} | {beatmapLevel.songName}";

            if (!beatmapLevel.hasPrecalculatedData)
            {
                message += " | v" + BeatmapSaveDataHelpers.GetVersion(_customLevelLoader._loadedBeatmapSaveData[beatmapLevel.levelID].customLevelFolderInfo.levelInfoJsonString);
            }

            Plugin.Log.Debug(message);

            _beatmapLevelCache.Init(beatmapLevel, InitializeBeatmapLevelCacheAsync);
            _cancellationToken = _beatmapLevelCache.CancellationTokenSource!.Token;

            await InvokeEventAsync(levelCollectionTableView, beatmapLevel, _eventProxyPatches.LevelCollectionTableViewDidSelectLevelDelegate!, _cancellationToken);
        }

        private async void HandleDidSelectLevel(LevelCollectionViewController levelCollectionViewController, BeatmapLevel beatmapLevel)
        {
            Plugin.Log.Trace("LevelCollectionViewController.didSelectLevelEvent");

            Assert.That(beatmapLevel == _beatmapLevelCache.BeatmapLevel);

            await InvokeEventAsync(levelCollectionViewController, beatmapLevel, _eventProxyPatches.LevelCollectionViewControllerDidSelectLevelDelegate!, _cancellationToken);
        }

        private async void HandleDidChangeContent(StandardLevelDetailViewController? standardLevelDetailViewController, StandardLevelDetailViewController.ContentType contentType)
        {
            Plugin.Log.Trace($"StandardLevelDetailViewController.didChangeContentEvent {contentType}");

            if (standardLevelDetailViewController != null)
            {
                Assert.That(standardLevelDetailViewController.beatmapLevel == _beatmapLevelCache.BeatmapLevel);
            }

            await InvokeEventAsync(standardLevelDetailViewController, contentType, _eventProxyPatches.StandardLevelDetailViewControllerDidChangeContentDelegate!, _cancellationToken);
        }

        [AffinityPatch(typeof(BeatmapLevelDataUtils), nameof(BeatmapLevelDataUtils.ReadAllTextFromPathAsync))]
        private void LogReadAllTextFromPathAsync(string path)
        {
            Plugin.Log.Debug($"ReadAllTextFromPathAsync {_beatmapLevelCache.BeatmapKey.ToString()} {path}");
        }

        [AffinityPatch(typeof(LevelCollectionViewController), nameof(LevelCollectionViewController.DidActivate))]
        [AffinityPrefix]
        private void ReplaceDidSelectLevelEvent(LevelCollectionViewController __instance, bool firstActivation)
        {
            if (!firstActivation)
            {
                return;
            }

            ref var didSelectLevelViewControllerEvent = ref Accessors.ViewControllerDidSelectLevelEventAccessor(ref __instance);
            didSelectLevelViewControllerEvent = HandleDidSelectLevel;

            ref var didSelectLevelViewEvent = ref Accessors.TableViewDidSelectLevelEventAccessor(ref __instance._levelCollectionTableView);
            didSelectLevelViewEvent = HandleDidSelectLevel;
        }

        [AffinityPatch(typeof(StandardLevelDetailViewController), nameof(StandardLevelDetailViewController.DidActivate))]
        [AffinityPrefix]
        private void ReplaceDidChangeContentEvent(StandardLevelDetailViewController __instance, bool firstActivation)
        {
            if (!firstActivation)
            {
                return;
            }

            ref var didChangeContentEvent = ref Accessors.DidChangeContentEventAccessor(ref __instance);
            didChangeContentEvent = HandleDidChangeContent;
        }

        [AffinityPatch(typeof(StandardLevelDetailView), nameof(StandardLevelDetailView.CreateBeatmapKey))]
        private async void SetBeatmapLevelCacheBeatmapKeyAsync(BeatmapKey __result)
        {
            Plugin.Log.Debug("Attempting to set beatmap level cache beatmap key");

            var tcs = _beatmapLevelCache.BeatmapKeyTaskCompletionSource!;
            var beatmapLevelData = await _beatmapLevelCache.BeatmapLevelDataLoadingTask!;

            // Task errored, faulted or canceled.
            if (beatmapLevelData == null)
            {
                tcs.SetResult(false);
                return;
            }

            Assert.That(beatmapLevelData == _beatmapLevelCache.BeatmapLevelData);

            if (!_beatmapLevelCache.DifficultyMatches(__result))
            {
                Plugin.Log.Debug($"Setting beatmap level cache beatmap key to {__result}");

                _beatmapLevelCache.InvalidateDifficulty();
                _beatmapLevelCache.BeatmapKey = __result;
            }

            tcs.TrySetResult(true);
        }

        [AffinityPatch(typeof(BeatmapDataLoader), nameof(BeatmapDataLoader.LoadBeatmapDataAsync))]
        [AffinityPrefix]
        private bool LoadBeatmapDataWithCacheAsync(ref Task<IReadonlyBeatmapData?> __result, IBeatmapLevelData beatmapLevelData, BeatmapKey beatmapKey, float startBpm, bool loadingForDesignatedEnvironment, IEnvironmentInfo? targetEnvironmentInfo, IEnvironmentInfo? originalEnvironmentInfo, BeatmapLevelDataVersion beatmapLevelDataVersion, GameplayModifiers? gameplayModifiers, PlayerSpecificSettings? playerSpecificSettings, bool enableBeatmapDataCaching)
        {
            Assert.That(UnityGame.OnMainThread);

            var request = new BeatmapDataRequest(beatmapLevelData, beatmapKey, startBpm, loadingForDesignatedEnvironment, targetEnvironmentInfo, originalEnvironmentInfo, beatmapLevelDataVersion, gameplayModifiers, playerSpecificSettings, enableBeatmapDataCaching);

            if (!_beatmapLevelCache.LevelMatches(beatmapLevelData))
            {
                Plugin.Log.Debug("Level data changed, returning original method");
                return true;
            }

            if (_beatmapLevelCache.BeatmapDataRequest?.Equals(request) == true)
            {
                Plugin.Log.Debug("Returning stored beatmap data task");
                __result = _beatmapLevelCache.BeatmapDataLoadingTask!;
                return false;
            }

            Plugin.Log.Debug("Starting new beatmap data request");

            _beatmapLevelCache.BeatmapDataRequest = request;
            __result = _beatmapLevelCache.BeatmapDataLoadingTask = request.Start(_beatmapDataLoader);

            return false;
        }
    }
}
