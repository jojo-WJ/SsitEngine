using UnityEngine;

namespace SsitEngine.QuestManager
{
    /// <summary>
    /// This subclass of QuestList provides facilities to show the list in a QuestJournalUI.
    /// </summary>
    [DisallowMultipleComponent]
    public class QuestJournal : IdentifiableQuestListContainer
    {
        //[Tooltip( "The Quest Journal UI to use. If unassigned, use the QuestMachine's default UI." )]
        //[SerializeField]
        //private UnityEngine.Object m_questJournalUI;

        //[Tooltip( "The Quest HUD to use. If unassigned, use the QuestMachine's default HUD." )]
        //[SerializeField]
        //private UnityEngine.Object m_questHUD;

        [Tooltip("Keep completed quests in the journal.")] [SerializeField]
        private bool m_rememberCompletedQuests;


        /// <summary>
        /// Keep completed quests in the journal.
        /// </summary>
        public bool RememberCompletedQuests
        {
            get { return m_rememberCompletedQuests; }
            set { m_rememberCompletedQuests = value; }
        }

        public override void Reset()
        {
            base.Reset();
            includeInSavedGameData = true;
        }

        /// <summary>
        /// Toggle visibility of the journal.
        /// </summary>
        public virtual void ToggleJournalUI()
        {
            //if (QuestJournalUi != null)
            //    QuestJournalUi.Toggle( this );
        }

        public virtual void AbandonQuest( Quest quest )
        {
            //if (quest == null || !quest.IsAbandonable) return;
            //if (quest.RememberIfAbandoned)
            //{
            //    quest.SetState( QuestState.Abandoned );
            //}
            //else
            //{
            //    DeleteQuest( quest.Id );
            //}
            //QuestMachineMessages.QuestAbandoned( this, quest.Id );
            //if (QuestJournalUi != null)
            //    QuestJournalUi.SelectQuest( null );
            //RepaintUIs();
        }

        public virtual void RepaintUIs()
        {
            //if (QuestJournalUi != null) QuestJournalUi.Repaint( this );
            //if (QuestHud != null) QuestHud.Repaint( this );
        }
    }
}