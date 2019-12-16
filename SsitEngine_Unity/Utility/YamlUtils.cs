using System.IO;
using SsitEngine.DebugLog;
using YamlDotNet.Serialization;

namespace SsitEngine.Unity
{
    public class YamlUtils
    {
        public static T Deserialize<T>( string yaml )
        {
            if (string.IsNullOrEmpty(yaml))
            {
                SsitDebug.Error("The yaml text is null");
                return default;
            }

            var sr = new StringReader(yaml);
            var deserializer = new Deserializer();
            var result = deserializer.Deserialize<T>(sr);


            sr.Close();
            sr.Dispose();
            return result;
        }
    }
}