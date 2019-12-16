using UnityEngine;

namespace SsitEngine.QuestManager
{

    /// <summary>
    /// 请求操作的抽象基类，在请求或请求节点时执行
    /// </summary>
    public abstract class QuestAction : QuestSubasset
    {

        /// <summary>
        /// Performs the quest action.
        /// </summary>
        public virtual void Execute(QuestNode node)
        {

        }

    }

}
