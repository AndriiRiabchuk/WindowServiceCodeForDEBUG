using Auction.DAL;
using Microsoft.EntityFrameworkCore;
using System;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.ServiceProcess;
using System.Threading;

namespace WindowServiceCodeForDEBUG
{

    class Program
    {
        private static int N = 0;

        static void Main(string[] args)
        {
            ServiceBase.Run(new Service());

        }

        public static void LotMonitor()
        {

            TimerCallback tm = new TimerCallback(finish);


            Timer timer = new Timer(tm, initializeDB(), 0, 60*1000);

            while (true)
            {

            };
        }

        public static void finish(object poo)
        {
            Console.WriteLine("FINISH");

            Context db = (Context)poo;
            db.Lots.Where(l => !l.IsFinished).Where(l => l.EndDate <= DateTime.Now).Include(l => l.Owner).Include(l => l.Winner).Include(l => l.Bids).ToList()
                .ForEach(l=>finishLot(l,db));
        }

        public static void finishLot(Lot lot,Context db)
        {
            
            
            var toOwner = Message.AuctionFinished(lot);
            var toWinner = Message.BidWin(lot);
           
            lot.IsFinished = true;
            db.Update(lot);
            db.SaveChanges();
            if (lot.Winner == null)
            {
                return;
            }
            var winner = db.Users.Find(lot.Winner.Id);
            var owner = db.Users.Find(lot.Owner.Id);

            owner.AddCash(winner.Withdraw(lot.Final_Price));
            db.Users.Update(winner);
            db.Users.Update(owner);
            db.SaveChanges();

            db.Messages.Add(toOwner);
            db.Messages.Add(toWinner);
            db.SaveChanges();
        }

        public static Context initializeDB()
        {
            string con = "data source = PC0007;Initial Catalog=Who; User Id = user; Password = 123";

            var optionsBuilder = new DbContextOptionsBuilder<Context>();
            optionsBuilder.UseSqlServer(con);

            return new Context(optionsBuilder.Options);
        }


        public class Context : DbContext
        {
            public DbSet<User> Users { get; set; }
            public DbSet<UserConfiguration> UserConfiguration { get; set; }
            public DbSet<Lot> Lots { get; set; }
            public DbSet<Bid> Bids { get; set; }
            public DbSet<Message> Messages { get; set; }

            public Context(DbContextOptions<Context> options)
                : base(options)
            {
                //Database.EnsureDeleted();
                Database.EnsureCreated();
            }

            protected override void OnModelCreating(ModelBuilder modelBuilder)
            {
                modelBuilder.Entity<ApplicationUser>()
                    .HasOne(c => c.User)
                    .WithOne(d => d.ApUser);
                modelBuilder.Entity<Lot>()
                    .HasOne(lot => lot.Owner)
                    .WithMany(ow => ow.LotOwners);
                modelBuilder.Entity<Lot>()
                    .HasOne(lot => lot.Winner)
                    .WithMany(ow => ow.LotWinners);
          
                base.OnModelCreating(modelBuilder);
            }

        }


        public static void SendMessages()
        {
            TimerCallback tm = new TimerCallback(SendMessageFromDB);

            Timer timer = new Timer(tm, initializeDB(), 0, 60*1000);
            while (true) { };
        }

        public static void SendMessageFromDB(object poo)
        {
            Console.WriteLine("HERE I AM");
            Context db = (Context)poo;
            var mes = db.Messages.Skip(N).Where(m => !m.IsSended).ToList();
            N += mes.Count;
            mes.ForEach(m =>GetMail(m.Email, m.Text));
            mes.ForEach(m => { m.IsSended = true; db.Update(m); });

            db.SaveChanges();
        }

        public static void GetMail(string email, String message)
        {
            var smtpClient = new SmtpClient("smtp.gmail.com")
            {
                Port = 587,
                Credentials = new NetworkCredential("mailsystemauction@gmail.com", "22041980lL"),
                EnableSsl = true,
            };

            smtpClient.Send("mailsystemauction@gmail.com", email, "sasasa", message);
        }
    }
}
