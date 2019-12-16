using System;
using System.Collections.Generic;
using UnityEngine;

// List<T>
namespace SsitEngine.Unity.Utility
{
    [Serializable]
    public class Serialization<T>
    {
        [SerializeField] private List<T> target;

        public Serialization( List<T> target )
        {
            this.target = target;
        }

        public List<T> ToList()
        {
            return target;
        }
    }


    [Serializable]
    public class Serialization<TKey, TValue> : ISerializationCallbackReceiver
    {
        [SerializeField] private List<TKey> keys;

        private Dictionary<TKey, TValue> target;

        [SerializeField] private List<TValue> values;

        public Serialization( Dictionary<TKey, TValue> target )
        {
            this.target = target;
        }

        public void OnBeforeSerialize()
        {
            keys = new List<TKey>(target.Keys);
            values = new List<TValue>(target.Values);
        }

        public void OnAfterDeserialize()
        {
            var count = Math.Min(keys.Count, values.Count);
            target = new Dictionary<TKey, TValue>(count);
            for (var i = 0; i < count; ++i) target.Add(keys[i], values[i]);
        }

        public Dictionary<TKey, TValue> ToDictionary()
        {
            return target;
        }
    }
}