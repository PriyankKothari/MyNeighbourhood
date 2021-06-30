using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Mail;
using System.IO;
using Datacom.IRIS.Common.Utils;

namespace Datacom.IRIS.Common
{
    public class EmailSender : IEmailSender
    {
        private SmtpClient _smtpClient;
        private MailAddress _fromAddress;
        

        public EmailSender()
        {
            _smtpClient = new SmtpClient();
        }

        public string EmailFromAddress
        {
            get
            {
                return _fromAddress.Address;
            }
        }

        
        

        public void SetSender(string fromAddress, string displayname)
        {
            _fromAddress = new MailAddress(fromAddress, displayname);
        }

        public void SetNetworkDeliveryMethod()
        {
            _smtpClient.DeliveryMethod = SmtpDeliveryMethod.Network;
        }

        public void SetPickUpDirectoryLocation(string folderPath)
        {
            _smtpClient.DeliveryMethod = SmtpDeliveryMethod.SpecifiedPickupDirectory;
            _smtpClient.PickupDirectoryLocation = folderPath;
        }

        public void SendEmail(string subject, string body, string recipients, bool isBodyHTML, byte[] attachmentContents, string attachmentName, string ccReceipients = null)
        {
            List<byte[]> attachements = new List<byte[]>();
            List<string> attachementsNames = new List<string>();

            attachements.Add(attachmentContents);
            attachementsNames.Add(attachmentName);
            SendEmail(subject, body, recipients, isBodyHTML, attachements, attachementsNames, ccReceipients);
        }

        public void SendEmail(string subject, string body, string recipients, bool isBodyHTML, List<byte[]> attachmentContents = null, List<string> attachmentNames = null, string ccReceipients = null)
        {

            if (string.IsNullOrEmpty(recipients) && string.IsNullOrEmpty(ccReceipients))
                throw new ApplicationException("Either recipients or CC recipients must be set.");

            string[] sendTo = new string[] { };
            if (!string.IsNullOrEmpty(recipients))
                sendTo = recipients.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);

            List<MailAddress> recipientsAddresses = new List<MailAddress>();
            List<MailAddress> ccRecipientsAddresses = null;
            List<Attachment> attachements = new List<Attachment>();
            foreach (string address in sendTo)
            {
                recipientsAddresses.Add(new MailAddress(address));
            }

            if (!string.IsNullOrEmpty(ccReceipients))
            {
                ccRecipientsAddresses = new List<MailAddress>();

                string[] cc = ccReceipients.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                foreach (string address in cc)
                {
                    ccRecipientsAddresses.Add(new MailAddress(address));
                }
            }

            if (attachmentContents != null)
            {
                int i = 0;
                foreach (byte[] contents in attachmentContents)
                {
                    MemoryStream ms = new MemoryStream(contents);
                    //using (MemoryStream ms = new MemoryStream(contents))
                    {
                        attachements.Add(new Attachment(ms, attachmentNames[i]));
                    }
                    i++;
                }
            }

            SendEmail(subject, body, recipientsAddresses.ToArray(), isBodyHTML, attachements.ToArray(), ccRecipientsAddresses == null? null : ccRecipientsAddresses.ToArray());

        }

        public void SendEmail(string subject, string body, MailAddress[] recipients, bool isBodyHTML, Attachment[] attachements, MailAddress[] ccRecipients = null)
        {
            MailMessage email = new MailMessage();
            recipients.ForEach(r => email.To.Add(r));
            if (ccRecipients != null)
                ccRecipients.ForEach(r => email.CC.Add(r));
            email.Subject = subject;
            email.Body = body;
            email.IsBodyHtml = isBodyHTML;
            attachements.ForEach(a => email.Attachments.Add(a));

            if (_fromAddress != null)
            {
                email.From = _fromAddress;
            }

            if (_smtpClient.DeliveryMethod == SmtpDeliveryMethod.SpecifiedPickupDirectory)
            {
                if (!Directory.Exists(_smtpClient.PickupDirectoryLocation))
                {
                    Directory.CreateDirectory(_smtpClient.PickupDirectoryLocation);
                }
            }

            _smtpClient.Send(email);
        }
    }
}
