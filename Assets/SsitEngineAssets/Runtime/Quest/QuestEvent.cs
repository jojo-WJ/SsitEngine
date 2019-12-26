/*
*┌──────────────────────────────────────────────────────────────┐
*│　描   述：                                                    
*│　作   者：xuXin                                              
*│　版   本：1.0.0                                                 
*│　创建时间：2019/8/5 10:35:36                     
*└──────────────────────────────────────────────────────────────┘
*/
using SsitEngine.QuestManager;

namespace Framework.Quest
{
    //任务相关消息定义
    public enum En_QuestsMsg
    {
        En_Interactive = EnQuestEvent.MaxValue,     //操作目标物体
        En_InteractiveActive,                       //激活操作目标物体
        En_InteractiveComplete,                     //操作目标物体完成

        En_WearEquip = En_Interactive + 10,         //穿戴个体防护
        En_WearEquipActive,

        En_ArrivedTo = En_WearEquip + 10,           //达到目的地
        En_ArrivedToActive,
        En_ArrivedToComplete,

        En_AdoptedMeasure = En_ArrivedTo + 10,                          //采用应急措施

        En_Technology = En_AdoptedMeasure + 10,                              //使用工艺规则
        En_TechnologyActive,
        //En_RangeEnterTrigger,                     //不知道啥玩意

        //【注意】只能往后加不能插入，插入就过分了，后果自负



        MaxValue,
    }

}
