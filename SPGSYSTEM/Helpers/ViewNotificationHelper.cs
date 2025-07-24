using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Microsoft.AspNetCore.Mvc.ViewFeatures;

namespace SPGSYSTEM.Helpers
{
    public static class ViewNotificationHelper
    {
        public static void SetViewNotification(this Controller controller, string message, string type = "info")
        {
            var viewKey = $"{controller.ControllerContext.ActionDescriptor.ControllerName}_{controller.ControllerContext.ActionDescriptor.ActionName}";
            var messageKey = $"notification_message_{viewKey}";
            var typeKey = $"notification_type_{viewKey}";
            
            controller.TempData[messageKey] = message;
            controller.TempData[typeKey] = type;
        }

        public static void SetViewSuccess(this Controller controller, string message)
        {
            controller.SetViewNotification(message, "success");
        }

        public static void SetViewError(this Controller controller, string message)
        {
            controller.SetViewNotification(message, "error");
        }

        public static void SetViewWarning(this Controller controller, string message)
        {
            controller.SetViewNotification(message, "warning");
        }

        public static void SetViewInfo(this Controller controller, string message)
        {
            controller.SetViewNotification(message, "info");
        }
    }
} 