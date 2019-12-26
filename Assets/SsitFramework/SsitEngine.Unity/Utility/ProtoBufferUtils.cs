using System;
using System.IO;
using ProtoBuf;
using SsitEngine.DebugLog;

namespace SsitEngine.Unity
{
    public class ProtoBufferUtils
    {
        public static byte[] Serialize( object data )
        {
            try
            {
                using (var ms = new MemoryStream())
                {
                    //使用ProtoBuf工具的序列化方法
                    Serializer.Serialize(ms, data);
                    //定义二级制数组，保存序列化后的结果
                    var result = new byte[ms.Length];
                    //将流的位置设为0，起始点
                    ms.Position = 0;
                    //将流中的内容读取到二进制数组中
                    ms.Read(result, 0, result.Length);
                    return result;
                }
            }
            catch (Exception ex)
            {
                SsitDebug.Error(ex.StackTrace);
                //序列化失败
                return null;
            }
        }


        public static T DeSerialize<T>( byte[] data )
        {
            try
            {
                using (var ms = new MemoryStream())
                {
                    //将消息写入流中
                    ms.Write(data, 0, data.Length);
                    //将流的位置归0
                    ms.Position = 0;
                    //使用工具反序列化对象
                    var result = Serializer.Deserialize<T>(ms);
                    return result;
                }
            }
            catch (Exception ex)
            {
                SsitDebug.Error("反序列化失败: " + ex.StackTrace);
                return default;
            }
        }
    }
}