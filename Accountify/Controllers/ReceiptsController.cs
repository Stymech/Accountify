using Microsoft.AspNetCore.Mvc;

namespace Accountify.Controllers
{
    public class ReceiptsController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
        [HttpPost]
        public JsonResult Delete(int receiptID)
        {
            // Your delete logic here
            Console.WriteLine("Tukilele: " + receiptID);
            return new JsonResult(Ok());
        }
    }
}
