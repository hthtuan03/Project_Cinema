using DuAnRapChieuPhim.Models;
using PagedList;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Web.Mvc;

namespace DuAnRapChieuPhim.Areas.Admin.Controllers
{
    public class AdminController : Controller
    {
        DbDataContext db = new DbDataContext(ConfigurationManager.ConnectionStrings["CinemaConnectionString"].ConnectionString);


        // GET: Admin/Admin
        public ActionResult Login()
        {
            return View();
        }
        public ActionResult Index(int? cinemaId = null, DateTime? filterDate = null)
        {
            ViewBag.Cinemas = db.RapChieus.ToList();
            var model = GetDoanhThuData(cinemaId, filterDate);
            return View(model);
        }

        public ActionResult ThongKeDoanhThu()
        {
            var model = GetDoanhThuData();
            return View(model);
        }

        private DoanhThuViewModel GetDoanhThuData(int? cinemaId = null, DateTime? filterDate = null)
        {
            var doanhThuData = new DoanhThuViewModel
            {
                Labels = new List<string>(),
                TicketData = new List<decimal>(),
                ComboData = new List<decimal>(),
                TicketCountByDay = new List<int>(),
                TicketCountByMonth = new List<int>(),
                TicketCountByYear = new List<int>()
            };

            var query = db.HoaDons.AsQueryable();

            if (cinemaId.HasValue)
            {
                query = query.Where(hd => hd.ChiTietHoaDons.Any(cthd => cthd.LichChieu.MaRap == cinemaId.Value));
            }

            if (filterDate.HasValue)
            {
                query = query.Where(hd =>
                    hd.NgayDat.HasValue &&
                    hd.NgayDat.Value.Year == filterDate.Value.Year &&
                    hd.NgayDat.Value.Month == filterDate.Value.Month);
            }

            var doanhThuList = query.ToList();

            var groupedData = doanhThuList
                .GroupBy(hd => new { hd.NgayDat.Value.Year, hd.NgayDat.Value.Month, hd.NgayDat.Value.Day })
                .OrderBy(g => g.Key.Year).ThenBy(g => g.Key.Month).ThenBy(g => g.Key.Day)
                .ToList();

            foreach (var group in groupedData)
            {
                var yearMonthDay = group.Key;
                var dayLabel = new DateTime(yearMonthDay.Year, yearMonthDay.Month, yearMonthDay.Day).ToString("dd-MM-yyyy");

                doanhThuData.Labels.Add(dayLabel);

                var totalTicketRevenue = group.Sum(hd => hd.TienVe ?? 0);
                var totalComboRevenue = group.Sum(hd => hd.TienCombo ?? 0);

                doanhThuData.TicketData.Add(totalTicketRevenue);
                doanhThuData.ComboData.Add(totalComboRevenue);

                var totalTicketCount = group.Sum(hd => hd.ChiTietHoaDons.Count(cthd => cthd.LichChieu != null));
                doanhThuData.TicketCountByDay.Add(totalTicketCount);

                // Calculate Monthly and Yearly ticket count as well
                var monthlyGroup = doanhThuList
                    .Where(hd => hd.NgayDat.Value.Year == yearMonthDay.Year && hd.NgayDat.Value.Month == yearMonthDay.Month)
                    .Sum(hd => hd.ChiTietHoaDons.Count(cthd => cthd.LichChieu != null));

                var yearlyGroup = doanhThuList
                    .Where(hd => hd.NgayDat.Value.Year == yearMonthDay.Year)
                    .Sum(hd => hd.ChiTietHoaDons.Count(cthd => cthd.LichChieu != null));

                doanhThuData.TicketCountByMonth.Add(monthlyGroup);
                doanhThuData.TicketCountByYear.Add(yearlyGroup);
            }

            return doanhThuData;
        }

        public ActionResult Movie(int? page)
        {
            ViewBag.TL = db.TheLoais.ToList();
            int pageSize = 5; // Số sản phẩm trên mỗi trang
            int pageNumber = (page ?? 1); // Trang mặc định là trang 1 nếu không có tham số page
            var phim = db.Phims.Where(u => !u.TrangThai.Contains("Ngưng chiếu")).ToPagedList(pageNumber, pageSize);

            return View(phim);
        }
        public ActionResult Slider(int? page)
        {
            int pageSize = 5; // Số sản phẩm trên mỗi trang
            int pageNumber = (page ?? 1); // Trang mặc định là trang 1 nếu không có tham số page
            var phim = db.Sliders.ToPagedList(pageNumber, pageSize);

            return View(phim);
        }
        public ActionResult News(int? page)
        {
            int pageSize = 5; // Số sản phẩm trên mỗi trang
            int pageNumber = (page ?? 1); // Trang mặc định là trang 1 nếu không có tham số page
            var phim = db.Tintucs.ToPagedList(pageNumber, pageSize);

            return View(phim);
        }
        public ActionResult TheLoai(int? page)
        {

            int pageSize = 5; // Số sản phẩm trên mỗi trang
            int pageNumber = (page ?? 1); // Trang mặc định là trang 1 nếu không có tham số page
            var phim = db.TheLoais.ToPagedList(pageNumber, pageSize);

            return View(phim);
        }

        public ActionResult Voucher(int? page)
        {

            int pageSize = 5; // Số sản phẩm trên mỗi trang
            int pageNumber = (page ?? 1); // Trang mặc định là trang 1 nếu không có tham số page
            var phim = db.Vouchers.ToPagedList(pageNumber, pageSize);

            return View(phim);
        }
        public ActionResult Feedbacks(int? page)
        {

            int pageSize = 5; // Số sản phẩm trên mỗi trang
            int pageNumber = (page ?? 1); // Trang mặc định là trang 1 nếu không có tham số page
            var phim = db.Feedbacks.ToPagedList(pageNumber, pageSize);

            return View(phim);
        }
        public ActionResult Food(int? page)
        {
            int pageSize = 5; // Số sản phẩm trên mỗi trang
            int pageNumber = (page ?? 1); // Trang mặc định là trang 1 nếu không có tham số page
            var doan = db.DoAns.Where(u => u.TrangThai.Contains("Đang bán")).ToPagedList(pageNumber, pageSize);

            return View(doan);
        }
        public ActionResult Chair(int? page)
        {
            int pageSize = 5; // Số sản phẩm trên mỗi trang
            int pageNumber = (page ?? 1); // Trang mặc định là trang 1 nếu không có tham số page
            var ghe = db.Ghes.ToPagedList(pageNumber, pageSize);

            return View(ghe);
        }
        public ActionResult Combo(int? page)
        {
            ViewBag.DoAn = db.DoAns.ToList();
            ViewBag.Nuoc = db.Nuocs.ToList();
            int pageSize = 5; // Số sản phẩm trên mỗi trang
            int pageNumber = (page ?? 1); // Trang mặc định là trang 1 nếu không có tham số page
            var ghe = db.Combos.Where(u => u.TrangThai.Contains("Đang bán")).ToPagedList(pageNumber, pageSize);

            return View(ghe);
        }
        public ActionResult Nuoc(int? page)
        {
            int pageSize = 5; // Số sản phẩm trên mỗi trang
            int pageNumber = (page ?? 1); // Trang mặc định là trang 1 nếu không có tham số page
            var ghe = db.Nuocs.Where(u => u.TrangThai.Contains("Đang bán")).ToPagedList(pageNumber, pageSize);

            return View(ghe);
        }
        public ActionResult Bill(int? page)
        {
            int pageSize = 5; // Số sản phẩm trên mỗi trang
            int pageNumber = (page ?? 1); // Trang mặc định là trang 1 nếu không có tham số page
            var hoadon = db.HoaDons.Where(u => u.TrangThai.Contains("Đã thanh toán")).ToPagedList(pageNumber, pageSize);

            return View(hoadon);
        }
        private string HashSHA256(string input)
        {
            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(input));

                StringBuilder builder = new StringBuilder();
                for (int i = 0; i < bytes.Length; i++)
                {
                    builder.Append(bytes[i].ToString("x2"));
                }

                return builder.ToString();
            }
        }
        public ActionResult DetailsHD(long id)
        {
            var cthd = from chiTiet in db.ChiTietHoaDons
                       join lichChieu in db.LichChieus on chiTiet.MaLichChieu equals lichChieu.MaChieuPhim
                       join ghe in db.Ghes on chiTiet.MaGhe equals ghe.MaGhe
                       join phong in db.Phongs on ghe.MaPhong equals phong.MaPhong
                       join phim in db.Phims on lichChieu.MaPhim equals phim.MaPhim
                       where chiTiet.MaHoaDon == id
                       select new CTHD
                       {
                           chiTiet = chiTiet,
                           lichChieu = lichChieu,
                           ghe = ghe,
                           phong = phong,
                           phim = phim
                       };
            var cthdcb = from hdcb in db.CTHDComBos
                         join cb in db.Combos on hdcb.MaCB equals cb.MaCB
                         join nuoc in db.Nuocs on cb.MaNuoc equals nuoc.MaNuoc
                         join doan in db.DoAns on cb.MaDoAn equals doan.MaDoAn
                         where hdcb.MaHoaDon == id
                         select new CTHDCB
                         {
                             cthd = hdcb,
                             cb = cb,
                             nuoc = nuoc,
                             doan = doan
                         };
            var hd = db.HoaDons.SingleOrDefault(u => u.MaHoaDon == id);
            ViewBag.hd = hd;
            ViewBag.cb = cthdcb.ToList();
            ViewBag.ghe = cthd.ToList();
            string orderCode = $"HD{id}"; // Mã đơn hàng

            ViewBag.QRCode = QRCodeGenerator.GenerateQRCode(orderCode);
            return View();
        }
        [HttpPost]
        public ActionResult Login(string taikhoan, string password)
        {
            if (string.IsNullOrEmpty(taikhoan) || string.IsNullOrEmpty(password))
            {
                // Handle empty username or password
                return RedirectToAction("Login");
            }

            // Hash the password using a secure hashing algorithm before comparing it with the database
            string hashedPassword = HashSHA256(password);

            //Check if the user exists in the database
            var user = db.TaiKhoans
                .FirstOrDefault(u => u.Username == taikhoan && u.Password == hashedPassword);

            if (user != null)
            {
                // User authenticated, you can set authentication cookies or session variables here
                // For example, you can use ASP.NET Core Identity for more advanced authentication features
                return RedirectToAction("Index"); // Redirect to a success page
            }
            else
            {
                // User not found or password doesn't match
                // You may want to handle incorrect login attempts or display an error message
                return RedirectToAction("Login");
            }
        }
        public ActionResult Showtimes(int? page)
        {
            ViewBag.Phim = db.Phims.ToList();
            ViewBag.Phong = db.Phongs.ToList();
            int pageSize = 5; // Số sản phẩm trên mỗi trang
            int pageNumber = (page ?? 1); // Trang mặc định là trang 1 nếu không có tham số page
            var lichchieu = db.LichChieus.ToPagedList(pageNumber, pageSize);

            return View(lichchieu);
        }
        public ActionResult Room(int? page)
        {
            int pageSize = 5; // Số sản phẩm trên mỗi trang
            int pageNumber = (page ?? 1); // Trang mặc định là trang 1 nếu không có tham số page
            var phong = db.Phongs.Where(u => u.TrangThai.Contains("Hoạt động")).ToPagedList(pageNumber, pageSize);

            return View(phong);
        }
        public ActionResult Account(int? page)
        {
            int pageSize = 5; // Số sản phẩm trên mỗi trang
            int pageNumber = (page ?? 1); // Trang mặc định là trang 1 nếu không có tham số page
            var taikhoan = db.TaiKhoans.ToPagedList(pageNumber, pageSize);

            return View(taikhoan);
        }
        public ActionResult Infor(int? page)
        {
            int pageSize = 5; // Số sản phẩm trên mỗi trang
            int pageNumber = (page ?? 1); // Trang mặc định là trang 1 nếu không có tham số page
            var thongtin = db.ThongTinCaNhans.ToPagedList(pageNumber, pageSize);

            return View(thongtin);
        }

        [HttpPost]
        public ActionResult FindMovie(int? page, FormCollection fr)
        {
            string movie_name = fr["movie_search"];
            int pageSize = 5;
            int pageNumber = (page ?? 1);
            var phimQuery = db.Phims.Where(u => !u.TrangThai.Contains("Ngưng chiếu"));
            if (!string.IsNullOrEmpty(movie_name))
            {
                phimQuery = phimQuery.Where(p => p.TenPhim.Contains(movie_name));
            }
            var phim = phimQuery.ToPagedList(pageNumber, pageSize);
            return View("Movie", phim);
        }
        [HttpPost]
        public ActionResult FindTheLoai(int? page, FormCollection fr)
        {
            string theloai_name = fr["movie_search"];
            int pageSize = 5;
            int pageNumber = (page ?? 1);
            var phimQuery = db.TheLoais.Where(x => x.MaTheLoai != 0);
            if (!string.IsNullOrEmpty(theloai_name))
            {
                phimQuery = phimQuery.Where(p => p.TenTheLoai.Contains(theloai_name));
            }
            var phim = phimQuery.ToPagedList(pageNumber, pageSize);
            return View("TheLoai", phim);
        }

        [HttpPost]
        public ActionResult FindVoucher(int? page, FormCollection fr)
        {
            string voucher_name = fr["movie_search"];
            int pageSize = 5;
            int pageNumber = (page ?? 1);
            var phimQuery = db.Vouchers.Where(x => x.ID != "");
            if (!string.IsNullOrEmpty(voucher_name))
            {
                phimQuery = phimQuery.Where(p => p.MaVoucher.Contains(voucher_name));
            }
            var phim = phimQuery.ToPagedList(pageNumber, pageSize);
            return View("Voucher", phim);
        }
        [HttpPost]
        public ActionResult FindFeedback(int? page, FormCollection fr)
        {
            string feedback_name = fr["movie_search"];
            int pageSize = 5;
            int pageNumber = (page ?? 1);
            var phimQuery = db.Feedbacks.Where(x => x.Id != 0);
            if (!string.IsNullOrEmpty(feedback_name))
            {
                phimQuery = phimQuery.Where(p => p.Name.Contains(feedback_name) || p.Email.Contains(feedback_name));
            }
            var phim = phimQuery.ToPagedList(pageNumber, pageSize);
            return View("Feedbacks", phim);
        }
        [HttpPost]
        public ActionResult FindFood(int? page, FormCollection fr)
        {
            string food_name = fr["food_search"];
            int pageSize = 5;
            int pageNumber = (page ?? 1);
            var foodQuery = db.DoAns.Where(u => u.TrangThai.Contains("Đang bán"));
            if (!string.IsNullOrEmpty(food_name))
            {
                foodQuery = foodQuery.Where(p => p.TenDoAn.Contains(food_name));
            }
            var food = foodQuery.ToPagedList(pageNumber, pageSize);
            return View("Food", food);
        }
        [HttpPost]
        public ActionResult FindDrink(int? page, FormCollection fr)
        {
            string drink_name = fr["drink_search"];
            int pageSize = 5;
            int pageNumber = (page ?? 1);
            var drinkQuery = db.Nuocs.Where(u => u.TrangThai.Contains("Đang bán"));
            if (!string.IsNullOrEmpty(drink_name))
            {
                drinkQuery = drinkQuery.Where(p => p.TenNuoc.Contains(drink_name));
            }
            var drink = drinkQuery.ToPagedList(pageNumber, pageSize);
            return View("Nuoc", drink);
        }
        [HttpPost]
        public ActionResult FindRoom(int? page, FormCollection fr)
        {
            string drink_name = fr["room_search"];
            int pageSize = 5;
            int pageNumber = (page ?? 1);
            var roomQuery = db.Phongs.Where(u => u.TrangThai.Contains("Hoạt động"));
            if (!string.IsNullOrEmpty(drink_name))
            {
                roomQuery = roomQuery.Where(p => p.TenPhong.Contains(drink_name));
            }
            var room = roomQuery.ToPagedList(pageNumber, pageSize);
            return View("Room", room);
        }
        public ActionResult Theatre(int? page)
        {
            var cumList = db.RapChieus.Select(r => r.Cum).Distinct().ToList();
            // Truyền danh sách cụm vào View
            ViewBag.CumList = cumList;
            int pageSize = 5; // Số sản phẩm trên mỗi trang
            int pageNumber = (page ?? 1); // Trang mặc định là trang 1 nếu không có tham số page
            string district = Request.QueryString["district"];
            var rap = db.RapChieus.Where(r => r.Cum == district).ToPagedList(pageNumber, pageSize);
            return View(rap);
        }
        public ActionResult Comment(int? id)
        {
            int? maphim;
            if (id != null)
            {
                maphim = id;
            }
            else
            {
                maphim = int.Parse(Request.QueryString["id"]);
            }
            var result = db.BinhLuans.Where(c => c.MaPhim == maphim).ToList();
            return View(result);
        }
        public ActionResult GroupofTheater()
        {
            var groupedData = db.RapChieus
            .GroupBy(p => p.Cum)
            .Select(g => g.Key).ToList();
            return View(groupedData);
        }
    }
}