using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NFe.Core
{
    class Email
    {
        public Email(MailFrom from, MailTo to, string subject, string body, string password)
        {
            From = from;
            To = to;
            Subject = subject;
            Body = body;
            Password = password;
        }

        public MailFrom From { get; private set; }
        public MailTo To { get; private set; }
        public string Subject { get; private set; }
        public string Body { get; private set; }
        public string Password { get; private set; }

        public static Email CreateDefaultContabilidadeEmail(string periodo, string razaoSocial, string cnpj)
        {
            return new Email(new MailFrom(ConfigurationManager.AppSettings["fromMailAccount"], ConfigurationManager.AppSettings["fromMailName"]),
                             new MailTo(ConfigurationManager.AppSettings["toMailAccount"], ConfigurationManager.AppSettings["toMailName"]),
                             $"Notas Fiscais {periodo} - {razaoSocial} - {cnpj}",
                             "Notas fiscais em anexo a esse e-mail. \n\nEsta é uma mensagem automática.", ConfigurationManager.AppSettings["fromMailPassword"]);
        }
    }
}
