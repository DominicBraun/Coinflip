using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using System;
using System.Text;
using CoinFlip.Daten;
using static System.Formats.Asn1.AsnWriter;
using System.Diagnostics;

namespace CoinFlip
{
    class Program
    {
        public static async Task Main(string[] args)
        {
            CoinflipContext dbContext = new CoinflipContext();
            User AktuellerBenutzer = null;
            bool loginloop = true;
            while (loginloop)
            {
                Console.Clear();
                Console.WriteLine("Willkommen im Coinflip");
                AktuellerBenutzer = await Anmeldung();
                if (AktuellerBenutzer != null)
                {
                    loginloop = false;
                }
            }
            int streak = 0;
            while (true)
            {
                Console.Clear();
                Console.WriteLine($"Willkommen im Coinflip, Sie sind eingeloggt als {AktuellerBenutzer.UserName}");
                Console.WriteLine($"Menu:\n1. Spielen\n2. Leaderboard\n3. Beenden");
                string Menuauswahl = Console.ReadLine();
                switch (Menuauswahl)
                {
                    case "1":
                        Console.Clear();
                        Random rand = new Random();
                        double winnings = 0;
                        Console.WriteLine("Welcome to the coin flip game!");
                        Console.WriteLine($"Aktuell Guthaben {AktuellerBenutzer.Balance}");
                        Console.WriteLine("Heads/Tails");
                        string Option = Console.ReadLine();
                        Console.WriteLine("Enter your bet amount:");
                        double betAmount = Convert.ToDouble(Console.ReadLine());

                        int flipResult = rand.Next(2); // generates a random number between 0 and 1

                        if (flipResult == 0)
                        {
                            Console.WriteLine("Heads");
                            if (Option == "Heads")
                            {
                                winnings += betAmount;
                                streak ++;
                                User UserBalanceEchange = await dbContext.User.FirstOrDefaultAsync(u => u.Id == AktuellerBenutzer.Id); // Angenommen, die ID ist bekannt
                                if (UserBalanceEchange != null)
                                {
                                    UserBalanceEchange.Balance += betAmount;
                                    await dbContext.SaveChangesAsync();
                                }
                            }
                            if (Option == "Tails")
                            {
                                winnings -= betAmount;
                                streak = 0;
                                User UserBalanceEchange = await dbContext.User.FirstOrDefaultAsync(u => u.Id == AktuellerBenutzer.Id); // Angenommen, die ID ist bekannt
                                if (UserBalanceEchange != null)
                                {
                                    UserBalanceEchange.Balance -= betAmount;
                                    await dbContext.SaveChangesAsync();
                                }
                            }
                        }
                        else
                        {
                            Console.WriteLine("Tails");
                            if (Option == "Tails")
                            {
                                winnings += betAmount;
                                streak++;
                                if (AktuellerBenutzer != null)
                                {
                                    AktuellerBenutzer.Balance += betAmount;
                                    await dbContext.SaveChangesAsync();
                                }
                            }
                            if (Option == "Heads")
                            {
                                winnings -= betAmount;
                                streak = 0;
                                if (AktuellerBenutzer != null)
                                {
                                    AktuellerBenutzer.Balance -= betAmount;
                                    await dbContext.SaveChangesAsync();
                                }
                            }
                        }
                        dbContext.Leaderboards.Add(new Leaderboard { UserId = AktuellerBenutzer.Id, Gewinn = winnings, Winstreak = streak});
                        await dbContext.SaveChangesAsync();
                        Console.WriteLine($"Winnings {winnings} ");
                        Console.WriteLine($"Balance {AktuellerBenutzer.Balance}");
                        Console.ReadKey();
                        break;
                    case "2":
                        LeaderBoardAnzeigen(AktuellerBenutzer ,dbContext);
                        break;
                    case "3":
                        Environment.Exit(0);
                        break;
                }
            }
        }
        static async Task LeaderBoardAnzeigen(User AktuellerBenutzer, CoinflipContext dbContext)
        {
            Console.Clear();
            var AlleScoreBoards = await dbContext.Leaderboards
                                                  .Include(l => l.UserUser) // Includiert die Kundendaten
                                                  .Where(l => l.UserUser.UserName == AktuellerBenutzer.UserName)
                                                  .ToListAsync();

            Console.WriteLine("-----------------------------------------");
            Console.WriteLine("Alle Scores:");
            foreach (var Score in AlleScoreBoards)
            {
                Console.WriteLine();
                Console.Write($"ID: {Score.Id} ".PadRight(10));
                Console.Write($"Winstreak: {Score.Winstreak} ".PadRight(20));
                Console.Write($"Gewinn: {Score.Gewinn} ".PadRight(20));
                Console.Write($"User: {Score.UserUser.UserName}");
            }
            Console.ReadKey();
        }
        public static async Task<User> Anmeldung()
        {
            CoinflipContext dbContext = new CoinflipContext();
            Console.WriteLine("\n1.Anmelden\n2.Registrieren\n3.Beenden");
            string Anmeldewauswahl = Console.ReadLine();
            switch (Anmeldewauswahl)
            {
                case "1":
                    Console.Write("Bitte gib deinen Benutzernamen ein: ");
                    string benutzernameLogin = Console.ReadLine();

                    Console.Write("Bitte gib dein Passwort ein: ");
                    string passwortLogin = Console.ReadLine();

                    User benutzerLogin = await dbContext.User.FirstOrDefaultAsync(u => u.UserName == benutzernameLogin && u.Password == passwortLogin);
                    if (benutzerLogin == null)
                    {
                        Console.WriteLine("Benutzername oder Passwort ist falsch.");
                        Console.ReadKey();
                        return null;
                    }
                    Console.Clear();
                    return benutzerLogin;
                case "2":
                    Console.WriteLine("Bitte geben sie ihren Benutzernamen ein: ");
                    string benutzername = Console.ReadLine();

                    Console.WriteLine("Bitte geben sie ihr Passwort ein: ");
                    string passwortregister = Console.ReadLine();

                    Console.WriteLine("Bitte geben sie ihre E-Mail-Adresse ein: ");
                    string email = Console.ReadLine();


                    User benutzerNeu = new User { UserName = benutzername, Password = passwortregister, Email = email, Balance = 100};

                    dbContext.User.Add(benutzerNeu);

                    dbContext.SaveChanges();

                    Console.Clear();
                    return benutzerNeu;
                case "3":
                    Environment.Exit(0);
                    return null;
                default:
                    Console.WriteLine("Falsche Eingabe. Versuche es erneut!");
                    Console.ReadKey();
                    Console.Clear();
                    return null;
            }
        }
    }
}