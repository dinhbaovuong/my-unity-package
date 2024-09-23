using System;

namespace VPackage.RxSystem
{
    public class ObserverAction<T> : Observer<T>
    {
        private Action<T> action;
        
        public ObserverAction(Action<T> action)
        {
            this.action = action;
        }
        
        public void Emit(T value)
        {
            action.Invoke(value);
        }
    }
}