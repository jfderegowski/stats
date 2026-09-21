using UnityEngine;

namespace fefek5.Stats.Runtime
{
    [CreateAssetMenu(fileName = "New Int Stat", menuName = "Stats/Int Stat")]
    public class IntStat : Stat<int>
    {
        [ContextMenu("Pull Async")]
        public void PullAsyncContextMenu() => PushAsync();

        [ContextMenu("Push Async")]
        public void PushAsyncContextMenu() => PushAsync();
    }
}