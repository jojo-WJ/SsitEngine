using System;
using System.Collections.Generic;
using SsitEngine.DebugLog;
using UnityEngine;

namespace SsitEngine.Unity.Avatar
{
    public static class AvatarHelper
    {
        #region Mesh Manager

        private const int COMBINE_TEXTURE_MAX = 512;
        private const string COMBINE_DIFFUSE_TEXTURE = "_MainTex";

        /// <summary>
        /// 1.替换蒙皮网格（或者直接替换模型换装部位的GameObject，因为合并的时候会合并所有的蒙皮网格，而不会关心它是否属于原来角色身体的一部分，而且如果需要替换的部位有多个配件拥有独立的网格和贴图，那这种方式都可以正常执行。我下面的代码就是直接替换了换装部位的GameObject）
        /// 2.合并所有蒙皮网格
        /// 3.刷新骨骼
        /// 4.附加材质（我下面是获取第一个材质作为默认材质）
        /// 5.合并贴图（贴图的宽高最好是2的N次方的值）
        /// 6.重新计算UV
        /// </summary>
        /// <param name="skeleton"></param>
        /// <param name="rootBone">根骨骼节点</param>
        /// <param name="meshes"></param>
        /// <param name="combine"></param>
        public static void CombineObject( GameObject skeleton, Transform rootBone, SkinnedMeshRenderer[] meshes,
            List<Transform> transforms, bool combine = false )
        {
            // Fetch all bones of the skeleton
            // transforms是骨架上所有的transform组件的List
            //List<Transform> transforms = new List<Transform>();
            //transforms.AddRange( skeleton.GetComponentsInChildren<Transform>() );

            // the list of materials
            // materials是所有身体部件上的所有Material组成的List
            var materials = new List<Material>();

            // the list of meshes
            // combineInstances是所有身体部件上所有的mesh组成的List
            var combineInstances = new List<CombineInstance>();

            //the list of bones
            var bones = new List<Transform>();

            // Below informations only are used for merge materilas(bool combine = true)
            List<Vector2[]> oldUV = null;
            Material newMaterial = null;
            Texture2D newDiffuseTex = null;

            // Collect information from meshes
            // 这里分别把所有Material，Mesh，Transform(骨头)保存到对应的List
            for (var i = 0; i < meshes.Length; i++)
            {
                if (meshes[i].gameObject.Equals(skeleton)) continue;
                var smr = meshes[i];
                // Collect materials
                materials.AddRange(smr.materials);

                // Collect meshes
                for (var sub = 0; sub < smr.sharedMesh.subMeshCount; sub++)
                {
                    var ci = new CombineInstance();
                    ci.mesh = smr.sharedMesh;
                    ci.subMeshIndex = sub;
                    combineInstances.Add(ci);
                }

                // Collect bones
                // 收集骨头有点区别：只收集骨架中有的。这应该会涉及到具体的美术标准。
                for (var j = 0; j < smr.bones.Length; j++)
                {
                    var tBase = 0;
                    for (tBase = 0; tBase < transforms.Count; tBase++)
                    {
                        if (transforms[tBase] == null)
                        {
                            SsitDebug.Error("不要在原始骨骼上绑任何装备，这都都是不合理的");
                            continue;
                        }
                        if (smr.bones[j].name == transforms[tBase].name)
                        {
                            //Debug.Log("Equals bones " + smr.bones[j].name);
                            bones.Add(transforms[tBase]);
                            break;
                        }
                    }
                }
            }

            // merge materials
            // Material合并，主要是处理贴图和其UV
            if (combine)
            {
                newMaterial = new Material(Shader.Find("Mobile/Diffuse"));
                oldUV = new List<Vector2[]>();
                // merge the texture
                // 合并贴图（从收集的各Material中取）
                // Textures是所有贴图组成的列表
                var Textures = new List<Texture2D>();
                for (var i = 0; i < materials.Count; i++)
                    Textures.Add(materials[i].GetTexture(COMBINE_DIFFUSE_TEXTURE) as Texture2D);

                //所有贴图合并到newDiffuseTex这张大贴图上
                newDiffuseTex = new Texture2D(COMBINE_TEXTURE_MAX, COMBINE_TEXTURE_MAX, TextureFormat.RGBA32, true);
                var uvs = newDiffuseTex.PackTextures(Textures.ToArray(), 0);
                newMaterial.mainTexture = newDiffuseTex;

                // reset uv
                // 根据原来单个上的uv算出合并后的uv，uva是单个的，uvb是合并后的。
                // uva取自combineInstances[j].mesh.uv
                // 用oldUV保存uva。为什么要保存uva？它不是单个吗？先跳过往下看
                // 计算好uvb赋值到到combineInstances[j].mesh.uv
                Vector2[] uva, uvb;
                for (var j = 0; j < combineInstances.Count; j++)
                {
                    //uva = (Vector2[])(combineInstances[j].mesh.uv);
                    uva = combineInstances[j].mesh.uv;
                    uvb = new Vector2[uva.Length];
                    for (var k = 0; k < uva.Length; k++)
                        uvb[k] = new Vector2(uva[k].x * uvs[j].width + uvs[j].x, uva[k].y * uvs[j].height + uvs[j].y);
                    //oldUV.Add(combineInstances[j].mesh.uv);
                    oldUV.Add(uva);
                    combineInstances[j].mesh.uv = uvb;
                }
            }

            // Create a new SkinnedMeshRenderer
            var r = skeleton.GetComponent<SkinnedMeshRenderer>();
            if (r == null)
                //GameObject.DestroyImmediate( oldSKinned );
                r = skeleton.AddComponent<SkinnedMeshRenderer>();
            r.sharedMesh = new Mesh();
            // Combine meshes
            r.sharedMesh.CombineMeshes(combineInstances.ToArray(), combine, false);
            r.rootBone = rootBone;
            // Use new bones
            r.bones = bones.ToArray();
            if (combine)
            {
                //Debug.Log( "combine " + combine );
                r.material = newMaterial;
                for (var i = 0; i < combineInstances.Count; i++)
                    // 这为什么要用oldUV，这不是保存的uva吗？它是单个的uv呀？
                    // 原因在于，这行代码其实并不影响显示，影响显示的是在Mesh合并前的uv。
                    // 这行的意义在于合并后，又要换部件时，在新的合并过程中找到正确的单个uv。
                    // 也是oldUV存在的意义。
                    combineInstances[i].mesh.uv = oldUV[i];
            }
            else
            {
                //Debug.Log( "combine " + combine );
                r.materials = materials.ToArray();
            }
        }

        /// <summary>
        /// 1.替换蒙皮网格（或者直接替换模型换装部位的GameObject，因为合并的时候会合并所有的蒙皮网格，而不会关心它是否属于原来角色身体的一部分，而且如果需要替换的部位有多个配件拥有独立的网格和贴图，那这种方式都可以正常执行。我下面的代码就是直接替换了换装部位的GameObject）
        /// 2.合并所有蒙皮网格
        /// 3.刷新骨骼
        /// 4.附加材质（我下面是获取第一个材质作为默认材质）
        /// 5.合并贴图（贴图的宽高最好是2的N次方的值）
        /// 6.重新计算UV
        /// </summary>
        /// <param name="skeleton"></param>
        /// <param name="rootBone">根骨骼节点</param>
        /// <param name="meshes"></param>
        /// <param name="transforms">骨骼节点Map</param>
        /// 
        public static void SimpleCombineObject( GameObject skeleton, Transform rootBone, SkinnedMeshRenderer[] meshes,
            List<Transform> transforms )
        {
            // Fetch all bones of the skeleton
            // transforms是骨架上所有的transform组件的List
            //List<Transform> transforms = new List<Transform>();
            //transforms.AddRange( skeleton.GetComponentsInChildren<Transform>() );

            //the list of bones
            var bones = new List<Transform>();


            // Collect information from meshes
            // 这里分别把所有Material，Mesh，Transform(骨头)保存到对应的List
            for (var i = 0; i < meshes.Length; i++)
            {
                if (meshes[i].gameObject.Equals(skeleton)) continue;
                var smr = meshes[i];
                bones.Clear();
                // Collect bones
                // 收集骨头有点区别：只收集骨架中有的。这应该会涉及到具体的美术标准。
                for (var j = 0; j < smr.bones.Length; j++)
                {
                    var tBase = 0;
                    for (tBase = 0; tBase < transforms.Count; tBase++)
                        if (smr.bones[j].name.Equals(transforms[tBase].name))
                        {
                            //Debug.Log("Equals bones " + smr.bones[j].name);
                            bones.Add(transforms[tBase]);
                            break;
                        }
                }

                smr.rootBone = rootBone;
                // Use new bones
                smr.bones = bones.ToArray();
            }
        }


        // Creates a character based on the currentConfiguration recycling a
        // character base, this way the position and animation of the character
        // are not changed.
        // 这个函数实际上并没有将各部分的子网格合并成一张网，而只是将他们合并到
        // 同一个Mesh下作为sub mesh。因为一张网格只能用一个材质，只有所有子网格
        // 都共享同一个材质时，合并成一张网才能保证材质应用正确。
        //public GameObject Generate( GameObject root )
        //{
        //    // The SkinnedMeshRenderers that will make up a character will be
        //    // combined into one SkinnedMeshRenderers using multiple materials.
        //    // This will speed up rendering the resulting character.
        //    List<CombineInstance> combineInstances = new List<CombineInstance>();
        //    List<Material> materials = new List<Material>();
        //    List<Transform> bones = new List<Transform>();

        //    //获得构成骨架的所有Transform
        //    Transform[] transforms = root.GetComponentsInChildren<Transform>();

        //    //一次处理构成身体的各部分
        //    foreach (CharacterElement element in currentConfiguration.Values)
        //    {
        //        //GetSkinnedMeshRenderer()内部Instantiat了一个由该部分肢体Assets构成的
        //        //GameObject，并返回Unity自动为其创建SinkedMeshRender。
        //        SkinnedMeshRenderer smr = element.GetSkinnedMeshRenderer();

        //        //注意smr.materials中包含的材质数量和顺序与下面的sub mesh是对应的
        //        materials.AddRange( smr.materials );
        //        for (int sub = 0; sub < smr.sharedMesh.subMeshCount; sub++)
        //        {
        //            CombineInstance ci = new CombineInstance();
        //            ci.mesh = smr.sharedMesh;
        //            ci.subMeshIndex = sub;
        //            combineInstances.Add( ci );
        //        }

        //        // As the SkinnedMeshRenders are stored in assetbundles that do not
        //        // contain their bones (those are stored in the characterbase assetbundles)
        //        // we need to collect references to the bones we are using
        //        // 网格点与骨骼的对应关系是通过Mesh数据结构中的BoneWeight数组来实现的。该数组
        //        // 与网格顶点数组对应，记录了每个网格点受骨骼（骨骼记录在SinkedMeshRender的bones
        //        // 数组中，按下标索引）影响的权重。
        //        // 而此处，示例程序提供的肢体Assets并不包含骨骼，而是返回骨骼名称。因此，推断
        //        // GetBoneNames()返回的骨骼名称应该与实际骨骼数组的顺序相同。
        //        foreach (string bone in element.GetBoneNames())
        //        {
        //            foreach (Transform transform in transforms)
        //            {
        //                //通过名字找到实际的骨骼
        //                if (transform.name != bone) continue;
        //                bones.Add( transform );
        //                break;
        //            }
        //        }

        //        Object.Destroy( smr.gameObject );
        //    }

        //    // Obtain and configure the SkinnedMeshRenderer attached to
        //    // the character base.
        //    // 至此，combineInstances、bones和materials三个数组中的数据对应关系是正确的。
        //    // 合并时，第二个参数是fals，表示保持子网格不变，只不过将它们统一到一个Mesh里
        //    // 来管理，这样只需采用一个SkinedMeshRender绘制，效率较高。
        //    SkinnedMeshRenderer r = root.GetComponent<SkinnedMeshRenderer>();
        //    r.sharedMesh = new Mesh();
        //    r.sharedMesh.CombineMeshes( combineInstances.ToArray(), false, false );
        //    r.bones = bones.ToArray();
        //    r.materials = materials.ToArray();

        //    return root;
        //}

        #endregion
    }

    public class EquipSlotException : Exception
    {
        public EquipSlotException( string msg )
            : base(msg)
        {
        }
    }
}