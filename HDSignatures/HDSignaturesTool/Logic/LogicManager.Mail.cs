using HDSignaturesTool.Types;
using M.Core.Mail;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HDSignaturesTool.Logic
{
    internal partial class LogicManager
    {
        public static void SendMail(string argMailFrom, string argMailTo, string argApplicationName, string argBedrijfsNaam)
        {
            // Mail sturen success
            MailSender mailSender = new MailSender();
            mailSender.createNewMail(argApplicationName + ": Running");
            mailSender.addFrom(argApplicationName, argMailFrom);

            mailSender.addTo("", argMailTo.Trim());

            mailSender.setMessagePlainText($"HDSignatures Draait nog bij {argBedrijfsNaam}");

            mailSender.sendMail();
        }
    }
}
