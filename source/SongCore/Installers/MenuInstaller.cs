using SongCore.Patches;
using SongCore.Patches.BeatmapLevelCache;
using SongCore.UI;
using Zenject;

namespace SongCore.Installers
{
    internal class MenuInstaller : Installer
    {
        public override void InstallBindings()
        {
            Container.Bind<SettingsController>().AsSingle();
            Container.BindInterfacesAndSelfTo<Loader>().AsSingle();
            Container.BindInterfacesAndSelfTo<ColorsUI>().AsSingle();
            Container.Bind<ProgressBar>().FromNewComponentOnNewGameObject().AsSingle();
            Container.BindInterfacesAndSelfTo<RequirementsUI>().AsSingle();
            Container.BindInterfacesAndSelfTo<EventProxyPatches>().AsSingle();
            Container.Bind<BeatmapLevelCache>().AsSingle();
            Container.BindInterfacesTo<BeatmapJsonCachePatches>().AsSingle();
            Container.BindInterfacesTo<BeatmapDataCachePatches>().AsSingle();
            Container.BindInterfacesTo<SongDataMenuPatches>().AsSingle();
            Container.BindInterfacesTo<InternalRestartPatch>().AsSingle();
        }
    }
}
