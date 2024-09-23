using System;
using System.Collections;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace VPackage.RxSystem.Examples
{
    public class TestRx : MonoBehaviour
    {
        [SerializeField] private GameObject keyObject;
        [SerializeField] private FloatRx hp;
        
        private Subject<string> subject = new Subject<string>();
        private Subject onTimeOut = new Subject();

        private Disposables disposables = new Disposables();
        private void Awake()
        {
            
        }

        private void Start()
        {
            hp.SubscribeUntilDestroy(v => Debug.Log("On Hp Changed: " + v), this);
            onTimeOut.SubscribeOnceUntilDestroy(OnTimeout, this);
        }

        private void Update()
        {
            if (Input.GetMouseButtonDown(0))
            {
                Rx.RunAction(() => Debug.Log("Nothing in your eyes"));
            }

            if (Input.GetMouseButtonDown(1))
            {
                Rx.RunActionDelay(() => Debug.Log("Delay"), 1f, true);
            }
            
            if (Input.GetKeyDown(KeyCode.Alpha2))
            {
                onTimeOut.Emit();
            }
            
            if(Input.GetKeyDown(KeyCode.Alpha3))
                TestWaitTokenTimeout();
        }

        async void Haha()
        {
            Debug.Log("Bắt đầu haha");
            WaitToken tw = new WaitToken();

            for (int i = 0; i < 10; i++)
            {
                Task t = new Task(() =>
                {
                    Thread.Sleep(1000);
                    tw.SetResult();
                });
                t.Start();
            }
           
            await tw;
            Debug.Log("đợi ok");
        }

        void OnTimeout()
        {
            Debug.Log("OnTimeout");
            disposables.Dispose();
        }

        void TestDeadLock()
        {
            Subject<string> sj = new Subject<string>(false);

            Action<string> action1 = s =>
            {
                Task t = new Task(() =>
                {
                    sj.UnSubscribe(null);
                });
                t.Start();
            };
            
            Action<string> action2 = s =>
            {
                int n = 2;
            };
            
            sj.Subscribe(s => Rx.RunAction(() => Debug.Log("Bắt đầu")));
            sj.Subscribe(action1);
            for (int i = 0; i < 10000; i++)
            {
                sj.Subscribe(action2);
            }
            
            sj.Subscribe(s => Rx.RunAction(() => Debug.Log("Ko deadlock")));
            
            Task a = new Task(() =>
            {
                Thread.Sleep(1000);
                sj.Emit("cc");
            });
            a.Start();
        }

        public void LogDelay()
        {
            Debug.Log("Call Run Action Delay");
            Rx.RunActionDelay(() => Debug.Log("Log Delay"), 3, true);
        }


        void TestWaitTokenTimeout()
        {
            WaitToken waitToken = new WaitToken();
            StartCoroutine(Timeline());

            waitToken.OnFinish(wt => Debug.Log("Wt finished"));
            waitToken.OnFinishTimeOutUnscaledTime(wt =>
            {
                if (wt.IsFinished == false)
                    Debug.Log("Wait TimeOut");
                else
                    Debug.Log("Wt finished");
                ;
            }, 3);
            
            Debug.Log("Start TestWaitTokenTimeout");

            
            
            IEnumerator Timeline()
            {
                yield return new WaitForSeconds(5);
                waitToken.SetResult();
            }
        }
    }
}