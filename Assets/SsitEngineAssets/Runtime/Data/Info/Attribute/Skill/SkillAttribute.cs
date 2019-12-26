using Framework.SceneObject;
using Table;

namespace Framework.Data
{
    /// <summary>
    /// 技能数据
    /// </summary>
    public class SkillAttribute : InfoData
    {
        public string guid;//技能的guid
        public int skillId;//技能的id
        public string name;//技能的名称
        public string iconId;//技能图标路径
        public string introduce;//技能介绍

        public int SkillType = 1;
        public int SkillMsgType = 0;
        public int SkillTriggerType = 0;
        public int SkillConditionType = 0;
        public int SkillConditionParam;
        public En_SkillEventType EventType;
        public string EventParam;
        public string ExtraMsgName;
        public string ExtraMsgParm;
        public bool IsOn = false;

        public int DefaultState;
        public Skill mOwner;

        public SkillAttribute(int skillId)
        {
            this.skillId = skillId;
        }

        /*public void Apply(MeasureSkillInfo skillInfo)
        {
            if (skillInfo != null)
            {
                this.guid = skillInfo.guid;
                this.skillId = skillInfo.skillId;
                this.name = skillInfo.name;
                if (!string.IsNullOrEmpty(skillInfo.iconId))
                {
                    this.iconId = skillInfo.iconId;
                }
                if (!string.IsNullOrEmpty(skillInfo.introduce))
                {
                    this.introduce = skillInfo.introduce;
                }
            }
        }*/

        public void SetOwner(Skill owner)
        {
            mOwner = owner;
        }
        public Skill GetOwner()
        {
            return mOwner;
        }

        public void ResetState()
        {
            mOwner = null;
            IsOn = false;

        }
    }
}
