using System;
using UnityEngine;

namespace JxDebug
{
    [Serializable]
    public class Entry
    {
        public delegate void EntryDelegate( Entry entry );

        private const float spacing = 5;
        private const string namePrefix = "entry";

        private static readonly float alpha = 0.3f;
        private static readonly Color backgroundColor;

        private static int nextId = 1000;
        private static readonly GUIContent foldoutCollapsedContent;
        private static readonly GUIContent foldoutExpandedContent;
        private static readonly GUIContent stackTraceContent;
        private static readonly GUIContent removeContent;
        private readonly SimpleTextBuilder builder = new SimpleTextBuilder();
        private float groupContentWidth;

        private readonly int id;
        private float lastEntriesSpacing;
        private int lastFontSize;
        private int lastGroupContentLength;
        private float lastTextHeight;
        private readonly string name;
        private readonly GUIContent textContent = new GUIContent();
        private Vector2 textSize;
        private readonly GUIStyle textStyle;
        private readonly GUIContent timeStampContent;
        private Vector2 timeStampSize;
        private float unExpadedTextHeight;

        static Entry()
        {
            backgroundColor = Color.black;
            backgroundColor.a = alpha;
            foldoutCollapsedContent = new GUIContent(JxDebug.Setting.entryCollapsedIcon);
            foldoutExpandedContent = new GUIContent(JxDebug.Setting.entryExpandedIcon);
            stackTraceContent = new GUIContent(JxDebug.Setting.stackTraceIcon);
            removeContent = new GUIContent(JxDebug.Setting.removeEntryIcon);
        }

        public Entry( EntryData data )
        {
            id = nextId++;
            name = namePrefix + id;
            data.CutOffExceedingText();
            this.data = data;
            timeStamp = data.timeStamp;
            timeStampContent = new GUIContent(timeStamp);
            textStyle = new GUIStyle(GUIUtils.TextStyle);
        }

        public EntryData data { get; private set; }
        public string timeStamp { get; private set; }
        public bool showStackTrace { get; set; }
        public bool isExpanded { get; set; }
        public EntryGroup group { get; set; }

        public float height => textSize.y + JxDebug.Setting.entriesSpacing * 2;

        public event EntryDelegate onEntryRemoved;
        public event EntryDelegate onEntryRebuilt;

        public void Draw( float positionY, float entryWidth, bool isVisible )
        {
            DoLayout(entryWidth);
            if (!isVisible)
            {
                return;
            }

            GUIUtils.DrawBox(new Rect(0, positionY, entryWidth, height), backgroundColor);
            var rect = new Rect(0, positionY + JxDebug.Setting.entriesSpacing, 0, 0);
            DrawFoldoutToggle(ref rect);
            rect.x += rect.width;
            if (JxDebug.Setting.showTimeStamp)
            {
                DrawTimeStamp(ref rect);
                rect.x += rect.width;
                rect.x += spacing;
            }
            if (data.icon != null)
            {
                DrawIcon(ref rect);
                rect.x += rect.width;
                rect.x += spacing;
            }
            DrawText(ref rect, textSize.x);
            rect.x += rect.width;
            if (JxDebug.Setting.groupIdenticalEntries)
            {
                DrawGroupContent(ref rect);
                rect.x += rect.width;
            }
            DrawStackTraceToggle(ref rect);
            rect.x += rect.width;
            DrawRemoveButton(ref rect);
        }

        private void DoLayout( float entryWidth )
        {
            //The styled is applied to calculate sizes properly
            ApplyStyle();
            builder.RebuildIfNecessary(this, textSize.x);
            SetContentText();
            RestoreStyle();

            if (lastFontSize != JxDebug.Setting.fontSize)
            {
                lastFontSize = JxDebug.Setting.fontSize;
                timeStampSize = textStyle.CalcSize(timeStampContent);
                groupContentWidth = textStyle.CalcSize(group.content).x;
            }
            else if (lastGroupContentLength != group.content.text.Length)
            {
                lastGroupContentLength = group.content.text.Length;
                groupContentWidth = textStyle.CalcSize(group.content).x;
            }

            CalculateTextSize(entryWidth);

            if (!isExpanded && !showStackTrace)
            {
                unExpadedTextHeight = textSize.y;
            }

            var alreadyRebuilt = false;
            if (JxDebug.Setting.entriesSpacing != lastEntriesSpacing)
            {
                lastEntriesSpacing = JxDebug.Setting.entriesSpacing;
                onEntryRebuilt(this);
                alreadyRebuilt = true;
            }
            if (textSize.y != lastTextHeight)
            {
                lastTextHeight = textSize.y;
                if (!alreadyRebuilt)
                {
                    onEntryRebuilt(this);
                }
            }
        }

        private void CalculateTextSize( float entryWidth )
        {
            textSize.x = CalculateTextWidth(entryWidth);
            textSize.y = textStyle.CalcHeight(textContent, textSize.x);
        }

        private float CalculateTextWidth( float entryWidth )
        {
            entryWidth -= foldoutCollapsedContent.image.width;
            entryWidth -= stackTraceContent.image.width;
            entryWidth -= removeContent.image.width;
            if (JxDebug.Setting.showTimeStamp)
            {
                entryWidth -= timeStampSize.x;
                entryWidth -= spacing;
            }
            if (data.icon != null)
            {
                entryWidth -= Mathf.Min(data.icon.height, unExpadedTextHeight);
                entryWidth -= spacing;
            }
            if (JxDebug.Setting.groupIdenticalEntries)
            {
                entryWidth -= groupContentWidth;
            }
            return entryWidth;
        }

        private void SetContentText()
        {
            textContent.text = isExpanded ? data.text : builder.simpleText;
            if (showStackTrace)
            {
                textContent.text += "\n\n" + data.stackTrace;
            }
        }

        private void DrawFoldoutToggle( ref Rect rect )
        {
            var oldY = rect.y;
            rect.width = foldoutCollapsedContent.image.width;
            rect.height = height - textSize.y + timeStampSize.y - JxDebug.Setting.entriesSpacing * 2;
            var expand = false;
            if (builder.needsExpandToggle)
            {
                expand = isExpanded;
                if (GUIUtils.DrawCenteredButton(rect, isExpanded ? foldoutExpandedContent : foldoutCollapsedContent,
                    Color.clear))
                {
                    expand = !isExpanded;
                }
            }
            isExpanded = expand;
            rect.y = oldY;
        }

        private void DrawTimeStamp( ref Rect rect )
        {
            rect.width = timeStampSize.x;
            rect.height = height;
            GUI.Label(rect, timeStampContent, textStyle);
        }

        private void DrawIcon( ref Rect rect )
        {
            rect.width = rect.height = Mathf.Min(data.icon.height, unExpadedTextHeight);
            GUI.DrawTexture(rect, data.icon, ScaleMode.ScaleToFit);
        }

        private void DrawText( ref Rect rect, float width )
        {
            rect.width = width;
            rect.height = height;

            GUIUtility.GetControlID(id, FocusType.Keyboard);
            ApplyStyle();
            GUI.SetNextControlName(name);
            if (Application.isMobilePlatform)
            {
                GUI.Label(rect, textContent.text, textStyle);
            }
            else
            {
                GUI.TextField(rect, textContent.text, textStyle);
            }
            RestoreStyle();
        }

        private void ApplyStyle()
        {
            textStyle.fontStyle = data.options.style;
            if (data.options.size > 0)
            {
                textStyle.fontSize = data.options.size;
            }
            textStyle.normal.textColor = data.options.color;
        }

        private void RestoreStyle()
        {
            textStyle.fontStyle = GUIUtils.TextStyle.fontStyle;
            textStyle.fontSize = GUIUtils.TextStyle.fontSize;
            textStyle.normal.textColor = GUIUtils.TextStyle.normal.textColor;
        }

        private void DrawGroupContent( ref Rect rect )
        {
            rect.width = groupContentWidth;
            rect.height = height;
            GUI.Label(rect, group.content, textStyle);
        }

        private void DrawStackTraceToggle( ref Rect rect )
        {
            rect.width = stackTraceContent.image.width;
            rect.height = height - textSize.y + timeStampSize.y - JxDebug.Setting.entriesSpacing * 2;
            if (GUIUtils.DrawCenteredButton(rect, stackTraceContent, Color.clear))
            {
                showStackTrace = !showStackTrace;
            }
        }

        private void DrawRemoveButton( ref Rect rect )
        {
            rect.width = removeContent.image.width;
            rect.height = height - textSize.y + timeStampSize.y - JxDebug.Setting.entriesSpacing * 2;
            if (GUIUtils.DrawCenteredButton(rect, removeContent, Color.clear))
            {
                Remove();
            }
        }

        public void Remove()
        {
            onEntryRemoved(this);
        }

        [Serializable]
        private class SimpleTextBuilder
        {
            private const string toBeContinuedText = " [...]";
            private readonly GUIContent content = new GUIContent();
            public bool needsExpandToggle;
            private float realTextWidth;
            public string simpleText = string.Empty;

            private float widthReservedForText;

            //Entry is received as a method parameter instead of as a constructor parameter to avoid Unity recursive serialization problems
            public void RebuildIfNecessary( Entry entry, float widthReservedForText )
            {
                var realTextWidth = entry.textStyle.CalcSize(entry.textContent).x;
                if (Event.current.type == EventType.Repaint &&
                    (!Mathf.Approximately(widthReservedForText, this.widthReservedForText) ||
                     realTextWidth != this.realTextWidth))
                {
                    this.realTextWidth = realTextWidth;
                    this.widthReservedForText = widthReservedForText;
                    Rebuild(entry);
                }
            }

            private void Rebuild( Entry entry )
            {
                needsExpandToggle = NeedsExpandToggle(entry);
                simpleText = needsExpandToggle ? BuildSimpleText(entry) : entry.data.text;
            }

            private bool NeedsExpandToggle( Entry entry )
            {
                //Size needs to be recalculated based on the whole data.text, not only textContent.text
                content.text = entry.data.text;
                var size = entry.textStyle.CalcSize(content);
                return size.x > widthReservedForText || content.text.Contains("\n") || content.text.Contains("\r");
            }

            private string BuildSimpleText( Entry entry )
            {
                Vector2 size;
                content.text = toBeContinuedText;
                var lastSimpleText = content.text;

                for (var i = 0; i < entry.data.text.Length; i++)
                {
                    content.text = string.Concat(entry.data.text.Substring(0, i), toBeContinuedText);
                    size = entry.textStyle.CalcSize(content);
                    if (size.x > widthReservedForText)
                    {
                        break;
                    }
                    lastSimpleText = content.text;
                    if (entry.data.text[i] == '\n' || entry.data.text[i] == '\r')
                    {
                        break;
                    }
                }
                return lastSimpleText;
            }
        }
    }
}