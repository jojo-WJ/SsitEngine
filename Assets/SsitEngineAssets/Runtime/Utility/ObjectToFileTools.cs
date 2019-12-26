using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Xml.Serialization;
using SsitEngine;
using UnityEngine;

public class ObjectToFileTools<T> where T : class
{
    /// <summary>
    /// 保存二进制（类型都必须添加序列化的特性）
    /// </summary>
    /// <param name="filePath"></param>
    /// <param name="value"></param>
    public static void SaveBinary( string filePath, T value )
    {
        if (string.IsNullOrEmpty(filePath))
        {
            Debug.LogWarning("文件名为空");
            return;
        }

        if (!Directory.Exists(Path.GetDirectoryName(filePath)))
            Directory.CreateDirectory(Path.GetDirectoryName(filePath) ??
                                      throw new SsitEngineException("FilePath is null"));

        using (var fsStream = new FileStream(filePath, FileMode.Create))
        {
            //二进制文件
            //声明二进制格式化器
            var bfFormatter = new BinaryFormatter();
            //序列化
            bfFormatter.Serialize(fsStream, value);
            //将文件写出到硬盘（防止中断丢失写入文件）
            fsStream.Flush();
        }
    }

    /// <summary>
    /// 保存Json（类型都必须添加序列化的特性）
    /// </summary>
    /// <param name="filePath"></param>
    /// <param name="value"></param>
    public static void SaveJson( string filePath, T value )
    {
        if (string.IsNullOrEmpty(filePath))
        {
            Debug.LogWarning("文件名为空");
            return;
        }

        if (!Directory.Exists(Path.GetDirectoryName(filePath)))
            Directory.CreateDirectory(Path.GetDirectoryName(filePath) ??
                                      throw new SsitEngineException("FilePath is null"));

        using (var fsStream = new FileStream(filePath, FileMode.Create))
        {
            //json文件
            var json = JsonUtility.ToJson(value);
            //读取utf8格式的文本
            var tmpBytes = Encoding.UTF8.GetBytes(json);
            //读取字符串
            //                byte[] tmpBytes = File.ReadAllBytes(json);
            fsStream.Write(tmpBytes, 0, tmpBytes.Length);

            //将文件写出到硬盘（防止中断丢失写入文件）
            fsStream.Flush();
        }
    }

    /// <summary>
    /// 保存Json（类型都必须添加序列化的特性）
    /// </summary>
    /// <param name="filePath"></param>
    /// <param name="value"></param>
    public static byte[] WriteJsonToBytes( T value )
    {
        //json文件
        var json = JsonUtility.ToJson(value);
        //读取utf8格式的文本
        return Encoding.UTF8.GetBytes(json);
    }

    /// <summary>
    /// 保存XML资源
    /// </summary>
    /// <param name="filePath"></param>
    /// <param name="value"></param>
    public static void SaveXML( string filePath, T value )
    {
        if (string.IsNullOrEmpty(filePath))
        {
            Debug.LogWarning("文件名为空");
            return;
        }

        if (!Directory.Exists(Path.GetDirectoryName(filePath)))
            Directory.CreateDirectory(Path.GetDirectoryName(filePath) ??
                                      throw new SsitEngineException("FilePath is null"));

        using (var fsStream = new FileStream(filePath, FileMode.Create))
        {
            var xmlformatter = new XmlSerializer(typeof(T));
            xmlformatter.Serialize(fsStream, value);
            fsStream.Flush();
        }
    }

    /// <summary>
    /// 读取二进制资源
    /// </summary>
    /// <param name="fileName">资源完整名称</param>
    /// <param name="isJson">是否json</param>
    /// <returns></returns>
    public static T ReadBinary( string fileName )
    {
        if (!File.Exists(fileName)) return null;

        T ret = null;
        using (var fsStream = new FileStream(fileName, FileMode.Open))
        {
            var streamReader = new StreamReader(fsStream);

            //二进制文件
            var bfFormatter = new BinaryFormatter();
            ret = bfFormatter.Deserialize(fsStream) as T;
            streamReader.Close();
        }

        return ret;
    }

    /// <summary>
    /// 读取Json资源
    /// </summary>
    /// <param name="fileName">资源完整名称</param>
    /// <param name="isJson">是否json</param>
    /// <returns></returns>
    public static T ReadJson( string fileName )
    {
        if (!File.Exists(fileName))
        {
            return null;
        }

        T ret = null;
        using (var fsStream = new FileStream(fileName, FileMode.Open))
        {
            var streamReader = new StreamReader(fsStream);
            //json文件
            var json = streamReader.ReadToEnd();
            ret = JsonUtility.FromJson<T>(json);
            streamReader.Close();
        }

        return ret;
    }

    /// <summary>
    /// 读取Json资源
    /// </summary>
    /// <param name="fileName">资源完整名称</param>
    /// <param name="isJson">是否json</param>
    /// <returns></returns>
    public static T ReadJsonStr( string json )
    {
        return JsonUtility.FromJson<T>(json);
    }

    /// <summary>
    /// 读取XML资源
    /// </summary>
    /// <param name="fileName"></param>
    /// <returns></returns>
    public static T ReadXml( string fileName )
    {
        if (!File.Exists(fileName)) return null;

        T ret = null;
        using (var fsStream = new FileStream(fileName, FileMode.Open))
        {
            var xmlformatter = new XmlSerializer(typeof(T));

            ret = xmlformatter.Deserialize(fsStream) as T;
            fsStream.Flush();
        }

        return ret;
    }

    /// <summary>
    /// 读取XML资源
    /// </summary>
    /// <param name="xmlStr"></param>
    /// <returns></returns>
    public static T ReadXml( byte[] xmlStr )
    {
        T ret = null;
        var ms = new MemoryStream(xmlStr);
        var xmlformatter = new XmlSerializer(typeof(T));
        ret = xmlformatter.Deserialize(ms) as T;
        return ret;
    }

    public static void Save( string filePath, T value )
    {
        if (!Directory.Exists(Path.GetDirectoryName(filePath)))
            Directory.CreateDirectory(Path.GetDirectoryName(filePath) ??
                                      throw new SsitEngineException("FilePath is null"));


        var bf = new BinaryFormatter();
        using (var fs = new FileStream(filePath, FileMode.OpenOrCreate))
        {
            bf.Serialize(fs, value);
            fs.Flush();
        }
    }

    public static T Read( string fileName )
    {
        if (!File.Exists(fileName))
        {
            Debug.LogError("Not Found File! " + fileName);
            return null;
        }

        var bf = new BinaryFormatter();
        T ret = null;
        using (var fs = new FileStream(fileName, FileMode.Open))
        {
            ret = bf.Deserialize(fs) as T;
        }

        return ret;
    }

    /// <summary>
    /// 读取二进制文件
    /// </summary>
    /// <param name="res"></param>
    /// <returns></returns>
    public static T Read( byte[] res )
    {
        var bf = new BinaryFormatter();
        T ret = null;
        using (var ms = new MemoryStream(res))
        {
            ret = bf.Deserialize(ms) as T;
        }

        return ret;
    }
}