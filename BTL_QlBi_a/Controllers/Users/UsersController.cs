using BTL_QlBi_a.Models.EF;
using Microsoft.AspNetCore.Mvc;

namespace BTL_QlBi_a.Controllers.Users
{
    public class UsersController : Controller
    {
        private readonly ApplicationDbContext _context;

        public UsersController(ApplicationDbContext context)
        {
            _context = context;
        }
        public async Task<IActionResult> Index()
        {
            return View();
        }
    }
}
