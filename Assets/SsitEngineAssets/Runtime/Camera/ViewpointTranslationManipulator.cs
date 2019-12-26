using DG.Tweening;
using UnityEngine;
using UnityEngine.Events;

namespace Framework
{
    /// <summary>
    /// 视点变换操作器
    /// </summary>
    public class ViewpointTranslationManipulator : MonoBehaviour, IInputState
    {
        private UnityAction onCancel;
        private Tweener tweenerPosition;
        private Tweener tweenerRotation;

        public ENCameraModeType CameraMode => ENCameraModeType.ViewpointTranslationManipulator;

        public void OnUpdate()
        {
        }

        public void OnLateUpdate()
        {
        }

        public bool CouldEnter()
        {
            return true;
        }

        public bool CouldLeave()
        {
            return null == tweenerPosition;
        }

        public void Enter()
        {
            enabled = true;
        }

        public bool Enable()
        {
            return enabled;
        }

        public void Leave()
        {
            enabled = false;
            Stop(false);
            Reset();
        }

        public void OnFixedUpdate()
        {
        }

        /// <summary>
        /// 转移视角，平滑插值
        /// </summary>
        /// <param name="position">目标相机位置</param>
        /// <param name="rotation">目标相机旋转</param>
        /// <param name="callbackCancel">中途取消回调</param>
        /// <param name="callbackComplete">完成回调</param>
        /// <param name="duration">耗时</param>
        public void TranslateViewpoint( Vector3 position, Vector3 rotation, UnityAction callbackCancel,
            UnityAction callbackComplete, float duration = 2f )
        {
            if (Camera.main)
            {
                if (null != onCancel) onCancel.Invoke();
                Stop(false);

                tweenerPosition = Camera.main.transform.DOMove(position, duration);
                tweenerRotation = Camera.main.transform.DORotate(rotation, duration);

                onCancel = callbackCancel;

                tweenerPosition.OnComplete(() =>
                {
                    Reset();
                    callbackComplete?.Invoke();
                });
            }
        }


        public void TranslateViewpoint( Vector3 position, Quaternion rotation
            , UnityAction callbackCancel = null
            , UnityAction callbackComplete = null
            , float duration = 2f )
        {
            if (Camera.main)
            {
                onCancel?.Invoke();

                Stop(false);

                tweenerPosition = Camera.main.transform.DOMove(position, duration);
                tweenerRotation = Camera.main.transform.DORotateQuaternion(rotation, duration);

                onCancel = callbackCancel;

                tweenerPosition.OnComplete(() =>
                {
                    Reset();
                    callbackComplete?.Invoke();
                });
            }
        }

        private void Reset()
        {
            tweenerPosition = null;
            tweenerRotation = null;
            onCancel = null;
        }

        public void Stop( bool complete )
        {
            if (null != tweenerPosition) tweenerPosition.Kill(complete);

            if (null != tweenerRotation) tweenerRotation.Kill(complete);

            Reset();
        }
    }
}