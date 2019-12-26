using System;
using System.Globalization;
using Framework.SceneObject;
using SsitEngine.Unity.SceneObject;
using UnityEngine;

namespace SsitEngine.Unity.Action
{
    /// <summary>
    /// 简单的UI交互行为(对象身上或者子物体上必须挂载animator组件)
    /// </summary>
    public class ActionAnimJoyControl : ActionBase
    {
        [SerializeField] private Animator animator;


        [Tooltip("是否直接控制角色前后方向移动")] [SerializeField]
        private bool isControlPlayerX;

        [Tooltip("是否直接控制角色前后方向移动")] [SerializeField]
        private bool isControlPlayerZ;

        [Tooltip("是否关联控制角色水平方向移动")] [SerializeField]
        private bool isRelationPlayerX;

        [Tooltip("是否关联控制角色前后方向移动")] [SerializeField]
        private bool isRelationPlayerZ;

        [Tooltip("看向对象")] [SerializeField] private Transform lookAtTrans;

        [Tooltip("关联人物状态")] [SerializeField] private EN_CharacterActionState m_state;

        [SerializeField] private float maxEularY = 60f;

        [SerializeField] private float minEularY = 5f;

        [SerializeField] private float mSpeedX = 10f;

        [SerializeField] private float mSpeedY = 1f;

        [AnimatorParameter] public string normalizeTime;


        public override void Execute( object sender, EnPropertyId actionId, string actionParam, object data = null )
        {
            if (actionId != EnPropertyId.SwitchControl) return;

            if (data is Player player)
            {
                var moveDelta = actionParam.ParseByDefault(Vector2.zero);
                if (player.SceneInstance.HasAuthority)
                    Move((BaseObject) sender, player, moveDelta);
            }
            else if (data is float animNor)
            {
                animNor = Mathf.Clamp(animNor, 0f, 0.4f);
                animator.SetFloat(normalizeTime, animNor);
            }
        }


        private void Move( BaseObject sender, Player player, Vector2 moveDelta )
        {
            //关联当前人物移动

            var deltaX = Math.Abs(moveDelta.x);
            var deltaY = Math.Abs(moveDelta.y);

            if (deltaX > float.Epsilon || deltaY > float.Epsilon)
            {
                if (deltaX > 0.0f)
                {
                    if (isRelationPlayerX)
                    {
                        if (m_state != EN_CharacterActionState.EN_CHA_None && player.State != m_state)
                            player.ChangeProperty(this, EnPropertyId.State, ((int) m_state).ToString());

                        //player.ChangeState(m_state);

                        //player.PlayerController.RotateToDirection(lookAtTrans.forward);
                        player.PlayerController.MoveCharacter(lookAtTrans.position);
                    }
                    else if (isControlPlayerX)
                    {
                        var represent = player.GetRepresent();
                        var curRot = represent.transform.localEulerAngles;
                        represent.transform.localRotation = Quaternion.Euler(curRot.x,
                            curRot.y + moveDelta.x * mSpeedX * Time.deltaTime, curRot.z);
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
                            curRot.z + moveDelta.y * mSpeedY * Time.deltaTime);
                    }

                    if (animator)
                    {
                        var curNormalTime = animator.GetFloat(normalizeTime);
                        curNormalTime += moveDelta.y * mSpeedY * Time.deltaTime;
                        curNormalTime = Mathf.Clamp(curNormalTime, 0f, 0.4f);
                        sender.ChangeProperty(this, EnPropertyId.AnimNormal,
                            curNormalTime.ToString(CultureInfo.InvariantCulture));
                        animator.SetFloat(normalizeTime, curNormalTime);
                    }
                }
            }
            else
            {
                //todo:stop
                //animator.SetFloat(normalizeTime, -1);
                if (player.State == m_state)
                    player.ChangeProperty(this, EnPropertyId.State,
                        ((int) EN_CharacterActionState.EN_CHA_Interactoin).ToString());
                //player.ChangeState(EN_CharacterActionState.EN_CHA_Interactoin);
            }
        }
    }
}