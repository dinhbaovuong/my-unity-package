using System;
using UnityEngine;

public class WaitToken<T> : WaitTokenBase<WaitToken<T>>
{
    private T result;
    private bool hasResult;
    
    
    
    #region Constructor

    /// <summary>
    /// Start wait now
    /// </summary>
    public WaitToken()
    {
    }

    /// <summary>
    /// Sometimes you initialize it but have not used it yet
    /// </summary>
    public WaitToken(bool startWaitNow)
    {
        isStartedWait = startWaitNow;
    }

    #endregion
    
    
    
    public override bool IsFinished => hasResult || isCanceled || isFaulted;
    public override bool HasResulted => hasResult;
    
    
    
    public T Result
    {
        get
        {
            if (IsFinished == false)
                throw new NullReferenceException("WaitToken is not finished. Please wait!");

            if (isCanceled)
                throw new Exception("No result because was canceled");

            if (isFaulted)
                throw Exception;

            return result;
        }
    }
    
    public void SetResult(T result)
    {
        lock (objectLock)
        {
            if (IsFinished)
            {
                Debug.LogError("WaitToken finished, cannot set result!");
                return;
            }

            this.result = result;
            hasResult = true;
        }

        InvokeListActionInMainThread();
        SetResultForListChild(result);
    }
    
    
    
    #region WaitToken Child Handle
    
    private void SetResultForListChild(T result)
    {
        if (listChild == null)
            return;

        for (int i = 0; i < listChild.Count; i++)
        {
            listChild[i].SetResult(result);
        }
    }
    
    protected override void SetResultFromThisToOther(WaitToken<T> other)
    {
        other.SetResult(result);
    }
    
    #endregion
    
    
    public static WaitToken<T> CreateHasResulted(T result)
    {
        var waitToken = new WaitToken<T>();
        waitToken.SetResult(result);
        return waitToken;
    }
}