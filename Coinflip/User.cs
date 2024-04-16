namespace CoinFlip
{
    internal class User
    {
        public int Id { get; set; }
        public string Email { get; set; } = null!;
        public string Password { get; set; } = null!;
        public string UserName { get; set; } = null!;
        public double Bet { get; set; }
        public double Balance { get; set; }
    }
}
