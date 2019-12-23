using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public static class UnityTransformExtension
{
    /// <summary>
    ///     查找子物体（递归查找）
    /// </summary>
    /// <param name="parentTras">父物体</param>
    /// <param name="goName">子物体的名称</param>
    /// <returns>找到的相应子物体</returns>
    public static T Find<T>( T parentTras, string goName ) where T : Component
    {
        if (goName == parentTras.name)
        {
            return parentTras.transform.GetComponent<T>();
        }
        //现在parentTrans的第一层找，找不见再去它的子物体中去找
        var child = parentTras.transform.Find(goName);
        if (child != null)
        {
            return child.GetComponent<T>();
        }

        Transform go = null;
        for (var i = 0; i < parentTras.transform.childCount; i++)
        {
            child = parentTras.transform.GetChild(i);
            go = Find(child, goName);
            if (go != null)
            {
                return go.GetComponent<T>();
            }
        }
        return null;
    }

    /// <summary>
    ///     递归查找子物体，返回第一个找见的子物体
    /// </summary>
    /// <param name="parentTrans"></param>
    /// <param name="goName"></param>
    /// <returns></returns>
    public static GameObject FindObj( this GameObject parentTrans, string goName )
    {
        return Find(parentTrans.transform, goName).gameObject;
    }

    /// <summary>
    ///     获取gameObject子物体中名字为goName的物体上的T组件
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="parentObj"></param>
    /// <param name="goName"></param>
    /// <returns></returns>
    public static T Find<T>( this GameObject parentObj, string goName ) where T : Component
    {
        return Find(parentObj.transform, goName).GetComponent<T>();
    }

    /// <summary>
    ///     查找子物体上的组件（递归查找）
    /// </summary>
    /// <param name="parentTras">父物体Transform</param>
    /// <param name="goName">子物体的名称</param>
    /// <returns>找到的相应子物体</returns>
    public static T Find<T>( this Transform parentTras, string goName = null ) where T : Component
    {
        var tran = Find(parentTras, goName);
        if (tran == null || tran.GetComponent<T>() == false)
        {
            return null;
        }


        return tran.GetComponent<T>();
    }

    /// <summary>获取一级的孩子</summary>
    /// <param name="parent">父物体位置</param>
    /// <returns>所有子物体数组</returns>
    public static List<T> GetFirstSblingChilds<T>( this T parent ) where T : Component
    {
        var childList = new List<T>();
        for (var i = 0; i < parent.transform.childCount; i++)
        {
            var t = parent.transform.GetChild(i).GetComponent<T>();
            if (t)
            {
                childList.Add(t);
            }
        }

        return childList;
    }


    /// <summary>
    ///     给子节点添加父对象
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <typeparam name="U"></typeparam>
    /// <param name="child"></param>
    /// <param name="parent"></param>
    /// <param name="worldPostionStay"></param>
    /// <returns></returns>
    public static T Parent<T, U>( this T child, U parent, bool worldPostionStay = false )
        where T : Component where U : Component
    {
        child.transform.SetParent(parent.transform, worldPostionStay);
        return child;
    }

    /// <summary>
    ///     将T设置为标准的Transform。LocalPosition =LocalEulerAngle =vector.zero;localScale =vector.one
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="obj"></param>
    /// <returns></returns>
    public static T SetStandardTrans<T>( this T obj ) where T : Component
    {
        obj.transform.localEulerAngles = Vector3.zero;
        obj.transform.localPosition = Vector3.zero;
        obj.transform.localScale = Vector3.one;
        return obj;
    }

    /// <summary>
    ///     设置相对位置
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="Obj"></param>
    /// <param name="localPos"></param>
    /// <returns></returns>
    public static T SetLocalPosition<T>( this T Obj, Vector3 localPos ) where T : Component
    {
        Obj.transform.localPosition = localPos;
        return Obj;
    }

    public static T SetLocalPosition<T>( this T Obj, float x, float y, float z ) where T : Component
    {
        Obj.transform.localPosition = new Vector3(x, y, z);
        return Obj;
    }

    /// <summary>
    ///     设置位置
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="Obj"></param>
    /// <param name="localPos"></param>
    /// <returns></returns>
    public static T SetPosition<T>( this T Obj, Vector3 localPos ) where T : Component
    {
        Obj.transform.position = localPos;
        return Obj;
    }

    public static T SetPosition<T>( this T Obj, float x, float y, float z ) where T : Component
    {
        Obj.transform.position = new Vector3(x, y, z);
        return Obj;
    }

    /// <summary>
    ///     设置相对欧拉角
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="Obj"></param>
    /// <param name="localPos"></param>
    /// <returns></returns>
    public static T SetLocalEulerAngle<T>( this T Obj, Vector3 localAngle ) where T : Component
    {
        Obj.transform.localEulerAngles = localAngle;
        return Obj;
    }

    public static T SetLocalEulerAngle<T>( this T Obj, float x, float y, float z ) where T : Component
    {
        Obj.transform.localEulerAngles = new Vector3(x, y, z);
        return Obj;
    }

    /// <summary>
    ///     设置欧拉角
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="Obj"></param>
    /// <param name="localPos"></param>
    /// <returns></returns>
    public static T SetEulerAngle<T>( this T Obj, Vector3 localAngle ) where T : Component
    {
        Obj.transform.eulerAngles = localAngle;
        return Obj;
    }

    public static T SetEulerAngle<T>( this T Obj, float x, float y, float z ) where T : Component
    {
        Obj.transform.eulerAngles = new Vector3(x, y, z);
        return Obj;
    }

    /// <summary>
    ///     设置相对于父物体的缩放
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="self"></param>
    /// <param name="vec"></param>
    /// <returns></returns>
    public static T SetLocalScale<T>( this T self, Vector3 vec ) where T : Component
    {
        self.transform.localScale = vec;
        return self;
    }

    public static T SetLocalScale<T>( this T self, float x, float y, float z ) where T : Component
    {
        self.transform.localScale = new Vector3(x, y, z);
        return self;
    }

    /// <summary>
    ///     设置Transform的AnchredPosition
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="self"></param>
    /// <param name="vec"></param>
    /// <returns></returns>
    public static T SetAnChoredPos<T>( this T self, Vector2 vec ) where T : Component
    {
        self.GetComponent<RectTransform>().anchoredPosition = vec;
        return self;
    }

    public static T SetAnChoredPos<T>( this T self, float x, float y ) where T : Component
    {
        self.GetComponent<RectTransform>().anchoredPosition = new Vector2(x, y);
        return self;
    }

    public static T SetSizeDelta<T>( this T self, Vector2 vec ) where T : Component
    {
        self.GetComponent<RectTransform>().sizeDelta = vec;
        return self;
    }

    public static T SetSizeDelta<T>( this T self, float x, float y ) where T : Component
    {
        self.GetComponent<RectTransform>().sizeDelta = new Vector2(x, y);
        return self;
    }


    /// <summary>
    ///     设置名称
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="Obj"></param>
    /// <param name="objName"></param>
    /// <returns></returns>
    public static T SetName<T>( this T Obj, string objName ) where T : Component
    {
        Obj.name = objName;
        return Obj;
    }

    /// <summary>
    ///     设置显隐
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="obj"></param>
    /// <param name="isActive"></param>
    /// <returns></returns>
    public static T SetActive<T>( this T obj, bool isActive ) where T : Component
    {
        obj.gameObject.SetActive(isActive);
        return obj;
    }


    public static T SetSbling<T>( this T obj, int sblingIndex ) where T : Component
    {
        obj.transform.SetSiblingIndex(sblingIndex);
        return obj;
    }

    /// <summary>
    ///     设置UI的交互属性
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="obj"></param>
    /// <param name="isActive"></param>
    /// <returns></returns>
    public static T SetInteractable<T>( this T obj, bool isActive ) where T : Selectable
    {
        obj.interactable = isActive;
        return obj;
    }
}