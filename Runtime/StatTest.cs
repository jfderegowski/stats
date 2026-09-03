using System;
using UnityEngine;

namespace Runtime
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
        [SerializeField] private TestStruct _testStruct;
    }
}