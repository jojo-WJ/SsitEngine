/*
*┌──────────────────────────────────────────────────────────────┐
*│　描   述：                                                    
*│　作   者：xuXin                                              
*│　版   本：1.0.0                                                 
*│　创建时间：2019/9/9 16:34:08                     
*└──────────────────────────────────────────────────────────────┘
*/

using SsitEngine.Unity;
using UnityEngine;

namespace Assets.Scripts.World.Component
{
    public class ScriptDayNightCycle : MonoBase
    {
        public Texture2D AmbientColor;

        public Light DirectionLight;
        private float fade;
        public bool FadeOnTime = true;
        public bool Freeze;
        public Texture2D LightColor;
        public bool LightRotation;
        public float MaxFade = 20;
        public Texture2D SkyColors;
        public float TimePerDay = 2;

        [Range(0, 1)] public float Timer;

        private float timeTemp;

        private void Start()
        {
            timeTemp = Time.time;
            fade = Timer;
        }

        private void Update()
        {
            if (!Freeze)
            {
                if (FadeOnTime)
                {
                    var timeperfade = TimePerDay / MaxFade;
                    if (Time.time >= timeTemp + timeperfade)
                    {
                        timeTemp = Time.time;
                        fade += 1.0f / MaxFade;
                    }
                    if (fade > 1.0f)
                    {
                        fade = 0;
                        Timer = 0;
                    }

                    Timer += (fade - Timer) / 10f;
                }
                else
                {
                    if (Timer > 1)
                        Timer = 0;
                    else
                        Timer += 1.0f * Time.deltaTime * (1.0f / TimePerDay);
                }
            }
            if (LightRotation)
                DirectionLight.transform.rotation = Quaternion.Euler(new Vector3(360 * Timer, 0, 0));
            var skyColor = SkyColors.GetPixelBilinear(Timer, 0);
            RenderSettings.skybox.SetColor("_Tint", skyColor);
            RenderSettings.fogColor = skyColor;
            RenderSettings.ambientLight = AmbientColor.GetPixelBilinear(Timer, 0);
            DirectionLight.color = LightColor.GetPixelBilinear(Timer, 0);
        }
    }
}