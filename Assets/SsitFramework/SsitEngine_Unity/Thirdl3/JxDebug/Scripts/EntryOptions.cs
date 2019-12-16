using System;
using UnityEngine;

namespace JxDebug{
    [Serializable]
    public struct EntryOptions {
        public FontStyle style;
        public int size;

        bool hasColorBeenAssigned;

        Color _color;
        public Color color {
            get {
                if(!hasColorBeenAssigned)
                    color = JxDebug.Setting.entryDefaultColor;
                return _color;
            }
            set {
                _color = value;
                hasColorBeenAssigned = true;
            }
        }
    }
}
