using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Globalization;
using System.Resources;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;



namespace DuAnRapChieuPhim.Helpers
{
    public static class LocalizationHelper
    {
        private static ResourceManager resourceManager = new ResourceManager("DuAnRapChieuPhim.Resources.resource", typeof(LocalizationHelper).Assembly);

        public static string GetResource(string resourceName)
        {
            // Thử truy xuất với InvariantCulture để kiểm tra tài nguyên mặc định
            string result = resourceManager.GetString(resourceName, CultureInfo.InvariantCulture);

            // Nếu không tìm thấy, thử lại với CultureInfo.CurrentCulture
            if (result == null)
            {
                result = resourceManager.GetString(resourceName, CultureInfo.CurrentCulture);
                if (result == null)
                {
                    throw new Exception($"Không tìm thấy tài nguyên với tên '{resourceName}'");
                }
            }

            return result;
        }
        public static string LocalizedAction(string actionName, string controllerName, object routeValues = null)
        {
            var lang = HttpContext.Current.Request.QueryString["lang"];
            if (string.IsNullOrEmpty(lang))
            {
                lang = "vi"; // Mặc định là tiếng Việt
            }

            var values = new RouteValueDictionary(routeValues);
            values["lang"] = lang; // Thêm tham số lang vào route values

            // Tạo URL dựa trên route values
            return new UrlHelper(HttpContext.Current.Request.RequestContext).Action(actionName, controllerName, values);
        }
    }
}
