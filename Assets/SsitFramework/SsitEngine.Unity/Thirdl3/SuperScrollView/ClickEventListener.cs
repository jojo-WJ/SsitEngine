using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace SuperScrollView
{
    public class ClickEventListener : MonoBehaviour, IPointerClickHandler, IPointerDownHandler, IPointerUpHandler
    {
        private Action<GameObject> mClickedHandler;
        private Action<GameObject> mDoubleClickedHandler;
        private Action<GameObject> mOnPointerDownHandler;
        private Action<GameObject> mOnPointerUpHandler;

        public bool IsPressd { get; private set; }

        public void OnPointerClick( PointerEventData eventData )
        {
            if (eventData.clickCount == 2)
            {
                if (mDoubleClickedHandler != null)
                {
                    mDoubleClickedHandler(gameObject);
                }
            }
            else
            {
                if (mClickedHandler != null)
                {
                    mClickedHandler(gameObject);
                }
            }
        }


        public void OnPointerDown( PointerEventData eventData )
        {
            IsPressd = true;
            if (mOnPointerDownHandler != null)
            {
                mOnPointerDownHandler(gameObject);
            }
        }

        public void OnPointerUp( PointerEventData eventData )
        {
            IsPressd = false;
            if (mOnPointerUpHandler != null)
            {
                mOnPointerUpHandler(gameObject);
            }
        }

        public static ClickEventListener Get( GameObject obj )
        {
            var listener = obj.GetComponent<ClickEventListener>();
            if (listener == null)
            {
                listener = obj.AddComponent<ClickEventListener>();
            }
            return listener;
        }

        public void SetClickEventHandler( Action<GameObject> handler )
        {
            mClickedHandler = handler;
        }

        public void SetDoubleClickEventHandler( Action<GameObject> handler )
        {
            mDoubleClickedHandler = handler;
        }

        public void SetPointerDownHandler( Action<GameObject> handler )
        {
            mOnPointerDownHandler = handler;
        }

        public void SetPointerUpHandler( Action<GameObject> handler )
        {
            mOnPointerUpHandler = handler;
        }
    }
}