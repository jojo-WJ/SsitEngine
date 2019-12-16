/*
*┌──────────────────────────────────────────────────────────────┐
*│　描   述：                                                    
*│　作   者：xuXin                                              
*│　版   本：1.0.0                                                 
*│　创建时间：2019/4/1 19:29:56                     
*└──────────────────────────────────────────────────────────────┘
*/

using System;
using UnityEngine;
using Random = System.Random;

namespace SsitEngine
{
    /// <summary>
    ///     随机相关的实用函数。
    /// </summary>
    public static class RandomUtils
    {
        private static Random s_Random;

        /// <summary>
        ///     静态构造
        /// </summary>
        /// <remarks>静态函数的调用时机，是在类被实例化或者静态成员被调用的时候进行调用，并且是由.net框架来调用静态构造函数来初始化静态成员变量</remarks>
        static RandomUtils()
        {
            s_Random = new Random((int) DateTime.Now.Ticks);
        }

        /// <summary>
        ///     设置随机数种子。
        /// </summary>
        /// <param name="seed">随机数种子。</param>
        public static void SetSeed( int seed )
        {
            s_Random = new Random(seed);
        }

        /// <summary>
        ///     返回非负随机数。
        /// </summary>
        /// <returns>大于等于零且小于 System.Int32.MaxValue 的 32 位带符号整数。</returns>
        public static int GetRandom()
        {
            return s_Random.Next();
        }

        /// <summary>
        ///     返回一个小于所指定最大值的非负随机数。
        /// </summary>
        /// <param name="maxValue">要生成的随机数的上界（随机数不能取该上界值）。maxValue 必须大于等于零。</param>
        /// <returns>大于等于零且小于 maxValue 的 32 位带符号整数，即：返回值的范围通常包括零但不包括 maxValue。不过，如果 maxValue 等于零，则返回 maxValue。</returns>
        public static int GetRandom( int maxValue )
        {
            return s_Random.Next(maxValue);
        }

        /// <summary>
        ///     返回一个指定范围内的随机整数。
        /// </summary>
        /// <param name="minValue">返回的随机数的下界（随机数可取该下界值）。</param>
        /// <param name="maxValue">返回的随机数的上界（随机数不能取该上界值）。maxValue 必须大于等于 minValue。</param>
        /// <returns>
        ///     一个大于等于 minValue 且小于 maxValue 的 32 位带符号整数，即：返回的值范围包括 minValue 但不包括 maxValue。如果 minValue 等于 maxValue，则返回
        ///     minValue。
        /// </returns>
        public static int GetRandom( int minValue, int maxValue )
        {
            return s_Random.Next(minValue, maxValue);
        }

        /// <summary>
        ///     返回一个指定范围内的随机单精度数
        /// </summary>
        /// <param name="minValue"></param>
        /// <param name="maxValue"></param>
        /// <returns>一个大于等于 minValue 且小于 maxValue 的 单精度浮点数</returns>
        public static float GetRandom( float minValue, float maxValue )
        {
            var delta = maxValue - minValue;
            return (float) GetRandomDouble() * delta + minValue;
        }

        /// <summary>
        ///     单位圆内
        /// </summary>
        /// <returns>一个单位圆内的坐标</returns>
        public static Vector2 InsideCircleUnit()
        {
            var theta = GetRandom(0, (float) Math.PI * 2);
            var radius = GetRandom(0, 1f);
            return new Vector2((float) Math.Cos(theta) * radius, (float) Math.Sin(theta) * radius);
        }

        /// <summary>
        ///     获取单位球内随机坐标
        /// </summary>
        /// <returns>一个单位球内的坐标</returns>
        public static Vector3 InsideSphereUnit()
        {
            var x = GetRandom(-10f, 10f);
            var y = GetRandom(-10f, 10f);
            var z = GetRandom(-10f, 10f);
            var radius = (float) Math.Sqrt(x * x + y * y + z * z);
            var rate = GetRandom(0, 1f);

            return new Vector3(rate * x / radius, rate * y / radius, rate * z / radius);
        }

        /// <summary>
        ///     获取单位圆上的随机坐标
        /// </summary>
        /// <returns></returns>
        public static Vector2 OnCircleUnit()
        {
            var theta = GetRandom(0, (float) Math.PI * 2);
            return new Vector2((float) Math.Cos(theta), (float) Math.Sin(theta));
        }

        /// <summary>
        ///     获取单位球面的随机坐标
        /// </summary>
        /// <returns>单位球面的随机坐标</returns>
        public static Vector3 OnSphereUnit()
        {
            var x = GetRandom(-10f, 10f);
            var y = GetRandom(-10f, 10f);
            var z = GetRandom(-10f, 10f);
            var radius = (float) Math.Sqrt(x * x + y * y + z * z);
            return new Vector3(x / radius, y / radius, z / radius);
        }

        /// <summary>
        ///     返回一个介于 0.0 和 1.0 之间的随机数。
        /// </summary>
        /// <returns>大于等于 0.0 并且小于 1.0 的双精度浮点数。</returns>
        public static double GetRandomDouble()
        {
            return s_Random.NextDouble();
        }

        /// <summary>
        ///     用随机数填充指定字节数组的元素。
        /// </summary>
        /// <param name="buffer">包含随机数的字节数组。</param>
        public static void GetRandomBytes( byte[] buffer )
        {
            s_Random.NextBytes(buffer);
        }
    }
}