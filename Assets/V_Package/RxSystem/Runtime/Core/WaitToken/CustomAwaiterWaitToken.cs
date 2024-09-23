using System.Runtime.CompilerServices;

public static class CustomAwaiterWaitToken
{
    public static TaskAwaiter<WaitTokenBase> GetAwaiter(this WaitTokenBase waitToken)
    {
        return waitToken.WaitTaskBase().GetAwaiter();
    }
    
    public static TaskAwaiter<WaitToken> GetAwaiter(this WaitToken waitToken)
    {
        return waitToken.WaitTask().GetAwaiter();
    }
    
    public static TaskAwaiter<WaitToken<T>> GetAwaiter<T>(this WaitToken<T> waitToken)
    {
        return waitToken.WaitTask().GetAwaiter();
    }
    
    public static TaskAwaiter<WaitToken<T1, T2>> GetAwaiter<T1, T2>(this WaitToken<T1, T2> waitToken)
    {
        return waitToken.WaitTask().GetAwaiter();
    }
}