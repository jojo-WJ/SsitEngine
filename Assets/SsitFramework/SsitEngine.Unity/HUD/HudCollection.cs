/*
*┌──────────────────────────────────────────────────────────────┐
*│　描   述：                                                    
*│　作   者：xuXin                                              
*│　版   本：1.0.0                                                 
*│　创建时间：2019/8/6 14:11:48                     
*└──────────────────────────────────────────────────────────────┘
*/

using System;
using UnityEngine;
using UnityEngine.Events;

namespace SsitEngine.Unity.HUD
{
    /// <summary>
    ///     HUD的预设集合
    /// </summary>
    [Serializable]
    public class HudCollection
    {
        public ScriptCompassBar CompassBarPrefab;

        //接口预留
        //public ScriptRadar RadarPrefab;
        //头部:hud
        public ScriptHudPrefab HUDPrefab;
        public ScriptIndicator IndicatorPrefab;

        [Tooltip("未完成-预留(不要用)")] public ScriptMinmap MinimapPrefab;
    }


    [Serializable]
    public enum NavigationElementType
    {
        Radar,
        CompassBar,
        Indicator,
        Minimap,
        HUD
    }


    [Serializable]
    public class NavigationElementEvent : UnityEvent<HudElement>
    {
    }


    [Serializable]
    public class NavigationTypeEvent : UnityEvent<HudElement, NavigationElementType>
    {
    }
}