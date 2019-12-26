/*
*┌──────────────────────────────────────────────────────────────┐
*│　描   述：                                                    
*│　作   者：xuXin                                              
*│　版   本：1.0.0                                                 
*│　创建时间：2019/5/27 16:55:34                     
*└──────────────────────────────────────────────────────────────┘
*/

using Framework;
using Framework.SceneObject;
using SsitEngine.Unity.SceneObject;
using SsitEngine.Unity.Sound;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;

namespace SsitEngine.Unity.Action
{
    public class ActionSound : ActionBase
    {
        [SerializeField] private AudioClip audioClip;

        private AudioSource m_audioSource;

        [SerializeField] private float m_delay;

        [SerializeField] private bool m_is3DSound;

        [SerializeField] private bool m_isloop;

        //Callback
        [FormerlySerializedAs("OnPlayEnd")] [SerializeField]
        private UnityEvent m_OnPlayEnd;

        [SerializeField] private float m_volume = 1;


        /// <summary>
        ///   <para>UnityEvent that is triggered when the Button is pressed.</para>
        /// </summary>
        public UnityEvent onPlayEnd
        {
            get => m_OnPlayEnd;
            set => m_OnPlayEnd = value;
        }

        public override void Execute( object sender, EnPropertyId actionId, string actionParam, object data = null )
        {
            if (audioClip == null)
                return;

            var state = (En_SwitchState) actionParam.ParseByDefault(0);
            switch (state)
            {
                case En_SwitchState.On:
                {
                    if (m_audioSource != null)
                    {
                        SoundManager.StopSFXObject(m_audioSource);
                        m_audioSource = null;
                    }
                    if (m_is3DSound)
                        m_audioSource = SoundManager.PlaySFX(audioClip, m_isloop, m_delay, m_volume, 1,
                            transform.position, m_OnPlayEnd, SoundDuckingSetting.DuckAll, 0.1f, 0.5f);
                    else
                        m_audioSource = SoundManager.PlaySFXUI(audioClip, m_isloop, m_delay, m_volume, 1, m_OnPlayEnd,
                            SoundDuckingSetting.DuckAll, 0.1f, 0.5f);
                }
                    break;
                case En_SwitchState.Off:
                {
                    if (m_audioSource != null)
                    {
                        SoundManager.StopSFXObject(m_audioSource);
                        m_audioSource = null;
                    }
                }
                    break;
            }
        }

        private void OnDestroy()
        {
            if (m_audioSource != null)
            {
                SoundManager.StopSFXObject(m_audioSource);
                m_audioSource = null;
            }
            audioClip = null;
            m_OnPlayEnd = null;
        }

#if UNITY_EDITOR
        [ContextMenu("Test")]
        private void Test()
        {
            Execute(null, 0, null);
        }
#endif
    }
}