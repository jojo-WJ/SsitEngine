/*
*┌──────────────────────────────────────────────────────────────┐
*│　描   述：                                                    
*│　作   者：xuXin                                              
*│　版   本：1.0.0                                                 
*│　创建时间：2019/11/27 20:40:28                     
*└──────────────────────────────────────────────────────────────┘
*/

using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

namespace SsitEngine.Unity.UI.Common.Event
{
    /// <summary>
    ///     UI触发事件
    /// </summary>
    public class UnityUITriggerEvent : UnityEvent<PointerEventData>
    {
    }

    /// <summary>
    ///     UI响应数据事件
    /// </summary>
    public class UnityUIBaseDataEvent : UnityEvent<BaseEventData>
    {
    }

    /// <summary>
    ///     UI移动响应数据事件
    /// </summary>
    public class UnityUIAxisEvent : UnityEvent<AxisEventData>
    {
    }

    /// <summary>Event trigger for UI behaviour.</summary>
    [AddComponentMenu("MGS/UCommon/Event/MonoEventTrigger")]
    public class MonoEventTrigger : MonoBehaviour, IEventSystemHandler, IPointerEnterHandler, IPointerExitHandler,
        IPointerDownHandler, IPointerUpHandler, IPointerClickHandler, IBeginDragHandler,
        IInitializePotentialDragHandler, IDragHandler, IEndDragHandler, IDropHandler, IScrollHandler,
        IUpdateSelectedHandler, ISelectHandler, IDeselectHandler, IMoveHandler, ISubmitHandler, ICancelHandler
    {
        /// <summary>On pointer enter event.</summary>
        public UnityUITriggerEvent onPointerEnter { get; } = new UnityUITriggerEvent();

        /// <summary>On pointer exit event.</summary>
        public UnityUITriggerEvent onPointerExit { get; } = new UnityUITriggerEvent();

        /// <summary>On pointer down event.</summary>
        public UnityUITriggerEvent onPointerDown { get; } = new UnityUITriggerEvent();

        /// <summary>On pointer up event.</summary>
        public UnityUITriggerEvent onPointerUp { get; } = new UnityUITriggerEvent();

        /// <summary>On pointer click event.</summary>
        public UnityUITriggerEvent onPointerClick { get; } = new UnityUITriggerEvent();

        public UnityUITriggerEvent onDoubleClickedClick { get; } = new UnityUITriggerEvent();

        /// <summary>On begin drag event.</summary>
        public UnityUITriggerEvent onBeginDrag { get; } = new UnityUITriggerEvent();

        /// <summary>On initialize potential drag event.</summary>
        public UnityUITriggerEvent onInitializePotentialDrag { get; } = new UnityUITriggerEvent();

        /// <summary>On drag event.</summary>
        public UnityUITriggerEvent onDrag { get; } = new UnityUITriggerEvent();

        /// <summary>On end drag event.</summary>
        public UnityUITriggerEvent onEndDrag { get; } = new UnityUITriggerEvent();

        /// <summary>On drop event.</summary>
        public UnityUITriggerEvent onDrop { get; } = new UnityUITriggerEvent();

        /// <summary>On scroll event.</summary>
        public UnityUITriggerEvent onScroll { get; } = new UnityUITriggerEvent();

        /// <summary>On update selected event.</summary>
        public UnityUIBaseDataEvent onUpdateSelected { get; } = new UnityUIBaseDataEvent();

        /// <summary>On select event.</summary>
        public UnityUIBaseDataEvent onSelect { get; } = new UnityUIBaseDataEvent();

        /// <summary>On deselect event.</summary>
        public UnityUIBaseDataEvent onDeselect { get; } = new UnityUIBaseDataEvent();

        /// <summary>On move event.</summary>
        public UnityUIAxisEvent onMove { get; } = new UnityUIAxisEvent();

        /// <summary>On submit event.</summary>
        public UnityUIBaseDataEvent onSubmit { get; } = new UnityUIBaseDataEvent();

        /// <summary>On cancel event.</summary>
        public UnityUIBaseDataEvent onCancel { get; } = new UnityUIBaseDataEvent();

        public bool IsPressd { get; private set; }

        /// <summary>On begin drag.</summary>
        /// <param name="eventData">Pointer event data.</param>
        public void OnBeginDrag( PointerEventData eventData )
        {
            onBeginDrag.Invoke(eventData);
        }

        /// <summary>On cancel.</summary>
        /// <param name="eventData">Pointer event data.</param>
        public void OnCancel( BaseEventData eventData )
        {
            onCancel.Invoke(eventData);
        }

        /// <summary>On deselect.</summary>
        /// <param name="eventData">Pointer event data.</param>
        public void OnDeselect( BaseEventData eventData )
        {
            onDeselect.Invoke(eventData);
        }

        /// <summary>On drag.</summary>
        /// <param name="eventData">Pointer event data.</param>
        public void OnDrag( PointerEventData eventData )
        {
            onDrag.Invoke(eventData);
        }

        /// <summary>On drop.</summary>
        /// <param name="eventData">Pointer event data.</param>
        public void OnDrop( PointerEventData eventData )
        {
            onDrop.Invoke(eventData);
        }

        /// <summary>On end drag.</summary>
        /// <param name="eventData">Pointer event data.</param>
        public void OnEndDrag( PointerEventData eventData )
        {
            onEndDrag.Invoke(eventData);
        }

        /// <summary>On initialize potential drag.</summary>
        /// <param name="eventData">Pointer event data.</param>
        public void OnInitializePotentialDrag( PointerEventData eventData )
        {
            onInitializePotentialDrag.Invoke(eventData);
        }

        /// <summary>On move.</summary>
        /// <param name="eventData">Pointer event data.</param>
        public void OnMove( AxisEventData eventData )
        {
            onMove.Invoke(eventData);
        }

        /// <summary>On pointer click.</summary>
        /// <param name="eventData">Pointer event data.</param>
        public void OnPointerClick( PointerEventData eventData )
        {
            if (eventData.clickCount == 2)
                onDoubleClickedClick.Invoke(eventData);
            else
                onPointerClick.Invoke(eventData);
        }

        /// <summary>On pointer down.</summary>
        /// <param name="eventData">Pointer event data.</param>
        public void OnPointerDown( PointerEventData eventData )
        {
            IsPressd = true;

            onPointerDown.Invoke(eventData);
        }

        /// <summary>On pointer enter.</summary>
        /// <param name="eventData">Pointer event data.</param>
        public void OnPointerEnter( PointerEventData eventData )
        {
            onPointerEnter.Invoke(eventData);
        }

        /// <summary>On pointer exit.</summary>
        /// <param name="eventData">Pointer event data.</param>
        public void OnPointerExit( PointerEventData eventData )
        {
            onPointerExit.Invoke(eventData);
        }

        /// <summary>On pointer up.</summary>
        /// <param name="eventData">Pointer event data.</param>
        public void OnPointerUp( PointerEventData eventData )
        {
            IsPressd = false;

            onPointerUp.Invoke(eventData);
        }

        /// <summary>On scroll.</summary>
        /// <param name="eventData">Pointer event data.</param>
        public void OnScroll( PointerEventData eventData )
        {
            onScroll.Invoke(eventData);
        }

        /// <summary>On select.</summary>
        /// <param name="eventData">Pointer event data.</param>
        public void OnSelect( BaseEventData eventData )
        {
            onSelect.Invoke(eventData);
        }

        /// <summary>On submit.</summary>
        /// <param name="eventData">Pointer event data.</param>
        public void OnSubmit( BaseEventData eventData )
        {
            onSubmit.Invoke(eventData);
        }

        /// <summary>On update selected.</summary>
        /// <param name="eventData">Pointer event data.</param>
        public void OnUpdateSelected( BaseEventData eventData )
        {
            onUpdateSelected.Invoke(eventData);
        }

        /// <summary>
        ///     获取Trigger
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static MonoEventTrigger Get( GameObject obj )
        {
            var listener = obj.GetComponent<MonoEventTrigger>();
            if (listener == null) listener = obj.AddComponent<MonoEventTrigger>();
            return listener;
        }
    }
}