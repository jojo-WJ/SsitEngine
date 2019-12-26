using System;

namespace JxDebug
{
    [Serializable]
    public class Filter
    {
        // 激活
        public bool isActive;

        public Filter( string tag, int hash )
        {
            Tag = tag;
            Hash = hash;
            isActive = true;
        }

        /// <summary>
        /// 标签
        /// </summary>
        public string Tag { get; private set; }

        /// <summary>
        /// Hash
        /// </summary>
        public int Hash { get; private set; }
    }
}