using TMPro;

namespace Framework.SceneObject
{
    /// <summary>
    /// 物体的3D标签
    /// </summary>
    public class SceneTipInstance : BaseSceneInstance
    {
        private TextMeshPro m_Detail;


        private TextMeshPro m_Logo;
        private TextMeshPro m_Name;
        protected EnObjectType m_Type = EnObjectType.Tag;

        public override EnObjectType Type => m_Type;

        private void Awake()
        {
            m_Logo = transform.Find<TextMeshPro>("Logo");
            m_Name = transform.Find<TextMeshPro>("Name");
            m_Detail = transform.Find<TextMeshPro>("Pos");
        }

        public void SetValue( string logo, string name, string detail )
        {
            SetLogo(logo);
            SetName(name);
            SetDetail(detail);
        }


        public void SetLogo( string logo )
        {
            if (!string.IsNullOrEmpty(logo)) m_Logo.SetText(logo);
        }

        public void SetName( string name )
        {
            if (!string.IsNullOrEmpty(name))
                m_Name.SetText(name);
        }

        public void SetDetail( string detail )
        {
            if (!string.IsNullOrEmpty(detail))
                m_Detail.SetText(detail);
        }
    }
}