/*
*┌──────────────────────────────────────────────────────────────┐
*│　描   述：                                                    
*│　作   者：xuXin                                              
*│　版   本：1.0.0                                                 
*│　创建时间：2019/11/27 21:02:08                     
*└──────────────────────────────────────────────────────────────┘
*/

using System.Collections.Generic;
using UnityEngine;

namespace SsitEngine.Unity.UI.Common.Utility
{
    /// <summary>Utility for mesh.</summary>
    public static class MeshUtility
    {
        /// <summary>Create vertices base on polygon.</summary>
        /// <param name="edge">Edge count of polygon.</param>
        /// <param name="radius">Radius of polygon.</param>
        /// <param name="center">Center of polygon.</param>
        /// <param name="rotation">Rotation of polygon.</param>
        /// <returns>Vertices base on polygon.</returns>
        public static List<Vector3> CreateVerticesBasePolygon(
            int edge,
            float radius,
            Vector3 center,
            Quaternion rotation )
        {
            var vector3List = new List<Vector3>();
            var num = 6.283185f / edge;
            for (var index = 0; index <= edge; ++index)
            {
                var f = num * index;
                vector3List.Add(center + rotation * new Vector3(Mathf.Cos(f), Mathf.Sin(f)) * radius);
            }
            return vector3List;
        }

        /// <summary>
        ///     Create triangles index base on polygon and center vertice.
        /// </summary>
        /// <param name="edge">Edge count of polygon.</param>
        /// <param name="center">Index of center vertice.</param>
        /// <param name="start">Index of start vertice.</param>
        /// <param name="clockwise">Triangle indexs is clockwise.</param>
        /// <returns>Triangles base on polygon.</returns>
        public static List<int> CreateTrianglesBasePolygon(
            int edge,
            int center,
            int start,
            bool clockwise = true )
        {
            var intList = new List<int>();
            var num = clockwise ? 0 : 1;
            for (var index = 0; index < edge; ++index)
            {
                intList.Add(start + index + num);
                intList.Add(start + index - num + 1);
                intList.Add(center);
            }
            return intList;
        }

        /// <summary>Create triangles index base on prism.</summary>
        /// <param name="polygon">Edge count of prism polygon.</param>
        /// <param name="segment">Segment count of prism vertices vertical division.</param>
        /// <param name="start">Start index of prism vertice.</param>
        /// <returns>Triangles index base on prism.</returns>
        public static List<int> CreateTrianglesBasePrism( int polygon, int segment, int start )
        {
            var intList = new List<int>();
            var num1 = polygon + 1;
            for (var index1 = 0; index1 < segment - 1; ++index1)
            {
                var num2 = num1 * index1;
                var num3 = num1 * (index1 + 1);
                for (var index2 = 0; index2 < polygon; ++index2)
                {
                    intList.Add(start + num2 + index2);
                    intList.Add(start + num2 + index2 + 1);
                    intList.Add(start + num3 + index2 + 1);
                    intList.Add(start + num2 + index2);
                    intList.Add(start + num3 + index2 + 1);
                    intList.Add(start + num3 + index2);
                }
            }
            return intList;
        }

        /// <summary>Create uv base on polygon.</summary>
        /// <param name="edge">Edge count of polygon.</param>
        /// <returns>UV base on polygon.</returns>
        public static List<Vector2> CreateUVBasePolygon( int edge )
        {
            var vector2List = new List<Vector2>();
            var num = 6.283185f / edge;
            var vector2 = Vector2.one * 0.5f;
            for (var index = 0; index <= edge; ++index)
            {
                var f = num * index;
                vector2List.Add(vector2 + new Vector2(Mathf.Cos(f), Mathf.Sin(f)) * 0.5f);
            }
            return vector2List;
        }

        /// <summary>Create uv base on prism.</summary>
        /// <param name="polygon">Edge count of prism polygon.</param>
        /// <param name="segment">Segment count of prism vertices vertical division.</param>
        /// <returns>UV base on prism.</returns>
        public static List<Vector2> CreateUVBasePrism( int polygon, int segment )
        {
            var vector2List = new List<Vector2>();
            var num1 = polygon + 1;
            var num2 = num1 * segment;
            var num3 = 1f / polygon;
            for (var index = 0; index < num2; ++index)
            {
                var x = num3 * (index % num1);
                float y = index / num1 % 2;
                vector2List.Add(new Vector2(x, y));
            }
            return vector2List;
        }
    }
}