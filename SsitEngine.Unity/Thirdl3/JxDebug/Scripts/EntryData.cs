using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using SsitEngine;
using UnityEngine;

namespace JxDebug
{
    [Serializable]
    public struct EntryData
    {
        private const int MAX_CHARACTERS = 16300;
        private const string EXTRA_TEXT = " [...]";

        public string timeStamp;
        public string text;
        public Texture2D icon;
        public string stackTrace;
        public List<string> tags;
        public EntryOptions options;
        private static Regex s_regex;

        //public EntryData(string text, params string[] tags) :this(text, new EntryOptions(), tags) { }

        public EntryData( string text, EntryOptions options, params string[] tags ) : this(text, null, null, options,
            tags)
        {
        }

        public EntryData( string text, Texture2D icon, params string[] tags ) : this(text, icon, new EntryOptions(),
            tags)
        {
        }

        public EntryData( string text, Texture2D icon, EntryOptions options, params string[] tags ) : this(text, icon,
            null, options, tags)
        {
        }

        public EntryData( string text, string stackTrace, Texture2D icon, params string[] tags ) : this(text, icon,
            stackTrace, new EntryOptions(), tags)
        {
        }

        public EntryData( string text, Texture2D icon, string stackTrace, EntryOptions options, params string[] tags )
        {
            if (s_regex == null)
            {
                s_regex = new Regex(">(.*)<");
            }

            this.text = text;
            this.icon = icon;
            this.stackTrace = stackTrace;
            this.options = options;
            this.tags = new List<string>(tags);
            timeStamp = DateTime.Now.ToString("[yyyy/MM/dd][HH:mm:ss] ");
            SanitizeTags();
        }

        /// <summary>
        /// 处理标签组
        /// </summary>
        private void SanitizeTags()
        {
            for (var i = 0; i < tags.Count; i++)
            {
                tags[i] = SanitizeTag(tags[i]);
            }
        }

        /// <summary>
        /// 处理标签
        /// </summary>
        /// <param name="tag"></param>
        /// <returns></returns>
        private string SanitizeTag( string tag )
        {
            tag = tag.Trim();
            tag = tag.Replace("\n", string.Empty);
            tag = tag.Replace("\r", string.Empty);
            tag = tag.Replace(" ", string.Empty);
            return tag;
        }

        /// <summary>
        /// 切断超文本
        /// </summary>
        public void CutOffExceedingText()
        {
            //if(text.Length + stackTrace.Length > MAX_CHARACTERS) 
            //text = text.Substring(0, MAX_CHARACTERS - stackTrace.Length - EXTRA_TEXT.Length) + EXTRA_TEXT;
            if (text.Length > MAX_CHARACTERS)
            {
                text = text.Substring(0, MAX_CHARACTERS - EXTRA_TEXT.Length) + EXTRA_TEXT;
            }
        }

        public override string ToString()
        {
            //todo:去除富文本
            var matchStr = text;
            if (s_regex.IsMatch(text))
            {
                matchStr = s_regex.Match(text).Value;
                matchStr = matchStr.Substring(1, matchStr.Length - 2);
            }
            if (string.IsNullOrEmpty(stackTrace))
            {
                return TextUtils.Format("{0} :{1} : {2}", timeStamp, tags[0], matchStr);
            }
            return TextUtils.Format("{0} :{1} : {2}\r\n{3}", timeStamp, tags[0], matchStr, stackTrace);

            //return string.Format("{0}: {1}\n\r{2}", tags[0], text, stackTrace);
        }
    }
}