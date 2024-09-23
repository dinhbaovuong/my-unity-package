namespace VPackage.RxSystem
{
    [System.Serializable]
    public class DoubleRx : VariableRx<double>
    {
        public static implicit operator double(DoubleRx v) => v.Value;
    }
}