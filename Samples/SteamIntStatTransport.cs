using System;
using System.Threading;
using System.Threading.Tasks;
using fefek5.Stats.Runtime;
using UnityEngine;

namespace fefek5.Stats.Samples
{
    [Serializable]
    public class SteamIntStatTransport : StatTransport<int>
    {
        [field: SerializeField] public SteamToys.Runtime.StatsSystem.IntStat SteamIntStat { get; set; }

        public override Task<int> PullAsync(CancellationToken cancellationToken)
        {
            SteamIntStat.TryPullFromSteam();
            
            return Task.FromResult(SteamIntStat.Value);
        }
        
        public override Task PushAsync(int value, CancellationToken cancellationToken)
        {
            SteamIntStat.Value = value;
            
            SteamIntStat.TryPushToSteam();
            
            return Task.CompletedTask;
        }
    }
}