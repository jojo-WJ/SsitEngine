using System;
using System.Collections;
using System.Collections.Generic;
using Invector.CharacterController;
using UnityEngine;

namespace Invector
{
    [vClassHeader("MoveSet Speed", "Use this to add extra speed into a specific MoveSet")]
    public class vMoveSetSpeed : vMonoBehaviour
    {
        private vThirdPersonMotor cc;

        private int currentMoveSet;
        private readonly vMoveSetControlSpeed defaultFree = new vMoveSetControlSpeed();
        private readonly vMoveSetControlSpeed defaultStrafe = new vMoveSetControlSpeed();

        public List<vMoveSetControlSpeed> listFree;
        public List<vMoveSetControlSpeed> listStrafe;

        private void Start()
        {
            cc = GetComponent<vThirdPersonMotor>();

            defaultFree.walkSpeed = cc.freeSpeed.walkSpeed;
            defaultFree.runningSpeed = cc.freeSpeed.runningSpeed;
            defaultFree.sprintSpeed = cc.freeSpeed.sprintSpeed;

            defaultStrafe.walkSpeed = cc.strafeSpeed.walkSpeed;
            defaultStrafe.runningSpeed = cc.strafeSpeed.runningSpeed;
            defaultStrafe.sprintSpeed = cc.strafeSpeed.sprintSpeed;

            StartCoroutine(UpdateMoveSetSpeed());
        }

        private IEnumerator UpdateMoveSetSpeed()
        {
            while (true)
            {
                yield return new WaitForSeconds(0.1f);
                ChangeSpeed();
            }
        }

        private void ChangeSpeed()
        {
            currentMoveSet = (int) Mathf.Round(cc.animator.GetFloat("MoveSet_ID"));
            var strafing = cc.isStrafing;
            if (strafing)
            {
                var extraSpeed = listStrafe.Find(l => l.moveset == currentMoveSet);
                if (extraSpeed != null)
                {
                    cc.freeSpeed.walkSpeed = extraSpeed.walkSpeed;
                    cc.freeSpeed.runningSpeed = extraSpeed.runningSpeed;
                    cc.freeSpeed.sprintSpeed = extraSpeed.sprintSpeed;
                    cc.freeSpeed.crouchSpeed = extraSpeed.crouchSpeed;
                }
                else
                {
                    cc.strafeSpeed.walkSpeed = defaultStrafe.walkSpeed;
                    cc.strafeSpeed.runningSpeed = defaultStrafe.runningSpeed;
                    cc.strafeSpeed.sprintSpeed = defaultStrafe.sprintSpeed;
                    cc.strafeSpeed.crouchSpeed = defaultStrafe.crouchSpeed;
                }
            }
            else
            {
                var extraSpeed = listFree.Find(l => l.moveset == currentMoveSet);
                if (extraSpeed != null)
                {
                    cc.freeSpeed.walkSpeed = extraSpeed.walkSpeed;
                    cc.freeSpeed.runningSpeed = extraSpeed.runningSpeed;
                    cc.freeSpeed.sprintSpeed = extraSpeed.sprintSpeed;
                    cc.freeSpeed.crouchSpeed = extraSpeed.crouchSpeed;
                }
                else
                {
                    cc.strafeSpeed.walkSpeed = defaultFree.walkSpeed;
                    cc.strafeSpeed.runningSpeed = defaultFree.runningSpeed;
                    cc.strafeSpeed.sprintSpeed = defaultFree.sprintSpeed;
                    cc.strafeSpeed.crouchSpeed = defaultFree.crouchSpeed;
                }
            }
        }

        [Serializable]
        public class vMoveSetControlSpeed
        {
            public float crouchSpeed = 1.5f;
            public int moveset;
            public float runningSpeed = 1.5f;
            public float sprintSpeed = 1.5f;
            public float walkSpeed = 1.5f;
        }
    }
}