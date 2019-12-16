/*
*┌──────────────────────────────────────────────────────────────┐
*│　描   述：                                                    
*│　作   者：xuXin                                              
*│　版   本：1.0.0                                                 
*│　创建时间：2019/4/11 15:08:52                     
*└──────────────────────────────────────────────────────────────┘
*/

using UnityEngine;

namespace SsitEngine.QuestManager
{    
    /// <summary>
    /// 任务步骤
    /// </summary>
    [System.Serializable]
    public class Step
    {
        [SerializeField]
        private Action succesAction;
        [SerializeField]
        private Action failureAction;
        [SerializeField]
        public int scoreValue;

        public Action SuccesAction
        {
            get
            {
                return succesAction;
            }

            set
            {
                succesAction = value;
            }
        }

        public Action FailureAction
        {
            get
            {
                return failureAction;
            }

            set
            {
                failureAction = value;
            }
        }

        public Step( Action succesAction, Action failureAction = null, int scoreValue = 0 )
        {
            this.SuccesAction = succesAction;
            this.FailureAction = failureAction;
            this.scoreValue = scoreValue;
        }

        public override string ToString()
        {
            return ((SuccesAction != null) ? SuccesAction.DisplayName : "(no-action)");
        }
    }
}
