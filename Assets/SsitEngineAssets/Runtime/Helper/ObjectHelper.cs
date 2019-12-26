using Framework.Data;
using Framework.SceneObject;
using Framework.SsitInput;
using SsitEngine.PureMVC.Patterns;
using UnityEngine;

namespace Framework.Helper
{
    /// <summary>
    /// 场景物体操作辅助类
    /// </summary>
    public partial class ObjectHelper
    {
        /// <summary>
        /// 删除场景物体
        /// </summary>
        /// <param name="sceneObject">要删除的物体</param>
        /// <param name="closeForm">要关闭的窗口</param>
        public static void DeleteSceneObject( BaseSceneInstance sceneObject )
        {
            if (sceneObject == null)
                return;
            var guid = sceneObject.Guid;
            DeleteSceneObjects(guids:guid);
        }

        /// <summary>
        /// 删除场景物体
        /// </summary>
        /// <param name="guids">要删除的物体列表</param>
        public static void DeleteSceneObjects( params string[] guids )
        {
            if (guids == null)
                return;
            for (int i = 0; i < guids.Length; i++)
            {
                DeleteSceneObject(guids[i]);
            }
            Facade.Instance.SendNotification((ushort) EnInputEvent.FinishDeleteObjects, guids);
        }

        public static void DeleteSceneObject( string guid )
        {
            if (string.IsNullOrEmpty(guid))
                return;

            //hack:根据应用层平台启用
            var temp = InputManager.Instance.currentSelectedInstance;

            /*if (temp && temp.Guid == guid)
            {
                Facade.Instance.SendNotification((ushort) UIOperationFormEvent.SelectedObject);
            }
            
            if (!GlobalManager.Instance.IsSync)
            {
                SsitApplication.Instance.DestoryObject(guid);
                Facade.Instance.SendNotification((ushort) EnInputEvent.FinishDeleteObject, guid);
            }
            else
            {
                var request = new CSDestorySceneObjectRequest {guid = guid, netId = 0};
                //Facade.Instance.SendNotification((ushort)EnMirrorEvent.SendMessage, new MessagePackage(ConstMessageID.CSDestorySceneObjectRequest, request), true);
            }*/
        }

        public static bool RaycastToVirtualEffect( Transform target, Vector3 lookAt, float attackRange,
            out RaycastHit hit )
        {
            // ignore self just to be sure
            Debug.DrawLine(target.position, target.position + lookAt * attackRange, Color.yellow, 1);
            if (Physics.Raycast(target.position, lookAt, out hit, attackRange, 1 << LayerUtils.VisualEffect))
            {
                // don't hit anything between
                // show ray for debugging
                Debug.DrawLine(target.position, hit.point, Color.red, 1);
                Debug.DrawLine(hit.point, hit.point + hit.normal, Color.blue, 1);
                return true;
            }

            hit = new RaycastHit();
            return false;
        }

        public static bool RaycastToVirtualEffect( Transform target, Ray ray, float attackRange, out RaycastHit hit )
        {
            Debug.DrawLine(ray.origin, ray.origin + ray.direction * attackRange, Color.yellow, 1);

            // ignore self just to be sure
            if (Physics.Raycast(ray, out hit, attackRange, 1 << LayerUtils.VisualEffect))
            {
                // don't hit anything between
                // show ray for debugging
                Debug.DrawLine(target.position, hit.point, Color.red, 1);
                Debug.DrawLine(hit.point, hit.point + hit.normal, Color.blue, 1);
                return true;
            }

            hit = new RaycastHit();
            return false;
        }
        
        public static string GetPlayerName(BaseObject obj)
        {
            string groupName = string.Empty;
            string showName = string.Empty;

            /*RoomProxy roomProxy = Facade.Instance.RetrieveProxy(RoomProxy.NAME) as RoomProxy;
            if (roomProxy != null)
            {
                GroupInfo curGroupInfo = roomProxy.GetGroupByID(obj.GetAttribute().GroupId);
                if (curGroupInfo == null)
                {
                    groupName = "外来人员";
                }
                else
                {
                    groupName = curGroupInfo.GroupName;
                }
            }*/

            var attri = obj.GetAttribute();
            if (attri is PlayerAttribute player)
            {
                if (!string.IsNullOrEmpty(player.Profession))
                    showName = string.Format("[{0}]{1}", player.Profession, player.Name);
                else
                    showName = player.Name;
            }
            else if (attri is VehicleAttribute uintInfo)
            {
                if (!string.IsNullOrEmpty(uintInfo.profession))
                    showName = string.Format("[{0}]{1}", uintInfo.profession, uintInfo.Name);
                else
                    showName = uintInfo.Name;
            }

            if (!string.IsNullOrEmpty(groupName))
            {
                return string.Format("<color=#FF7F00>{0}</color>\n<color=#E066FF>{1}</color>", groupName, showName);
            }
            return string.Format("<color=#12ff00>[{0}]{1}</color>", groupName, showName);
        }

    }
}