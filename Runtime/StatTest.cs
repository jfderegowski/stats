using System;
using UnityEngine;

namespace fefek5.Stats.Runtime
{
    [Serializable]
    public class TestStruct
    {
        [SerializeField] private int _int;
    }
    
    public class StatTest : MonoBehaviour
    {
        [SerializeField] private int _exampleNumber;
        [SerializeField] private Stat<int> _intStat;
        [SerializeField] private Stat<int> _intStat2;
        [SerializeField] private TestStruct _testStruct;
    }
}