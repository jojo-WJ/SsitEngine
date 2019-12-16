// BitArray

using System;
using System.Collections;
using System.Text;
using UnityEngine;

namespace SsitEngine.Unity.Utility
{
    [Serializable]
    public class SerializationBitArray : ISerializationCallbackReceiver
    {
        [SerializeField] private string flags;

        private BitArray target;

        public SerializationBitArray( BitArray target )
        {
            this.target = target;
        }

        public void OnBeforeSerialize()
        {
            var ss = new StringBuilder(target.Length);
            for (var i = 0; i < target.Length; ++i) ss.Insert(0, target[i] ? '1' : '0');

            flags = ss.ToString();
        }

        public void OnAfterDeserialize()
        {
            target = new BitArray(flags.Length);
            for (var i = 0; i < flags.Length; ++i) target.Set(flags.Length - i - 1, flags[i] == '1');
        }

        public BitArray ToBitArray()
        {
            return target;
        }
    }
}