using System;

namespace VPackage.RxSystem
{
    public class DisposableAction : Disposable
    {
        private Action action;
        
        public DisposableAction(Action action)
        {
            this.action = action;
        }
        
        public void Dispose()
        {
            action?.Invoke();
            action = null;
        }
    }
}