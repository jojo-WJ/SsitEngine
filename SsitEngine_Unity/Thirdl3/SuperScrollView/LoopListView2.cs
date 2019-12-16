using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace SuperScrollView
{
    public enum ItemCornerEnum
    {
        LeftBottom = 0,
        LeftTop,
        RightTop,
        RightBottom
    }


    public enum ListItemArrangeType
    {
        TopToBottom,
        BottomToTop,
        LeftToRight,
        RightToLeft
    }

    public class ItemPool
    {
        private static int mCurItemIdCount;
        private int mInitCreateCount = 1;
        private RectTransform mItemParent;
        private float mPadding;
        private readonly List<LoopListViewItem2> mPooledItemList = new List<LoopListViewItem2>();
        private string mPrefabName;
        private GameObject mPrefabObj;
        private float mStartPosOffset;
        private readonly List<LoopListViewItem2> mTmpPooledItemList = new List<LoopListViewItem2>();

        public void Init( GameObject prefabObj, float padding, float startPosOffset, int createCount,
            RectTransform parent )
        {
            mPrefabObj = prefabObj;
            mPrefabName = mPrefabObj.name;
            mInitCreateCount = createCount;
            mPadding = padding;
            mStartPosOffset = startPosOffset;
            mItemParent = parent;
            mPrefabObj.SetActive(false);
            for (var i = 0; i < mInitCreateCount; ++i)
            {
                var tViewItem = CreateItem();
                RecycleItemReal(tViewItem);
            }
        }

        public LoopListViewItem2 GetItem()
        {
            mCurItemIdCount++;
            LoopListViewItem2 tItem = null;
            if (mTmpPooledItemList.Count > 0)
            {
                var count = mTmpPooledItemList.Count;
                tItem = mTmpPooledItemList[count - 1];
                mTmpPooledItemList.RemoveAt(count - 1);
                tItem.gameObject.SetActive(true);
            }
            else
            {
                var count = mPooledItemList.Count;
                if (count == 0)
                {
                    tItem = CreateItem();
                }
                else
                {
                    tItem = mPooledItemList[count - 1];
                    mPooledItemList.RemoveAt(count - 1);
                    tItem.gameObject.SetActive(true);
                }
            }
            tItem.Padding = mPadding;
            tItem.ItemId = mCurItemIdCount;
            return tItem;
        }

        public void DestroyAllItem()
        {
            ClearTmpRecycledItem();
            var count = mPooledItemList.Count;
            for (var i = 0; i < count; ++i) Object.DestroyImmediate(mPooledItemList[i].gameObject);
            mPooledItemList.Clear();
        }

        public LoopListViewItem2 CreateItem()
        {
            var go = Object.Instantiate(mPrefabObj, Vector3.zero, Quaternion.identity, mItemParent);
            go.SetActive(true);
            var rf = go.GetComponent<RectTransform>();
            rf.localScale = Vector3.one;
            rf.anchoredPosition3D = Vector3.zero;
            rf.localEulerAngles = Vector3.zero;
            var tViewItem = go.GetComponent<LoopListViewItem2>();
            tViewItem.ItemPrefabName = mPrefabName;
            tViewItem.StartPosOffset = mStartPosOffset;
            return tViewItem;
        }

        private void RecycleItemReal( LoopListViewItem2 item )
        {
            item.gameObject.SetActive(false);
            mPooledItemList.Add(item);
        }

        public void RecycleItem( LoopListViewItem2 item )
        {
            mTmpPooledItemList.Add(item);
        }

        public void ClearTmpRecycledItem()
        {
            var count = mTmpPooledItemList.Count;
            if (count == 0) return;
            for (var i = 0; i < count; ++i) RecycleItemReal(mTmpPooledItemList[i]);
            mTmpPooledItemList.Clear();
        }
    }

    [Serializable]
    public class ItemPrefabConfData
    {
        public int mInitCreateCount;
        public GameObject mItemPrefab;
        public float mPadding;
        public float mStartPosOffset;
    }


    public class LoopListViewInitParam
    {
        public float mDistanceForNew0 = 200;

        public float mDistanceForNew1 = 200;

        // all the default values
        public float mDistanceForRecycle0 = 300; //mDistanceForRecycle0 should be larger than mDistanceForNew0
        public float mDistanceForRecycle1 = 300; //mDistanceForRecycle1 should be larger than mDistanceForNew1
        public float mItemDefaultWithPaddingSize = 20; //item's default size (with padding)
        public float mSmoothDumpRate = 0.3f;
        public float mSnapFinishThreshold = 0.01f;
        public float mSnapVecThreshold = 145;

        public static LoopListViewInitParam CopyDefaultInitParam()
        {
            return new LoopListViewInitParam();
        }
    }

    public enum SnapStatus
    {
        NoTargetSet = 0,
        TargetHasSet = 1,
        SnapMoving = 2,
        SnapMoveFinish = 3
    }


    public class LoopListView2 : MonoBehaviour, IBeginDragHandler, IEndDragHandler, IDragHandler
    {
        private Vector2 mAdjustedVec;

        private int mCurReadyMaxItemIndex;
        private int mCurReadyMinItemIndex;
        private readonly SnapData mCurSnapData = new SnapData();
        private float mDistanceForNew0 = 200;
        private float mDistanceForNew1 = 200;
        private float mDistanceForRecycle0 = 300;
        private float mDistanceForRecycle1 = 300;
        private float mItemDefaultWithPaddingSize = 20;

        private readonly List<LoopListViewItem2> mItemList = new List<LoopListViewItem2>();

        private readonly Dictionary<string, ItemPool> mItemPoolDict = new Dictionary<string, ItemPool>();
        private readonly List<ItemPool> mItemPoolList = new List<ItemPool>();
        private ItemPosMgr mItemPosMgr;

        [SerializeField] private List<ItemPrefabConfData> mItemPrefabDataList = new List<ItemPrefabConfData>();

        [SerializeField] private Vector2 mItemSnapPivot = Vector2.zero;

        private readonly Vector3[] mItemWorldCorners = new Vector3[4];


        private Vector3 mLastFrameContainerPos = Vector3.zero;
        private int mLastItemIndex;
        private float mLastItemPadding;
        private Vector3 mLastSnapCheckPos = Vector3.zero;
        private int mLeftSnapUpdateExtraCount = 1;
        private int mListUpdateCheckFrameCount;
        private bool mListViewInited;
        private bool mNeedAdjustVec;
        private bool mNeedCheckNextMaxItem = true;
        private bool mNeedCheckNextMinItem = true;
        public Action mOnBeginDragAction;
        public Action mOnDragingAction;
        public Action mOnEndDragAction;
        private Func<LoopListView2, int, LoopListViewItem2> mOnGetItemByIndex;
        public Action<LoopListView2, LoopListViewItem2> mOnSnapItemFinished;
        public Action<LoopListView2, LoopListViewItem2> mOnSnapNearestChanged;
        private PointerEventData mPointerEventData;
        private ClickEventListener mScrollBarClickEventListener;
        private RectTransform mScrollRectTransform;
        private float mSmoothDumpRate = 0.3f;
        private float mSmoothDumpVel;
        private float mSnapFinishThreshold = 0.1f;
        private float mSnapVecThreshold = 145;

        private readonly Vector3[] mViewPortRectLocalCorners = new Vector3[4];
        private RectTransform mViewPortRectTransform;

        [SerializeField] private Vector2 mViewPortSnapPivot = Vector2.zero;

        [field: SerializeField] public ListItemArrangeType ArrangeType { get; set; } = ListItemArrangeType.TopToBottom;

        public bool IsVertList { get; private set; }

        public int ItemTotalCount { get; private set; }

        public RectTransform ContainerTrans { get; private set; }

        public ScrollRect ScrollRect { get; private set; }

        public bool IsDraging { get; private set; }

        [field: SerializeField] public bool ItemSnapEnable { get; set; } = false;

        [field: SerializeField] public bool SupportScrollBar { get; set; } = true;

        public int ShownItemCount => mItemList.Count;

        public float ViewPortSize
        {
            get
            {
                if (IsVertList)
                    return mViewPortRectTransform.rect.height;
                return mViewPortRectTransform.rect.width;
            }
        }

        public float ViewPortWidth => mViewPortRectTransform.rect.width;

        public float ViewPortHeight => mViewPortRectTransform.rect.height;

        //Get the nearest item index with the viewport snap point.
        public int CurSnapNearestItemIndex { get; private set; } = -1;


        public virtual void OnBeginDrag( PointerEventData eventData )
        {
            if (eventData.button != PointerEventData.InputButton.Left) return;
            IsDraging = true;
            CacheDragPointerEventData(eventData);
            mCurSnapData.Clear();
            if (mOnBeginDragAction != null) mOnBeginDragAction();
        }

        public virtual void OnDrag( PointerEventData eventData )
        {
            if (eventData.button != PointerEventData.InputButton.Left) return;
            CacheDragPointerEventData(eventData);
            if (mOnDragingAction != null) mOnDragingAction();
        }

        public virtual void OnEndDrag( PointerEventData eventData )
        {
            if (eventData.button != PointerEventData.InputButton.Left) return;
            IsDraging = false;
            mPointerEventData = null;
            if (mOnEndDragAction != null) mOnEndDragAction();
            ForceSnapUpdateCheck();
        }

        public ItemPrefabConfData GetItemPrefabConfData( string prefabName )
        {
            foreach (var data in mItemPrefabDataList)
            {
                if (data.mItemPrefab == null)
                {
                    Debug.LogError("A item prefab is null ");
                    continue;
                }
                if (prefabName == data.mItemPrefab.name) return data;
            }
            return null;
        }

        public void OnItemPrefabChanged( string prefabName )
        {
            var data = GetItemPrefabConfData(prefabName);
            if (data == null) return;
            ItemPool pool = null;
            if (mItemPoolDict.TryGetValue(prefabName, out pool) == false) return;
            var firstItemIndex = -1;
            var pos = Vector3.zero;
            if (mItemList.Count > 0)
            {
                firstItemIndex = mItemList[0].ItemIndex;
                pos = mItemList[0].CachedRectTransform.anchoredPosition3D;
            }
            RecycleAllItem();
            ClearAllTmpRecycledItem();
            pool.DestroyAllItem();
            pool.Init(data.mItemPrefab, data.mPadding, data.mStartPosOffset, data.mInitCreateCount, ContainerTrans);
            if (firstItemIndex >= 0) RefreshAllShownItemWithFirstIndexAndPos(firstItemIndex, pos);
        }

        /*
        InitListView method is to initiate the LoopListView2 component. There are 3 parameters:
        itemTotalCount: the total item count in the listview. If this parameter is set -1, then means there are infinite items, and scrollbar would not be supported, and the ItemIndex can be from –MaxInt to +MaxInt. If this parameter is set a value >=0 , then the ItemIndex can only be from 0 to itemTotalCount -1.
        onGetItemByIndex: when a item is getting in the scrollrect viewport, and this Action will be called with the item’ index as a parameter, to let you create the item and update its content.
        */
        public void InitListView( int itemTotalCount,
            Func<LoopListView2, int, LoopListViewItem2> onGetItemByIndex,
            LoopListViewInitParam initParam = null )
        {
            if (initParam != null)
            {
                mDistanceForRecycle0 = initParam.mDistanceForRecycle0;
                mDistanceForNew0 = initParam.mDistanceForNew0;
                mDistanceForRecycle1 = initParam.mDistanceForRecycle1;
                mDistanceForNew1 = initParam.mDistanceForNew1;
                mSmoothDumpRate = initParam.mSmoothDumpRate;
                mSnapFinishThreshold = initParam.mSnapFinishThreshold;
                mSnapVecThreshold = initParam.mSnapVecThreshold;
                mItemDefaultWithPaddingSize = initParam.mItemDefaultWithPaddingSize;
            }
            ScrollRect = gameObject.GetComponent<ScrollRect>();
            if (ScrollRect == null)
            {
                Debug.LogError("ListView Init Failed! ScrollRect component not found!");
                return;
            }
            if (mDistanceForRecycle0 <= mDistanceForNew0)
                Debug.LogError("mDistanceForRecycle0 should be bigger than mDistanceForNew0");
            if (mDistanceForRecycle1 <= mDistanceForNew1)
                Debug.LogError("mDistanceForRecycle1 should be bigger than mDistanceForNew1");
            mCurSnapData.Clear();
            mItemPosMgr = new ItemPosMgr(mItemDefaultWithPaddingSize);
            mScrollRectTransform = ScrollRect.GetComponent<RectTransform>();
            ContainerTrans = ScrollRect.content;
            mViewPortRectTransform = ScrollRect.viewport;
            if (mViewPortRectTransform == null) mViewPortRectTransform = mScrollRectTransform;
            if (ScrollRect.horizontalScrollbarVisibility == ScrollRect.ScrollbarVisibility.AutoHideAndExpandViewport &&
                ScrollRect.horizontalScrollbar != null)
                Debug.LogError("ScrollRect.horizontalScrollbarVisibility cannot be set to AutoHideAndExpandViewport");
            if (ScrollRect.verticalScrollbarVisibility == ScrollRect.ScrollbarVisibility.AutoHideAndExpandViewport &&
                ScrollRect.verticalScrollbar != null)
                Debug.LogError("ScrollRect.verticalScrollbarVisibility cannot be set to AutoHideAndExpandViewport");
            IsVertList = ArrangeType == ListItemArrangeType.TopToBottom ||
                         ArrangeType == ListItemArrangeType.BottomToTop;
            ScrollRect.horizontal = !IsVertList;
            ScrollRect.vertical = IsVertList;
            SetScrollbarListener();
            AdjustPivot(mViewPortRectTransform);
            AdjustAnchor(ContainerTrans);
            AdjustContainerPivot(ContainerTrans);
            InitItemPool();
            mOnGetItemByIndex = onGetItemByIndex;
            if (mListViewInited) Debug.LogError("LoopListView2.InitListView method can be called only once.");
            mListViewInited = true;
            ResetListView();
            //SetListItemCount(itemTotalCount, true);
            mCurSnapData.Clear();
            ItemTotalCount = itemTotalCount;
            if (ItemTotalCount < 0) SupportScrollBar = false;
            if (SupportScrollBar)
                mItemPosMgr.SetItemMaxCount(ItemTotalCount);
            else
                mItemPosMgr.SetItemMaxCount(0);
            mCurReadyMaxItemIndex = 0;
            mCurReadyMinItemIndex = 0;
            mLeftSnapUpdateExtraCount = 1;
            mNeedCheckNextMaxItem = true;
            mNeedCheckNextMinItem = true;
            UpdateContentSize();
        }

        private void SetScrollbarListener()
        {
            mScrollBarClickEventListener = null;
            Scrollbar curScrollBar = null;
            if (IsVertList && ScrollRect.verticalScrollbar != null) curScrollBar = ScrollRect.verticalScrollbar;
            if (!IsVertList && ScrollRect.horizontalScrollbar != null) curScrollBar = ScrollRect.horizontalScrollbar;
            if (curScrollBar == null) return;
            var listener = ClickEventListener.Get(curScrollBar.gameObject);
            mScrollBarClickEventListener = listener;
            listener.SetPointerUpHandler(OnPointerUpInScrollBar);
            listener.SetPointerDownHandler(OnPointerDownInScrollBar);
        }

        private void OnPointerDownInScrollBar( GameObject obj )
        {
            mCurSnapData.Clear();
        }

        private void OnPointerUpInScrollBar( GameObject obj )
        {
            ForceSnapUpdateCheck();
        }

        public void ResetListView( bool resetPos = true )
        {
            mViewPortRectTransform.GetLocalCorners(mViewPortRectLocalCorners);
            if (resetPos) ContainerTrans.anchoredPosition3D = Vector3.zero;
            ForceSnapUpdateCheck();
        }


        /*
        This method may use to set the item total count of the scrollview at runtime. 
        If this parameter is set -1, then means there are infinite items,
        and scrollbar would not be supported, and the ItemIndex can be from –MaxInt to +MaxInt. 
        If this parameter is set a value >=0 , then the ItemIndex can only be from 0 to itemTotalCount -1.  
        If resetPos is set false, then the scrollrect’s content position will not changed after this method finished.
        */
        public void SetListItemCount( int itemCount, bool resetPos = true )
        {
            if (itemCount == ItemTotalCount) return;
            mCurSnapData.Clear();
            ItemTotalCount = itemCount;
            if (ItemTotalCount < 0) SupportScrollBar = false;
            if (SupportScrollBar)
                mItemPosMgr.SetItemMaxCount(ItemTotalCount);
            else
                mItemPosMgr.SetItemMaxCount(0);
            if (ItemTotalCount == 0)
            {
                mCurReadyMaxItemIndex = 0;
                mCurReadyMinItemIndex = 0;
                mNeedCheckNextMaxItem = false;
                mNeedCheckNextMinItem = false;
                RecycleAllItem();
                ClearAllTmpRecycledItem();
                UpdateContentSize();
                return;
            }
            if (mCurReadyMaxItemIndex >= ItemTotalCount) mCurReadyMaxItemIndex = ItemTotalCount - 1;
            mLeftSnapUpdateExtraCount = 1;
            mNeedCheckNextMaxItem = true;
            mNeedCheckNextMinItem = true;
            if (resetPos)
            {
                MovePanelToItemIndex(0, 0);
                return;
            }
            if (mItemList.Count == 0)
            {
                MovePanelToItemIndex(0, 0);
                return;
            }
            var maxItemIndex = ItemTotalCount - 1;
            var lastItemIndex = mItemList[mItemList.Count - 1].ItemIndex;
            if (lastItemIndex <= maxItemIndex)
            {
                UpdateContentSize();
                UpdateAllShownItemsPos();
                return;
            }
            MovePanelToItemIndex(maxItemIndex, 0);
        }

        //To get the visible item by itemIndex. If the item is not visible, then this method return null.
        public LoopListViewItem2 GetShownItemByItemIndex( int itemIndex )
        {
            var count = mItemList.Count;
            if (count == 0) return null;
            if (itemIndex < mItemList[0].ItemIndex || itemIndex > mItemList[count - 1].ItemIndex) return null;
            var i = itemIndex - mItemList[0].ItemIndex;
            return mItemList[i];
        }


        /*
         All visible items is stored in a List<LoopListViewItem2> , which is named mItemList;
         this method is to get the visible item by the index in visible items list. The parameter index is from 0 to mItemList.Count.
        */
        public LoopListViewItem2 GetShownItemByIndex( int index )
        {
            var count = mItemList.Count;
            if (index < 0 || index >= count) return null;
            return mItemList[index];
        }

        public LoopListViewItem2 GetShownItemByIndexWithoutCheck( int index )
        {
            return mItemList[index];
        }

        public int GetIndexInShownItemList( LoopListViewItem2 item )
        {
            if (item == null) return -1;
            var count = mItemList.Count;
            if (count == 0) return -1;
            for (var i = 0; i < count; ++i)
                if (mItemList[i] == item)
                    return i;
            return -1;
        }


        public void DoActionForEachShownItem( Action<LoopListViewItem2, object> action, object param )
        {
            if (action == null) return;
            var count = mItemList.Count;
            if (count == 0) return;
            for (var i = 0; i < count; ++i) action(mItemList[i], param);
        }


        public LoopListViewItem2 NewListViewItem( string itemPrefabName )
        {
            ItemPool pool = null;
            if (mItemPoolDict.TryGetValue(itemPrefabName, out pool) == false) return null;
            var item = pool.GetItem();
            var rf = item.GetComponent<RectTransform>();
            rf.SetParent(ContainerTrans);
            rf.localScale = Vector3.one;
            rf.anchoredPosition3D = Vector3.zero;
            rf.localEulerAngles = Vector3.zero;
            item.ParentListView = this;
            return item;
        }

        /*
        For a vertical scrollrect, when a visible item’s height changed at runtime, then this method should be called to let the LoopListView2 component reposition all visible items’ position.
        For a horizontal scrollrect, when a visible item’s width changed at runtime, then this method should be called to let the LoopListView2 component reposition all visible items’ position.
        */
        public void OnItemSizeChanged( int itemIndex )
        {
            var item = GetShownItemByItemIndex(itemIndex);
            if (item == null) return;
            if (SupportScrollBar)
            {
                if (IsVertList)
                    SetItemSize(itemIndex, item.CachedRectTransform.rect.height, item.Padding);
                else
                    SetItemSize(itemIndex, item.CachedRectTransform.rect.width, item.Padding);
            }
            UpdateContentSize();
            UpdateAllShownItemsPos();
        }


        /*
        To update a item by itemIndex.if the itemIndex-th item is not visible, then this method will do nothing.
        Otherwise this method will first call onGetItemByIndex(itemIndex) to get a updated item and then reposition all visible items'position. 
        */
        public void RefreshItemByItemIndex( int itemIndex )
        {
            var count = mItemList.Count;
            if (count == 0) return;
            if (itemIndex < mItemList[0].ItemIndex || itemIndex > mItemList[count - 1].ItemIndex) return;
            var firstItemIndex = mItemList[0].ItemIndex;
            var i = itemIndex - firstItemIndex;
            var curItem = mItemList[i];
            var pos = curItem.CachedRectTransform.anchoredPosition3D;
            RecycleItemTmp(curItem);
            var newItem = GetNewItemByIndex(itemIndex);
            if (newItem == null)
            {
                RefreshAllShownItemWithFirstIndex(firstItemIndex);
                return;
            }
            mItemList[i] = newItem;
            if (IsVertList)
                pos.x = newItem.StartPosOffset;
            else
                pos.y = newItem.StartPosOffset;
            newItem.CachedRectTransform.anchoredPosition3D = pos;
            OnItemSizeChanged(itemIndex);
            ClearAllTmpRecycledItem();
        }

        //snap move will finish at once.
        public void FinishSnapImmediately()
        {
            UpdateSnapMove(true);
        }

        /*
        This method will move the scrollrect content’s position to ( the positon of itemIndex-th item + offset ),
        and offset is from 0 to scrollrect viewport size. 
        */
        public void MovePanelToItemIndex( int itemIndex, float offset )
        {
            ScrollRect.StopMovement();
            mCurSnapData.Clear();
            if (ItemTotalCount == 0) return;
            if (itemIndex < 0 && ItemTotalCount > 0) return;
            if (ItemTotalCount > 0 && itemIndex >= ItemTotalCount) itemIndex = ItemTotalCount - 1;
            if (offset < 0) offset = 0;
            var pos = Vector3.zero;
            var viewPortSize = ViewPortSize;
            if (offset > viewPortSize) offset = viewPortSize;
            if (ArrangeType == ListItemArrangeType.TopToBottom)
            {
                var containerPos = ContainerTrans.anchoredPosition3D.y;
                if (containerPos < 0) containerPos = 0;
                pos.y = -containerPos - offset;
            }
            else if (ArrangeType == ListItemArrangeType.BottomToTop)
            {
                var containerPos = ContainerTrans.anchoredPosition3D.y;
                if (containerPos > 0) containerPos = 0;
                pos.y = -containerPos + offset;
            }
            else if (ArrangeType == ListItemArrangeType.LeftToRight)
            {
                var containerPos = ContainerTrans.anchoredPosition3D.x;
                if (containerPos > 0) containerPos = 0;
                pos.x = -containerPos + offset;
            }
            else if (ArrangeType == ListItemArrangeType.RightToLeft)
            {
                var containerPos = ContainerTrans.anchoredPosition3D.x;
                if (containerPos < 0) containerPos = 0;
                pos.x = -containerPos - offset;
            }

            RecycleAllItem();
            var newItem = GetNewItemByIndex(itemIndex);
            if (newItem == null)
            {
                ClearAllTmpRecycledItem();
                return;
            }
            if (IsVertList)
                pos.x = newItem.StartPosOffset;
            else
                pos.y = newItem.StartPosOffset;
            newItem.CachedRectTransform.anchoredPosition3D = pos;
            if (SupportScrollBar)
            {
                if (IsVertList)
                    SetItemSize(itemIndex, newItem.CachedRectTransform.rect.height, newItem.Padding);
                else
                    SetItemSize(itemIndex, newItem.CachedRectTransform.rect.width, newItem.Padding);
            }
            mItemList.Add(newItem);
            UpdateContentSize();
            UpdateListView(viewPortSize + 100, viewPortSize + 100, viewPortSize, viewPortSize);
            AdjustPanelPos();
            ClearAllTmpRecycledItem();
            ForceSnapUpdateCheck();
            UpdateSnapMove(false, true);
        }

        //update all visible items.
        public void RefreshAllShownItem()
        {
            var count = mItemList.Count;
            if (count == 0) return;
            RefreshAllShownItemWithFirstIndex(mItemList[0].ItemIndex);
        }


        public void RefreshAllShownItemWithFirstIndex( int firstItemIndex )
        {
            var count = mItemList.Count;
            if (count == 0) return;
            var firstItem = mItemList[0];
            var pos = firstItem.CachedRectTransform.anchoredPosition3D;
            RecycleAllItem();
            for (var i = 0; i < count; ++i)
            {
                var curIndex = firstItemIndex + i;
                var newItem = GetNewItemByIndex(curIndex);
                if (newItem == null) break;
                if (IsVertList)
                    pos.x = newItem.StartPosOffset;
                else
                    pos.y = newItem.StartPosOffset;
                newItem.CachedRectTransform.anchoredPosition3D = pos;
                if (SupportScrollBar)
                {
                    if (IsVertList)
                        SetItemSize(curIndex, newItem.CachedRectTransform.rect.height, newItem.Padding);
                    else
                        SetItemSize(curIndex, newItem.CachedRectTransform.rect.width, newItem.Padding);
                }

                mItemList.Add(newItem);
            }
            UpdateContentSize();
            UpdateAllShownItemsPos();
            ClearAllTmpRecycledItem();
        }


        public void RefreshAllShownItemWithFirstIndexAndPos( int firstItemIndex, Vector3 pos )
        {
            RecycleAllItem();
            var newItem = GetNewItemByIndex(firstItemIndex);
            if (newItem == null) return;
            if (IsVertList)
                pos.x = newItem.StartPosOffset;
            else
                pos.y = newItem.StartPosOffset;
            newItem.CachedRectTransform.anchoredPosition3D = pos;
            if (SupportScrollBar)
            {
                if (IsVertList)
                    SetItemSize(firstItemIndex, newItem.CachedRectTransform.rect.height, newItem.Padding);
                else
                    SetItemSize(firstItemIndex, newItem.CachedRectTransform.rect.width, newItem.Padding);
            }
            mItemList.Add(newItem);
            UpdateContentSize();
            UpdateAllShownItemsPos();
            UpdateListView(mDistanceForRecycle0, mDistanceForRecycle1, mDistanceForNew0, mDistanceForNew1);
            ClearAllTmpRecycledItem();
        }


        private void RecycleItemTmp( LoopListViewItem2 item )
        {
            if (item == null) return;
            if (string.IsNullOrEmpty(item.ItemPrefabName)) return;
            ItemPool pool = null;
            if (mItemPoolDict.TryGetValue(item.ItemPrefabName, out pool) == false) return;
            pool.RecycleItem(item);
        }


        private void ClearAllTmpRecycledItem()
        {
            var count = mItemPoolList.Count;
            for (var i = 0; i < count; ++i) mItemPoolList[i].ClearTmpRecycledItem();
        }


        private void RecycleAllItem()
        {
            foreach (var item in mItemList) RecycleItemTmp(item);
            mItemList.Clear();
        }


        private void AdjustContainerPivot( RectTransform rtf )
        {
            var pivot = rtf.pivot;
            if (ArrangeType == ListItemArrangeType.BottomToTop)
                pivot.y = 0;
            else if (ArrangeType == ListItemArrangeType.TopToBottom)
                pivot.y = 1;
            else if (ArrangeType == ListItemArrangeType.LeftToRight)
                pivot.x = 0;
            else if (ArrangeType == ListItemArrangeType.RightToLeft) pivot.x = 1;
            rtf.pivot = pivot;
        }


        private void AdjustPivot( RectTransform rtf )
        {
            var pivot = rtf.pivot;

            if (ArrangeType == ListItemArrangeType.BottomToTop)
                pivot.y = 0;
            else if (ArrangeType == ListItemArrangeType.TopToBottom)
                pivot.y = 1;
            else if (ArrangeType == ListItemArrangeType.LeftToRight)
                pivot.x = 0;
            else if (ArrangeType == ListItemArrangeType.RightToLeft) pivot.x = 1;
            rtf.pivot = pivot;
        }

        private void AdjustContainerAnchor( RectTransform rtf )
        {
            var anchorMin = rtf.anchorMin;
            var anchorMax = rtf.anchorMax;
            if (ArrangeType == ListItemArrangeType.BottomToTop)
            {
                anchorMin.y = 0;
                anchorMax.y = 0;
            }
            else if (ArrangeType == ListItemArrangeType.TopToBottom)
            {
                anchorMin.y = 1;
                anchorMax.y = 1;
            }
            else if (ArrangeType == ListItemArrangeType.LeftToRight)
            {
                anchorMin.x = 0;
                anchorMax.x = 0;
            }
            else if (ArrangeType == ListItemArrangeType.RightToLeft)
            {
                anchorMin.x = 1;
                anchorMax.x = 1;
            }
            rtf.anchorMin = anchorMin;
            rtf.anchorMax = anchorMax;
        }


        private void AdjustAnchor( RectTransform rtf )
        {
            var anchorMin = rtf.anchorMin;
            var anchorMax = rtf.anchorMax;
            if (ArrangeType == ListItemArrangeType.BottomToTop)
            {
                anchorMin.y = 0;
                anchorMax.y = 0;
            }
            else if (ArrangeType == ListItemArrangeType.TopToBottom)
            {
                anchorMin.y = 1;
                anchorMax.y = 1;
            }
            else if (ArrangeType == ListItemArrangeType.LeftToRight)
            {
                anchorMin.x = 0;
                anchorMax.x = 0;
            }
            else if (ArrangeType == ListItemArrangeType.RightToLeft)
            {
                anchorMin.x = 1;
                anchorMax.x = 1;
            }
            rtf.anchorMin = anchorMin;
            rtf.anchorMax = anchorMax;
        }

        private void InitItemPool()
        {
            foreach (var data in mItemPrefabDataList)
            {
                if (data.mItemPrefab == null)
                {
                    Debug.LogError("A item prefab is null ");
                    continue;
                }
                var prefabName = data.mItemPrefab.name;
                if (mItemPoolDict.ContainsKey(prefabName))
                {
                    Debug.LogError("A item prefab with name " + prefabName + " has existed!");
                    continue;
                }
                var rtf = data.mItemPrefab.GetComponent<RectTransform>();
                if (rtf == null)
                {
                    Debug.LogError("RectTransform component is not found in the prefab " + prefabName);
                    continue;
                }
                AdjustAnchor(rtf);
                AdjustPivot(rtf);
                var tItem = data.mItemPrefab.GetComponent<LoopListViewItem2>();
                if (tItem == null) data.mItemPrefab.AddComponent<LoopListViewItem2>();
                var pool = new ItemPool();
                pool.Init(data.mItemPrefab, data.mPadding, data.mStartPosOffset, data.mInitCreateCount, ContainerTrans);
                mItemPoolDict.Add(prefabName, pool);
                mItemPoolList.Add(pool);
            }
        }

        private void CacheDragPointerEventData( PointerEventData eventData )
        {
            if (mPointerEventData == null) mPointerEventData = new PointerEventData(EventSystem.current);
            mPointerEventData.button = eventData.button;
            mPointerEventData.position = eventData.position;
            mPointerEventData.pointerPressRaycast = eventData.pointerPressRaycast;
            mPointerEventData.pointerCurrentRaycast = eventData.pointerCurrentRaycast;
        }

        private LoopListViewItem2 GetNewItemByIndex( int index )
        {
            if (SupportScrollBar && index < 0) return null;
            if (ItemTotalCount > 0 && index >= ItemTotalCount) return null;
            var newItem = mOnGetItemByIndex(this, index);
            if (newItem == null) return null;
            newItem.ItemIndex = index;
            newItem.ItemCreatedCheckFrameCount = mListUpdateCheckFrameCount;
            return newItem;
        }


        private void SetItemSize( int itemIndex, float itemSize, float padding )
        {
            mItemPosMgr.SetItemSize(itemIndex, itemSize + padding);
            if (itemIndex >= mLastItemIndex)
            {
                mLastItemIndex = itemIndex;
                mLastItemPadding = padding;
            }
        }

        private void GetPlusItemIndexAndPosAtGivenPos( float pos, ref int index, ref float itemPos )
        {
            mItemPosMgr.GetItemIndexAndPosAtGivenPos(pos, ref index, ref itemPos);
        }


        private float GetItemPos( int itemIndex )
        {
            return mItemPosMgr.GetItemPos(itemIndex);
        }


        public Vector3 GetItemCornerPosInViewPort( LoopListViewItem2 item,
            ItemCornerEnum corner = ItemCornerEnum.LeftBottom )
        {
            item.CachedRectTransform.GetWorldCorners(mItemWorldCorners);
            return mViewPortRectTransform.InverseTransformPoint(mItemWorldCorners[(int) corner]);
        }


        private void AdjustPanelPos()
        {
            var count = mItemList.Count;
            if (count == 0) return;
            UpdateAllShownItemsPos();
            var viewPortSize = ViewPortSize;
            var contentSize = GetContentPanelSize();
            if (ArrangeType == ListItemArrangeType.TopToBottom)
            {
                if (contentSize <= viewPortSize)
                {
                    var pos = ContainerTrans.anchoredPosition3D;
                    pos.y = 0;
                    ContainerTrans.anchoredPosition3D = pos;
                    mItemList[0].CachedRectTransform.anchoredPosition3D =
                        new Vector3(mItemList[0].StartPosOffset, 0, 0);
                    UpdateAllShownItemsPos();
                    return;
                }
                var tViewItem0 = mItemList[0];
                tViewItem0.CachedRectTransform.GetWorldCorners(mItemWorldCorners);
                var topPos0 = mViewPortRectTransform.InverseTransformPoint(mItemWorldCorners[1]);
                if (topPos0.y < mViewPortRectLocalCorners[1].y)
                {
                    var pos = ContainerTrans.anchoredPosition3D;
                    pos.y = 0;
                    ContainerTrans.anchoredPosition3D = pos;
                    mItemList[0].CachedRectTransform.anchoredPosition3D =
                        new Vector3(mItemList[0].StartPosOffset, 0, 0);
                    UpdateAllShownItemsPos();
                    return;
                }
                var tViewItem1 = mItemList[mItemList.Count - 1];
                tViewItem1.CachedRectTransform.GetWorldCorners(mItemWorldCorners);
                var downPos1 = mViewPortRectTransform.InverseTransformPoint(mItemWorldCorners[0]);
                var d = downPos1.y - mViewPortRectLocalCorners[0].y;
                if (d > 0)
                {
                    var pos = mItemList[0].CachedRectTransform.anchoredPosition3D;
                    pos.y = pos.y - d;
                    mItemList[0].CachedRectTransform.anchoredPosition3D = pos;
                    UpdateAllShownItemsPos();
                }
            }
            else if (ArrangeType == ListItemArrangeType.BottomToTop)
            {
                if (contentSize <= viewPortSize)
                {
                    var pos = ContainerTrans.anchoredPosition3D;
                    pos.y = 0;
                    ContainerTrans.anchoredPosition3D = pos;
                    mItemList[0].CachedRectTransform.anchoredPosition3D =
                        new Vector3(mItemList[0].StartPosOffset, 0, 0);
                    UpdateAllShownItemsPos();
                    return;
                }
                var tViewItem0 = mItemList[0];
                tViewItem0.CachedRectTransform.GetWorldCorners(mItemWorldCorners);
                var downPos0 = mViewPortRectTransform.InverseTransformPoint(mItemWorldCorners[0]);
                if (downPos0.y > mViewPortRectLocalCorners[0].y)
                {
                    var pos = ContainerTrans.anchoredPosition3D;
                    pos.y = 0;
                    ContainerTrans.anchoredPosition3D = pos;
                    mItemList[0].CachedRectTransform.anchoredPosition3D =
                        new Vector3(mItemList[0].StartPosOffset, 0, 0);
                    UpdateAllShownItemsPos();
                    return;
                }
                var tViewItem1 = mItemList[mItemList.Count - 1];
                tViewItem1.CachedRectTransform.GetWorldCorners(mItemWorldCorners);
                var topPos1 = mViewPortRectTransform.InverseTransformPoint(mItemWorldCorners[1]);
                var d = mViewPortRectLocalCorners[1].y - topPos1.y;
                if (d > 0)
                {
                    var pos = mItemList[0].CachedRectTransform.anchoredPosition3D;
                    pos.y = pos.y + d;
                    mItemList[0].CachedRectTransform.anchoredPosition3D = pos;
                    UpdateAllShownItemsPos();
                }
            }
            else if (ArrangeType == ListItemArrangeType.LeftToRight)
            {
                if (contentSize <= viewPortSize)
                {
                    var pos = ContainerTrans.anchoredPosition3D;
                    pos.x = 0;
                    ContainerTrans.anchoredPosition3D = pos;
                    mItemList[0].CachedRectTransform.anchoredPosition3D =
                        new Vector3(0, mItemList[0].StartPosOffset, 0);
                    UpdateAllShownItemsPos();
                    return;
                }
                var tViewItem0 = mItemList[0];
                tViewItem0.CachedRectTransform.GetWorldCorners(mItemWorldCorners);
                var leftPos0 = mViewPortRectTransform.InverseTransformPoint(mItemWorldCorners[1]);
                if (leftPos0.x > mViewPortRectLocalCorners[1].x)
                {
                    var pos = ContainerTrans.anchoredPosition3D;
                    pos.x = 0;
                    ContainerTrans.anchoredPosition3D = pos;
                    mItemList[0].CachedRectTransform.anchoredPosition3D =
                        new Vector3(0, mItemList[0].StartPosOffset, 0);
                    UpdateAllShownItemsPos();
                    return;
                }
                var tViewItem1 = mItemList[mItemList.Count - 1];
                tViewItem1.CachedRectTransform.GetWorldCorners(mItemWorldCorners);
                var rightPos1 = mViewPortRectTransform.InverseTransformPoint(mItemWorldCorners[2]);
                var d = mViewPortRectLocalCorners[2].x - rightPos1.x;
                if (d > 0)
                {
                    var pos = mItemList[0].CachedRectTransform.anchoredPosition3D;
                    pos.x = pos.x + d;
                    mItemList[0].CachedRectTransform.anchoredPosition3D = pos;
                    UpdateAllShownItemsPos();
                }
            }
            else if (ArrangeType == ListItemArrangeType.RightToLeft)
            {
                if (contentSize <= viewPortSize)
                {
                    var pos = ContainerTrans.anchoredPosition3D;
                    pos.x = 0;
                    ContainerTrans.anchoredPosition3D = pos;
                    mItemList[0].CachedRectTransform.anchoredPosition3D =
                        new Vector3(0, mItemList[0].StartPosOffset, 0);
                    UpdateAllShownItemsPos();
                    return;
                }
                var tViewItem0 = mItemList[0];
                tViewItem0.CachedRectTransform.GetWorldCorners(mItemWorldCorners);
                var rightPos0 = mViewPortRectTransform.InverseTransformPoint(mItemWorldCorners[2]);
                if (rightPos0.x < mViewPortRectLocalCorners[2].x)
                {
                    var pos = ContainerTrans.anchoredPosition3D;
                    pos.x = 0;
                    ContainerTrans.anchoredPosition3D = pos;
                    mItemList[0].CachedRectTransform.anchoredPosition3D =
                        new Vector3(0, mItemList[0].StartPosOffset, 0);
                    UpdateAllShownItemsPos();
                    return;
                }
                var tViewItem1 = mItemList[mItemList.Count - 1];
                tViewItem1.CachedRectTransform.GetWorldCorners(mItemWorldCorners);
                var leftPos1 = mViewPortRectTransform.InverseTransformPoint(mItemWorldCorners[1]);
                var d = leftPos1.x - mViewPortRectLocalCorners[1].x;
                if (d > 0)
                {
                    var pos = mItemList[0].CachedRectTransform.anchoredPosition3D;
                    pos.x = pos.x - d;
                    mItemList[0].CachedRectTransform.anchoredPosition3D = pos;
                    UpdateAllShownItemsPos();
                }
            }
        }


        private void Update()
        {
            if (mListViewInited == false) return;
            if (mNeedAdjustVec)
            {
                mNeedAdjustVec = false;
                if (IsVertList)
                {
                    if (ScrollRect.velocity.y * mAdjustedVec.y > 0) ScrollRect.velocity = mAdjustedVec;
                }
                else
                {
                    if (ScrollRect.velocity.x * mAdjustedVec.x > 0) ScrollRect.velocity = mAdjustedVec;
                }
            }
            if (SupportScrollBar) mItemPosMgr.Update(false);
            UpdateSnapMove();
            UpdateListView(mDistanceForRecycle0, mDistanceForRecycle1, mDistanceForNew0, mDistanceForNew1);
            ClearAllTmpRecycledItem();
            mLastFrameContainerPos = ContainerTrans.anchoredPosition3D;
        }

        //update snap move. if immediate is set true, then the snap move will finish at once.
        private void UpdateSnapMove( bool immediate = false, bool forceSendEvent = false )
        {
            if (ItemSnapEnable == false) return;
            if (IsVertList)
                UpdateSnapVertical(immediate, forceSendEvent);
            else
                UpdateSnapHorizontal(immediate, forceSendEvent);
        }


        public void UpdateAllShownItemSnapData()
        {
            if (ItemSnapEnable == false) return;
            var count = mItemList.Count;
            if (count == 0) return;
            var pos = ContainerTrans.anchoredPosition3D;
            var tViewItem0 = mItemList[0];
            tViewItem0.CachedRectTransform.GetWorldCorners(mItemWorldCorners);
            float start = 0;
            float end = 0;
            float itemSnapCenter = 0;
            float snapCenter = 0;
            if (ArrangeType == ListItemArrangeType.TopToBottom)
            {
                snapCenter = -(1 - mViewPortSnapPivot.y) * mViewPortRectTransform.rect.height;
                var topPos1 = mViewPortRectTransform.InverseTransformPoint(mItemWorldCorners[1]);
                start = topPos1.y;
                end = start - tViewItem0.ItemSizeWithPadding;
                itemSnapCenter = start - tViewItem0.ItemSize * (1 - mItemSnapPivot.y);
                for (var i = 0; i < count; ++i)
                {
                    mItemList[i].DistanceWithViewPortSnapCenter = snapCenter - itemSnapCenter;
                    if (i + 1 < count)
                    {
                        start = end;
                        end = end - mItemList[i + 1].ItemSizeWithPadding;
                        itemSnapCenter = start - mItemList[i + 1].ItemSize * (1 - mItemSnapPivot.y);
                    }
                }
            }
            else if (ArrangeType == ListItemArrangeType.BottomToTop)
            {
                snapCenter = mViewPortSnapPivot.y * mViewPortRectTransform.rect.height;
                var bottomPos1 = mViewPortRectTransform.InverseTransformPoint(mItemWorldCorners[0]);
                start = bottomPos1.y;
                end = start + tViewItem0.ItemSizeWithPadding;
                itemSnapCenter = start + tViewItem0.ItemSize * mItemSnapPivot.y;
                for (var i = 0; i < count; ++i)
                {
                    mItemList[i].DistanceWithViewPortSnapCenter = snapCenter - itemSnapCenter;
                    if (i + 1 < count)
                    {
                        start = end;
                        end = end + mItemList[i + 1].ItemSizeWithPadding;
                        itemSnapCenter = start + mItemList[i + 1].ItemSize * mItemSnapPivot.y;
                    }
                }
            }
            else if (ArrangeType == ListItemArrangeType.RightToLeft)
            {
                snapCenter = -(1 - mViewPortSnapPivot.x) * mViewPortRectTransform.rect.width;
                var rightPos1 = mViewPortRectTransform.InverseTransformPoint(mItemWorldCorners[2]);
                start = rightPos1.x;
                end = start - tViewItem0.ItemSizeWithPadding;
                itemSnapCenter = start - tViewItem0.ItemSize * (1 - mItemSnapPivot.x);
                for (var i = 0; i < count; ++i)
                {
                    mItemList[i].DistanceWithViewPortSnapCenter = snapCenter - itemSnapCenter;
                    if (i + 1 < count)
                    {
                        start = end;
                        end = end - mItemList[i + 1].ItemSizeWithPadding;
                        itemSnapCenter = start - mItemList[i + 1].ItemSize * (1 - mItemSnapPivot.x);
                    }
                }
            }
            else if (ArrangeType == ListItemArrangeType.LeftToRight)
            {
                snapCenter = mViewPortSnapPivot.x * mViewPortRectTransform.rect.width;
                var leftPos1 = mViewPortRectTransform.InverseTransformPoint(mItemWorldCorners[1]);
                start = leftPos1.x;
                end = start + tViewItem0.ItemSizeWithPadding;
                itemSnapCenter = start + tViewItem0.ItemSize * mItemSnapPivot.x;
                for (var i = 0; i < count; ++i)
                {
                    mItemList[i].DistanceWithViewPortSnapCenter = snapCenter - itemSnapCenter;
                    if (i + 1 < count)
                    {
                        start = end;
                        end = end + mItemList[i + 1].ItemSizeWithPadding;
                        itemSnapCenter = start + mItemList[i + 1].ItemSize * mItemSnapPivot.x;
                    }
                }
            }
        }


        private void UpdateSnapVertical( bool immediate = false, bool forceSendEvent = false )
        {
            if (ItemSnapEnable == false) return;
            var count = mItemList.Count;
            if (count == 0) return;
            var pos = ContainerTrans.anchoredPosition3D;
            var needCheck = pos.y != mLastSnapCheckPos.y;
            mLastSnapCheckPos = pos;
            if (!needCheck)
                if (mLeftSnapUpdateExtraCount > 0)
                {
                    mLeftSnapUpdateExtraCount--;
                    needCheck = true;
                }
            if (needCheck)
            {
                var tViewItem0 = mItemList[0];
                tViewItem0.CachedRectTransform.GetWorldCorners(mItemWorldCorners);
                var curIndex = -1;
                float start = 0;
                float end = 0;
                float itemSnapCenter = 0;
                var curMinDist = float.MaxValue;
                float curDist = 0;
                float curDistAbs = 0;
                float snapCenter = 0;
                if (ArrangeType == ListItemArrangeType.TopToBottom)
                {
                    snapCenter = -(1 - mViewPortSnapPivot.y) * mViewPortRectTransform.rect.height;
                    var topPos1 = mViewPortRectTransform.InverseTransformPoint(mItemWorldCorners[1]);
                    start = topPos1.y;
                    end = start - tViewItem0.ItemSizeWithPadding;
                    itemSnapCenter = start - tViewItem0.ItemSize * (1 - mItemSnapPivot.y);
                    for (var i = 0; i < count; ++i)
                    {
                        curDist = snapCenter - itemSnapCenter;
                        curDistAbs = Mathf.Abs(curDist);
                        if (curDistAbs < curMinDist)
                        {
                            curMinDist = curDistAbs;
                            curIndex = i;
                        }
                        else
                        {
                            break;
                        }

                        if (i + 1 < count)
                        {
                            start = end;
                            end = end - mItemList[i + 1].ItemSizeWithPadding;
                            itemSnapCenter = start - mItemList[i + 1].ItemSize * (1 - mItemSnapPivot.y);
                        }
                    }
                }
                else if (ArrangeType == ListItemArrangeType.BottomToTop)
                {
                    snapCenter = mViewPortSnapPivot.y * mViewPortRectTransform.rect.height;
                    var bottomPos1 = mViewPortRectTransform.InverseTransformPoint(mItemWorldCorners[0]);
                    start = bottomPos1.y;
                    end = start + tViewItem0.ItemSizeWithPadding;
                    itemSnapCenter = start + tViewItem0.ItemSize * mItemSnapPivot.y;
                    for (var i = 0; i < count; ++i)
                    {
                        curDist = snapCenter - itemSnapCenter;
                        curDistAbs = Mathf.Abs(curDist);
                        if (curDistAbs < curMinDist)
                        {
                            curMinDist = curDistAbs;
                            curIndex = i;
                        }
                        else
                        {
                            break;
                        }

                        if (i + 1 < count)
                        {
                            start = end;
                            end = end + mItemList[i + 1].ItemSizeWithPadding;
                            itemSnapCenter = start + mItemList[i + 1].ItemSize * mItemSnapPivot.y;
                        }
                    }
                }

                if (curIndex >= 0)
                {
                    var oldNearestItemIndex = CurSnapNearestItemIndex;
                    CurSnapNearestItemIndex = mItemList[curIndex].ItemIndex;
                    if (forceSendEvent || mItemList[curIndex].ItemIndex != oldNearestItemIndex)
                        if (mOnSnapNearestChanged != null)
                            mOnSnapNearestChanged(this, mItemList[curIndex]);
                }
                else
                {
                    CurSnapNearestItemIndex = -1;
                }
            }
            if (CanSnap() == false)
            {
                ClearSnapData();
                return;
            }
            var v = Mathf.Abs(ScrollRect.velocity.y);
            UpdateCurSnapData();
            if (mCurSnapData.mSnapStatus != SnapStatus.SnapMoving) return;
            if (v > 0) ScrollRect.StopMovement();
            var old = mCurSnapData.mCurSnapVal;
            mCurSnapData.mCurSnapVal = Mathf.SmoothDamp(mCurSnapData.mCurSnapVal, mCurSnapData.mTargetSnapVal,
                ref mSmoothDumpVel, mSmoothDumpRate);
            var dt = mCurSnapData.mCurSnapVal - old;

            if (immediate || Mathf.Abs(mCurSnapData.mTargetSnapVal - mCurSnapData.mCurSnapVal) < mSnapFinishThreshold)
            {
                pos.y = pos.y + mCurSnapData.mTargetSnapVal - old;
                mCurSnapData.mSnapStatus = SnapStatus.SnapMoveFinish;
                if (mOnSnapItemFinished != null)
                {
                    var targetItem = GetShownItemByItemIndex(CurSnapNearestItemIndex);
                    if (targetItem != null) mOnSnapItemFinished(this, targetItem);
                }
            }
            else
            {
                pos.y = pos.y + dt;
            }

            if (ArrangeType == ListItemArrangeType.TopToBottom)
            {
                var maxY = mViewPortRectLocalCorners[0].y + ContainerTrans.rect.height;
                pos.y = Mathf.Clamp(pos.y, 0, maxY);
                ContainerTrans.anchoredPosition3D = pos;
            }
            else if (ArrangeType == ListItemArrangeType.BottomToTop)
            {
                var minY = mViewPortRectLocalCorners[1].y - ContainerTrans.rect.height;
                pos.y = Mathf.Clamp(pos.y, minY, 0);
                ContainerTrans.anchoredPosition3D = pos;
            }
        }


        private void UpdateCurSnapData()
        {
            var count = mItemList.Count;
            if (count == 0)
            {
                mCurSnapData.Clear();
                return;
            }

            if (mCurSnapData.mSnapStatus == SnapStatus.SnapMoveFinish)
            {
                if (mCurSnapData.mSnapTargetIndex == CurSnapNearestItemIndex) return;
                mCurSnapData.mSnapStatus = SnapStatus.NoTargetSet;
            }
            if (mCurSnapData.mSnapStatus == SnapStatus.SnapMoving)
            {
                if (mCurSnapData.mSnapTargetIndex == CurSnapNearestItemIndex || mCurSnapData.mIsForceSnapTo) return;
                mCurSnapData.mSnapStatus = SnapStatus.NoTargetSet;
            }
            if (mCurSnapData.mSnapStatus == SnapStatus.NoTargetSet)
            {
                var nearestItem = GetShownItemByItemIndex(CurSnapNearestItemIndex);
                if (nearestItem == null) return;
                mCurSnapData.mSnapTargetIndex = CurSnapNearestItemIndex;
                mCurSnapData.mSnapStatus = SnapStatus.TargetHasSet;
                mCurSnapData.mIsForceSnapTo = false;
            }
            if (mCurSnapData.mSnapStatus == SnapStatus.TargetHasSet)
            {
                var targetItem = GetShownItemByItemIndex(mCurSnapData.mSnapTargetIndex);
                if (targetItem == null)
                {
                    mCurSnapData.Clear();
                    return;
                }
                UpdateAllShownItemSnapData();
                mCurSnapData.mTargetSnapVal = targetItem.DistanceWithViewPortSnapCenter;
                mCurSnapData.mCurSnapVal = 0;
                mCurSnapData.mSnapStatus = SnapStatus.SnapMoving;
            }
        }

        //Clear current snap target and then the LoopScrollView2 will auto snap to the CurSnapNearestItemIndex.
        public void ClearSnapData()
        {
            mCurSnapData.Clear();
        }

        public void SetSnapTargetItemIndex( int itemIndex )
        {
            mCurSnapData.mSnapTargetIndex = itemIndex;
            mCurSnapData.mSnapStatus = SnapStatus.TargetHasSet;
            mCurSnapData.mIsForceSnapTo = true;
        }

        public void ForceSnapUpdateCheck()
        {
            if (mLeftSnapUpdateExtraCount <= 0) mLeftSnapUpdateExtraCount = 1;
        }

        private void UpdateSnapHorizontal( bool immediate = false, bool forceSendEvent = false )
        {
            if (ItemSnapEnable == false) return;
            var count = mItemList.Count;
            if (count == 0) return;
            var pos = ContainerTrans.anchoredPosition3D;
            var needCheck = pos.x != mLastSnapCheckPos.x;
            mLastSnapCheckPos = pos;
            if (!needCheck)
                if (mLeftSnapUpdateExtraCount > 0)
                {
                    mLeftSnapUpdateExtraCount--;
                    needCheck = true;
                }
            if (needCheck)
            {
                var tViewItem0 = mItemList[0];
                tViewItem0.CachedRectTransform.GetWorldCorners(mItemWorldCorners);
                var curIndex = -1;
                float start = 0;
                float end = 0;
                float itemSnapCenter = 0;
                var curMinDist = float.MaxValue;
                float curDist = 0;
                float curDistAbs = 0;
                float snapCenter = 0;
                if (ArrangeType == ListItemArrangeType.RightToLeft)
                {
                    snapCenter = -(1 - mViewPortSnapPivot.x) * mViewPortRectTransform.rect.width;
                    var rightPos1 = mViewPortRectTransform.InverseTransformPoint(mItemWorldCorners[2]);
                    start = rightPos1.x;
                    end = start - tViewItem0.ItemSizeWithPadding;
                    itemSnapCenter = start - tViewItem0.ItemSize * (1 - mItemSnapPivot.x);
                    for (var i = 0; i < count; ++i)
                    {
                        curDist = snapCenter - itemSnapCenter;
                        curDistAbs = Mathf.Abs(curDist);
                        if (curDistAbs < curMinDist)
                        {
                            curMinDist = curDistAbs;
                            curIndex = i;
                        }
                        else
                        {
                            break;
                        }

                        if (i + 1 < count)
                        {
                            start = end;
                            end = end - mItemList[i + 1].ItemSizeWithPadding;
                            itemSnapCenter = start - mItemList[i + 1].ItemSize * (1 - mItemSnapPivot.x);
                        }
                    }
                }
                else if (ArrangeType == ListItemArrangeType.LeftToRight)
                {
                    snapCenter = mViewPortSnapPivot.x * mViewPortRectTransform.rect.width;
                    var leftPos1 = mViewPortRectTransform.InverseTransformPoint(mItemWorldCorners[1]);
                    start = leftPos1.x;
                    end = start + tViewItem0.ItemSizeWithPadding;
                    itemSnapCenter = start + tViewItem0.ItemSize * mItemSnapPivot.x;
                    for (var i = 0; i < count; ++i)
                    {
                        curDist = snapCenter - itemSnapCenter;
                        curDistAbs = Mathf.Abs(curDist);
                        if (curDistAbs < curMinDist)
                        {
                            curMinDist = curDistAbs;
                            curIndex = i;
                        }
                        else
                        {
                            break;
                        }

                        if (i + 1 < count)
                        {
                            start = end;
                            end = end + mItemList[i + 1].ItemSizeWithPadding;
                            itemSnapCenter = start + mItemList[i + 1].ItemSize * mItemSnapPivot.x;
                        }
                    }
                }


                if (curIndex >= 0)
                {
                    var oldNearestItemIndex = CurSnapNearestItemIndex;
                    CurSnapNearestItemIndex = mItemList[curIndex].ItemIndex;
                    if (forceSendEvent || mItemList[curIndex].ItemIndex != oldNearestItemIndex)
                        if (mOnSnapNearestChanged != null)
                            mOnSnapNearestChanged(this, mItemList[curIndex]);
                }
                else
                {
                    CurSnapNearestItemIndex = -1;
                }
            }
            if (CanSnap() == false)
            {
                ClearSnapData();
                return;
            }
            var v = Mathf.Abs(ScrollRect.velocity.x);
            UpdateCurSnapData();
            if (mCurSnapData.mSnapStatus != SnapStatus.SnapMoving) return;
            if (v > 0) ScrollRect.StopMovement();
            var old = mCurSnapData.mCurSnapVal;
            mCurSnapData.mCurSnapVal = Mathf.SmoothDamp(mCurSnapData.mCurSnapVal, mCurSnapData.mTargetSnapVal,
                ref mSmoothDumpVel, mSmoothDumpRate);
            var dt = mCurSnapData.mCurSnapVal - old;

            if (immediate || Mathf.Abs(mCurSnapData.mTargetSnapVal - mCurSnapData.mCurSnapVal) < mSnapFinishThreshold)
            {
                pos.x = pos.x + mCurSnapData.mTargetSnapVal - old;
                mCurSnapData.mSnapStatus = SnapStatus.SnapMoveFinish;
                if (mOnSnapItemFinished != null)
                {
                    var targetItem = GetShownItemByItemIndex(CurSnapNearestItemIndex);
                    if (targetItem != null) mOnSnapItemFinished(this, targetItem);
                }
            }
            else
            {
                pos.x = pos.x + dt;
            }

            if (ArrangeType == ListItemArrangeType.LeftToRight)
            {
                var minX = mViewPortRectLocalCorners[2].x - ContainerTrans.rect.width;
                pos.x = Mathf.Clamp(pos.x, minX, 0);
                ContainerTrans.anchoredPosition3D = pos;
            }
            else if (ArrangeType == ListItemArrangeType.RightToLeft)
            {
                var maxX = mViewPortRectLocalCorners[1].x + ContainerTrans.rect.width;
                pos.x = Mathf.Clamp(pos.x, 0, maxX);
                ContainerTrans.anchoredPosition3D = pos;
            }
        }

        private bool CanSnap()
        {
            if (IsDraging) return false;
            if (mScrollBarClickEventListener != null)
                if (mScrollBarClickEventListener.IsPressd)
                    return false;

            if (IsVertList)
            {
                if (ContainerTrans.rect.height <= ViewPortHeight) return false;
            }
            else
            {
                if (ContainerTrans.rect.width <= ViewPortWidth) return false;
            }

            float v = 0;
            if (IsVertList)
                v = Mathf.Abs(ScrollRect.velocity.y);
            else
                v = Mathf.Abs(ScrollRect.velocity.x);
            if (v > mSnapVecThreshold) return false;
            if (v < 2) return true;
            float diff = 3;
            var pos = ContainerTrans.anchoredPosition3D;
            if (ArrangeType == ListItemArrangeType.LeftToRight)
            {
                var minX = mViewPortRectLocalCorners[2].x - ContainerTrans.rect.width;
                if (pos.x < minX - diff || pos.x > diff) return false;
            }
            else if (ArrangeType == ListItemArrangeType.RightToLeft)
            {
                var maxX = mViewPortRectLocalCorners[1].x + ContainerTrans.rect.width;
                if (pos.x > maxX + diff || pos.x < -diff) return false;
            }
            else if (ArrangeType == ListItemArrangeType.TopToBottom)
            {
                var maxY = mViewPortRectLocalCorners[0].y + ContainerTrans.rect.height;
                if (pos.y > maxY + diff || pos.y < -diff) return false;
            }
            else if (ArrangeType == ListItemArrangeType.BottomToTop)
            {
                var minY = mViewPortRectLocalCorners[1].y - ContainerTrans.rect.height;
                if (pos.y < minY - diff || pos.y > diff) return false;
            }
            return true;
        }


        public void UpdateListView( float distanceForRecycle0, float distanceForRecycle1, float distanceForNew0,
            float distanceForNew1 )
        {
            mListUpdateCheckFrameCount++;
            if (IsVertList)
            {
                var needContinueCheck = true;
                var checkCount = 0;
                var maxCount = 9999;
                while (needContinueCheck)
                {
                    checkCount++;
                    if (checkCount >= maxCount)
                    {
                        Debug.LogError("UpdateListView Vertical while loop " + checkCount +
                                       " times! something is wrong!");
                        break;
                    }
                    needContinueCheck = UpdateForVertList(distanceForRecycle0, distanceForRecycle1, distanceForNew0,
                        distanceForNew1);
                }
            }
            else
            {
                var needContinueCheck = true;
                var checkCount = 0;
                var maxCount = 9999;
                while (needContinueCheck)
                {
                    checkCount++;
                    if (checkCount >= maxCount)
                    {
                        Debug.LogError("UpdateListView  Horizontal while loop " + checkCount +
                                       " times! something is wrong!");
                        break;
                    }
                    needContinueCheck = UpdateForHorizontalList(distanceForRecycle0, distanceForRecycle1,
                        distanceForNew0, distanceForNew1);
                }
            }
        }


        private bool UpdateForVertList( float distanceForRecycle0, float distanceForRecycle1, float distanceForNew0,
            float distanceForNew1 )
        {
            if (ItemTotalCount == 0)
            {
                if (mItemList.Count > 0) RecycleAllItem();
                return false;
            }
            if (ArrangeType == ListItemArrangeType.TopToBottom)
            {
                var itemListCount = mItemList.Count;
                if (itemListCount == 0)
                {
                    var curY = ContainerTrans.anchoredPosition3D.y;
                    if (curY < 0) curY = 0;
                    var index = 0;
                    var pos = -curY;
                    if (SupportScrollBar)
                    {
                        GetPlusItemIndexAndPosAtGivenPos(curY, ref index, ref pos);
                        pos = -pos;
                    }
                    var newItem = GetNewItemByIndex(index);
                    if (newItem == null) return false;
                    if (SupportScrollBar) SetItemSize(index, newItem.CachedRectTransform.rect.height, newItem.Padding);
                    mItemList.Add(newItem);
                    newItem.CachedRectTransform.anchoredPosition3D = new Vector3(newItem.StartPosOffset, pos, 0);
                    UpdateContentSize();
                    return true;
                }
                var tViewItem0 = mItemList[0];
                tViewItem0.CachedRectTransform.GetWorldCorners(mItemWorldCorners);
                var topPos0 = mViewPortRectTransform.InverseTransformPoint(mItemWorldCorners[1]);
                var downPos0 = mViewPortRectTransform.InverseTransformPoint(mItemWorldCorners[0]);

                if (!IsDraging && tViewItem0.ItemCreatedCheckFrameCount != mListUpdateCheckFrameCount
                               && downPos0.y - mViewPortRectLocalCorners[1].y > distanceForRecycle0)
                {
                    mItemList.RemoveAt(0);
                    RecycleItemTmp(tViewItem0);
                    if (!SupportScrollBar)
                    {
                        UpdateContentSize();
                        CheckIfNeedUpdataItemPos();
                    }
                    return true;
                }

                var tViewItem1 = mItemList[mItemList.Count - 1];
                tViewItem1.CachedRectTransform.GetWorldCorners(mItemWorldCorners);
                var topPos1 = mViewPortRectTransform.InverseTransformPoint(mItemWorldCorners[1]);
                var downPos1 = mViewPortRectTransform.InverseTransformPoint(mItemWorldCorners[0]);
                if (!IsDraging && tViewItem1.ItemCreatedCheckFrameCount != mListUpdateCheckFrameCount
                               && mViewPortRectLocalCorners[0].y - topPos1.y > distanceForRecycle1)
                {
                    mItemList.RemoveAt(mItemList.Count - 1);
                    RecycleItemTmp(tViewItem1);
                    if (!SupportScrollBar)
                    {
                        UpdateContentSize();
                        CheckIfNeedUpdataItemPos();
                    }
                    return true;
                }


                if (mViewPortRectLocalCorners[0].y - downPos1.y < distanceForNew1)
                {
                    if (tViewItem1.ItemIndex > mCurReadyMaxItemIndex)
                    {
                        mCurReadyMaxItemIndex = tViewItem1.ItemIndex;
                        mNeedCheckNextMaxItem = true;
                    }
                    var nIndex = tViewItem1.ItemIndex + 1;
                    if (nIndex <= mCurReadyMaxItemIndex || mNeedCheckNextMaxItem)
                    {
                        var newItem = GetNewItemByIndex(nIndex);
                        if (newItem == null)
                        {
                            mCurReadyMaxItemIndex = tViewItem1.ItemIndex;
                            mNeedCheckNextMaxItem = false;
                            CheckIfNeedUpdataItemPos();
                        }
                        else
                        {
                            if (SupportScrollBar)
                                SetItemSize(nIndex, newItem.CachedRectTransform.rect.height, newItem.Padding);
                            mItemList.Add(newItem);
                            var y = tViewItem1.CachedRectTransform.anchoredPosition3D.y -
                                    tViewItem1.CachedRectTransform.rect.height - tViewItem1.Padding;
                            newItem.CachedRectTransform.anchoredPosition3D = new Vector3(newItem.StartPosOffset, y, 0);
                            UpdateContentSize();
                            CheckIfNeedUpdataItemPos();

                            if (nIndex > mCurReadyMaxItemIndex) mCurReadyMaxItemIndex = nIndex;
                            return true;
                        }
                    }
                }

                if (topPos0.y - mViewPortRectLocalCorners[1].y < distanceForNew0)
                {
                    if (tViewItem0.ItemIndex < mCurReadyMinItemIndex)
                    {
                        mCurReadyMinItemIndex = tViewItem0.ItemIndex;
                        mNeedCheckNextMinItem = true;
                    }
                    var nIndex = tViewItem0.ItemIndex - 1;
                    if (nIndex >= mCurReadyMinItemIndex || mNeedCheckNextMinItem)
                    {
                        var newItem = GetNewItemByIndex(nIndex);
                        if (newItem == null)
                        {
                            mCurReadyMinItemIndex = tViewItem0.ItemIndex;
                            mNeedCheckNextMinItem = false;
                        }
                        else
                        {
                            if (SupportScrollBar)
                                SetItemSize(nIndex, newItem.CachedRectTransform.rect.height, newItem.Padding);
                            mItemList.Insert(0, newItem);
                            var y = tViewItem0.CachedRectTransform.anchoredPosition3D.y +
                                    newItem.CachedRectTransform.rect.height + newItem.Padding;
                            newItem.CachedRectTransform.anchoredPosition3D = new Vector3(newItem.StartPosOffset, y, 0);
                            UpdateContentSize();
                            CheckIfNeedUpdataItemPos();
                            if (nIndex < mCurReadyMinItemIndex) mCurReadyMinItemIndex = nIndex;
                            return true;
                        }
                    }
                }
            }
            else
            {
                if (mItemList.Count == 0)
                {
                    var curY = ContainerTrans.anchoredPosition3D.y;
                    if (curY > 0) curY = 0;
                    var index = 0;
                    var pos = -curY;
                    if (SupportScrollBar) GetPlusItemIndexAndPosAtGivenPos(-curY, ref index, ref pos);
                    var newItem = GetNewItemByIndex(index);
                    if (newItem == null) return false;
                    if (SupportScrollBar) SetItemSize(index, newItem.CachedRectTransform.rect.height, newItem.Padding);
                    mItemList.Add(newItem);
                    newItem.CachedRectTransform.anchoredPosition3D = new Vector3(newItem.StartPosOffset, pos, 0);
                    UpdateContentSize();
                    return true;
                }
                var tViewItem0 = mItemList[0];
                tViewItem0.CachedRectTransform.GetWorldCorners(mItemWorldCorners);
                var topPos0 = mViewPortRectTransform.InverseTransformPoint(mItemWorldCorners[1]);
                var downPos0 = mViewPortRectTransform.InverseTransformPoint(mItemWorldCorners[0]);

                if (!IsDraging && tViewItem0.ItemCreatedCheckFrameCount != mListUpdateCheckFrameCount
                               && mViewPortRectLocalCorners[0].y - topPos0.y > distanceForRecycle0)
                {
                    mItemList.RemoveAt(0);
                    RecycleItemTmp(tViewItem0);
                    if (!SupportScrollBar)
                    {
                        UpdateContentSize();
                        CheckIfNeedUpdataItemPos();
                    }
                    return true;
                }

                var tViewItem1 = mItemList[mItemList.Count - 1];
                tViewItem1.CachedRectTransform.GetWorldCorners(mItemWorldCorners);
                var topPos1 = mViewPortRectTransform.InverseTransformPoint(mItemWorldCorners[1]);
                var downPos1 = mViewPortRectTransform.InverseTransformPoint(mItemWorldCorners[0]);
                if (!IsDraging && tViewItem1.ItemCreatedCheckFrameCount != mListUpdateCheckFrameCount
                               && downPos1.y - mViewPortRectLocalCorners[1].y > distanceForRecycle1)
                {
                    mItemList.RemoveAt(mItemList.Count - 1);
                    RecycleItemTmp(tViewItem1);
                    if (!SupportScrollBar)
                    {
                        UpdateContentSize();
                        CheckIfNeedUpdataItemPos();
                    }
                    return true;
                }

                if (topPos1.y - mViewPortRectLocalCorners[1].y < distanceForNew1)
                {
                    if (tViewItem1.ItemIndex > mCurReadyMaxItemIndex)
                    {
                        mCurReadyMaxItemIndex = tViewItem1.ItemIndex;
                        mNeedCheckNextMaxItem = true;
                    }
                    var nIndex = tViewItem1.ItemIndex + 1;
                    if (nIndex <= mCurReadyMaxItemIndex || mNeedCheckNextMaxItem)
                    {
                        var newItem = GetNewItemByIndex(nIndex);
                        if (newItem == null)
                        {
                            mNeedCheckNextMaxItem = false;
                            CheckIfNeedUpdataItemPos();
                        }
                        else
                        {
                            if (SupportScrollBar)
                                SetItemSize(nIndex, newItem.CachedRectTransform.rect.height, newItem.Padding);
                            mItemList.Add(newItem);
                            var y = tViewItem1.CachedRectTransform.anchoredPosition3D.y +
                                    tViewItem1.CachedRectTransform.rect.height + tViewItem1.Padding;
                            newItem.CachedRectTransform.anchoredPosition3D = new Vector3(newItem.StartPosOffset, y, 0);
                            UpdateContentSize();
                            CheckIfNeedUpdataItemPos();
                            if (nIndex > mCurReadyMaxItemIndex) mCurReadyMaxItemIndex = nIndex;
                            return true;
                        }
                    }
                }


                if (mViewPortRectLocalCorners[0].y - downPos0.y < distanceForNew0)
                {
                    if (tViewItem0.ItemIndex < mCurReadyMinItemIndex)
                    {
                        mCurReadyMinItemIndex = tViewItem0.ItemIndex;
                        mNeedCheckNextMinItem = true;
                    }
                    var nIndex = tViewItem0.ItemIndex - 1;
                    if (nIndex >= mCurReadyMinItemIndex || mNeedCheckNextMinItem)
                    {
                        var newItem = GetNewItemByIndex(nIndex);
                        if (newItem == null)
                        {
                            mNeedCheckNextMinItem = false;
                            return false;
                        }
                        if (SupportScrollBar)
                            SetItemSize(nIndex, newItem.CachedRectTransform.rect.height, newItem.Padding);
                        mItemList.Insert(0, newItem);
                        var y = tViewItem0.CachedRectTransform.anchoredPosition3D.y -
                                newItem.CachedRectTransform.rect.height - newItem.Padding;
                        newItem.CachedRectTransform.anchoredPosition3D = new Vector3(newItem.StartPosOffset, y, 0);
                        UpdateContentSize();
                        CheckIfNeedUpdataItemPos();
                        if (nIndex < mCurReadyMinItemIndex) mCurReadyMinItemIndex = nIndex;
                        return true;
                    }
                }
            }

            return false;
        }


        private bool UpdateForHorizontalList( float distanceForRecycle0, float distanceForRecycle1,
            float distanceForNew0, float distanceForNew1 )
        {
            if (ItemTotalCount == 0)
            {
                if (mItemList.Count > 0) RecycleAllItem();
                return false;
            }
            if (ArrangeType == ListItemArrangeType.LeftToRight)
            {
                if (mItemList.Count == 0)
                {
                    var curX = ContainerTrans.anchoredPosition3D.x;
                    if (curX > 0) curX = 0;
                    var index = 0;
                    var pos = -curX;
                    if (SupportScrollBar) GetPlusItemIndexAndPosAtGivenPos(-curX, ref index, ref pos);
                    var newItem = GetNewItemByIndex(index);
                    if (newItem == null) return false;
                    if (SupportScrollBar) SetItemSize(index, newItem.CachedRectTransform.rect.width, newItem.Padding);
                    mItemList.Add(newItem);
                    newItem.CachedRectTransform.anchoredPosition3D = new Vector3(pos, newItem.StartPosOffset, 0);
                    UpdateContentSize();
                    return true;
                }
                var tViewItem0 = mItemList[0];
                tViewItem0.CachedRectTransform.GetWorldCorners(mItemWorldCorners);
                var leftPos0 = mViewPortRectTransform.InverseTransformPoint(mItemWorldCorners[1]);
                var rightPos0 = mViewPortRectTransform.InverseTransformPoint(mItemWorldCorners[2]);

                if (!IsDraging && tViewItem0.ItemCreatedCheckFrameCount != mListUpdateCheckFrameCount
                               && mViewPortRectLocalCorners[1].x - rightPos0.x > distanceForRecycle0)
                {
                    mItemList.RemoveAt(0);
                    RecycleItemTmp(tViewItem0);
                    if (!SupportScrollBar)
                    {
                        UpdateContentSize();
                        CheckIfNeedUpdataItemPos();
                    }
                    return true;
                }

                var tViewItem1 = mItemList[mItemList.Count - 1];
                tViewItem1.CachedRectTransform.GetWorldCorners(mItemWorldCorners);
                var leftPos1 = mViewPortRectTransform.InverseTransformPoint(mItemWorldCorners[1]);
                var rightPos1 = mViewPortRectTransform.InverseTransformPoint(mItemWorldCorners[2]);
                if (!IsDraging && tViewItem1.ItemCreatedCheckFrameCount != mListUpdateCheckFrameCount
                               && leftPos1.x - mViewPortRectLocalCorners[2].x > distanceForRecycle1)
                {
                    mItemList.RemoveAt(mItemList.Count - 1);
                    RecycleItemTmp(tViewItem1);
                    if (!SupportScrollBar)
                    {
                        UpdateContentSize();
                        CheckIfNeedUpdataItemPos();
                    }
                    return true;
                }


                if (rightPos1.x - mViewPortRectLocalCorners[2].x < distanceForNew1)
                {
                    if (tViewItem1.ItemIndex > mCurReadyMaxItemIndex)
                    {
                        mCurReadyMaxItemIndex = tViewItem1.ItemIndex;
                        mNeedCheckNextMaxItem = true;
                    }
                    var nIndex = tViewItem1.ItemIndex + 1;
                    if (nIndex <= mCurReadyMaxItemIndex || mNeedCheckNextMaxItem)
                    {
                        var newItem = GetNewItemByIndex(nIndex);
                        if (newItem == null)
                        {
                            mCurReadyMaxItemIndex = tViewItem1.ItemIndex;
                            mNeedCheckNextMaxItem = false;
                            CheckIfNeedUpdataItemPos();
                        }
                        else
                        {
                            if (SupportScrollBar)
                                SetItemSize(nIndex, newItem.CachedRectTransform.rect.width, newItem.Padding);
                            mItemList.Add(newItem);
                            var x = tViewItem1.CachedRectTransform.anchoredPosition3D.x +
                                    tViewItem1.CachedRectTransform.rect.width + tViewItem1.Padding;
                            newItem.CachedRectTransform.anchoredPosition3D = new Vector3(x, newItem.StartPosOffset, 0);
                            UpdateContentSize();
                            CheckIfNeedUpdataItemPos();

                            if (nIndex > mCurReadyMaxItemIndex) mCurReadyMaxItemIndex = nIndex;
                            return true;
                        }
                    }
                }

                if (mViewPortRectLocalCorners[1].x - leftPos0.x < distanceForNew0)
                {
                    if (tViewItem0.ItemIndex < mCurReadyMinItemIndex)
                    {
                        mCurReadyMinItemIndex = tViewItem0.ItemIndex;
                        mNeedCheckNextMinItem = true;
                    }
                    var nIndex = tViewItem0.ItemIndex - 1;
                    if (nIndex >= mCurReadyMinItemIndex || mNeedCheckNextMinItem)
                    {
                        var newItem = GetNewItemByIndex(nIndex);
                        if (newItem == null)
                        {
                            mCurReadyMinItemIndex = tViewItem0.ItemIndex;
                            mNeedCheckNextMinItem = false;
                        }
                        else
                        {
                            if (SupportScrollBar)
                                SetItemSize(nIndex, newItem.CachedRectTransform.rect.width, newItem.Padding);
                            mItemList.Insert(0, newItem);
                            var x = tViewItem0.CachedRectTransform.anchoredPosition3D.x -
                                    newItem.CachedRectTransform.rect.width - newItem.Padding;
                            newItem.CachedRectTransform.anchoredPosition3D = new Vector3(x, newItem.StartPosOffset, 0);
                            UpdateContentSize();
                            CheckIfNeedUpdataItemPos();
                            if (nIndex < mCurReadyMinItemIndex) mCurReadyMinItemIndex = nIndex;
                            return true;
                        }
                    }
                }
            }
            else
            {
                if (mItemList.Count == 0)
                {
                    var curX = ContainerTrans.anchoredPosition3D.x;
                    if (curX < 0) curX = 0;
                    var index = 0;
                    var pos = -curX;
                    if (SupportScrollBar)
                    {
                        GetPlusItemIndexAndPosAtGivenPos(curX, ref index, ref pos);
                        pos = -pos;
                    }
                    var newItem = GetNewItemByIndex(index);
                    if (newItem == null) return false;
                    if (SupportScrollBar) SetItemSize(index, newItem.CachedRectTransform.rect.width, newItem.Padding);
                    mItemList.Add(newItem);
                    newItem.CachedRectTransform.anchoredPosition3D = new Vector3(pos, newItem.StartPosOffset, 0);
                    UpdateContentSize();
                    return true;
                }
                var tViewItem0 = mItemList[0];
                tViewItem0.CachedRectTransform.GetWorldCorners(mItemWorldCorners);
                var leftPos0 = mViewPortRectTransform.InverseTransformPoint(mItemWorldCorners[1]);
                var rightPos0 = mViewPortRectTransform.InverseTransformPoint(mItemWorldCorners[2]);

                if (!IsDraging && tViewItem0.ItemCreatedCheckFrameCount != mListUpdateCheckFrameCount
                               && leftPos0.x - mViewPortRectLocalCorners[2].x > distanceForRecycle0)
                {
                    mItemList.RemoveAt(0);
                    RecycleItemTmp(tViewItem0);
                    if (!SupportScrollBar)
                    {
                        UpdateContentSize();
                        CheckIfNeedUpdataItemPos();
                    }
                    return true;
                }

                var tViewItem1 = mItemList[mItemList.Count - 1];
                tViewItem1.CachedRectTransform.GetWorldCorners(mItemWorldCorners);
                var leftPos1 = mViewPortRectTransform.InverseTransformPoint(mItemWorldCorners[1]);
                var rightPos1 = mViewPortRectTransform.InverseTransformPoint(mItemWorldCorners[2]);
                if (!IsDraging && tViewItem1.ItemCreatedCheckFrameCount != mListUpdateCheckFrameCount
                               && mViewPortRectLocalCorners[1].x - rightPos1.x > distanceForRecycle1)
                {
                    mItemList.RemoveAt(mItemList.Count - 1);
                    RecycleItemTmp(tViewItem1);
                    if (!SupportScrollBar)
                    {
                        UpdateContentSize();
                        CheckIfNeedUpdataItemPos();
                    }
                    return true;
                }


                if (mViewPortRectLocalCorners[1].x - leftPos1.x < distanceForNew1)
                {
                    if (tViewItem1.ItemIndex > mCurReadyMaxItemIndex)
                    {
                        mCurReadyMaxItemIndex = tViewItem1.ItemIndex;
                        mNeedCheckNextMaxItem = true;
                    }
                    var nIndex = tViewItem1.ItemIndex + 1;
                    if (nIndex <= mCurReadyMaxItemIndex || mNeedCheckNextMaxItem)
                    {
                        var newItem = GetNewItemByIndex(nIndex);
                        if (newItem == null)
                        {
                            mCurReadyMaxItemIndex = tViewItem1.ItemIndex;
                            mNeedCheckNextMaxItem = false;
                            CheckIfNeedUpdataItemPos();
                        }
                        else
                        {
                            if (SupportScrollBar)
                                SetItemSize(nIndex, newItem.CachedRectTransform.rect.width, newItem.Padding);
                            mItemList.Add(newItem);
                            var x = tViewItem1.CachedRectTransform.anchoredPosition3D.x -
                                    tViewItem1.CachedRectTransform.rect.width - tViewItem1.Padding;
                            newItem.CachedRectTransform.anchoredPosition3D = new Vector3(x, newItem.StartPosOffset, 0);
                            UpdateContentSize();
                            CheckIfNeedUpdataItemPos();

                            if (nIndex > mCurReadyMaxItemIndex) mCurReadyMaxItemIndex = nIndex;
                            return true;
                        }
                    }
                }

                if (rightPos0.x - mViewPortRectLocalCorners[2].x < distanceForNew0)
                {
                    if (tViewItem0.ItemIndex < mCurReadyMinItemIndex)
                    {
                        mCurReadyMinItemIndex = tViewItem0.ItemIndex;
                        mNeedCheckNextMinItem = true;
                    }
                    var nIndex = tViewItem0.ItemIndex - 1;
                    if (nIndex >= mCurReadyMinItemIndex || mNeedCheckNextMinItem)
                    {
                        var newItem = GetNewItemByIndex(nIndex);
                        if (newItem == null)
                        {
                            mCurReadyMinItemIndex = tViewItem0.ItemIndex;
                            mNeedCheckNextMinItem = false;
                        }
                        else
                        {
                            if (SupportScrollBar)
                                SetItemSize(nIndex, newItem.CachedRectTransform.rect.width, newItem.Padding);
                            mItemList.Insert(0, newItem);
                            var x = tViewItem0.CachedRectTransform.anchoredPosition3D.x +
                                    newItem.CachedRectTransform.rect.width + newItem.Padding;
                            newItem.CachedRectTransform.anchoredPosition3D = new Vector3(x, newItem.StartPosOffset, 0);
                            UpdateContentSize();
                            CheckIfNeedUpdataItemPos();
                            if (nIndex < mCurReadyMinItemIndex) mCurReadyMinItemIndex = nIndex;
                            return true;
                        }
                    }
                }
            }

            return false;
        }


        private float GetContentPanelSize()
        {
            if (SupportScrollBar)
            {
                var tTotalSize = mItemPosMgr.mTotalSize > 0 ? mItemPosMgr.mTotalSize - mLastItemPadding : 0;
                if (tTotalSize < 0) tTotalSize = 0;
                return tTotalSize;
            }
            var count = mItemList.Count;
            if (count == 0) return 0;
            if (count == 1) return mItemList[0].ItemSize;
            if (count == 2) return mItemList[0].ItemSizeWithPadding + mItemList[1].ItemSize;
            float s = 0;
            for (var i = 0; i < count - 1; ++i) s += mItemList[i].ItemSizeWithPadding;
            s += mItemList[count - 1].ItemSize;
            return s;
        }


        private void CheckIfNeedUpdataItemPos()
        {
            var count = mItemList.Count;
            if (count == 0) return;
            if (ArrangeType == ListItemArrangeType.TopToBottom)
            {
                var firstItem = mItemList[0];
                var lastItem = mItemList[mItemList.Count - 1];
                var viewMaxY = GetContentPanelSize();
                if (firstItem.TopY > 0 || firstItem.ItemIndex == mCurReadyMinItemIndex && firstItem.TopY != 0)
                {
                    UpdateAllShownItemsPos();
                    return;
                }
                if (-lastItem.BottomY > viewMaxY ||
                    lastItem.ItemIndex == mCurReadyMaxItemIndex && -lastItem.BottomY != viewMaxY)
                {
                    UpdateAllShownItemsPos();
                }
            }
            else if (ArrangeType == ListItemArrangeType.BottomToTop)
            {
                var firstItem = mItemList[0];
                var lastItem = mItemList[mItemList.Count - 1];
                var viewMaxY = GetContentPanelSize();
                if (firstItem.BottomY < 0 || firstItem.ItemIndex == mCurReadyMinItemIndex && firstItem.BottomY != 0)
                {
                    UpdateAllShownItemsPos();
                    return;
                }
                if (lastItem.TopY > viewMaxY || lastItem.ItemIndex == mCurReadyMaxItemIndex && lastItem.TopY != viewMaxY
                ) UpdateAllShownItemsPos();
            }
            else if (ArrangeType == ListItemArrangeType.LeftToRight)
            {
                var firstItem = mItemList[0];
                var lastItem = mItemList[mItemList.Count - 1];
                var viewMaxX = GetContentPanelSize();
                if (firstItem.LeftX < 0 || firstItem.ItemIndex == mCurReadyMinItemIndex && firstItem.LeftX != 0)
                {
                    UpdateAllShownItemsPos();
                    return;
                }
                if (lastItem.RightX > viewMaxX ||
                    lastItem.ItemIndex == mCurReadyMaxItemIndex && lastItem.RightX != viewMaxX)
                    UpdateAllShownItemsPos();
            }
            else if (ArrangeType == ListItemArrangeType.RightToLeft)
            {
                var firstItem = mItemList[0];
                var lastItem = mItemList[mItemList.Count - 1];
                var viewMaxX = GetContentPanelSize();
                if (firstItem.RightX > 0 || firstItem.ItemIndex == mCurReadyMinItemIndex && firstItem.RightX != 0)
                {
                    UpdateAllShownItemsPos();
                    return;
                }
                if (-lastItem.LeftX > viewMaxX ||
                    lastItem.ItemIndex == mCurReadyMaxItemIndex && -lastItem.LeftX != viewMaxX)
                    UpdateAllShownItemsPos();
            }
        }


        private void UpdateAllShownItemsPos()
        {
            var count = mItemList.Count;
            if (count == 0) return;

            mAdjustedVec = (ContainerTrans.anchoredPosition3D - mLastFrameContainerPos) / Time.deltaTime;

            if (ArrangeType == ListItemArrangeType.TopToBottom)
            {
                float pos = 0;
                if (SupportScrollBar) pos = -GetItemPos(mItemList[0].ItemIndex);
                var pos1 = mItemList[0].CachedRectTransform.anchoredPosition3D.y;
                var d = pos - pos1;
                var curY = pos;
                for (var i = 0; i < count; ++i)
                {
                    var item = mItemList[i];
                    item.CachedRectTransform.anchoredPosition3D = new Vector3(item.StartPosOffset, curY, 0);
                    curY = curY - item.CachedRectTransform.rect.height - item.Padding;
                }
                if (d != 0)
                {
                    Vector2 p = ContainerTrans.anchoredPosition3D;
                    p.y = p.y - d;
                    ContainerTrans.anchoredPosition3D = p;
                }
            }
            else if (ArrangeType == ListItemArrangeType.BottomToTop)
            {
                float pos = 0;
                if (SupportScrollBar) pos = GetItemPos(mItemList[0].ItemIndex);
                var pos1 = mItemList[0].CachedRectTransform.anchoredPosition3D.y;
                var d = pos - pos1;
                var curY = pos;
                for (var i = 0; i < count; ++i)
                {
                    var item = mItemList[i];
                    item.CachedRectTransform.anchoredPosition3D = new Vector3(item.StartPosOffset, curY, 0);
                    curY = curY + item.CachedRectTransform.rect.height + item.Padding;
                }
                if (d != 0)
                {
                    var p = ContainerTrans.anchoredPosition3D;
                    p.y = p.y - d;
                    ContainerTrans.anchoredPosition3D = p;
                }
            }
            else if (ArrangeType == ListItemArrangeType.LeftToRight)
            {
                float pos = 0;
                if (SupportScrollBar) pos = GetItemPos(mItemList[0].ItemIndex);
                var pos1 = mItemList[0].CachedRectTransform.anchoredPosition3D.x;
                var d = pos - pos1;
                var curX = pos;
                for (var i = 0; i < count; ++i)
                {
                    var item = mItemList[i];
                    item.CachedRectTransform.anchoredPosition3D = new Vector3(curX, item.StartPosOffset, 0);
                    curX = curX + item.CachedRectTransform.rect.width + item.Padding;
                }
                if (d != 0)
                {
                    var p = ContainerTrans.anchoredPosition3D;
                    p.x = p.x - d;
                    ContainerTrans.anchoredPosition3D = p;
                }
            }
            else if (ArrangeType == ListItemArrangeType.RightToLeft)
            {
                float pos = 0;
                if (SupportScrollBar) pos = -GetItemPos(mItemList[0].ItemIndex);
                var pos1 = mItemList[0].CachedRectTransform.anchoredPosition3D.x;
                var d = pos - pos1;
                var curX = pos;
                for (var i = 0; i < count; ++i)
                {
                    var item = mItemList[i];
                    item.CachedRectTransform.anchoredPosition3D = new Vector3(curX, item.StartPosOffset, 0);
                    curX = curX - item.CachedRectTransform.rect.width - item.Padding;
                }
                if (d != 0)
                {
                    var p = ContainerTrans.anchoredPosition3D;
                    p.x = p.x - d;
                    ContainerTrans.anchoredPosition3D = p;
                }
            }
            if (IsDraging)
            {
                ScrollRect.OnBeginDrag(mPointerEventData);
                ScrollRect.Rebuild(CanvasUpdate.PostLayout);
                ScrollRect.velocity = mAdjustedVec;
                mNeedAdjustVec = true;
            }
        }

        private void UpdateContentSize()
        {
            var size = GetContentPanelSize();
            if (IsVertList)
            {
                if (ContainerTrans.rect.height != size)
                    ContainerTrans.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, size);
            }
            else
            {
                if (ContainerTrans.rect.width != size)
                    ContainerTrans.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, size);
            }
        }

        private class SnapData
        {
            public float mCurSnapVal;
            public bool mIsForceSnapTo;
            public SnapStatus mSnapStatus = SnapStatus.NoTargetSet;
            public int mSnapTargetIndex;
            public float mTargetSnapVal;

            public void Clear()
            {
                mSnapStatus = SnapStatus.NoTargetSet;
                mIsForceSnapTo = false;
            }
        }
    }
}