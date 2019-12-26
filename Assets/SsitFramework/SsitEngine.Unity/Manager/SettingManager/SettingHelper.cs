/**
*┌──────────────────────────────────────────────────────────────┐
*│　描   述：                                                    
*│　作   者：xuXin                                              
*│　版   本：1.0.0                                                 
*│　创建时间：2019/4/10 9:40:09                     
*└──────────────────────────────────────────────────────────────┘
*/

using System;
using SsitEngine.Setting;
using UnityEngine;

namespace SsitEngine.Unity.Setting
{
    /// <summary>
    ///     引擎端的设置辅助器
    /// </summary>
    public class SettingHelper : ISettingHelper
    {
        public bool Load()
        {
            return false;
        }

        public bool Save()
        {
            return false;
        }

        /// <inheritdoc />
        public bool HasSetting( string settingName )
        {
            return PlayerPrefs.HasKey(settingName);
        }

        /// <inheritdoc />
        public void RemoveSetting( string settingName )
        {
            PlayerPrefs.DeleteKey(settingName);
        }

        /// <inheritdoc />
        public void RemoveAllSettings()
        {
            PlayerPrefs.DeleteAll();
        }

        /// <inheritdoc />
        public bool GetBool( string settingName )
        {
            return PlayerPrefs.GetString(settingName) == "0";
        }

        /// <inheritdoc />
        public bool GetBool( string settingName, bool defaultValue )
        {
            return PlayerPrefs.GetString(settingName, defaultValue ? "0" : "1") == "0";
        }

        /// <inheritdoc />
        public void SetBool( string settingName, bool value )
        {
            PlayerPrefs.SetString(settingName, value ? "0" : "1");
        }

        /// <inheritdoc />
        public int GetInt( string settingName )
        {
            return PlayerPrefs.GetInt(settingName);
        }

        /// <inheritdoc />
        public int GetInt( string settingName, int defaultValue )
        {
            return PlayerPrefs.GetInt(settingName, defaultValue);
        }

        /// <inheritdoc />
        public void SetInt( string settingName, int value )
        {
            PlayerPrefs.SetInt(settingName, value);
        }

        /// <inheritdoc />
        public float GetFloat( string settingName )
        {
            return PlayerPrefs.GetFloat(settingName);
        }

        /// <inheritdoc />
        public float GetFloat( string settingName, float defaultValue )
        {
            return PlayerPrefs.GetFloat(settingName, defaultValue);
        }

        /// <inheritdoc />
        public void SetFloat( string settingName, float value )
        {
            PlayerPrefs.SetFloat(settingName, value);
        }

        /// <inheritdoc />
        public string GetString( string settingName )
        {
            return PlayerPrefs.GetString(settingName);
        }

        /// <inheritdoc />
        public string GetString( string settingName, string defaultValue )
        {
            return PlayerPrefs.GetString(settingName, defaultValue);
        }

        /// <inheritdoc />
        public void SetString( string settingName, string value )
        {
            PlayerPrefs.SetString(settingName, value);
        }

        /// <inheritdoc />
        public T GetObject<T>( string settingName )
        {
            var str = GetString(settingName);
            try
            {
                return JsonUtility.FromJson<T>(str);
            }
            catch (Exception e)
            {
                throw new SsitEngineException(e.ToString());
            }
        }

        /// <inheritdoc />
        public object GetObject( Type objectType, string settingName )
        {
            var str = GetString(settingName);
            try
            {
                return JsonUtility.FromJson(str, objectType);
            }
            catch (Exception e)
            {
                throw new SsitEngineException(e.ToString());
            }
        }

        /// <inheritdoc />
        public T GetObject<T>( string settingName, T defaultObj )
        {
            var str = GetString(settingName, string.Empty);
            try
            {
                return JsonUtility.FromJson<T>(str);
            }
            catch (Exception e)
            {
                return defaultObj;
            }
        }

        public object GetObject( Type objectType, string settingName, object defaultObj )
        {
            var str = GetString(settingName, string.Empty);
            try
            {
                return JsonUtility.FromJson(str, objectType);
            }
            catch (Exception e)
            {
                return defaultObj;
            }
        }

        /// <inheritdoc />
        public void SetObject<T>( string settingName, T obj )
        {
            try
            {
                var str = JsonUtility.ToJson(obj);
                SetString(settingName, str);
            }
            catch (Exception e)
            {
                throw new SsitEngineException(e.ToString());
            }
        }

        /// <inheritdoc />
        public void SetObject( string settingName, object obj )
        {
            try
            {
                var str = JsonUtility.ToJson(obj);
                SetString(settingName, str);
            }
            catch (Exception e)
            {
                throw new SsitEngineException(e.ToString());
            }
        }
    }
}