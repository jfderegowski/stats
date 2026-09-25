using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using fefek5.SaveDataVariable.Runtime;
using fefek5.Toys.Runtime.Attributes;
using UnityEngine;

namespace fefek5.Stats.Runtime
{
    public class Stat<T> : ScriptableObject
    {
        #region Properties

        public T Value
        {
            get => GetValue();
            set => SetValue(value);
        }

        public bool IsDirty => _value.IsDirty;

        #endregion

        #region Inspector Fields

        [SerializeField] private SaveVar<T> _value = new("Stats.json", new SaveKey("STAT_NAME"));

        [field: SerializeReference, SelectType]
        public List<StatTransport<T>> Transports { get; private set; } = new();

        #endregion

        #region Getters and Setters

        private T GetValue() => _value;

        public virtual void SetValue(T value) => _value.SetValueWithoutNotifying(value);

        #endregion

        public virtual async Task PullAsync() => await PullAsync(CancellationToken.None);
        
        public virtual async Task PullAsync(CancellationToken cancellationToken)
        {
            if (_value.IsDirty)
                await _value.PullAsync(cancellationToken);
        }

        public virtual async Task PullAsync(StatTransport<T> transport, CancellationToken cancellationToken)
        {
            try
            {
                var value = await transport.PullAsync(cancellationToken);

                await _value.SetValueAsync(value, cancellationToken);
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception e)
            {
                Debug.LogException(e);
                throw;
            }
        }

        public virtual async Task PushAsync() => await PushAsync(CancellationToken.None);
        
        public virtual async Task PushAsync(CancellationToken cancellationToken)
        {
            try
            {
                if (_value.IsDirty)
                    await _value.PushAsync(cancellationToken);

                foreach (var transport in Transports)
                    await transport.PushAsync(Value, cancellationToken);
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception e)
            {
                Debug.LogException(e);
                throw;
            }
        }
    }
}