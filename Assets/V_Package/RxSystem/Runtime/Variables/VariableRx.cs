using System;
using UnityEngine;
using Object = UnityEngine.Object;

namespace VPackage.RxSystem
{
    [System.Serializable]
    public class VariableRx<T> where T : struct
    {
        [SerializeField] private T value;
        private Subject<T> subject = new Subject<T>();

        public T Value
        {
            get => value;
            set
            {
                this.value = value;
                subject.Emit(value);
            }
        }

        public VariableRx()
        {
        }

        public VariableRx(T value)
        {
            this.value = value;
        }
        
        

        public Disposable Subscribe(Action<T> callBack)
        {
            return subject.Subscribe(callBack);
        }
        
        public Disposable SubscribeOnce(Action<T> callBack)
        {
            return subject.SubscribeOnce(callBack);
        }
        
        public Disposable SubscribeUntilDestroy(Action<T> callBack, Object unityObject)
        {
            return subject.SubscribeUntilDestroy(callBack, unityObject);
        }
        
        public Disposable SubscribeOnceUntilDestroy(Action<T> callBack, Object unityObject)
        {
            return subject.SubscribeOnceUntilDestroy(callBack, unityObject);
        }
    }
}