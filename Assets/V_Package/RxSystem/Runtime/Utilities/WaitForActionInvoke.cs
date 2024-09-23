using System;
using UnityEngine;

namespace ZeroX.RxSystem
{
    public class WaitForActionInvoke : CustomYieldInstruction
    {
        private bool invoked = false;
        public override bool keepWaiting => invoked == false;
        
        public WaitForActionInvoke(ref Action action)
        {
            action += () => invoked = true;
        }
    }
}