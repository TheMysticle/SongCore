using System;
using SiraUtil.Affinity;

namespace SongCore.Patches.BeatmapLevelCache
{
    /// <summary>
    /// These patches redirect game events needed for <see cref="BeatmapDataCachePatches"/> to work.
    /// </summary>
    internal class EventProxyPatches : IAffinity
    {
        public Delegate? LevelCollectionTableViewDidSelectLevelDelegate { get; private set; }
        public Delegate? LevelCollectionViewControllerDidSelectLevelDelegate { get; private set; }
        public Delegate? StandardLevelDetailViewControllerDidChangeContentDelegate { get; private set; }

        [AffinityPatch(typeof(LevelCollectionTableView), $"add_{nameof(LevelCollectionTableView.didSelectLevelEvent)}")]
        [AffinityPrefix]
        private bool RedirectLevelCollectionTableViewDidSelectLevelEventAdd(Delegate value)
        {
            LevelCollectionTableViewDidSelectLevelDelegate = Delegate.Combine(LevelCollectionTableViewDidSelectLevelDelegate, value);

            return false;
        }

        [AffinityPatch(typeof(LevelCollectionTableView), $"remove_{nameof(LevelCollectionTableView.didSelectLevelEvent)}")]
        [AffinityPrefix]
        private bool RedirectLevelCollectionTableViewDidSelectLevelEventRemove(Delegate value)
        {
            LevelCollectionTableViewDidSelectLevelDelegate = Delegate.Remove(LevelCollectionTableViewDidSelectLevelDelegate, value);

            return false;
        }

        [AffinityPatch(typeof(LevelCollectionViewController), $"add_{nameof(LevelCollectionViewController.didSelectLevelEvent)}")]
        [AffinityPrefix]
        private bool RedirectLevelCollectionViewControllerDidSelectLevelEventAdd(Delegate value)
        {
            LevelCollectionViewControllerDidSelectLevelDelegate = Delegate.Combine(LevelCollectionViewControllerDidSelectLevelDelegate, value);

            return false;
        }

        [AffinityPatch(typeof(LevelCollectionViewController), $"remove_{nameof(LevelCollectionViewController.didSelectLevelEvent)}")]
        [AffinityPrefix]
        private bool RedirectLevelCollectionViewControllerDidSelectLevelEventRemove(Delegate value)
        {
            LevelCollectionViewControllerDidSelectLevelDelegate = Delegate.Remove(LevelCollectionViewControllerDidSelectLevelDelegate, value);

            return false;
        }

        [AffinityPatch(typeof(StandardLevelDetailViewController), $"add_{nameof(StandardLevelDetailViewController.didChangeContentEvent)}")]
        [AffinityPrefix]
        private bool RedirectDidChangeContentEventAdd(Delegate value)
        {
            StandardLevelDetailViewControllerDidChangeContentDelegate = Delegate.Combine(StandardLevelDetailViewControllerDidChangeContentDelegate, value);

            return false;
        }

        [AffinityPatch(typeof(StandardLevelDetailViewController), $"remove_{nameof(StandardLevelDetailViewController.didChangeContentEvent)}")]
        [AffinityPrefix]
        private bool RedirectDidChangeContentEventRemove(Delegate value)
        {
            StandardLevelDetailViewControllerDidChangeContentDelegate = Delegate.Remove(StandardLevelDetailViewControllerDidChangeContentDelegate, value);

            return false;
        }
    }
}
