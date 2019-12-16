using UnityEngine;

namespace SsitEngine.Unity
{
    /// <summary>
    ///     Untiy Gameject对象的扩展
    /// </summary>
    public static class GameObjectExtension
    {
        /// <summary>
        ///     查找子节点对象
        /// </summary>
        /// <param name="go">扩展的类型对象</param>
        /// <param name="name">名称</param>
        /// <param name="endsWith">特定名称后缀</param>
        /// <returns></returns>
        public static GameObject FindTheChildNode( this GameObject go, string name, bool endsWith = false )
        {
            if (endsWith ? go.transform.name == name : go.transform.name.EndsWith(name)) return go;
            foreach (Transform child in go.transform)
            {
                var result = child.FindDeepChild(name);
                if (result != null) return result.gameObject;
            }

            //SsitDebug.Warning(TextUtils.Format("查找对象不存在 {0}", name));
            return null;
        }

        /// <summary>
        ///     获取节点脚本
        /// </summary>
        public static T GetChildNodeComponentScripts<T>( this GameObject goParent, string childName,
            bool endsWith = false ) where T : Component
        {
            var searchTranformNode = FindTheChildNode(goParent, childName);
            if (searchTranformNode)
                return searchTranformNode.gameObject.GetComponent<T>();
            return null;
        }

        /// <summary>
        ///     给子节点添加脚本
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="goParent"></param>
        /// <param name="childName"></param>
        /// <returns></returns>
        public static T GetOrAddChildComponent<T>( GameObject goParent, string childName ) where T : Component
        {
            GameObject searchTranformNode = null; //查找特定子节点
            searchTranformNode = FindTheChildNode(goParent, childName);
            if (searchTranformNode)
                return searchTranformNode.gameObject.GetOrAddComponent<T>();
            return null;
        }


        public static T GetOrAddComponent<T>( this GameObject go ) where T : Component
        {
            var com = go.GetComponent<T>();
            if (com != null)
                return com;
            return go.AddComponent<T>();
        }


        /// <summary>
        ///     搜索指定名称子物体组件-GameObject
        /// </summary>
        /// <typeparam name="T">脚本类型</typeparam>
        /// <param name="go">物体预制</param>
        /// <param name="subnode">子物体名称</param>
        /// <returns>返回脚本组件</returns>
        public static T Get<T>( this GameObject go, string subnode ) where T : Component
        {
            if (go != null)
            {
                var sub = go.transform.Find(subnode);
                if (sub != null) return sub.GetComponent<T>();
            }
            return null;
        }

        /// <summary>
        ///     搜索指定名称子物体组件-Transform
        /// </summary>
        /// <typeparam name="T">脚本类型</typeparam>
        /// <param name="tr">物体Transform</param>
        /// <param name="subnode">子物体名称</param>
        /// <returns>返回脚本组件</returns>
        public static T Get<T>( this Transform tr, string subnode ) where T : Component
        {
            if (tr != null) return tr.gameObject.Get<T>(subnode);
            return null;
        }

        /// <summary>
        ///     搜索指定名称子物体组件-componet
        /// </summary>
        /// <typeparam name="T">脚本类型</typeparam>
        /// <param name="cp">组件</param>
        /// <param name="subnode">子物体名称</param>
        /// <returns>返回脚本组件</returns>
        public static T Get<T>( this Component cp, string subnode ) where T : Component
        {
            if (cp != null) return cp.gameObject.Get<T>(subnode);
            return null;
        }

        /// <summary>
        ///     查找子对象-Gameobject
        /// </summary>
        /// <param name="go"></param>
        /// <param name="subnode">子对象名称</param>
        /// <returns></returns>
        public static GameObject Child( this GameObject go, string subnode )
        {
            return go.transform.Child(subnode);
        }

        /// <summary>
        ///     查找子对象-Transform
        /// </summary>
        /// <param name="go"></param>
        /// <param name="subnode">子对象名称</param>
        /// <returns></returns>
        public static GameObject Child( this Transform go, string subnode )
        {
            var tran = go.Find(subnode);
            if (tran == null) return null;
            return tran.gameObject;
        }


        /// <summary>
        ///     清除所有子节点-Gameobject
        /// </summary>
        /// <param name="go">待清除父物体</param>
        public static void ClearAllChild( this GameObject go, bool immediate = false )
        {
            if (go == null) return;
            go.transform.ClearAllChild(immediate);
        }

        /// <summary>
        ///     清除所有子节点-Transform
        /// </summary>
        /// <param name="tr">待清除父物体</param>
        public static void ClearAllChild( this Transform tr, bool immediate = false )
        {
            if (tr == null) return;
            for (var i = tr.childCount - 1; i >= 0; i--)
                if (immediate)
                    Object.DestroyImmediate(tr.GetChild(i).gameObject);
                else
                    Object.Destroy(tr.GetChild(i).gameObject);
        }

        /// <summary>Set layer include it's children.</summary>
        public static void BroadcastLayer( this GameObject gameObject, int layer )
        {
            gameObject.layer = layer;
            foreach (Component component in gameObject.transform)
                component.gameObject.BroadcastLayer(layer);
        }
    }
}