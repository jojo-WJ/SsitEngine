using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace SsitEngine.Unity.UI.Common.Utility
{
    /// <summary>Utility for EventSystem.</summary>
    public static class EventSystemUtility
    {
        /// <summary>Check mouse pointer is over target gameobject?</summary>
        /// <param name="target">Target gameobject.</param>
        /// <returns>Pointer is over target gameobject?</returns>
        public static bool CheckPointerOverGameObject( GameObject target )
        {
            if (EventSystem.current == null || !EventSystem.current.IsPointerOverGameObject())
                return false;
            var eventData = new PointerEventData(EventSystem.current)
            {
                position = Input.mousePosition
            };
            var raycastResults = new List<RaycastResult>();
            EventSystem.current.RaycastAll(eventData, raycastResults);
            foreach (var raycastResult in raycastResults)
                if (raycastResult.gameObject == target)
                    return true;
            return false;
        }
    }
}