using System;
using System.Collections.Generic;
using UnityEngine;

namespace VPackage.RxSystem
{
    public class Subject : ISubject
    {
        //Public Field
        public bool onlyEmitOnMainThread = true;
        
        //Not Public Field
        protected HashSet<Observer> listObserver = new HashSet<Observer>();
        protected bool isDisposed;
        protected bool isEmitting;

        //Static Field
        private static string disposedErrorMessage = "Cannot Emit because disposed";

        //Temp Field
        private readonly Queue<Action> actionWhileEmittingQueue = new Queue<Action>();
        private bool handingActionWhileEmittingQueue = false;
        

        public Subject()
        { }
        
        public Subject(bool onlyEmitOnMainThread)
        {
            this.onlyEmitOnMainThread = onlyEmitOnMainThread;
        }


        protected void InternalEmit()
        {
            //Emit
            isEmitting = true;
            foreach (var observer in listObserver)
            {
                try
                {
                    observer.Emit();
                }
                catch (Exception e)
                {
                    Debug.LogException(e);
                }
            }
            isEmitting = false;

            
            
            //Handling Queue
            if(handingActionWhileEmittingQueue)
                return;

            handingActionWhileEmittingQueue = true;
            while (actionWhileEmittingQueue.Count > 0)
            {
                try
                {
                    actionWhileEmittingQueue.Dequeue().Invoke();
                }
                catch (Exception e)
                {
                    Debug.LogException(e);
                }
            }
            handingActionWhileEmittingQueue = false;
        }

        public void Emit()
        {
            if (isDisposed)
            {
                Debug.LogError(disposedErrorMessage);
                return;
            }
            
            if (isEmitting)
            {
                actionWhileEmittingQueue.Enqueue(Emit);
                return;
            }

            lock (this)
            {
                if (onlyEmitOnMainThread)
                {
                    if (Rx.IsMainThread)
                    {
                        InternalEmit();
                    }
                    else
                    {
                        Rx.RunAction(InternalEmit);
                    }
                }
                else
                {
                    InternalEmit();
                }
            }
        }

        public Disposable Subscribe(Observer observer)
        {
            lock (this)
            {
                if (isDisposed)
                {
                    Debug.LogError(disposedErrorMessage);
                    return null;
                }

                if (isEmitting)
                {
                    actionWhileEmittingQueue.Enqueue(() => Subscribe(observer));
                }
                else
                {
                    if (listObserver.Add(observer) == false)
                    {
                        Debug.LogError("Cannot add an observer twice");
                        return null;
                    }
                }

                DisposableAction disposableAction = new DisposableAction(() =>
                {
                    UnSubscribe(observer);
                });

                return disposableAction;
            }
        }
        
        public void UnSubscribe(Observer observer)
        {
            lock (this)
            {
                if (isEmitting)
                {
                    actionWhileEmittingQueue.Enqueue(() => UnSubscribe(observer));
                }
                else
                {
                    listObserver.Remove(observer);
                }
            }
        }

        public void Dispose()
        {
            lock (this)
            {
                if (isEmitting)
                {
                    actionWhileEmittingQueue.Enqueue(Dispose);
                }
                else
                {
                    isDisposed = true;
                    listObserver = null;
                }
            }
        }
    }
}