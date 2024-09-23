using VPackage.RxSystem;

public static class DisposableExtension
{
    public static T AddTo<T>(this T disposable, Disposables disposables) where T : Disposable
    {
        disposables.Add(disposable);
        return disposable;
    }
}