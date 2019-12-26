using System;
using Framework.SceneObject;
using SsitEngine.Unity.SceneObject;
using UnityEngine;

namespace SsitEngine.Unity.Action
{
    /// <summary>
    /// 简单的UI交互行为
    /// </summary>
    public class ActionJoyControl : ActionBase
    {
        [SerializeField] 
        private float c_fSpeed = 10f;

        [Tooltip("是否直接控制角色前后方向移动")] [SerializeField]
        private bool isControlPlayerX;

        [Tooltip("是否直接控制角色前后方向移动")] [SerializeField]
        private bool isControlPlayerZ;

        [Tooltip("是否关联控制角色水平方向移动")] [SerializeField]
        private bool isRelationPlayerX;

        [Tooltip("是否关联控制角色前后方向移动")] [SerializeField]
        private bool isRelationPlayerZ;

        [Tooltip("看向对象")] [SerializeField]
#pragma warning disable 649
        private Transform lookAtTrans;
#pragma warning restore 649
        [Tooltip("垂直控制物体")] [SerializeField] private GameObject m_HorizentalAngle;

        [Tooltip("关联人物状态")] [SerializeField] private EN_CharacterActionState m_state;

        [Tooltip("水平控制物体")] [SerializeField] private GameObject m_VerticalAngle;

        [Range(5, 60)] [SerializeField] private float maxEularY = 60f;

        [Range(5, 60)] [SerializeField] private float minEularY = 5f;

        public bool IsControlPlayerX
        {
            get => isControlPlayerX;
            set => isControlPlayerX = value;
        }

        public bool IsControlPlayerZ
        {
            get => isControlPlayerZ;
            set => isControlPlayerZ = value;
        }

        public bool IsRelationPlayerZ
        {
            get => isRelationPlayerZ;
            set => isRelationPlayerZ = value;
        }

        public bool IsRelationPlayerX
        {
            get => isRelationPlayerX;
            set => isRelationPlayerX = value;
        }

        public GameObject VerticalAngle
        {
            get => m_VerticalAngle;
            set => m_VerticalAngle = value;
        }

        public GameObject HorizentalAngle
        {
            get => m_HorizentalAngle;
            set => m_HorizentalAngle = value;
        }

        public override void Execute( object sender, EnPropertyId actionId, string actionParam, object data = null )
        {
            if (actionId != EnPropertyId.SwitchControl) return;
            var moveDelta = actionParam.ParseByDefault(Vector2.zero);


            if (data is Player player)
            {
                if (player.SceneInstance.HasAuthority)
                    Move((BaseObject) sender, player, moveDelta);
            }
            else if (data == sender)
            {
                Move(moveDelta);
            }
        }


        private void Move( BaseObject sender, Player player, Vector2 moveDelta )
        {
            float deltaX = 0;
            float deltaY = 0;

            if (m_HorizentalAngle)
            {
                var curEulerAngle = m_HorizentalAngle.transform.eulerAngles;

                deltaX = moveDelta.x * c_fSpeed * Time.deltaTime + curEulerAngle.y;
                m_HorizentalAngle.transform.eulerAngles = new Vector3(curEulerAngle.x, deltaX, curEulerAngle.z);
            }

            if (m_VerticalAngle)
            {
                var curEulerAngle = m_VerticalAngle.transform.eulerAngles;
                if (curEulerAngle.x > 180f) curEulerAngle.x -= 360f;
                deltaY = Mathf.Clamp(moveDelta.y * c_fSpeed * Time.deltaTime + curEulerAngle.x, minEularY, maxEularY);
                m_VerticalAngle.transform.eulerAngles = new Vector3(deltaY, curEulerAngle.y, curEulerAngle.z);
            }

            sender.ChangeProperty(sender, EnPropertyId.SwitchControl, new Vector2(deltaX, deltaY).ToString());

            //关联当前人物移动
            deltaX = Math.Abs(moveDelta.x);
            deltaY = Math.Abs(moveDelta.y);

            if (deltaX > float.Epsilon || deltaY > float.Epsilon)
            {
                if (deltaX > 0.0f)
                {
                    if (isRelationPlayerX)
                    {
                        if (m_state != EN_CharacterActionState.EN_CHA_None && player.State != m_state)
                            player.ChangeProperty(this, EnPropertyId.State, ((int) m_state).ToString());
                        //player.ChangeState(m_state);

                        player.PlayerController.DirectMoveCharacter(lookAtTrans);
                    }
                    else if (isControlPlayerX)
                    {
                        if (player.SceneInstance.HasAuthority)
                        {
                            var represent = player.GetRepresent();
                            var curRot = represent.transform.localEulerAngles;
                            represent.transform.localRotation = Quaternion.Euler(curRot.x,
                                curRot.y * c_fSpeed * Time.deltaTime + moveDelta.x, curRot.z);
                        }
                    }
                }

                if (deltaY > 0.0f)
                {
                    if (isRelationPlayerZ)
                    {
                        if (m_state != EN_CharacterActionState.EN_CHA_None && player.State != m_state)
                            player.ChangeProperty(this, EnPropertyId.State, ((int) m_state).ToString());
                        //player.ChangeState(m_state);
                        player.PlayerController.MoveCharacter(lookAtTrans.position);
                    }
                    else if (isControlPlayerZ)
                    {
                        var represent = player.GetRepresent();
                        var curRot = represent.transform.localPosition;
                        represent.transform.localPosition = new Vector3(curRot.x, curRot.y,
                            curRot.z + moveDelta.y * c_fSpeed * Time.deltaTime);
                    }
                }
            }
            else
            {
                if (player.State == m_state)
                    player.ChangeProperty(this, EnPropertyId.State,
                        ((int) EN_CharacterActionState.EN_CHA_Interactoin).ToString());
                //player.ChangeState(EN_CharacterActionState.EN_CHA_Interactoin);
            }
        }

        private void Move( Vector2 moveDelta )
        {
            if (m_HorizentalAngle)
            {
                var curEulerAngle = m_HorizentalAngle.transform.eulerAngles;
                m_HorizentalAngle.transform.eulerAngles = new Vector3(curEulerAngle.x, moveDelta.x, curEulerAngle.z);
            }

            if (m_VerticalAngle)
            {
                var curEulerAngle = m_VerticalAngle.transform.eulerAngles;
                m_VerticalAngle.transform.eulerAngles = new Vector3(moveDelta.y, curEulerAngle.y, curEulerAngle.z);
            }
        }
    }
}