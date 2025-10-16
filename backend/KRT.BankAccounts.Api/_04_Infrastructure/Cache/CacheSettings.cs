namespace KRT.BankAccounts.Api._04_Infrastructure.Cache
{
    public class CacheSettings
    {
        public int DefaultExpirationMinutes { get; set; } = 60;
        public int AccountListExpirationMinutes { get; set; } = 60;
        public int AccountDetailsExpirationMinutes { get; set; } = 10;
    }
}
