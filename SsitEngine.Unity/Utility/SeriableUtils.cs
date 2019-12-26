/*
*┌──────────────────────────────────────────────────────────────┐
*│　描   述：序列化辅助工具类                                                    
*│　作   者：xuXin                                              
*│　版   本：1.0.0                                                 
*│　创建时间：2019/5/8 18:59:04                     
*└──────────────────────────────────────────────────────────────┘
*/

using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;

namespace SsitEngine.Unity.Utility
{
    /// <summary>
    ///     序列化辅助工具类
    /// </summary>
    public class SerializationUtils
    {
        /// <summary>
        ///     对象克隆
        /// </summary>
        /// <typeparam name="T">指定的对象类型</typeparam>
        /// <param name="RealObject">实际克隆的对象</param>
        /// <returns></returns>
        public static T Clone<T>( T RealObject )
        {
            using (Stream objectStream = new MemoryStream())
            {
                //利用 System.Runtime.Serialization序列化与反序列化完成引用对象的复制  
                IFormatter formatter = new BinaryFormatter();
                formatter.Serialize(objectStream, RealObject);
                objectStream.Seek(0, SeekOrigin.Begin);
                return (T) formatter.Deserialize(objectStream);
            }
        }
    }
}