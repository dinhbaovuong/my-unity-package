namespace VPackage.RxSystem
{
    public interface ISubject<T> : Observer<T>, Observable<T>, Disposable
    {
    }
}