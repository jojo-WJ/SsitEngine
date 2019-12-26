using UnityEngine;
using UnityEngine.UI;

public class vSliderSizeControl : MonoBehaviour
{
    public float multipScale = 0.1f;
    private float oldMaxValue;
    public RectTransform rectTransform;
    public Slider slider;

    private void OnDrawGizmosSelected()
    {
        UpdateScale();
    }

    public void UpdateScale()
    {
        if (rectTransform && slider)
            if (slider.maxValue != oldMaxValue)
            {
                var sizeDelta = rectTransform.sizeDelta;
                sizeDelta.x = slider.maxValue * multipScale;
                rectTransform.sizeDelta = sizeDelta;
                oldMaxValue = slider.maxValue;
            }
    }
}