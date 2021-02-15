using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;

namespace NFe.Core
{
    public class MailTo
    {
        public MailTo(string account, string name)
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
