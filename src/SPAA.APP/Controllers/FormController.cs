using Microsoft.AspNetCore.Mvc;
using SPAA.APP.Models;
using SPAA.Business.Interfaces;
using SPAA.Data.Context;
using System.Diagnostics;

namespace SPAA.App.Controllers
{
    public class FormController : Controller
    {
        public IActionResult FormAluno()
        {
            ViewData["LayoutType"] = "formulario";
            return View();
        }
    }

}
