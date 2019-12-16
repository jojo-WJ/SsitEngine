using System.Collections.Generic;
using System.Text;

namespace SsitEngine
{
    /// <summary>
    ///     字典扩展类
    /// </summary>
    public static class DictionaryExtension
    {
        /// <summary>
        ///     输出字典的Key值拼接
        /// </summary>
        /// <typeparam name="TKey"></typeparam>
        /// <typeparam name="TValue"></typeparam>
        /// <param name="dict"></param>
        /// <returns>返回字典的Key值拼接的字符出</returns>
        public static string KeysToString<TKey, TValue>( this Dictionary<TKey, TValue> dict )
        {
            var sb = new StringBuilder();
            foreach (var value in dict.Keys) sb.AppendLine(value.ToString());
            return sb.ToString();
        }

        /// <summary>
        ///     输出字典的Key值拼接
        /// </summary>
        /// <typeparam name="TKey"></typeparam>
        /// <typeparam name="TValue"></typeparam>
        /// <param name="dict"></param>
        /// <returns>返回字典的Key值拼接的字符出</returns>
        public static string KeysToString<TKey, TValue>( this IDictionary<TKey, TValue> dict )
        {
            var sb = new StringBuilder();
            foreach (var value in dict.Keys) sb.AppendLine(value.ToString());
            return sb.ToString();
        }

        /// <summary>
        ///     尝试将键和值添加到字典中：如果不存在，才添加；存在，不添加也不抛导常
        /// </summary>
        /// <returns>添加后字典</returns>
        public static Dictionary<TKey, TValue> TryAdd<TKey, TValue>( this Dictionary<TKey, TValue> dict, TKey key,
            TValue value )
        {
            if (dict.ContainsKey(key) == false)
                dict.Add(key, value);
            return dict;
        }

        /// <summary>
        ///     将键和值添加或替换到字典中：如果不存在，则添加；存在，则替换
        /// </summary>
        /// <returns>添加后字典</returns>
        public static Dictionary<TKey, TValue> AddOrPeplace<TKey, TValue>( this Dictionary<TKey, TValue> dict, TKey key,
            TValue value )
        {
            dict[key] = value;
            return dict;
        }

        /// <summary>
        ///     获取与指定的键相关联的值，如果没有则返回输入的默认值
        /// </summary>
        /// <returns>指定key得Value值 ，如果不存在放回默认值</returns>
        public static TValue GetValue<TKey, TValue>( this Dictionary<TKey, TValue> dict, TKey key, TValue defaultValue )
        {
            return dict.ContainsKey(key) ? dict[key] : defaultValue;
        }

        /// <summary>
        ///     向字典中批量添加键值对
        /// </summary>
        /// <typeparam name="TKey"></typeparam>
        /// <typeparam name="TValue"></typeparam>
        /// <param name="dict"></param>
        /// <param name="values"></param>
        /// <param name="replaceExisted"></param>
        /// <returns>添加后字典</returns>
        public static Dictionary<TKey, TValue> AddRange<TKey, TValue>( this Dictionary<TKey, TValue> dict,
            IEnumerable<KeyValuePair<TKey, TValue>> values, bool replaceExisted )
        {
            foreach (var item in values)
                if (dict.ContainsKey(item.Key) == false || replaceExisted)
                    dict[item.Key] = item.Value;
            return dict;
        }

        /// <summary>
        ///     向字典中批量添加键值对
        /// </summary>
        /// <typeparam name="TKey"></typeparam>
        /// <typeparam name="TValue"></typeparam>
        /// <param name="dict"></param>
        /// <param name="values"></param>
        /// <param name="replaceExisted">替换已经存在的</param>
        /// <returns>添加后字典</returns>
        public static Dictionary<TKey, TValue> AddRange<TKey, TValue>( this Dictionary<TKey, TValue> dict,
            Dictionary<TKey, TValue> values, bool replaceExisted )
        {
            foreach (var item in values)
                if (dict.ContainsKey(item.Key) == false || replaceExisted)
                    dict[item.Key] = item.Value;
            return dict;
        }
    }
}