using Fool.CardGame.Web.Models;
using Fool.Core.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace Fool.CardGame.Web.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ITableService _tableService;

        public HomeController(ILogger<HomeController> logger, ITableService tableService)
        {
            _logger = logger;
            _tableService = tableService;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }


        [HttpPost]
        public Guid CreateTable()
        {
            var tableId = _tableService.CreateTable();
            return tableId;
        }

        [HttpPost]
        public void JoinTable([FromBody]JoinTableModel model)
        {
            _tableService.SitToTheTable(model.PlayerSecret, model.PlayerName, model.TableGuid);
        }


        [HttpGet]
        public JsonResult GetStatus(string playerSecret)
        {
            var result = _tableService.GetStatus(playerSecret);
            return Json(result);
        }



        [HttpPost]
        public void Attack([FromBody]AttackingModel model)
        {
            _tableService.Attack(model.PlayerSecret, model.PlayerName, model.CardIds);
        }


        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
