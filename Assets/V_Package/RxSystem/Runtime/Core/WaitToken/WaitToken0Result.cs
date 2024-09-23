using UnityEngine;

public class WaitToken : WaitTokenBase<WaitToken>
{
    protected bool hasResult;



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
    
    public void SetResult()
    {
        lock (objectLock)
        {
            if (IsFinished)
            {
                Debug.LogError("WaitToken finished, cannot set result!");
                return;
            }
            
            hasResult = true;
        }
            
        InvokeListActionInMainThread();
        SetResultForListChild();
    }
    
    

    #region WaitToken Child Handle

    private void SetResultForListChild()
    {
        if (listChild == null)
            return;

        for (int i = 0; i < listChild.Count; i++)
        {
            listChild[i].SetResult();
        }
    }
    
    protected override void SetResultFromThisToOther(WaitToken other)
    {
        other.SetResult();
    }

    #endregion
    
    
    
    public static WaitToken CreateHasResulted()
    {
        var waitToken = new WaitToken();
        waitToken.SetResult();
        return waitToken;
    }
}