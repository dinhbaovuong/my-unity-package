namespace VPackage.RxSystem
{
    [System.Serializable]
    public class FloatRx : VariableRx<float>
    {
        public static implicit operator float(FloatRx v) => v.Value;
    }
}