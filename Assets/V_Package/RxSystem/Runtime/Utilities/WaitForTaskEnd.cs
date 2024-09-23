using System;
using System.Threading.Tasks;
using UnityEngine;

public class WaitForTaskEnd : CustomYieldInstruction
{
    private Task m_task;
    private Exception exception;

    public Task task => m_task;
    public Exception Exception => exception;

    public override bool keepWaiting
    {
        get
        {
            if (m_task.IsCompleted)
            {
                if (m_task.IsCanceled)
                    Debug.LogError("Task Canceled");

                if (m_task.IsFaulted)
                    exception = m_task.Exception;

                return false;
            }

            return true;
        }
    }

    public WaitForTaskEnd(Action actionToExecuteInAnotherThread)
    {
        m_task = new Task(actionToExecuteInAnotherThread);
        m_task.Start();
    }

    public WaitForTaskEnd(Task task, bool startTask = false)
    {
        m_task = task;
        if(startTask)
            task.Start();
    }
}