using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using Random = System.Random;

/// <summary>
///     Some useful extension functions to use in the SoundManager.
/// </summary>
public static class SoundManagerTools
{
    private static readonly Random random = new Random();

    /// <summary>
    ///     Shuffle the specified list.
    /// </summary>
    /// <param name='theList'>
    ///     The list.
    /// </param>
    /// <typeparam name='T'>
    ///     The 1st type parameter.
    /// </typeparam>
    public static void Shuffle<T>( ref List<T> theList )
    {
        var n = theList.Count;
        while (n > 1)
        {
            n--;
            var k = random.Next(n + 1);
            var val = theList[k];
            theList[k] = theList[n];
            theList[n] = val;
        }
    }

    /// <summary>
    ///     Shuffles two lists together identically.
    /// </summary>
    /// <param name='theList'>
    ///     The list.
    /// </param>
    /// <param name='otherList'>
    ///     The second list.
    /// </param>
    /// <typeparam name='T'>
    ///     The 1st type parameter.
    /// </typeparam>
    /// <typeparam name='K'>
    ///     The 2nd type parameter.
    /// </typeparam>
    public static void ShuffleTwo<T, K>( ref List<T> theList, ref List<K> otherList )
    {
        var n = theList.Count;
        while (n > 1)
        {
            n--;
            var k = random.Next(n + 1);
            var val = theList[k];
            theList[k] = theList[n];
            theList[n] = val;

            if (otherList.Count != theList.Count)
            {
                Debug.LogError("Can't shuffle both lists because this " + typeof(T) +
                               " list doesn't have the same amount of items.");
                continue;
            }
            var otherVal = otherList[k];
            otherList[k] = otherList[n];
            otherList[n] = otherVal;
        }
    }

    public static void make2D( ref AudioSource theAudioSource )
    {
        theAudioSource.spatialBlend = 0f;
    }

    public static void make3D( ref AudioSource theAudioSource )
    {
        theAudioSource.spatialBlend = 1f;
    }

    /// <summary>
    ///     Vary a float with restrictions.
    /// </summary>
    /// <returns>
    ///     The varied float.
    /// </returns>
    /// <param name='theFloat'>
    ///     The float.
    /// </param>
    /// <param name='variance'>
    ///     Variance.
    /// </param>
    /// <param name='minimum'>
    ///     Minimum value.
    /// </param>
    /// <param name='maximum'>
    ///     Maximum value.
    /// </param>
    public static float VaryWithRestrictions( this float theFloat, float variance, float minimum = 0f,
        float maximum = 1f )
    {
        var max = theFloat * (1f + variance);
        var min = theFloat * (1f - variance);

        if (max > maximum)
        {
            max = maximum;
        }
        if (min < minimum)
        {
            min = minimum;
        }

        return UnityEngine.Random.Range(min, max);
    }

    /// <summary>
    ///     Vary a float.
    /// </summary>
    /// <returns>
    ///     变化的值.
    /// </returns>
    /// <param name='theFloat'>
    ///     The float.
    /// </param>
    /// <param name='variance'>
    ///     Variance.
    /// </param>
    public static float Vary( this float theFloat, float variance )
    {
        var max = theFloat * (1f + variance);
        var min = theFloat * (1f - variance);

        return UnityEngine.Random.Range(min, max);
    }

    /// <summary>
    ///     获取当前类的所有字段信息
    /// </summary>
    /// <param name="type"></param>
    /// <returns></returns>
    public static FieldInfo[] GetAllFieldInfos( this Type type )
    {
        if (type == null)
        {
            return new FieldInfo[0];
        }
        /*
         BindingFlags.Public            可在搜索中包含公共成员
         BindingFlags.NonPublic         可在搜索中包含非公共成员（即私有成员和受保护的成员）
         BindingFlags.FlattenHierarchy  可包含层次结构上的静态成员
         BindingFlags.IgnoreCase，      表示忽略 name 的大小写。
         BindingFlags.DeclaredOnly，    仅搜索 Type 上声明的成员，而不搜索被简单继承的成员。
         BindingFlags.Instance          可搜索实例成员
         */

        var flags =
            BindingFlags.Public |
            BindingFlags.NonPublic |
            BindingFlags.Instance |
            BindingFlags.DeclaredOnly;

        return type.GetFields(flags).Concat(GetAllFieldInfos(type.BaseType))
            .Where(f => !f.IsDefined(typeof(HideInInspector), true))
            .ToArray();
    }
}