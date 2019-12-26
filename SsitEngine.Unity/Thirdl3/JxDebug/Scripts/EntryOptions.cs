using System;
using UnityEngine;

namespace JxDebug
{
    [Serializable]
    public struct EntryOptions
    {
        public FontStyle style;
        public int size;

        private bool hasColorBeenAssigned;

        private Color _color;

        public Color color
        {
            get
            {
                if (!hasColorBeenAssigned)
                {
                    color = JxDebug.Setting.entryDefaultColor;
                }
                return _color;
            }
            set
            {
                _color = value;
                hasColorBeenAssigned = true;
            }
        }
    }
}