using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using HMUI;
using SiraUtil.Affinity;
using SongCore.Data;
using SongCore.UI;
using SongCore.Utilities;
using UnityEngine;
using Zenject;

namespace SongCore.Patches
{
    internal class SongDataMenuPatches : IAffinity, IInitializable, IDisposable
    {
        private readonly LevelCollectionViewController _levelCollectionViewController;
        private readonly StandardLevelDetailViewController _standardLevelDetailViewController;
        private readonly CustomLevelLoader _customLevelLoader;
        private readonly RequirementsUI _requirementsUI;
        private readonly PluginConfig _config;
        private readonly Dictionary<string, Dictionary<BeatmapDifficulty, string>> _characteristicDifficultyLabels = new();
        private readonly Dictionary<string, Sprite> _characteristicDetailsSprites = new();

        private SongData? _songData;
        private bool _actionButtonInteractable;
        private bool _practiceButtonInteractable;

        private SongDataMenuPatches(LevelCollectionViewController levelCollectionViewController, StandardLevelDetailViewController standardLevelDetailViewController, CustomLevelLoader customLevelLoader, RequirementsUI requirementsUI, PluginConfig config)
        {
            _levelCollectionViewController = levelCollectionViewController;
            _standardLevelDetailViewController = standardLevelDetailViewController;
            _customLevelLoader = customLevelLoader;
            _requirementsUI = requirementsUI;
            _config = config;
        }

        public void Initialize()
        {
            _levelCollectionViewController.didSelectLevelEvent += HandleLevelCollectionViewControllerDidSelectLevel;
        }

        public void Dispose()
        {
            _levelCollectionViewController.didSelectLevelEvent -= HandleLevelCollectionViewControllerDidSelectLevel;

            foreach (var sprite in _characteristicDetailsSprites.Values)
            {
                SpriteAsyncLoader.DestroySprite(sprite);
            }
        }

        private void HandleLevelCollectionViewControllerDidSelectLevel(LevelCollectionViewController levelCollectionViewController, BeatmapLevel beatmapLevel)
        {
            _songData = Collections.GetCustomLevelSongData(beatmapLevel.levelID);

            if (_songData == null)
            {
                return;
            }

            _characteristicDifficultyLabels.Clear();
            foreach (var difficultyData in _songData._difficulties)
            {
                _characteristicDifficultyLabels.TryAdd(difficultyData._beatmapCharacteristicName, new Dictionary<BeatmapDifficulty, string>());
                if (!string.IsNullOrWhiteSpace(difficultyData._difficultyLabel))
                {
                    _characteristicDifficultyLabels[difficultyData._beatmapCharacteristicName].TryAdd(difficultyData._difficulty, difficultyData._difficultyLabel);
                }
            }
        }

        [AffinityPatch(typeof(BeatmapDifficultySegmentedControlController), nameof(BeatmapDifficultySegmentedControlController.SetData))]
        private void SetData(BeatmapDifficultySegmentedControlController __instance)
        {
            if (_songData == null || !_config.DisplayDiffLabels)
            {
                return;
            }

            var selectedBeatmapCharacteristic = _standardLevelDetailViewController._standardLevelDetailView._beatmapCharacteristicSegmentedControlController.selectedBeatmapCharacteristic.serializedName;
            __instance._difficultySegmentedControl._texts = __instance._difficulties
                .Select((diff, i) => _characteristicDifficultyLabels.TryGetValue(selectedBeatmapCharacteristic, out var difficultyLabels) && difficultyLabels.TryGetValue(diff, out var difficultyLabel)
                    ? GetDifficultyLabel(difficultyLabel) ?? __instance._difficultySegmentedControl._texts[i]
                    : __instance._difficultySegmentedControl._texts[i])
                .ToArray();

            for (var i = 0; i < __instance._difficultySegmentedControl._texts.Count; i++)
            {
                ((TextSegmentedControlCell)__instance._difficultySegmentedControl.cells[i]).text = __instance._difficultySegmentedControl._texts[i];
            }
        }

        [AffinityPatch(typeof(LevelBar), nameof(LevelBar.SetupData))]
        private void SetupData(LevelBar __instance, BeatmapDifficulty beatmapDifficulty, BeatmapCharacteristicSO beatmapCharacteristic)
        {
            if (_songData == null || !_config.DisplayDiffLabels || !__instance._showDifficultyAndCharacteristic)
            {
                return;
            }

            if (_characteristicDifficultyLabels.TryGetValue(beatmapCharacteristic.serializedName, out var difficultyLabels) && difficultyLabels.TryGetValue(beatmapDifficulty, out var difficultyLabel))
            {
                __instance._difficultyText.text = GetDifficultyLabel(difficultyLabel) ?? __instance._difficultyText.text;
            }

            var characteristicDetails = _songData._characteristicDetails?.FirstOrDefault(d => d._beatmapCharacteristicName == beatmapCharacteristic.serializedName);
            if (characteristicDetails != null)
            {
                var sprite = GetCharacteristicIcon(characteristicDetails._characteristicIconFilePath);
                if (sprite != null)
                {
                    __instance._characteristicIconImageView.sprite = sprite;
                }
            }
        }

        // TODO: Find a way to add a limitation to the size of the text.
        private string? GetDifficultyLabel(string difficultyLabel)
        {
            return string.IsNullOrWhiteSpace(difficultyLabel) ? null : difficultyLabel.Replace("<", "<\u200B").Replace(">", ">\u200B");
        }

        [AffinityPatch(typeof(BeatmapCharacteristicSegmentedControlController), nameof(BeatmapCharacteristicSegmentedControlController.SetData))]
        private void SelectDefaultCharacteristic(BeatmapCharacteristicSegmentedControlController __instance)
        {
            if (_songData == null || _songData._defaultCharacteristic == null || _songData._defaultCharacteristic == __instance.selectedBeatmapCharacteristic.serializedName)
            {
                return;
            }

            var index = __instance._currentlyAvailableBeatmapCharacteristics.FindIndex(c => c.serializedName == _songData._defaultCharacteristic);
            if (index != -1)
            {
                __instance._segmentedControl.SelectCellWithNumber(index);
                __instance._selectedBeatmapCharacteristic = __instance._currentlyAvailableBeatmapCharacteristics[index];
            }
        }

        [AffinityPatch(typeof(BeatmapCharacteristicSegmentedControlController), nameof(BeatmapCharacteristicSegmentedControlController.SetData))]
        private void SetCosmeticCharacteristic(BeatmapCharacteristicSegmentedControlController __instance, BeatmapCharacteristicSO selectedBeatmapCharacteristic)
        {
            if (_songData == null || _songData._characteristicDetails == null || !_config.DisplayCustomCharacteristics)
            {
                return;
            }

            foreach (var characteristicDetails in _songData._characteristicDetails)
            {
                var index = __instance._currentlyAvailableBeatmapCharacteristics.FindIndex(c => c.serializedName == characteristicDetails._beatmapCharacteristicName);

                if (index == -1)
                {
                    continue;
                }

                var dataItem = __instance._segmentedControl._dataItems[index];
                var cell = (IconSegmentedControlCell)__instance._segmentedControl.cells[index];

                if (!string.IsNullOrWhiteSpace(characteristicDetails._characteristicLabel))
                {
                    dataItem.hintText = characteristicDetails._characteristicLabel;
                    cell.hintText = characteristicDetails._characteristicLabel;
                }

                var icon = GetCharacteristicIcon(characteristicDetails._characteristicIconFilePath);
                if (icon != null)
                {
                    dataItem.icon = icon;
                    cell.sprite = icon;
                }
            }
        }

        private Sprite? GetCharacteristicIcon(string? characteristicIconFilePath)
        {
            if (string.IsNullOrWhiteSpace(characteristicIconFilePath))
            {
                return null;
            }

            var spritePath = Path.Combine(_customLevelLoader._loadedBeatmapSaveData[_standardLevelDetailViewController.beatmapLevel.levelID].customLevelFolderInfo.folderPath, characteristicIconFilePath);
            if (!_characteristicDetailsSprites.TryGetValue(spritePath, out var icon))
            {
                if ((icon = Utils.LoadSpriteFromFile(spritePath)) != null)
                {
                    _characteristicDetailsSprites.Add(spritePath, icon);
                }
            }

            return icon;
        }

        [AffinityPatch(typeof(StandardLevelDetailView), nameof(StandardLevelDetailView.RefreshContent))]
        private void ProcessBeatmapRequirements(StandardLevelDetailView __instance)
        {
            var beatmapLevel = _standardLevelDetailViewController.beatmapLevel;
            var beatmapKey = __instance.beatmapKey;
            var actionButton = __instance.actionButton;
            var practiceButton = __instance.practiceButton;

            actionButton.interactable = true;
            practiceButton.interactable = true;

            _requirementsUI.ButtonGlowColor = false;
            _requirementsUI.ButtonInteractable = false;

            if (_songData == null)
            {
                return;
            }

            var wipFolderSong = false;
            var difficultyData = Collections.GetCustomLevelSongDifficultyData(beatmapKey);
            if (difficultyData != null)
            {
                //If no additional information is present
                if (!difficultyData.additionalDifficultyData._requirements.Any() &&
                    !difficultyData.additionalDifficultyData._suggestions.Any() &&
                    !difficultyData.additionalDifficultyData._warnings.Any() &&
                    !difficultyData.additionalDifficultyData._information.Any() &&
                    !_songData!.contributors.Any() && !Utils.DiffHasColors(difficultyData))
                {
                    _requirementsUI.ButtonGlowColor = false;
                    _requirementsUI.ButtonInteractable = false;
                }
                else if (!difficultyData.additionalDifficultyData._warnings.Any())
                {
                    _requirementsUI.ButtonGlowColor = true;
                    _requirementsUI.ButtonInteractable = true;
                    _requirementsUI.SetRainbowColors(Utils.DiffHasColors(difficultyData));
                }
                else if (difficultyData.additionalDifficultyData._warnings.Any())
                {
                    _requirementsUI.ButtonGlowColor = true;
                    _requirementsUI.ButtonInteractable = true;
                    if (difficultyData.additionalDifficultyData._warnings.Contains("WIP"))
                    {
                        actionButton.interactable = false;
                    }

                    _requirementsUI.SetRainbowColors(Utils.DiffHasColors(difficultyData));
                }
            }

            if (beatmapLevel.levelID.EndsWith(" WIP", StringComparison.Ordinal))
            {
                _requirementsUI.ButtonGlowColor = true;
                _requirementsUI.ButtonInteractable = true;
                actionButton.interactable = false;
                wipFolderSong = true;

                if (difficultyData != null)
                {
                    _requirementsUI.SetRainbowColors(Utils.DiffHasColors(difficultyData));
                }
            }

            if (difficultyData != null)
            {
                foreach (var requirement in difficultyData.additionalDifficultyData._requirements)
                {
                    if (!Collections.capabilities.Contains(requirement))
                    {
                        actionButton.interactable = false;
                        practiceButton.interactable = false;
                        _requirementsUI.ButtonGlowColor = true;
                        _requirementsUI.ButtonInteractable = true;
                    }
                }
            }

            if (beatmapKey.beatmapCharacteristic.serializedName == "MissingCharacteristic")
            {
                actionButton.interactable = false;
                practiceButton.interactable = false;
                _requirementsUI.ButtonGlowColor = true;
                _requirementsUI.ButtonInteractable = true;
            }

            _requirementsUI.beatmapLevel = beatmapLevel;
            _requirementsUI.beatmapKey = beatmapKey;
            _requirementsUI.songData = _songData;
            _requirementsUI.diffData = difficultyData;
            _requirementsUI.wipFolder = wipFolderSong;
        }

        [AffinityPatch(typeof(StandardLevelDetailView), nameof(StandardLevelDetailView.CheckIfBeatmapLevelDataExists))]
        [AffinityPrefix]
        private void SaveButtonsState(StandardLevelDetailView __instance)
        {
            _actionButtonInteractable = __instance.actionButton.interactable;
            _practiceButtonInteractable = __instance.practiceButton.interactable;
        }

        [AffinityPatch(typeof(StandardLevelDetailView), nameof(StandardLevelDetailView.CheckIfBeatmapLevelDataExists))]
        private void RestoreButtonsState(StandardLevelDetailView __instance)
        {
            __instance.actionButton.interactable = _actionButtonInteractable;
            __instance.practiceButton.interactable = _practiceButtonInteractable;
        }

        [AffinityPatch(typeof(StandardLevelScenesTransitionSetupDataSO), nameof(StandardLevelScenesTransitionSetupDataSO.InitColorInfo))]
        private void SetSoloOverrideColorScheme(StandardLevelScenesTransitionSetupDataSO __instance)
        {
            if (_config is { CustomSongNoteColors: false, CustomSongEnvironmentColors: false, CustomSongObstacleColors: false })
            {
                return;
            }

            var songData = Collections.GetCustomLevelSongDifficultyData(__instance.beatmapKey);
            var overrideColorScheme = GetOverrideColorScheme(songData, __instance.colorScheme);
            if (overrideColorScheme is null)
            {
                return;
            }

            __instance.usingOverrideColorScheme = true;
            __instance.colorScheme = overrideColorScheme;
        }

        [AffinityPatch(typeof(MultiplayerLevelScenesTransitionSetupDataSO), nameof(MultiplayerLevelScenesTransitionSetupDataSO.InitColorInfo))]
        private void SetMultiplayerOverrideColorScheme(MultiplayerLevelScenesTransitionSetupDataSO __instance)
        {
            if (_config is { CustomSongNoteColors: false, CustomSongEnvironmentColors: false, CustomSongObstacleColors: false })
            {
                return;
            }

            var songData = Collections.GetCustomLevelSongDifficultyData(__instance.beatmapKey);
            var overrideColorScheme = GetOverrideColorScheme(songData, __instance.colorScheme);
            if (overrideColorScheme is null)
            {
                return;
            }

            __instance.usingOverrideColorScheme = true;
            __instance.colorScheme = overrideColorScheme;
        }

        private ColorScheme? GetOverrideColorScheme(SongData.DifficultyData? songDifficultyData, ColorScheme currentColorScheme)
        {
            if (songDifficultyData is null || (songDifficultyData._colorLeft == null && songDifficultyData._colorRight == null && songDifficultyData._envColorLeft == null && songDifficultyData._envColorRight == null &&
                                               songDifficultyData._envColorWhite == null && songDifficultyData._obstacleColor == null && songDifficultyData._envColorLeftBoost == null && songDifficultyData._envColorRightBoost == null &&
                                               songDifficultyData._envColorWhiteBoost == null))
            {
                return null;
            }

            if (_config.CustomSongNoteColors)
            {
                Plugin.Log.Debug("Custom song note colors On");
            }

            if (_config.CustomSongEnvironmentColors)
            {
                Plugin.Log.Debug("Custom song environment colors On");
            }

            if (_config.CustomSongObstacleColors)
            {
                Plugin.Log.Debug("Custom song obstacle colors On");
            }

            var saberLeft = songDifficultyData._colorLeft == null || !_config.CustomSongNoteColors
                ? currentColorScheme.saberAColor
                : Utils.ColorFromMapColor(songDifficultyData._colorLeft);
            var saberRight = songDifficultyData._colorRight == null || !_config.CustomSongNoteColors
                ? currentColorScheme.saberBColor
                : Utils.ColorFromMapColor(songDifficultyData._colorRight);
            var envLeft = songDifficultyData._envColorLeft == null || !_config.CustomSongEnvironmentColors
                ? songDifficultyData._colorLeft == null ? currentColorScheme.environmentColor0 : Utils.ColorFromMapColor(songDifficultyData._colorLeft)
                : Utils.ColorFromMapColor(songDifficultyData._envColorLeft);
            var envRight = songDifficultyData._envColorRight == null || !_config.CustomSongEnvironmentColors
                ? songDifficultyData._colorRight == null ? currentColorScheme.environmentColor1 : Utils.ColorFromMapColor(songDifficultyData._colorRight)
                : Utils.ColorFromMapColor(songDifficultyData._envColorRight);
            var envWhite = songDifficultyData._envColorWhite == null || !_config.CustomSongEnvironmentColors
                ? currentColorScheme.environmentColorW
                : Utils.ColorFromMapColor(songDifficultyData._envColorWhite);
            var envLeftBoost = songDifficultyData._envColorLeftBoost == null || !_config.CustomSongEnvironmentColors
                ? currentColorScheme.environmentColor0Boost
                : Utils.ColorFromMapColor(songDifficultyData._envColorLeftBoost);
            var envRightBoost = songDifficultyData._envColorRightBoost == null|| !_config.CustomSongEnvironmentColors
                ? currentColorScheme.environmentColor1Boost
                : Utils.ColorFromMapColor(songDifficultyData._envColorRightBoost);
            var envWhiteBoost = songDifficultyData._envColorWhiteBoost == null  || !_config.CustomSongEnvironmentColors
                ? currentColorScheme.environmentColorWBoost
                : Utils.ColorFromMapColor(songDifficultyData._envColorWhiteBoost);
            var obstacle = songDifficultyData._obstacleColor == null || !_config.CustomSongObstacleColors
                ? currentColorScheme.obstaclesColor
                : Utils.ColorFromMapColor(songDifficultyData._obstacleColor);

            return new ColorScheme("SongCoreMapColorScheme", "SongCore Map Color Scheme", true, "SongCore Map Color Scheme", false, true, saberLeft, saberRight, true,
                envLeft, envRight, envWhite, envLeftBoost != default && envRightBoost != default, envLeftBoost, envRightBoost, envWhiteBoost, obstacle);
        }
    }
}
