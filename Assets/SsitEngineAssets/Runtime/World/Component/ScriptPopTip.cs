/*
*┌──────────────────────────────────────────────────────────────┐
*│　描   述：UI弹窗对象                                                    
*│　作   者：xuXin                                              
*│　版   本：1.0.0                                                 
*│　创建时间：2019/9/21 11:19:15                     
*└──────────────────────────────────────────────────────────────┘
*/

using SsitEngine.Unity;
using UnityEngine;
using UnityEngine.UI;

namespace Framework.SceneObject
{
    public class ScriptPopTip : MonoBase
    {
        [SerializeField] [AddBindPath("tips")] private Text m_text;

        public float PreferredHeight => m_text.preferredHeight;

        public RectTransform RectTransform => m_text.rectTransform;


        public void SetText( string str )
        {
            m_text.text = str;
        }
    }
}