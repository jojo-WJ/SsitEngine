/*
*┌──────────────────────────────────────────────────────────────┐
*│　描   述：                                                    
*│　作   者：xuXin                                              
*│　版   本：1.0.0                                                 
*│　创建时间：2019/7/15 11:35:53                     
*└──────────────────────────────────────────────────────────────┘
*/

using SsitEngine.Unity.Sound;
using UnityEngine;
using UnityEngine.Events;

namespace Framework.Event
{
    public class EventSound : Ss_Event
    {
        private string clipName;

        public UnityEvent FinishCallback = new UnityEvent();
        private AudioSource mAudioSource;
        private SoundDuckingSetting setting;
        private float volume;


        public EventSound( int id, float time ) : base(id, time)
        {
        }

        public override void Init( EventDataInfo info )
        {
            var soundInfo = info as EventSoundInfo;
            if (soundInfo == null)
            {
                Debug.LogError(" tweenInfo expression");
                return;
            }

            clipName = soundInfo.clipName;
            setting = soundInfo.setting;
            volume = soundInfo.volume;
            //Debug.Log("clipName" + clipName);
            FinishCallback.AddListener(InternalFinishCallback);
            base.Init(info);
        }


        protected override void ExecuteImpl()
        {
            //SoundManager.Load(clipName);
            mAudioSource = SoundManager.PlaySFXUI(clipName, false, 0, volume, 1, FinishCallback, setting, 0.1f, 0.5f);
        }


        protected override void AbortImpl()
        {
            if (mAudioSource) SoundManager.StopSFXObject(mAudioSource);
        }

        public override void Shutdown()
        {
            base.Shutdown();
        }

        private void InternalFinishCallback()
        {
            mAudioSource = null;
        }
    }
}