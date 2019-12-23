/*
*┌──────────────────────────────────────────────────────────────┐
*│　描    述：平台主程序入口                                                    
*│　作    者：xuxin                                              
*│　版    本：1.0.0                                                 
*│　创建时间：2019/11/16 14:39:52                             
*└──────────────────────────────────────────────────────────────┘
*/

using System.Collections.Generic;
using UnityEngine;

namespace Framework.Editor
{
    [CreateAssetMenu(fileName = "BindSetting.asset", menuName = "Editor/JxTools/JxWindow")]
    public class BindSetting : ScriptableObject
    {
        //[FormerlySerializedAs("builtInRefrence")]
        public List<string> m_builtInRefrence;
    }
}