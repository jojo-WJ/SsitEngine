using System;
using System.Collections.Generic;
using UnityEngine;

namespace SsitEngine.QuestManager
{
    /// <summary>
    /// 任务文本列表
    /// 1、设置运行时的引用
    /// </summary>
    [Serializable]
    public class QuestContentSet
    {
        [SerializeField] private List<QuestContent> m_contentList = new List<QuestContent>();

        /// <summary>
        /// The content contained in this content set.
        /// </summary>
        public List<QuestContent> contentList
        {
            get => m_contentList;
            set => m_contentList = value;
        }

        public void SetRuntimeReferences( Quest quest, QuestNode questNode )
        {
            if (contentList == null)
            {
                return;
            }
            for (var i = 0; i < contentList.Count; i++)
            {
                if (contentList[i] != null)
                {
                    contentList[i].SetRuntimeReferences(quest, questNode);
                }
            }
        }

        public void DestroySubassets()
        {
            QuestSubasset.DestroyList(contentList);
        }
    }
}