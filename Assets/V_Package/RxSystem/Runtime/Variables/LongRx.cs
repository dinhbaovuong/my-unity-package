namespace VPackage.RxSystem
{
    [System.Serializable]
    public class LongRx : VariableRx<long>
    {
        public static implicit operator long(LongRx v) => v.Value;
    }
}