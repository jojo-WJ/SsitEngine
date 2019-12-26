using System;
using System.Runtime.Serialization;
using SsitEngine.EzReplay;

namespace Framework.Data
{
    /// <summary>
    /// 场景信息数据
    /// </summary>
    [Serializable]
    public class SceneInfo : InfoData
    {
        public string Guid; //场景guid
        public string SceneType; //场景类型
        public WeatherInfo weatherInfo = new WeatherInfo(); //场景天气信息

        #region 回放

        public override bool IsDifferentTo( SavedBase state, Object2PropertiesMapping o2m )
        {
            return isChange;
        }

        #endregion


        #region 序列化

        public SceneInfo()
        {
        }

        public SceneInfo( SerializationInfo info, StreamingContext context )
        {
            weatherInfo = (WeatherInfo) info.GetValue("mWeatherCache", typeof(WeatherInfo));
        }

        public override void GetObjectData( SerializationInfo info, StreamingContext context )
        {
            info.AddValue("mWeatherCache", weatherInfo, typeof(WeatherInfo));
        }

        #endregion
    }
    
     /// <summary>
    /// 天气信息
    /// </summary>
    [Serializable]
    public class WeatherInfo : InfoData, ISerializable
    {
        public EnWeather Weather; //天气类型
        public EnWindDirection WindDirection; //风向
        public int WindLevel; //风级
        public double WindVelocity = 1f; //风速

        public override string ToString()
        {
            var str = string.Empty;

            switch (Weather)
            {
                case EnWeather.SUN:
                {
                    str += "晴";
                }
                    break;
                case EnWeather.FOG:
                {
                    str += "雾";
                }
                    break;
                case EnWeather.RAINY:
                {
                    str += "雨";
                }
                    break;
                case EnWeather.SNOWY:
                {
                    str += "雪";
                }
                    break;
                case EnWeather.WINDY:
                {
                    str += "风";
                }
                    break;
                case EnWeather.CLOUDY:
                {
                    str += "多云";
                }
                    break;
            }

            str += '-';
            switch (WindDirection)
            {
                case EnWindDirection.EAST:
                {
                    str += "东风";
                }
                    break;
                case EnWindDirection.WEST:
                {
                    str += "西风";
                }
                    break;
                case EnWindDirection.SOUTH:
                {
                    str += "南风";
                }
                    break;
                case EnWindDirection.NORTH:
                {
                    str += "北风";
                }
                    break;
                case EnWindDirection.NORTHEAST:
                {
                    str += "东北风";
                }
                    break;
                case EnWindDirection.NORTHWEST:
                {
                    str += "西北风";
                }
                    break;
                case EnWindDirection.SOUTHEAST:
                {
                    str += "东南风";
                }
                    break;
                case EnWindDirection.SOUTHWEST:
                {
                    str += "西南风";
                }
                    break;
                case EnWindDirection.NOWIND:
                {
                    str += "无风";
                }
                    break;
            }

            str += "-" + WindLevel + "级";
            return str;
        }

        #region 序列化

        public WeatherInfo()
        {
        }

        public WeatherInfo( SerializationInfo info, StreamingContext context )
        {
            Weather = (EnWeather) info.GetValue("Weather", typeof(EnWeather));
            WindDirection = (EnWindDirection) info.GetValue("WindDirection", typeof(EnWindDirection));
            WindLevel = info.GetInt32("WindLevel");

            //WindVelocity = info.GetInt64("WindVelocity");
        }

        public override void GetObjectData( SerializationInfo info, StreamingContext context )
        {
            info.AddValue("Weather", Weather);
            info.AddValue("WindDirection", WindDirection);
            info.AddValue("WindLevel", WindLevel);
            //info.AddValue( "WindVelocity", this.Weather,typeof(Int64) );
        }

        #endregion
    }
}