using SsitEngine.Unity.UI;
using UnityEngine;
using UnityEngine.UI;

namespace SsitEngine.Unity
{
    /*
     * Public Members
     */

    /// <summary>
    ///     字符相关的实用函数
    /// </summary>
    public static class UiUtils
    {
        /// <summary>
        ///     设置时间
        /// </summary>
        /// <param name="time"></param>
        /// <param name="targetText"></param>
        public static void SetTimeText( float time, Text targetText )
        {
            var totalHour = (int) time / 3600;
            var totalMinute = (int) (time - 3600 * totalHour) / 60;
            var totalSecond = (int) (time - 3600 * totalHour) % 60;

            var timeText = string.Format("{0:D2}:{1:D2}:{2:D2}", totalHour, totalMinute, totalSecond);
            if (targetText != null)
            {
                targetText.text = timeText;
            }
        }

        /// <summary>
        ///     设置文本
        /// </summary>
        /// <param name="text"></param>
        /// <param name="targetText"></param>
        public static void ChangeText( string text, Text targetText )
        {
            if (targetText == null)
            {
                Debug.LogError("UiText is null");
                return;
            }
            targetText.text = text;
        }

        public static void ChangeButtonText( string text, Button button )
        {
            if (button == null)
            {
                Debug.LogError("UiText is null");
                return;
            }

            var uiText = button.GetComponentInChildren<Text>(true);
            if (uiText == null)
            {
                Debug.LogError("Button's Text is null");
                return;
            }

            uiText.text = text;
        }

        /// <summary>
        ///     设置进度条
        /// </summary>
        /// <param name="value"></param>
        /// <param name="slider"></param>
        public static void SetProcessSlider( float value, Slider slider )
        {
            if (slider == null)
            {
                Debug.LogError("slider is null");
                return;
            }
            slider.value = value;
        }

        /// <summary>
        ///     隐藏主画布的透明度（除过不理睬父物体布局的对象）
        /// </summary>
        /// <param name="value"></param>
        public static void HideCanvasRootAlpha( float value )
        {
            var canvas = Engine.Instance.Platform.MainCanvas;

            if (canvas)
            {
                var gGroup = canvas.GetComponent<CanvasGroup>();
                if (gGroup)
                {
                    gGroup.alpha = value;
                }
            }
        }

        /// <summary>
        ///     隐藏画布节点的透明度（除过不理睬父物体布局的对象）
        /// </summary>
        /// <param name="formType">节点类型</param>
        /// <param name="value">透明度值</param>
        public static void HideCanvasAlpha( UIFormType formType, float value )
        {
            var canvas = UIManager.Instance.GetCanvasNode(formType);

            if (canvas)
            {
                var gGroup = canvas.GetOrAddComponent<CanvasGroup>();
                if (gGroup)
                {
                    gGroup.alpha = value;
                }
            }
        }
    }
}