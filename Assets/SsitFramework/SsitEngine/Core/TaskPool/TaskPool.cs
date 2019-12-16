/*
*┌──────────────────────────────────────────────────────────────┐
*│　描   述：抽象引用对象                                                    
*│　作   者：xuXin                                              
*│　版   本：1.0.0                                                 
*│　创建时间：2019/3/28 15:08:40              
*└──────────────────────────────────────────────────────────────┘
*/

using System.Collections.Generic;

namespace SsitEngine.Core.TaskPool
{
    /// <summary>
    ///     任务池。
    /// </summary>
    /// <typeparam name="T">任务类型。</typeparam>
    public sealed class TaskPool<T> where T : ITask
    {
        private readonly Stack<ITaskAgent<T>> m_FreeAgents;
        private readonly LinkedList<T> m_WaitingTasks;
        private readonly LinkedList<ITaskAgent<T>> m_WorkingAgents;

        /// <summary>
        ///     初始化任务池的新实例。
        /// </summary>
        public TaskPool()
        {
            m_FreeAgents = new Stack<ITaskAgent<T>>();
            m_WorkingAgents = new LinkedList<ITaskAgent<T>>();
            m_WaitingTasks = new LinkedList<T>();
        }

        /// <summary>
        ///     获取任务代理总数量。
        /// </summary>
        public int TotalAgentCount => FreeAgentCount + WorkingAgentCount;

        /// <summary>
        ///     获取可用任务代理数量。
        /// </summary>
        public int FreeAgentCount => m_FreeAgents.Count;

        /// <summary>
        ///     获取工作中任务代理数量。
        /// </summary>
        public int WorkingAgentCount => m_WorkingAgents.Count;

        /// <summary>
        ///     获取等待任务数量。
        /// </summary>
        public int WaitingTaskCount => m_WaitingTasks.Count;

        /// <summary>
        ///     任务池轮询。
        /// </summary>
        /// <param name="elapseSeconds">逻辑流逝时间，以秒为单位。</param>
        /// <param name="realElapseSeconds">真实流逝时间，以秒为单位。</param>
        public void Update( float elapseSeconds, float realElapseSeconds )
        {
            var current = m_WorkingAgents.First;
            while (current != null)
            {
                if (current.Value.Task.Done)
                {
                    var next = current.Next;
                    current.Value.Reset();
                    m_FreeAgents.Push(current.Value);
                    m_WorkingAgents.Remove(current);
                    current = next;
                    continue;
                }

                current.Value.Update(elapseSeconds, realElapseSeconds);
                current = current.Next;
            }

            while (FreeAgentCount > 0 && WaitingTaskCount > 0)
            {
                var agent = m_FreeAgents.Pop();
                var agentNode = m_WorkingAgents.AddLast(agent);
                var task = m_WaitingTasks.First.Value;
                m_WaitingTasks.RemoveFirst();
                agent.Start(task, realElapseSeconds);
                if (task.Done)
                {
                    agent.Reset();
                    m_FreeAgents.Push(agent);
                    m_WorkingAgents.Remove(agentNode);
                }
            }
        }

        /// <summary>
        ///     关闭并清理任务池。
        /// </summary>
        public void Shutdown()
        {
            while (FreeAgentCount > 0) m_FreeAgents.Pop().Shutdown();

            foreach (var workingAgent in m_WorkingAgents) workingAgent.Shutdown();
            m_WorkingAgents.Clear();

            m_WaitingTasks.Clear();
        }

        /// <summary>
        ///     增加任务代理。
        /// </summary>
        /// <param name="agent">要增加的任务代理。</param>
        public void AddAgent( ITaskAgent<T> agent )
        {
            if (agent == null) throw new SsitEngineException("Task agent is invalid.");

            agent.Initialize();
            m_FreeAgents.Push(agent);
        }

        /// <summary>
        ///     增加指定工作的任务代理。
        /// </summary>
        /// <param name="agent">要增加的任务代理。</param>
        public void AddWorkAgent( ITaskAgent<T> agent, T task, float realElapseSeconds )
        {
            if (agent == null) throw new SsitEngineException("Task agent is invalid.");

            agent.Initialize();
            agent.Start(task, realElapseSeconds);
            m_FreeAgents.Push(agent);
        }

        /// <summary>
        ///     增加任务。
        /// </summary>
        /// <param name="task">要增加的任务。</param>
        public void AddTask( T task )
        {
            var current = m_WaitingTasks.First;
            while (current != null)
            {
                if (task.Priority > current.Value.Priority) break;

                current = current.Next;
            }

            if (current != null)
                m_WaitingTasks.AddBefore(current, task);
            else
                m_WaitingTasks.AddLast(task);
        }

        /// <summary>
        ///     移除任务。
        /// </summary>
        /// <param name="serialId">要移除任务的序列编号。</param>
        /// <returns>被移除的任务。</returns>
        public T RemoveTask( ulong serialId )
        {
            foreach (var waitingTask in m_WaitingTasks)
                if (waitingTask.Id == serialId)
                {
                    m_WaitingTasks.Remove(waitingTask);
                    waitingTask.Shutdown();
                    return waitingTask;
                }

            foreach (var workingAgent in m_WorkingAgents)
                if (workingAgent.Task.Id == serialId)
                {
                    workingAgent.Reset();
                    m_FreeAgents.Push(workingAgent);
                    m_WorkingAgents.Remove(workingAgent);
                    return workingAgent.Task;
                }

            return default;
        }

        /// <summary>
        ///     移除任务。
        /// </summary>
        /// <param name="task">要移除任务。</param>
        /// <returns>是否移除任务。</returns>
        public bool RemoveTask( ITask task )
        {
            foreach (var waitingTask in m_WaitingTasks)
                if (waitingTask.Id == task.Id)
                {
                    m_WaitingTasks.Remove(waitingTask);
                    waitingTask.Shutdown();
                    return m_WaitingTasks.Remove(waitingTask);
                }

            foreach (var workingAgent in m_WorkingAgents)
                if (workingAgent.Task.Id == task.Id)
                {
                    workingAgent.Reset();
                    m_FreeAgents.Push(workingAgent);
                    m_WorkingAgents.Remove(workingAgent);
                    return true;
                }

            return false;
        }

        /// <summary>
        ///     移除所有任务。
        /// </summary>
        public void RemoveAllTasks()
        {
            m_WaitingTasks.Clear();
            foreach (var workingAgent in m_WorkingAgents)
            {
                workingAgent.Reset();
                m_FreeAgents.Push(workingAgent);
            }
            m_WorkingAgents.Clear();
        }
    }
}