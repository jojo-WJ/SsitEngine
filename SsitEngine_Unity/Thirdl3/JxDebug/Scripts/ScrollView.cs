using System;
using UnityEngine;

namespace JxDebug
{
    [Serializable]
    public class ScrollView
    {
        public delegate void ScrollViewDrawer( Rect rect, Vector2 scrollPosition );

        private Rect contentsRect;

        [HideInInspector] public Vector2 position;

        [SerializeField] protected ScrollBar scrollBar;

        private float? targetScrollTo;
        private Rect viewRect;

        public bool isScrollbarVisible => contentsRect.height > viewRect.height;
        public float scrollBarWidth => scrollBar.width;

        public void Draw( Rect viewRect, Rect contentsRect, ScrollViewDrawer contentsDrawer )
        {
            this.viewRect = viewRect;
            this.contentsRect = contentsRect;
            HandleScrollWheel();
            DrawVerticalScrollBar();
            DrawViewRect(contentsDrawer);
            if (targetScrollTo.HasValue)
            {
                DoScrollToTarget();
            }
        }

        private void HandleScrollWheel()
        {
            if (isScrollbarVisible)
            {
                if (Event.current.type != EventType.ScrollWheel || !viewRect.Contains(Event.current.mousePosition))
                {
                    return;
                }
                position.y += scrollBar.sensitivity * Event.current.delta.y;
                Event.current.Use();
            }
        }

        private void DrawVerticalScrollBar()
        {
            if (isScrollbarVisible)
            {
                viewRect.width -= scrollBar.width;
                position.y =
                    scrollBar.Draw(new Rect(viewRect.x + viewRect.width, viewRect.y, scrollBar.width, viewRect.height),
                        position.y, contentsRect.height);
            }
        }

        private void DrawViewRect( ScrollViewDrawer contentsDrawer )
        {
            GUI.BeginGroup(viewRect);
            contentsRect.y -= viewRect.y;
            contentsRect.y -= position.y;
            GUI.BeginGroup(contentsRect);
            contentsDrawer(viewRect, position);
            GUI.EndGroup();
            GUI.EndGroup();
        }

        private void DoScrollToTarget()
        {
            position.y = targetScrollTo.Value;
            targetScrollTo = null;
        }

        public void ScrollToBottom()
        {
            ScrollTo(float.PositiveInfinity);
        }

        public void ScrollToTop()
        {
            ScrollTo(0);
        }

        public void ScrollTo( float position )
        {
            targetScrollTo = position;
        }
    }
}