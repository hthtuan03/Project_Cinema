using DuAnRapChieuPhim.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.NetworkInformation;
using System.Web;
using System.Web.Mvc;
using static System.Net.WebRequestMethods;
using System.Web.Routing;
using System.Text.RegularExpressions;
using System.Security.Cryptography;
using System.Text;
using System.Configuration;

namespace DuAnRapChieuPhim.Areas.Admin.Controllers
{
    public class XuLyController : Controller
    {
        DbDataContext db = new DbDataContext(ConfigurationManager.ConnectionStrings["CinemaConnectionString"].ConnectionString);

        // GET: Admin/XuLy
        public JsonResult EditFood(int id)
        {
            Session["MaDoAn"] = id;
            var food = db.DoAns.SingleOrDefault(u => u.MaDoAn == id);
            var foodData = new
            {
                MaDoAn = food.MaDoAn,
                TenDoAn = food.TenDoAn,
                Gia = food.Gia,
                Loai = food.Loai
            };
            return Json(new { success = true, food = foodData }, JsonRequestBehavior.AllowGet);
        }
        public JsonResult EditTL(int id)
        {
            Session["MaTL"] = id;
            var food = db.TheLoais.SingleOrDefault(u => u.MaTheLoai == id);
            var foodData = new
            {
                MaTheLoai = food.MaTheLoai,
                TenTheLoai = food.TenTheLoai
            };
            return Json(new { success = true, TL = foodData }, JsonRequestBehavior.AllowGet);
        }

        public JsonResult EditVC(string id)
        {
            Session["MaVC"] = id;
            var food = db.Vouchers.SingleOrDefault(u => u.ID == id);
            return Json(new { success = true, TL = food }, JsonRequestBehavior.AllowGet);
        }
        public JsonResult EditNuoc(int id)
        {
            Session["MaNuoc"] = id;
            var food = db.Nuocs.SingleOrDefault(u => u.MaNuoc == id);
            var foodData = new
            {
                MaNuoc = food.MaNuoc,
                TenNuoc = food.TenNuoc,
                Gia = food.Gia,
                Loai = food.Loai
            };
            return Json(new { success = true, Nuoc = foodData }, JsonRequestBehavior.AllowGet);
        }
        public JsonResult EditCB(int id)
        {
            Session["MaCB"] = id;
            var food = db.Combos.SingleOrDefault(u => u.MaCB == id);
            var foodData = new
            {
                TenCombo = food.TenCB,
                Gia = food.Gia,
                MoTa = food.MoTa,
                MaDoAn = food.MaDoAn,
                MaNuoc = food.MaNuoc,
                SoLuongDA = food.SLDoAn,
                SoLuongNuoc = food.SLNuoc,
                GhiChu = food.GhiChu,
            };
            return Json(new { success = true, CB = foodData }, JsonRequestBehavior.AllowGet);
        }
        public JsonResult EditRoom(int id)
        {
            Session["MaPhong"] = id;

            var phong = db.Phongs.SingleOrDefault(u => u.MaPhong == id);

            if (phong == null)
            {
                return Json(new { success = false, message = "Phòng không tồn tại." });
            }

            var phongData = new
            {
                MaPhong = phong.MaPhong,
                TenPhong = phong.TenPhong,
                SoChoNgoi = phong.SoChoNgoi
            };

            return Json(new { success = true, phong = phongData }, JsonRequestBehavior.AllowGet);
        }
        public JsonResult EditShowTime(int id)
        {
            Session["MaLichChieu"] = id;
            var food = db.LichChieus.SingleOrDefault(u => u.MaChieuPhim == id);
            var foodData = new
            {
                NgonNgu = food.NgonNgu,
                GioChieu = food.GioChieu.ToString(), // Convert TimeSpan to DateTime and then format
                GioKetThuc = food.GioKetThuc.ToString(),
                Phong = food.Phong,
                MaPhim = food.MaPhim,
                NgayChieu = food.NgayChieu
            };
            return Json(new { success = true, lc = foodData }, JsonRequestBehavior.AllowGet);
        }

        public JsonResult EditInfor(int id)
        {
            var infor = db.ThongTinCaNhans.SingleOrDefault(u => u.MaKH == id);
            Session["MaKH"] = id;
            if (infor == null)
            {
                return Json(new { success = false, message = "Khách hàng không tồn tại." });
            }

            var inforData = new
            {
                MaKH = infor.MaKH,
                Ten = infor.Ten,
                SDT = infor.SDT,
                Email = infor.Email,
                ThongTinChiTieu = infor.ThongTinChiTieu,
                CapDoThe = infor.CapDoThe,
                MaThe = infor.MaThe
            };

            return Json(new { success = true, infor = inforData }, JsonRequestBehavior.AllowGet);
        }
        public JsonResult EditAccount(string id)
        {
            var tk = db.TaiKhoans.SingleOrDefault(u => u.Username == id);

            if (tk == null)
            {
                return Json(new { success = false, message = "Khách hàng không tồn tại." });
            }

            var tkData = new
            {
                Username = tk.Username,
                Password = tk.Password,
                ChucDanh = tk.ChucDanh
            };

            return Json(new { success = true, tk = tkData }, JsonRequestBehavior.AllowGet);
        }
        public JsonResult EditChair(int id)
        {
            Session["MaGhe"] = id;

            var ghe = db.Ghes.SingleOrDefault(u => u.MaGhe == id);

            if (ghe == null)
            {
                return Json(new { success = false, message = "Khách hàng không tồn tại." });
            }

            var tkData = new
            {
                MaGhe = ghe.MaGhe,
                TrangThai = ghe.TrangThai,
                MaPhong = ghe.MaPhong,
                TenGhe = ghe.TenGhe,
                LoaiGhe = ghe.LoaiGhe,
                Gia = ghe.GiaTien
            };

            return Json(new { success = true, ghe = tkData }, JsonRequestBehavior.AllowGet);
        }
        public JsonResult EditMovie(int id)
        {
            Session["MaPhim"] = id;
            var phim = db.Phims.SingleOrDefault(u => u.MaPhim == id);

            if (phim == null)
            {
                return Json(new { success = false, message = "Khách hàng không tồn tại." });
            }

            var phimData = new
            {
                MaPhim = phim.MaPhim,
                TenPhim = phim.TenPhim,
                Ngay = phim.NgayRaMat,
                TrangThai = phim.TrangThai,
                TheLoai = phim.TheLoai,
                ThoiLuong = phim.ThoiLuong,
                DienVien = phim.DienVien,
                DaoDien = phim.DaoDien,
                NgonNgu = phim.NgonNgu,
                Video = phim.Video,
            };

            return Json(new { success = true, phim = phimData }, JsonRequestBehavior.AllowGet);
        }
        public JsonResult EditBill(int id)
        {
            Session["MaHoaDon"] = id;

            var hoaDon = db.HoaDons.SingleOrDefault(u => u.MaHoaDon == id);

            if (hoaDon == null)
            {
                return Json(new { success = false, message = "Khách hàng không tồn tại." });
            }

            var hoaDonData = new
            {
                MaHoaDon = hoaDon.MaHoaDon,
                MaKH = hoaDon.MaKH,
                HoTen = hoaDon.HoTen,
                SoDienThoai = hoaDon.SoDienThoai,
                Email = hoaDon.Email,

            };

            return Json(new { success = true, hoaDon = hoaDonData }, JsonRequestBehavior.AllowGet);
        }
        public JsonResult Editnew(int id)
        {
            Session["Matin"] = id;

            var hoaDon = db.Tintucs.SingleOrDefault(u => u.Matin == id);

            if (hoaDon == null)
            {
                return Json(new { success = false, message = "Khách hàng không tồn tại." });
            }

            var hoaDonData = new
            {
                Ten = hoaDon.Chudetin,
                batdau = hoaDon.Ngaybatdau,
                kt = hoaDon.Ngayketthuc,
                mt = hoaDon.Mota,
                dk = hoaDon.DieuKien,
                time = hoaDon.ThoiHan
            };

            return Json(new { success = true, tk = hoaDonData }, JsonRequestBehavior.AllowGet);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult AddDoAn(FormCollection formCollection)
        {
            string TenDoAn = formCollection["TenDoAn"];
            decimal Gia = Convert.ToDecimal(formCollection["Gia"]);
            string Loai = formCollection["Loai"];
            DoAn d = new DoAn
            {
                TenDoAn = TenDoAn,
                Gia = Gia,
                Loai = Loai
            };

            db.DoAns.InsertOnSubmit(d);
            db.SubmitChanges();
            // Có thể xử lý hình đại diện ở đây nếu cần.


            return RedirectToAction("Food", "Admin", new { area = "Admin" });
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult AddAccount(FormCollection formCollection)
        {
            string u = formCollection["UserName"];
            string p = formCollection["Password"];
            string cv = formCollection["ChucDanh"];
            TaiKhoan tk = new TaiKhoan
            {
                Username = u,
                Password = HashSHA256(p),
                ChucDanh = cv
            };
            db.TaiKhoans.InsertOnSubmit(tk);
            db.SubmitChanges();
            // Có thể xử lý hình đại diện ở đây nếu cần.


            return RedirectToAction("Account", "Admin", new { area = "Admin" });
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
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult AddNuoc(FormCollection formCollection)
        {
            string TenNuoc = formCollection["TenNuoc"];
            decimal Gia = Convert.ToDecimal(formCollection["Gia"]);
            string Loai = formCollection["Loai"];
            Nuoc d = new Nuoc
            {
                TenNuoc = TenNuoc,
                Gia = Gia,
                Loai = Loai,
                TrangThai = "Đang bán"
            };

            db.Nuocs.InsertOnSubmit(d);
            db.SubmitChanges();
            // Có thể xử lý hình đại diện ở đây nếu cần.


            return RedirectToAction("Nuoc", "Admin", new { area = "Admin" });
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult AddTL(FormCollection formCollection)
        {
            string TenTheLoai = formCollection["TenTheLoai"];
            TheLoai d = new TheLoai
            {
                TenTheLoai = TenTheLoai
            };

            db.TheLoais.InsertOnSubmit(d);
            db.SubmitChanges();
            // Có thể xử lý hình đại diện ở đây nếu cần.


            return RedirectToAction("TheLoai", "Admin", new { area = "Admin" });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult AddVC(FormCollection formCollection)
        {
            string MaVoucher = formCollection["MaVoucher"];

            DateTime NgayHetHan;
            if (!DateTime.TryParse(formCollection["NgayHetHan"], out NgayHetHan))
            {
            }

            decimal GiamGiaPhanTram;
            if (!decimal.TryParse(formCollection["GiamGiaPhanTram"], out GiamGiaPhanTram))
            {
            }

            int MaxLuotSD;
            if (!int.TryParse(formCollection["MaxLuotSD"], out MaxLuotSD))
            {
            }

            int SoLuotDaSD;
            if (!int.TryParse(formCollection["SoLuotDaSD"], out SoLuotDaSD))
            {
            }

            bool TrangThai;
            if (!bool.TryParse(formCollection["TrangThai"], out TrangThai))
            {
            }

            Voucher d = new Voucher
            {
                ID = Guid.NewGuid().ToString(),
                MaVoucher = MaVoucher,
                NgayHetHan = NgayHetHan,
                GiamGiaPhanTram = GiamGiaPhanTram,
                MaxLuotSD = MaxLuotSD,
                SoLuotDaSD = SoLuotDaSD,
                TrangThai = TrangThai
            };

            db.Vouchers.InsertOnSubmit(d);
            db.SubmitChanges();

            return RedirectToAction("Voucher", "Admin", new { area = "Admin" });
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult AddCB(FormCollection formCollection)
        {
            int MaDoAn = Convert.ToInt32(formCollection["MaDoAn"]);
            int MaNuoc = Convert.ToInt32(formCollection["MaNuoc"]);
            int SoLuongDA = Convert.ToInt32(formCollection["SoLuongDA"]);
            int SoLuongNuoc = Convert.ToInt32(formCollection["SoLuongNuoc"]);
            string TenCombo = formCollection["TenCombo"];
            string GhiChu = formCollection["GhiChu"];
            decimal Gia = Convert.ToDecimal(formCollection["Gia"]);
            string Mota = formCollection["Loai"];
            var anhBia = Request.Files["HinhAnh"];
            string imagePath = SaveImage(anhBia);
            Combo d = new Combo
            {
                TenCB = TenCombo,
                MoTa = Mota,
                Gia = Gia,
                MaDoAn = MaDoAn,
                MaNuoc = MaNuoc,
                SLDoAn = SoLuongDA,
                SLNuoc = SoLuongNuoc,
                HinhAnh = imagePath,
                GhiChu = GhiChu,
                TrangThai = "Đang bán"
            };

            db.Combos.InsertOnSubmit(d);
            db.SubmitChanges();
            // Có thể xử lý hình đại diện ở đây nếu cần.


            return RedirectToAction("Combo", "Admin", new { area = "Admin" });
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult AddSlider(FormCollection formCollection)
        {
            string Ten = formCollection["Name"];
            var anhBia = Request.Files["HinhAnh"];
            string imagePath = SaveImage(anhBia);
            Slider d = new Slider
            {
                TenHinh = Ten,
                HinhAnh = imagePath,
            };

            db.Sliders.InsertOnSubmit(d);
            db.SubmitChanges();
            // Có thể xử lý hình đại diện ở đây nếu cần.


            return RedirectToAction("Slider", "Admin", new { area = "Admin" });
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Addnew(FormCollection formCollection)
        {
            string Ten = formCollection["Ten"];
            DateTime batdau = DateTime.Parse(formCollection["batdau"]);
            DateTime kt = DateTime.Parse(formCollection["kt"]);
            string mt = formCollection["mt"];
            string dk = formCollection["dk"];
            string time = formCollection["time"];
            var anhBia = Request.Files["HinhAnh"];
            string imagePath = SaveImage(anhBia);
            Tintuc d = new Tintuc
            {
                Chudetin = Ten,
                Ngaybatdau = batdau,
                Ngayketthuc = kt,
                Mota = mt,
                DieuKien = dk,
                ThoiHan = time,
                Anhbia = imagePath,
            };

            db.Tintucs.InsertOnSubmit(d);
            db.SubmitChanges();
            // Có thể xử lý hình đại diện ở đây nếu cần.


            return RedirectToAction("News", "Admin", new { area = "Admin" });
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Editnew(FormCollection formCollection)
        {
            string Ten = formCollection["Ten"];
            DateTime batdau = DateTime.Parse(formCollection["batdau"]);
            DateTime kt = DateTime.Parse(formCollection["kt"]);
            string mt = formCollection["mt"];
            string dk = formCollection["dk"];
            string time = formCollection["time"];
            var anhBia = Request.Files["HinhAnh"];
            string imagePath = SaveImage(anhBia);
            int id = (int)Session["Matin"];
            Tintuc d = db.Tintucs.SingleOrDefault(u => u.Matin == id);


            d.Chudetin = Ten;
            d.Ngaybatdau = batdau;
            d.Ngayketthuc = kt;
            d.Mota = mt;
            d.DieuKien = dk;
            d.ThoiHan = time;
            if (anhBia != null && anhBia.ContentLength > 0)
            {
                d.Anhbia = imagePath;

            }
            db.SubmitChanges();
            // Có thể xử lý hình đại diện ở đây nếu cần.


            return RedirectToAction("News", "Admin", new { area = "Admin" });
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EditCB(FormCollection formCollection)
        {
            int MaDoAn = Convert.ToInt32(formCollection["MaDoAn"]);
            int MaNuoc = Convert.ToInt32(formCollection["MaNuoc"]);
            int SoLuongDA = Convert.ToInt32(formCollection["SoLuongDA"]);
            int SoLuongNuoc = Convert.ToInt32(formCollection["SoLuongNuoc"]);
            string TenCombo = formCollection["TenCombo"];
            decimal Gia = Convert.ToDecimal(formCollection["Gia"]);
            string Mota = formCollection["Loai"];
            var anhBia = Request.Files["HinhAnh"];
            string GhiChu = formCollection["GhiChu"];
            string imagePath = SaveImage(anhBia);

            // Xử lý các bước cập nhật thông tin đối tượng
            int id = 0; // Giá trị mặc định nếu Session["MaDoAn"] là null
            object maDoAnObj = Session["MaCB"];
            if (maDoAnObj != null && maDoAnObj is int)
            {
                id = (int)maDoAnObj;
            }
            Combo existingDoAn = db.Combos.SingleOrDefault(d => d.MaCB == id);

            if (existingDoAn == null)
            {
                return HttpNotFound();
            }
            existingDoAn.GhiChu = GhiChu;
            existingDoAn.TenCB = TenCombo;
            existingDoAn.MoTa = Mota;
            existingDoAn.Gia = Gia;
            existingDoAn.MaDoAn = MaDoAn;
            existingDoAn.MaNuoc = MaNuoc;
            existingDoAn.SLDoAn = SoLuongDA;
            existingDoAn.SLNuoc = SoLuongNuoc;
            if (anhBia != null && anhBia.ContentLength > 0)
            {
                existingDoAn.HinhAnh = imagePath;
            }

            db.SubmitChanges();

            // Chuyển hướng đến trang chi tiết hoặc danh sách đối tượng
            return RedirectToAction("Combo", "Admin", new { area = "Admin" });
        }

        private string ConvertToEmbedLink(string youtubeLink)
        {
            var regex = new Regex(@"(?:https?://)?(?:www\.)?(?:youtube\.com/(?:[^/\n\s]+/\S+/|(?:v|e(?:mbed)?)/|\S*?[?&]v=)|youtu\.be/)([a-zA-Z0-9_-]{11})");

            var match = regex.Match(youtubeLink);

            if (match.Success)
            {
                // Chuyển đổi thành định dạng embed link
                var videoId = match.Groups[1].Value;
                return "https://www.youtube.com/embed/" + videoId;
            }

            // Trả về chuỗi rỗng nếu không thành công
            return string.Empty;
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult AddMovie(FormCollection formCollection)
        {
            string TenPhim = formCollection["TenPhim"];
            DateTime Ngay = DateTime.Parse(formCollection["Ngay"]);
            string TrangThai = formCollection["TrangThai"];
            string[] TheLoai = formCollection.GetValues("TheLoai");
            string theLoai = string.Join(",", TheLoai);
            int ThoiLuong = Convert.ToInt32(formCollection["ThoiLuong"]);
            string DienVien = formCollection["DienVien"];
            string DaoDien = formCollection["DaoDien"];
            string NgonNgu = formCollection["NgonNgu"];
            var anhBia = Request.Files["AnhBia"];
            string Video = ConvertToEmbedLink(formCollection["Video"]);
            string imagePath = SaveImage(anhBia);
            Phim d = new Phim
            {
                TenPhim = TenPhim,
                NgayRaMat = Ngay,
                TrangThai = TrangThai,
                TheLoai = theLoai,
                DaoDien = DaoDien,
                DienVien = DienVien,
                ThoiLuong = ThoiLuong,
                NgonNgu = NgonNgu,
                Video = Video,
                AnhBia = imagePath
            };

            db.Phims.InsertOnSubmit(d);
            db.SubmitChanges();
            // Có thể xử lý hình đại diện ở đây nếu cần.


            return RedirectToAction("Movie", "Admin", new { area = "Admin" });
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EditMovie(FormCollection formCollection)
        {
            string TenPhim = formCollection["TenPhim"];
            DateTime Ngay = DateTime.Parse(formCollection["Ngay"]);
            string TrangThai = formCollection["TrangThai"];
            string[] TheLoai = formCollection.GetValues("TheLoai");
            string theLoai = string.Join(",", TheLoai);
            int ThoiLuong = Convert.ToInt32(formCollection["ThoiLuong"]);
            string DienVien = formCollection["DienVien"];
            string DaoDien = formCollection["DaoDien"];
            string NgonNgu = formCollection["NgonNgu"];
            var anhBia = Request.Files["AnhBia"];
            string Video = ConvertToEmbedLink(formCollection["Video"]);
            string imagePath = SaveImage(anhBia);

            // Xử lý các bước cập nhật thông tin đối tượng
            int id = 0; // Giá trị mặc định nếu Session["MaDoAn"] là null
            object maDoAnObj = Session["MaPhim"];
            if (maDoAnObj != null && maDoAnObj is int)
            {
                id = (int)maDoAnObj;
            }
            Phim existingDoAn = db.Phims.SingleOrDefault(d => d.MaPhim == id);

            if (existingDoAn == null)
            {
                return HttpNotFound();
            }
            existingDoAn.TenPhim = TenPhim;
            existingDoAn.NgayRaMat = Ngay;
            existingDoAn.TrangThai = TrangThai;
            existingDoAn.TheLoai = theLoai;
            existingDoAn.ThoiLuong = ThoiLuong;
            existingDoAn.DienVien = DienVien;
            existingDoAn.DaoDien = DaoDien;
            existingDoAn.NgonNgu = NgonNgu;
            existingDoAn.Video = Video;
            if (anhBia != null && anhBia.ContentLength > 0)
            {
                existingDoAn.AnhBia = imagePath;
            }

            db.SubmitChanges();

            // Chuyển hướng đến trang chi tiết hoặc danh sách đối tượng
            return RedirectToAction("Movie", "Admin", new { area = "Admin" });
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EditAction(FormCollection formCollection)
        {
            // Lấy giá trị từ form
            string tenDoAn = formCollection["TenDoAn"];
            decimal gia = Convert.ToDecimal(formCollection["Gia"]);
            string loai = formCollection["Loai"];

            // Xử lý các bước cập nhật thông tin đối tượng
            int id = 0; // Giá trị mặc định nếu Session["MaDoAn"] là null
            object maDoAnObj = Session["MaDoAn"];
            if (maDoAnObj != null && maDoAnObj is int)
            {
                id = (int)maDoAnObj;
            }
            DoAn existingDoAn = db.DoAns.SingleOrDefault(d => d.MaDoAn == id);

            if (existingDoAn == null)
            {
                return HttpNotFound();
            }

            existingDoAn.TenDoAn = tenDoAn;
            existingDoAn.Gia = gia;
            existingDoAn.Loai = loai;

            db.SubmitChanges();

            // Chuyển hướng đến trang chi tiết hoặc danh sách đối tượng
            return RedirectToAction("Food", "Admin", new { area = "Admin" });
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EditTL(FormCollection formCollection)
        {
            // Lấy giá trị từ form
            string tenDoAn = formCollection["TenTheLoai"];

            // Xử lý các bước cập nhật thông tin đối tượng
            int id = 0; // Giá trị mặc định nếu Session["MaDoAn"] là null
            object maDoAnObj = Session["MaTL"];
            if (maDoAnObj != null && maDoAnObj is int)
            {
                id = (int)maDoAnObj;
            }
            TheLoai existingDoAn = db.TheLoais.SingleOrDefault(d => d.MaTheLoai == id);

            if (existingDoAn == null)
            {
                return HttpNotFound();
            }

            existingDoAn.TenTheLoai = tenDoAn;

            db.SubmitChanges();

            // Chuyển hướng đến trang chi tiết hoặc danh sách đối tượng
            return RedirectToAction("TheLoai", "Admin", new { area = "Admin" });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EditVC(FormCollection formCollection)
        {
            // Lấy giá trị từ form
            string MaVoucher = formCollection["MaVoucher"];

            DateTime NgayHetHan;
            if (!DateTime.TryParse(formCollection["NgayHetHan"], out NgayHetHan))
            {
            }

            decimal GiamGiaPhanTram;
            if (!decimal.TryParse(formCollection["GiamGiaPhanTram"], out GiamGiaPhanTram))
            {
            }

            int MaxLuotSD;
            if (!int.TryParse(formCollection["MaxLuotSD"], out MaxLuotSD))
            {
            }

            int SoLuotDaSD;
            if (!int.TryParse(formCollection["SoLuotDaSD"], out SoLuotDaSD))
            {
            }

            bool TrangThai;
            if (!bool.TryParse(formCollection["TrangThai"], out TrangThai))
            {
            }
            // Xử lý các bước cập nhật thông tin đối tượng
            string id = null; // Giá trị mặc định nếu Session["MaDoAn"] là null
            object maDoAnObj = Session["MaVC"];
            if (maDoAnObj != null && maDoAnObj is string)
            {
                id = (string)maDoAnObj;
            }
            Voucher existingDoAn = db.Vouchers.SingleOrDefault(d => d.ID.Equals(maDoAnObj));

            if (existingDoAn == null)
            {
                return HttpNotFound();
            }

            existingDoAn.MaVoucher = MaVoucher;
            existingDoAn.NgayHetHan = NgayHetHan;
            existingDoAn.GiamGiaPhanTram = GiamGiaPhanTram;
            existingDoAn.MaxLuotSD = MaxLuotSD;
            existingDoAn.SoLuotDaSD = SoLuotDaSD;
            existingDoAn.TrangThai = TrangThai;

            db.SubmitChanges();

            // Chuyển hướng đến trang chi tiết hoặc danh sách đối tượng
            return RedirectToAction("Voucher", "Admin", new { area = "Admin" });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EditAccount(FormCollection formCollection)
        {
            // Lấy giá trị từ form
            string cv = formCollection["ChucDanh"];
            string u = formCollection["UserName"] != null ? formCollection["UserName"].ToString() : "";
            // Xử lý các bước cập nhật thông tin đối tượng
            TaiKhoan existingDoAn = db.TaiKhoans.SingleOrDefault(tk => tk.Username.Contains(u));

            if (existingDoAn == null)
            {
                return HttpNotFound();
            }

            existingDoAn.ChucDanh = cv;

            db.SubmitChanges();

            // Chuyển hướng đến trang chi tiết hoặc danh sách đối tượng
            return RedirectToAction("Account", "Admin", new { area = "Admin" });
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EditKH(FormCollection formCollection)
        {
            // Lấy giá trị từ form
            string t = formCollection["Ten"];
            string sdt = formCollection["SDT"];
            string e = formCollection["Email"];
            string ct = formCollection["ThongTinChiTieu"];
            int cd = int.TryParse(formCollection["CapDoThe"], out cd) ? cd : 0;
            string mt = formCollection["MaThe"];

            int id = (int)Session["MaKH"];
            // Xử lý các bước cập nhật thông tin đối tượng
            ThongTinCaNhan existingDoAn = db.ThongTinCaNhans.SingleOrDefault(tk => tk.MaKH == id);

            if (existingDoAn == null)
            {
                return HttpNotFound();
            }
            existingDoAn.Ten = t;
            existingDoAn.SDT = sdt;
            existingDoAn.Email = e;

            db.SubmitChanges();

            // Chuyển hướng đến trang chi tiết hoặc danh sách đối tượng
            return RedirectToAction("Infor", "Admin", new { area = "Admin" });
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EditNuoc(FormCollection formCollection)
        {
            // Lấy giá trị từ form
            string tenNuoc = formCollection["TenNuoc"];
            decimal gia = Convert.ToDecimal(formCollection["Gia"]);
            string loai = formCollection["Loai"];

            // Xử lý các bước cập nhật thông tin đối tượng
            int id = 0; // Giá trị mặc định nếu Session["MaDoAn"] là null
            object maDoAnObj = Session["MaNuoc"];
            if (maDoAnObj != null && maDoAnObj is int)
            {
                id = (int)maDoAnObj;
            }
            Nuoc existingDoAn = db.Nuocs.SingleOrDefault(d => d.MaNuoc == id);

            if (existingDoAn == null)
            {
                return HttpNotFound();
            }

            existingDoAn.TenNuoc = tenNuoc;
            existingDoAn.Gia = gia;
            existingDoAn.Loai = loai;

            db.SubmitChanges();

            // Chuyển hướng đến trang chi tiết hoặc danh sách đối tượng
            return RedirectToAction("Nuoc", "Admin", new { area = "Admin" });
        }
        public ActionResult DLNuoc(int itemId)
        {
            var nuoc = db.Nuocs.SingleOrDefault(u => u.MaNuoc == itemId);
            nuoc.TrangThai = "Ngưng bán";
            db.SubmitChanges();
            return RedirectToAction("Nuoc", "Admin", new { area = "Admin" });
        }
        public ActionResult DLS(int itemId)
        {
            var nuoc = db.Sliders.SingleOrDefault(u => u.ma == itemId);
            db.Sliders.DeleteOnSubmit(nuoc);
            db.SubmitChanges();
            return RedirectToAction("Slider", "Admin", new { area = "Admin" });
        }
        public ActionResult DLN(int itemId)
        {
            var nuoc = db.Tintucs.SingleOrDefault(u => u.Matin == itemId);
            db.Tintucs.DeleteOnSubmit(nuoc);
            db.SubmitChanges();
            return RedirectToAction("News", "Admin", new { area = "Admin" });
        }
        public ActionResult DLTK(string itemId)
        {
            var tk = db.TaiKhoans.SingleOrDefault(u => u.Username.Contains(itemId));
            db.TaiKhoans.DeleteOnSubmit(tk);
            db.SubmitChanges();
            return RedirectToAction("Account", "Admin", new { area = "Admin" });
        }
        public ActionResult DLCB(int itemId)
        {
            var nuoc = db.Combos.SingleOrDefault(u => u.MaCB == itemId);
            nuoc.TrangThai = "Ngưng bán";
            db.SubmitChanges();
            return RedirectToAction("Combo", "Admin", new { area = "Admin" });
        }
        public ActionResult DLMovie(int itemId)
        {
            var nuoc = db.Phims.SingleOrDefault(u => u.MaPhim == itemId);
            nuoc.TrangThai = "Ngưng chiếu";
            db.SubmitChanges();
            return RedirectToAction("Movie", "Admin", new { area = "Admin" });
        }
        public ActionResult DLPhong(int itemId)
        {
            var nuoc = db.Phongs.SingleOrDefault(u => u.MaPhong == itemId);
            nuoc.TrangThai = "Ngưng sử dụng";
            db.SubmitChanges();
            return RedirectToAction("Room", "Admin", new { area = "Admin" });
        }
        public ActionResult DLFood(int itemId)
        {
            var nuoc = db.DoAns.SingleOrDefault(u => u.MaDoAn == itemId);
            nuoc.TrangThai = "Ngưng bán";
            db.SubmitChanges();
            return RedirectToAction("Nuoc", "Admin", new { area = "Admin" });
        }
        public ActionResult DLTheater(int itemId)
        {
            var rap = db.RapChieus.SingleOrDefault(u => u.MaRap == itemId);
            db.RapChieus.DeleteOnSubmit(rap);
            db.SubmitChanges();
            return RedirectToAction("Movie", "Admin", new { area = "Admin" });
        }
        public ActionResult DLComment(int itemId)
        {
            var comment = db.BinhLuans.SingleOrDefault(u => u.MaBL == itemId);
            db.BinhLuans.DeleteOnSubmit(comment);
            db.SubmitChanges();
            return RedirectToAction("Comment", "Admin", new { area = "Admin", id = comment.MaPhim });
        }
        public ActionResult DLFeedback(int itemId)
        {
            var feedback = db.Feedbacks.SingleOrDefault(u => u.Id == itemId);
            db.Feedbacks.DeleteOnSubmit(feedback);
            db.SubmitChanges();
            return RedirectToAction("Feedbacks", "Admin", new { area = "Admin" });
        }
        private string SaveImage(HttpPostedFileBase image)
        {
            if (image != null && image.ContentLength > 0)
            {
                var directoryPath = Server.MapPath("~/IMG/");
                if (!Directory.Exists(directoryPath))
                {
                    Directory.CreateDirectory(directoryPath);
                }

                var fileName = Path.GetFileName(image.FileName);
                var imagePath = Path.Combine(directoryPath, fileName);
                image.SaveAs(imagePath);

                return fileName;
            }

            return null;
        }
        public ActionResult AddLichChieu(FormCollection formCollection)
        {
            string NgonNgu = formCollection["NgonNgu"];
            string gioChieuString = formCollection["GioChieu"].ToString();
            TimeSpan gioChieu = TimeSpan.TryParse(gioChieuString, out gioChieu) ? gioChieu : TimeSpan.Zero;
            string gioKetthucString = formCollection["GioKetThuc"].ToString();
            TimeSpan gioKetthuc = TimeSpan.TryParse(gioKetthucString, out gioKetthuc) ? gioKetthuc : TimeSpan.Zero;
            string phongString = formCollection["Phong"].ToString();
            int phong = int.TryParse(phongString, out phong) ? phong : 0;
            string MaPhim = formCollection["MaPhim"].ToString();
            int phim = int.TryParse(MaPhim, out phim) ? phim : 0;
            DateTime Ngay = DateTime.Parse(formCollection["NgayChieu"]);

            LichChieu d = new LichChieu
            {
                NgonNgu = NgonNgu,
                GioChieu = gioChieu,
                GioKetThuc = gioKetthuc,
                Phong = phong,
                MaPhim = phim,
                NgayChieu = Ngay
            };

            db.LichChieus.InsertOnSubmit(d);
            db.SubmitChanges();


            return RedirectToAction("Showtimes", "Admin", new { area = "Admin" });
        }
        [HttpPost]
        public JsonResult CheckScheduleConflict(LichChieu model)
        {
            var isConflict = IsScheduleConflict(model);

            return Json(new { isConflict });
        }

        private bool IsScheduleConflict(LichChieu model)
        {
            var existingSchedule = db.LichChieus
                .FirstOrDefault(s =>
                    s.NgayChieu == model.NgayChieu &&
                    s.Phong == model.Phong &&
                    (
                        (model.GioChieu >= s.GioChieu && model.GioChieu < s.GioKetThuc) ||
                        (model.GioKetThuc > s.GioChieu && model.GioKetThuc <= s.GioKetThuc)
                    )
                );

            return existingSchedule != null;
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EditLC(FormCollection formCollection)
        {
            string NgonNgu = formCollection["NgonNgu"];
            string gioChieuString = formCollection["GioChieu"].ToString();
            TimeSpan gioChieu = TimeSpan.TryParse(gioChieuString, out gioChieu) ? gioChieu : TimeSpan.Zero;
            string gioKetthucString = formCollection["GioKetThuc"].ToString();
            TimeSpan gioKetthuc = TimeSpan.TryParse(gioKetthucString, out gioKetthuc) ? gioKetthuc : TimeSpan.Zero;
            string phongString = formCollection["Phong"].ToString();
            int phong = int.TryParse(phongString, out phong) ? phong : 0;
            string MaPhim = formCollection["MaPhim"].ToString();
            int phim = int.TryParse(MaPhim, out phim) ? phim : 0;
            int id = (int)Session["MaLichChieu"];

            LichChieu existingDoAn = db.LichChieus.SingleOrDefault(d => d.MaChieuPhim == id);

            if (existingDoAn == null)
            {
                return HttpNotFound();
            }
            existingDoAn.NgonNgu = NgonNgu;
            existingDoAn.GioChieu = gioChieu;
            existingDoAn.GioKetThuc = gioKetthuc;
            existingDoAn.Phong = phong;
            existingDoAn.MaPhim = phim;


            db.SubmitChanges();

            // Chuyển hướng đến trang chi tiết hoặc danh sách đối tượng
            return RedirectToAction("Showtimes", "Admin", new { area = "Admin" });
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult AddPhong(FormCollection formCollection)
        {
            string TenPhong = formCollection["TenPhong"];
            string SoChoNgoi = formCollection["SoChoNgoi"].ToString();
            int sochongoi = int.TryParse(SoChoNgoi, out sochongoi) ? sochongoi : 0;


            Phong d = new Phong
            {
                TenPhong = TenPhong,
                SoChoNgoi = sochongoi,
                TrangThai = "Hoạt động"
            };

            db.Phongs.InsertOnSubmit(d);
            db.SubmitChanges();


            return RedirectToAction("Room", "Admin", new { area = "Admin" });
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EditPhong(FormCollection formCollection)
        {
            string TenPhong = formCollection["TenPhong"];
            string SoChoNgoi = formCollection["SoChoNgoi"].ToString();
            int sochongoi = int.TryParse(SoChoNgoi, out sochongoi) ? sochongoi : 0;

            int id = (int)Session["MaPhong"];
            Phong existingDoAn = db.Phongs.SingleOrDefault(d => d.MaPhong == id);

            if (existingDoAn == null)
            {
                return HttpNotFound();
            }
            existingDoAn.TenPhong = TenPhong;
            existingDoAn.SoChoNgoi = sochongoi;


            db.SubmitChanges();

            // Chuyển hướng đến trang chi tiết hoặc danh sách đối tượng
            return RedirectToAction("Room", "Admin", new { area = "Admin" });
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EditGhe(FormCollection formCollection)
        {
            string TenPhong = formCollection["TenGhe"];
            string SoChoNgoi = formCollection["LoaiGhe"];
            decimal gia = Decimal.TryParse(formCollection["Gia"].ToString(), out gia) ? gia : 0;
            int id = (int)Session["MaGhe"];
            Ghe existingDoAn = db.Ghes.SingleOrDefault(d => d.MaGhe == id);

            if (existingDoAn == null)
            {
                return HttpNotFound();
            }
            existingDoAn.TenGhe = TenPhong;
            existingDoAn.LoaiGhe = SoChoNgoi;
            existingDoAn.GiaTien = gia;

            db.SubmitChanges();

            // Chuyển hướng đến trang chi tiết hoặc danh sách đối tượng
            return RedirectToAction("Chair", "Admin", new { area = "Admin" });
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult AddGhe(FormCollection formCollection)
        {
            string TrangThai = formCollection["TrangThai"];
            string TenGhe = formCollection["TenGhe"];
            string LoaiGhe = formCollection["LoaiGhe"];
            string MaPhong = formCollection["MaPhong"].ToString();
            int phong = int.TryParse(MaPhong, out phong) ? phong : 0;



            Ghe d = new Ghe
            {
                TrangThai = TrangThai,
                MaPhong = phong,
                LoaiGhe = LoaiGhe,
                TenGhe = TenGhe,
            };

            db.Ghes.InsertOnSubmit(d);
            db.SubmitChanges();


            return RedirectToAction("Chair", "Admin", new { area = "Admin" });
        }
        public ActionResult AddTheater()
        {
            // Lấy danh sách các cụm rạp từ cơ sở dữ liệu (giả sử bạn có bảng RapChieu chứa các cụm)
            var cumList = db.RapChieus.Select(r => r.Cum).Distinct().ToList();
            // Truyền danh sách cụm vào View
            ViewBag.CumList = cumList;
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult AddTheater(FormCollection formCollection)
        {
            int MaRap = int.Parse(formCollection["MaRap"]);
            string TenRap = formCollection["TenRap"];
            string Diachi = formCollection["Diachi"];
            string Cum = formCollection["Cum"];


            RapChieu r = new RapChieu
            {
                MaRap = MaRap,
                TenRap = TenRap,
                DiaChi = Diachi,
                Cum = Cum
            };
            db.RapChieus.InsertOnSubmit(r);
            db.SubmitChanges();
            return RedirectToAction("GroupofTheater", "Admin", new { area = "Admin" });
        }
        public JsonResult EditTheater(int id)
        {
            Session["MaRap"] = id;
            var rap = db.RapChieus.SingleOrDefault(u => u.MaRap == id);
            if (rap == null)
            {
                return Json(new { success = false, message = "Khách hàng không tồn tại." });
            }
            var tkData = new
            {
                MaRap = rap.MaRap,
                TenRap = rap.TenRap,
                Diachi = rap.DiaChi,
                Cum = rap.Cum,
            };
            return Json(new { success = true, theater = tkData }, JsonRequestBehavior.AllowGet);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EditTheater(FormCollection formCollection)
        {
            string TenRap = formCollection["TenRap"];
            string Diachi = formCollection["Diachi"];
            string Cum = formCollection["Cum"];
            int id = (int)Session["MaRap"];
            RapChieu existingRapChieu = db.RapChieus.SingleOrDefault(d => d.MaRap == id);
            if (existingRapChieu == null)
            {
                return HttpNotFound();
            }
            existingRapChieu.TenRap = TenRap;
            existingRapChieu.DiaChi = Diachi;
            existingRapChieu.Cum = Cum;
            db.SubmitChanges();
            // Chuyển hướng đến trang chi tiết hoặc danh sách đối tượng
            return RedirectToAction("GroupofTheater", "Admin", new { area = "Admin" });
        }

    }

}