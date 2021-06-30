using System;
namespace Datacom.IRIS.Common
{
    public interface IEmailSender
    {
        void SendEmail(string subject, string body, System.Net.Mail.MailAddress[] recipients, bool isBodyHTML, System.Net.Mail.Attachment[] attachements, System.Net.Mail.MailAddress[] ccRecipients = null);
        void SendEmail(string subject, string body, string recipients, bool isBodyHTML, byte[] attachmentContents, string attachmentName, string ccRecipients = null);
        void SendEmail(string subject, string body, string recipients, bool isBodyHTML, System.Collections.Generic.List<byte[]> attachmentContents = null, System.Collections.Generic.List<string> attachmentNames = null, string ccRecipients = null);
        void SetNetworkDeliveryMethod();
        void SetPickUpDirectoryLocation(string folderPath);
        void SetSender(string fromAddress, string displayname);
        string EmailFromAddress { get; }
    }
}
