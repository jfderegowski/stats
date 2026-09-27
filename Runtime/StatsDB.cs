using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using fefek5.SaveDataVariable.Runtime;
using Runtime;
using UnityEngine;

namespace fefek5.Stats.Runtime
{
    /// <summary>
    /// The stats of the game gathered in one asset, saved together in one file.
    /// <para>
    /// The stats it holds are sub-assets of it, created and removed from its inspector, which
    /// "Window/Stats/Stats DB" opens. A stat can still be an asset of its own; the DB only knows
    /// the ones inside it, and <see cref="Stat.WriteTo"/> and <see cref="Stat.ReadFrom"/> save
    /// any other one.
    /// </para>
    /// <para>
    /// Nothing here runs on its own: values stay in memory until <see cref="Save"/>, so call it at
    /// natural boundaries like the end of a round or on quit, and <see cref="Load"/> at start.
    /// </para>
    /// </summary>
    public class StatsDB : SingletonObject<StatsDB>
    {
        #region Properties

        /// <summary>The stats held by this asset, in the order they were added.</summary>
        public IReadOnlyList<Stat> Stats => _stats;

        /// <summary>Full path of the file the stats are saved to.</summary>
        public string Path => System.IO.Path.GetFullPath(System.IO.Path.Combine(Application.persistentDataPath, _relativePath));

        #endregion

        #region Inspector Fields

        [SerializeField, Tooltip("File the stats are saved to, relative to Application.persistentDataPath.")]
        private string _relativePath = "Stats.json";

        [SerializeField, Tooltip("The stats held by this asset, all of them sub-assets of it. Managed from its inspector.")]
        private List<Stat> _stats = new();

        #endregion

        #region Save and Load

        /// <summary>Writes every stat to the file, replacing what it held.</summary>
        public void Save() => CreateSaveData().Save(Path);

        /// <inheritdoc cref="Save"/>
        public async Awaitable SaveAsync(CancellationToken cancellationToken = default) =>
            await CreateSaveData().SaveAsync(Path, cancellationToken);

        /// <summary>
        /// Reads the file and hands every stat its value. A stat the file does not have, or every
        /// stat when there is no file yet, gets its default value.
        /// </summary>
        public void Load()
        {
            var path = Path;
            var saveData = new SaveData();

            if (File.Exists(path))
                saveData.Load(path);

            ReadFrom(saveData);
        }

        /// <inheritdoc cref="Load"/>
        public async Awaitable LoadAsync(CancellationToken cancellationToken = default)
        {
            var path = Path;
            var saveData = new SaveData();

            if (File.Exists(path))
                await saveData.LoadAsync(path, cancellationToken);

            ReadFrom(saveData);
        }

        private SaveData CreateSaveData()
        {
            var saveData = new SaveData();

            foreach (var stat in _stats)
            {
                if (stat)
                    stat.WriteTo(saveData);
            }

            return saveData;
        }

        private void ReadFrom(SaveData saveData)
        {
            foreach (var stat in _stats)
            {
                if (stat)
                    stat.ReadFrom(saveData);
            }
        }

        public void ResetAllToDefault()
        {
            foreach (var stat in _stats)
            {
                if (stat)
                    stat.ResetToDefault();
            }
        }

        #endregion

        #region Transports

        /// <summary>Sends every stat to its transports.</summary>
        public async Task PushAsync(CancellationToken cancellationToken = default)
        {
            foreach (var stat in _stats)
            {
                if (stat)
                    await stat.PushAsync(cancellationToken);
            }
        }

        /// <summary>Takes every stat from its transports.</summary>
        public async Task PullAsync(CancellationToken cancellationToken = default)
        {
            foreach (var stat in _stats)
            {
                if (stat)
                    await stat.PullAsync(cancellationToken);
            }
        }

        #endregion
    }
}
