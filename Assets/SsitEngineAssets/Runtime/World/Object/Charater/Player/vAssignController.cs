using SsitEngine.Unity.HUD;
using UnityEngine;

public class vAssignController : MonoBehaviour
{
    public enum ENSimulatType
    {
        ENS_None,
        ENS_Patient,
        ENS_AssignWorker
    }

    [SerializeField] private Animator animator;

    private string animName = "Null";

    public Transform BLPoint;
    public Transform BRPoint;
    public bool isAddListenerInput = true;

    private bool isInit;

    //hud操作
    private HudElement mHud;

    [SerializeField] private ThirdPersonController mParentContorller;

    public ENSimulatType mSimulatType = ENSimulatType.ENS_Patient;


    public void SetDisplayAssign( bool value, int layer = 0, string animName = "Null" )
    {
        this.animName = animName;
        gameObject.SetActive(value);
        if (value && animator) animator.Play(animName, layer);
    }

    #region Main Method

    private void OnEnable()
    {
        Init();
        if (mParentContorller != null)
            //类型动画播放
            if (animator)
                switch (mSimulatType)
                {
                    case ENSimulatType.ENS_AssignWorker:
                        animator.Play("Stretcher", 3);
                        break;
                }
        if (mHud)
            mHud.IsActive = true;
    }

    private void OnDisable()
    {
        if (mHud)
        {
            mHud.IsActive = false;
            mHud.SetHUDActive(false);
        }
    }

    public void Update()
    {
        UpdateAnimator();
    }

    public void LateUpdate()
    {
        mHud?.UpdateHUDElement(mHud);
    }

    #endregion

    #region HUD

    public void InitHUD( string showName, string message )
    {
        if (mHud == null) mHud = transform.Find("HeadHost")?.GetComponent<HudElement>();

        if (mHud != null)
        {
            if (mHud.Hud == null)
            {
                mHud.AttachTo(NavigationElementType.HUD, true);
                ChangeHUDText(showName, message);
            }
            else
            {
                ChangeHUDText(showName, message);
            }
        }
    }

    public void RemoveHUD()
    {
        if (mHud != null) mHud.AttachTo(NavigationElementType.HUD, false);
    }

    public void ChangeHUDText( string showName, string message )
    {
        if (mHud != null)
            if (mHud.Hud != null)
            {
                mHud.Hud.ChangeNameText(showName);
                mHud.Hud.ChangeStateText(message);
            }
    }

    #endregion

    #region Animation Variable

    //int baseLayer { get { return animator.GetLayerIndex("Base Layer"); } }
    //int underBodyLayer { get { return animator.GetLayerIndex("UnderBody"); } }
    //int rightArmLayer { get { return animator.GetLayerIndex("RightArm"); } }
    //int leftArmLayer { get { return animator.GetLayerIndex("LeftArm"); } }
    //int upperBodyLayer { get { return animator.GetLayerIndex("UpperBody"); } }
    //int fullbodyLayer { get { return animator.GetLayerIndex("FullBody"); } }

    #endregion

    #region Animtion

    private void Init()
    {
        if (isInit)
            return;
        if (animator != null)
            animator = GetComponent<Animator>();

        if (animator != null)
            animator.updateMode = AnimatorUpdateMode.Normal;
        isInit = true;
    }

    private void UpdateAnimator()
    {
        if (mParentContorller == null || mParentContorller.animator == null)
            return;

        // get parentAnimation
        var InputHorizontal = mParentContorller.animator.GetFloat("InputHorizontal");
        var InputVertical = mParentContorller.animator.GetFloat("InputVertical");

        // strafe movement get the input 1 or -1
        animator.SetFloat("InputHorizontal", InputHorizontal);
        animator.SetFloat("InputVertical", InputVertical);
    }

    private void OnAnimatorIK( int layerIndex )
    {
        if (animator.GetCurrentAnimatorStateInfo(3).IsName("Stretcher"))
        {
            animator.SetIKPositionWeight(AvatarIKGoal.LeftHand, 1);
            animator.SetIKPosition(AvatarIKGoal.LeftHand, BLPoint.position);

            animator.SetIKPositionWeight(AvatarIKGoal.RightHand, 1);
            animator.SetIKPosition(AvatarIKGoal.RightHand, BRPoint.position);
            //ani.SetIKRotationWeight(AvatarIKGoal.RightHand, 1);
            //ani.SetIKRotation(AvatarIKGoal.RightHand, player.rotation);
        }
    }

    #endregion


    #region AssignTriggle

    //public TriggleType TriggleType
    //{
    //    get
    //    {
    //        return TriggleType.CharactorDis_Triggle;
    //    }
    //}
    //public bool Check(BaseSceneInstance sceneObj)
    //{
    //    if (!sceneObj || mPlayer == null || mPlayer.InfoData == null)
    //    {
    //        return false;
    //    }
    //    Framework.SceneObject.PlayerInstance target = sceneObj as Framework.SceneObject.PlayerInstance;
    //    RoomProxy roomProxy = Facade.Instance.RetrieveProxy(RoomProxy.NAME) as RoomProxy;
    //    if (roomProxy == null)
    //    {
    //        return false;
    //    }
    //    if (target && target.LinkObject.GetAttribute().GroupId == roomProxy.GetCurrentGroup().ID && ((PlayerAttribute)mPlayer.InfoData).mAssigned == target)
    //    {
    //        return true;
    //    }
    //    return false;
    //}

    //public void Enter(BaseSceneInstance sceneObj)
    //{
    //    PlayerAttribute playerAttribute = mPlayer.InfoData as PlayerAttribute;

    //    //切换状态
    //    ((PlayerAttribute)playerAttribute.mPostObj.InfoData).mProcessState = ENProcessState.EPS_Processing;
    //    playerAttribute.mProcessState = ENProcessState.EPS_Processing;
    //    ((PlayerAttribute)playerAttribute.mAssigned.InfoData).mProcessState = ENProcessState.EPS_ForceProcessing;
    //    //转移控制目标
    //    SceneStateProxy sceneStateProxy = Facade.Instance.RetrieveProxy(SceneStateProxy.NAME) as SceneStateProxy;
    //    if (sceneStateProxy != null)
    //    {
    //        sceneStateProxy.SetSelectedObject(mPlayer);
    //    }
    //    //((PlayerInstance)sceneObj).thirdPersonManipulator.StopMove();
    //    mPlayer.DoStrecherPatient();
    //    //发送UI改变通知
    //    Facade.Instance.SendNotification((ushort)ConstNotification.StrecherPatientFinished);

    //}

    //public void Stay(BaseSceneInstance sceneObj)
    //{

    //}

    //public void Exit(BaseSceneInstance sceneObj)
    //{

    //}

    //public BaseSceneInstance OnPostTriggle()
    //{
    //    return mPlayer;
    //}

    //public ObjectDispayInfo GetDisplayInfo()
    //{
    //    throw new System.NotImplementedException();
    //}

    #endregion
}