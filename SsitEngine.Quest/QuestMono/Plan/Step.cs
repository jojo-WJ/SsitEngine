/*
*┌──────────────────────────────────────────────────────────────┐
*│　描   述：                                                    
*│　作   者：xuXin                                              
*│　版   本：1.0.0                                                 
*│　创建时间：2019/4/11 15:08:52                     
*└──────────────────────────────────────────────────────────────┘
*/

using System;
using UnityEngine;

namespace SsitEngine.QuestManager
{
    /// <summary>
    /// 任务步骤
    /// </summary>
    [Serializable]
    public class Step
    {
        [SerializeField] private Action failureAction;

        [SerializeField] public int scoreValue;

        [SerializeField] private Action succesAction;

        public Step( Action succesAction, Action failureAction = null, int scoreValue = 0 )
        {
            SuccesAction = succesAction;
            FailureAction = failureAction;
            this.scoreValue = scoreValue;
        }

        public Action SuccesAction
        {
            get => succesAction;

            set => succesAction = value;
        }

        public Action FailureAction
        {
            get => failureAction;

            set => failureAction = value;
        }

        public override string ToString()
        {
            return SuccesAction != null ? SuccesAction.DisplayName : "(no-action)";
        }
    }
}