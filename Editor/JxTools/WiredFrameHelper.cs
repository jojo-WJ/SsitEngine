/*
*┌──────────────────────────────────────────────────────────────┐
*│　描    述：平台主程序入口                                                    
*│　作    者：xuxin                                              
*│　版    本：1.0.0                                                 
*│　创建时间：2019/11/15 10:46:22                             
*└──────────────────────────────────────────────────────────────┘
*/

using UnityEngine;
using UnityEditor;
using NUnit.Framework;

namespace SsitEngine.Editor
{
    public class WiredFrameHelper
    {
        [MenuItem("Help/HideWiredFrame")]
        static void HideWire()
        {
            GameObject[] gos = Selection.gameObjects;
            if (gos.Length != 0)
            {
                foreach (var go in gos)
                {
                    Renderer[] renderers = go.GetComponentsInChildren<Renderer>();
                    foreach (var renderer in renderers)
                    {
//                    UnityEditor.EditorUtility.SetSelectedWireframeHidden(renderer, true);
                        UnityEditor.EditorUtility.SetSelectedRenderState(renderer, EditorSelectedRenderState.Hidden);
                    }
                }
            }
        }


        [MenuItem("Help/ShowWiredFrame")]
        static void ShowWire()
        {
            GameObject[] gos = Selection.gameObjects;
            if (gos.Length != 0)
            {
                foreach (var go in gos)
                {
                    Renderer[] renderers = go.GetComponentsInChildren<Renderer>();
                    foreach (var renderer in renderers)
                    {
//                    UnityEditor.EditorUtility.SetSelectedWireframeHidden(renderer, false);

                        UnityEditor.EditorUtility.SetSelectedRenderState(renderer, EditorSelectedRenderState.Highlight);
                    }
                }
            }
        }
    }
}