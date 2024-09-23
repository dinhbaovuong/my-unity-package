using System.Collections;
using System.Collections.Generic;

namespace VPackage.RxSystem
{
    public class Disposables : Disposable
    {
        readonly HashSet<Disposable> listDisposable = new HashSet<Disposable>();
        readonly object objectLock = new object();
        
        public void Dispose()
        {
            lock (objectLock)
            {
                foreach (var disposable in listDisposable)
                {
                    disposable.Dispose();
                }
            
                listDisposable.Clear();
            }
        }

        public bool Add(Disposable disposable)
        {
            if (disposable == null)
                return false;
            
            lock (objectLock)
            {
                return listDisposable.Add(disposable);
            }
        }

        public bool Remove(Disposable disposable)
        {
            lock (objectLock)
            {
                return listDisposable.Remove(disposable);
            }
        }
        
        public bool Contains(Disposable disposable)
        {
            lock (objectLock)
            {
                return listDisposable.Contains(disposable);
            }
        }
        
        public int Count
        {
            get
            {
                lock (objectLock)
                {
                    return listDisposable.Count;
                }
            }
        }
    }
}