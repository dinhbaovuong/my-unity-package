namespace VPackage.RxSystem
{
    public interface Observable<out T>
    {
        Disposable Subscribe(Observer<T> observer);
    }
}