namespace KRT.BankAccounts.Api._04_Infrastructure.Messaging
{
    public class RabbitMqSettings
    {
        public string Host { get; set; } = string.Empty;
        public int Port { get; set; } = 5672;
        public string Username { get; set; } = "guest";
        public string Password { get; set; } = "guest";
        public string Exchange { get; set; } = "krt.bank.exchange";
        public string Queue { get; set; } = "krt.account.events";
        public string RoutingKey { get; set; } = "account.*";
    }
}
