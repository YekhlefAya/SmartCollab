using Microsoft.AspNetCore.Mvc;

namespace SmartCollab.Controllers;

//public class AuthController : Controller
//{
//    public IActionResult Login() => View();
//    public IActionResult Register() => View();
//    public IActionResult ForgotPassword() => View();
//    public IActionResult ResetPassword() => View();
//}

//public class DashboardController : Controller
//{
//    public IActionResult Index() => View();
//}

//public class ProfileController : Controller
//{
//    //public IActionResult Index() => View();
//    public IActionResult Edit() => View();
//    public IActionResult ChangePassword() => View();
//    public IActionResult Notifications() => View();
//    public IActionResult Favorites() => View();
//}

//public class ProjectsController : Controller
//{
//    public IActionResult Index() => View();
//    public IActionResult Create() => View();
//    public IActionResult Edit(int id) => View();
//    public IActionResult Details(int id) => View();
//    public IActionResult Archive() => View();
//    public IActionResult InviteMember(int id) => View();
//}

public class TasksController : Controller
{
    public IActionResult Index() => RedirectToAction("List");
    //public IActionResult List() => View();
    public IActionResult Kanban() => View();
    public IActionResult Create() => View();
    public IActionResult Edit(int id) => View();
    public IActionResult Details(int id) => View();
}

public class FilesController : Controller
{
    public IActionResult Index() => View();
}

public class NotificationsController : Controller
{
    public IActionResult Index() => View();
}

public class CalendarController : Controller
{
    public IActionResult Index() => View();
}

public class SearchController : Controller
{
    public IActionResult Index() => View();
}

public class ExportController : Controller
{
    public IActionResult Index() => View();
}

public class ErrorsController : Controller
{
    [Route("Errors/403")]
    public IActionResult Forbidden() => View("403");
    [Route("Errors/404")]
    public new IActionResult NotFound() => View("404");
    [Route("Errors/500")]
    public IActionResult ServerError() => View("500");
}
