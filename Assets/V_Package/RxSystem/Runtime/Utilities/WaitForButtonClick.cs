using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace ZeroX.RxSystem
{
    public class WaitForButtonClick : CustomYieldInstruction
    {
        private int required = 1;
        private int progress;
        private Button button;
        private UnityAction actButtonClick;

        public override bool keepWaiting
        {
            get
            {
                return button != null && progress < required;
            }
        }

        public WaitForButtonClick(Button button)
        {
            this.button = button;
            this.required = 1;
            
            if (button != null)
            {
                actButtonClick = Button_OnClick;
                button.onClick.AddListener(actButtonClick);
            }
            else
            {
                Debug.LogError("Button to wait is null!");
            }
        }
        
        public WaitForButtonClick(Button button, int required)
        {
            this.button = button;
            this.required = required;

            if (button != null)
            {
                actButtonClick = Button_OnClick;
                button.onClick.AddListener(actButtonClick);
            }
            else
            {
                Debug.LogError("Button to wait is null!");
            }
        }

        void Button_OnClick()
        {
            progress++;
            if (progress >= required)
            {
                if (button != null)
                {
                    button.onClick.RemoveListener(actButtonClick);
                }
            }
        }
    }
}