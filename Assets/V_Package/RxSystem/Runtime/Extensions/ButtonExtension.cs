using UnityEngine.Events;
using UnityEngine.UI;

namespace ZeroX.RxSystem
{
    public static class ButtonExtension
    {
        public static WaitToken WaitClick(this Button button)
        {
            WaitToken wt = new WaitToken();
            
            UnityAction buttonAction = null;
            buttonAction = () =>
            {
                wt.SetResult();
                button.onClick.RemoveListener(buttonAction);
            };
            
            button.onClick.AddListener(buttonAction);
            return wt;
        }
    }
}