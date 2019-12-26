/*
*┌──────────────────────────────────────────────────────────────┐
*│　描   述：                                                    
*│　作   者：xuXin                                              
*│　版   本：1.0.0                                                 
*│　创建时间：2019/8/6 14:21:28                     
*└──────────────────────────────────────────────────────────────┘
*/
//#define ActiveRadar

using System;
using System.Collections.Generic;
using SsitEngine.PureMVC.Interfaces;
using SsitEngine.Unity.HUD.Interface;
using UnityEngine;

namespace SsitEngine.Unity.HUD
{
    /// <summary>
    ///     Hud管理器
    /// </summary>
    public class HudManager : ManagerBase<HudManager>
    {
        #region Variables

        // REFERENCES
        protected Camera m_playerCamera;
        protected Transform m_playerController;
        protected _RotationReference m_rotationReference = _RotationReference.Camera;

#if ActiveRadar
        //雷达

        [Tooltip("如果要使用雷达功能，请启用")]
        public bool useRadar = true;
        [Tooltip("选择要使用的雷达模式")]
        public RadarModes radarMode = RadarModes.RotateRadar;
        [Tooltip("定义雷达变焦。更改值以缩放雷达。默认雷达变焦设置为1。")]
        public float radarZoom = 1f;
        [Tooltip("定义雷达半径。半径以外的元素将显示在雷达的边界上")]
        public float radarRadius = 50f;
        [Tooltip("定义最大雷达半径。半径之外的元素将被隐藏")]
        public float radarMaxRadius = 75f;
        [Tooltip("如果要显示指向上/下的箭头（如果元素实际位于某个距离的上方或下方），请启用。")]
        public bool useRadarHeightSystem = true;
        [Tooltip("向上激活元素上方箭头的最小距离")]
        public float radarDistanceAbove = 10f;
        [Tooltip("向下的最小距离激活元素的下箭头。")]
        public float radarDistanceBelow = 10f;
        [Tooltip("(DEBUG) 启用以显示雷达的高度小控件")]
        public bool showRadarHeightGizmos = false;
        [SerializeField] protected Vector2 radarHeightGizmoSize = new Vector2(100f, 100f);
        [SerializeField] protected Color radarHeightGizmoColor = new Color(0f, 0f, 1f, .4f);
#endif


        // 罗盘

        [Tooltip("是否使用指南针功能")] public bool useCompassBar = true;
        [Tooltip("定义指南针半径。不忽略半径的元素将隐藏在此半径之外")] public float compassBarRadius = 150f;

        // 指示器
        [Tooltip("是否启用指示器")] public bool useIndicators = true;

        [Tooltip("定义指示器半径。不忽略半径的指示器将隐藏在此半径之外。")]
        public float indicatorRadius = 25f;

        [Tooltip("定义指示器自动隐藏的距离。（0=无自动隐藏）")] public float indicatorHideDistance = 3f;

        [Tooltip("当元素不在屏幕上时，如果要使用屏幕外指示器，请启用。")]
        public bool useOffscreenIndicators = true;

        [Tooltip("增加该值可使指示器远离屏幕边界")] public float indicatorOffscreenBorder = .075f;
        [Tooltip("如果要在定义的半径内按距离缩放指示器，请启用")] public bool useIndicatorScaling = true;

        [Tooltip("定义指示器刻度半径。指示器将在此半径内缩放。必须小于或等于指示器半径")]
        public float indicatorScaleRadius = 15f;

        [Tooltip("指示器的最小刻度。如果不希望指示器缩放，请将值设置为1")]
        public float indicatorMinScale = .8f;

        // hud
        [Tooltip("是否启HUD")] public bool useHud; //当前系统使用统一hud卡顿太严重了，没办法
        [Tooltip("定义指示器自动隐藏的距离。（0=无自动隐藏）")] public float hudHideDistance = 3.5f;

        [Tooltip("定义指示器半径。不忽略半径的指示器将隐藏在此半径之外。")]
        public float hudRadius = 25f;

        [Tooltip("定义指示器刻度半径。指示器将在此半径内缩放。必须小于或等于指示器半径")]
        public float hudScaleRadius = 15f;

        [Tooltip("指示器的最小刻度。如果不希望指示器缩放，请将值设置为1")]
        public float hudMinScale = .8f;

        // 小地图 测试版
        //[Tooltip("是否启用小地图")]
        //public bool useMinimap = false;
        //[Tooltip("小地图资源")]
        //public MapAsset minimapProfile;
        //[Tooltip("选择小地图模式")]
        //public MinimapModes minimapMode = MinimapModes.RotatePlayer;
        //[Tooltip("定义小地图缩放")]
        //public float minimapScale = .25f;
        //[Tooltip("定义小地图半径。半径之外的元素将显示在小地图的边界上")]
        //public float minimapRadius = 75f;
        //[Tooltip("(DEBUG) 启用以显示小地图边界小控件")]
        //public bool showMinimapBounds = true;
        //[SerializeField]
        //protected Color minimapBoundsGizmoColor = new Color(0f, 1f, 0f, .85f);
        //[Tooltip("Enable, 如果要显示指向上/下的箭头（如果元素实际位于某个距离的上方或下方）")]
        //public bool useMinimapHeightSystem = true;
        //[Tooltip("Minimum distance upwards to activate the element's ABOVE arrow.")]
        //public float minimapDistanceAbove = 10f;
        //[Tooltip("Minimum distance downwards to activate the element's BELOW arrow.")]
        //public float minimapDistanceBelow = 10f;
        //[Tooltip("(DEBUG) Enable to show the minimap's height gizmos.")]
        //public bool showMinimapHeightGizmos = false;
        //[SerializeField]
        //protected Vector2 minimapHeightGizmoSize = new Vector2(100f, 100f);
        //[SerializeField]
        //protected Color minimapHeightGizmoColor = new Color(0f, 0f, 1f, .4f);

        [HideInInspector] public List<HudElement> NavigationElements;

        //ui interface


        //event handler


        public Camera PlayerCamera
        {
            get
            {
                if (m_playerCamera == null && Camera.main != null)
                {
                    m_playerCamera = Camera.main;
                }
                return m_playerCamera;
            }
        }

        public Transform PlayerController
        {
            get => m_playerController;
            set => m_playerController = value;
        }

        public _RotationReference RotationReference
        {
            get => m_rotationReference;
            set => m_rotationReference = value;
        }

        public ICompassBar CompassBarCanvas { get; private set; }

        public IIndicator IndicatorCanvas { get; private set; }

        public IMinMap MinMapCanvas { get; private set; }

        public IHud HudCanvas { get; private set; }

        #endregion

        #region Main Methods

        private void LateUpdate()
        {
            // update navigation elements
            UpdateNavigationElements();

            // get rotation reference
            var rotationReference = GetRotationReference();

            if (rotationReference == null)
            {
                return;
            }

            // update radar
            //if (useRadar)
            //	_HUDNavigationCanvas.UpdateRadar (rotationReference, radarMode);

            // update compass bar
            if (useCompassBar)
            {
                CompassBarCanvas?.UpdateCompassBar(rotationReference);
            }

            // update minimap
            //if (useMinimap && minimapProfile != null && m_playerController != null)
            //    mMinMapCanvas?.UpdateMinimap(rotationReference, minimapMode, m_playerController, minimapProfile, minimapScale);
        }

        private void UpdateNavigationElements()
        {
            if (NavigationElements.Count <= 0 || PlayerCamera == null)
            {
                return;
            }

            // update navigation elements
            foreach (var element in NavigationElements)
            {
                if (element == null)
                {
                    continue;
                }

                // check if element is active
                if (!element.IsActive)
                {
                    // disable all marker instances
                    //element.SetMarkerActive(NavigationElementType.Radar, false);
                    element.SetMarkerActive(NavigationElementType.CompassBar, false);
                    element.SetMarkerActive(NavigationElementType.Minimap, false);
                    element.SetIndicatorActive(false);

                    // skip the element
                    continue;
                }

                // cache element values
                var _worldPos = element.GetPosition();
                var _screenPos = m_playerCamera.WorldToScreenPoint(_worldPos);


                float _distance = 0;
                if (m_playerController != null)
                {
                    _distance = element.GetDistance(m_playerController.transform);
                }
                else
                {
                    _distance = element.GetDistance(m_playerCamera.transform);
                }


                // update radar
#if ActiveRadar
                if (useRadar && element.Radar != null)
                {
                    UpdateRadarElement(element, _screenPos, _distance);
                }
#endif


                // update compass bar
                if (useCompassBar && element.CompassBar != null)
                {
                    UpdateCompassBarElement(element, _screenPos, _distance);
                }

                // update indicator
                if (useIndicators && element.Indicator != null)
                {
                    UpdateIndicatorElement(element, _screenPos, _distance);
                }

                if (useHud && element.Hud != null)
                {
                    UpdateHUDElement(element, _screenPos, _distance);
                }

                // update minimap
                //if (useMinimap && element.Minimap != null)
                //    UpdateMinimapElement(element, _screenPos, _distance);
            }
        }

        /// <summary>
        ///     Add a navigation element to the collection.
        /// </summary>
        /// <param name="element">Element.</param>
        public void AddNavigationElement( HudElement element )
        {
            if (element == null)
            {
                return;
            }

            // add element, if it doesn't exist yet
            if (!NavigationElements.Contains(element))
            {
                NavigationElements.Add(element);
            }
        }

        /// <summary>
        ///     Remove a navigation element from the collection.
        /// </summary>
        /// <param name="element">Element.</param>
        public void RemoveNavigationElement( HudElement element )
        {
            if (element == null || NavigationElements == null)
            {
                return;
            }

            // remove element from list
            NavigationElements.Remove(element);
        }


        #region 系统雷达（躲猫猫呃）

#if ActiveRadar
        /// <summary>
        /// Enable / Disable the radar feature in runtime.
        /// </summary>
        /// <param name="value">value</param>
        public void EnableRadar(bool value)
        {
            if (useRadar != value)
            {
                useRadar = value;
                _HUDNavigationCanvas.ShowRadar(value);
            }
        }
#endif

        #endregion

        #region 系统罗盘

        /// <summary>
        ///     Enable / Disable the compass bar feature in runtime.
        /// </summary>
        /// <param name="value">value</param>
        public void EnableCompassBar( bool value )
        {
            if (useCompassBar != value)
            {
                useCompassBar = value;
                CompassBarCanvas?.ShowCompassBar(value);
            }
        }

        #endregion

        #region 系统小地图

        /// <summary>
        ///     Enable / Disable the minimap feature in runtime.
        /// </summary>
        /// <param name="value">value</param>
        public void EnableMinimap( bool value )
        {
            //if (useMinimap != value)
            //{
            //    useMinimap = value;
            //    mMinMapCanvas?.ShowMinimap(value);
            //}
        }

        #endregion

        #region 系统指示器

        /// <summary>
        ///     Enable / Disable the indicator feature in runtime.
        /// </summary>
        /// <param name="value">value</param>
        public void EnableIndicators( bool value )
        {
            if (useIndicators != value)
            {
                useIndicators = value;
                IndicatorCanvas?.ShowIndicators(value);
            }
        }

        #endregion

        #region 系统HUD

        /// <summary>
        ///     Enable / Disable the indicator feature in runtime.
        /// </summary>
        /// <param name="value">value</param>
        public void EnableHUD( bool value )
        {
            if (useHud != value)
            {
                useHud = value;
                HudCanvas?.ShowHuds(value);
            }
        }

        #endregion

        #endregion

        #region Radar Methods

#if ActiveRadar
        void UpdateRadarElement(HUDNavigationElement element, Vector3 screenPos, float distance)
        {
            float _scaledRadius = radarRadius * radarZoom;
            float _scaledMaxRadius = radarMaxRadius * radarZoom;

            // check if element is hidden within the radar
            if (element.hideInRadar)
            {
                element.SetMarkerActive(NavigationElementType.Radar, false);
                return;
            }

            // check distance
            if (distance > _scaledRadius)
            {
                // invoke events
                if (element.IsInRadarRadius)
                {
                    element.IsInRadarRadius = false;
                    element.OnLeaveRadius.Invoke(element, NavigationElementType.Radar);
                }

                // check max distance
                if (distance > _scaledMaxRadius && !element.ignoreRadarRadius)
                {
                    element.SetMarkerActive(NavigationElementType.Radar, false);
                    return;
                }

                // set scaled distance when out of range
                distance = _scaledRadius;
            }
            else
            {
                // invoke events
                if (!element.IsInRadarRadius)
                {
                    element.IsInRadarRadius = true;
                    element.OnEnterRadius.Invoke(element, NavigationElementType.Radar);
                }
            }

            // rotate marker within radar with gameobject?
            Transform rotationReference = GetRotationReference();
            if (radarMode == RadarModes.RotateRadar)
            {
                element.Radar.PrefabRect.rotation = Quaternion.identity;
                if (element.rotateWithGameObject)
                    element.Radar.Icon.transform.rotation =
 Quaternion.Euler(new Vector3(0f, 0f, -element.transform.eulerAngles.y + rotationReference.eulerAngles.y));
            }
            else
            {
                if (element.rotateWithGameObject)
                    element.Radar.Icon.transform.rotation =
 Quaternion.Euler(new Vector3(0f, 0f, -element.transform.eulerAngles.y));
            }

            // keep marker icon identity rotation?
            if (!element.rotateWithGameObject)
                element.Radar.Icon.transform.rotation = Quaternion.identity;

            // set marker active
            element.SetMarkerActive(NavigationElementType.Radar, true);

            // calculate marker position
            Vector3 posOffset = element.GetPositionOffset(PlayerController.position);
            Vector3 markerPos = new Vector3(posOffset.x, posOffset.z, 0f);
            markerPos.Normalize();
            markerPos *=
 (distance / _scaledRadius) * (_HUDNavigationCanvas.Radar.ElementContainer.GetRadius() - element.GetIconRadius(NavigationElementType.Radar));

            // set marker position
            element.SetMarkerPosition(NavigationElementType.Radar, markerPos);

            // handle marker's above/below arrows
            element.ShowRadarAboveArrow(useRadarHeightSystem && element.useRadarHeightSystem && element.IsInRadarRadius && -posOffset.y < -radarDistanceAbove);
            element.ShowRadarBelowArrow(useRadarHeightSystem && element.useRadarHeightSystem && element.IsInRadarRadius && -posOffset.y > radarDistanceBelow);
        }
#endif

        #endregion

        #region CompassBar Methods

        private void UpdateCompassBarElement( HudElement element, Vector3 screenPos, float distance )
        {
            if (CompassBarCanvas == null)
            {
                element.SetMarkerActive(NavigationElementType.CompassBar, false);
                return;
            }

            // check if element is hidden within the compass bar
            if (element.HideInCompassBar)
            {
                element.SetMarkerActive(NavigationElementType.CompassBar, false);
                return;
            }

            // check distance
            if (distance > compassBarRadius && !element.ignoreCompassBarRadius)
            {
                element.SetMarkerActive(NavigationElementType.CompassBar, false);

                // invoke events
                if (element.IsInCompassBarRadius)
                {
                    element.IsInCompassBarRadius = false;
                    element.OnLeaveRadius.Invoke(element, NavigationElementType.CompassBar);
                }
                return;
            }

            // invoke events
            if (!element.IsInCompassBarRadius)
            {
                element.IsInCompassBarRadius = true;
                element.OnEnterRadius.Invoke(element, NavigationElementType.CompassBar);
            }

            // set marker position
            if (screenPos.z <= 0)
            {
                // hide marker and skip element
                element.SetMarkerActive(NavigationElementType.CompassBar, false);
                return;
            }

            // show compass bar distance?
            element.ShowCompassBarDistance((int) distance);

            // set marker active
            element.SetMarkerActive(NavigationElementType.CompassBar, true);

            // set marker position
            element.SetMarkerPosition(NavigationElementType.CompassBar, screenPos,
                CompassBarCanvas.CompassBar.ElementContainer);
        }

        #endregion

        #region Indicator Methods

        private void UpdateIndicatorElement( HudElement element, Vector3 screenPos, float distance )
        {
            if (IndicatorCanvas == null)
            {
                element.SetIndicatorActive(false);
                return;
            }

            if (useIndicators && element.ShowIndicator)
            {
                // check indicator distance
                if (distance > indicatorRadius && !element.ignoreIndicatorRadius)
                {
                    element.SetIndicatorActive(false);

                    // invoke events
                    if (element.IsInIndicatorRadius)
                    {
                        element.IsInIndicatorRadius = false;
                        element.OnLeaveRadius.Invoke(element, NavigationElementType.Indicator);
                    }
                }
                else
                {
                    // check if element is visible on screen
                    var _isElementOnScreen = element.IsVisibleOnScreen(screenPos);
                    if (!_isElementOnScreen)
                    {
                        if (useOffscreenIndicators && element.showOffscreenIndicator)
                        {
                            // flip if indicator is behind us
                            if (screenPos.z < 0f)
                            {
                                screenPos.x = Screen.width - screenPos.x;
                                screenPos.y = Screen.height - screenPos.y;
                            }

                            // calculate off-screen position/rotation
                            var screenCenter = new Vector3(Screen.width, Screen.height, 0f) / 2f;
                            screenPos -= screenCenter;
                            var angle = Mathf.Atan2(screenPos.y, screenPos.x);
                            angle -= 90f * Mathf.Deg2Rad;
                            var cos = Mathf.Cos(angle);
                            var sin = -Mathf.Sin(angle);
                            var cotangent = cos / sin;
                            screenPos = screenCenter + new Vector3(sin * 50f, cos * 50f, 0f);

                            // is indicator inside the defined bounds?
                            var offset = Mathf.Min(screenCenter.x, screenCenter.y);
                            offset = Mathf.Lerp(0f, offset, indicatorOffscreenBorder);
                            var screenBounds = screenCenter - new Vector3(offset, offset, 0f);
                            var boundsY = cos > 0f ? screenBounds.y : -screenBounds.y;
                            screenPos = new Vector3(boundsY / cotangent, boundsY, 0f);

                            // when out of bounds, get point on appropriate side
                            if (screenPos.x > screenBounds.x) // out => right
                            {
                                screenPos = new Vector3(screenBounds.x, screenBounds.x * cotangent, 0f);
                            }
                            else if (screenPos.x < -screenBounds.x) // out => left
                            {
                                screenPos = new Vector3(-screenBounds.x, -screenBounds.x * cotangent, 0f);
                            }
                            screenPos += screenCenter;

                            // update indicator rotation
                            element.SetIndicatorOffscreenRotation(Quaternion.Euler(0f, 0f, angle * Mathf.Rad2Deg));
                        }
                        else
                        {
                            // hide indicator offscreen
                            element.SetIndicatorActive(false);
                            return;
                        }
                    }

                    // show indicator distance?
                    element.ShowIndicatorDistance(_isElementOnScreen, (int) distance);

                    // set indicator on/offscreen
                    element.SetIndicatorOnOffscreen(_isElementOnScreen);

                    // update indicator values
                    element.SetIndicatorPosition(screenPos, IndicatorCanvas.Indicator.ElementContainer);

                    element.SetIndicatorScale(distance, indicatorScaleRadius, indicatorMinScale);
                    element.SetIndicatorActive(indicatorHideDistance > 0f && !element.ignoreIndicatorHideDistance
                        ? distance > indicatorHideDistance
                        : true);

                    // invoke events
                    if (!element.IsInIndicatorRadius)
                    {
                        element.IsInIndicatorRadius = true;
                        element.OnEnterRadius.Invoke(element, NavigationElementType.Indicator);
                    }
                }
            }
            else
            {
                element.SetIndicatorActive(false);
            }
        }

        #endregion

        #region HUD Method

        private void UpdateHUDElement( HudElement element, Vector3 screenPos, float distance )
        {
            if (HudCanvas == null)
            {
                element.SetHUDActive(false);
                return;
            }

            if (useHud && element.showHud)
            {
                // check indicator distance
                if (distance > hudRadius && !element.ignoreHudRadius)
                {
                    element.SetHUDActive(false);

                    // invoke events
                    if (element.IsInHudRadius)
                    {
                        element.IsInHudRadius = false;
                        element.OnLeaveRadius.Invoke(element, NavigationElementType.HUD);
                    }
                }
                else
                {
                    // check if element is visible on screen
                    var _isElementOnScreen = element.IsVisibleOnScreen(screenPos);
                    if (!_isElementOnScreen)
                    {
                        element.SetHUDActive(false);
                        return;
                    }

                    // update indicator values
                    element.SetHudPosition(screenPos, ref element.hudV, element.smoothTime,
                        HudCanvas.HUD.ElementContainer);

                    element.SetHudScale(distance, hudScaleRadius, hudMinScale);
                    element.SetHUDActive(hudHideDistance > 0f && !element.ignoreIndicatorHideDistance
                        ? distance > hudHideDistance
                        : true);

                    // invoke events
                    if (!element.IsInHudRadius)
                    {
                        element.IsInHudRadius = true;
                        element.OnEnterRadius.Invoke(element, NavigationElementType.HUD);
                    }
                }
            }
            else
            {
                element.SetHUDActive(false);
            }
        }

        #endregion

        #region Minimap Methods 待测试（私人版）

        //void UpdateMinimapElement(HudElement element, Vector3 screenPos, float distance)
        //{
        //    if (mMinMapCanvas == null)
        //    {
        //        element.SetMarkerActive(NavigationElementType.Minimap, false);
        //        return;
        //    }

        //    // check if element is hidden within the minimap
        //    //if (element.hideInMinimap)
        //    {
        //        element.SetMarkerActive(NavigationElementType.Minimap, false);
        //        return;
        //    }

        //    // check distance
        //    if (distance > minimapRadius)
        //    {
        //        // invoke events
        //        if (element.IsInMinimapRadius)
        //        {
        //            element.IsInMinimapRadius = false;
        //            element.OnLeaveRadius.Invoke(element, NavigationElementType.Minimap);
        //        }

        //        // hide element
        //        //if (!element.ignoreMinimapRadius)
        //        {
        //            element.SetMarkerActive(NavigationElementType.Minimap, false);
        //            return;
        //        }
        //    }
        //    else
        //    {
        //        // invoke events
        //        if (!element.IsInMinimapRadius)
        //        {
        //            element.IsInMinimapRadius = true;
        //            element.OnEnterRadius.Invoke(element, NavigationElementType.Minimap);
        //        }
        //    }

        //    // rotate marker within minimap with gameobject?
        //    //if (element.rotateWithGameObjectMM)
        //    //element.Minimap.Icon.transform.rotation = Quaternion.Euler(new Vector3(0f, 0f, -element.transform.eulerAngles.y));
        //    //else
        //    //element.Minimap.Icon.transform.rotation = Quaternion.identity;

        //    // set marker active
        //    element.SetMarkerActive(NavigationElementType.Minimap, true);

        //    // calculate marker position
        //    Vector2 unitScale = minimapProfile.GetMapUnitScale();
        //    Vector3 posOffset = element.GetPositionOffset(m_playerController.position);
        //    Vector3 markerPos = new Vector3(posOffset.x * unitScale.x, posOffset.z * unitScale.y, 0f) * minimapScale;

        //    // adjust marker position when using minimap rotation mode
        //    if (minimapMode == MinimapModes.RotateMinimap)
        //        markerPos = m_playerController.MinimapRotationOffset(markerPos);

        //    // always keep marker within minimap rect
        //    bool outOfBounds = false;

        //    markerPos = mMinMapCanvas.Minimap.ElementContainer.KeepInRectBounds(markerPos, out outOfBounds);


        //    // set marker position
        //    element.SetMarkerPosition(NavigationElementType.Minimap, markerPos);

        //    // handle marker's above/below arrows
        //    //element.ShowMinimapAboveArrow(useMinimapHeightSystem && element.useMinimapHeightSystem && !outOfBounds && -posOffset.y < -minimapDistanceAbove);
        //    //element.ShowMinimapBelowArrow(useMinimapHeightSystem && element.useMinimapHeightSystem && !outOfBounds && -posOffset.y > minimapDistanceBelow);
        //}

        #endregion

        #region Module Method

        /// <inheritdoc />
        public override string ModuleName => typeof(HudManager).FullName;

        /// <summary>
        ///     计时器模块优先级
        /// </summary>
        public override int Priority => (int) EnModuleType.ENMODULEDEFAULT;


        /// <inheritdoc />
        public override void OnSingletonInit()
        {
            NavigationElements = new List<HudElement>();
        }

        /// <summary>
        ///     计时器刷新
        /// </summary>
        /// <param name="elapsed"></param>
        public override void OnUpdate( float elapsed )
        {
        }

        /// <inheritdoc />
        public override void Shutdown()
        {
            base.Shutdown();

            //销毁hudElment
            NavigationElements.Clear();
            NavigationElements = null;

            CompassBarCanvas = null;
            IndicatorCanvas = null;
            MinMapCanvas = null;

            Destroy(gameObject);
        }

        #endregion

        #region Event Method

        private void OnEnable()
        {
            m_msgList = new[]
            {
                (ushort) EnHudEvent.SetCompassBar,
                (ushort) EnHudEvent.SetIndicator,
                (ushort) EnHudEvent.SetMinmap,
                (ushort) EnHudEvent.SetObjectHud
            };
            RegisterMsg(m_msgList);
        }

        private void OnDisable()
        {
            UnRegisterMsg(m_msgList);
        }

        /// <inheritdoc />
        public override void HandleNotification( INotification notification )
        {
            switch (notification.Id)
            {
                case (ushort) EnHudEvent.SetCompassBar:
                {
                    CompassBarCanvas = (ICompassBar) notification.Body;
                }
                    break;
                case (ushort) EnHudEvent.SetIndicator:
                {
                    IndicatorCanvas = (IIndicator) notification.Body;
                }
                    break;
                case (ushort) EnHudEvent.SetMinmap:
                {
                    MinMapCanvas = (IMinMap) notification.Body;
                }
                    break;
                case (ushort) EnHudEvent.SetObjectHud:
                {
                    HudCanvas = (IHud) notification.Body;
                }
                    break;
            }
        }

        #endregion

        #region Utility Methods

        public Transform GetRotationReference()
        {
            return m_rotationReference == _RotationReference.Camera ? PlayerCamera?.transform : m_playerController;
        }

        /*

                void OnDrawGizmos()
                {
        #if UNITY_EDITOR
                    if (PlayerController == null || Selection.activeGameObject != this.gameObject)
                        return;

                    // draw height system debug gizmos
                    bool _radarHeightGizmos = useRadarHeightSystem && showRadarHeightGizmos;
                    bool _minimapHeightGizmos = useMinimapHeightSystem && showMinimapHeightGizmos;
                    if (_radarHeightGizmos || _minimapHeightGizmos) {
                        Vector3 playerPos = PlayerController.position;

                        // draw radar height planes
                        if (_radarHeightGizmos) {
                            Gizmos.color = radarHeightGizmoColor;
                            Gizmos.DrawCube (playerPos + (Vector3.up * radarDistanceAbove), new Vector3(radarHeightGizmoSize.x, .01f, radarHeightGizmoSize.y));
                            Gizmos.DrawCube (playerPos - (Vector3.up * radarDistanceBelow), new Vector3(radarHeightGizmoSize.x, .01f, radarHeightGizmoSize.y));
                        }

                        // draw minimap height planes
                        if (_minimapHeightGizmos) {
                            Gizmos.color = minimapHeightGizmoColor;
                            Gizmos.DrawCube (playerPos + (Vector3.up * minimapDistanceAbove), new Vector3(minimapHeightGizmoSize.x, .01f, minimapHeightGizmoSize.y));
                            Gizmos.DrawCube (playerPos - (Vector3.up * minimapDistanceBelow), new Vector3(minimapHeightGizmoSize.x, .01f, minimapHeightGizmoSize.y));
                        }
                    }

                    // draw map bounds
                    if (showMinimapBounds && minimapProfile != null) {
                        Gizmos.color = minimapBoundsGizmoColor;
                        Gizmos.DrawWireCube (minimapProfile.MapBounds.center, minimapProfile.MapBounds.size);
                    }
        #endif
                }*/

        #endregion
    }


    #region Subclasses

    [Serializable]
    public enum _RotationReference
    {
        Camera,
        Controller
    }


    [Serializable]
    public enum RadarModes
    {
        RotateRadar,
        RotatePlayer
    }


    [Serializable]
    public enum MinimapModes
    {
        RotateMinimap,
        RotatePlayer
    }

    #endregion
}