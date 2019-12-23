/*
*┌──────────────────────────────────────────────────────────────┐
*│　描   述：                                                    
*│　作   者：xuXin                                              
*│　版   本：1.0.0                                                 
*│　创建时间：2019/8/6 15:30:18                     
*└──────────────────────────────────────────────────────────────┘
*/

using UnityEngine;

namespace SsitEngine.Unity.HUD
{
    public static class HudExtension
    {
        #region CompassBar Extension Methods

        public static void ShowCompassBarDistance( this HudElement element, int distance = 0 )
        {
            if (element.CompassBar.DistanceText == null)
            {
                return;
            }

            // only update if value has changed
            var useDistanceText = element.useCompassBarDistanceText;
            if (useDistanceText != element.CompassBar.DistanceText.gameObject.activeSelf)
            {
                element.CompassBar.DistanceText.gameObject.SetActive(useDistanceText);
            }

            // update distance text if active
            if (useDistanceText) // TODO add TextMeshPro support?
            {
                element.CompassBar.DistanceText.text = string.Format(element.compassBarDistanceTextFormat, distance);
            }
        }

        #endregion

        #region Extension Methods

        public static float GetDistance( this HudElement element, Transform other )
        {
            return Vector2.Distance(new Vector2(element.transform.position.x, element.transform.position.z),
                new Vector2(other.position.x, other.position.z));
        }


        public static Vector3 GetPosition( this HudElement element )
        {
            return element.transform.position;
        }


        public static Vector3 GetPositionOffset( this HudElement element, Vector3 otherPosition )
        {
            return element.transform.position - otherPosition;
        }


        public static float GetRadius( this RectTransform rect )
        {
            var arr = new Vector3[4];
            rect.GetLocalCorners(arr);
            var _radius = Mathf.Abs(arr[0].y);
            if (Mathf.Abs(arr[0].x) < Mathf.Abs(arr[0].y))
            {
                _radius = Mathf.Abs(arr[0].x);
            }

            return _radius;
        }


        public static Vector3 KeepInRectBounds( this RectTransform rect, Vector3 markerPos, out bool outOfBounds )
        {
            var oldPos = markerPos;
            markerPos = Vector3.Min(markerPos, rect.rect.max);
            markerPos = Vector3.Max(markerPos, rect.rect.min);

            outOfBounds = oldPos != markerPos;

            return markerPos;
        }


        //public static float GetIconRadius(this HudElement element, NavigationElementType elementType)
        //{
        //    float radius = (elementType == NavigationElementType.Radar) ? element.Radar.PrefabRect.sizeDelta.x : element.Minimap.PrefabRect.sizeDelta.x;
        //    return radius / 2f;
        //}


        public static bool IsVisibleOnScreen( this HudElement element, Vector3 screenPos )
        {
            return screenPos.z > 0 && screenPos.x > 0 && screenPos.x < Screen.width && screenPos.y > 0 &&
                   screenPos.y < Screen.height;
        }


        public static void SetMarkerPosition( this HudElement element, NavigationElementType elementType,
            Vector3 markerPos, RectTransform parentRect = null )
        {
            // set marker position
            /*if (elementType == NavigationElementType.Radar)
                element.Radar.transform.localPosition = markerPos;*/
            if (elementType == NavigationElementType.HUD)
            {
                element.Hud.transform.localPosition = markerPos;
            }
            else if (elementType == NavigationElementType.CompassBar)
            {
                element.CompassBar.transform.position =
                    new Vector3(markerPos.x + parentRect.localPosition.x, parentRect.position.y, 0f);
            }
            else if (elementType == NavigationElementType.Minimap)
            {
                element.Minimap.transform.localPosition = markerPos;
            }
        }


        public static void SetMarkerActive( this HudElement element, NavigationElementType elementType, bool value )
        {
            // get marker gameobject
            GameObject markerGO = null;
            switch (elementType)
            {
                //case NavigationElementType.Radar:
                //    markerGO = element.Radar.gameObject;
                //    break;
                case NavigationElementType.HUD:
                    markerGO = element.Hud?.gameObject;
                    break;
                case NavigationElementType.CompassBar:
                    markerGO = element.CompassBar?.gameObject;
                    break;
                case NavigationElementType.Minimap:
                    markerGO = element.Minimap?.gameObject;
                    break;
            }

            // set marker gameobject active/inactive
            if (markerGO != null)
                // only update if value has changed
            {
                if (value != markerGO.activeSelf)
                {
                    // invoke events
                    if (value) // appeared
                    {
                        element.OnAppear.Invoke(element, elementType);
                    }
                    else // disappeared
                    {
                        element.OnDisappear.Invoke(element, elementType);
                    }

                    // set active state
                    markerGO.gameObject.SetActive(value);
                }
            }
        }

        public static Vector3 ConvertPosition( Canvas canvas, Vector3 pos, out bool isInRect )
        {
            Vector3 screenVec3Pos;
            screenVec3Pos = Camera.main.WorldToScreenPoint(pos);
            var screenVec2Pos = new Vector2(screenVec3Pos.x, screenVec3Pos.y);

            Vector3 xx;
            if (RectTransformUtility.ScreenPointToWorldPointInRectangle((RectTransform) canvas.transform, screenVec2Pos,
                canvas.worldCamera, out xx))
            {
                isInRect = true;
                return xx;
            }

            isInRect = false;
            return xx;
        }

        #endregion


        #region Radar Extension Methods

        //public static void ShowRadarAboveArrow(this HudElement element, bool value)
        //{
        //    if (element.Radar.ArrowAbove == null)
        //        return;

        //    // only update if value has changed
        //    if (value != element.Radar.ArrowAbove.gameObject.activeSelf)
        //        element.Radar.ArrowAbove.gameObject.SetActive(value);
        //}


        //public static void ShowRadarBelowArrow(this HudElement element, bool value)
        //{
        //    if (element.Radar.ArrowBelow == null)
        //        return;

        //    // only update if value has changed
        //    if (value != element.Radar.ArrowBelow.gameObject.activeSelf)
        //        element.Radar.ArrowBelow.gameObject.SetActive(value);
        //}

        #endregion


        #region Indicator Extension Methods

        public static void SetIndicatorActive( this HudElement element, bool value )
        {
            if (element.Indicator == null)
            {
                return;
            }

            // only update, if value has changed
            if (value != element.Indicator.gameObject.activeSelf)
            {
                // invoke events
                if (value) // appeared
                {
                    element.OnAppear.Invoke(element, NavigationElementType.Indicator);
                }
                else // disappeared
                {
                    element.OnDisappear.Invoke(element, NavigationElementType.Indicator);
                }

                // set indicator active/inactive
                element.Indicator.gameObject.SetActive(value);
            }
        }


        public static void ShowIndicatorDistance( this HudElement element, bool onScreen, int distance = 0 )
        {
            // show/hide distance text
            var distanceText =
                onScreen ? element.Indicator.OnscreenDistanceText : element.Indicator.OffscreenDistanceText;
            if (distanceText != null)
            {
                var showDistance = onScreen
                    ? element.useIndicatorDistanceText
                    : element.useIndicatorDistanceText && element.showOffscreenIndicatorDistance;

                // only update if value has changed
                if (showDistance != distanceText.gameObject.activeSelf)
                {
                    distanceText.gameObject.SetActive(showDistance);
                }

                // update distance text if active
                if (showDistance) // TODO add TextMeshPro support?
                {
                    distanceText.text =
                        string.Format(
                            onScreen
                                ? element.indicatorOnscreenDistanceTextFormat
                                : element.indicatorOffscreenDistanceTextFormat, distance);
                }
            }
        }


        public static void SetIndicatorOnOffscreen( this HudElement element, bool value )
        {
            // show/hide onscreen rect
            if (element.Indicator.OnscreenRect != null)
                // only update, if value has changed
            {
                if (value != element.Indicator.OnscreenRect.gameObject.activeSelf)
                {
                    element.Indicator.OnscreenRect.gameObject.SetActive(value);
                }
            }

            // show/hide offscreen rect
            if (element.Indicator.OffscreenRect != null)
                // only update, if value has changed
            {
                if (!value != element.Indicator.OffscreenRect.gameObject.activeSelf)
                {
                    element.Indicator.OffscreenRect.gameObject.SetActive(!value);
                }
            }
        }


        public static void SetIndicatorPosition( this HudElement element, Vector3 indicatorPos,
            RectTransform parentRect )
        {
            // set indicator position
            element.Indicator.transform.position = new Vector3(indicatorPos.x + parentRect.localPosition.x,
                indicatorPos.y + parentRect.localPosition.y, 0f);
        }


        public static void SetIndicatorOffscreenRotation( this HudElement element, Quaternion rotation )
        {
            // set indicator rotation
            if (element.Indicator.OffscreenPointer != null)
            {
                element.Indicator.OffscreenPointer.transform.rotation = rotation;
            }
        }


        public static void SetIndicatorScale( this HudElement element, float distance, float scaleRadius,
            float minScale )
        {
            if (element.ignoreIndicatorScaling)
            {
                return;
            }

            // set indicator scale
            var scale = (distance - 1f) / (scaleRadius - 1f);
            scale = Mathf.Clamp01(scale);
            element.Indicator.PrefabRect.localScale = Vector2.Lerp(Vector2.one * minScale, Vector2.one, scale);
        }

        #endregion

        #region HUD Extension Methods

        /// <summary>
        ///     设置HUD激活
        /// </summary>
        /// <param name="element"></param>
        /// <param name="value"></param>
        public static void SetHUDActive( this HudElement element, bool value )
        {
            // only update, if value has changed
            if (element.Hud && value != element.Hud.gameObject.activeSelf)
            {
                // invoke events
                if (value) // appeared
                {
                    element.OnAppear.Invoke(element, NavigationElementType.HUD);
                }
                else // disappeared
                {
                    element.OnDisappear.Invoke(element, NavigationElementType.HUD);
                }

                // set indicator active/inactive
                element.Hud.gameObject.SetActive(value);
            }
        }

        /// <summary>
        ///     设置Hud位置
        /// </summary>
        /// <param name="element"></param>
        /// <param name="indicatorPos"></param>
        /// <param name="ve"></param>
        /// <param name="smoothT"></param>
        /// <param name="parentRect"></param>
        public static void SetHudPosition( this HudElement element, Vector3 indicatorPos, ref Vector3 ve, float smoothT,
            RectTransform parentRect )
        {
            // set indicator position
            var tarPos = new Vector3(indicatorPos.x + parentRect.localPosition.x,
                indicatorPos.y + parentRect.localPosition.y, 0f);
            var curPos = element.Hud.transform.position;
            element.Hud.transform.position = Vector3.SmoothDamp(curPos, tarPos, ref ve, smoothT);
        }

        /// <summary>
        ///     设置Hud缩放
        /// </summary>
        /// <param name="element"></param>
        /// <param name="distance"></param>
        /// <param name="scaleRadius"></param>
        /// <param name="minScale"></param>
        public static void SetHudScale( this HudElement element, float distance, float scaleRadius, float minScale )
        {
            if (element.ignoreHudRadius)
            {
                return;
            }

            // set indicator scale
            var scale = (distance - 1f) / (scaleRadius - 1f);
            scale = Mathf.Clamp01(scale);
            element.Hud.PrefabRect.localScale = Vector2.Lerp(Vector2.one * minScale, Vector2.one, scale);
        }

        #endregion

        #region Minimap Extension Methods (私人版)

        public static Vector3 MinimapRotationOffset( this Transform _transform, Vector3 position )
        {
            var offset = position.x * new Vector2(_transform.right.x, -_transform.right.z);
            offset += position.y * new Vector2(-_transform.forward.x, _transform.forward.z);
            return offset;
        }


        public static void ShowMinimapAboveArrow( this HudElement element, bool value )
        {
            if (element.Minimap.ArrowAbove == null)
            {
                return;
            }

            // only update if value has changed
            if (value != element.Minimap.ArrowAbove.gameObject.activeSelf)
            {
                element.Minimap.ArrowAbove.gameObject.SetActive(value);
            }
        }


        public static void ShowMinimapBelowArrow( this HudElement element, bool value )
        {
            if (element.Minimap.ArrowBelow == null)
            {
                return;
            }

            // only update if value has changed
            if (value != element.Minimap.ArrowBelow.gameObject.activeSelf)
            {
                element.Minimap.ArrowBelow.gameObject.SetActive(value);
            }
        }

        #endregion


        #region MapProfile Extension Methods

        public static Vector2 GetMapUnitScale( this MapAsset profile )
        {
            return new Vector2(profile.MapTextureSize.x / profile.MapBounds.size.x,
                profile.MapTextureSize.y / profile.MapBounds.size.z);
        }


        public static float GetMapAspect( this MapAsset profile )
        {
            return profile.MapTextureSize.x / (float) profile.MapTextureSize.y;
        }

        #endregion
    }
}