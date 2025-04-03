using DuAnRapChieuPhim.Models;
using Microsoft.AspNet.Identity;
using Newtonsoft.Json;
using PagedList;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.Entity;
using System.Diagnostics;
using System.Linq;
using System.Net.Mail;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Web;
using System.Web.Mvc;
using Microsoft.AspNet.SignalR;
using System.Web.UI;
using System.IO;
using ZXing.QrCode.Internal;
using DuAnRapChieuPhim.Hubs;
using System.Threading.Tasks;
using iText.Kernel.Pdf;
using iText.Layout;
using iText.Layout.Element;
using System.IO;
using iText.Kernel.Pdf.Canvas.Draw;
using iText.Layout.Properties;
using iText.Kernel.Geom;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Net.Http.Headers;
using System.Net.Http;
using Microsoft.Owin.Security;
namespace DuAnRapChieuPhim.Controllers
{
    public class HomeController : Controller
    {
        DbDataContext db = new DbDataContext(ConfigurationManager.ConnectionStrings["CinemaConnectionString"].ConnectionString);
        public void GoogleLogin()
        {
            // Chuyển hướng người dùng tới Google để xác thực
            HttpContext.GetOwinContext().Authentication.Challenge(
                new Microsoft.Owin.Security.AuthenticationProperties
                {
                    RedirectUri = Url.Action("GoogleCallback", "Home")
                },
                "Google"
            );
        }

        public ActionResult GoogleCallback()
        {
            // Xử lý phản hồi từ Google
            var loginInfo = HttpContext.GetOwinContext().Authentication.GetExternalLoginInfo();
            if (loginInfo == null)
            {
                return RedirectToAction("Login");
            }
            var user = db.ThongTinCaNhans.FirstOrDefault(u => u.Email == loginInfo.Email);
            if (user != null)
            {
                var tokenHandler = new JwtSecurityTokenHandler();
                var key = Encoding.ASCII.GetBytes("vG5*X9^mP!xYq3$7@lK8#d2%Jz&W0^QfT&4w9?L6s");
                var tokenDescriptor = new SecurityTokenDescriptor
                {
                    Subject = new ClaimsIdentity(new[]
                    {
                        new Claim("UserId", user.MaKH.ToString()),
                        new Claim("UserName", user.Ten),
                        new Claim("Email", user.Email)
                    }),
                    Expires = DateTime.UtcNow.AddHours(2),
                    SigningCredentials = new SigningCredentials(
                        new SymmetricSecurityKey(key),
                        SecurityAlgorithms.HmacSha256Signature
                    )
                };

                var token = tokenHandler.CreateToken(tokenDescriptor);
                string jwtToken = tokenHandler.WriteToken(token);
                // Tạo cookie 
                var cookie = new HttpCookie("AuthToken", jwtToken)
                {
                    HttpOnly = false,
                    Secure = Request.IsSecureConnection, // Bỏ Secure nếu chạy trên HTTP
                    Expires = DateTime.UtcNow.AddHours(2),
                    Path = "/", // Đảm bảo cookie có thể truy cập toàn trang web
                    Domain = "" // Để trống để áp dụng cho toàn bộ domain
                };

                Response.Cookies.Add(cookie);
                Session["KH"] = user;
                if (Session["PreviousPage"] != null)
                {
                    var previousPage = Session["PreviousPage"].ToString();
                    Session.Remove("PreviousPage");
                    return Redirect(previousPage);
                }

                return RedirectToAction("Index", "Home");
            }
            else
            {
                var thongTinCaNhan = new ThongTinCaNhan
                {
                    Ten = loginInfo.DefaultUserName,
                    SDT = "",
                    Email = loginInfo.Email,
                    Matkhau = "",
                };

                db.ThongTinCaNhans.InsertOnSubmit(thongTinCaNhan);
                db.SubmitChanges();

                var tokenHandler = new JwtSecurityTokenHandler();
                var key = Encoding.ASCII.GetBytes("vG5*X9^mP!xYq3$7@lK8#d2%Jz&W0^QfT&4w9?L6s");
                var tokenDescriptor = new SecurityTokenDescriptor
                {
                    Subject = new ClaimsIdentity(new[]
                    {
                        new Claim("UserId", thongTinCaNhan.MaKH.ToString()),
                        new Claim("UserName", thongTinCaNhan.Ten),
                        new Claim("Email", thongTinCaNhan.Email)
                    }),
                    Expires = DateTime.UtcNow.AddHours(2),
                    SigningCredentials = new SigningCredentials(
                        new SymmetricSecurityKey(key),
                        SecurityAlgorithms.HmacSha256Signature
                    )
                };

                var token = tokenHandler.CreateToken(tokenDescriptor);
                string jwtToken = tokenHandler.WriteToken(token);
                // Tạo cookie 
                var cookie = new HttpCookie("AuthToken", jwtToken)
                {
                    HttpOnly = false,
                    Secure = Request.IsSecureConnection, // Bỏ Secure nếu chạy trên HTTP
                    Expires = DateTime.UtcNow.AddHours(2),
                    Path = "/", // Đảm bảo cookie có thể truy cập toàn trang web
                    Domain = "" // Để trống để áp dụng cho toàn bộ domain
                };

                Response.Cookies.Add(cookie);
                Session["KH"] = user;
                if (Session["PreviousPage"] != null)
                {
                    var previousPage = Session["PreviousPage"].ToString();
                    Session.Remove("PreviousPage");
                    return Redirect(previousPage);
                }

                return RedirectToAction("Index", "Home");

            }
        }

        public ActionResult Login(string succes, string err)
        {
            ViewData["succes"] = null;
            ViewData["err"] = null;
            if (succes != null)
            {
                ViewData["succes"] = succes;
            }
            if (err != null)
            {
                ViewData["err"] = err;
            }
            return View();

        }


        [HttpPost]
        public ActionResult Login(FormCollection formCollection)
        {
            string emailOrPhone = formCollection["loginInput"];
            string password = HashSHA256(formCollection["passwordInput"]);

            // Kiểm tra thông tin đăng nhập trong cơ sở dữ liệu
            var user = db.ThongTinCaNhans
                .FirstOrDefault(u => (u.Email == emailOrPhone || u.SDT == emailOrPhone) && u.Matkhau == password);

            if (user != null)
            {
                // Tạo JWT token
                var tokenHandler = new JwtSecurityTokenHandler();
                var key = Encoding.ASCII.GetBytes("vG5*X9^mP!xYq3$7@lK8#d2%Jz&W0^QfT&4w9?L6s");
                var tokenDescriptor = new SecurityTokenDescriptor
                {
                    Subject = new ClaimsIdentity(new[]
                    {
                new Claim("UserId", user.MaKH.ToString()),
                new Claim("UserName", user.Ten),
                new Claim("Email", user.Email)
            }),
                    Expires = DateTime.UtcNow.AddHours(2),
                    SigningCredentials = new SigningCredentials(
                        new SymmetricSecurityKey(key),
                        SecurityAlgorithms.HmacSha256Signature
                    )
                };

                var token = tokenHandler.CreateToken(tokenDescriptor);
                string jwtToken = tokenHandler.WriteToken(token);
                // Tạo cookie 
                var cookie = new HttpCookie("AuthToken", jwtToken)
                {
                    HttpOnly = false,
                    Secure = Request.IsSecureConnection, // Bỏ Secure nếu chạy trên HTTP
                    Expires = DateTime.UtcNow.AddHours(2),
                    Path = "/", // Đảm bảo cookie có thể truy cập toàn trang web
                    Domain = "" // Để trống để áp dụng cho toàn bộ domain
                };

                Response.Cookies.Add(cookie);
                Session["KH"] = user;
                // Điều hướng đến trang trước đó hoặc trang chủ
                if (Session["PreviousPage"] != null)
                {
                    var previousPage = Session["PreviousPage"].ToString();
                    Session.Remove("PreviousPage");
                    return Redirect(previousPage);
                }

                return RedirectToAction("Index", "Home");
            }
            else
            {
                // Quay lại trang login với thông báo lỗi
                return RedirectToAction("Login", new { err = "Thông tin đăng nhập không đúng" });
            }
        }

        public ThongTinCaNhan GetUserFromJwt()
        {
            // Kiểm tra Request trước khi truy cập Cookies
            if (Request == null)
            {

                return null;
            }
            // Kiểm tra cookie
            var tokenCookie = Request.Cookies["AuthToken"];


            var token = tokenCookie.Value;
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes("vG5*X9^mP!xYq3$7@lK8#d2%Jz&W0^QfT&4w9?L6s");

            try
            {
                var tokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    ClockSkew = TimeSpan.Zero
                };

                var claimsPrincipal = tokenHandler.ValidateToken(
                    token,
                    tokenValidationParameters,
                    out SecurityToken validatedToken
                );

                var jwtToken = (JwtSecurityToken)validatedToken;
                var userId = jwtToken.Claims.First(x => x.Type == "UserId").Value;
                var userName = jwtToken.Claims.First(x => x.Type == "UserName").Value;

                return new ThongTinCaNhan
                {
                    MaKH = int.Parse(userId),
                    Ten = userName
                };
            }
            catch
            {
                // Xóa cookie không hợp lệ
                Response.Cookies["AuthToken"].Expires = DateTime.Now.AddDays(-1);
                return null;
            }
        }


        [HttpPost]
        public JsonResult CheckUserExistence(string inputType, string inputInfo)
        {
            // Kiểm tra sự tồn tại của người dùng trong bảng ThongTinCaNhan
            bool userExists = CheckUserExists(inputType, inputInfo);

            return Json(new { exist = userExists });
        }

        // Hàm kiểm tra sự tồn tại của người dùng
        private bool CheckUserExists(string inputType, string inputInfo)
        {

            // Kiểm tra sự tồn tại của người dùng với số điện thoại hoặc email
            if (inputType.ToLower() == "phone")
            {
                // Kiểm tra số điện thoại
                return db.ThongTinCaNhans.Any(u => u.SDT == inputInfo);
            }
            else if (inputType.ToLower() == "email")
            {
                // Kiểm tra email
                return db.ThongTinCaNhans.Any(u => u.Email == inputInfo);
            }

            // Trường hợp không xác định loại thông tin, trả về false
            return false;

        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ChangePassword(string currentPassword, string newPassword)
        {
            var nguoidung = GetUserFromJwt();
            int userId = nguoidung.MaKH;

            var user = db.ThongTinCaNhans.FirstOrDefault(u => u.MaKH == userId);

            // Kiểm tra mật khẩu hiện tại
            if (user != null && user.Matkhau == HashSHA256(currentPassword))
            {
                user.Matkhau = HashSHA256(newPassword);
                db.SubmitChanges();
                return RedirectToAction("EditTT", new { id = nguoidung.MaKH, succes = "Đổi mật khẩu thành công." });
            }
            else
            {
                return RedirectToAction("EditTT", new { id = nguoidung.MaKH, err = "Mật khẩu hiện tại không đúng." });
            }
        }
        [HttpPost]
        public ActionResult UpdateProfile(string ten, string sdt, string email)
        {
            var nguoidung = GetUserFromJwt();
            int userId = nguoidung.MaKH;

            var user = db.ThongTinCaNhans.FirstOrDefault(u => u.MaKH == userId);

            // Kiểm tra mật khẩu hiện tại
            if (user != null)
            {
                user.Ten = ten;
                user.SDT = sdt;
                user.Email = email;
                db.SubmitChanges();
                return RedirectToAction("EditTT", new { id = nguoidung.MaKH, succes = "Đổi thông tin thành công." });
            }
            else
            {
                return RedirectToAction("EditTT", new { id = nguoidung.MaKH, err = "Đổi thông tin không thành công." });
            }
        }
        [SavePreviousPage]
        public ActionResult Ve(int? page)
        {
            int pageSize = 9; // Số sản phẩm trên mỗi trang
            int pageNumber = (page ?? 1); // Trang mặc định là trang 1 nếu không có tham số page
            ThongTinCaNhan nguoidung = GetUserFromJwt();
            var ve = db.HoaDons.Where(u => u.MaKH == nguoidung.MaKH && u.TrangThai.Contains("Đã thanh toán")).ToPagedList(pageNumber, pageSize);
            return View(ve);
        }
        [SavePreviousPage]
        public ActionResult MovieShow(int? page)
        {
            int pageSize = 9; // Số sản phẩm trên mỗi trang
            int pageNumber = (page ?? 1); // Trang mặc định là trang 1 nếu không có tham số page
            ViewBag.TheloaiList = db.TheLoais.ToList();
            var phim = db.Phims.Where(u => u.TrangThai.Contains("Đang chiếu")).ToPagedList(pageNumber, pageSize);
            return View(phim);
        }
        [SavePreviousPage]
        public ActionResult MovieSoone(int? page)
        {
            int pageSize = 9; // Số sản phẩm trên mỗi trang
            int pageNumber = (page ?? 1); // Trang mặc định là trang 1 nếu không có tham số page
            ViewBag.TheloaiList = db.TheLoais.ToList();
            var phim = db.Phims.Where(u => u.TrangThai.Contains("Sắp chiếu")).ToPagedList(pageNumber, pageSize);
            return View(phim);
        }
        [HttpPost]
        public ActionResult DK(FormCollection form)
        {
            string hoTen = form["HoTen"];
            string soDienThoai = form["SoDienThoai"];
            string email = form["Email"];
            string matKhau = form["MatKhau"];

            if (db.ThongTinCaNhans.Any(x => x.SDT == soDienThoai))
            {
                return RedirectToAction("Login", new { err = "Số điện thoại đã tồn tại" });
            }

            if (db.ThongTinCaNhans.Any(x => x.Email == email))
            {
                return RedirectToAction("Login", new { err = "Email đã tồn tại" });
            }

            if (!string.IsNullOrEmpty(hoTen) && !string.IsNullOrEmpty(soDienThoai) &&
                !string.IsNullOrEmpty(email) && !string.IsNullOrEmpty(matKhau))
            {
                string hashedPassword = HashSHA256(matKhau);

                var thongTinCaNhan = new ThongTinCaNhan
                {
                    Ten = hoTen,
                    SDT = soDienThoai,
                    Email = email,
                    Matkhau = hashedPassword,
                };

                db.ThongTinCaNhans.InsertOnSubmit(thongTinCaNhan);
                db.SubmitChanges();

                // Tạo token JWT
                var tokenHandler = new JwtSecurityTokenHandler();
                var key = Encoding.ASCII.GetBytes("vG5*X9^mP!xYq3$7@lK8#d2%Jz&W0^QfT&4w9?L6s"); // Đổi thành khóa bí mật của bạn
                var tokenDescriptor = new SecurityTokenDescriptor
                {
                    Subject = new ClaimsIdentity(new Claim[]
                    {
                new Claim(ClaimTypes.Name, thongTinCaNhan.Email),
                new Claim("UserId", thongTinCaNhan.MaKH.ToString())
                    }),
                    Expires = DateTime.UtcNow.AddHours(1),
                    SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
                };
                var token = tokenHandler.CreateToken(tokenDescriptor);
                string jwtToken = tokenHandler.WriteToken(token);

                // Trả về token dưới dạng JSON
                return Json(new { success = true, token = jwtToken });
            }

            return RedirectToAction("Login", new { err = "Vui lòng nhập đầy đủ thông tin" });
        }
        [AcceptVerbs("GET", "POST")]
        public JsonResult ForgotPassword(string inputType, string inputInfo)
        {
            // Kiểm tra sự tồn tại của người dùng trong bảng ThongTinCaNhan
            var user = GetUser(inputType, inputInfo);

            if (user != null)
            {
                // Tạo mật khẩu mới
                string newPassword = GenerateRandomPassword();

                // Cập nhật mật khẩu mới cho người dùng trong cơ sở dữ liệu
                UpdatePassword(user, newPassword);

                // Gửi mật khẩu mới đến email của người dùng
                SendNewPasswordByEmail(user.Email, newPassword);

                return Json(new { success = true, message = "Mật khẩu mới đã được gửi đến email của bạn." });
            }

            return Json(new { success = false, message = "Không tìm thấy người dùng." });
        }
        private ThongTinCaNhan GetUser(string inputType, string inputInfo)
        {
            // Kiểm tra sự tồn tại của người dùng với số điện thoại hoặc email
            if (inputType.ToLower() == "phone")
            {
                // Kiểm tra số điện thoại
                return db.ThongTinCaNhans.FirstOrDefault(u => u.SDT == inputInfo);
            }
            else if (inputType.ToLower() == "email")
            {
                // Kiểm tra email
                return db.ThongTinCaNhans.FirstOrDefault(u => u.Email == inputInfo);
            }

            // Trường hợp không xác định loại thông tin, trả về null
            return null;

        }

        // Hàm gửi mật khẩu mới đến email của người dùng

        public ActionResult EditTT(int id, string succes, string err)
        {
            ViewData["succes"] = null;
            ViewData["err"] = null;
            if (succes != null)
            {
                ViewData["succes"] = succes;
            }
            if (err != null)
            {
                ViewData["err"] = err;
            }
            var tt = db.ThongTinCaNhans.SingleOrDefault(u => u.MaKH == id);
            return View(tt);
        }
        public ActionResult Logout()
        {
            // Xóa cookie
            if (Request.Cookies["AuthToken"] != null)
            {
                var cookie = new HttpCookie("AuthToken")
                {
                    Expires = DateTime.Now.AddDays(-1)
                };
                Response.Cookies.Add(cookie);
            }
            Session.Remove("KH");
            return RedirectToAction("Index");
        }
        private string GenerateRandomPassword()
        {
            // Bảng ký tự bạn muốn sử dụng để tạo mật khẩu
            string chars = "!@#$%^&*()-_=+abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";

            // Độ dài của mật khẩu
            int passwordLength = 8;

            // Sử dụng StringBuilder để hiệu quả khi thêm ký tự
            StringBuilder password = new StringBuilder();

            // Random số để chọn ký tự từ bảng
            Random random = new Random();

            // Tạo mật khẩu ngẫu nhiên
            for (int i = 0; i < passwordLength; i++)
            {
                int index = random.Next(chars.Length);
                password.Append(chars[index]);
            }

            return password.ToString();
        }

        private void UpdatePassword(ThongTinCaNhan user, string newPassword)
        {

            // Hash mật khẩu mới
            string hashedPassword = HashSHA256(newPassword);

            // Cập nhật mật khẩu mới đã được hash
            user.Matkhau = hashedPassword;
            db.SubmitChanges();

        }
        // Hàm mã hóa mật khẩu bằng SHA-256
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
        public ActionResult ThanhToanQuaThe(string selectedCombos, string voucherCode)
        {

            var user = GetUserFromJwt();
            var nguoidung = db.ThongTinCaNhans.SingleOrDefault(u => u.MaKH == user.MaKH);
            List<int> listGhe = Session["ListGhe"] as List<int>;
            List<ComboDat> comboList = JsonConvert.DeserializeObject<List<ComboDat>>(selectedCombos);
            decimal tienve = 0;
            var danhSachGheTrongList = db.Ghes.Where(g => listGhe.Contains(g.MaGhe)).ToList();

            foreach (var sp in danhSachGheTrongList)
            {
                tienve += sp.GiaTien.Value;
            }
            decimal tiencb = 0;

            foreach (var combo in comboList)
            {

                // Giả sử "MaCB" và "SoLuong" đều có trong mỗi dictionary
                int maCB = combo.MaCB;
                int soLuong = combo.SoLuong;
                // Kiểm tra xem MaCB có tồn tại trong bảng cb không
                var cb = db.Combos.FirstOrDefault(c => c.MaCB == maCB);

                if (cb != null)
                {
                    // Nếu MaCB tồn tại, tính tổng tiền và cộng vào biến tổng
                    decimal giaTien = cb.Gia.Value;
                    decimal thanhTien = giaTien * soLuong;
                    tiencb += thanhTien;

                }
            }
            Session["ListCB"] = comboList;
            var tongtien = tienve + tiencb;
            HoaDon dh = new HoaDon
            {
                MaHoaDon = DateTime.Now.Ticks,
                HoTen = nguoidung.Ten,
                SoDienThoai = nguoidung.SDT,
                Email = nguoidung.Email,
                MaKH = nguoidung.MaKH,
                TrangThai = "Chưa thanh toán",
                TienVe = tienve,
                TienCombo = tiencb,
                TongTien = tongtien,
                NgayDat = DateTime.Now.Date
            };

            // Kiểm tra nếu có voucher
            if (!string.IsNullOrEmpty(voucherCode))
            {
                var voucher = db.Vouchers.SingleOrDefault(u => u.MaVoucher == voucherCode);

                if (voucher != null)  // Nếu voucher hợp lệ
                {
                    dh.SoTienGiam = tongtien * voucher.GiamGiaPhanTram.Value / 100;
                    // Tính toán giảm giá và cập nhật tổng tiền
                    tongtien = tongtien - (tongtien * voucher.GiamGiaPhanTram.Value / 100);

                    // Lưu voucher vào Session
                    Session["voucher"] = voucherCode;

                    // Cập nhật thông tin voucher vào hóa đơn
                    dh.TongTien = tongtien;
                    dh.TenVoucher = voucher.MaVoucher;  // Tên voucher
                                                        // Số tiền giảm
                }
            }

            // Thêm hóa đơn vào cơ sở dữ liệu
            db.HoaDons.InsertOnSubmit(dh);
            db.SubmitChanges();
            var urlPayment = "";
            Session["MaDH"] = dh.MaHoaDon;

            string vnp_Returnurl = ConfigurationManager.AppSettings["vnp_Returnurl"]; //URL nhan ket qua tra ve 
            string vnp_Url = ConfigurationManager.AppSettings["vnp_Url"]; //URL thanh toan cua VNPAY 
            string vnp_TmnCode = ConfigurationManager.AppSettings["vnp_TmnCode"]; //Ma định danh merchant kết nối (Terminal Id)
            string vnp_HashSecret = ConfigurationManager.AppSettings["vnp_HashSecret"]; //Secret Key


            VnPayLibrary vnpay = new VnPayLibrary();
            long amount = (long)dh.TongTien;
            vnpay.AddRequestData("vnp_Version", VnPayLibrary.VERSION);
            vnpay.AddRequestData("vnp_Command", "pay");
            vnpay.AddRequestData("vnp_TmnCode", vnp_TmnCode);
            vnpay.AddRequestData("vnp_Amount", (amount * 100).ToString()); //Số tiền thanh toán. Số tiền không mang các ký tự phân tách thập phân, phần nghìn, ký tự tiền tệ. Để gửi số tiền thanh toán là 100,000 VND (một trăm nghìn VNĐ) thì merchant cần nhân thêm 100 lần (khử phần thập phân), sau đó gửi sang VNPAY là: 10000000
            long madonhang = dh.MaHoaDon;
            vnpay.AddRequestData("vnp_CreateDate", DateTime.Now.ToString("yyyyMMddHHmmss"));
            vnpay.AddRequestData("vnp_CurrCode", "VND");
            vnpay.AddRequestData("vnp_IpAddr", Utils.GetIpAddress());

            vnpay.AddRequestData("vnp_Locale", "vn");
            vnpay.AddRequestData("vnp_OrderInfo", "Thanh toan don hang:" + madonhang);
            vnpay.AddRequestData("vnp_OrderType", "other"); //default value: other

            vnpay.AddRequestData("vnp_ReturnUrl", vnp_Returnurl);
            vnpay.AddRequestData("vnp_TxnRef", madonhang.ToString()); // Mã tham chiếu của giao dịch tại hệ thống của merchant. Mã này là duy nhất dùng để phân biệt các đơn hàng gửi sang VNPAY. Không được trùng lặp trong ngày

            //Add Params of 2.1.0 Version
            //Billing

            urlPayment = vnpay.CreateRequestUrl(vnp_Url, vnp_HashSecret);
            return Redirect(urlPayment);
        }
        public ActionResult Return()
        {
            if (Request.QueryString.Count > 0)
            {
                string vnp_HashSecret = ConfigurationManager.AppSettings["vnp_HashSecret"]; //Chuoi bi mat
                var vnpayData = Request.QueryString;
                VnPayLibrary vnpay = new VnPayLibrary();

                foreach (string s in vnpayData)
                {
                    if (!string.IsNullOrEmpty(s) && s.StartsWith("vnp_"))
                    {
                        vnpay.AddResponseData(s, vnpayData[s]);
                    }
                }
                string vnp_ResponseCode = vnpay.GetResponseData("vnp_ResponseCode");
                string vnp_TransactionStatus = vnpay.GetResponseData("vnp_TransactionStatus");
                String vnp_SecureHash = Request.QueryString["vnp_SecureHash"];

                bool checkSignature = vnpay.ValidateSignature(vnp_SecureHash, vnp_HashSecret);
                if (checkSignature)
                {
                    if (vnp_ResponseCode == "00" && vnp_TransactionStatus == "00")
                    {
                        List<int> listGhe = Session["ListGhe"] as List<int>;
                        List<ComboDat> listcb = Session["ListCB"] as List<ComboDat>;
                        long madh = (long)Session["MaDH"];
                        int macp = (int)Session["MaCP"];
                        foreach (var cb in listcb)
                        {
                            var combo = db.Combos.SingleOrDefault(u => u.MaCB == cb.MaCB);
                            CTHDComBo cthdcb = new CTHDComBo
                            {
                                MaCB = cb.MaCB,
                                SoLuong = cb.SoLuong,
                                GiaTien = combo.Gia.Value,
                                MaHoaDon = madh
                            };
                            db.CTHDComBos.InsertOnSubmit(cthdcb);
                        }
                        foreach (var ghe in listGhe)
                        {
                            var lc = db.LichChieus.SingleOrDefault(u => u.MaChieuPhim == macp);
                            var g = db.Ghes.SingleOrDefault(u => u.MaPhong == lc.Phong && u.MaGhe == ghe);
                            ChiTietHoaDon cthd = new ChiTietHoaDon
                            {
                                ThoiGianDat = DateTime.Now.Date,
                                ThoiGianChieu = lc.NgayChieu,
                                TongTien = g.GiaTien,
                                MaHoaDon = madh,
                                MaLichChieu = macp,
                                MaGhe = ghe,
                            };
                            db.ChiTietHoaDons.InsertOnSubmit(cthd);
                        }
                        var dh = db.HoaDons.SingleOrDefault(u => u.MaHoaDon == madh);
                        dh.TrangThai = "Đã thanh toán";
                        db.SubmitChanges();

                    }
                    else
                    {

                        ViewBag.InnerText = "Có lỗi xảy ra trong quá trình xử lý.Mã lỗi: " + vnp_ResponseCode;
                    }
                }
            }
            return View();
        }
        public ActionResult ThanhToanDiem(string selectedCombos, string voucherCode)
        {
            var user = GetUserFromJwt();
            var nguoidung = db.ThongTinCaNhans.SingleOrDefault(u => u.MaKH == user.MaKH);
            List<int> listGhe = Session["ListGhe"] as List<int>;
            List<ComboDat> comboList = JsonConvert.DeserializeObject<List<ComboDat>>(selectedCombos);
            decimal tienve = 0;
            var danhSachGheTrongList = db.Ghes.Where(g => listGhe.Contains(g.MaGhe)).ToList();

            foreach (var sp in danhSachGheTrongList)
            {
                tienve += sp.GiaTien.Value;
            }
            decimal tiencb = 0;

            foreach (var combo in comboList)
            {

                // Giả sử "MaCB" và "SoLuong" đều có trong mỗi dictionary
                int maCB = combo.MaCB;
                int soLuong = combo.SoLuong;
                // Kiểm tra xem MaCB có tồn tại trong bảng cb không
                var cb = db.Combos.FirstOrDefault(c => c.MaCB == maCB);

                if (cb != null)
                {
                    // Nếu MaCB tồn tại, tính tổng tiền và cộng vào biến tổng
                    decimal giaTien = cb.Gia.Value;
                    decimal thanhTien = giaTien * soLuong;
                    tiencb += thanhTien;

                }
            }
            Session["ListCB"] = comboList;
            var tongtien = tienve + tiencb;
            HoaDon dh = new HoaDon
            {
                MaHoaDon = DateTime.Now.Ticks,
                HoTen = nguoidung.Ten,
                SoDienThoai = nguoidung.SDT,
                Email = nguoidung.Email,
                MaKH = nguoidung.MaKH,
                TrangThai = "Chưa thanh toán",
                TienVe = tienve,
                TienCombo = tiencb,
                TongTien = tongtien,
                NgayDat = DateTime.Now.Date
            };

            // Kiểm tra nếu có voucher
            if (!string.IsNullOrEmpty(voucherCode))
            {
                var voucher = db.Vouchers.SingleOrDefault(u => u.MaVoucher == voucherCode);

                if (voucher != null)  // Nếu voucher hợp lệ
                {
                    dh.SoTienGiam = tongtien * voucher.GiamGiaPhanTram.Value / 100;  // Số tiền giảm
                    // Tính toán giảm giá và cập nhật tổng tiền
                    tongtien = tongtien - (tongtien * voucher.GiamGiaPhanTram.Value / 100);

                    // Cập nhật thông tin voucher vào hóa đơn
                    dh.TongTien = tongtien;
                    dh.TenVoucher = voucher.MaVoucher;  // Tên voucher

                    voucher.SoLuotDaSD++;
                }
            }

            // Thêm hóa đơn vào cơ sở dữ liệu
            db.HoaDons.InsertOnSubmit(dh);
            db.SubmitChanges();
            foreach (var cb in comboList)
            {
                var combo = db.Combos.SingleOrDefault(u => u.MaCB == cb.MaCB);
                CTHDComBo cthdcb = new CTHDComBo
                {
                    MaCB = cb.MaCB,
                    SoLuong = cb.SoLuong,
                    GiaTien = combo.Gia.Value,
                    MaHoaDon = dh.MaHoaDon
                };
                db.CTHDComBos.InsertOnSubmit(cthdcb);
                db.SubmitChanges();
            }
            int macp = (int)Session["MaCP"];
            foreach (var ghe in listGhe)
            {
                var lc = db.LichChieus.SingleOrDefault(u => u.MaChieuPhim == macp);
                var g = db.Ghes.SingleOrDefault(u => u.MaPhong == lc.Phong && u.MaGhe == ghe);
                ChiTietHoaDon cthd = new ChiTietHoaDon
                {
                    ThoiGianDat = DateTime.Now.Date,
                    ThoiGianChieu = lc.NgayChieu,
                    TongTien = g.GiaTien,
                    MaHoaDon = dh.MaHoaDon,
                    MaLichChieu = macp,
                    MaGhe = ghe,
                };
                db.ChiTietHoaDons.InsertOnSubmit(cthd);
                db.SubmitChanges();
            }
            dh.TrangThai = "Đã thanh toán";

            nguoidung.Diem -= (int)dh.TongTien;
            db.SubmitChanges();
            SendEmail(dh.MaHoaDon);
            return RedirectToAction("DetailsHD", "Home", new { id = dh.MaHoaDon });
        }
        public ActionResult ThanhToanMOMO(string selectedCombos, string voucherCode)
        {


            var partnerCode = ConfigurationManager.AppSettings["PartnerCode"];
            var accessKey = ConfigurationManager.AppSettings["AccessKey"];
            var secretKey = ConfigurationManager.AppSettings["SecretKey"];
            var returnUrl = ConfigurationManager.AppSettings["ReturnUrl"];
            var notifyUrl = ConfigurationManager.AppSettings["NotifyUrl"];
            var momoApiUrl = ConfigurationManager.AppSettings["MomoApiUrl"];
            var requestType = ConfigurationManager.AppSettings["RequestType"];
            var ipnUrl = ConfigurationManager.AppSettings["IpnUrl"];


            var requestId = DateTime.Now.Ticks.ToString();
            string orderInfo = "SDK team.";
            var user = GetUserFromJwt();
            var nguoidung = db.ThongTinCaNhans.SingleOrDefault(u => u.MaKH == user.MaKH);
            List<int> listGhe = Session["ListGhe"] as List<int>;
            List<ComboDat> comboList = JsonConvert.DeserializeObject<List<ComboDat>>(selectedCombos);
            decimal tienve = 0;
            var danhSachGheTrongList = db.Ghes.Where(g => listGhe.Contains(g.MaGhe)).ToList();

            foreach (var sp in danhSachGheTrongList)
            {
                tienve += sp.GiaTien.Value;
            }
            decimal tiencb = 0;

            foreach (var combo in comboList)
            {

                // Giả sử "MaCB" và "SoLuong" đều có trong mỗi dictionary
                int maCB = combo.MaCB;
                int soLuong = combo.SoLuong;
                // Kiểm tra xem MaCB có tồn tại trong bảng cb không
                var cb = db.Combos.FirstOrDefault(c => c.MaCB == maCB);

                if (cb != null)
                {
                    // Nếu MaCB tồn tại, tính tổng tiền và cộng vào biến tổng
                    decimal giaTien = cb.Gia.Value;
                    decimal thanhTien = giaTien * soLuong;
                    tiencb += thanhTien;

                }
            }
            Session["ListCB"] = comboList;
            var tongtien = tienve + tiencb;
            HoaDon dh = new HoaDon
            {
                MaHoaDon = DateTime.Now.Ticks,
                HoTen = nguoidung.Ten,
                SoDienThoai = nguoidung.SDT,
                Email = nguoidung.Email,
                MaKH = nguoidung.MaKH,
                TrangThai = "Chưa thanh toán",
                TienVe = tienve,
                TienCombo = tiencb,
                TongTien = tongtien,
                NgayDat = DateTime.Now.Date
            };

            // Kiểm tra nếu có voucher
            if (!string.IsNullOrEmpty(voucherCode))
            {
                var voucher = db.Vouchers.SingleOrDefault(u => u.MaVoucher == voucherCode);

                if (voucher != null)  // Nếu voucher hợp lệ
                {
                    dh.SoTienGiam = tongtien * voucher.GiamGiaPhanTram.Value / 100;  // Số tiền giảm
                                                                                     // Tính toán giảm giá và cập nhật tổng tiền
                    tongtien = (int)(tongtien - (tongtien * voucher.GiamGiaPhanTram.Value / 100));

                    // Lưu voucher vào Session
                    Session["voucher"] = voucherCode;

                    // Cập nhật thông tin voucher vào hóa đơn
                    dh.TongTien = tongtien;
                    dh.TenVoucher = voucher.MaVoucher;  // Tên voucher

                }
            }

            // Thêm hóa đơn vào cơ sở dữ liệu
            db.HoaDons.InsertOnSubmit(dh);
            db.SubmitChanges();
            Session["MaDH"] = dh.MaHoaDon;
            string extraData = "abc@gmail.com";

            string rawHash = $"accessKey={accessKey}&amount={dh.TongTien}&extraData={extraData}&ipnUrl={ipnUrl}&orderId={dh.MaHoaDon}&orderInfo={orderInfo}&partnerCode={partnerCode}&redirectUrl={returnUrl}&requestId={dh.MaHoaDon}&requestType={requestType}";


            var signature = ComputeHmacSha256(rawHash, secretKey);


            var requestData = new
            {
                accessKey = accessKey,
                partnerCode = partnerCode,
                requestType = requestType,
                notifyUrl = notifyUrl,
                redirectUrl = returnUrl,
                ipnUrl = ipnUrl,
                orderId = dh.MaHoaDon,
                amount = dh.TongTien,
                orderInfo = orderInfo,
                requestId = dh.MaHoaDon,
                extraData = extraData,
                signature = signature
            };


            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri(momoApiUrl);
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                var content = new StringContent(JsonConvert.SerializeObject(requestData), Encoding.UTF8, "application/json");

                var response = client.PostAsync("", content).Result;
                var responseContent = response.Content.ReadAsStringAsync().Result;

                // Giải mã JSON response từ Momo
                var result = JsonConvert.DeserializeObject<dynamic>(responseContent);

                ViewBag.RequestUrl = momoApiUrl;
                ViewBag.ResponseContent = responseContent;

                // Kiểm tra xem resultCode có bằng 0 không và payUrl có tồn tại không
                if (result != null && result.resultCode == 0 && result.payUrl != null)
                {
                    // Chuyển hướng đến payUrl để thực hiện thanh toán
                    return Redirect(result.payUrl.ToString());
                }
                else
                {
                    // Nếu thanh toán thất bại, hiển thị thông báo lỗi
                    ViewBag.Message = "Thanh toán thất bại!";
                    ViewBag.AlertType = "danger";
                    return View("Error");
                }
            }

        }
        private string ComputeHmacSha256(string message, string secretKey)
        {
            var keyBytes = Encoding.UTF8.GetBytes(secretKey);
            var messageBytes = Encoding.UTF8.GetBytes(message);

            byte[] hashBytes;

            using (var hmac = new HMACSHA256(keyBytes))
            {
                hashBytes = hmac.ComputeHash(messageBytes);
            }

            var hashString = BitConverter.ToString(hashBytes).Replace("-", "").ToLower();
            Debug.WriteLine("Hash String: " + hashString);
            return hashString;
        }
        public ActionResult ReturnMOMO()
        {
            var queryString = Request.QueryString;

            // Trích xuất các tham số từ query string
            string partnerCode = queryString["partnerCode"];
            string orderId = queryString["orderId"];
            string requestId = queryString["requestId"];
            string amount = queryString["amount"];
            string orderInfo = queryString["orderInfo"];
            string orderType = queryString["orderType"];
            string transId = queryString["transId"];
            string resultCode = queryString["resultCode"];
            long madh = (long)Session["MaDH"];
            var dh = db.HoaDons.SingleOrDefault(u => u.MaHoaDon == madh);
            if (resultCode == "0")
            {
                // Nếu thanh toán thành công, xử lý dữ liệu
                List<int> listGhe = Session["ListGhe"] as List<int>;
                List<ComboDat> listcb = Session["ListCB"] as List<ComboDat>;

                int macp = (int)Session["MaCP"];

                // Lưu các combo vào chi tiết hóa đơn
                foreach (var cb in listcb)
                {
                    var combo = db.Combos.SingleOrDefault(u => u.MaCB == cb.MaCB);
                    CTHDComBo cthdcb = new CTHDComBo
                    {
                        MaCB = cb.MaCB,
                        SoLuong = cb.SoLuong,
                        GiaTien = combo.Gia.Value,
                        MaHoaDon = madh
                    };
                    db.CTHDComBos.InsertOnSubmit(cthdcb);
                    db.SubmitChanges();
                }

                // Lưu các ghế vào chi tiết hóa đơn
                foreach (var ghe in listGhe)
                {
                    var lc = db.LichChieus.SingleOrDefault(u => u.MaChieuPhim == macp);
                    var g = db.Ghes.SingleOrDefault(u => u.MaPhong == lc.Phong && u.MaGhe == ghe);
                    ChiTietHoaDon cthd = new ChiTietHoaDon
                    {
                        ThoiGianDat = DateTime.Now.Date,
                        ThoiGianChieu = lc.NgayChieu,
                        TongTien = g.GiaTien,
                        MaHoaDon = madh,
                        MaLichChieu = macp,
                        MaGhe = ghe,
                    };
                    db.ChiTietHoaDons.InsertOnSubmit(cthd);
                    db.SubmitChanges();
                }

                // Cập nhật trạng thái của hóa đơn và thông tin khách hàng
                dh.TrangThai = "Đã thanh toán";
                var user = GetUserFromJwt();
                var userdb = db.ThongTinCaNhans.SingleOrDefault(u => u.MaKH == user.MaKH);
                userdb.Diem += (int)dh.TongTien / 1000;
                db.SubmitChanges();

                // Nếu có voucher, cập nhật số lượt sử dụng
                if (Session["voucher"] != null)
                {
                    var voucherCode = Session["voucher"] as string;
                    var voucher = db.Vouchers.SingleOrDefault(u => u.MaVoucher == voucherCode);
                    voucher.SoLuotDaSD++;
                    db.SubmitChanges();
                    Session.Remove("voucher");
                }

                // Gửi email thông báo thành công
                SendEmail(madh);
                return View();
            }
            else
            {
                // Nếu thanh toán thất bại, hiển thị thông báo
                ViewBag.Message = "Thanh toán thất bại!";
            }

            return View("Error");
        }

        
        public ActionResult ThongTin()
        {
            var nguoidung = GetUserFromJwt();
            var tt = db.ThongTinCaNhans.SingleOrDefault(u => u.MaKH == nguoidung.MaKH);
            return View(tt);
        }
        public ActionResult Combo(float totalGiaTien, string selectedSeats)
        {
            ViewBag.TienGhe = totalGiaTien;
            List<int> seatList = JsonConvert.DeserializeObject<List<int>>(selectedSeats);
            List<string> seatNames = new List<string>();
            foreach (int seatId in seatList)
            {
                // Tìm ghế trong bảng Ghe dựa trên mã ghế
                Ghe seat = db.Ghes.FirstOrDefault(g => g.MaGhe == seatId);

                // Nếu tìm thấy ghế, thêm tên ghế vào danh sách
                if (seat != null)
                {
                    seatNames.Add(seat.TenGhe);
                }
            }
            Session["ListGhe"] = seatList;
            // Lưu danh sách tên ghế vào ViewBag để sử dụng trong View
            ViewBag.SeatNames = seatNames;
            var cb = db.Combos.ToList();
            var user = GetUserFromJwt();
            var diem = db.ThongTinCaNhans.SingleOrDefault(u => u.MaKH == user.MaKH).Diem;
            ViewBag.Diem = diem;
            return View(cb);
        }



        [SavePreviousPage]
        public ActionResult Payment()
        {
            if (Request.QueryString.Count > 0)
            {
                string vnp_HashSecret = ConfigurationManager.AppSettings["vnp_HashSecret"]; //Chuoi bi mat
                var vnpayData = Request.QueryString;
                VnPayLibrary vnpay = new VnPayLibrary();

                foreach (string s in vnpayData)
                {
                    if (!string.IsNullOrEmpty(s) && s.StartsWith("vnp_"))
                    {
                        vnpay.AddResponseData(s, vnpayData[s]);
                    }
                }
                string vnp_ResponseCode = vnpay.GetResponseData("vnp_ResponseCode");
                string vnp_TransactionStatus = vnpay.GetResponseData("vnp_TransactionStatus");
                String vnp_SecureHash = Request.QueryString["vnp_SecureHash"];

                bool checkSignature = vnpay.ValidateSignature(vnp_SecureHash, vnp_HashSecret);
                if (checkSignature)
                {
                    if (vnp_ResponseCode == "00" && vnp_TransactionStatus == "00")
                    {
                        List<int> listGhe = Session["ListGhe"] as List<int>;
                        List<ComboDat> listcb = Session["ListCB"] as List<ComboDat>;
                        long madh = (long)Session["MaDH"];
                        int macp = (int)Session["MaCP"];
                        foreach (var cb in listcb)
                        {
                            var combo = db.Combos.SingleOrDefault(u => u.MaCB == cb.MaCB);
                            CTHDComBo cthdcb = new CTHDComBo
                            {
                                MaCB = cb.MaCB,
                                SoLuong = cb.SoLuong,
                                GiaTien = combo.Gia.Value,
                                MaHoaDon = madh
                            };
                            db.CTHDComBos.InsertOnSubmit(cthdcb);
                            db.SubmitChanges();
                        }
                        foreach (var ghe in listGhe)
                        {
                            var lc = db.LichChieus.SingleOrDefault(u => u.MaChieuPhim == macp);
                            var g = db.Ghes.SingleOrDefault(u => u.MaPhong == lc.Phong && u.MaGhe == ghe);
                            ChiTietHoaDon cthd = new ChiTietHoaDon
                            {
                                ThoiGianDat = DateTime.Now.Date,
                                ThoiGianChieu = lc.NgayChieu,
                                TongTien = g.GiaTien,
                                MaHoaDon = madh,
                                MaLichChieu = macp,
                                MaGhe = ghe,
                            };
                            db.ChiTietHoaDons.InsertOnSubmit(cthd);
                            db.SubmitChanges();
                        }
                        var dh = db.HoaDons.SingleOrDefault(u => u.MaHoaDon == madh);
                        dh.TrangThai = "Đã thanh toán";
                        var user = GetUserFromJwt();
                        var userdb = db.ThongTinCaNhans.SingleOrDefault(u => u.MaKH == user.MaKH);
                        userdb.Diem += (int)dh.TongTien / 1000;
                        db.SubmitChanges();
                        if (Session["voucher"] != null)
                        {
                            var voucherCode = Session["voucher"] as string;
                            var voucher = db.Vouchers.SingleOrDefault(u => u.MaVoucher == voucherCode);
                            voucher.SoLuotDaSD++;
                            db.SubmitChanges();
                            Session.Remove("voucher");
                        }
                        SendEmail(madh);
                    }
                    else
                    {

                        ViewBag.InnerText = "Có lỗi xảy ra trong quá trình xử lý.Mã lỗi: " + vnp_ResponseCode;
                    }
                }
            }
            Session.Remove("voucher");
            return View();
        }
        [SavePreviousPage]
        public ActionResult ChonGhe(int id, int MaChieuPhim)
        {
            var lc = db.LichChieus.SingleOrDefault(u => u.MaChieuPhim == MaChieuPhim);
            var cthd = db.ChiTietHoaDons.Where(u => u.MaLichChieu == lc.MaChieuPhim && u.ThoiGianChieu.Value.Date == lc.NgayChieu.Value.Date && u.HoaDon.TrangThai == "Đã thanh toán");
            var ghe = db.Ghes.Where(u => u.MaPhong == lc.Phong);

            foreach (var chiTiet in cthd)
            {
                var gheTrongChiTiet = ghe.FirstOrDefault(g => g.MaGhe == chiTiet.MaGhe);
                if (gheTrongChiTiet != null)
                {
                    gheTrongChiTiet.TrangThai = "Đã đặt";
                    // Gửi thông điệp tới tất cả client để cập nhật trạng thái ghế
                    var hubContext = GlobalHost.ConnectionManager.GetHubContext<DatVe>();
                    hubContext.Clients.All.receiveSeatStatus(chiTiet.MaGhe, "");
                }
            }

            Session["MaCP"] = MaChieuPhim;
            // Cập nhật trạng thái "Trống" cho những ghế không trùng khớp
            foreach (var gheKhongTrongChiTiet in ghe.Where(g => !cthd.Any(c => c.MaGhe == g.MaGhe)))
            {
                gheKhongTrongChiTiet.TrangThai = "Trống";
            }
            // Cập nhật thay đổi vào cơ sở dữ liệu
            db.SubmitChanges();

            return View(ghe);
        }
        [SavePreviousPage]
        [HttpGet]
        public ActionResult TimKiem(string searchQuery, string trangThai, int? theLoaiId, int? page)
        {
            int pageSize = 9;
            int pageNumber = (page ?? 1);

            var query = db.Phims.AsQueryable(); // Bắt đầu với tất cả các phim

            // Thêm điều kiện tìm kiếm theo tên phim, đạo diễn, diễn viên và trạng thái
            if (!string.IsNullOrEmpty(searchQuery))
            {
                query = query.Where(p =>
                    p.TenPhim.Contains(searchQuery) ||
                    p.DaoDien.Contains(searchQuery) ||
                    p.DienVien.Contains(searchQuery));
            }

            if (!string.IsNullOrEmpty(trangThai))
            {
                query = query.Where(p => p.TrangThai.Contains(trangThai));
            }

            // Thêm điều kiện lọc theo thể loại
            if (theLoaiId.HasValue && theLoaiId > 0)
            {
                var tenTheLoai = db.TheLoais
                    .Where(u => u.MaTheLoai == theLoaiId)
                    .Select(u => u.TenTheLoai)
                    .FirstOrDefault();

                query = query.Where(p => p.TheLoai.Contains(tenTheLoai));
            }


            // Thực hiện phân trang và trả về kết quả
            var ketQuaTimKiem = query.ToPagedList(pageNumber, pageSize);

            ViewBag.TT = trangThai;
            ViewBag.TheloaiList = db.TheLoais.ToList();

            return View(ketQuaTimKiem);
        }
        [HttpPost]
        public ActionResult ThemBinhLuan(string noiDung, int maPhim, int? danhGia)
        {
            var user = GetUserFromJwt();
            if (user == null)
            {
                return RedirectToAction("Login", "Home");
            }

            DateTime thoiGian = DateTime.Now;
            int maKH = user.MaKH;

            // Giả sử bạn đã định nghĩa ViewBag.BL trước đó
            ViewBag.BL = db.BinhLuans
                .Where(bl => bl.MaPhim == maPhim) // Lọc bình luận theo mã phim
                .ToList();

            BinhLuan binhLuan = new BinhLuan
            {
                ThoiGian = thoiGian,
                NoiDung = noiDung,
                MaPhim = maPhim,
                MaKH = maKH,
                SoluongLike = 0, // Khởi tạo số lượng like về 0
                SoluongDislike = 0, // Khởi tạo số lượng dislike về 0
                DanhGia = danhGia // Không cần xử lý null vì đã định nghĩa là int?
            };

            db.BinhLuans.InsertOnSubmit(binhLuan);
            db.SubmitChanges();

            return RedirectToAction("Detail", new { id = maPhim });
        }

        [HttpPost]
        public ActionResult LikeComment(int maBL)
        {
            var binhLuan = db.BinhLuans.SingleOrDefault(bl => bl.MaBL == maBL);
            if (binhLuan != null)
            {
                binhLuan.SoluongLike++; // Tăng số lượng like
                db.SubmitChanges();
            }

            return RedirectToAction("Detail", new { id = binhLuan.MaPhim });
        }

        [HttpPost]
        public ActionResult DislikeComment(int maBL)
        {
            var binhLuan = db.BinhLuans.SingleOrDefault(bl => bl.MaBL == maBL);
            if (binhLuan != null)
            {
                binhLuan.SoluongDislike++; // Tăng số lượng dislike
                db.SubmitChanges();
            }

            return RedirectToAction("Detail", new { id = binhLuan.MaPhim });
        }






        public JsonResult GetDetails(int phimId, int? ngayChieu)
        {
            var phim = db.LichChieus.Where(u => u.MaPhim == phimId);
            string ngonNgu = "";
            var rapChieuTimes = new List<RapChieuTimes>();

            if (ngayChieu.HasValue)
            {
                phim = phim.Where(lc => lc.NgayChieu == DateTime.Now.Date.AddDays(ngayChieu.Value));
            }
            else
            {
                phim = phim.Where(lc => lc.NgayChieu == DateTime.Now.Date);
            }

            // Lấy ngôn ngữ của phim
            ngonNgu = phim.FirstOrDefault()?.NgonNgu;

            // Nhóm lịch chiếu theo rạp chiếu và lấy danh sách thời gian chiếu cho từng rạp chiếu
            var groupedByRapChieu = phim
                .GroupBy(lc => lc.MaRap)
                .Select(group => new RapChieuTimes
                {
                    RapChieu = group.First().RapChieu.TenRap,
                    GioChieuList = group
                        .Where(lc =>
                            lc.NgayChieu > DateTime.Now.Date || // Nếu ngày chiếu lớn hơn ngày hôm nay
                            (lc.NgayChieu == DateTime.Now.Date && lc.GioChieu.Value > DateTime.Now.TimeOfDay)) // Nếu ngày chiếu là hôm nay và giờ chiếu lớn hơn giờ hiện tại
                        .Select(lc => new Time
                        {
                            MaChieuPhim = lc.MaChieuPhim,
                            hours = lc.GioChieu.Value.Hours,
                            minutes = lc.GioChieu.Value.Minutes
                        })
                        .ToList()
                })
                .ToList();

            return Json(new { phimId = phimId, ngonNgu = ngonNgu, rapChieuTimes = groupedByRapChieu, ngayChieu = ngayChieu }, JsonRequestBehavior.AllowGet);
        }

        // Định nghĩa lớp RapChieuTimes để nhóm dữ liệu
        public class RapChieuTimes
        {
            public string RapChieu { get; set; }
            public List<Time> GioChieuList { get; set; }
        }

        public class Time
        {
            public int MaChieuPhim { get; set; }
            public int hours { get; set; }
            public int minutes { get; set; }
        }


        [SavePreviousPage]
        public ActionResult Detail(int id)
        {
            var binhLuans = db.BinhLuans.Where(u => u.MaPhim == id)
    .Include(b => b.ThongTinCaNhan)
    .ToList();

            ViewBag.BL = binhLuans;
            var ct = db.Phims.SingleOrDefault(u => u.MaPhim == id);
            return View(ct);
        }
        [SavePreviousPage]
        public ActionResult DetailNew(int id)
        {
            var ct = db.Tintucs.SingleOrDefault(u => u.Matin == id);
            return View(ct);
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

            // Tạo chuỗi thông tin cho mã QR, bao gồm thông tin chi tiết hóa đơn
            var qrContent = new
            {
                MaHoaDon = hd.MaHoaDon,
                KhachHang = hd.HoTen, // Giả sử bạn có trường này trong HoaDon
                TongTien = hd.TongTien, // Tổng tiền
                ChiTiet = cthd.Select(x => new
                {
                    TenPhim = x.phim.TenPhim,
                    Ghe = x.ghe.TenGhe,
                    ThoiGian = x.lichChieu.NgayChieu // Thời gian chiếu, nếu có
                }).ToList(),
                Combos = cthdcb.Select(x => new
                {
                    Combo = x.cb.TenCB,
                    Nuoc = x.nuoc.TenNuoc,
                    DoAn = x.doan.TenDoAn
                }).ToList()
            };

            // Chuyển đổi nội dung thành chuỗi JSON
            string qrContentJson = JsonConvert.SerializeObject(qrContent);

            // Tạo mã QR với nội dung JSON
            ViewBag.QRCode = QRCodeGenerator.GenerateQRCode(qrContentJson);

            return View();
        }



        private void SendNewPasswordByEmail(string userEmail, string newPassword)
        {
            // Thực hiện việc gửi email
            // Thông tin cấu hình SMTP và Email Server
            string smtpServer = "smtp.gmail.com";
            int smtpPort = 587; // Port của SMTP Server
            string smtpUsername = "49sneaker@gmail.com";
            string smtpPassword = "lxmx jwbk ahds hlzr";

            // Địa chỉ email người gửi
            string fromEmail = "49sneaker@gmail.com";

            // Tạo đối tượng MailMessage
            MailMessage mailMessage = new MailMessage(fromEmail, userEmail)
            {
                Subject = "Reset Password",
                Body = $"Mật khẩu mới của bạn là: {newPassword}",
                IsBodyHtml = false
            };

            // Tạo đối tượng SmtpClient
            SmtpClient smtpClient = new SmtpClient(smtpServer, smtpPort)
            {
                Credentials = new NetworkCredential(smtpUsername, smtpPassword),
                EnableSsl = true
            };

            // Gửi email
            try
            {
                smtpClient.Send(mailMessage);
            }
            catch (Exception ex)
            {
                // Xử lý nếu có lỗi khi gửi email
                // Log lỗi hoặc thông báo lỗi cho người quản trị hệ thống
                Console.WriteLine(ex.Message);
            }
        }
        private void SendEmail(long id)
        {
            // Lấy thông tin hóa đơn
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

            // Lấy thông tin Combo
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

            // Thông tin người dùng
            var nguoidung = GetUserFromJwt();
            var user = db.ThongTinCaNhans.SingleOrDefault(u => u.MaKH == nguoidung.MaKH);
            string email = user.Email;

            // Tạo nội dung cho QR code
            var firstDetail = cthd.FirstOrDefault();
            var comboDetails = cthdcb.ToList();

            // Xây dựng nội dung QR code với thông tin cần thiết
            string qrCode = $"Ten Phim: {firstDetail?.phim.TenPhim}\n----------------------------------------------------\n" +
                $"Thoi gian chieu: {firstDetail?.lichChieu.NgayChieu:dd/MM/yyyy} - {firstDetail?.lichChieu.GioChieu}\n---------------------------------------------------\n" +
                $"Phong chieu: {firstDetail?.phong.TenPhong}\n---------------------------------------------------\n" +
                $"Ghe ngoi: {firstDetail?.ghe.TenGhe}\n---------------------------------------------------\n";


            // Tạo nội dung email
            string emailBody = GenerateEmailHtml(id, qrCode, comboDetails, cthd.ToList());

            // Thông tin email của bạn
            string senderEmail = "49sneaker@gmail.com";
            string senderPassword = "lxmx jwbk ahds hlzr";
            string smtpServer = "smtp.gmail.com";
            int smtpPort = 587; // Hoặc cổng SMTP tương ứng của bạn

            // Tạo SmtpClient
            SmtpClient smtpClient = new SmtpClient(smtpServer)
            {
                Port = smtpPort,
                Credentials = new NetworkCredential(senderEmail, senderPassword),
                EnableSsl = true
            };

            // Tạo MailMessage
            MailMessage mailMessage = new MailMessage(senderEmail, email)
            {
                Subject = "Thông Tin Hóa Đơn",
                Body = emailBody,
                IsBodyHtml = true
            };

            string serverPath = Server.MapPath("~");
            string imagePath = QRCodeGenerator.SaveQRCodeToFile(id, qrCode, serverPath);
            Attachment attachment = new Attachment(imagePath);
            mailMessage.Attachments.Add(attachment);

            try
            {
                smtpClient.Send(mailMessage);
            }
            catch (Exception ex)
            {
                // Xử lý nếu có lỗi khi gửi email
                // Log lỗi hoặc thông báo lỗi cho người quản trị hệ thống
                Console.WriteLine(ex.Message);
            }
        }







        private string GenerateEmailHtml(long id, string qrCode, List<CTHDCB> cb, List<CTHD> ghe)
        {
            var hd = db.HoaDons.SingleOrDefault(u => u.MaHoaDon == id);
            // Chuỗi HTML cố định
            string html = $@"
        <div class='container mt-4 text-center'>
            <h2 class='text-white' style='font-weight:bold'>Thông Tin Hóa Đơn Cinema</h2>

            <p class='text-white' style='font-weight:bold'>Mã QR ở trong file đính kèm:</p>
             <div class='mt-4'>
        <table class='table table-bordered table-dark'>
            <thead>
                <tr>
                    <th>Mã hóa đơn</th>
                    <th>Họ tên</th>
                    <th>Số điện thoại</th>
                    <th>Email</th>
                    <th>Trạng thái</th>
                    <th>Tiền vé</th>
                    <th>Tiền combo</th>
                    <th>Tổng tiền</th>
                </tr>
            </thead>
            <tbody>";


            html += $@"  
                    <tr>
                        <td>{hd.MaHoaDon}</td>
                        <td>{hd.HoTen}</td>
                        <td>{hd.SoDienThoai}</td>
                        <td>{hd.Email}</td>
                        <td>{hd.TrangThai}</td>
                        <td>{hd.TienVe}</td>
                        <td>{hd.TienCombo}</td>
                        <td>{hd.TongTien}</td>
                   </tr>";

            html += @"
</tbody>
        </table>
    </div>
            <div class='mt-4'>
                <h3 class='text-white' style='font-weight:bold'>Chi Tiết Combo</h3>
                <table class='table table-bordered table-dark'>
                    <thead> 
                        <tr>
                            <th>Tên Combo</th>
                            <th>Mô tả</th>
                            <th>Tên đồ ăn</th>
                            <th>Số lượng đồ ăn</th>
                            <th>Size đồ ăn</th>
                            <th>Tên nước</th>
                            <th>Số lượng nước</th>
                            <th>Size nước</th>
                            <th>Số lượng combo đã mua</th>
                            <th>Giá Combo</th>
                        </tr>
                    </thead>
                    <tbody>";

            // Thêm dòng cho mỗi phần tử trong danh sách cb
            foreach (var item in cb)
            {
                html += $@"
                        <tr>
                            <td>{item.cb.TenCB}</td>
                            <td>{item.cb.MoTa}</td>
                            <td>{item.doan.TenDoAn}</td>
                            <td>{item.cb.SLDoAn}</td>
                            <td>{item.doan.Loai}</td>
                            <td>{item.nuoc.TenNuoc}</td>
                            <td>{item.cb.SLNuoc}</td>
                            <td>{item.nuoc.Loai}</td>
                            <td>{item.cthd.SoLuong}</td>
                            <td>{item.cthd.GiaTien}</td>
                        </tr>";
            }

            // Thêm phần HTML cho chi tiết ghế vào chuỗi
            html += @"
                    </tbody>
                </table>
            </div>

            <div class='mt-4'>
                <h3 class='text-white' style='font-weight:bold'>Chi Tiết Vé đã mua</h3>
                <table class='table table-bordered table-dark'>
                    <thead>
                        <tr>
                            <th>Tên phim</th>
                            <th>Thời lượng</th>
                            <th>Phòng</th>
                            <th>Tên ghế</th>
                            <th>Thời gian chiếu</th>
                            <th>Giá Tiền</th>
                        </tr>
                    </thead>
                    <tbody>";

            // Thêm dòng cho mỗi phần tử trong danh sách ghe
            foreach (var item in ghe)
            {
                html += $@"
                        <tr>
                            <td>{item.phim.TenPhim}</td>
                            <td>{item.phim.ThoiLuong} phút</td>
                            <td>{item.phong.TenPhong}</td>
                            <td>{item.ghe.TenGhe}</td>
                            <td>{item.lichChieu.NgayChieu:dd/MM/yyyy} {item.lichChieu.GioChieu}</td>
                            <td>{item.chiTiet.TongTien}</td>
                        </tr>";
            }

            // Kết thúc chuỗi HTML
            html += @"
                    </tbody>
                </table>
            </div>
        </div>";

            return html;
        }


        [SavePreviousPage]
        public ActionResult News(int? page)
        {
            int pageSize = 6; // Số sản phẩm trên mỗi trang
            int pageNumber = (page ?? 1); // Trang mặc định là trang 1 nếu không có tham số page
            var tt = db.Tintucs.ToPagedList(pageNumber, pageSize);
            ViewBag.Slider = db.Sliders.ToList();
            return View(tt);
        }
        [SavePreviousPage]
        // GET: Home
        public ActionResult Index()
        {
            var currentDate = DateTime.Now;

     
            
            var phim = db.Phims.ToList();
            var sliders = db.Sliders.ToList(); // Lấy danh sách hình ảnh từ bảng Slider
            ViewBag.Sliders = sliders; // Truyền danh sách slider vào view
            ViewBag.TheloaiList = db.TheLoais.ToList();
            ViewBag.PhimList = db.LichChieus
                         .Where(l => l.NgayChieu >= DateTime.Now.Date) // Kiểm tra ngày giờ lớn hơn hiện tại
                         .Select(l => l.Phim) // Lấy thông tin phim từ lịch chiếu
                         .Distinct() // Loại bỏ phim trùng lặp
                         .ToList();

            return View(phim);

        }
        public JsonResult CheckVoucher(string voucherCode)
        {
            // Kiểm tra voucher trong cơ sở dữ liệu
            var voucher = db.Vouchers.FirstOrDefault(v => v.MaVoucher == voucherCode && v.TrangThai == true && v.SoLuotDaSD < v.MaxLuotSD && v.NgayHetHan >= DateTime.Now);

            if (voucher != null)
            {
                // Voucher hợp lệ, trả lại discount (giả sử voucher có trường Discount là phần trăm giảm giá)
                return Json(new { isValid = true, discount = voucher.GiamGiaPhanTram });
            }

            // Nếu voucher không hợp lệ
            return Json(new { isValid = false, discount = 0 });
        }
        [HttpPost]
        public async Task<ActionResult> SubmitFeedback(string name, string email, string message)
        {
            // Kiểm tra dữ liệu đầu vào
            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(message))
            {
                TempData["Error"] = "Vui lòng điền đầy đủ thông tin.";
                return RedirectToAction("Index"); // Hoặc trang hiện tại
            }

            var feedback = new Feedback
            {
                Name = name,
                Email = email,
                Message = message,
                SubmittedAt = DateTime.Now // Lưu thời gian gửi
            };

            // Lưu phản hồi vào cơ sở dữ liệu
            using (var db = new ApplicationDbContext())
            {
                db.Feedbacks.Add(feedback); // Thêm feedback mới vào DbSet
                await db.SaveChangesAsync(); // Lưu các thay đổi vào cơ sở dữ liệu
            }

            // Gửi email
            var senderEmail = new MailAddress("49sneaker@gmail.com", "Cinema Feedback");
            var receiverEmail = new MailAddress("49sneaker@gmail.com", "Cinema Admin"); // Email nhận phản hồi
            var password = "lxmx jwbk ahds hlzr"; // Mật khẩu email của bạn
            var sub = "Feedback từ người dùng";
            string body = $"Tên người gửi: {name}\nEmail: {email}\nNội dung phản hồi: {message}";

            var smtp = new SmtpClient
            {
                Host = "smtp.gmail.com", // SMTP server của Gmail
                Port = 587,
                EnableSsl = true,
                DeliveryMethod = SmtpDeliveryMethod.Network,
                UseDefaultCredentials = false,
                Credentials = new NetworkCredential(senderEmail.Address, password)
            };

            using (var mess = new MailMessage(senderEmail, receiverEmail)
            {
                Subject = sub,
                Body = body
            })
            {
                try
                {
                    await smtp.SendMailAsync(mess);
                    TempData["Message"] = "Cảm ơn bạn đã gửi phản hồi!";
                }
                catch (Exception)
                {
                    TempData["Error"] = "Đã có lỗi xảy ra khi gửi phản hồi. Vui lòng thử lại.";
                }
            }

            return RedirectToAction("Index"); // Điều hướng về trang chính hoặc trang bạn mong muốn
        }




        public ActionResult Blog(int? page)
        {
            int pageSize = 5; // Số bài viết trên mỗi trang
            int pageNumber = page ?? 1; // Trang hiện tại, mặc định là 1 nếu không có giá trị
            var blogs = db.Blogs.OrderBy(b => b.NgayDang).ToPagedList(pageNumber, pageSize);
            return View(blogs);
        }



        public ActionResult DetailBlog(int id)
        {
            // Lấy bài viết theo MaBlog bằng cách sử dụng FirstOrDefault
            var blog = db.Blogs.FirstOrDefault(b => b.MaBlog == id);

            if (blog == null)
            {
                return HttpNotFound(); // Trả về 404 nếu không tìm thấy bài viết
            }

            return View(blog); // Truyền dữ liệu đến view
        }

        public ActionResult QRCheck()
        {

            return View();
        }
        public ActionResult PrintInvoice(long id)
        {
            // Truy vấn trực tiếp hóa đơn và các chi tiết liên quan
            var hd = db.HoaDons.FirstOrDefault(h => h.MaHoaDon == id);

            // Lấy thông tin chi tiết combo
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

            // Lấy thông tin chi tiết vé
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

            using (var stream = new MemoryStream())
            {
                using (var writer = new PdfWriter(stream))
                {
                    using (var pdf = new PdfDocument(writer))
                    {
                        var document = new iText.Layout.Document(pdf);

                        // Thêm trang mới
                        pdf.AddNewPage(PageSize.A4);
                        var pageSize = PageSize.A4;

                        // Thêm hình nền
                        var imagePath = Server.MapPath("~/IMG/bghoadon.png");
                        var backgroundImage = new iText.Layout.Element.Image(iText.IO.Image.ImageDataFactory.Create(imagePath));
                        backgroundImage.SetFixedPosition(0, 0);
                        backgroundImage.SetHeight(pageSize.GetHeight());
                        backgroundImage.SetWidth(pageSize.GetWidth());
                        document.Add(backgroundImage);

                        // Tiêu đề hóa đơn
                        var title = new Paragraph("VE XEM PHIM")
                            .SetFontSize(24)
                            .SetBold()
                            .SetTextAlignment(TextAlignment.CENTER)
                            .SetMarginBottom(40);
                        document.Add(title);
                        document.Add(new LineSeparator(new SolidLine()));
                        document.Add(new Paragraph("THONG TIN KHACH HANG")
                            .SetFontSize(18)
                            .SetBold()
                            .SetMarginTop(10));
                        document.Add(new Paragraph($"Ma Hoa Don: {hd.MaHoaDon}")
                            .SetFontSize(12)
                            .SetMarginBottom(5));
                        document.Add(new Paragraph($"Ho Va Ten: {hd.HoTen}")
                            .SetFontSize(12)
                            .SetMarginBottom(5));
                        document.Add(new Paragraph($"So Dien Thoai: {hd.SoDienThoai}")
                            .SetFontSize(12)
                            .SetMarginBottom(5));
                        document.Add(new Paragraph($"Tien Ve: {hd.TienVe} VND")
                            .SetFontSize(12)
                            .SetMarginBottom(5));
                        document.Add(new Paragraph($"Tien Combo: {hd.TienCombo} VND")
                            .SetFontSize(12)
                            .SetMarginBottom(5));
                        document.Add(new Paragraph($"Tong Tien: {hd.TongTien} VND")
                            .SetFontSize(12)
                            .SetMarginBottom(20));

                        document.Add(new LineSeparator(new SolidLine()));
                        // Chi tiết combo
                        document.Add(new Paragraph("CHI TIET COMBO")
                            .SetFontSize(18)
                            .SetBold()
                            .SetMarginTop(10));
                        foreach (var item in cthdcb)
                        {
                            document.Add(new Paragraph($"Ten Combo: {item.cb.TenCB}, Mo Ta: {item.cb.MoTa}, Gia: {item.cthd.GiaTien} VND")
                                .SetFontSize(12)
                                .SetMarginBottom(5));
                        }
                        document.Add(new LineSeparator(new SolidLine()));
                        // Chi tiết vé đã mua
                        document.Add(new Paragraph("CHI TIET VE DA MUA")
                            .SetFontSize(18)
                            .SetBold()
                            .SetMarginTop(10));
                        foreach (var item in cthd)
                        {
                            document.Add(new Paragraph($"Ten Phim: {item.phim.TenPhim}, Phong Chieu: {item.phong.TenPhong}, Ghe: {item.ghe.TenGhe}, Thoi Gian Chieu: {item.lichChieu.NgayChieu:dd/MM/yyyy} {item.lichChieu.GioChieu}")
                                .SetFontSize(12)
                                .SetMarginBottom(5));
                        }

                        document.Close();
                    }
                }

                byte[] bytes = stream.ToArray();
                return File(bytes, "application/pdf", "CINEMA_Ticket.pdf");
            }
        }
        public ActionResult CancelTicket(long id)
        {
            var hoaDon = db.HoaDons.FirstOrDefault(hd => hd.MaHoaDon == id);
            var cthd = db.ChiTietHoaDons.FirstOrDefault(u => u.MaHoaDon == hoaDon.MaHoaDon);

            if (hoaDon != null)
            {
                if (DateTime.Now >= cthd.LichChieu.NgayChieu)
                {
                    TempData["Error"] = "Không thể hủy vé vì cần phải hủy vé trước 1 ngày";
                    return RedirectToAction("Ve");
                }
                // Kiểm tra nếu trạng thái là "Đã thanh toán"
                if (hoaDon.TrangThai == "Đã thanh toán")
                {
                    hoaDon.TrangThai = "Đã hủy"; // Thay đổi trạng thái thành "Đã hủy"
                    var user = GetUserFromJwt();
                    if (user != null)
                    {
                        var userdb = db.ThongTinCaNhans.SingleOrDefault(u => u.MaKH == user.MaKH);
                        userdb.Diem += (int)hoaDon.TongTien;
                    }
                    db.SubmitChanges(); // Lưu thay đổi vào cơ sở dữ liệu
                    TempData["Success"] = "Hủy vé thành công!";
                    return RedirectToAction("Ve");
                }
                else
                {
                    TempData["Error"] = "Không thể hủy vé vì trạng thái không phải là 'Đã thanh toán'.";
                    return RedirectToAction("Ve");
                }
            }
            else
            {
                TempData["Error"] = "Hóa đơn không tồn tại.";
                return RedirectToAction("Ve");
            }


        }

        public JsonResult GetRapByPhim(int maPhim)
        {
            var rapList = db.RapChieus
                            .Where(r => r.LichChieus.Any(lc => lc.MaPhim == maPhim))
                            .Select(r => new { r.MaRap, r.TenRap })
                            .ToList();
            return Json(rapList, JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetNgayByRap(int maRap, int maPhim)
        {
            var ngayList = db.LichChieus
                    .Where(lc => lc.MaRap == maRap && lc.MaPhim == maPhim && lc.NgayChieu >= DateTime.Now)
                    .Select(lc => lc.NgayChieu) // Chỉ lấy NgayChieu từ CSDL
                    .Distinct() // Loại bỏ các giá trị trùng lặp
                    .ToList() // Thực hiện truy vấn trước
                    .Where(nc => nc.HasValue) // Loại bỏ giá trị null
                    .Select(nc => nc.Value.ToString("dd-MM-yyyy")) // Định dạng ngày sau khi lấy từ CSDL
                    .ToList();
            return Json(ngayList, JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetSuatByNgay(int maRap, string ngayChieu)
        {
            DateTime parsedNgayChieu;
            if (!DateTime.TryParseExact(ngayChieu, "yyyy-MM-dd", null, System.Globalization.DateTimeStyles.None, out parsedNgayChieu))
            {
                return Json(new { error = "Invalid date format" }, JsonRequestBehavior.AllowGet);
            }

            // Kiểm tra nếu ngày người dùng chọn bằng ngày hiện tại
            var suatList = db.LichChieus
                .Where(lc => lc.MaRap == maRap && lc.NgayChieu == parsedNgayChieu.Date &&
                    (lc.NgayChieu > DateTime.Now.Date || lc.GioChieu >= DateTime.Now.TimeOfDay)) // Kiểm tra ngày và giờ
                .Select(lc => new
                {
                    lc.MaChieuPhim,
                    lc.GioChieu
                })
                .ToList() // Thực hiện truy vấn
                .Select(lc => new
                {
                    lc.MaChieuPhim,
                    GioBatDau = lc.GioChieu.HasValue ? lc.GioChieu.Value.ToString(@"hh\:mm") : null // Định dạng giờ
                })
                .ToList();

            return Json(suatList, JsonRequestBehavior.AllowGet);
        }
        public ActionResult DSVoucher(int? page, int? size)
        {

            // Mặc định trang và kích thước trang nếu không được truyền
            int pageNumber = page ?? 1;
            int pageSize = size ?? 5;

            // Truy vấn dữ liệu từ cơ sở dữ liệu
            var vouchers = db.Vouchers.Where(v => v.TrangThai == true && v.SoLuotDaSD < v.MaxLuotSD && v.NgayHetHan >= DateTime.Now).OrderBy(v => v.ID).ToPagedList(pageNumber, pageSize);

            return View(vouchers);
        }



    }
}


