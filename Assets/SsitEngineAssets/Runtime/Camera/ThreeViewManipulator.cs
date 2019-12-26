using Framework;
using Framework.SsitInput;
using UnityEngine;

// 操作说明
// 1、左键拖拽旋转
// 2、右键拖拽平移
// 3、滚轮滚动缩放

// 开发进度
// 1、先只做俯视图

public class ThreeViewManipulator : MonoBehaviour, IInputState
{
    public static Vector3 homePositionOfOverview = new Vector3(40f, 200f, 120f); // 俯视图相机home位置

    private float current_rotation_V; //垂直旋转结果

    public float distanceToBasePlane;

    private bool m_bMouseOverUI;

    private readonly bool[] m_mouseButtonDown = {false, false};
    private readonly Vector3[] m_mouseButtonDownPosition = new Vector3[2];
    private readonly Vector3[] m_mouseButtonPositionDelta = new Vector3[2];
    private readonly Vector3[] m_mouseButtonPositionLast = new Vector3[2];
    public float max_down_angle = -90.0f; //越小，头抬得越低
    public float max_up_angle = 90.0f; //越大，头抬得越高

    public ENCameraModeType CameraMode => ENCameraModeType.ThreeViewManipulator;

    public void OnUpdate()
    {
        // 0 鼠标左键; 1 鼠标右键
        for (var i = 0; i < 2; ++i)
        {
            //if ( Input.GetMouseButtonDown( i ) || ( Input.touchCount > 0 && Input.GetTouch( 0 ).phase == TouchPhase.Began ) )
            {
#if IPHONE || ANDROID
                m_bMouseOverUI =
 UnityUtils.IsPointerOverUI( Input.GetTouch( 0 ).position, ConstValue.c_sIgnoreUI, ConstValue.c_sIgnoreUIMobileControls );
#else
                m_bMouseOverUI = InputHelper.IsPointerOverUI(Input.mousePosition);
#endif
            }

            if (Input.GetMouseButtonDown(i) && !m_bMouseOverUI)
            {
                m_mouseButtonDown[i] = true;
                m_mouseButtonDownPosition[i] = Input.mousePosition;
                m_mouseButtonPositionLast[i] = Input.mousePosition;
            }

            if (Input.GetMouseButtonUp(i))
            {
                m_mouseButtonDown[i] = false;
                m_mouseButtonDownPosition[i] = Vector2.zero;
                m_mouseButtonPositionLast[i] = Vector2.zero;
            }
        }
    }

    public void OnLateUpdate()
    {
        var mousePositionCurr = Input.mousePosition;

        if (m_mouseButtonDown[0])
        {
            m_mouseButtonPositionDelta[0] = Input.mousePosition - m_mouseButtonPositionLast[0];
            m_mouseButtonPositionLast[0] = Input.mousePosition;
        }
        else if (m_mouseButtonDown[1])
        {
            m_mouseButtonPositionDelta[1] = Input.mousePosition - m_mouseButtonPositionLast[1];
            m_mouseButtonPositionLast[1] = Input.mousePosition;
        }

        transform.Translate(
            -(transform.up * m_mouseButtonPositionDelta[0].y + transform.right * m_mouseButtonPositionDelta[0].x) *
            transform.position.y * 0.002f, Space.World);
        transform.localEulerAngles = new Vector3(-current_rotation_V,
            transform.localEulerAngles.y + m_mouseButtonPositionDelta[1].x * 0.1f, 0f);

        if (!m_bMouseOverUI)
            distanceToBasePlane +=
                -Input.GetAxis("Mouse ScrollWheel") * transform.position.y * 0.1f; // 缩放( 以 y=0 为基础平面 )
        //if ( transform.position.y + distanceToBasePlane > 0.0f && transform.position.y + distanceToBasePlane < 1000.0f )
        {
            transform.Translate(Vector3.up * distanceToBasePlane, Space.World); // 相对于世界坐标y轴向上
        }

        m_mouseButtonPositionDelta[0] *= 0.9f; // 移动递减
        m_mouseButtonPositionDelta[1] *= 0.9f; // 移动递减
        distanceToBasePlane = Mathf.Abs(distanceToBasePlane) < 0.01f ? 0.0f : distanceToBasePlane * 0.9f; // 缩放递减
    }


    public bool Enable()
    {
        return enabled;
    }

    public bool CouldEnter()
    {
        return true;
    }

    public bool CouldLeave()
    {
        return true;
    }

    public void Enter()
    {
        enabled = true;
    }

    public void Leave()
    {
        enabled = false;
    }

    private void Start()
    {
        current_rotation_V = -90.0f; // 俯视
        transform.Translate(Vector3.up * 200.0f, Space.World); // 相对于世界坐标y轴向上
    }

    public void OnFixedUpdate()
    {
    }
}