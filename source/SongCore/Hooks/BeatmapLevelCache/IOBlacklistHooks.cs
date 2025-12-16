using System;
using System.Collections.Concurrent;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using ModestTree;
using MonoMod.RuntimeDetour;
using Zenject;

namespace SongCore.Hooks.BeatmapLevelCache
{
    /// <summary>
    /// This forces consumers to use cached data when reading blacklisted files.
    /// </summary>
    internal class IOBlacklistHooks : IInitializable, IDisposable
    {
        private Hook _readHook = null!;
        private Hook _readAsyncHook = null!;

        public static ConcurrentDictionary<string, string> FilesBlacklist { get; } = new();
        public static AsyncLocal<bool> AllowIO { get; } = new();

        public void Initialize()
        {
            _readHook = new Hook(typeof(FileStream).GetMethod(nameof(FileStream.Read), BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly)!, ReadHook, true);
            _readAsyncHook = new Hook(typeof(FileStream).GetMethod(nameof(FileStream.ReadAsync), BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly)!, ReadAsyncHook, true);
        }

        public void Dispose()
        {
            _readHook.Dispose();
            _readAsyncHook.Dispose();
        }

        private int ReadHook(Func<FileStream, byte[], int, int, int> original, FileStream instance, byte[] array, int offset, int count)
        {
            AssertFileNotBlacklisted(Path.GetFullPath(instance.Name));
            return original(instance, array, offset, count);
        }

        private Task<int> ReadAsyncHook(Func<FileStream, byte[], int, int, CancellationToken, Task<int>> original, FileStream instance, byte[] buffer, int offset, int count, CancellationToken cancellationToken)
        {
            AssertFileNotBlacklisted(Path.GetFullPath(instance.Name));
            return original(instance, buffer, offset, count, cancellationToken);
        }

        private void AssertFileNotBlacklisted(string filePath)
        {
            try
            {
                Assert.That(!FilesBlacklist.TryGetValue(filePath, out var reason) || AllowIO.Value, $"File '{filePath}' is blacklisted. Reason: {reason}");
            }
            catch (ZenjectException ex)
            {
                // Ensure we don't get silenced by try-catch blocks.
                Plugin.Log.Error(ex.ToString());
                throw;
            }
        }
    }
}
