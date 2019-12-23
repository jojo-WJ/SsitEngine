using UnityEngine;

namespace SsitEngine.QuestManager
{
    /// <summary>
    /// Heading text UI content.
    /// </summary>
    public class HeadingTextQuestContent : QuestContent
    {
        private static readonly string UnassignedQuestNameField = "Quest";

        [Tooltip("Heading level (1=main heading, 2=subheading, etc.)")] [Range(1, 5)] [SerializeField]
        private int m_headingLevel = 1;

        [Tooltip("Text to show in heading text style.")] [SerializeField]
        private string m_headingText;

        [Tooltip("Use the quest's Title for the heading text.")] [SerializeField]
        private bool m_useQuestTitle;

        public bool useQuestTitle
        {
            get => m_useQuestTitle;
            set => m_useQuestTitle = value;
        }

        public int headingLevel
        {
            get => m_headingLevel;
            set => m_headingLevel = value;
        }

        /// <summary>
        /// Text to show in heading text style.
        /// </summary>
        public string headingText
        {
            get => m_useQuestTitle ? quest != null ? quest.Title : UnassignedQuestNameField : m_headingText;
            set => m_headingText = value;
        }

        public override string OriginalText
        {
            get => headingText;
            set => headingText = value;
        }

        public override string GetEditorName()
        {
            return useQuestTitle ? "Heading: <Quest Title>" : "Heading: " + headingText;
        }
    }
}