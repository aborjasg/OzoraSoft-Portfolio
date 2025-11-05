using OzoraSoft.Library.Enums.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

// ToastNotificationService domain
namespace OzoraSoft.Library.Messaging.UI_Components
{
    public class ToastService
    {
        // Event for showing toasts
        public event Action<enmLogLevel, string, string, string> OnShow;

        // Event for hiding toasts
        public event Action OnHide;
        
        /// <summary>
        /// Shows a generic toast.
        /// </summary>
        public void ShowToast(enmLogLevel type, string messageTitle, string messageSubtitle, string messageBody)
        {
            OnShow?.Invoke(type, messageTitle, messageSubtitle, messageBody);
        }

        public void ShowToast(enmLogLevel type, string messageBody)
        {
            var messageSubtitle = DateTime.Now.ToString(UtilsForMessages.DateTimeFormat_Short);
            OnShow?.Invoke(type, type.ToString(), messageSubtitle, messageBody);
        }

        /// <summary>
        /// Hides the currently displayed toast.
        /// </summary>
        public void HideToast()
        {
            OnHide?.Invoke();
        }                
    }
}
