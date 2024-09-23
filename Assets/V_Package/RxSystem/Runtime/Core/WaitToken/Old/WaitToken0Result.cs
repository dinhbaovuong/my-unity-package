using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace VPackage.RxSystem.OldWaitToken
{
    public class WaitToken : CustomYieldInstruction
    {
        private bool isSucceeded;
        private bool isCanceled;
        private bool isFaulted;
        
        private long? errorCode;
        private string errorMessage;
        private Exception exception;
        private Exception exceptionFromErrorCodeAndMessage;
        
        private List<Action<WaitToken>> listOnFinish;
        private Task<WaitToken> waitTask;
        private List<WaitToken> listChild;
        
        readonly object objectLock = new object();
        
        
        
        #region Constructor

        /// <summary>
        /// Start wait now
        /// </summary>
        public WaitToken(){}

        /// <summary>
        /// Sometimes you initialize it but have not used it yet
        /// </summary>
        public WaitToken(bool startWaitNow)
        {
            IsStartedWait = startWaitNow;
        }

        #endregion
        
        
        
        public bool IsFinished => isSucceeded || isCanceled || isFaulted;
        public bool IsSucceeded => isSucceeded;
        public bool IsCanceled => isCanceled;
        public bool IsFaulted => isFaulted;
        public bool IsFaultedOrCanceled => IsFaulted || IsCanceled;
        
        
        
        /// <summary>
        /// Sometimes you initialize it but have not used it yet
        /// </summary>
        public bool IsStartedWait { get; private set; } = true;
        
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
        public Exception Exception
        {
            get
            {
                if (IsFinished == false)
                    return null;

                if (isFaulted == false)
                    return null;

                if (exception != null)
                    return exception;

                if(exceptionFromErrorCodeAndMessage == null)
                    exceptionFromErrorCodeAndMessage = new Exception(FaultToString());
                return exceptionFromErrorCodeAndMessage;
            }
        }

        
        
        public void StartWait()
        {
            IsStartedWait = true;
        }
        
        
        
        public void SetSuccess()
        {
            lock (objectLock)
            {
                if (IsFinished)
                {
                    Debug.LogError("WaitToken finished, cannot set success!");
                    return;
                }
            
                isSucceeded = true;
            }
            
            InvokeListActionInMainThread(listOnFinish);
            SetSuccessForListChild();
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
            
            InvokeListActionInMainThread(listOnFinish);
            CancelForListChild();
        }

        public void SetFault(long? errorCode, string errorMessage, Exception exception)
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
                this.exception = exception;
                isFaulted = true;
            }
            
            InvokeListActionInMainThread(listOnFinish);
            SetFaultForListChild(errorCode, errorMessage, exception);
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

            if (exception != null)
                sb.AppendLine("Exception: " + exception);

            return sb.ToString();
        }


        
        /// <summary>
        /// action sẽ được gọi sau khi Finish. Được gọi ngay lập tức nếu đã Finished
        /// </summary>
        public WaitToken OnFinish(Action<WaitToken> action)
        {
            lock (objectLock)
            {
                if (IsFinished == false)
                {
                    if(listOnFinish == null)
                        listOnFinish = new List<Action<WaitToken>>();
                    
                    listOnFinish.Add(action);
                    return this;
                }
            }
            
            InvokeActionInMainThread(action);
            return this;
        }

        public Task<WaitToken> WaitTask()
        {
            lock (objectLock)
            {
                if (waitTask == null)
                {
                    waitTask = new Task<WaitToken>(() =>
                    {
                        while (!IsFinished)
                        {
                            Thread.Sleep(16);
                        }
                        return this;
                    });
                    
                    waitTask.Start();
                }
            }
            
            return waitTask;
        }
        
        

        #region WaitToken Child Handle
        
        public void NotifyOtherWhenFinished(WaitToken other)
        {
            lock (objectLock)
            {
                if(IsFinished == false) //Nếu chưa finish mới cần add chứ finish rồi thì khỏi cần
                {
                    if (listChild == null)
                        listChild = new List<WaitToken>();

                    if (listChild.Contains(other))
                        return;

                    listChild.Add(other);
                    return;
                }
            }
            
            if(isCanceled)
                other.Cancel();
            else if(isFaulted)
                other.SetFault(errorCode, errorMessage, exception);
            else
                other.SetSuccess();
        }
        
        private void SetSuccessForListChild()
        {
            if (listChild == null)
                return;

            for (int i = 0; i < listChild.Count; i++)
            {
                listChild[i].SetSuccess();
            }
        }
        
        private void CancelForListChild()
        {
            if (listChild == null)
                return;

            for (int i = 0; i < listChild.Count; i++)
            {
                listChild[i].Cancel();
            }
        }
        
        private void SetFaultForListChild(long? errorCode, string errorMessage, Exception exception)
        {
            if (listChild == null)
                return;

            for (int i = 0; i < listChild.Count; i++)
            {
                listChild[i].SetFault(errorCode, errorMessage, exception);
            }
        }
        
        #endregion

        #region WaitToken Other Handle

        public void WaitOtherFinished(WaitToken other)
        {
            other.NotifyOtherWhenFinished(this);
        }

        #endregion
        

        void TryInvokeAction(Action<WaitToken> action)
        {
            try
            {
                action.Invoke(this);
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
        }
        
        void InvokeListActionInMainThread(List<Action<WaitToken>> listOnFinish)
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
        
        void InvokeActionInMainThread(Action<WaitToken> onFinish)
        {
            if (Rx.IsMainThread)
                TryInvokeAction(onFinish);
            else
                Rx.RunAction(() => TryInvokeAction(onFinish));
        }

        

        public static WaitToken CreateSucceeded()
        {
            var waitToken = new WaitToken();
            waitToken.SetSuccess();
            return waitToken;
        }
        
        public static WaitToken CreateCanceled()
        {
            var waitToken = new WaitToken();
            waitToken.Cancel();
            return waitToken;
        }
        
        public static WaitToken CreateFaulted(long? errorCode, string errorMessage, Exception exception)
        {
            var waitToken = new WaitToken();
            waitToken.SetFault(errorCode, errorMessage, exception);
            return waitToken;
        }

        public static WaitToken CreateFaulted(Exception exception)
        {
            var waitToken = new WaitToken();
            waitToken.SetFault(exception);
            return waitToken;
        }
        
        public static WaitToken CreateFaulted(long? errorCode)
        {
            var waitToken = new WaitToken();
            waitToken.SetFault(errorCode);
            return waitToken;
        }
        
        public static WaitToken CreateFaulted(string errorMessage)
        {
            var waitToken = new WaitToken();
            waitToken.SetFault(errorMessage);
            return waitToken;
        }
        
        public static WaitToken CreateFaulted(long? errorCode, string errorMessage)
        {
            var waitToken = new WaitToken();
            waitToken.SetFault(errorCode, errorMessage);
            return waitToken;
        }
    }
}
