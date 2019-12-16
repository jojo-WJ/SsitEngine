/*
*┌──────────────────────────────────────────────────────────────┐
*│　描   述：                                                    
*│　作   者：xuXin                                              
*│　版   本：1.0.0                                                 
*│　创建时间：2019/4/11 15:10:36                     
*└──────────────────────────────────────────────────────────────┘
*/
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace SsitEngine.QuestManager
{
    /// <summary>
    /// 任务模板
    /// </summary>
    [Serializable]
    public class QuestTemplate
    {
        public string version = "1.1.0";
        
        [SerializeField]
        private List<Step> steps; // Steps taken.

        public QuestTemplate()
        {
            steps = new List<Step>();
        }

        public QuestTemplate( QuestTemplate plan, Step step )
        {
            this.steps = new List<Step>();
            if (plan != null)
                this.steps.AddRange(plan.steps);
            if (step != null)
                steps.Add(step);
        }

        public List<Step> Steps
        {
            get
            {
                return steps;
            }

            set
            {
                steps = value;
            }
        }

        public void AddStep( Step step )
        {
            if (this.steps == null)
            {
                this.steps = new List<Step>();
            }
            this.steps.Add(step);
        }

        public int GetAllScore()
        {
            return steps.Sum(step => step.scoreValue);
        }

        public override string ToString()
        {
            var s = string.Empty;
            for (int i = 0; i < steps.Count; i++)
            {
                s += steps[i] + ", ";
            }
            s += ">";
            return s;
        }
    }
}