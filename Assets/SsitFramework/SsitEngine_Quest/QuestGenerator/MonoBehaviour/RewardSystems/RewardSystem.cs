using UnityEngine;

namespace SsitEngine.QuestManager
{

    public abstract class RewardSystem
    {
        /// <summary>
        /// 确定性奖励
        /// </summary>
        /// <param name="points"></param>
        /// <param name="quest"></param>
        /// <returns></returns>
        public abstract int DetermineReward( int points, Quest quest );


        public abstract int DetermineReward(int points, QuestNode questNode);
    }
}
