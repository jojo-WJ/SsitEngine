using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using Framework;
using Framework.Data;
using Framework.SceneObject;
using SsitEngine.Unity.SceneObject;
using UnityEngine;

namespace Framework.Utility
{
    public static class Utilitys
    {
        #region MD5

        /// <summary>
        /// HashToMD5Hex
        /// </summary>
        /// <param name="sourceStr">需计算字符串</param>
        public static string HashToMD5Hex( string sourceStr )
        {
            byte[] aBytes = Encoding.UTF8.GetBytes(sourceStr);
            using (MD5CryptoServiceProvider md5 = new MD5CryptoServiceProvider())
            {
                byte[] result = md5.ComputeHash(aBytes);
                StringBuilder builder = new StringBuilder();
                for (int i = 0; i < result.Length; i++)
                {
                    builder.Append(result[i].ToString("x2"));
                }
                return builder.ToString();
            }
        }

        #endregion

        #region 角色

        /// <summary>
        /// 状态
        /// </summary>
        /// <param name="player"></param>
        /// <param name="state"></param>
        /// <returns></returns>
        public static bool OperatorStateCheck( Player player, EN_CharacterActionState state )
        {
            if (player.State == state)
            {
                return true;
            }
            else
            {
                MessageInfo messageInfo = new MessageInfo()
                {
                    MessageType = EnMessageType.SYSTEM,
                    UserDisplayName = player.GetAttribute().Name,
                    //MessageContent = DataContentProxy.GetTipContent(EnText.OperatorExceptionTip)
                };
                //PopTipHelper.PopTip(messageInfo, false);

                return false;
            }

            //if (!player.OnStateCheckCallback(player,(int)player.State,(int)state))
            //{

            //}
            //return true;
        }

        /// <summary>
        /// 监测权限
        /// </summary>
        /// <param name="itemObj">道具</param>
        public static bool CheckAuthiroty( BaseSceneInstance itemObj )
        {
            if (itemObj == null || itemObj.LinkObject == null)
                return false;
            return true;
        }
        #endregion

        #region 时间

        public static long ConvertDataTimeToLong( DateTime time )
        {
            DateTime startTime = TimeZone.CurrentTimeZone.ToLocalTime(new DateTime(1970, 1, 1, 0, 0, 0, 0));
            return (long) (time - startTime).TotalSeconds;
        }

        //timeSpan转换为DateTime
        public static DateTime ConvertLongToDateTime( long span )
        {
            DateTime time = DateTime.MinValue;
            DateTime startTime = TimeZone.CurrentTimeZone.ToLocalTime(new DateTime(1970, 1, 1, 0, 0, 0, 0));
            time = startTime.AddSeconds(span);
            return time;
        }

        #endregion

        #region 检测

        /// <summary>
        /// 判断是否是鼠标输入。注意：该方法并不完美
        /// </summary>
        /// <returns></returns>
        public static bool IsMousePlatform()
        {
            return Application.platform == RuntimePlatform.WindowsPlayer
                   || Application.platform == RuntimePlatform.WindowsEditor
                   || Application.platform == RuntimePlatform.OSXEditor
                   || Application.platform == RuntimePlatform.OSXPlayer;
        }

        #endregion
        
        #region Editor

#if UNITY_STANDALONE_WIN && UNITY_EDITOR
        /// <summary>
        /// 计算文件的MD5值
        /// </summary>
        public static string Md5File( string file )
        {
            try
            {
                FileStream fs = new FileStream(file, FileMode.Open);
                MD5 md5 = new MD5CryptoServiceProvider();
                byte[] aRetVal = md5.ComputeHash(fs);
                fs.Close();

                StringBuilder sb = new StringBuilder();
                for (int i = 0; i < aRetVal.Length; i++)
                {
                    sb.Append(aRetVal[i].ToString("x2"));
                }
                return sb.ToString();
            }
            catch (Exception ex)
            {
                throw new Exception("md5file() fail, error:" + ex.Message);
            }
        }

        /// <summary>
        /// 取得文件文本
        /// </summary>
        public static string GetFileText( string path )
        {
            return File.ReadAllText(path);
        }
#endif

        #endregion

    }
}