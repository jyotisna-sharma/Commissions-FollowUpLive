using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using MyAgencyVault.BusinessLibrary.Base;
using System.Runtime.Serialization;
using DLinq = DataAccessLayer.LinqtoEntity;
using MyAgencyVault.BusinessLibrary.Masters;
using DataAccessLayer.LinqtoEntity;
using System.Web;
using System.IO;
using System.Threading;
using System.Net;
using MyAgencyVault.BusinessLibrary.ReportingService;
using System.Data.EntityClient;
using System.Data.SqlClient;
using System.Data;

namespace MyAgencyVault.BusinessLibrary
{
    [DataContract]
    public class Report
    {
        public static List<Report> GetReports()
        {
            List<Report> reports = new List<Report>();
            using (DLinq.CommissionDepartmentEntities DataModel = Entity.DataModel)
            {
                foreach (DLinq.MasterReportList report in DataModel.MasterReportLists)
                {
                    Report rpt = new Report();
                    rpt.Id = report.ReportId;
                    rpt.Code = report.ReportCode;
                    rpt.Description = report.ReportDescription;
                    rpt.GroupName = report.ReportGroupName;
                    rpt.Name = report.ReportName;

                    reports.Add(rpt);
                }
                reports = reports.OrderBy(s => s.GroupName).OrderBy(s => s.Name).ToList();
                return reports;
            }
        }

        public static void SavePayeeStatementReportTemp(PayeeStatementReport report)
        {

            DLinq.CommissionDepartmentEntities ctx = new DLinq.CommissionDepartmentEntities(); //create your entity object here
            EntityConnection ec = (EntityConnection)ctx.Connection;
            SqlConnection sc = (SqlConnection)ec.StoreConnection; //get the SQLConnection that your entity object would use
            string adoConnStr = sc.ConnectionString;

            using (SqlConnection con = new SqlConnection(adoConnStr))
            {
                using (SqlCommand cmd = new SqlCommand("Usp_SavePayeeStatementReports", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@ReportId", report.ReportId);
                    cmd.Parameters.AddWithValue("@LicenceID", report.LicenseeId);
                    cmd.Parameters.AddWithValue("@Batch", report.BatcheIds);
                    cmd.Parameters.AddWithValue("@Payee", report.AgentIds);
                    cmd.Parameters.AddWithValue("@Reports", report.ReportNames);

                    if (report.PaymentType == "Unpaid")
                    {
                        //isPaymentType = false;
                        cmd.Parameters.AddWithValue("@PaymentType", false);
                    }
                    else if (report.PaymentType == "Paid")
                    {
                        //isPaymentType = true;
                        cmd.Parameters.AddWithValue("@PaymentType", true);
                    }
                    else
                    {

                        cmd.Parameters.AddWithValue("@PaymentType", DBNull.Value);
                    }

                    cmd.Parameters.AddWithValue("@Zero", report.IsZero);
                    cmd.Parameters.AddWithValue("@IsSplit", report.IsSplit);
                    cmd.Parameters.AddWithValue("@IsPayorRate", report.IsPayorRate);

                    con.Open();
                    int intCount = cmd.ExecuteNonQuery();
                    con.Close();


                }
            }

        }

        public static void SavePayeeStatementReport(PayeeStatementReport report)
        {
            using (DLinq.CommissionDepartmentEntities DataModel = Entity.DataModel)
            {
                DLinq.PayeeStatementReport payeeReportData = new DLinq.PayeeStatementReport();
                payeeReportData.Batch = report.BatcheIds;
                payeeReportData.LicenceID = report.LicenseeId;
                payeeReportData.Payee = report.AgentIds;
                payeeReportData.Reports = report.ReportNames;
                payeeReportData.ReportId = report.ReportId;
                payeeReportData.ReportOn = DateTime.Now;
                //Added
                payeeReportData.Zero = report.IsZero;

                if (report.PaymentType == "Unpaid")
                    payeeReportData.PaymentType = false;
                else if (report.PaymentType == "Paid")
                    payeeReportData.PaymentType = true;
                else
                    payeeReportData.PaymentType = null;


                DataModel.PayeeStatementReports.AddObject(payeeReportData);
                DataModel.SaveChanges();
            }
        }

        public static void SaveAuditReport(AuditReport report)
        {
            using (DLinq.CommissionDepartmentEntities DataModel = Entity.DataModel)
            {
                DLinq.AuditReport auditReportData = (from f in DataModel.AuditReports where f.ReportId == report.ReportId select f).FirstOrDefault();
                // DLinq.AuditReport auditReportData = new DLinq.AuditReport();
                if (auditReportData == null)
                {
                    auditReportData = new DLinq.AuditReport();
                    auditReportData.Payor = report.PayorIds;
                    auditReportData.LicenceID = report.LicenseeId;
                    auditReportData.Payee = report.AgentIds;
                    auditReportData.Reports = report.ReportNames;
                    auditReportData.ReportId = report.ReportId;
                    auditReportData.ReportOn = DateTime.Now;
                    auditReportData.OrderBy = report.OrderBy;
                    //Newly added
                    auditReportData.FilterBy = report.FilterBy;

                    auditReportData.InvoiceFrom = report.FromInvoiceDate;
                    auditReportData.InvoiceTo = report.ToInvoiceDate;

                    DataModel.AddToAuditReports(auditReportData);
                    DataModel.SaveChanges();
                }
            }
        }

        public static void SaveManagementReport(ManagementReport report)
        {
            using (DLinq.CommissionDepartmentEntities DataModel = Entity.DataModel)
            {
                DLinq.ManagementReport mgmtReportData = new DLinq.ManagementReport();
                mgmtReportData.Payor = report.PayorIds;
                mgmtReportData.Carrier = report.CarrierIds;
                mgmtReportData.Product = report.ProductIds;
                mgmtReportData.LicenceID = report.LicenseeId;
                mgmtReportData.Payee = report.AgentIds;
                mgmtReportData.Reports = report.ReportNames;
                mgmtReportData.ReportId = report.ReportId;
                mgmtReportData.ReportOn = DateTime.Now;
                mgmtReportData.OrderBy = report.OrderBy;

                mgmtReportData.EffectiveFrom = report.FromEffectiveDate;
                mgmtReportData.EffectiveTo = report.ToEffectiveDate;
                mgmtReportData.TrackFrom = report.FromTrackDate;
                mgmtReportData.TrackTo = report.ToTrackDate;
                mgmtReportData.TermFrom = report.FromTermDate;
                mgmtReportData.TermTo = report.ToTermDate;

                mgmtReportData.PremiumFrom = report.BeginPremium;
                mgmtReportData.PremiumTo = report.EndPremium;
                mgmtReportData.EnrolledFrom = report.BeginEnrolled;
                mgmtReportData.EnrolledTo = report.EndEnrolled;
                mgmtReportData.EligibleFrom = report.BeginEligible;
                mgmtReportData.EligibleTo = report.EndEligible;
                //Added 
                mgmtReportData.EffectiveMonth = report.EffectiveMonth;

                DLinq.MasterPolicyMode policyMode = DataModel.MasterPolicyModes.FirstOrDefault(s => s.Name == report.PolicyMode);
                if (policyMode != null)
                    mgmtReportData.PolicyMode = policyMode.PolicyModeId;
                else
                    mgmtReportData.PolicyMode = null;

                DLinq.MasterPolicyTerminationReason policyTermReason = DataModel.MasterPolicyTerminationReasons.FirstOrDefault(s => s.Name == report.PolicyTermReason);
                if (policyTermReason != null)
                    mgmtReportData.TermReason = policyTermReason.PTReasonId;
                else
                    mgmtReportData.TermReason = null;

                DLinq.MasterPolicyStatu policyStatus = DataModel.MasterPolicyStatus.FirstOrDefault(s => s.Name == report.PolicyType);

                if (report.PolicyType == "Active")
                    mgmtReportData.PolicyType = 1;
                else if (report.PolicyType == "Pending")
                    mgmtReportData.PolicyType = 2;
                else if (report.PolicyType == "Active/Pending")
                    mgmtReportData.PolicyType = 3;
                else if (report.PolicyType == "Terminated")
                    mgmtReportData.PolicyType = 4;
                else if (report.PolicyType == "Deleted")
                    mgmtReportData.PolicyType = 5;
                else if (report.PolicyType == "All")
                    mgmtReportData.PolicyType = 6;
                else
                    mgmtReportData.PolicyType = null;

                if (report.TrackPayment == "Yes")
                    mgmtReportData.TrackPayment = true;
                else if (report.TrackPayment == "No")
                    mgmtReportData.TrackPayment = false;
                else
                    mgmtReportData.TrackPayment = null;

                DataModel.ManagementReports.AddObject(mgmtReportData);
                DataModel.SaveChanges();
            }
        }

        public static PrintReportOutput PrintReport(Guid Id, string reportType, string Format)
        {
            PrintReportOutput printOutput = new PrintReportOutput();
            using (DLinq.CommissionDepartmentEntities DataModel = Entity.DataModel)
            {
                string[] rpts = reportType.Split(',');
                List<string> tmpRpts = new List<string>(rpts);
                string code = tmpRpts[0];
                DLinq.MasterReportList Report = DataModel.MasterReportLists.FirstOrDefault(s => s.ReportCode == code);
                string historyID = null;
                string deviceInfo = null;
                string format = null;
                if (Format == "")
                {
                    format = "PDF";
                }
                else
                {
                    format = Format;
                }
                Byte[] results;
                string encoding = String.Empty;
                string mimeType = String.Empty;
                string extension = String.Empty;
                Warning[] warnings = null;
                string[] streamIDs = null;

                string KeyValue = SystemConstant.GetKeyValue("ServerWebDevPath");
                WebDevPath ObjWebDevPath = WebDevPath.GetWebDevPath(KeyValue);

                ReportExecutionService rsExec = new ReportExecutionService();
                rsExec.Credentials = new NetworkCredential(ObjWebDevPath.UserName, ObjWebDevPath.Password, ObjWebDevPath.DomainName);
                rsExec.Timeout = System.Threading.Timeout.Infinite;
                //rsExec.Timeout = 1000000;                    

                ExecutionInfo ei = rsExec.LoadReport("/MAVReport/" + Report.ReportGroupName, historyID);
                ParameterValue[] rptParameters = new ParameterValue[2];

                rptParameters[0] = new ParameterValue();
                rptParameters[0].Name = "ReportID";
                //just in case: we don't want any SQL injection strings
                rptParameters[0].Value = Id.ToString();
                rptParameters[1] = new ParameterValue();
                rptParameters[1].Name = "ReportList";
                rptParameters[1].Value = reportType;
                //render the PDF

                rsExec.SetExecutionParameters(rptParameters, "en-us");

                //results = rsExec.Render(format, deviceInfo, out extension, out encoding, out mimeType, out warnings, out streamIDs);
                results = rsExec.Render(format, deviceInfo, out extension, out encoding, out mimeType, out warnings, out streamIDs);
                String path = null;
                if (format == "PDF")
                {
                    path = Path.GetTempPath() + Id.ToString() + @".pdf";
                }
                else if (format == "Excel")
                {
                    path = Path.GetTempPath() + Id.ToString() + @".xls";

                }
                System.IO.FileStream fs = new System.IO.FileStream(path, System.IO.FileMode.Create);
                fs.Write(results, 0, results.Length);
                fs.Close();

                FileUtility ObjUpload = FileUtility.CreateClient(ObjWebDevPath.URL, ObjWebDevPath.UserName, ObjWebDevPath.Password, ObjWebDevPath.DomainName);
                AutoResetEvent autoResetEvent = new AutoResetEvent(false);
                ObjUpload.UploadComplete += (i, j) =>
                {
                    autoResetEvent.Set();
                };
                ObjUpload.Upload(path, @"Reports/" + Path.GetFileName(path));
                autoResetEvent.WaitOne();
                File.Delete(path);

                if (format == "PDF")
                {
                    printOutput.FileName = Id.ToString() + @".pdf";

                }
                else if (format == "Excel")
                {
                    printOutput.FileName = Id.ToString() + @".xls";
                }
                if (tmpRpts.Contains("PS"))
                {
                    DLinq.PayeeStatementReport report = DataModel.PayeeStatementReports.FirstOrDefault(s => s.ReportId == Id);

                    if (report.PaymentType != null && report.PaymentType == true)
                        printOutput.ShowPaidPopup = false;
                    else
                        printOutput.ShowPaidPopup = true;

                    string[] batches = report.Batch.Split(',');
                    List<string> tmpBatches = new List<string>(batches);
                    tmpBatches.Remove(string.Empty);
                    List<Guid> batchGuids = tmpBatches.Select(s => new Guid(s)).ToList();
                    printOutput.BatchIds = batchGuids;
                }
                else
                {
                    printOutput.ShowPaidPopup = false;
                    printOutput.BatchIds = null;
                }
            }


            return printOutput;

        }      

        public static bool PrintReportAndSendMail(Guid Id, string reportType, Guid userId)
        {
            using (DLinq.CommissionDepartmentEntities DataModel = Entity.DataModel)
            {
                string[] rpts = reportType.Split(',');
                List<string> tmpRpts = new List<string>(rpts);
                string code = tmpRpts[0];
                DLinq.MasterReportList Report = DataModel.MasterReportLists.FirstOrDefault(s => s.ReportCode == code);
                string historyID = null;
                string deviceInfo = null;
                string format = "PDF";
                Byte[] results;
                string encoding = String.Empty;
                string mimeType = String.Empty;
                string extension = String.Empty;
                Warning[] warnings = null;
                string[] streamIDs = null;

                string KeyValue = SystemConstant.GetKeyValue("ServerWebDevPath");
                WebDevPath ObjWebDevPath = WebDevPath.GetWebDevPath(KeyValue);
                ReportExecutionService rsExec = new ReportExecutionService();
                rsExec.Credentials = new NetworkCredential(ObjWebDevPath.UserName, ObjWebDevPath.Password, ObjWebDevPath.DomainName);
                rsExec.Timeout = System.Threading.Timeout.Infinite;
                ExecutionInfo ei = rsExec.LoadReport("/MAVReport/" + Report.ReportGroupName, historyID);
                ParameterValue[] rptParameters = new ParameterValue[2];

                rptParameters[0] = new ParameterValue();
                rptParameters[0].Name = "ReportID";
                //just in case: we don't want any SQL injection strings
                rptParameters[0].Value = Id.ToString();
                rptParameters[1] = new ParameterValue();
                rptParameters[1].Name = "ReportList";
                rptParameters[1].Value = reportType;
                //render the PDF
                rsExec.SetExecutionParameters(rptParameters, "en-us");
                results = rsExec.Render(format, deviceInfo, out extension, out encoding, out mimeType, out warnings, out streamIDs);
                String path = Path.GetTempPath() + Id.ToString() + @".pdf";

                
                System.IO.FileStream fs = new System.IO.FileStream(path, System.IO.FileMode.Create);
                fs.Write(results, 0, results.Length);
                fs.Close();


                DLinq.UserDetail userDetail = DataModel.UserDetails.FirstOrDefault(s => s.UserCredentialId == userId);
                if (userDetail != null)
                {
                    if (!string.IsNullOrEmpty(userDetail.Email))
                        return MailServerDetail.sendMailWithAttachment(userDetail.Email, "Report", "Mail with Report", path);
                    else
                        return false;
                }
                else
                    return false;
               
                   // return MailServerDetail.sendMailWithAttachment("vinod.yadav@hanusoftware.com", "Report", "Mail with Report", path);
                
            }
        }

        [DataMember]
        public Guid Id { get; set; }
        [DataMember]
        public string Code { get; set; }
        [DataMember]
        public string Name { get; set; }
        [DataMember]
        public string GroupName { get; set; }
        [DataMember]
        public string Description { get; set; }
    }
        

    [DataContract]
    public class PrintReportOutput
    {
        [DataMember]
        public string FileName { get; set; }
        [DataMember]
        public bool ShowPaidPopup { get; set; }
        [DataMember]
        public List<Guid> BatchIds { get; set; }
    }

    [DataContract]
    public class PayeeStatementReport
    {
        [DataMember]
        public Guid ReportId { get; set; }
        [DataMember]
        public Guid LicenseeId { get; set; }
        [DataMember]
        public string BatcheIds { get; set; }
        [DataMember]
        public string AgentIds { get; set; }
        [DataMember]
        public string ReportNames { get; set; }
        [DataMember]
        public string PaymentType { get; set; }
        [DataMember]
        public bool IsZero { get; set; }        
        [DataMember]
        public bool IsPayorRate { get; set; }
        [DataMember]
        public bool IsSplit { get; set; }
    }

    [DataContract]
    public class AuditReport
    {
        [DataMember]
        public Guid ReportId { get; set; }
        [DataMember]
        public Guid LicenseeId { get; set; }
        [DataMember]
        public string PayorIds { get; set; }
        [DataMember]
        public string AgentIds { get; set; }
        [DataMember]
        public string ReportNames { get; set; }
        [DataMember]
        public DateTime? FromInvoiceDate { get; set; }
        [DataMember]
        public DateTime? ToInvoiceDate { get; set; }
        [DataMember]
        public string OrderBy { get; set; }
        //Added filter type
        [DataMember]
        public int FilterBy { get; set; }
    }

    [DataContract]
    public class ManagementReport
    {
        [DataMember]
        public Guid ReportId { get; set; }
        [DataMember]
        public Guid LicenseeId { get; set; }
        [DataMember]
        public string PayorIds { get; set; }
        [DataMember]
        public string CarrierIds { get; set; }
        [DataMember]
        public string ProductIds { get; set; }
        [DataMember]
        public string AgentIds { get; set; }
        [DataMember]
        public string ReportNames { get; set; }
        [DataMember]
        public string PolicyType { get; set; }
        [DataMember]
        public string PolicyMode { get; set; }
        [DataMember]
        public string PolicyTermReason { get; set; }
        [DataMember]
        public string TrackPayment { get; set; }
        [DataMember]
        public DateTime? FromEffectiveDate { get; set; }
        [DataMember]
        public DateTime? ToEffectiveDate { get; set; }
        [DataMember]
        public DateTime? FromTrackDate { get; set; }
        [DataMember]
        public DateTime? ToTrackDate { get; set; }
        [DataMember]
        public DateTime? FromTermDate { get; set; }
        [DataMember]
        public DateTime? ToTermDate { get; set; }
        [DataMember]
        public decimal? BeginPremium { get; set; }
        [DataMember]
        public decimal? EndPremium { get; set; }
        [DataMember]
        public int? BeginEnrolled { get; set; }
        [DataMember]
        public int? EndEnrolled { get; set; }
        [DataMember]
        public int? BeginEligible { get; set; }
        [DataMember]
        public int? EndEligible { get; set; }
        [DataMember]
        public string OrderBy { get; set; }
        [DataMember]
        public DateTime? InvoiceFrom { get; set; }
        [DataMember]
        public DateTime? InvoiceTo { get; set; }
        [DataMember]
        public int? EffectiveMonth { get; set; }      
       
    }    
}
