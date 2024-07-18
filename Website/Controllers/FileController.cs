using Microsoft.AspNetCore.Mvc;
using Website.Models;
using Website.Services;

namespace Website.Controllers
{
    public class FileController : Controller
    {
        private UploadFile _UploadFile;
        public FileController(UploadFile uploadFile)
        {
            _UploadFile = uploadFile;
        }
        public async Task<IActionResult> Index()
        {
            var data = await _UploadFile.GetAll();
            return View(data);
        }
        public IActionResult Add()
        {
            return View();
        }
        public async Task<IActionResult> Store(mFile data)
        {
            await _UploadFile.Store(data);
            return RedirectToAction("Index");
        }
    }
}
