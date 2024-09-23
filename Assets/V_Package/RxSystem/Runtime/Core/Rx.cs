using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UnityEngine.SceneManagement;
using VPackage.RxSystem;
using Object = UnityEngine.Object;

public class Rx : MonoBehaviour
{
    private static Thread mainThread = null;
    protected static Rx Instance;
    protected static object objectLock = new object();
    
    static readonly List<Action> listActionExecuteInMainThread = new List<Action>();
    static readonly List<DelayAction> listDelayActionExecuteInMainThread = new List<DelayAction>();
    
    private static readonly Dictionary<Disposable, Object> dictDisposableWhenObjectDestroyed = new Dictionary<Disposable, Object>();
    
    #region Time Field

    private static float time { get; set; }
    private static float unscaledTime { get; set; }
    
    /// <summary>
    /// When game pause, this time not increase
    /// </summary>
    public static float AppTime { get; private set; }
    
    /// <summary>
    /// When game pause, this time not increase
    /// </summary>
    public static float UnscaledAppTime { get; private set; }

    #endregion


    private static bool applicationPauseStatus = false;
    private static bool skipUpdateAppTimeOnce = false;
    
    
    //Events
    public static readonly Subject onUpdate = new Subject();
    public static readonly Subject<bool> onApplicationPause = new Subject<bool>();
    public static readonly Subject onApplicationQuit = new Subject();
    

    #region MonoBehaviour Method

    private void Awake()
    {
        mainThread = Thread.CurrentThread;
        UpdateTime();
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void Start()
    {
        UpdateTime();
    }

    private void Update()
    {
        UpdateTime();
        UpdateAppTime();
        
        ExecuteListActionExecuteInMainThread();
        ExecuteListDelayActionExecuteInMainThread();

        onUpdate.Emit();
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        CheckAndDisposeIfObjectDestroyed();
    }

    private void OnApplicationPause(bool pauseStatus)
    {
        applicationPauseStatus = pauseStatus;
        
        if (applicationPauseStatus == false)
            skipUpdateAppTimeOnce = true;
        
        
        
        onApplicationPause.Emit(pauseStatus);
    }

    private void OnApplicationQuit()
    {
        onApplicationQuit.Emit();
    }
    
    #endregion
    

    [RuntimeInitializeOnLoadMethod]
    private static void AutoInitialize()
    {
        lock (objectLock)
        {
            Instance = new GameObject("ZeroRx").AddComponent<Rx>();
            DontDestroyOnLoad(Instance.gameObject);
            
            Debug.Log("Auto Initialize ZeroRx");
        }
    }

    /// <summary>
    /// Kiểm tra xem thread gọi property này có phải main thread hay không
    /// </summary>
    public static bool IsMainThread => Thread.CurrentThread == mainThread;

    static void UpdateTime()
    {
        time = Time.time;
        unscaledTime = Time.unscaledTime;
    }

    private void UpdateAppTime()
    {
        if(applicationPauseStatus)
            return;

        if (skipUpdateAppTimeOnce)
        {
            skipUpdateAppTimeOnce = false;
            return;
        }

        AppTime += Time.deltaTime;
        UnscaledAppTime += Time.unscaledDeltaTime;
    }
    
    #region Execute In Main Thread

    static void ExecuteListActionExecuteInMainThread()
    {
        lock (objectLock)
        {
            if(listActionExecuteInMainThread.Count == 0)
                return;

            for (int i = 0; i < listActionExecuteInMainThread.Count; i++)
            {
                var action = listActionExecuteInMainThread[i];
                try
                {
                    action?.Invoke();
                }
                catch (Exception e)
                {
                    Debug.LogException(e);
                }
            }

            listActionExecuteInMainThread.Clear();
        }
    }

    static void ExecuteListDelayActionExecuteInMainThread()
    {
        lock (objectLock)
        {
            if(listDelayActionExecuteInMainThread.Count == 0)
                return;

            for (int i = 0; i < listDelayActionExecuteInMainThread.Count; i++)
            {
                var delayAction = listDelayActionExecuteInMainThread[i];
                
                if (delayAction.initialized == false)
                {
                    delayAction.Initialize();
                    continue;
                }
                
                if (delayAction.IsTimeToExecute())
                {
                    listDelayActionExecuteInMainThread.RemoveAt(i);
                    i--;
                    
                    try
                    {
                        delayAction.action?.Invoke();
                    }
                    catch (Exception e)
                    {
                        Debug.LogException(e);
                    }
                }
            }
        }
    }

    /// <summary>
    /// Run action in main thread
    /// </summary>
    public static void RunAction(Action action)
    {
        if (IsMainThread)
        {
            try
            {
                action?.Invoke();
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
            
            return;
        }
        
        lock (objectLock)
        {
            listActionExecuteInMainThread.Add(action);
        }
    }

    /// <summary>
    /// Run action in main thread. Delay use Time.time or Time.unscaledTime
    /// </summary>
    public static void RunActionDelay(Action action, float delayTime, bool useUnscaledTime)
    {
        //Nếu không delay và đạt đủ điều kiện thì thực hiện luôn
        if (delayTime <= 0 && IsMainThread)
        {
            try
            {
                action?.Invoke();
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
            return;
        }
        
        
        lock (objectLock)
        {
            DelayAction delayAction = new DelayAction(action, delayTime, useUnscaledTime);
            listDelayActionExecuteInMainThread.Add(delayAction);
        }
    }

    /// <summary>
    /// Run action in main thread. Delay use Time.time
    /// </summary>
    public static void RunActionDelayScaledTime(Action action, float delayTime)
    {
        RunActionDelay(action, delayTime, false);
    }

    /// <summary>
    /// Run action in main thread. Delay use Time.unscaledTime
    /// </summary>
    public static void RunActionDelayUnscaledTime(Action action, float delayTime)
    {
        RunActionDelay(action, delayTime, true);
    }
    
    public static WaitToken<T> RunFunc<T>(Func<T> func)
    {
        if (IsMainThread)
        {
            try
            {
                var result = func.Invoke();
                return WaitToken<T>.CreateHasResulted(result);
            }
            catch (Exception e)
            {
                Debug.LogException(e);
                return WaitToken<T>.CreateFaulted(e);
            }
        }

        WaitToken<T> wt = new WaitToken<T>();
        RunAction(() =>
        {
            try
            {
                T result = func.Invoke();
                wt.SetResult(result);
            }
            catch (Exception e)
            {
                Debug.LogException(e);
                wt.SetFault(e);
            }
        });
        
        return wt;
    }

    public static WaitToken<T> RunFuncDelay<T>(Func<T> func, float delay, bool ignoreTimeScale)
    {
        if (delay <= 0 && IsMainThread)
        {
            try
            {
                var result = func.Invoke();
                return WaitToken<T>.CreateHasResulted(result);
            }
            catch (Exception e)
            {
                Debug.LogException(e);
                return WaitToken<T>.CreateFaulted(e);
            }
        }
        
        WaitToken<T> wt = new WaitToken<T>();
        RunActionDelay(() =>
        {
            try
            {
                T result = func.Invoke();
                wt.SetResult(result);
            }
            catch (Exception e)
            {
                Debug.LogException(e);
                wt.SetFault(e);
            }
        }, delay, ignoreTimeScale);
        
        return wt;
    }

    #endregion

    #region Disposer

    void CheckAndDisposeIfObjectDestroyed()
    {
        lock (objectLock)
        {
            if(dictDisposableWhenObjectDestroyed.Count == 0)
                return;

            List<Disposable> listRemove = null;
            foreach (var kv in dictDisposableWhenObjectDestroyed)
            {
                if (kv.Value == null)
                {
                    kv.Key.Dispose();
                    
                    if(listRemove == null)
                        listRemove = new List<Disposable>();
                    
                    listRemove.Add(kv.Key);
                }
            }

            if (listRemove != null)
            {
                foreach (var d in listRemove)
                {
                    dictDisposableWhenObjectDestroyed.Remove(d);
                }
            }
        }
    }

    public static void DisposeWhenObjectDestroyed(Disposable disposable, Object unityObject)
    {
        lock (objectLock)
        {
            dictDisposableWhenObjectDestroyed.Add(disposable, unityObject);
        }
    }
    
    public static void CancelDisposeWhenObjectDestroyed(Disposable disposable)
    {
        lock (objectLock)
        {
            dictDisposableWhenObjectDestroyed.Remove(disposable);
        }
    }

    #endregion

    #region Coroutine
    
    /// <summary>
    /// If you call when ZeroRx.Instance is null, the code will still be executed when ZeroRx init, but will return a null coroutine.
    /// </summary>
    public static Coroutine RunCoroutine(IEnumerator routine)
    {
        if (Instance == null)
        {
            RunAction(() =>
            {
                Instance.StartCoroutine(routine);
            });

            return null;
        }
        else
        {
            if (IsMainThread)
            {
                return Instance.StartCoroutine(routine);
            }
            else
            {
                Debug.LogError("Only call RunCoroutine in main thread, use RunCoroutineAsync instead");
                return null;
            }
        }
    }

    public static void StopRunCoroutine(Coroutine coroutine)
    {
        if(coroutine == null)
            return;
        
        if (IsMainThread)
        {
            Instance.StopCoroutine(coroutine);
        }
        else
        {
            RunAction(() => Instance.StopCoroutine(coroutine));
        }
    }

    /// <summary>
    /// Start coroutine from outside main thread
    /// </summary>
    /// <param name="routine"></param>
    /// <returns></returns>
    public static WaitToken<Coroutine> RunCoroutineAsync(IEnumerator routine)
    {
        if (IsMainThread)
        {
            var coroutine = Instance.StartCoroutine(routine);
            return WaitToken<Coroutine>.CreateHasResulted(coroutine);
        }
        else
        {
            WaitToken<Coroutine> wt = new WaitToken<Coroutine>();
            RunAction(() =>
            {
                var coroutine = Instance.StartCoroutine(routine);
                wt.SetResult(coroutine);
            });
            return wt;
        }
    }

    #endregion
}