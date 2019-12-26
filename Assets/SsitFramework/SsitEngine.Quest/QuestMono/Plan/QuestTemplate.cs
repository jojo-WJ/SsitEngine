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
        [SerializeField] private List<Step> steps; // Steps taken.

        public string version = "1.1.0";

        public QuestTemplate()
        {
            steps = new List<Step>();
        }

        public QuestTemplate( QuestTemplate plan, Step step )
        {
            steps = new List<Step>();
            if (plan != null)
            {
                steps.AddRange(plan.steps);
            }
            if (step != null)
            {
                steps.Add(step);
            }
        }

        public List<Step> Steps
        {
            get => steps;

            set => steps = value;
        }

        public void AddStep( Step step )
        {
            if (steps == null)
            {
                steps = new List<Step>();
            }
            steps.Add(step);
        }

        public int GetAllScore()
        {
            return steps.Sum(step => step.scoreValue);
        }

        public override string ToString()
        {
            var s = string.Empty;
            for (var i = 0; i < steps.Count; i++)
            {
                s += steps[i] + ", ";
            }
            s += ">";
            return s;
        }
    }
}