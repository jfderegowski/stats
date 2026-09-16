using System;
using System.Threading;
using System.Threading.Tasks;

namespace Runtime
{
    [Serializable]
    public abstract class StatTransport<T>
    {
        public abstract Task<T> PullAsync(CancellationToken cancellationToken);
        
        public abstract Task PushAsync(T value, CancellationToken cancellationToken);
    }
}