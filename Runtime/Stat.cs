using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using fefek5.SaveDataVariable.Runtime;
using fefek5.Toys.Runtime.Attributes;
using UnityEngine;

namespace fefek5.Stats.Runtime
{
    /// <summary>
    /// The non generic handle of a stat, so <see cref="StatsDB"/> and inspectors can hold stats of
    /// mixed value types. The value itself lives in <see cref="Stat{T}"/>.
    /// </summary>
    public abstract class Stat : ScriptableObject
    {
        [field: SerializeField, Tooltip("Key the value is saved under. Two stats saved together must not share it.")]
        public SaveKey SaveKey { get; private set; } = SaveKey.RandomKey;

        /// <summary>Puts the value into <paramref name="saveData"/>, without touching any file.</summary>
        public abstract void WriteTo(SaveData saveData);

        /// <summary>
        /// Takes the value from <paramref name="saveData"/>, or the default value when it has none.
        /// </summary>
        public abstract void ReadFrom(SaveData saveData);

        public abstract void ResetToDefault();

        /// <summary>Sends the value to every transport.</summary>
        public abstract Task PushAsync(CancellationToken cancellationToken = default);

        /// <summary>Takes the value from every transport in list order, so the last one wins.</summary>
        public abstract Task PullAsync(CancellationToken cancellationToken = default);

        [ContextMenu("Push Async")]
        protected void PushAsyncContextMenu() => _ = PushAsync();

        [ContextMenu("Pull Async")]
        protected void PullAsyncContextMenu() => _ = PullAsync();
    }

    /// <summary>
    /// A stat whose value is a plain field, so reading and writing it every frame costs nothing.
    /// Saving it is <see cref="StatsDB.Save"/> and sending it anywhere is <see cref="PushAsync"/>,
    /// both meant for natural boundaries like the end of a round, the way Steam separates
    /// <c>SetStat</c> from <c>StoreStats</c>.
    /// </summary>
    public abstract class Stat<T> : Stat
    {
        #region Properties

        public T Value
        {
            get => GetValue();
            set => SetValue(value);
        }

        public T DefaultValue
        {
            get => GetDefaultValue();
            set => SetDefaultValue(value);
        }

        #endregion

        #region Inspector Fields

        [SerializeField, Tooltip("Value the stat starts with until it is loaded or set.")]
        private T _defaultValue;

        [field: SerializeReference, SelectType]
        public List<StatTransport<T>> Transports { get; private set; } = new();

        #endregion

        // Not serialized: a serialized field would dirty the asset while playing in the editor
        // and bake the last played value into the build.
        [NonSerialized] private T _value;

        protected virtual void OnEnable() => _value = _defaultValue;

        #region Getters and Setters

        public virtual T GetValue() => _value;

        public virtual void SetValue(T value) => _value = value;

        public virtual T GetDefaultValue() => _defaultValue;

        public virtual void SetDefaultValue(T value) => _defaultValue = value;

        #endregion

        #region Save Data

        public override void WriteTo(SaveData saveData) => saveData.SetKey(SaveKey, Value);

        public override void ReadFrom(SaveData saveData) => Value = saveData.GetKey(SaveKey, DefaultValue);

        public override void ResetToDefault() => Value = DefaultValue;

        #endregion

        #region Transports

        public override async Task PushAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                foreach (var transport in Transports)
                    await transport.PushAsync(Value, cancellationToken);
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception e)
            {
                Debug.LogException(e, this);
                throw;
            }
        }

        public override async Task PullAsync(CancellationToken cancellationToken = default)
        {
            foreach (var transport in Transports)
                await PullAsync(transport, cancellationToken);
        }

        public virtual async Task PullAsync(StatTransport<T> transport, CancellationToken cancellationToken)
        {
            try
            {
                Value = await transport.PullAsync(cancellationToken);
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception e)
            {
                Debug.LogException(e, this);
                throw;
            }
        }

        #endregion

        public override string ToString() => Value?.ToString() ?? "null";
    }
}
