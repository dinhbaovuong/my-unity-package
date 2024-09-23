using System;
using UnityEngine;

namespace VPackage.RxSystem
{
    public class DelayAction
    {
        public float delayTime;
        private bool ignoreTimeScale;
        
        public bool initialized = false;
        private float timeExecute;
        public Action action;

        public DelayAction(Action action, float delayTime, bool ignoreTimeScale)
        {
            this.action = action;
            this.delayTime = delayTime;
            this.ignoreTimeScale = ignoreTimeScale;
        }

        //Only call on main thread
        public void Initialize()
        {
            initialized = true;

            if (ignoreTimeScale)
                timeExecute = Time.unscaledTime + delayTime;
            else
                timeExecute = Time.time + delayTime;
        }

        //Only call on main thread and after initialize
        public bool IsTimeToExecute()
        {
            if (ignoreTimeScale)
                return Time.unscaledTime >= timeExecute;
            else
                return Time.time >= timeExecute;
        }
    }
}