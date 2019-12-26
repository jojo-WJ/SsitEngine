using UnityEngine.UI;
#if UNITY_EDITOR
using UnityEngine;

namespace U3DExtends
{
    [RequireComponent(typeof(RectTransform))]
    [ExecuteInEditMode]
    public class Decorate : MonoBehaviour
    {
        [SerializeField] [HideInInspector] private Image _image;

        private Vector3 _lastPos = new Vector3(-1, -1);
        private Vector2 _lastSize = Vector2.zero;
        private string spr_path = "";

        public string SprPath
        {
            get => spr_path;
            set => LoadSpr(value);
        }

        public void LoadSpr( string path )
        {
            InitComponent();
            if (spr_path != path)
            {
                spr_path = path;
                _image.sprite = UIEditorHelper.LoadSpriteInLocal(path);
                _image.SetNativeSize();
                gameObject.name = CommonHelper.GetFileNameByPath(path);
                //Debug.Log("_image.sprite :" + (_image.sprite != null).ToString());
            }
        }

        protected void Start()
        {
            InitComponent();
        }

        protected void InitComponent()
        {
            if (_image == null)
                _image = GetComponent<Image>();
        }

        public bool IsChangedTrans()
        {
            var curTrans = transform as RectTransform;
            if (curTrans.localPosition == _lastPos && curTrans.sizeDelta == _lastSize)
                return false;
            return true;
        }

        public void SaveTrans()
        {
            var rectTrans = transform as RectTransform;
            _lastPos = rectTrans.localPosition;
            _lastSize = rectTrans.sizeDelta;
        }
    }
}
#endif