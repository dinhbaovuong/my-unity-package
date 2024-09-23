using System;
using UnityEngine;

public class WaitToken<T1, T2> : WaitTokenBase<WaitToken<T1, T2>>
{
    private T1 result1;
    private T2 result2;

    private bool hasResult1;
    private bool hasResult2;
    
    
    
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
    
    
    
    public override bool IsFinished => (hasResult1 && hasResult2) || isCanceled || isFaulted;
    public override bool HasResulted => hasResult1 && hasResult2;
    
    
    
    public T1 Result1
    {
        get
        {
            if (IsFinished == false)
                throw new NullReferenceException("WaitToken is not finished. Please wait!");

            if (isCanceled)
                throw new Exception("No result because was canceled");

            if (isFaulted)
                throw Exception;

            return result1;
        }
    }

    public T2 Result2
    {
        get
        {
            if (IsFinished == false)
                throw new NullReferenceException("WaitToken is not finished. Please wait!");

            if (isCanceled)
                throw new Exception("No result because was canceled");

            if (isFaulted)
                throw Exception;

            return result2;
        }
    }
    
    public void SetResult1(T1 result1)
    {
        bool hasResult2InLock = false;
        lock (objectLock)
        {
            if (IsFinished)
            {
                Debug.LogError("WaitToken finished, cannot set result!");
                return;
            }

            if (hasResult1)
            {
                Debug.LogError("Result 1 has been set, cannot set again!");
                return;
            }

            this.result1 = result1;
            hasResult1 = true;
            hasResult2InLock = hasResult2;
        }

        if (hasResult2InLock)
            InvokeListActionInMainThread();

        SetResult1ForListChild(result1);
    }

    public void SetResult2(T2 result2)
    {
        bool hasResult1InLock = false;
        lock (objectLock)
        {
            if (IsFinished)
            {
                Debug.LogError("WaitToken finished, cannot set result!");
                return;
            }

            if (hasResult2)
            {
                Debug.LogError("Result 2 has been set, cannot set again!");
                return;
            }

            this.result2 = result2;
            hasResult2 = true;
            hasResult1InLock = hasResult1;
        }

        if (hasResult1InLock)
            InvokeListActionInMainThread();

        SetResult2ForListChild(result2);
    }
    
    
    
    #region WaitToken Child Handle
    
    private void SetResult1ForListChild(T1 result1)
    {
        if (listChild == null)
            return;

        for (int i = 0; i < listChild.Count; i++)
        {
            listChild[i].SetResult1(result1);
        }
    }

    private void SetResult2ForListChild(T2 result2)
    {
        if (listChild == null)
            return;

        for (int i = 0; i < listChild.Count; i++)
        {
            listChild[i].SetResult2(result2);
        }
    }
    
    protected override void SetResultFromThisToOther(WaitToken<T1, T2> other)
    {
        other.SetResult1(result1);
        other.SetResult2(result2);
    }
    
    #endregion
    
    
    
    public static WaitToken<T1, T2> CreateHasResulted(T1 result1, T2 result2)
    {
        var waitToken = new WaitToken<T1, T2>();
        waitToken.SetResult1(result1);
        waitToken.SetResult2(result2);
        return waitToken;
    }
}