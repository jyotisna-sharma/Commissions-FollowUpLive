using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MyAgencyVault.BusinessLibrary.Base;
using System.Runtime.Serialization;
using DLinq = DataAccessLayer.LinqtoEntity;
using DataAccessLayer.LinqtoEntity;
using MyAgencyVault.BusinessLibrary.Masters;
using MyAgencyVault.EmailFax;

namespace MyAgencyVault.BusinessLibrary
{
    [DataContract]
    public class MailServerDetail
    {
        [DataMember]
        public string ServerName { get; set; }
        [DataMember]
        public string UserName { get; set; }
        [DataMember]
        public string Password { get; set; }
        [DataMember]
        public string Email { get; set; }
        [DataMember]
        public string PortNo { get; set; }

        public static MailServerDetail getMailServerDetail()
        {
            using (DLinq.CommissionDepartmentEntities DataModel = Entity.DataModel)
            {
                MailServerDetail mail = new MailServerDetail();

                mail.ServerName = (from p in DataModel.MasterSystemConstants
                 where p.Name == "MailServerName"
                 select p.Value).FirstOrDefault();

                mail.Email = (from p in DataModel.MasterSystemConstants
                                 where p.Name == "MailServerEMail"
                                 select p.Value).FirstOrDefault();

                mail.UserName = (from p in DataModel.MasterSystemConstants
                                 where p.Name == "MailServerUserName"
                                 select p.Value).FirstOrDefault();

                mail.Password = (from p in DataModel.MasterSystemConstants
                                 where p.Name == "MailServerPassword"
                                 select p.Value).FirstOrDefault();

                mail.PortNo = (from p in DataModel.MasterSystemConstants
                                 where p.Name == "MailServerPortNo"
                                 select p.Value).FirstOrDefault();

                return mail;
            }
        }

        public static bool sendMail(string toAddress, string subject, string body)
        {
            MailServerDetail mInfo = getMailServerDetail();

            if (string.IsNullOrEmpty(toAddress))
                toAddress = mInfo.Email;

            bool mailSendSuccessfully = true;

            try
            {
                EmailFax.EmailFax wpfMail = new EmailFax.EmailFax(mInfo.UserName, mInfo.Password, mInfo.Email, toAddress, mInfo.ServerName, mInfo.PortNo, subject, body);
                wpfMail.SendEmail();
            }
            catch
            {
                mailSendSuccessfully = false;
            }

            return mailSendSuccessfully;
        }

        public static bool sendMailWithAttachment(string toAddress, string subject, string body,string fileName)
        {
            MailServerDetail mInfo = getMailServerDetail();

            if (string.IsNullOrEmpty(toAddress))
                toAddress = mInfo.Email;

            bool mailSendSuccessfully = true;

            try
            {
                EmailFax.EmailFax wpfMail = new EmailFax.EmailFax(mInfo.UserName, mInfo.Password, mInfo.Email, toAddress, mInfo.ServerName, mInfo.PortNo, subject, body);
                wpfMail.AttachmentPath = fileName;
                wpfMail.SendEmail();
               
            }
            catch
            {
                mailSendSuccessfully = false;
            }

            return mailSendSuccessfully;
            
        }

        public static string GetAlertCommissionDepartmentMail()
        {
            using (DLinq.CommissionDepartmentEntities DataModel = Entity.DataModel)
            {

                DLinq.MasterSystemConstant Constant = (from s in DataModel.MasterSystemConstants
                                                       where s.Name == "AlertCommissionDeptMailId"
                                                       select s).FirstOrDefault();
                return Constant.Value;
            }
        }
    }
}
