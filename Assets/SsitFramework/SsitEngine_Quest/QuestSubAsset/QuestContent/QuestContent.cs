using System;
using UnityEngine;
using System.Collections.Generic;

namespace SsitEngine.QuestManager
{

    /// <summary>
    /// UI内容的抽象基类
    /// 1、设置任务引用实例
    /// 2、添加任务内容标签
    /// </summary>
    public abstract class QuestContent : QuestSubasset
    {
        public virtual string OriginalText
        {
            get { return string.Empty; }
            set { }
        }

        public virtual string RuntimeText { get { return QuestMachineTags.ReplaceTags( OriginalText, quest ); } }

        public static void SetRuntimeReferences( List<QuestContent> contentList, Quest quest, QuestNode questNode )
        {
            if (contentList == null)
                return;
            for (int i = 0; i < contentList.Count; i++)
            {
                if (contentList[i] != null)
                    contentList[i].SetRuntimeReferences( quest, questNode );
            }
        }

        public override void AddTagsToDictionary()
        {
            AddTagsToDictionary( OriginalText );
        }

    }

}
