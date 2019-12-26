using Framework.Logic;
using SsitEngine.Unity.Msic;
using UnityEngine;
using UnityEngine.UI;

namespace Framework.Tip
{
    public class TagTip : SsitMonoBase
    {
        public bool isInUse;
        private Canvas mCanvas;

        [AddBindPath("LogoDetailInput")] [SerializeField]
        private Text mLogoDetailInput;

        [AddBindPath("LogoInput")] [SerializeField]
        private Text mLogoInput;

        [AddBindPath("LogoNameInput")] [SerializeField]
        private Text mLogoNameInput;

        [AddBindPath("TipContent")] [SerializeField]
        private Text mTipContent;

        private Vector3 offset;
        public GameObject root;

        /// <summary>
        /// get mTarget's Host
        /// </summary>
        public Vector3 Host
        {
            get
            {
                var hostRoot = root.transform.Find("HeadHost");
                return hostRoot ? hostRoot.position + offset : root.transform.position + offset;
            }
        }

        private void Awake()
        {
            mCanvas = GameObject.FindWithTag(ConstValue.c_sUICanvas).GetComponent<Canvas>();

            //mLogoInput = gameObject.GetChildNodeComponentScripts<Text>("LogoInput/Text");
            //mLogoNameInput = gameObject.GetChildNodeComponentScripts<Text>("NameInput1/Text");
            //mLogoDetailInput = gameObject.GetChildNodeComponentScripts<Text>("DetailInput/Text");
        }

        public void SetTagInfo( GameObject owner )
        {
            root = owner;
            if (root == null)
            {
                isInUse = false;
                gameObject.SetActive(false);
            }
        }

        public void SetTagInfo( GameObject owner, string name, string logo, string detail, Vector3 offset )
        {
            isInUse = true;
            this.offset = offset;
            mLogoNameInput.text = name;
            mLogoDetailInput.text = detail;
            mLogoInput.text = logo;

            mLogoInput.gameObject.SetActive(true);
            mLogoDetailInput.gameObject.SetActive(true);
            mLogoNameInput.gameObject.SetActive(true);

            mTipContent.gameObject.SetActive(false);

            root = owner;
            gameObject.SetActive(true);
            OnShow();
        }

        public void SetTipInfo( GameObject owner, string tip, Vector3 offset )
        {
            isInUse = true;
            mLogoInput.gameObject.SetActive(false);
            mLogoDetailInput.gameObject.SetActive(false);
            mLogoNameInput.gameObject.SetActive(false);

            mTipContent.text = tip;
            mTipContent.gameObject.SetActive(true);
            root = owner;
            gameObject.SetActive(true);
            OnShow();
        }


        public bool IsInView( Vector3 worldPos )
        {
            var camTransform = Camera.main.transform;
            Vector2 viewPos = Camera.main.WorldToViewportPoint(worldPos);
            var dir = (worldPos - camTransform.position).normalized;
            var dot = Vector3.Dot(camTransform.forward, dir); //判断物体是否在相机前面

            if (dot > 0 && viewPos.x >= 0 && viewPos.x <= 1 && viewPos.y >= 0 && viewPos.y <= 1)
                return true;
            return false;
        }

        public void OnShow()
        {
            var targetPos = Vector2.zero;
            if (WorldToUI(Host, out targetPos)) transform.localPosition = targetPos;
            ////RectTransformUtility.ScreenPointToLocalPointInRectangle(mCanvas.transform as RectTransform,
            ////    Camera.main.WorldToScreenPoint(pos, mCanvas.worldCamera, out targetPos));

            //RectTransformUtility.ScreenPointToLocalPointInRectangle(mCanvas.transform as RectTransform, Camera.main.WorldToScreenPoint(pos, mCanvas.worldCamera.stereoActiveEye);
            //Vector2 world2ScreenPos = Camera.main.WorldToScreenPoint(pos);
            //this.transform.position = world2ScreenPos;
            //CanvasScaler canvasScaler = mCanvas.GetComponent<CanvasScaler>();
            //this.transform.localScale *= canvasScaler.scaleFactor;
        }

        public bool WorldToUI( Vector3 world, out Vector2 pos )
        {
            var in_main_vp = Camera.main.WorldToViewportPoint(world);
            if (in_main_vp.z < 0)
            {
                pos = Vector3.zero;
                return false;
            }

            in_main_vp.x -= 0.5f;
            in_main_vp.y -= 0.5f;

            var in_screen = new Vector3(mCanvas.worldCamera.pixelWidth * in_main_vp.x,
                mCanvas.worldCamera.pixelHeight * in_main_vp.y, 0);
            pos = in_screen;

            //CanvasScaler cs = mCanvas.GetComponent<CanvasScaler>();
            //pos.x /= (Screen.width / cs.referenceResolution.x);
            //pos.y /= (Screen.height / cs.referenceResolution.y);

            pos.x /= mCanvas.scaleFactor;
            pos.y /= mCanvas.scaleFactor;

            return true;
        }
    }
}