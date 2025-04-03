using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DuAnRapChieuPhim.Models
{
    public class CTHDCB
    {
        public CTHDComBo cthd { get; set; }
        public Combo cb { get; set; }
        public Nuoc nuoc { get; set; }
        public DoAn doan { get; set; }
    }
}