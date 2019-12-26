using Framework;
using Framework.SceneObject;
using SsitEngine.Core.ObjectPool;
using SsitEngine.QuestManager;
using UnityEngine;

public class QuestIndicatorHUD : ObjectBase
{
    private BaseSceneInstance mOwner;

    private GameObject TargetObj
    {
        get
        {
            return m_target as GameObject;
        }
    }

    public QuestIndicatorHUD( object target ) : base(target)
    {
        if (TargetObj!=null)
        {
            TargetObj.transform.SetParent(QuestManager.Instance.transform);
        }
    }

    protected override void OnSpawn()
    {
        TargetObj.SetActive(true);
        TargetObj.transform.localScale = Vector3.one;
    }

    protected override void OnUnspawn()
    {
        TargetObj.SetActive(false);
    }

    protected override void Release( bool isShutdown )
    {
        mOwner = null;
    }

    public override void Shutdown()
    {
        base.Shutdown();
        if (TargetObj != null)
        {
            Object.Destroy(TargetObj);
        }
    }

    public BaseSceneInstance Owner
    {
        get
        {
            return mOwner;
        }

        set
        {
            if (!value)
            {
                return;
            }
            mOwner = value;
            //mOwner.IndicatorHud = this;
            
            switch (value.Type)
            {
                case EnObjectType.Fire:
                    break;
                case EnObjectType.Gas:
                    break;
                case EnObjectType.Headquarters:
                    break;
                case EnObjectType.Obstacle:
                    break;
                case EnObjectType.Patient:
                    break;
                case EnObjectType.GamePlayer:
                    break;
                case EnObjectType.XFP:
                case EnObjectType.Annihilator:
                case EnObjectType.WaterPipe:
                case EnObjectType.PumpSwitch:
                    TargetObj.transform.position = value.transform.position + Vector3.up * 1.5f;
                    break;
                case EnObjectType.SirenHand:

                case EnObjectType.Valve:
                    break;
                case EnObjectType.Vehicle:
                    break;
                case EnObjectType.Trigger:
                case EnObjectType.Tag:
                    break;
            }
        }
    }


}
