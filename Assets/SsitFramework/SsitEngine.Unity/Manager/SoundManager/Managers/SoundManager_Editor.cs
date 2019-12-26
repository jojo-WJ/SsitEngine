using System.Collections;
using UnityEngine;
#if UNITY_EDITOR
using System.Collections.Generic;

namespace SsitEngine.Unity.Unity
{
    public class SoundManager
    {
        /// <summary>
        ///     Editor variable -- IGNORE AND DO NOT MODIFY
        /// </summary>
        [HideInInspector] public const string VIEW = "view";

        /// <summary>
        ///     Editor variable -- IGNORE AND DO NOT MODIFY
        /// </summary>
        [HideInInspector] public const string EDIT = "edit";

        /// <summary>
        ///     Editor variable -- IGNORE AND DO NOT MODIFY
        /// </summary>
        public const string HIDE = "hide";

        private bool _viewAll;

        /// <summary>
        ///     Editor variable -- IGNORE AND DO NOT MODIFY
        /// </summary>
        [HideInInspector] public float autoBaseVolume = 1f;

        /// <summary>
        ///     Editor variable -- IGNORE AND DO NOT MODIFY
        /// </summary>
        [HideInInspector] public float autoPitchVariation = 0f;

        /// <summary>
        ///     Editor variable -- IGNORE AND DO NOT MODIFY
        /// </summary>
        [HideInInspector] public int autoPrepoolAmount = 0;

        /// <summary>
        ///     Editor variable -- IGNORE AND DO NOT MODIFY
        /// </summary>
        [HideInInspector] public float autoVolumeVariation = 0f;

        /// <summary>
        ///     Editor variable -- IGNORE AND DO NOT MODIFY
        /// </summary>
        [HideInInspector] public int groupAddIndex = 0;

        /// <summary>
        ///     Editor variable -- IGNORE AND DO NOT MODIFY
        /// </summary>
        [HideInInspector] public bool helpOn = false;

        /// <summary>
        ///     Editor variable -- IGNORE AND DO NOT MODIFY
        /// </summary>
        [HideInInspector] public bool showAdd = true;

        /// <summary>
        ///     Editor variable -- IGNORE AND DO NOT MODIFY
        /// </summary>
        [HideInInspector] public bool showAsGrouped = false;

        /// <summary>
        ///     Editor variable -- IGNORE AND DO NOT MODIFY
        /// </summary>
        [HideInInspector] public bool showDev = true;

        /// <summary>
        ///     Editor variable -- IGNORE AND DO NOT MODIFY
        /// </summary>
        [HideInInspector] public bool showInfo = true;

        /// <summary>
        ///     Editor variable -- IGNORE AND DO NOT MODIFY
        /// </summary>
        [HideInInspector] public bool showList = true;

        /// <summary>
        ///     Editor variable -- IGNORE AND DO NOT MODIFY
        /// </summary>
        [HideInInspector] public bool showSFX = true;

        /// <summary>
        ///     Editor variable -- IGNORE AND DO NOT MODIFY
        /// </summary>
        [HideInInspector] public List<bool> showSFXDetails = new List<bool>();

        /// <summary>
        ///     Editor variable -- IGNORE AND DO NOT MODIFY
        /// </summary>
        [SerializeField] public Hashtable songStatus = new Hashtable();

        /// <summary>
        ///     Editor variable -- IGNORE AND DO NOT MODIFY
        /// </summary>
        public bool viewAll
        {
            get => _viewAll;
            set
            {
                _viewAll = value;
                var keys = new List<string>();
                foreach (DictionaryEntry de in songStatus)
                {
                    keys.Add(de.Key.ToString());
                }

                foreach (var key in keys)
                {
                    if (_viewAll)
                    {
                        songStatus[key] = VIEW;
                    }
                    else
                    {
                        songStatus[key] = HIDE;
                    }
                }
            }
        }
    }
}
#endif