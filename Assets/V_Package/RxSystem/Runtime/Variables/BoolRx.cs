namespace VPackage.RxSystem
{
    [System.Serializable]
    public class BoolRx : VariableRx<bool>
    {
        public static implicit operator bool(BoolRx v) => v.Value;
    }
}