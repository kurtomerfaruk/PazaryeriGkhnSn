using Microsoft.AspNetCore.Mvc;

namespace Pazaryeri.Controllers
{
    public class BaseController:Controller
    {
        protected void SetActiveMenu(string menuName)
        {
            ViewData["ActiveMenu"] = menuName;
        }
    }
}
