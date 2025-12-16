using System;
using System.Linq;
using MonoMod.RuntimeDetour;
using Zenject;

namespace SongCore.Hooks
{
    // TODO: Remove missing characteristic. Might end up in wiped save data.
    internal class CustomCharacteristicsHook : IInitializable, IDisposable
    {
        private Hook _getBeatmapCharacteristicBySerializedNameHook = null!;

        public void Initialize()
        {
            _getBeatmapCharacteristicBySerializedNameHook = new Hook(typeof(BeatmapCharacteristicCollection).GetMethod(nameof(BeatmapCharacteristicCollection.GetBeatmapCharacteristicBySerializedName))!, GetCustomCharacteristic, true);
        }

        public void Dispose()
        {
            _getBeatmapCharacteristicBySerializedNameHook.Dispose();
        }

        private BeatmapCharacteristicSO? GetCustomCharacteristic(Func<BeatmapCharacteristicCollection, string, BeatmapCharacteristicSO> original, BeatmapCharacteristicCollection instance, string serializedName)
        {
            var result = original(instance, serializedName);

            if (result != null)
            {
                return result;
            }

            var customCharacteristic = Collections.customCharacteristics.FirstOrDefault(c => c.serializedName == serializedName);
            return customCharacteristic != null ? customCharacteristic : Collections.customCharacteristics.FirstOrDefault(c => c.serializedName == "MissingCharacteristic");
        }
    }
}
