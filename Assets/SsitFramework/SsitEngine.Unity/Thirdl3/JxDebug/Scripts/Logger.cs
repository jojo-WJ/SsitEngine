using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace JxDebug
{
    [Serializable]
    public class Logger
    {
        private readonly List<EntryData> buffer = new List<EntryData>();

        private readonly int bufferSize = 1000;

        [SerializeField] protected int capacity;

        private Rect contentsRect;
        private int currentFilter;
        private EntryDrawData[] drawData = new EntryDrawData[0];
        private readonly List<EntryDrawData> drawDataList = new List<EntryDrawData>();
        private int[] drawingDrawDataIndexes = new int[0];
        private int[] drawingEntriesIndexes = new int[0];
        private Entry[] entries = new Entry[0];
        private readonly List<Entry> entriesList = new List<Entry>();
        private bool entryJustRemoved;
        private Filter[] filters = new Filter[0];
        private readonly List<Filter> filtersList = new List<Filter>();
        private EntryGroup[] groups = new EntryGroup[0];
        private readonly List<EntryGroup> groupsList = new List<EntryGroup>();
        private float lastContentHeight;
        private bool lastGroupEntries;
        private bool lastScrollbarVisible;

        [SerializeField] protected ScrollView scrollView;

        private readonly bool[] tagHashesUsed = new bool[32];
        private Entry tempEntry;
        private EntryGroup tempGroup;
        private Rect viewRect;

        public event Action<Filter> onFilterAdded;
        public event Action<Filter> onFilterRemoved;

        public void Initialize()
        {
            for (var i = 0; i < entriesList.Count; i++)
            {
                SetupEntry(entriesList[i]);
            }
            lastGroupEntries = JxDebug.Setting.groupIdenticalEntries;
        }

        private void SetupEntry( Entry entry )
        {
            SetGroupToEntry(entry);
            entry.onEntryRebuilt += OnEntryRebuilt;
            entry.onEntryRemoved += OnEntryRemoved;
        }

        public void Draw( float positionY, float height )
        {
            CalculateCurrentFilter();
            viewRect = new Rect(0, positionY, Screen.width / JxDebug.Setting.scale, height);
            contentsRect = new Rect(viewRect.x, viewRect.y, viewRect.width, lastContentHeight);
            if (buffer.Count > 0)
            {
                AddNewEntries();
            }
            else if (lastGroupEntries != JxDebug.Setting.groupIdenticalEntries)
            {
                SelectDrawingEntries();
            }
            lastGroupEntries = JxDebug.Setting.groupIdenticalEntries;
            GUIUtils.DrawBox(viewRect, JxDebug.Setting.mainColor);
            entryJustRemoved = false;
            scrollView.Draw(viewRect, contentsRect, DrawOnlyVisibleEntries);
            if (!scrollView.isScrollbarVisible && lastScrollbarVisible)
            {
                scrollView.ScrollToTop();
            }
            else if (scrollView.isScrollbarVisible && !lastScrollbarVisible)
            {
                scrollView.ScrollToBottom();
            }
            lastScrollbarVisible = scrollView.isScrollbarVisible;
        }

        private void CalculateCurrentFilter()
        {
            currentFilter = 0;
            for (var i = 0; i < filters.Length; i++)
            {
                if (filters[i].isActive)
                {
                    currentFilter += filters[i].Hash;
                }
            }
        }

        private void AddNewEntries()
        {
            for (var i = 0; i < buffer.Count; i++)
            {
                tempEntry = new Entry(buffer[i]);
                SetupEntry(tempEntry);
                entriesList.Add(tempEntry);
                var drawData = new EntryDrawData();
                drawData.justCreated = true;
                drawData.tagsHash = CalculateTagsHash(tempEntry);
                drawDataList.Add(drawData);
            }
            buffer.Clear();

            while (entriesList.Count > capacity)
            {
                RemoveEntry(entriesList[0]);
            }

            MakeEntriesArray();
            SelectDrawingEntries();
        }

        private int CalculateTagsHash( Entry entry )
        {
            var tagsHash = 0;
            var tags = entry.data.tags.ToArray();
            AddFilters(tags);
            for (var i = 0; i < tags.Length; i++)
            {
                for (var j = 0; j < filters.Length; j++)
                {
                    if (tags[i] == filters[j].Tag)
                    {
                        tagsHash += filters[j].Hash;
                        break;
                    }
                }
            }
            return tagsHash;
        }

        private void SetGroupToEntry( Entry entry )
        {
            var createNewGroup = true;
            for (var i = 0; i < groups.Length; i++)
            {
                if (groups[i].CanAcceptEntry(entry))
                {
                    groups[i].Add(entry);
                    createNewGroup = false;
                    break;
                }
            }

            if (createNewGroup)
            {
                tempGroup = new EntryGroup(entry);
                groupsList.Add(tempGroup);
                groups = groupsList.ToArray();
            }
        }

        private void MakeEntriesArray()
        {
            entries = entriesList.ToArray();
            drawData = drawDataList.ToArray();
        }

        private void SelectDrawingEntries()
        {
            if (JxDebug.Setting.groupIdenticalEntries)
            {
                SelectGroupEntries();
            }
            else
            {
                SelectIndividualEntries();
            }
        }

        private void SelectGroupEntries()
        {
            drawingEntriesIndexes = new int[groups.Length];
            drawingDrawDataIndexes = new int[drawingEntriesIndexes.Length];
            for (var i = 0; i < groups.Length; i++)
            {
                var index = entriesList.IndexOf(groups[i].lastEntry);
                drawingEntriesIndexes[i] = index;
                drawingDrawDataIndexes[i] = index;
            }
        }

        private void SelectIndividualEntries()
        {
            drawingEntriesIndexes = new int[entries.Length];
            drawingDrawDataIndexes = new int[drawingEntriesIndexes.Length];
            for (var i = 0; i < entries.Length; i++)
            {
                drawingEntriesIndexes[i] = i;
                drawingDrawDataIndexes[i] = i;
            }
        }

        private void OnEntryRebuilt( Entry entry )
        {
            var index = Array.IndexOf(entries, entry);
            var entryHeight = entry.height;
            drawDataList[index].height = entryHeight;
            drawData[index].height = entryHeight;
            if (lastContentHeight + entryHeight > viewRect.height &&
                scrollView.position.y + viewRect.height >= contentsRect.height)
            {
                scrollView.ScrollToBottom();
            }
        }

        private void DrawOnlyVisibleEntries( Rect rect, Vector2 scrollPosition )
        {
            lastContentHeight = 0;
            var rectHeight = rect.height;
            for (var i = 0; i < drawingEntriesIndexes.Length; i++)
            {
                try
                {
                    tempEntry = entries[drawingEntriesIndexes[i]];
                }
                catch
                {
                    tempEntry = null;
                }
                var isFiltered = CheckIsEntryFiltered(drawData[drawingDrawDataIndexes[i]]);
                if (!isFiltered)
                {
                    continue;
                }
                var isVisible = lastContentHeight + drawData[drawingDrawDataIndexes[i]].height >= scrollPosition.y &&
                                lastContentHeight < scrollPosition.y + rectHeight;
                if (isVisible || drawData[drawingDrawDataIndexes[i]].justCreated)
                {
                    drawData[drawingDrawDataIndexes[i]].justCreated = false;
                    tempEntry.Draw(lastContentHeight, rect.width, isVisible);
                }
                if (!entryJustRemoved)
                {
                    lastContentHeight += drawData[drawingDrawDataIndexes[i]].height;
                }
                else
                {
                    entryJustRemoved = false;
                }
            }
        }

        private bool CheckIsEntryFiltered( EntryDrawData drawData )
        {
            if (!JxDebug.Setting.useAndFiltering)
            {
                return (currentFilter & drawData.tagsHash) != 0;
            }
            return (currentFilter & drawData.tagsHash) == currentFilter;
        }

        public void AddEntry( EntryData entry )
        {
            if (buffer.Count >= bufferSize)
            {
                Clear();
            }
            if (string.IsNullOrEmpty(entry.stackTrace))
            {
                entry.stackTrace = StackTraceUtility.ExtractStackTrace().Trim();
            }
            if (entry.tags == null)
            {
                entry.tags = new List<string>();
            }
            if (entry.tags.Count == 0 && !entry.tags.Contains(JxDebug.Setting.defaultTag))
            {
                entry.tags.Add(JxDebug.Setting.defaultTag);
            }

            buffer.Add(entry);
        }

        public Entry[] GetEntries()
        {
            return entries;
        }

        public void Clear()
        {
            for (var i = entries.Length - 1; i >= 0; i--)
            {
                RemoveEntry(entries[i]);
            }
            MakeEntriesArray();
            SelectDrawingEntries();
        }

        private void OnEntryRemoved( Entry entry )
        {
            if (lastGroupEntries)
            {
                RemoveWholeGroup(entry.group);
            }
            else
            {
                RemoveEntry(entry);
            }
            MakeEntriesArray();
            SelectDrawingEntries();
        }

        private void RemoveWholeGroup( EntryGroup group )
        {
            var groupEntries = group.entries;
            for (var i = 0; i < groupEntries.Length; i++)
            {
                RemoveEntryAlone(groupEntries[i]);
            }
            RemoveGroup(group);
        }

        private void RemoveEntryAlone( Entry entry )
        {
            var index = entriesList.IndexOf(entry);
            entriesList.Remove(entry);
            drawDataList.RemoveAt(index);
            entryJustRemoved = true;
            RemoveFiltersIfUnused(entry.data.tags.ToArray());
        }

        private void RemoveFiltersIfUnused( string[] tags )
        {
            for (var i = 0; i < tags.Length; i++)
            {
                var numberOfUses = entriesList.Count(x => x.data.tags.Contains(tags[i]));
                if (numberOfUses == 0)
                {
                    var filter = filters.First(x => x.Tag == tags[i]);
                    filtersList.Remove(filter);
                    filters = filtersList.ToArray();
                    var tagHashIndex = (int) Math.Log(filter.Hash, 2);
                    tagHashesUsed[tagHashIndex] = false;
                    if (onFilterRemoved != null)
                    {
                        onFilterRemoved(filter);
                    }
                }
            }
        }

        private void RemoveGroup( EntryGroup group )
        {
            groupsList.Remove(group);
            groups = groupsList.ToArray();
        }

        private void RemoveEntry( Entry entry )
        {
            RemoveEntryAlone(entry);
            entry.group.Remove(entry);
            if (entry.group.entries.Length == 0)
            {
                RemoveGroup(entry.group);
            }
        }

        public void AddFilters( params string[] tags )
        {
            foreach (var tag in tags)
            {
                AddFilter(tag);
            }
        }

        private void AddFilter( string tag )
        {
            var alreadyExists = false;
            for (var i = 0; i < filters.Length; i++)
            {
                if (filters[i].Tag == tag)
                {
                    alreadyExists = true;
                    break;
                }
            }
            if (!alreadyExists)
            {
                for (var i = 0; i < tagHashesUsed.Length; i++)
                {
                    if (tagHashesUsed[i])
                    {
                        continue;
                    }
                    var filter = new Filter(tag, (int) Mathf.Pow(2, i));
                    filtersList.Add(filter);
                    filters = filtersList.ToArray();
                    tagHashesUsed[i] = true;
                    if (onFilterAdded != null)
                    {
                        onFilterAdded(filter);
                    }
                    break;
                }
            }
        }

        public void SetFilter( string tag, bool on )
        {
            for (var i = 0; i < filters.Length; i++)
            {
                if (filters[i].Tag == tag)
                {
                    filters[i].isActive = on;
                    return;
                }
            }
        }

        public Filter[] GetFilters()
        {
            return filters;
        }
    }

    [Serializable]
    internal class EntryDrawData
    {
        public float height;
        public bool justCreated;
        public int tagsHash;
    }
}