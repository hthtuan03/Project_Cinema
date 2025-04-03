using DuAnRapChieuPhim.Models;
using Microsoft.AspNet.SignalR;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web;
using System.Collections.Concurrent;
using System.Threading;

namespace DuAnRapChieuPhim.Hubs
{
    public class DatVe : Hub
    {
        private static ConcurrentDictionary<string, Timer> seatTimers = new ConcurrentDictionary<string, Timer>();

        public void Hello()
        {
            Clients.All.hello();
        }

        public void UpdateSeatStatus(string MaGhe, string status)
        {
            Clients.All.receiveSeatStatus(MaGhe, status);
        }

        public void SelectSeat(string MaGhe)
        {
            Clients.Others.lockSeat(MaGhe);

            Timer timer = new Timer(TimerCallback, MaGhe, 30000, Timeout.Infinite);
            seatTimers[MaGhe] = timer;
        }

        public void DeselectSeat(string MaGhe)
        {
            Clients.Others.unlockSeat(MaGhe);

            if (seatTimers.TryRemove(MaGhe, out Timer timer))
            {
                timer.Dispose();
            }
        }

        public void TimeoutSeat(string MaGhe)
        {
            Clients.All.revertSeat(MaGhe);

            if (seatTimers.TryRemove(MaGhe, out Timer timer))
            {
                timer.Dispose();
            }
        }

        private void TimerCallback(object state)
        {
            string MaGhe = (string)state;
            TimeoutSeat(MaGhe);
        }
    }
}
