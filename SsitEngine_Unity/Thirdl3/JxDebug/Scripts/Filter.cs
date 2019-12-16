using System;

namespace JxDebug
{
    [Serializable]
    public class Filter
    {
        // 激活
        public bool isActive;
        /// <summary>
        /// 标签
        /// </summary>
        public string Tag { get; private set; }
        /// <summary>
        /// Hash
        /// </summary>
        public int Hash { get; private set; }

        public Filter( string tag, int hash )
        {
            this.Tag = tag;
            this.Hash = hash;
            isActive = true;
        }
    }
}