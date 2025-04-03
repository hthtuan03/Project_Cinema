using System.Data.Entity;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;

namespace DuAnRapChieuPhim.Models
{
    // You can add profile data for the user by adding more properties to your ApplicationUser class, please visit https://go.microsoft.com/fwlink/?LinkID=317594 to learn more.
    public class ApplicationUser : IdentityUser
    {
        public async Task<ClaimsIdentity> GenerateUserIdentityAsync(UserManager<ApplicationUser> manager)
        {
            // Note the authenticationType must match the one defined in CookieAuthenticationOptions.AuthenticationType
            var userIdentity = await manager.CreateIdentityAsync(this, DefaultAuthenticationTypes.ApplicationCookie);
            // Add custom user claims here
            return userIdentity;
        }
    }

    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        
        public DbSet<Feedback> Feedbacks { get; set; }
        public DbSet<Voucher> Vouchers  { get; set; }
        public ApplicationDbContext()
            : base("CinemaConnectionString", throwIfV1Schema: false)
        {
        }

        public class DbContext : System.Data.Entity.DbContext
        {
            public DbSet<Ghe> Ghes { get; set; }
            
            // Các DbSet khác nếu có
        }

        public static ApplicationDbContext Create()
        {
            return new ApplicationDbContext();
        }
    } 
    
    
}