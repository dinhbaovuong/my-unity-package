using System;
using UnityEngine;
using VPackage.RxSystem;
using Object = UnityEngine.Object;

public static class SubscribeExtension
{
    #region No Parameter

    public static Disposable Subscribe(this Observable observable, Action onEmit)
    {
        return observable.Subscribe(new ObserverAction(onEmit));
    }
    
    public static Disposable SubscribeUntilDestroy(this Observable observable, Action onEmit, Object unityObject)
    {
        if (unityObject == null)
        {
            Debug.LogError("Cannot subscribe with object null!");
            return null;
        }
    
        Disposable disposable = null;
        ObserverAction observerAction = new ObserverAction(() =>
        {
            if (unityObject == null)
            {
                Rx.CancelDisposeWhenObjectDestroyed(disposable);
                disposable.Dispose();
                return;
            }
        
            onEmit.Invoke();
        });
        
        
        disposable = observable.Subscribe(observerAction);
        Rx.DisposeWhenObjectDestroyed(disposable, unityObject);
        return disposable;
    }
    
    public static Disposable SubscribeOnce(this Observable observable, Action onEmit)
    {
        Disposable disposable = null;
        var observerAction = new ObserverAction(() =>
        {
            disposable.Dispose();
            onEmit.Invoke();
        });
        
        
        disposable = observable.Subscribe(observerAction);
        return disposable;
    }
    
    public static Disposable SubscribeOnceUntilDestroy(this Observable observable, Action onEmit, Object unityObject)
    {
        if (unityObject == null)
        {
            Debug.LogError("Cannot subscribe with object null!");
            return null;
        }
    
        Disposable disposable = null;
        ObserverAction observerAction = new ObserverAction(() =>
        {
            if (unityObject == null)
            {
                Rx.CancelDisposeWhenObjectDestroyed(disposable);
                disposable.Dispose();
                return;
            }
        
            disposable.Dispose();
            onEmit.Invoke();
        });
        
        
        disposable = observable.Subscribe(observerAction);
        Rx.DisposeWhenObjectDestroyed(disposable, unityObject);
        return disposable;
    }

    #endregion

    #region One Parameter

    public static Disposable Subscribe<T>(this Observable<T> observable, Action<T> onEmit)
    {
        return observable.Subscribe(new ObserverAction<T>(onEmit));
    }
    
    public static Disposable SubscribeUntilDestroy<T>(this Observable<T> source, Action<T> onEmit, Object unityObject)
    {
        if (unityObject == null)
        {
            Debug.LogError("Cannot subscribe with object null!");
            return null;
        }
    
        Disposable disposable = null;
        var observerAction = new ObserverAction<T>(p =>
        {
            if (unityObject == null)
            {
                Rx.CancelDisposeWhenObjectDestroyed(disposable);
                disposable.Dispose();
                return;
            }

            onEmit.Invoke(p);
        });
        
        
        disposable = source.Subscribe(observerAction);
        Rx.DisposeWhenObjectDestroyed(disposable, unityObject);
        return disposable;
    }

    public static Disposable SubscribeOnce<T>(this Observable<T> observable, Action<T> onEmit)
    {
        Disposable disposable = null;
        var observerAction = new ObserverAction<T>(p =>
        {
            disposable.Dispose();
            onEmit.Invoke(p);
        });
        
        
        disposable = observable.Subscribe(observerAction);
        return disposable;
    }
    
    public static Disposable SubscribeOnceUntilDestroy<T>(this Observable<T> source, Action<T> onEmit, Object unityObject)
    {
        if (unityObject == null)
        {
            Debug.LogError("Cannot subscribe with object null!");
            return null;
        }
    
        Disposable disposable = null;
        var observerAction = new ObserverAction<T>(p =>
        {
            if (unityObject == null)
            {
                Rx.CancelDisposeWhenObjectDestroyed(disposable);
                disposable.Dispose();
                return;
            }

            disposable.Dispose();
            onEmit.Invoke(p);
        });
        
        
        disposable = source.Subscribe(observerAction);
        Rx.DisposeWhenObjectDestroyed(disposable, unityObject);
        return disposable;
    }
    
    #endregion

    #region WaitEmit - No Parameter

    public static WaitToken WaitEmit(this Observable observable)
    {
        WaitToken waitToken = new WaitToken();
        Disposable disposable = null;
        var observerAction = new ObserverAction(() =>
        {
            disposable.Dispose();
            waitToken.SetResult();
        });

        
        disposable = observable.Subscribe(observerAction);
        return waitToken;
    }
    
    public static WaitToken WaitEmitUntilDestroy(this Observable observable, Object unityObject)
    {
        WaitToken waitToken = new WaitToken();
        Disposable disposable = null;
        var observerAction = new ObserverAction(() =>
        {
            if (unityObject == null)
            {
                Rx.CancelDisposeWhenObjectDestroyed(disposable);
                disposable.Dispose();
                waitToken.Cancel();
                return;
            }
            
            disposable.Dispose();
            waitToken.SetResult();
        });

        
        disposable = observable.Subscribe(observerAction);
        return waitToken;
    }

    #endregion
    
    #region WaitEmit - One Parameter

    public static WaitToken<T> WaitEmit<T>(this Observable<T> observable)
    {
        WaitToken<T> waitToken = new WaitToken<T>();
        Disposable disposable = null;
        var observerAction = new ObserverAction<T>(p =>
        {
            disposable.Dispose();
            waitToken.SetResult(p);
        });

        
        disposable = observable.Subscribe(observerAction);
        return waitToken;
    }
    
    public static WaitToken<T> WaitEmitUntilDestroy<T>(this Observable<T> observable, Object unityObject)
    {
        WaitToken<T> waitToken = new WaitToken<T>();
        Disposable disposable = null;
        var observerAction = new ObserverAction<T>(p =>
        {
            if (unityObject == null)
            {
                Rx.CancelDisposeWhenObjectDestroyed(disposable);
                disposable.Dispose();
                waitToken.Cancel();
                return;
            }

            disposable.Dispose();
            waitToken.SetResult(p);
        });

        
        disposable = observable.Subscribe(observerAction);
        return waitToken;
    }

    #endregion
}