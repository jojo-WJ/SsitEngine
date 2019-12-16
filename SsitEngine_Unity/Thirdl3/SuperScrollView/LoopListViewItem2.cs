using UnityEngine;

namespace SuperScrollView
{
    public class LoopListViewItem2 : MonoBehaviour
    {
        private RectTransform mCachedRectTransform;

        public object UserObjectData { get; set; } = null;

        public int UserIntData1 { get; set; } = 0;

        public int UserIntData2 { get; set; } = 0;

        public string UserStringData1 { get; set; } = null;

        public string UserStringData2 { get; set; } = null;

        public float DistanceWithViewPortSnapCenter { get; set; } = 0;

        public float StartPosOffset { get; set; } = 0;

        public int ItemCreatedCheckFrameCount { get; set; } = 0;

        public float Padding { get; set; }

        public RectTransform CachedRectTransform
        {
            get
            {
                if (mCachedRectTransform == null) mCachedRectTransform = gameObject.GetComponent<RectTransform>();
                return mCachedRectTransform;
            }
        }

        public string ItemPrefabName { get; set; }

        public int ItemIndex { get; set; } = -1;

        public int ItemId { get; set; } = -1;


        public bool IsInitHandlerCalled { get; set; } = false;

        public LoopListView2 ParentListView { get; set; } = null;

        public float TopY
        {
            get
            {
                var arrageType = ParentListView.ArrangeType;
                if (arrageType == ListItemArrangeType.TopToBottom)
                    return CachedRectTransform.anchoredPosition3D.y;
                if (arrageType == ListItemArrangeType.BottomToTop)
                    return CachedRectTransform.anchoredPosition3D.y + CachedRectTransform.rect.height;
                return 0;
            }
        }

        public float BottomY
        {
            get
            {
                var arrageType = ParentListView.ArrangeType;
                if (arrageType == ListItemArrangeType.TopToBottom)
                    return CachedRectTransform.anchoredPosition3D.y - CachedRectTransform.rect.height;
                if (arrageType == ListItemArrangeType.BottomToTop) return CachedRectTransform.anchoredPosition3D.y;
                return 0;
            }
        }


        public float LeftX
        {
            get
            {
                var arrageType = ParentListView.ArrangeType;
                if (arrageType == ListItemArrangeType.LeftToRight)
                    return CachedRectTransform.anchoredPosition3D.x;
                if (arrageType == ListItemArrangeType.RightToLeft)
                    return CachedRectTransform.anchoredPosition3D.x - CachedRectTransform.rect.width;
                return 0;
            }
        }

        public float RightX
        {
            get
            {
                var arrageType = ParentListView.ArrangeType;
                if (arrageType == ListItemArrangeType.LeftToRight)
                    return CachedRectTransform.anchoredPosition3D.x + CachedRectTransform.rect.width;
                if (arrageType == ListItemArrangeType.RightToLeft) return CachedRectTransform.anchoredPosition3D.x;
                return 0;
            }
        }

        public float ItemSize
        {
            get
            {
                if (ParentListView.IsVertList)
                    return CachedRectTransform.rect.height;
                return CachedRectTransform.rect.width;
            }
        }

        public float ItemSizeWithPadding => ItemSize + Padding;
    }
}