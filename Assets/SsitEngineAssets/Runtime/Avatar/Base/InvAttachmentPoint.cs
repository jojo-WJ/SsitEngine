using SsitEngine.Unity.Timer;
using UnityEngine;

namespace SsitEngine.Unity.Avatar
{
    public class InvAttachmentPoint : MonoBehaviour
    {
        [Disable] public GameObject mCurGameObject;


#if UNITY_EDITOR
        //[PreviewTexture( 60 )]
        //public string textureURL = "https://ss1.bdstatic.com/70cFvXSh_Q1YnxGkpoWK1HF6hhy/it/u=4237120115,924064044&fm=26&gp=0.jpg";
        [Space(5)] [Header("式样展示仅限编辑器")] [PreviewTexture]
        public Texture preview;

#endif
        [EnumLabel("装备类型插巢")] public InvSlot slot;

        [EnumLabel("使用类型节点")] public InvUseNodeType useNodeType;

        [EnumLabel("使用类型插巢")] public InvUseSlot useSlot;

        #region MyRegion

        public void Attach( GameObject represent, int combineIndex, Vector3 offset )
        {
            if (represent != null)
            {
                //Temp deal default equip
                transform.DestroyChildren();

                mCurGameObject = represent;
                switch ((InvCombineType) combineIndex)
                {
                    case InvCombineType.Inv_Combine:
                    case InvCombineType.Inv_SimpleCombine:
                        represent.SetActive(false);
                        break;
                }

                var ct = represent.transform;

                ct.parent = transform;
                ct.localPosition = Vector3.zero + offset;
                ct.localRotation = Quaternion.identity;
                ct.localScale = Vector3.one;
            }
        }

        public void Attach( GameObject represent )
        {
            if (represent != null)
            {
                //Temp deal default equip
                transform.DestroyChildren();

                var ct = represent.transform;

                ct.parent = transform;
                ct.localPosition = Vector3.zero;
                ct.localRotation = Quaternion.identity;
                ct.localScale = Vector3.one;
            }
        }

        public void Detach( bool isDestory = true )
        {
            mCurGameObject = null;
        }

        private TimerEventTask mFollowTask;

        public void Follow( GameObject represent )
        {
            if (represent != null)
            {
                //Temp deal default equip
//                transform.DestroyChildren();
//
//                Transform ct = represent.transform;
//
//                ct.parent = transform;
//                ct.localPosition = Vector3.zero;
//                ct.localRotation = Quaternion.identity;
//                ct.localScale = Vector3.one;
                if (mFollowTask != null) DeFollow();
                mFollowTask = Engine.Instance.Platform.AddTimerEvent(TimerEventType.TeveDelaySpanAlways, 0, 0, 0.25f,
                    ( eve, timeElapsed, data ) =>
                    {
                        var ct = (data as GameObject)?.transform;
                        //ct.parent = transform;
                        ct.position = transform.position;
                        ct.rotation = transform.rotation;
                        //ct.localScale = Vector3.one;
                    }, represent);
            }
        }

        public void DeFollow()
        {
            if (mFollowTask != null)
            {
                Engine.Instance.Platform.RemoveTimerEvent(mFollowTask);
                mFollowTask = null;
            }
            mCurGameObject = null;
        }

        public void DirectDetach( bool isDestory = true )
        {
            if (mCurGameObject != null) Destroy(mCurGameObject);
            mCurGameObject = null;
        }

        public void Reset( bool isDestory = true )
        {
            transform.DestroyChildren();
        }


        public void SetActive( bool value )
        {
            for (var i = 0; i < transform.childCount; i++) transform.GetChild(i).gameObject.SetActive(value);
        }

        #endregion
    }
}