using UnityEngine;

namespace SsitEngine.QuestManager
{

    /// <summary>
    /// Body text UI content.
    /// </summary>
    public class BodyTextQuestContent : QuestContent
    {

        [Tooltip("Text to show in regular body text style.")]
        [SerializeField]
        private string m_bodyText;

        /// <summary>
        /// Text to show in regular body text style.
        /// </summary>
        public string BodyText
        {
            get { return m_bodyText; }
            set { m_bodyText = value; }
        }

        public override string OriginalText
        {
            get { return BodyText; }
            set { BodyText = value; }
       }

        public override string GetEditorName()
        {
            return (BodyText == null) ? "Body Text" : "Text: " + BodyText;
        }

    }

}
