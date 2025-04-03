using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DuAnRapChieuPhim.Models
{
    public class CTHD
    {
        public ChiTietHoaDon chiTiet { get; set; }
        public LichChieu lichChieu { get; set; }
        public Ghe ghe { get; set; }
        public Phong phong { get; set; }
        public Phim phim { get; set; }
    }
}