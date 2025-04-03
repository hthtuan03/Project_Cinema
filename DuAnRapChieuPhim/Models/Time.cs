using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DuAnRapChieuPhim.Models
{
    public class Time
    {
        public int MaChieuPhim { get;set;}
        public int hours { get; set; }
        public int minutes { get; set; }
        public int seconds { get; set; }
    }
    public class DoanhThuViewModel
    {
        public List<string> Labels { get; set; }
        public List<decimal> TicketData { get; set; }
        public List<decimal> ComboData { get; set; }
        public List<int> TicketCountByDay { get; internal set; }
        public List<int> TicketCountByMonth { get; internal set; }
        public List<int> TicketCountByYear { get; internal set; }
    }
}