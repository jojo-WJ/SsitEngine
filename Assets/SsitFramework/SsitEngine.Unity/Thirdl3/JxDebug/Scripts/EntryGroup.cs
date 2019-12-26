using System.Collections.Generic;
using UnityEngine;

namespace JxDebug
{
    public class EntryGroup
    {
        public GUIContent content;
        private readonly List<Entry> entryList = new List<Entry>();

        public EntryGroup( Entry entry )
        {
            content = new GUIContent();
            Add(entry);
        }

        public Entry[] entries { get; private set; }
        public Entry lastEntry { get; private set; }

        public void Add( Entry entry )
        {
            entryList.Add(entry);
            entries = entryList.ToArray();
            lastEntry = entry;
            entry.group = this;
            UpdateContent();
        }

        private void UpdateContent()
        {
            content.text = "(" + entryList.Count + ")";
        }

        public void Remove( Entry entry )
        {
            entryList.Remove(entry);
            entries = entryList.ToArray();
            if (lastEntry == entry && entries.Length > 0)
            {
                lastEntry = entries[entries.Length - 1];
            }
            UpdateContent();
        }

        public bool CanAcceptEntry( Entry entry )
        {
            return entry.data.text == lastEntry.data.text && entry.data.stackTrace == lastEntry.data.stackTrace;
        }
    }
}