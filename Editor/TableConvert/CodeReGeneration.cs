using System;
using System.IO;
using UnityEngine;

namespace SsitEngine.Editor
{
    public class CodeReGeneration : IDisposable
    {
        private readonly string filePath;

        public CodeReGeneration( string fPath )
        {
            filePath = fPath;
            if (!File.Exists(filePath))
            {
                Debug.LogError(filePath + "路径下文件不存在");
            }
        }

        public void Dispose()
        {
        }

        public void WriteBelow( string below, string text )
        {
            var streamReader = new StreamReader(filePath);
            var text_all = streamReader.ReadToEnd();
            streamReader.Close();

            var beginIndex = text_all.IndexOf(below);
            if (beginIndex == -1)
            {
                Debug.LogError(filePath + "中没有找到标致" + below);
                return;
            }

            var endIndex = text_all.LastIndexOf("\n", beginIndex + below.Length);

            text_all = text_all.Substring(0, endIndex) + "\n" + text + "\n" + text_all.Substring(endIndex);

            var streamWriter = new StreamWriter(filePath);
            streamWriter.Write(text_all);
            streamWriter.Close();
        }

        public void Replace( string below, string newText )
        {
            var streamReader = new StreamReader(filePath);
            var text_all = streamReader.ReadToEnd();
            streamReader.Close();

            var beginIndex = text_all.IndexOf(below);
            if (beginIndex == -1)
            {
                Debug.LogError(filePath + "中没有找到标致" + below);
                return;
            }

            text_all = text_all.Replace(below, newText);
            var streamWriter = new StreamWriter(filePath);
            streamWriter.Write(text_all);
            streamWriter.Close();
        }


        public void ToLuaC()
        {
        }
    }
}