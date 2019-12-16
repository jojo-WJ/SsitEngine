using System;
using System.Collections.Generic;
using UnityEngine;

namespace JxDebug
{
    [Serializable]
    public abstract class Window<T, TCurrent>
    {
        protected const float padding = 5;
        protected const float titleHeight = 16;

        [SerializeField]
        protected ScrollView scrollView;
        [SerializeField]
        [Tooltip("A value of -1 indicates no limit")]
        protected int capacity;
        [SerializeField]
        [Tooltip("A value of -1 indicates the whole screen")]
        protected float minWidth;
        [SerializeField]
        [Tooltip("If checked, the window will adapt to its contents")]
        protected bool expandToFitContents;
        [SerializeField]
        protected int entriesToShow;

        protected List<T> entries = new List<T>();
        float entryHeight;
        Rect rect;
        GUIContent closeButtonContent;
        GUIContent titleContent;
        Color backgroundColor;
        Color entriesColor;
        int lastFontSize = -1;
        float largestEntry;
        int _currentIndex;
        protected abstract GUIContent Title { get; }

        public abstract TCurrent Current { get; }

        public bool IsOpen { get; private set; }

        protected virtual int Size
        {
            get
            {
                return entries.Count;
            }
        }

        public float Height
        {
            get
            {
                return entryHeight * Mathf.Min(entriesToShow, Mathf.Max(0, Size)) + titleHeight;
            }
        }

        public float ViewHeight
        {
            get
            {
                return Height - titleHeight;
            }
        }

        public float ContentsHeight
        {
            get
            {
                return entryHeight * Size;
            }
        }

        protected int CurrentIndex
        {
            get { return _currentIndex; }
            private set
            {
                _currentIndex = value;
                ClampScrollPosition();
            }
        }

        public void Initialize()
        {
            closeButtonContent = new GUIContent(JxDebug.Setting.closeWindowIcon);
            titleContent = Title;
            OnInitialized();
        }

        protected virtual void OnInitialized() { }

        void ClampScrollPosition()
        {
            float indexPosition = entryHeight * CurrentIndex;
            //Ensures the selected index is always shown in the window
            scrollView.position.y = Mathf.Clamp(scrollView.position.y, indexPosition - ViewHeight + (ContentsHeight > ViewHeight ? entryHeight : 0), indexPosition);
        }

        public void AddRange( T[] entries )
        {
            for (int i = 0; i < entries.Length; i++)
                AddInternal(entries[i]);
            NavigateToLast();
        }

        public void Add( T entry )
        {
            AddInternal(entry);
            NavigateToLast();
        }

        void AddInternal( T entry )
        {
            entries.Add(entry);
            if (capacity != -1 && entries.Count > capacity)
                entries.RemoveAt(0);
        }

        public virtual void Navigate( int direction )
        {
            NavigateTo(CurrentIndex + direction);
        }

        public void NavigateTo( int index )
        {
            CurrentIndex = Mathf.Clamp(index, 0, Mathf.Max(0, entries.Count - 1));
        }

        public void NavigateToLast()
        {
            NavigateTo(entries.Count - 1);
        }

        public void NavigateToFirst()
        {
            NavigateTo(0);
        }

        public void Clear()
        {
            entries.Clear();
            NavigateToFirst();
        }

        public void Draw( Vector2 position )
        {
            if (!IsOpen)
                return;

            float width = CalculateWidth(position);
            if (JxDebug.Setting.fontSize != lastFontSize)
            {
                entryHeight = GUIUtils.TextStyle.CalcHeight(new GUIContent(), width);
                lastFontSize = JxDebug.Setting.fontSize;
            }
            rect = new Rect(position.x, position.y - Height, width, Height);
            GUI.Window(0, rect, DrawWindow, GUIContent.none, GUIStyle.none);
        }

        float CalculateWidth( Vector2 position )
        {
            float maxWidth = Screen.width / JxDebug.Setting.scale - position.x;
            float width = minWidth;
            if (minWidth == -1)
                width = maxWidth;
            else if (expandToFitContents)
            {
                width = Mathf.Max(minWidth, largestEntry) + padding;
                if (scrollView.isScrollbarVisible)
                    width += scrollView.scrollBarWidth;
            }
            width = Mathf.Min(maxWidth, width);
            return width;
        }

        void DrawWindow( int id )
        {
            if (Event.current.type == EventType.Layout)
                return;
            rect.x = rect.y = 0;
            GUIUtils.DrawBox(rect, backgroundColor);
            DrawTitle();
            rect.y += titleHeight;
            rect.height -= titleHeight;
            ConstructColors();
            Rect contentsRect = new Rect(rect.x, rect.y, rect.width, ContentsHeight);
            scrollView.Draw(rect, contentsRect, DrawOnlyVisibleEntries);
        }

        void DrawTitle()
        {
            float width = rect.width;
            float height = rect.height;
            rect.height = titleHeight;
            GUI.Label(rect, titleContent, GUIUtils.CenteredTextStyle);
            rect.width = titleHeight;
            rect.x = width - rect.width;
            if (GUIUtils.DrawButton(rect, closeButtonContent, Color.clear))
                Close();
            rect.x = 0;
            rect.width = width;
            rect.height = height;
        }

        void ConstructColors()
        {
            float h, s, v, backGroundV, entriesV;
            h = s = v = backGroundV = entriesV = 0;
            ColorUtils.RGBToHSV(JxDebug.Setting.mainColor, out h, out s, out v);
            backGroundV = v * 0.4f;
            backgroundColor = ColorUtils.HSVToRGB(h, s, backGroundV);
            entriesV = v * 0.6f;
            entriesColor = ColorUtils.HSVToRGB(h, s, entriesV);
        }

        void DrawOnlyVisibleEntries( Rect rect, Vector2 scrollPosition )
        {
            OnBeginDraw();
            largestEntry = 0;

            int size = this.Size;
            for (int i = 0; i < size; i++)
            {
                float positionY = entryHeight * i;
                bool isVisible = positionY + entryHeight > scrollPosition.y && positionY < scrollPosition.y + rect.height;
                if (!isVisible)
                    continue;

                largestEntry = Mathf.Max(GetEntryWidth(entries[i]), largestEntry);
                Rect entryRect = new Rect(0, positionY, Mathf.Max(largestEntry, rect.width) + padding, entryHeight);
                if (GUIUtils.DrawButton(entryRect, GUIContent.none, CurrentIndex == i ? JxDebug.Setting.secondaryColor : entriesColor))
                {
                    NavigateTo(i);
                    OnEntryClicked(entries[i]);
                }
                entryRect.x = padding;
                DrawEntry(entryRect, entries[i]);
            }
            largestEntry += padding;
        }

        protected virtual void OnBeginDraw() { }
        protected abstract void OnEntryClicked( T entry );
        protected abstract void DrawEntry( Rect rect, T entry );
        protected abstract float GetEntryWidth( T entry );

        public void Open()
        {
            IsOpen = true;
            OnOpen();
        }

        public void Close()
        {
            IsOpen = false;
        }

        public void ToggleOpen()
        {
            if (IsOpen)
                Close();
            else
                Open();
        }

        protected virtual void OnOpen() { }
    }
}