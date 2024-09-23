using UnityEngine;

namespace ZeroX.RxSystem
{
    public class WaitForSecondsUnscaledTime : CustomYieldInstruction
    {
        public float waitTime;
        
        private float waitUntilTime = -1f;
        
        

        public override bool keepWaiting
        {
            get
            {
                if (waitUntilTime < 0.0)
                    waitUntilTime = Time.unscaledTime + waitTime;
                
                bool keepWaiting = Time.unscaledTime < waitUntilTime;
                if (!keepWaiting)
                    this.Reset();
                
                return keepWaiting;
            }
        }

        public WaitForSecondsUnscaledTime(float time)
        {
            waitTime = time;
        }

        public override void Reset()
        { 
            this.waitUntilTime = -1f;
        }
    }
}