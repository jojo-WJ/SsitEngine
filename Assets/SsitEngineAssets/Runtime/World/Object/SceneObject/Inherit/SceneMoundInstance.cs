using System.Collections.Generic;
using SsitEngine.PureMVC.Patterns;
using UnityEngine;

namespace Framework.SceneObject
{
    public class SceneMoundInstance : BaseSceneInstance, ITriggle
    {
        private List<Vector3> mNodes;
        private Vector3 curSelectPos;

        public override void Init(string guid = null)
        {
            base.Init(guid);
            CanEdit = true;
            mType = EnObjectType.Mound;
        }

        public void SetCurSelectNode(Vector3 tmp)
        {
            curSelectPos = tmp;
        }

        public override Vector3 GetPosition()
        {
            return curSelectPos;
        }

        #region Internal Members
        public void SetVertexes(List<Vector3> nodes)
        {
            mNodes = nodes;
        }
        public List<Vector3> GetVertexes()
        {
            return mNodes;
        }

        #endregion

        #region ITrigger

        public TriggleType TriggleType
        {
            get { return TriggleType.MouseClick_Draw_Triggle; }
        }
        public bool Check(BaseSceneInstance baseObj)
        {
            return false;
        }

        public void Enter(BaseSceneInstance sceneObj)
        {
        }

        public void Stay(BaseSceneInstance sceneObj)
        {
        }

        public void Exit(BaseSceneInstance sceneObj)
        {
        }

        public BaseSceneInstance OnPostTriggle(Vector3 point)
        {
            curSelectPos = point;
            return this;
        }

        #endregion

        #region 回放

        public override void SynchronizeProperties(SavedBase savedState, bool isReset, bool isFristFrame)
        {
            if (gameObject.activeSelf != savedState.isActive)
            {
                gameObject.SetActive(savedState.isActive);
                //todo:通知代理刷新
                Facade.Instance.SendNotification((ushort)EnDarwArrowEvent.OnAddDrawnArrow,Guid,savedState.isActive);
            }
        }

        #endregion
    }
}



