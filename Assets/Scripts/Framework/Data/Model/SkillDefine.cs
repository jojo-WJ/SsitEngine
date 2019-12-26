/*
*┌──────────────────────────────────────────────────────────────┐
*│　描   述：                                                    
*│　作   者：xuXin                                              
*│　版   本：1.0.0                                                 
*│　创建时间：2019/5/9 9:35:06                     
*└──────────────────────────────────────────────────────────────┘
*/

using Framework.Data;
using SsitEngine.Data;

namespace Table
{
    public partial class SkillDefine : DataBase
    {
        public override int Id => SkillID;

        public override T Create<T>( int dataId )
        {
            var skill = new SkillAttribute(SkillID);
            skill.name = SkillName;
            skill.iconId = SkillIconPath;
            skill.introduce = SkillDesc;
            skill.SkillType = SkillType;
            skill.SkillMsgType = SkillMsgType;
            skill.SkillTriggerType = SkillTriggerType;
            skill.SkillConditionType = SkillConditionType;
            skill.SkillConditionParam = SkillConditionParam;
            skill.EventType = (En_SkillEventType) EventType;
            skill.EventParam = EventParam;
            skill.ExtraMsgName = ExtraMsgName;
            skill.ExtraMsgParm = ExtraMsgParm;
            skill.DefaultState = SkillState;
            return skill as T;
        }

        public override void Apply( object obj )
        {
        }
    }
}