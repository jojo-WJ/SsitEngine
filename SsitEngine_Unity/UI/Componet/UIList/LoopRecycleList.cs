/*
*┌──────────────────────────────────────────────────────────────┐
*│　描   述：                                                    
*│　作   者：xuXin                                              
*│　版   本：1.0.0                                                 
*│　创建时间：2019/11/28 14:32:43                     
*└──────────────────────────────────────────────────────────────┘
*/

using UnityEngine.EventSystems;

namespace SsitEngine.Unity.UI
{
    public class LoopRecycleList : MonoBase, IBeginDragHandler, IEndDragHandler, IDragHandler
    {
        #region 接口实现

        public void OnBeginDrag( PointerEventData eventData )
        {
            /*if (eventData.button != PointerEventData.InputButton.Left)
            {
                return;
            }
            mIsDraging = true;
            CacheDragPointerEventData(eventData);
            mCurSnapData.Clear();
            if (mOnBeginDragAction != null)
            {
                mOnBeginDragAction();
            }*/
        }

        public void OnEndDrag( PointerEventData eventData )
        {
            /*if (eventData.button != PointerEventData.InputButton.Left)
            {
                return;
            }
            mIsDraging = false;
            mPointerEventData = null;
            if (mOnEndDragAction != null)
            {
                mOnEndDragAction();
            }
            ForceSnapUpdateCheck();*/
        }

        public void OnDrag( PointerEventData eventData )
        {
            /*if (eventData.button != PointerEventData.InputButton.Left)
            {
                return;
            }
            CacheDragPointerEventData(eventData);
            if (mOnDragingAction != null)
            {
                mOnDragingAction();
            }*/
        }

        #endregion
    }
}