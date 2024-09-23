namespace VPackage.RxSystem
{
    public interface Observable
    {
        Disposable Subscribe(Observer observer);
    }
}