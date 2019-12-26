using System.Collections.Generic;
using System.Linq;
using Framework;
using Framework.SceneObject;
using SsitEngine.DebugLog;
using SsitEngine.Unity.SceneObject;
using UnityEngine;

namespace SsitEngine.Unity.Avatar
{
    /// <summary>
    /// 维护Attach Points，可以是骨骼节点、骨骼节点子节点、装备的特殊节点
    /// </summary>
    public class EquipController : MonoBehaviour
    {
        // 骨骼节点、
        private readonly List<Transform> mAvararTras = new List<Transform>();

        public Character mCharacter;

        // 装备巢
        private Dictionary<InvSlot, InvAttachmentPoint> mEquipAttachMaps;

        // 骨骼跟节点、
        private Transform mRootBone;

        // 跟节点

        // 使用巢
        private Dictionary<InvUseSlot, Dictionary<InvUseNodeType, InvAttachmentPoint>> mUseAttachMaps;


        /*
        * 装备克隆
        */
        public void Clone( Item[] equips )
        {
            //clear all equip
            var enumrator = mEquipAttachMaps.GetEnumerator();
            while (enumrator.MoveNext())
            {
                var tt = enumrator.Current;
                tt.Value.DirectDetach();
            }
            enumrator.Dispose();

            for (var i = 0; i < equips.Length; i++)
            {
                var equip = equips[i];
                if (equip != null)
                {
                    var slot = equip.GetAttribute().Slot;
                    OnIntervalEquipClone(slot, null, equip, Vector3.zero);
                    InternalAvatar(slot, null, equip, null, equip.GetAttribute());
                }
            }

            void OnIntervalEquipClone( InvSlot slot, Item pre, Item cur, Vector3 offset )
            {
                // Get the slot's attachment points
                InvAttachmentPoint equipPoint = null;

                if (mEquipAttachMaps.ContainsKey(slot))
                    equipPoint = mEquipAttachMaps[slot];
                else
                    throw new EquipSlotException(string.Format("{0}要装备的装备巢不存在", slot));
                // detach pre equip
                if (pre != null) SsitApplication.Instance.DestoryItem(pre.Guid);

                // attach this equip
                if (cur != null) cur.AttachClone(equipPoint, offset);
            }
        }


        #region New Version

        [Disable] public bool isInit;

        public GameObject _Skeleton { get; private set; }

        public void Init( Character Character )
        {
            mCharacter = Character;
            InitAttachPoint();
        }

        public void InitAttachPoint()
        {
            if (isInit)
                return;

            isInit = true;
            // bind entity
            _Skeleton = transform.Find("Entity").gameObject;
            if (_Skeleton == null)
            {
                Debug.LogError("npc's Entity is null");
                return;
            }
            // bind avatarTras
            mRootBone = _Skeleton.transform.Find("Bip001");
            if (mRootBone == null)
            {
                Debug.LogError("npc's AvatarRoot is null");
                return;
            }
            mAvararTras.AddRange(mRootBone.GetComponentsInChildren<Transform>(true));

            // init Dictionary
            mEquipAttachMaps = new Dictionary<InvSlot, InvAttachmentPoint>();
            mUseAttachMaps = new Dictionary<InvUseSlot, Dictionary<InvUseNodeType, InvAttachmentPoint>>();

            //Search points in childs
            var mAttachments = transform.Find("Entity").GetComponentsInChildren<InvAttachmentPoint>();

            // Equip the item visually
            for (int i = 0, imax = mAttachments.Length; i < imax; ++i)
            {
                var ip = mAttachments[i];
                if (ip.useNodeType == InvUseNodeType.InvNone)
                {
                    if (mEquipAttachMaps.ContainsKey(ip.slot))
                        throw new EquipSlotException($"{ip.slot}要装备的装备巢重复 {ip.name}");
                    mEquipAttachMaps.Add(ip.slot, ip);
                }
                else
                {
                    if (mUseAttachMaps.ContainsKey(ip.useSlot))
                    {
                        mUseAttachMaps[ip.useSlot].Add(ip.useNodeType, ip);
                    }
                    else
                    {
                        mUseAttachMaps.Add(ip.useSlot, new Dictionary<InvUseNodeType, InvAttachmentPoint>());
                        mUseAttachMaps[ip.useSlot].Add(ip.useNodeType, ip);
                    }
                }
            }
        }

        public void DestroyEquip( Item item )
        {
            SsitApplication.Instance.DestoryItem(item.Guid);
        }

        /*
        * 装备挂接
        */

        internal void Equip( InvSlot slot, Item pre, Item item, Vector3 offset )
        {
            Replace(slot, pre, item, offset);
        }

        private void Replace( InvSlot slot, Item pre, Item item, Vector3 offset )
        {
            var preAttr = pre?.GetAttribute();
            var curAttr = item?.GetAttribute();

            OnIntervalEquip(slot, pre, item, offset);
            InternalAvatar(slot, pre, item, preAttr, curAttr);
        }

        private void OnIntervalEquip( InvSlot slot, Item pre, Item cur, Vector3 offset )
        {
            // Get the slot's attachment points
            InvAttachmentPoint equipPoint = null;

            if (mEquipAttachMaps.ContainsKey(slot))
                equipPoint = mEquipAttachMaps[slot];
            else
                throw new EquipSlotException(string.Format("{0}要装备的装备巢不存在", slot));
            // detach pre equip
            if (pre != null && pre.Guid != cur.Guid)
            {
                if (pre.Guid == cur.Guid)
                {
                }
                else
                {
                    SsitApplication.Instance.DestoryItem(pre.Guid);
                }
            }

            // attach this equip
            if (cur != null) cur.Attach(equipPoint, offset);
        }

        private void InternalAvatar( InvSlot slot, Item preItem, Item curItem, ItemAtrribute pre, ItemAtrribute cur )
        {
            // TODO:check 材质切换

            // Check this item can need combine mesh
            switch ((InvCombineType) cur.CombineIndex)
            {
                case InvCombineType.Inv_Combine:
                {
                    InternalCombineAllSkinnerMesh();
                    break;
                }
                case InvCombineType.Inv_SimpleCombine:
                {
                    if (pre != null)
                        switch ((InvCombineType) cur.CombineIndex)
                        {
                            case InvCombineType.Inv_Combine:
                            {
                                InternalCombineAllSkinnerMesh();
                                break;
                            }
                        }
                    InternalCombineSkinnerMesh(curItem, cur);
                    break;
                }
                case InvCombineType.Inv_SimpleBone:
                {
                    if (pre != null)
                        switch ((InvCombineType) cur.CombineIndex)
                        {
                            case InvCombineType.Inv_Combine:
                            {
                                InternalCombineAllSkinnerMesh();
                                break;
                            }
                            case InvCombineType.Inv_SimpleBone:
                            {
                                InternalCombineSkinnerMesh(curItem, cur);
                                break;
                            }
                        }
                    InternalSimpleBoneSkinnerMesh(curItem, cur);
                    break;
                }
                case InvCombineType.Inv_Normal:
                {
                    break;
                }
                case InvCombineType.Inv_ComMaterial:
                {
                    //hack:可以曲线实现，再次创建预设，修改预设材质，形成新的装备
                    break;
                }
            }
        }

        //[Obsolete("弃用版本")]
        private void InternalCombineAllSkinnerMesh()
        {
            var allCombineSkinnerMeshes = new List<SkinnedMeshRenderer>();
            var Items = mCharacter.GetEquipItem();
            var allCombineEquips = Items.ToList().FindAll(x =>
            {
                return x != null && x.GetAttribute().CombineIndex == (int) InvCombineType.Inv_Combine;
            });

            // temp attach item
            for (var i = 0; i < allCombineEquips.Count; i++)
            {
                var point = allCombineEquips[i].AttachPoint;
                if (point.mCurGameObject != null)
                {
                    var tempRenderer = point.mCurGameObject.GetComponentsInChildren<SkinnedMeshRenderer>(true);
                    allCombineSkinnerMeshes.AddRange(tempRenderer);
                }
            }

            if (allCombineSkinnerMeshes.Count > 0)
                AvatarHelper.CombineObject(_Skeleton, mRootBone, allCombineSkinnerMeshes.ToArray(), mAvararTras);
        }

        private void InternalCombineSkinnerMesh( Item item, ItemAtrribute preAtrribute )
        {
            var allCombineSkinnerMeshes = new List<SkinnedMeshRenderer>();
            var point = item.AttachPoint;
            if (point.mCurGameObject != null && preAtrribute.CombineIndex == (int) InvCombineType.Inv_SimpleCombine)
            {
                var tempRenderer = point.mCurGameObject.GetComponentsInChildren<SkinnedMeshRenderer>(true);
                allCombineSkinnerMeshes.AddRange(tempRenderer);
            }
            AvatarHelper.CombineObject(point.gameObject, mRootBone, allCombineSkinnerMeshes.ToArray(), mAvararTras);
        }

        private void InternalCombineSkinnerMesh( InvAttachmentPoint point, ItemAtrribute preAtrribute )
        {
            var allCombineSkinnerMeshes = new List<SkinnedMeshRenderer>();
            if (point.mCurGameObject != null && preAtrribute.CombineIndex == (int) InvCombineType.Inv_SimpleCombine)
            {
                var tempRenderer = point.mCurGameObject.GetComponentsInChildren<SkinnedMeshRenderer>(true);
                allCombineSkinnerMeshes.AddRange(tempRenderer);
            }
            AvatarHelper.CombineObject(point.gameObject, mRootBone, allCombineSkinnerMeshes.ToArray(), mAvararTras);
        }

        private void InternalSimpleBoneSkinnerMesh( Item item, ItemAtrribute preAtrribute )
        {
            var allCombineSkinnerMeshes = new List<SkinnedMeshRenderer>();
            var point = item.AttachPoint;
            if (point.mCurGameObject != null && preAtrribute.CombineIndex == (int) InvCombineType.Inv_SimpleBone)
            {
                var tempRenderer = point.mCurGameObject.GetComponentsInChildren<SkinnedMeshRenderer>(true);
                allCombineSkinnerMeshes.AddRange(tempRenderer);
            }
            AvatarHelper.SimpleCombineObject(point.gameObject, mRootBone, allCombineSkinnerMeshes.ToArray(),
                mAvararTras);
        }

        private void InternalSimpleBoneSkinnerMesh( InvAttachmentPoint point, ItemAtrribute preAtrribute )
        {
            var allCombineSkinnerMeshes = new List<SkinnedMeshRenderer>();
            if (point.mCurGameObject != null && preAtrribute.CombineIndex == (int) InvCombineType.Inv_SimpleBone)
            {
                var tempRenderer = point.mCurGameObject.GetComponentsInChildren<SkinnedMeshRenderer>(true);
                allCombineSkinnerMeshes.AddRange(tempRenderer);
            }
            AvatarHelper.SimpleCombineObject(point.gameObject, mRootBone, allCombineSkinnerMeshes.ToArray(),
                mAvararTras);
        }

        /*
        * 装备使用(仅限挂接中装备使用、融合的不在使用范围)
        */
        public void UseEquip( Item item )
        {
            // get useNode
            InvAttachmentPoint useNode = null;
            var attri = item.GetAttribute();
            if (attri == null)
                return;
            var useSlot = attri.UseSlot;
            var useNodeTypeType = attri.UseNodeType;
            if (mUseAttachMaps.ContainsKey(useSlot))
                if (mUseAttachMaps[useSlot].ContainsKey(useNodeTypeType))
                    useNode = mUseAttachMaps[useSlot][useNodeTypeType];

            // if node is not exist return
            if (useNode == null)
            {
                SsitDebug.Error("装备使用节点异常");
                return;
            }

            //todo: check node has item ,if has unUse it and use current

            //use
            item.Attach(useNode, Vector3.zero);
        }

        /// <summary>
        /// 卸载使用中的装备(仅限挂接中装备使用、融合的不在使用范围)
        /// </summary>
        /// <param name="item"></param>
        public void UnUseEquip( Item item )
        {
            // Get the slot's attachment points
            InvAttachmentPoint equipPoint = null;
            var attri = item.GetAttribute();
            if (attri == null)
                return;

            var slot = attri.Slot;

            if (mEquipAttachMaps.ContainsKey(slot))
                equipPoint = mEquipAttachMaps[slot];
            else
                throw new EquipSlotException($"{slot}要装备的装备巢不存在");

            //equip
            item.Attach(equipPoint, Vector3.zero);
        }

        /*
         * 交互对象挂接
         */

        public void Attach( InvUseSlot useSlot, InvUseNodeType useNodeTypeType, GameObject handler )
        {
            // get useNode
            InvAttachmentPoint useNode = null;
            if (mUseAttachMaps.ContainsKey(useSlot))
                if (mUseAttachMaps[useSlot].ContainsKey(useNodeTypeType))
                    useNode = mUseAttachMaps[useSlot][useNodeTypeType];

            // if node is not exist return
            if (useNode == null) return;

            // todo:check node has item ,if has unUse it and use current

            // attach item in useNode
            useNode.Attach(handler);
        }

        public void Dettach( InvUseSlot useSlot, InvUseNodeType useNodeTypeType, GameObject handler )
        {
            // get useNode
            InvAttachmentPoint useNode = null;
            if (mUseAttachMaps.ContainsKey(useSlot))
                if (mUseAttachMaps[useSlot].ContainsKey(useNodeTypeType))
                    useNode = mUseAttachMaps[useSlot][useNodeTypeType];

            // if node is not exist return
            if (useNode == null) return;

            // todo:check node has item ,if has unUse it and use current

            // attach item in useNode
            useNode.Detach();
        }

        public void Follow( InvUseSlot useSlot, InvUseNodeType useNodeTypeType, GameObject handler )
        {
            // get useNode
            InvAttachmentPoint useNode = null;
            if (mUseAttachMaps.ContainsKey(useSlot))
                if (mUseAttachMaps[useSlot].ContainsKey(useNodeTypeType))
                    useNode = mUseAttachMaps[useSlot][useNodeTypeType];

            // if node is not exist return
            if (useNode == null) return;

            // todo:check node has item ,if has unUse it and use current

            // attach item in useNode
            useNode.Follow(handler);
        }

        public void DeFollow( InvUseSlot useSlot, InvUseNodeType useNodeTypeType, GameObject handler )
        {
            // get useNode
            InvAttachmentPoint useNode = null;
            if (mUseAttachMaps.ContainsKey(useSlot))
                if (mUseAttachMaps[useSlot].ContainsKey(useNodeTypeType))
                    useNode = mUseAttachMaps[useSlot][useNodeTypeType];

            // if node is not exist return
            if (useNode == null) return;

            // todo:check node has item ,if has unUse it and use current

            // attach item in useNode
            useNode.DeFollow();
        }

        /*
        * 挂点获取
        */

        public InvAttachmentPoint GetEquipAttachPoint( InvSlot slotType )
        {
            InvAttachmentPoint ret = null;
            mEquipAttachMaps.TryGetValue(slotType, out ret);
            return ret;
        }

        public InvAttachmentPoint GetUseHandAttachPoint( InvUseNodeType nodeType )
        {
            InvAttachmentPoint ret = null;
            mUseAttachMaps[InvUseSlot.InvUseHand].TryGetValue(nodeType, out ret);
            return ret;
        }

        #endregion
    }
}