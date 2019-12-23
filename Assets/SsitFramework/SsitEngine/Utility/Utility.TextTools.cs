using System;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Text;
using System.Text.RegularExpressions;

namespace SsitEngine
{
    /// <summary>
    ///     字符相关的实用函数
    /// </summary>
    public static class TextUtils
    {
        [ThreadStatic] private static StringBuilder s_cachedStringBuilder;

        /// <summary>
        ///     获取格式化字符串。
        /// </summary>
        /// <param name="format">字符串格式。</param>
        /// <param name="arg0">字符串参数 0。</param>
        /// <returns>格式化后的字符串。</returns>
        public static string Format( string format, object arg0 )
        {
            if (format == null)
            {
                throw new SsitEngineException("Format is invalid.");
            }

            if (s_cachedStringBuilder == null)
            {
                s_cachedStringBuilder = new StringBuilder();
            }
            s_cachedStringBuilder.Length = 0;
            s_cachedStringBuilder.AppendFormat(format, arg0);
            return s_cachedStringBuilder.ToString();
        }

        /// <summary>
        ///     获取格式化字符串。
        /// </summary>
        /// <param name="format">字符串格式。</param>
        /// <param name="arg0">字符串参数 0。</param>
        /// <param name="arg1">字符串参数 1。</param>
        /// <returns>格式化后的字符串。</returns>
        public static string Format( string format, object arg0, object arg1 )
        {
            if (format == null)
            {
                throw new SsitEngineException("Format is invalid.");
            }
            if (s_cachedStringBuilder == null)
            {
                s_cachedStringBuilder = new StringBuilder();
            }
            s_cachedStringBuilder.Length = 0;
            s_cachedStringBuilder.AppendFormat(format, arg0, arg1);
            return s_cachedStringBuilder.ToString();
        }

        /// <summary>
        ///     获取格式化字符串。
        /// </summary>
        /// <param name="format">字符串格式。</param>
        /// <param name="args">字符串参数。</param>
        /// <returns>格式化后的字符串。</returns>
        public static string Format( string format, params object[] args )
        {
            if (format == null)
            {
                throw new SsitEngineException("Format is invalid.");
            }

            if (args == null)
            {
                throw new SsitEngineException("Args is invalid.");
            }
            if (s_cachedStringBuilder == null)
            {
                s_cachedStringBuilder = new StringBuilder();
            }
            s_cachedStringBuilder.Length = 0;
            s_cachedStringBuilder.AppendFormat(format, args);
            return s_cachedStringBuilder.ToString();
        }

        /// <summary>
        ///     将文本按行切分。
        /// </summary>
        /// <param name="text">要切分的文本。</param>
        /// <returns>按行切分后的文本。</returns>
        public static string[] SplitToLines( string text )
        {
            return text.Split(new[] {"\r", "\n"}, StringSplitOptions.RemoveEmptyEntries);
        }

        /// <summary>
        ///     根据类型和名称获取完整名称。
        /// </summary>
        /// <typeparam name="T">类型。</typeparam>
        /// <param name="name">名称。</param>
        /// <returns>完整名称。</returns>
        public static string GetFullName<T>( string name )
        {
            return GetFullName(typeof(T), name);
        }

        /// <summary>
        ///     根据类型和名称获取完整名称。
        /// </summary>
        /// <param name="type">类型。</param>
        /// <param name="name">名称。</param>
        /// <returns>完整名称。</returns>
        public static string GetFullName( Type type, string name )
        {
            if (type == null)
            {
                throw new SsitEngineException("Type is invalid.");
            }

            var typeName = type.FullName;
            return string.IsNullOrEmpty(name) ? typeName : Format("{0}.{1}", typeName, name);
        }

        /// <summary>
        ///     获取用于编辑器显示的名称。
        /// </summary>
        /// <param name="fieldName">字段名称。</param>
        /// <returns>编辑器显示名称。</returns>
        public static string FieldNameForDisplay( string fieldName )
        {
            if (string.IsNullOrEmpty(fieldName))
            {
                return string.Empty;
            }

            var str = Regex.Replace(fieldName, @"^m_", string.Empty);
            str = Regex.Replace(str, @"((?<=[a-z])[A-Z]|[A-Z](?=[a-z]))", @" $1").TrimStart();
            return str;
        }

        #region LitJsonTools

        /// <summary>
        ///     对于使用LitJson的json解析工具可以通过此方法将json数据中16进制的字符转为utf8的编码格式
        /// </summary>
        /// <param name="data">json数据</param>
        /// <returns>可识别的json中文字符</returns>
        public static string LitJson2Utf8( string data )
        {
            var reg = new Regex(@"(?i)\\[uU]([0-9a-f]{4})");
            return reg.Replace(data,
                delegate( Match m ) { return ((char) Convert.ToInt32(m.Groups[1].Value, 16)).ToString(); });
        }

        #endregion

        /// <summary>
        ///     为指定对象生成唯一名称.
        /// </summary>
        /// <returns>构造名称.</returns>
        /// <param name="id">用于生成名称的ID.</param>
        public static string GenerateUniqueName( object id )
        {
            return id.GetHashCode().ToString(CultureInfo.InvariantCulture);
        }

        /// <summary>
        ///     Decompress a GZ compressed string.
        /// </summary>
        /// <returns>The decompressed string.</returns>
        /// <param name="value">Compressed string value.</param>
        public static byte[] UnZip( string value )
        {
            var gZipBuffer = Convert.FromBase64String(value);
            using (var memoryStream = new MemoryStream())
            {
                var dataLength = BitConverter.ToInt32(gZipBuffer, 0);
                memoryStream.Write(gZipBuffer, 4, gZipBuffer.Length - 4);
                var buffer = new byte[dataLength];
                memoryStream.Position = 0;
                using (var gZipStream = new GZipStream(memoryStream, CompressionMode.Decompress))
                {
                    gZipStream.Read(buffer, 0, buffer.Length);
                }
                return buffer;
            }
        }

        #region SafeConvert

        private const string CommaTag = "%COMMA%";
        private const string DoubleQuoteTag = "%QUOTE%";
        private const string NewlineTag = "%NEWLINE%";

        /// <summary>
        ///     将一个字符串转换为32位整数
        /// </summary>
        /// <param name="s">Source string.</param>
        /// <returns>转换后的32位整数，转换失败默认会返回0.</returns>
        public static int ToInt( string s )
        {
            int result;
            return int.TryParse(s, out result) ? result : 0;
        }

        /// <summary>
        ///     将一个字符串转换为无符号的短整形整数.
        /// </summary>
        /// <param name="s">源字符串.</param>
        /// <returns>转换后的无符号的短整形整数，转换失败默认会返回0.</returns>
        public static ushort ToUshort( string s )
        {
            ushort result = 0;
            ushort.TryParse(s, out result);
            return result;
        }

        /// <summary>
        ///     将一个字符串转换为单精度浮点数.
        /// </summary>
        /// <param name="s">源字符串.</param>
        /// <returns>转换后的单精度浮点数，转换失败默认会返回0.</returns>
        public static float ToFloat( string s )
        {
            float result;
            return float.TryParse(s, NumberStyles.Any, CultureInfo.InvariantCulture, out result) ? result : 0;
        }


        /// <summary>
        ///     清理字符串，以便可以序列化为基于字符串的序列化系统（针对有特定违法的字符 ',''\','\n'）.
        /// </summary>
        /// <param name="s">目标字符出啊.</param>
        /// <returns>字符清除后的文本.</returns>
        public static string ToSerializedElement( string s )
        {
            if (s.Contains(",") || s.Contains("\"") || s.Contains("\n"))
            {
                return s.Replace(",", CommaTag).Replace("\"", DoubleQuoteTag).Replace("\n", NewlineTag);
            }
            return s;
        }

        /// <summary>
        ///     对先前已清理过的字符串进行反向处理，以便在基于字符串的序列化系统中使用.
        /// </summary>
        /// <param name="s">特殊处理过的字符串.</param>
        /// <returns>原始的字符串.</returns>
        public static string FromSerializedElement( string s )
        {
            if (s.Contains(CommaTag) || s.Contains(DoubleQuoteTag) || s.Contains(NewlineTag))
            {
                return s.Replace(CommaTag, ",").Replace(DoubleQuoteTag, "\"").Replace(NewlineTag, "\n");
            }
            return s;
        }

        #endregion

        #region 字符串解码ByBase64

        /*
        【Base64】
        -base64的编码都是按字符串长度，以每3个8bit的字符为一组，
        -然后针对每组，首先获取每个字符的ASCII编码，
        -然后将ASCII编码转换成8bit的二进制，得到一组3*8=24bit的字节
        -然后再将这24bit划分为4个6bit的字节，并在每个6bit的字节前面都填两个高位0，得到4个8bit的字节
        -然后将这4个8bit的字节转换成10进制，对照Base64编码表 （下表），得到对应编码后的字符。
        */

        #endregion

        #region 字符串特定正则检测

        /// <summary>
        ///     是否为数字
        /// </summary>
        public static bool IsNumber( string strNumber )
        {
            var regex = new Regex("[^0-9]");
            return !regex.IsMatch(strNumber);
        }

        /// <summary>
        ///     判断字符串是否是手机号
        /// </summary>
        /// <param name="mobile"></param>
        /// <returns></returns>
        public static bool ValidateMobile( string mobile )
        {
            var regex = new Regex(@"/^1\d{10}$/");
            return !regex.IsMatch(mobile);
        }

        /// <summary>
        ///     检测字符串是否是指定规则的字符密码格式
        /// </summary>
        /// <param name="password">指定判断的密码字符串</param>
        /// <returns>是否符合规定的字符密码</returns>
        public static bool CheckPassWord( string password )
        {
            //6-16位数字和字母组成
            /*
             ^ 匹配一行的开头位置
             (?![0-9]+$) 预测该位置后面不全是数字
             (?![a-zA-Z]+$) 预测该位置后面不全是字母
             [0-9A-Za-z] {6,16} 由6-16位数字或这字母组成
             $ 匹配行尾位置
             */
            var regex = new Regex(@"^(?![0-9]+$)(?![a-zA-Z]+$)[0-9A-Za-z]{6,16}$");
            return !regex.IsMatch(password);
        }

        #endregion

        #region 字符串与DataTime

        /// <summary>
        ///     时间转换位长整形数
        /// </summary>
        /// <param name="time">时间</param>
        /// <returns>长整形数</returns>
        public static long ConvertDataTimeToLong( DateTime time )
        {
            var startTime = TimeZone.CurrentTimeZone.ToLocalTime(new DateTime(1970, 1, 1, 0, 0, 0, 0));
            return (long) (time - startTime).TotalSeconds;
        }

        /// <summary>
        ///     长整形转换DataTime
        /// </summary>
        /// <param name="span">事件数值</param>
        /// <returns>时间结构体</returns>
        public static DateTime ConvertLongToDateTime( long span )
        {
            var time = DateTime.MinValue;
            var startTime = TimeZone.CurrentTimeZone.ToLocalTime(new DateTime(1970, 1, 1, 0, 0, 0, 0));
            time = startTime.AddSeconds(span);
            return time;
        }

        /// <summary>
        ///     计算两个时间点的时间周期（10：12：33）
        /// </summary>
        /// <param name="d1"></param>
        /// <param name="d2"></param>
        /// <returns></returns>
        public static string ConvertDuationFromDataTime( DateTime d1, DateTime d2 )
        {
            var dt = d1 - d2;
            return Format("{0:D2}:{1:D2}:{2:D2}", dt.Hours, dt.Minutes, dt.Seconds);
        }

        /// <summary>
        ///     比较两个日期大小
        /// </summary>
        /// <param name="dateStr1">日期1</param>
        /// <param name="dateStr2">日期2</param>
        /// <remarks>
        ///     方法一：Convert.ToDateTime(string)[当前使用后面的自己扩展]
        ///     string格式有要求，必须是yyyy-MM-dd hh:mm:ss
        ///     ================================================
        ///     方法二：Convert.ToDateTime(string, IFormatProvider)
        ///     DateTime dt;
        ///     DateTimeFormatInfo dtFormat = new System.GlobalizationDateTimeFormatInfo();
        ///     dtFormat.ShortDatePattern = "yyyy/MM/dd";
        ///     dt = Convert.ToDateTime("2011/05/26", dtFormat);
        ///     ================================================
        ///     方法三：DateTime.ParseExact()
        ///     string dateString = "20110526";
        ///     DateTime dt = DateTime.ParseExact(dateString, "yyyyMMdd", System.Globalization.CultureInfo.CurrentCulture);
        ///     或者
        ///     DateTime dt = DateTime.ParseExact(dateString, "yyyyMMdd", System.Globalization.CultureInfo.InvariantCulture);
        /// </remarks>
        public static int CompanyDate( string dateStr1, string dateStr2 )
        {
            //将日期字符串转换为日期对象
            var t1 = Convert.ToDateTime(dateStr1);
            var t2 = Convert.ToDateTime(dateStr2);
            //通过DateTIme.Compare()进行比较（）
            return DateTime.Compare(t1, t2);
        }

        /// <summary>
        ///     /Int数值转成string时间显示
        /// </summary>
        /// <param name="time"></param>
        /// <returns>Int————string 显示时分秒</returns>
        public static string FormatIntToStringDate( int time )
        {
            var days = time / (3600 * 24);
            var hours = time / 3600 - days * 24;
            var minutes = time / 60 - hours * 60 - days * 60 * 24;
            var seconds = time - minutes * 60 - hours * 3600 - days * 3600 * 24;

            var showTime = "";
            if (days == 0)
            {
                showTime = Format("{0:D2}:{1:D2}:{2:D2}", hours, minutes, seconds);
            }
            else
            {
                showTime = Format("{0:D2}:{1:D2}:{2:D2}:{3:D2}", days, hours, minutes, seconds);
            }

            return showTime;
        }

        #endregion
    }
}