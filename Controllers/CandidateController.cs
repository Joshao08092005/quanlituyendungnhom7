using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using System.Linq;
using aznews.Models; 

namespace aznews.Controllers
{
    public class CandidateController : Controller
    {
        // 1. Sửa kiểu dữ liệu thành DataContext (khớp với Program.cs)
        private readonly DataContext _context;

        // 2. Tiêm DataContext vào Constructor
        public CandidateController(DataContext context)
        {
            _context = context;
        }

        // 3. Trang Bảng điều khiển (Việc làm & Lịch sử nộp)
        public async Task<IActionResult> Index()
        {
            var jobs = await _context.Jobs
                .Include(j => j.Company) 
                .Where(j => j.Status == "APPROVED")
                .OrderByDescending(j => j.CreatedAt)
                .Take(10)
                .ToListAsync();

            return View(jobs);
        }

        // 4. Trang Quản lý CV
        public IActionResult MyCV()
        {
            return View();
        }

        // 5. Trang Chỉnh sửa Hồ sơ cá nhân
        public IActionResult Profile()
        {
            return View();
        }

        // 6. Trang hiển thị xác nhận Nộp CV
        [HttpGet]
        public IActionResult ApplyJob(int jobId)
        {
            ViewBag.JobId = jobId; 
            return View();
        }

        // 7. Xử lý lưu thông tin khi bấm nút "Xác nhận Nộp"
        [HttpPost]
        public IActionResult ApplyJob(int jobId, int cvId)
        {
            return RedirectToAction("Index"); 
        }
    }
}