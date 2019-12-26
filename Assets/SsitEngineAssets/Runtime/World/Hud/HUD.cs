using System;
using Framework.Helper;
using Framework.SceneObject;
using SsitEngine.Unity;
using UnityEngine;
using UnityEngine.UI;

namespace Framework.Hud
{
    public class HUD : MonoBehaviour
    {
        public bool isInUse;

        private readonly bool isVisiable = true;
        private float maxScale;
        private CanvasGroup mCanvasGroup;

        public float mDelay = 10;
        private float mDist;

        private float minScale;

        public float mMaxDisPlayDis = 50;
        public float mMinDisPlayDis = 1;

        private Vector3 mPos;

        public float mScaleFactor = 3;
        public Text mStateText;
        private GameObject mTarget;

        public Text mText;
        private int mUpdateIntervalDis;

        /// <summary>
        /// get mTarget's Host
        /// </summary>
        public Vector3 Host
        {
            get
            {
                var hostRoot = mTarget.transform.Find("HeadHost");
                return hostRoot ? hostRoot.position : mTarget.transform.position;
            }
        }


        public float Distance
        {
            get => mDist;
            set
            {
                var floor = Mathf.FloorToInt(value * 2);
                if (floor != mUpdateIntervalDis)
                {
                    mUpdateIntervalDis = floor;
                    mDist = value;
                }
            }
        }

        public CanvasGroup _CanvasGroup
        {
            get
            {
                if (!mCanvasGroup) mCanvasGroup = GetComponent<CanvasGroup>();
                return mCanvasGroup;
            }
        }

        private void Awake()
        {
            if (!mText)
            {
                mText = transform.Get<Text>("NameText");
            }
            if (!mStateText)
            {
                mStateText = transform.Get<Text>("StateText");
            }
            mPos = transform.position;
        }


        /// <summary>
        /// set target
        /// </summary>
        /// <param name="player"></param>
        public void SetPlayer( GameObject player )
        {
            mTarget = player;
            ResetScaleLevel(EnObjectType.GamePlayer);

            TurnSize();
            gameObject.SetActive(true);
        }

        public void SetPlayer( BaseObject player )
        {
            mTarget = player.SceneInstance.gameObject;
            isInUse = true;
            ResetScaleLevel(player.SceneInstance.Type);
            ChangeNameText(ObjectHelper.GetPlayerName(player));
            TurnSize();
            //player.OnHUDChange = ChangeNameText;
            //player.OnHUDStateChange = ChangeStateText;
        }

        public void ChangePlayer( GameObject obj )
        {
            mTarget = obj;
            TurnSize();
        }

        /// <summary>
        /// Mono  
        /// </summary>
        private void LateUpdate()
        {
            if (!CheckUse()) return;
            gameObject.SetActive(mTarget.gameObject.activeSelf);

            TurnSize();
        }

        /// <summary>
        /// Follow Camera Trun Size
        /// </summary>
        public void TurnSize()
        {
            if (!isVisiable) return;
            if (!Camera.main) return;
            //更新位置
            if (mPos != Host)
            {
                transform.position = Host;
                mPos = Host;
            }
            //更新旋转
            transform.rotation =
                Quaternion.Slerp(transform.rotation, Camera.main.transform.rotation, Time.deltaTime * mDelay);

            //更新缩放
            Distance = (Camera.main.transform.position - transform.position).magnitude;
            var scale = Mathf.Lerp(minScale, maxScale, mDist / mScaleFactor);
            scale = (float) Math.Round(scale, 2);

            var tarScale = new Vector3(scale, scale, 1);
            transform.localScale = Vector3.Lerp(transform.localScale, tarScale, Time.deltaTime * mDelay);

            var threshold = mDist > mMaxDisPlayDis || mDist < mMinDisPlayDis ? 0 : 1;
            //this._CanvasGroup.alpha = Mathf.Lerp(this._CanvasGroup.alpha, threshold, Time.deltaTime);
            _CanvasGroup.alpha = threshold;
        }

        /// <summary>
        /// Check target is Use
        /// </summary>
        /// <returns></returns>
        public bool CheckUse()
        {
            if (mTarget == null)
            {
                isInUse = false;
                gameObject.SetActive(false);
                return false;
            }
            return true;
        }

        /// <summary>
        /// Change Text by Name
        /// </summary> 
        /// <param name="msg"></param>
        /// <param name="enable"></param>>
        public void ChangeNameText( string msg, bool enable = true )
        {
            if (!string.IsNullOrEmpty(msg) && mText) mText.text = msg;
            gameObject.SetActive(enable);
        }

        /// <summary>
        /// change state context
        /// </summary>
        /// <param name="msg"></param>
        /// <param name="enable"></param>>
        public void ChangeStateText( string msg )
        {
            if (mText) mStateText.text = msg;
        }


        //IEnumerator TweenScale()
        //{
        //    float timer = 0;
        //    float scale = Mathf.Lerp(0.02f, 10, mDist / 1e4f * mScaleFactor);
        //    scale = (float)System.Math.Round(scale, 2);
        //    Vector3 tarScale = new Vector3(scale, scale, 1);
        //    Vector3 curScale = transform.localScale;
        //    while (timer <= mScaleDuration)
        //    {
        //        timer += Time.deltaTime;
        //        transform.localScale = Vector3.Lerp(curScale, tarScale, timer / mScaleDuration);
        //        yield return new WaitForEndOfFrame();
        //    }
        //    yield return null;
        //}

        public void ResetScaleLevel( EnObjectType type )
        {
            switch (type)
            {
                case EnObjectType.GamePlayer:
                    minScale = 0.25f;
                    maxScale = 1.2f;
                    mMinDisPlayDis = 1.5f;
                    mMaxDisPlayDis = 25;
                    mScaleFactor = 10;
                    break;
                case EnObjectType.Vehicle:
                    minScale = 1;
                    maxScale = 2.5f;
                    mMinDisPlayDis = 10f;
                    mMaxDisPlayDis = 30;
                    mScaleFactor = 30;

                    break;
            }
        }
    }
}