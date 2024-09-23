namespace VPackage.RxSystem
{
    public interface Observer<in T>
    {
        void Emit(T value);
    }
}