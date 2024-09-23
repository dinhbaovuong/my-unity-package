using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

public abstract class WaitTokenBase<TWaitToken> : WaitTokenBase where TWaitToken : WaitTokenBase<TWaitToken>, new()
{
    protected List<Action<TWaitToken>> listOnFinish;
    protected Task<TWaitToken> waitTask;
    protected List<TWaitToken> listChild;
    protected TWaitToken parent;
    
    
    /// <summary>
    /// action sẽ được gọi sau khi Finish. Được gọi ngay lập tức nếu đã Finished
    /// </summary>
    public TWaitToken OnFinish(Action<TWaitToken> action)
    {
        lock (objectLock)
        {
            if (IsFinished == false)
            {
                if(listOnFinish == null)
                    listOnFinish = new List<Action<TWaitToken>>();
                
                listOnFinish.Add(action);
                return (TWaitToken)this;
            }
        }
        
        InvokeActionInMainThread(action);
        return (TWaitToken)this;
    }

    public TWaitToken OnFinishTimeOut(Action<TWaitToken> action, float timeOut, bool useUnscaledTime)
    {
        if (IsFinished)
        {
            InvokeActionInMainThread(action);
            return (TWaitToken)this;
        }

        if (Rx.IsMainThread)
            Rx.RunCoroutine(Timeline());
        else
            Rx.RunCoroutineAsync(Timeline());
        
        return (TWaitToken)this;
        
        
        
        

        IEnumerator Timeline()
        {
            float startTime = useUnscaledTime ? Time.unscaledTime : Time.time;
            while (true)
            {
                yield return null;
                
                //Check Finished
                if (IsFinished)
                {
                    InvokeActionInMainThread(action);
                    yield break;
                }
                
                //Check timeout
                float currentTime = useUnscaledTime ? Time.unscaledTime : Time.time;
                if (currentTime - startTime > timeOut)
                {
                    InvokeActionInMainThread(action);
                    yield break;
                }
            }
        }
    }

    public TWaitToken OnFinishTimeOutScaledTime(Action<TWaitToken> action, float timeOut)
    {
        return OnFinishTimeOut(action, timeOut, false);
    }
    
    public TWaitToken OnFinishTimeOutUnscaledTime(Action<TWaitToken> action, float timeOut)
    {
        return OnFinishTimeOut(action, timeOut, true);
    }
    
    public Task<TWaitToken> WaitTask()
    {
        lock (objectLock)
        {
            if (waitTask == null)
            {
                waitTask = new Task<TWaitToken>(() =>
                {
                    while (!IsFinished)
                    {
                        Thread.Sleep(16);
                    }
                    return (TWaitToken)this;
                });
                
                waitTask.Start();
            }
        }
        
        return waitTask;
    }
    
    
    
    #region WaitToken Child Handle
    
    public void AddChild(TWaitToken child)
    {
        if (child == this)
        {
            Debug.LogError("Cannot add self is child");
            return;
        }
        
        if(child.parent == this)
            return;

        if (child.parent != null)
        {
            Debug.LogError("This child already has parent");
            return;
        }

        lock (objectLock)
        {
            if(IsFinished == false) //Nếu chưa finish mới cần add chứ finish rồi thì khỏi cần
            {
                if (listChild == null)
                    listChild = new List<TWaitToken>();

                if (listChild.Contains(child))
                    return;

                listChild.Add(child);
                return;
            }
        }
        
        if(isCanceled)
            child.Cancel();
        else if(isFaulted)
            child.SetFault(errorCode, errorMessage, rawException);
        else
            SetResultFromThisToOther(child);
    }

    public TWaitToken CreateChild()
    {
        TWaitToken child = new TWaitToken();
        this.AddChild(child);
        return child;
    }

    protected abstract void SetResultFromThisToOther(TWaitToken other);

    protected override void CancelForListChild()
    {
        if (listChild == null)
            return;

        for (int i = 0; i < listChild.Count; i++)
        {
            listChild[i].Cancel();
        }
    }
    
    protected override void SetFaultForListChild(long? errorCode, string errorMessage, Exception rawException)
    {
        if (listChild == null)
            return;

        for (int i = 0; i < listChild.Count; i++)
        {
            listChild[i].SetFault(errorCode, errorMessage, rawException);
        }
    }

    #endregion
    
    #region WaitToken Other Handle

    public void SetParent(TWaitToken parent)
    {
        parent.AddChild((TWaitToken)this);
    }

    #endregion
    
    protected void TryInvokeAction(Action<TWaitToken> action)
    {
        try
        {
            action.Invoke((TWaitToken)this);
        }
        catch (Exception e)
        {
            Debug.LogException(e);
        }
    }
    
    protected override void InvokeListActionInMainThread()
    {
        if(listOnFinish == null)
            return;

        if (Rx.IsMainThread)
        {
            foreach (var action in listOnFinish)
            {
                TryInvokeAction(action);
            }
        }
        else
        {
            Rx.RunAction(() => 
            {
                foreach (var action in listOnFinish)
                {
                    TryInvokeAction(action);
                }
            });
        }
    }
    
    protected void InvokeActionInMainThread(Action<TWaitToken> onFinish)
    {
        if (Rx.IsMainThread)
            TryInvokeAction(onFinish);
        else
            Rx.RunAction(() => TryInvokeAction(onFinish));
    }
    
    
    
    public static TWaitToken CreateCanceled()
    {
        var waitToken = new TWaitToken();
        waitToken.Cancel();
        return waitToken;
    }
    
    public static TWaitToken CreateFaulted(long? errorCode, string errorMessage, Exception rawException)
    {
        var waitToken = new TWaitToken();
        waitToken.SetFault(errorCode, errorMessage, rawException);
        return waitToken;
    }

    public static TWaitToken CreateFaulted(Exception rawException)
    {
        var waitToken = new TWaitToken();
        waitToken.SetFault(rawException);
        return waitToken;
    }
    
    public static TWaitToken CreateFaulted(long? errorCode)
    {
        var waitToken = new TWaitToken();
        waitToken.SetFault(errorCode);
        return waitToken;
    }
    
    public static TWaitToken CreateFaulted(string errorMessage)
    {
        var waitToken = new TWaitToken();
        waitToken.SetFault(errorMessage);
        return waitToken;
    }
    
    public static TWaitToken CreateFaulted(long? errorCode, string errorMessage)
    {
        var waitToken = new TWaitToken();
        waitToken.SetFault(errorCode, errorMessage);
        return waitToken;
    }
}