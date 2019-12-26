using System;
using UnityEngine;

namespace SsitEngine.QuestManager
{
    /// <summary>
    /// 任务音频源标识符类型
    /// </summary>
    public enum QuestAudioSourceIdentifierType
    {
        /// <summary>
        /// Use the audio source on the current main camera.
        /// </summary>
        MainCamera,

        /// <summary>
        /// Use the audio source on the current Quest Machine Configuration instance.
        /// </summary>
        QuestMachine,

        /// <summary>
        /// Use the audio source on the GameObject with the specified tag.
        /// </summary>
        GameObjectWithTag,

        /// <summary>
        /// Use the audio source on the GameObject with the specified name.
        /// </summary>
        GameObjectWithName,

        /// <summary>
        /// Use the soundManager's audio clip
        /// </summary>
        SoundManager
    }

    /// <summary>
    /// Specifies which audio source to use to play audio.
    /// </summary>
    [Serializable]
    public class QuestAudioSourceIdentifier
    {
        [Tooltip("Tag or GameObject name.")] [SerializeField]
        private string m_id = string.Empty;

        [Tooltip("How to identify the audio source.")] [SerializeField]
        private QuestAudioSourceIdentifierType m_type = QuestAudioSourceIdentifierType.MainCamera;

        /// <summary>
        /// How to identify the audio source.
        /// </summary>
        public QuestAudioSourceIdentifierType type
        {
            get => m_type;
            set => m_type = value;
        }

        /// <summary>
        /// Tag or GameObject name.
        /// </summary>
        public string id
        {
            get => m_id;
            set => m_id = value;
        }

        /// <summary>
        /// Play a one shot audio clip through the specified audio source.
        /// </summary>
        /// <param name="audioClip"></param>
        public void Play( AudioClip audioClip )
        {
            var audioSource = FindAudioSource();
            if (audioSource != null)
            {
                audioSource.PlayOneShot(audioClip);
            }
        }

        private AudioSource FindAudioSource()
        {
            var go = FindAudioSourceGameObject();
            if (go == null)
            {
                return null;
            }
            var audioSource = go.GetComponent<AudioSource>();
            if (audioSource == null)
            {
                audioSource = go.AddComponent<AudioSource>();
                audioSource.playOnAwake = false;
                audioSource.loop = false;
            }
            return audioSource;
        }

        private GameObject FindAudioSourceGameObject()
        {
            switch (type)
            {
                default:
                case QuestAudioSourceIdentifierType.MainCamera:
                    return Camera.main != null ? Camera.main.gameObject : null;
                case QuestAudioSourceIdentifierType.QuestMachine:
                    return QuestManager.Instance != null ? QuestManager.Instance.gameObject : null;
                case QuestAudioSourceIdentifierType.GameObjectWithTag:
                    return GameObject.FindGameObjectWithTag(id);
                case QuestAudioSourceIdentifierType.GameObjectWithName:
                    return GameObject.Find(id);
                case QuestAudioSourceIdentifierType.SoundManager:
                    //todo:完成音效管理器的接入
                    return null;
            }
        }
    }
}