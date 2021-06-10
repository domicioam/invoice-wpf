using System.Net.Mail;

namespace NFe.Core
{
    public class MailFrom
    {
        public MailFrom(string account, string name)
        {
            Address = new MailAddress(account, name);
            Name = name;
            Account = account;
        }

        public MailAddress Address { get; }
        public string Name { get; }
        public string Account { get; }
    }
}
