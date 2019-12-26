using System;
using System.Diagnostics;
using System.Globalization;
using System.Text;
using Debug = UnityEngine.Debug;
using Random = UnityEngine.Random;
#if UNITY_EDITOR
using UnityEngine;

namespace U3DExtends
{
    public class CommonHelper
    {
        //生成parent下的唯一控件名
        public static string GenerateUniqueName( GameObject parent, string type )
        {
            var widgets = parent.GetComponentsInChildren<RectTransform>();
            var test_num = 1;
            var test_name = type + "_" + test_num;
            RectTransform uiBase = null;
            var prevent_death_count = 0; //防止死循环
            do
            {
                test_name = type + "_" + test_num;
                uiBase = Array.Find(widgets, p => p.gameObject.name == test_name);
                test_num = test_num + Random.Range(1, (prevent_death_count + 1) * 2);
                if (prevent_death_count++ >= 100)
                    break;
            } while (uiBase != null);

            return test_name;
        }

        public static bool IsPointInRect( Vector3 mouse_abs_pos, RectTransform trans )
        {
            if (trans != null)
            {
                var l_t_x = trans.position.x;
                var l_t_y = trans.position.y;
                var r_b_x = l_t_x + trans.sizeDelta.x;
                var r_b_y = l_t_y - trans.sizeDelta.y;
                if (mouse_abs_pos.x >= l_t_x && mouse_abs_pos.y <= l_t_y && mouse_abs_pos.x <= r_b_x &&
                    mouse_abs_pos.y >= r_b_y) return true;
            }
            return false;
        }

        public static Color StringToColor( string hexString )
        {
            var start_index = 2;
            if (hexString.Length == 8)
                start_index = 2;
            else if (hexString.Length == 6)
                start_index = 0;
            var r = byte.Parse(hexString.Substring(start_index, 2), NumberStyles.HexNumber);
            var g = byte.Parse(hexString.Substring(start_index + 2, 2), NumberStyles.HexNumber);
            var b = byte.Parse(hexString.Substring(start_index + 4, 2), NumberStyles.HexNumber);
            return new Color(r / 255.0f, g / 255.0f, b / 255.0f);
        }

        public static string ColorToString( Color color )
        {
            var hexString = "";
            var color_hex = "00";
            color_hex = string.Format("{0:x}", (int) Mathf.Floor(color.r * 255));
            if (color_hex.Length < 2)
                color_hex = "0" + color_hex;
            hexString = hexString + color_hex;
            color_hex = string.Format("{0:x}", (int) Mathf.Floor(color.g * 255));
            if (color_hex.Length < 2)
                color_hex = "0" + color_hex;
            hexString = hexString + color_hex;
            color_hex = string.Format("{0:x}", (int) Mathf.Floor(color.b * 255));
            if (color_hex.Length < 2)
                color_hex = "0" + color_hex;
            hexString = hexString + color_hex;
            //UnityEngine.Debug.Log("hexString :" + hexString);
            return hexString;
        }

        public static Process ProcessCommand( string command, string argument )
        {
            Debug.Log(string.Format("command : {0} argument{1}", command, argument));
            var start = new ProcessStartInfo(command);
            start.Arguments = argument;
            start.CreateNoWindow = true;
            start.ErrorDialog = true;
            start.UseShellExecute = false;

            if (start.UseShellExecute)
            {
                start.RedirectStandardOutput = false;
                start.RedirectStandardError = false;
                start.RedirectStandardInput = false;
            }
            else
            {
                start.RedirectStandardOutput = true;
                start.RedirectStandardError = true;
                start.RedirectStandardInput = true;
                start.StandardOutputEncoding = Encoding.UTF8;
                start.StandardErrorEncoding = Encoding.UTF8;
            }

            var p = Process.Start(start);
            //string output = p.StandardOutput.ReadToEnd();
            //p.WaitForExit();
            return p;
        }

        //获取文件名
        public static string GetFileNameByPath( string path )
        {
            path = path.Replace("\\", "/");
            var last_gang_index = path.LastIndexOf("/");
            if (last_gang_index == -1)
                return "";
            last_gang_index++;
            var name = path.Substring(last_gang_index, path.Length - last_gang_index);
            var last_dot_index = name.LastIndexOf('.');
            if (last_dot_index == -1)
                return "";
            name = name.Substring(0, last_dot_index);
            return name;
        }

        //获取文件类型后缀
        public static string GetFileTypeSuffixByPath( string path )
        {
            path = path.Replace("\\", "/");
            var last_dot_index = path.LastIndexOf('.');
            if (last_dot_index == -1)
                return "";
            last_dot_index++;
            var type_str = path.Substring(last_dot_index, path.Length - last_dot_index);
            return type_str;
        }
    }
}
#endif