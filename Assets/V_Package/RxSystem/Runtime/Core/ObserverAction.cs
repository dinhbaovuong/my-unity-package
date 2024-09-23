using System;

namespace VPackage.RxSystem
{
    public class ObserverAction : Observer
    {
        private Action action;
        
        public ObserverAction(Action action)
        {
            this.action = action;
        }
        
        public void Emit()
        {
            action.Invoke();
        }
    }
}