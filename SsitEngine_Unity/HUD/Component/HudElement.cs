/*
*┌──────────────────────────────────────────────────────────────┐
*│　描   述：                                                    
*│　作   者：xuXin                                              
*│　版   本：1.0.0                                                 
*│　创建时间：2019/8/6 14:03:58                     
*└──────────────────────────────────────────────────────────────┘
*/
//#define ActiveRadar

using UnityEngine;

namespace SsitEngine.Unity.HUD
{
    /// <summary>
    ///     hud元素
    /// </summary>
    public class HudElement : MonoBehaviour
    {
        #region Extension

        /// <summary>
        ///     更新hud（宫外部调用）
        /// </summary>
        /// <param name="element"></param>
        public void UpdateHUDElement( HudElement element )
        {
            if (element == null)
            {
                return;
            }

            if (!element.IsActive)
            {
                // disable all marker instances
                //element.SetMarkerActive(NavigationElementType.Radar, false);
                element.SetMarkerActive(NavigationElementType.CompassBar, false);
                element.SetMarkerActive(NavigationElementType.Minimap, false);
                element.SetIndicatorActive(false);

                // skip the element
            }

            var hudManager = HudManager.Instance;
            if (Hud == null || hudManager.HudCanvas == null || !gameObject.activeSelf)
            {
                element.SetHUDActive(false);
                return;
            }

            var _worldPos = element.GetPosition();
            var _screenPos = hudManager.PlayerCamera.WorldToScreenPoint(_worldPos);

            float distance = 0;
            if (hudManager.PlayerController != null)
            {
                distance = element.GetDistance(hudManager.PlayerController.transform);
            }
            else
            {
                distance = element.GetDistance(hudManager.PlayerCamera.transform);
            }

            if (element.showHud)
            {
                // check indicator distance
                if (distance > hudManager.hudRadius && !element.ignoreHudRadius)
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
                    var _isElementOnScreen = element.IsVisibleOnScreen(_screenPos);
                    if (!_isElementOnScreen)
                    {
                        element.SetHUDActive(false);
                        return;
                    }

                    // update indicator values
                    element.SetHudPosition(_screenPos, ref element.hudV, element.smoothTime,
                        hudManager.HudCanvas.HUD.ElementContainer);

                    element.SetHudScale(distance, hudManager.hudScaleRadius, hudManager.hudMinScale);
                    element.SetHUDActive(hudManager.hudHideDistance > 0f && !element.ignoreIndicatorHideDistance
                        ? distance > hudManager.hudHideDistance
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

        #region Variables

        public bool isPlayAwake;

        // MISC
        // todo :ScriptObject Test 
        // public HudSetting Settings;
        //hud预设管理器
        public HudCollection Prefabs = new HudCollection();

        // 雷达属性参数设置
        //public bool hideInRadar = false;
        //public bool ignoreRadarRadius = false;
        //public bool rotateWithGameObject = true;
        //public bool useRadarHeightSystem = true;

        // 罗盘参数
        [Header("罗盘")] public bool hideInCompassBar;

        public bool ignoreCompassBarRadius;
        public bool useCompassBarDistanceText = true;
        public string compassBarDistanceTextFormat = "{0}m";

        // INDICATOR SETTINGS
        [Header("指示器")] public bool showIndicator;

        public bool showOffscreenIndicator = true;
        public bool ignoreIndicatorRadius = true;
        public bool ignoreIndicatorHideDistance;
        public bool ignoreIndicatorScaling;
        public bool useIndicatorDistanceText = true;
        public bool showOffscreenIndicatorDistance;
        public string indicatorOnscreenDistanceTextFormat = "{0}m";
        public string indicatorOffscreenDistanceTextFormat = "{0}";

        // HUD
        [Header("HUD")] public bool showHud;

        public bool ignoreHudRadius = true;

        [HideInInspector] public Vector3 hudV;

        public float smoothTime;
        // MINIMAP
        //public bool hideInMinimap = false;
        //public bool ignoreMinimapRadius = false;
        //public bool rotateWithGameObjectMM = true;
        //public bool useMinimapHeightSystem = true;


        // EVENTS
        public NavigationElementEvent OnElementReady = new NavigationElementEvent();
        public NavigationTypeEvent OnAppear = new NavigationTypeEvent();
        public NavigationTypeEvent OnDisappear = new NavigationTypeEvent();
        public NavigationTypeEvent OnEnterRadius = new NavigationTypeEvent();
        public NavigationTypeEvent OnLeaveRadius = new NavigationTypeEvent();


        [HideInInspector] public bool m_isActive;


        //[HideInInspector]
        //public ScriptRadar Radar;
        [HideInInspector] public ScriptCompassBar CompassBar;

        [HideInInspector] public ScriptIndicator Indicator;

        [HideInInspector] public ScriptMinmap Minimap;

        [HideInInspector] public ScriptHud Hud;

        //[HideInInspector]
        //public bool IsInRadarRadius;
        [HideInInspector] public bool IsInCompassBarRadius;

        [HideInInspector] public bool IsInIndicatorRadius;

        [HideInInspector] public bool IsInHudRadius;

        [HideInInspector] public bool IsInMinimapRadius;


        protected bool _isInitialized;

        public bool HideInCompassBar
        {
            get => hideInCompassBar;
            set => hideInCompassBar = value;
        }

        public bool ShowIndicator
        {
            get => showIndicator;
            set => showIndicator = value;
        }

        public bool IsActive
        {
            get => m_isActive;
            set => m_isActive = value;
        }

        #endregion


        #region Main Methods

        private void Start()
        {
            // disable, if navigation system is missing
            if (HudManager.Instance == null)
            {
                Debug.LogError("HUDNavigationSystem not found in scene!");
                enabled = false;
                return;
            }

            // initialize settings
            //InitializeSettings();

            // initialize components
            if (isPlayAwake)
            {
                // 
                Initialize();
                IsActive = true;
            }
        }


        public void AttachTo( NavigationElementType elementType, bool value, bool isDestory = true )
        {
            switch (elementType)
            {
                case NavigationElementType.Radar:
                    //todo:
                    break;
                case NavigationElementType.CompassBar:
                {
                    if (value)
                    {
                        CreateCompassBarMarker();
                    }
                    else if (CompassBar != null && isDestory)
                    {
                        Destroy(CompassBar.gameObject);
                    }
                }
                    break;
                case NavigationElementType.Minimap:
                {
                    if (value)
                    {
                        CreateMinimapMarker();
                    }
                    else if (Minimap != null && isDestory)
                    {
                        Destroy(Minimap.gameObject);
                    }
                }
                    break;
                case NavigationElementType.Indicator:
                {
                    if (value)
                    {
                        CreateIndicatorMarker();
                    }
                    else if (Indicator != null && isDestory)
                    {
                        Destroy(Indicator.gameObject);
                    }
                }
                    break;
                case NavigationElementType.HUD:
                {
                    if (value)
                    {
                        CreateHUDMarker();
                    }
                    else if (Hud != null && isDestory)
                    {
                        Destroy(Hud.gameObject);
                    }
                }
                    break;
            }

            if (value)
            {
                if (HudManager.Instance != null)
                {
                    HudManager.Instance.AddNavigationElement(this);
                }
            }
            else
            {
                // remove element from the navigation system
                if (HudManager.Instance != null)
                {
                    HudManager.Instance.RemoveNavigationElement(this);
                }
            }

            IsActive = value;
        }


        private void OnDestroy()
        {
            // remove element from the navigation system
            if (Engine.Instance.HasModule(typeof(HudManager).FullName))
            {
                if (HudManager.Instance != null)
                {
                    HudManager.Instance.RemoveNavigationElement(this);
                }
            }
            // destroy all marker references
            //if (Radar != null)
            //    Destroy(Radar.gameObject);
            if (CompassBar != null)
            {
                Destroy(CompassBar.gameObject);
            }
            if (Indicator != null)
            {
                Destroy(Indicator.gameObject);
            }
            if (Minimap != null)
            {
                Destroy(Minimap.gameObject);
            }
            if (Hud != null)
            {
                Destroy(Hud.gameObject);
            }
        }


        public void Refresh()
        {
            enabled = false;

            // reset markers
            //Radar = null;
            CompassBar = null;
            Indicator = null;
            Minimap = null;
            Hud = null;
            // create marker references
            CreateMarkerReferences();

            enabled = true;
        }

        #endregion


        #region Utility Methods

        private void InitializeSettings()
        {
            // if (Settings == null)

            // misc
            //this.Prefabs = Settings.Prefabs;

#if ActiveRadar
            // radar settings
            this.hideInRadar = Settings.hideInRadar;
            this.ignoreRadarRadius = Settings.ignoreRadarRadius;
            this.rotateWithGameObject = Settings.rotateWithGameObject;
            this.useRadarHeightSystem = Settings.useRadarHeightSystem;
#endif

            //// compass bar settings
            //this.hideInCompassBar = Settings.hideInCompassBar;
            //this.ignoreCompassBarRadius = Settings.ignoreCompassBarRadius;
            //this.useCompassBarDistanceText = Settings.useCompassBarDistanceText;
            //this.compassBarDistanceTextFormat = Settings.compassBarDistanceTextFormat;

            //// indicator settings
            //this.showIndicator = Settings.showIndicator;
            //this.showOffscreenIndicator = Settings.showOffscreenIndicator;
            //this.ignoreIndicatorRadius = Settings.ignoreIndicatorRadius;
            //this.ignoreIndicatorHideDistance = Settings.ignoreIndicatorHideDistance;
            //this.ignoreIndicatorScaling = Settings.ignoreIndicatorScaling;
            //this.useIndicatorDistanceText = Settings.useIndicatorDistanceText;
            //this.showOffscreenIndicatorDistance = Settings.showOffscreenIndicatorDistance;
            //this.indicatorOnscreenDistanceTextFormat = Settings.indicatorOnscreenDistanceTextFormat;
            //this.indicatorOffscreenDistanceTextFormat = Settings.indicatorOffscreenDistanceTextFormat;

            // minimap settings
            //this.hideInMinimap = Settings.hideInMinimap;
            //this.ignoreMinimapRadius = Settings.ignoreMinimapRadius;
            //this.rotateWithGameObjectMM = Settings.rotateWithGameObjectMM;
            //this.useMinimapHeightSystem = Settings.useMinimapHeightSystem;
        }


        public void Initialize()
        {
            // create marker references
            CreateMarkerReferences();

            // add element to the navigation system
            if (HudManager.Instance != null)
            {
                HudManager.Instance.AddNavigationElement(this);
            }

            // set as initialized
            _isInitialized = true;

            // invoke events
            OnElementReady.Invoke(this);
        }


        private void CreateMarkerReferences()
        {
            //CreateRadarMarker();
            CreateCompassBarMarker();
            CreateIndicatorMarker();
            CreateMinimapMarker();
        }


        private void CreateRadarMarker()
        {
            //if (Prefabs.RadarPrefab == null)
            //    return;

            // create radar gameobject
            //GameObject radarGO = Instantiate(Prefabs.RadarPrefab.gameObject, Vector3.zero, Quaternion.identity) as GameObject;
            //radarGO.transform.SetParent(HudManager.Instance.Radar.ElementContainer, false);
            //radarGO.SetActive(false);

            //// assign radar prefab
            //Radar = radarGO.GetComponent<ScriptRadar>();
        }


        public void CreateCompassBarMarker()
        {
            if (Prefabs.CompassBarPrefab == null || CompassBar != null)
            {
                return;
            }

            // create compass bar gameobject
            var compassBarGO = Instantiate(Prefabs.CompassBarPrefab.gameObject, Vector3.zero, Quaternion.identity);
            compassBarGO.transform.SetParent(HudManager.Instance.CompassBarCanvas.ElementContainer, false);
            compassBarGO.SetActive(false);

            // assign compass bar prefab
            CompassBar = compassBarGO.GetComponent<ScriptCompassBar>();
        }


        public void CreateIndicatorMarker()
        {
            if (Prefabs.IndicatorPrefab == null || Indicator != null)
            {
                return;
            }

            // create indicator gameobject
            var indicatorGO = Instantiate(Prefabs.IndicatorPrefab.gameObject, Vector3.zero, Quaternion.identity);
            indicatorGO.transform.SetParent(HudManager.Instance.IndicatorCanvas.ElementContainer, false);
            indicatorGO.SetActive(false);

            // assign indicator prefab
            Indicator = indicatorGO.GetComponent<ScriptIndicator>();
        }

        /// <summary>
        ///     创建小地图预设
        /// </summary>
        private void CreateMinimapMarker()
        {
            if (Prefabs.MinimapPrefab == null || Minimap != null)
            {
                return;
            }

            // create minimap gameobject
            var minimapGO = Instantiate(Prefabs.MinimapPrefab.gameObject, Vector3.zero, Quaternion.identity);
            minimapGO.transform.SetParent(HudManager.Instance.MinMapCanvas.ElementContainer, false);
            minimapGO.SetActive(false);

            // assign minimap prefab
            Minimap = minimapGO.GetComponent<ScriptMinmap>();
        }

        /// <summary>
        ///     创建HUD预设
        /// </summary>
        private void CreateHUDMarker()
        {
            if (Prefabs.HUDPrefab == null || Hud != null)
            {
                return;
            }

            // create minimap gameobject
            var hudGo = Instantiate(Prefabs.HUDPrefab.gameObject, Vector3.zero, Quaternion.identity);
            hudGo.transform.SetParent(HudManager.Instance.HudCanvas.ElementContainer, false);
            hudGo.SetActive(false);

            // assign minimap prefab
            Hud = hudGo.GetComponent<ScriptHud>();
        }

        #endregion
    }
}