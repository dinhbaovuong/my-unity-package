using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

public abstract class WaitTokenBase : CustomYieldInstruction
{
    protected readonly object objectLock = new object();
    
    protected bool isStartedWait = true;
    protected bool isCanceled;
    protected bool isFaulted;
    
    protected long? errorCode;
    protected string errorMessage;
    protected Exception rawException;
    protected Exception faultStringException;

    private Task<WaitTokenBase> waitTaskBase;
    
    public abstract bool IsFinished { get; }
    public abstract bool HasResulted { get; }
    public bool IsCanceled => isCanceled;
    public bool IsFaulted => isFaulted;
    public bool IsFaultedOrCanceled => IsFaulted || IsCanceled;
    public bool HasResultedOrUnfinished => HasResulted || !IsFinished;
    public bool IsUnfinished => !IsFinished;

    
    /// <summary>
    /// Sometimes you initialize it but have not used it yet
    /// </summary>
    public bool IsStartedWait => isStartedWait;
    
    /// <summary>
    /// Started wait and not finished
    /// </summary>
    public bool IsWaiting => IsStartedWait && !IsFinished;
    
    /// <summary>
    /// For IEnumerator
    /// </summary>
    public override bool keepWaiting => !IsFinished;
    
    
    
    public long? ErrorCode => errorCode;
    public string ErrorMessage => errorMessage;
    
    /// <summary>
    /// if rawException is not set, will use Exception with FaultToString()
    /// </summary>
    public Exception Exception
    {
        get
        {
            if (IsFinished == false)
                return null;

            if (isFaulted == false)
                return null;

            if (rawException != null)
                return rawException;

            if(faultStringException == null)
                faultStringException = new Exception(FaultToString());
            return faultStringException;
        }
    }
    
    
    
    public void StartWait()
    {
        isStartedWait = true;
    }
    
    
    
    public void Cancel()
    {
        lock (objectLock)
        {
            if (IsFinished)
            {
                Debug.LogWarning("WaitToken finished, cannot cancel!");
                return;
            }
            
            isCanceled = true;
        }
        
        InvokeListActionInMainThread();
        CancelForListChild();
    }
    
    public void SetFault(long? errorCode, string errorMessage, Exception rawException)
    {
        lock (objectLock)
        {
            if (IsFinished)
            {
                Debug.LogError("WaitToken finished, cannot set fault!");
                return;
            }

            this.errorCode = errorCode;
            this.errorMessage = errorMessage;
            this.rawException = rawException;
            isFaulted = true;
        }
        
        InvokeListActionInMainThread();
        SetFaultForListChild(errorCode, errorMessage, rawException);
    }
    
    public void SetFault(Exception exception)
    {
        SetFault(null, null, exception);
    }

    public void SetFault(long? errorCode)
    {
        SetFault(errorCode, null, null);
    }

    public void SetFault(string errorMessage)
    {
        SetFault(null, errorMessage, null);
    }

    public void SetFault(long? errorCode, string errorMessage)
    {
        SetFault(errorCode, errorMessage, null);
    }
    
    public virtual string FaultToString()
    {
        if (IsFinished == false)
        {
            Debug.LogError("Cannot get fault string because wait token is not finished");
            return null;
        }
        
        if (isFaulted == false)
        {
            Debug.LogError("Cannot get fault string because wait token is not fault");
            return null;
        }
        
        StringBuilder sb = new StringBuilder();
        
        if (errorCode != null)
            sb.AppendLine("Error Code: " + errorCode.Value);

        if (errorMessage != null)
            sb.AppendLine("Error Message: " + errorMessage);

        if (rawException != null)
            sb.AppendLine("Exception: " + rawException);

        return sb.ToString();
    }


    #region WaitToken Child Handle

    protected abstract void CancelForListChild();

    protected abstract void SetFaultForListChild(long? errorCode, string errorMessage, Exception rawException);

    #endregion

    protected abstract void InvokeListActionInMainThread();
    
    public Task<WaitTokenBase> WaitTaskBase()
    {
        lock (objectLock)
        {
            if (waitTaskBase == null)
            {
                waitTaskBase = new Task<WaitTokenBase>(() =>
                {
                    while (!IsFinished)
                    {
                        Thread.Sleep(16);
                    }
                    return this;
                });
                
                waitTaskBase.Start();
            }
        }
        
        return waitTaskBase;
    }

    public void SetFaultToOther(WaitTokenBase other)
    {
        other.SetFault(errorCode, errorMessage, rawException);
    }
}