#define DEBUG
using System;
using System.Linq;
using System.Collections.Generic;
using System.Transactions;
using MyAgencyVault.BusinessLibrary;
using MyAgencyVault.FollowUpProcess;
using System.Diagnostics;
using System.Threading;
using MyAgencyVault.BusinessLibrary.Masters;


namespace MyAgencyVault.FollowUpProcess
{
    public class FollowUpStatus
    {
        public Guid PolicyId;
        public bool IsComplete = false;
        public Exception ex;
        public string Actions = "";
        public bool IsTracked = false;
    }

    public class ConstantTrm
    {
        public const string ModeNull = "--Mode is Null--";

    }

    public class FollowUpService
    {
        public static void FollowUpProc()
        {
            string strServiceValue = SystemConstant.GetKeyValue("FollowUpService");
            ActionLogger.Logger.WriteFollowUpLog("*****************FollowUpService - FollowUpProc Starts**************");
            if (strServiceValue == "Stop")
            {
                return;
            }

            double DaysCnt = Convert.ToDouble(SystemConstant.GetKeyValue(MasterSystemConst.NextFollowUpRunDaysCount.ToString()));
            if (DaysCnt == 0)
            {
                return;
            }

            Dictionary<string, object> parameters = new Dictionary<string, object>();
            ActionLogger.Logger.WriteFollowUpLog("Please wait ..fetching policy list from database");
            List<PolicyDetailsData> PolicyLst = Policy.GetAllPolicyForFollowupservice();

            ActionLogger.Logger.WriteFollowUpLog("Policy list found from database, count: " + PolicyLst.Count);

            DateTime dtNextdate = new DateTime();
            dtNextdate = System.DateTime.Now;

          //Acme commenetd  -already handled in SP
         //   PolicyLst = new List<PolicyDetailsData>(PolicyLst.Where(p => p.LastFollowUpRuns != null && Convert.ToDateTime(p.LastFollowUpRuns).AddDays(DaysCnt) <= dtNextdate)).ToList();

          //  PolicyLst = PolicyLst.OrderBy(p => p.LastFollowUpRuns).ToList();

            foreach (PolicyDetailsData _Policy in PolicyLst)
            {
                ActionLogger.Logger.WriteFollowUpLog("Policy ID starting: " + _Policy.PolicyId); 
               // if (Policy.FollowUpRunsRequired(_Policy.PolicyId)) 
                {
                    try
                    {
                        ActionLogger.Logger.WriteFollowUpLog("Follow up running  for Policy number: " + _Policy.PolicyNumber); 
                        FollowUpStatus _FollowUpStatus = FollowUpProcedure(_Policy, _Policy.IsTrackPayment);
                        //decimal dbPmc = PostUtill.CalculatePMC(_Policy.PolicyId);
                        //decimal dbPac = PostUtill.CalculatePAC(_Policy.PolicyId);
                        //ActionLogger.Logger.WriteFollowUpLog("Follow up run success, updating PAC/PMC: " + _Policy.PolicyNumber); 
                        //PolicyLearnedField.UpdatePACPMC(_Policy.PolicyId);
                    }
                    catch (Exception ex)
                    {
                        ActionLogger.Logger.WriteFollowUpErrorLog("Issue  for Policy ID: " + _Policy.PolicyId);

                        //System.Console.WriteLine("\nIssue  for Policy number: " + _Policy.PolicyNumber);
                        ActionLogger.Logger.WriteFollowUpErrorLog(ex.StackTrace.ToString());
                    }
                    finally
                    {
                        PolicyLocking.UnlockPolicy(_Policy.PolicyId);
                    }
                }
            }
            if (PolicyLst.Count == 0)
            {
                ActionLogger.Logger.WriteFollowUpLog("**No policy found for processing**"); 
            }
            ActionLogger.Logger.WriteFollowUpLog("*****************FollowUpService - FollowUpProc Completed**************");
        }


    

        private static FollowUpStatus FollowUpProcedure(PolicyDetailsData FollowPolicy, bool IsTrackPayment)
        {
            //FollowPolicy.PolicyId = new Guid("3CE3C34C-323E-4C33-8388-004899BA77DC");

            PolicyDetailsData policy = PostUtill.GetPolicy(FollowPolicy.PolicyId); /*new Guid("FFA1938B-8D46-4E81-8763-90519AFF4613")/*FollowPolicy.PolicyId);*/ // Policy.FillPolicyDataOnPolicyID(FollowPolicy.PolicyId);  //Acme commneted - 
            ActionLogger.Logger.WriteFollowUpLog("Details fetching for policyID: " + FollowPolicy.PolicyId);

            if (policy == null)
            {
                ActionLogger.Logger.WriteFollowUpErrorLog("Details not found for policyID: " + FollowPolicy.PolicyId);
                return null;
            }

            policy.LearnedFields = PolicyLearnedField.GetPolicyLearnedFieldsPolicyWise(FollowPolicy.PolicyId);

            FollowUpStatus _FollowUpStatus = new FollowUpStatus();
            _FollowUpStatus.IsTracked = false;
            bool IsAutoTrmDateUpadte = true;
            bool isPaymenyExist = false;
        //    bool ReturnFlag = false;
            DateTime? StoreFirstMissingMonth = null;
            int noOfMissingCount = 0;
          
            MasterPolicyMode? _MasterPolicyMode = PostUtill.ModeEntryFromDeu(policy, null, false);
            ActionLogger.Logger.WriteFollowUpLog("PolicyID: " + FollowPolicy.PolicyId + ", _MasterPolicyMode : " + _MasterPolicyMode);
            
            //Acme added following , as caused exception otherwise.
            if (_MasterPolicyMode == null)
            {
                ActionLogger.Logger.WriteFollowUpLog("_MasterPolicyMode from policy: " + policy.PolicyModeId);
               //  return null;
                if (_MasterPolicyMode == null && policy.PolicyModeId != null)
                {
                    ActionLogger.Logger.WriteFollowUpLog("_MasterPolicyMode not found for policyID from deu , so setting from policy level: " + FollowPolicy.PolicyId);
                    _MasterPolicyMode = (MasterPolicyMode)policy.PolicyModeId;
                }
                else
                {
                    ActionLogger.Logger.WriteFollowUpLog("_MasterPolicyMode not found for policyID: " + FollowPolicy.PolicyId);
                    return null;
                }
            }

            Policy.UpdateLastFollowupRunsWithTodayDate(FollowPolicy.PolicyId);
            ActionLogger.Logger.WriteFollowUpLog("PolicyID: " + FollowPolicy.PolicyId + ", Last follow up runs updated");
            PaymentMode _PaymentMode = PostUtill.ConvertMode(_MasterPolicyMode.Value);

            FollowUpDate _FollowUpDate = FollowUpUtill.CalculateFollowUpDateRange(_PaymentMode, policy, isPaymenyExist);

            if (_FollowUpDate != null)
            {
                ActionLogger.Logger.WriteFollowUpLog("PolicyID: " + FollowPolicy.PolicyId + ", FollowupDate range found as: " + _FollowUpDate.ToStringDump());
            }
            else
            {
                ActionLogger.Logger.WriteFollowUpLog("PolicyID: " + FollowPolicy.PolicyId + ", FollowupDate range found null ");
            }
            //Advance payment
        /*    bool invoiceDateBelongsToRange = false;
      //      bool bAvailableIntoRange = false;
            List<DateTime> dtAdavaceDateRange = new List<DateTime>();
            int intMulitply = 0;
            if (_FollowUpDate != null && policy.Advance != null) //Acme added check for advance not null to avoid going inside 
            {
                DateTime? originalEffectiveDate = policy.OriginalEffectiveDate;
                //Need to find invoice date and serch range inserted invoice date   

                int? intAdvance = null;
                if (originalEffectiveDate != null)
                {
                    intAdvance = policy.Advance;
                    intMulitply = GetRangValue(_PaymentMode);

                    for (int j = 0; j < intAdvance * intMulitply; j++)
                    {
                        dtAdavaceDateRange.Add(originalEffectiveDate.Value.AddMonths(j));
                    }
                }
                ActionLogger.Logger.WriteFollowUpLog("PolicyID: " + FollowPolicy.PolicyId + ", Advance date range found as: " + dtAdavaceDateRange.ToStringDump());
            }
            else
            {
                ActionLogger.Logger.WriteFollowUpLog("PolicyID: " + FollowPolicy.PolicyId + ", No Advance date range found   ");
            }*/
            //Get All Payment
            List<PolicyPaymentEntriesPost> _AllPaymentEntriesOnPolicyFormissing = PolicyPaymentEntriesPost.GetPolicyPaymentEntryPolicyIDWise(policy.PolicyId);
            //Get All  resolved issue
            List<PolicyPaymentEntriesPost> _AllResolvedorClosedIssueId = PolicyPaymentEntriesPost.GetAllResolvedorClosedIssueId(policy.PolicyId);

          /*  if (_AllPaymentEntriesOnPolicyFormissing.Count > 0)
            {
                ActionLogger.Logger.WriteFollowUpLog("PolicyID: " + FollowPolicy.PolicyId + ", _AllPaymentEntriesOnPolicyFormissing.Count   " + _AllPaymentEntriesOnPolicyFormissing.Count);
                isPaymenyExist = true;
                invoiceDateBelongsToRange = false;
                
                for (int j = 0; j < _AllPaymentEntriesOnPolicyFormissing.Count; j++)
                {
                    for (int k = 0; k < dtAdavaceDateRange.Count; k++)
                    {
                        if (_AllPaymentEntriesOnPolicyFormissing[j].InvoiceDate.Equals(dtAdavaceDateRange[k]))
                        {
                            invoiceDateBelongsToRange = true;
                            break;
                        }
                        else
                        {
                            invoiceDateBelongsToRange = false;
                        }
                    }
                    if (invoiceDateBelongsToRange)
                    {
                       // invoiceDateBelongsToRange = true;
                        ActionLogger.Logger.WriteFollowUpLog("PolicyID: " + FollowPolicy.PolicyId + ", invoiceDateBelongsToRange is true");
                        break;
                    }
                }
            }
            else
            {
                ActionLogger.Logger.WriteFollowUpLog("PolicyID: " + FollowPolicy.PolicyId + ", _AllPaymentEntriesOnPolicyFormissing.Count  0 ");
                isPaymenyExist = false;
            }*/

            try
            {
                if (IsTrackPayment)
                {
                    ActionLogger.Logger.WriteFollowUpLog("PolicyID: " + FollowPolicy.PolicyId + ", IsTrackPayment true ");
                    _FollowUpStatus.IsTracked = true;

                    List<DateRange> _DateRangeForMissingLst = FollowUpUtill.MakeFollowUpDateRangeForMissing(_FollowUpDate, _PaymentMode);

                   if(_DateRangeForMissingLst == null)
                    {
                        ActionLogger.Logger.WriteFollowUpErrorLog("PolicyID: " + FollowPolicy.PolicyId + ",  _DateRangeForMissingLst calculation failed ");
                    }
                   else
                    {
                        ActionLogger.Logger.WriteFollowUpLog("PolicyID: " + FollowPolicy.PolicyId + ", DateRangeForMissingLst calculated successfully");
                    }

                    List<DisplayFollowupIssue> FollowupIssueLst = FollowupIssue.GetIssues(FollowPolicy.PolicyId);
                    if (FollowupIssueLst != null)
                    {
                        ActionLogger.Logger.WriteFollowUpLog("PolicyID: " + FollowPolicy.PolicyId + ", FollowupIssueLst count: " + FollowupIssueLst.Count);
                        #region Random
                        if (_PaymentMode == PaymentMode.Random)
                        {
                            //Acme commented as already initialized:  FollowupIssueLst = FollowupIssue.GetIssues(policy.PolicyId);
                            //InValided The Issue
                            foreach (DisplayFollowupIssue follw in FollowupIssueLst)
                            {
                                if (follw.IsResolvedFromCommDashboard != true)
                                {
                                    MyAgencyVault.BusinessLibrary.FollowUpUtill.UpdateIssueIdOfPaymentsForIssueId(follw.IssueId, null);
                                    FollowupIssue.DeleteIssue(follw.IssueId);
                                }
                            }

                        }
                        #endregion
                        if (_FollowUpDate  == null || (_FollowUpDate !=null && (_FollowUpDate.FromDate == null || _FollowUpDate.ToDate == null || _FollowUpDate.FromDate > _FollowUpDate.ToDate)))//&& !ReturnFlag)
                        {
                            foreach (DisplayFollowupIssue follw in FollowupIssueLst)//FollowupIssueLst1
                            {
                                if (follw.IsResolvedFromCommDashboard != true)
                                {
                                    FollowupIssue.Delete(follw.IssueId);
                                }
                            }
                        }
                        else
                        {
                            //Get the issue range and delete before and after issue of the range
                            List<DisplayFollowupIssue> _FollowupIssueDoInValid = FollowupIssueLst.Where(p => (p.FromDate < _FollowUpDate.FromDate || p.ToDate > _FollowUpDate.ToDate)).ToList();

                            foreach (DisplayFollowupIssue closedIssue in _FollowupIssueDoInValid)
                            {
                                if (closedIssue.IssueStatusId != (int)FollowUpIssueStatus.Closed && closedIssue.IsResolvedFromCommDashboard != true)
                                {
                                    MyAgencyVault.BusinessLibrary.FollowUpUtill.UpdateIssueIdOfPaymentsForIssueId(closedIssue.IssueId, null);
                                    FollowupIssue.DeleteIssue(closedIssue.IssueId);
                                }
                            }

                            //Acme understanding - followin is re-init, as some issues are previously deleted 
                            FollowupIssueLst = FollowupIssue.GetIssues(FollowPolicy.PolicyId);

                            ActionLogger.Logger.WriteFollowUpLog("PolicyID: " + FollowPolicy.PolicyId + ", FollowupIssueLst count 2: " + FollowupIssueLst.Count);

                            //GEt Issue which is not closed and not the varriance into the payment
                            foreach (DisplayFollowupIssue follw in FollowupIssueLst.Where(p => p.IssueStatusId != (int)FollowUpIssueStatus.Closed).Where(p => p.IssueCategoryID != (int)FollowUpIssueCategory.VarSchedule))
                            {
                                bool flag = false;

                                for (int idx = 0; idx < _DateRangeForMissingLst.Last().RANGE; idx++)
                                {
                                    DateTime? FirstDate = _DateRangeForMissingLst.Where(p => p.RANGE == idx + 1).ToList()[0].STARTDATE;
                                    DateTime? LastDate = FollowUpUtill.LastDate(Convert.ToDateTime(FirstDate).AddMonths((int)_PaymentMode - 1)); ;// _DateRangeForMissingLst.Where(p => p.RANGE == idx + 1).ToList()[(int)_PaymentMode - 1].ENDDATE;
                                    if (follw.FromDate == FirstDate && follw.ToDate == LastDate)
                                    {
                                        flag = true;
                                        break;
                                    }
                                    else
                                    {
                                        flag = false;
                                    }
                                }
                                if (!flag && follw.IsResolvedFromCommDashboard != true)
                                {
                                    MyAgencyVault.BusinessLibrary.FollowUpUtill.UpdateIssueIdOfPaymentsForIssueId(follw.IssueId, null);
                                    FollowupIssue.DeleteIssue(follw.IssueId);
                                }
                            }
                        }
                    }
                    else
                    {
                        ActionLogger.Logger.WriteFollowUpLog("PolicyID: " + FollowPolicy.PolicyId + ", FollowupIssueLst count 0 ");
                    }

                     ActionLogger.Logger.WriteFollowUpLog("PolicyID: " + FollowPolicy.PolicyId + ", Processing starts for  _DateRangeForMissingLst");

                    // FollowupIssueLst = FollowupIssue.GetIssues(FollowPolicy.PolicyId);
                    for (int idx = 0; idx < _DateRangeForMissingLst.Last().RANGE; idx++)
                    {
                        //Acme understanding - folowing is the first/last date range of the list
                        //e.g. if mode is monthly and no f months are 2- say april and may
                        //first date for april - Apr01 and last - apr30 and similarly for may in next iteration 
                        DateTime? FirstDate = _DateRangeForMissingLst.Where(p => p.RANGE == idx + 1).ToList()[0].STARTDATE;
                        DateTime? LastDate = FollowUpUtill.LastDate(Convert.ToDateTime(FirstDate).AddMonths((int)_PaymentMode - 1));

                        //Get the payment at given range
                        List<PolicyPaymentEntriesPost> _PolicyPaymentEntriesFormissing = _AllPaymentEntriesOnPolicyFormissing.Where(p => p.InvoiceDate >= FirstDate.Value).Where(p => p.InvoiceDate <= LastDate.Value).ToList<PolicyPaymentEntriesPost>();
                        ActionLogger.Logger.WriteFollowUpLog("PolicyID: " + FollowPolicy.PolicyId + ", _PolicyPaymentEntriesFormissing.Count: " + _PolicyPaymentEntriesFormissing.Count);

                        bool Rflag = FollowUpUtill.ISExistsResolveIssuesForDateRange(FirstDate.Value, LastDate.Value, policy.PolicyId, FollowupIssueLst);
                        ActionLogger.Logger.WriteFollowUpLog("PolicyID: " + FollowPolicy.PolicyId + ", Rflag: " + Rflag);

                        try
                        {
                            ActionLogger.Logger.WriteFollowUpLog("PolicyID: " + FollowPolicy.PolicyId + ", FollowupIssueLst calculating ");
                            FollowupIssueLst = FollowupIssue.GetIssues(FollowPolicy.PolicyId);
                            ActionLogger.Logger.WriteFollowUpLog("PolicyID: " + FollowPolicy.PolicyId + ", FollowupIssueLst calculated " + FollowupIssueLst.Count);
                        }
                        catch (Exception ex)
                        {
                            ActionLogger.Logger.WriteFollowUpErrorLog("PolicyID: " + FollowPolicy.PolicyId + ", FollowupIssueLst exception " + ex.Message);
                        }

                        List<DisplayFollowupIssue> issfolllst = null;
                        if (FollowupIssueLst != null)
                        {
                            issfolllst = FollowupIssueLst.Where(p => (p.FromDate == FirstDate && p.ToDate == LastDate)).Where(p => (p.IssueCategoryID == (int)FollowUpIssueCategory.MissFirst || p.IssueCategoryID == (int)FollowUpIssueCategory.MissInv)).ToList();
                            ActionLogger.Logger.WriteFollowUpLog("PolicyID: " + FollowPolicy.PolicyId + ", issfolllst calculated ");

                        }
                        //    Acme comparing     FollowupIssueLst.Where(p => p.FromDate == FirstDate).Where(p => p.ToDate == LastDate).Where(p => (p.IssueCategoryID == (int)FollowUpIssueCategory.MissFirst || p.IssueCategoryID == (int)FollowUpIssueCategory.MissInv)).ToList();
                        if (_PolicyPaymentEntriesFormissing.Count == 0)
                        {
                            ActionLogger.Logger.WriteFollowUpLog("PolicyID: " + FollowPolicy.PolicyId + ", _PolicyPaymentEntriesFormissing.Count == 0 entered");
                            StoreFirstMissingMonth = FirstDate;
                            noOfMissingCount++;
                            //List<DisplayFollowupIssue> issfolllst = FollowupIssueLst.Where(p => (p.FromDate == FirstDate && p.ToDate == LastDate)).Where(p => (p.IssueCategoryID == (int)FollowUpIssueCategory.MissFirst || p.IssueCategoryID == (int)FollowUpIssueCategory.MissInv)).ToList();
                            //Acme comparing  -   FollowupIssueLst.Where(p => (p.FromDate == FirstDate && p.ToDate == LastDate)).Where(p => (p.IssueCategoryID == (int)FollowUpIssueCategory.MissFirst || p.IssueCategoryID == (int)FollowUpIssueCategory.MissInv)).ToList();

                            bool isIssueRaise = true;
                            ActionLogger.Logger.WriteFollowUpLog("PolicyID: " + policy.PolicyId + ", Advance found as: " + policy.Advance);

                            //Aug 23, 2019 - Check  advance field and return if payment received
                            if (policy != null && policy.Advance > 0)
                            {
                                isIssueRaise = IsIssueToBeRaised((int)policy.Advance, (DateTime)FirstDate, policy.OriginalEffectiveDate);

                            }
                            ActionLogger.Logger.WriteFollowUpLog("PolicyID: " + FollowPolicy.PolicyId + ", isIssueRaise: " + isIssueRaise);

                            if (isIssueRaise)
                            {
                                if (issfolllst != null && issfolllst.Count() > 0)
                                {
                                    ActionLogger.Logger.WriteFollowUpLog("PolicyID: " + FollowPolicy.PolicyId + ", Iissfolllst.Count > 0");
                                    if (Rflag)
                                    {
                                        StoreFirstMissingMonth = null;
                                        noOfMissingCount = 0;
                                    }

                                    //Acme - Following restructured - as same code in next condition also   
                                    DisplayFollowupIssue tempfis = issfolllst.FirstOrDefault();
                                    if (FirstDate <= policy.OriginalEffectiveDate && policy.OriginalEffectiveDate <= LastDate && tempfis.IssueCategoryID != (int)FollowUpIssueCategory.MissFirst)
                                    {
                                        tempfis.IssueCategoryID = (int)FollowUpIssueCategory.MissFirst;
                                    }
                                    else if (tempfis.IssueCategoryID != (int)FollowUpIssueCategory.MissInv)
                                    {
                                        tempfis.IssueCategoryID = (int)FollowUpIssueCategory.MissInv;
                                    }
                                    FollowupIssue.AddUpdate(tempfis);
                                    ActionLogger.Logger.WriteFollowUpLog("PolicyID: " + FollowPolicy.PolicyId + ", Issue closed with ID : " + tempfis.IssueId + ", category: " + tempfis.IssueCategoryID);
                                }
                                else //when follow issue list is empty 
                                {
                                    //Acme understanding - Following is being added, as no payment entry found in expected date range 
                                    ActionLogger.Logger.WriteFollowUpLog("PolicyID: " + FollowPolicy.PolicyId + ", Iissfolllst.Count = 0");
                                    if (FirstDate <= policy.OriginalEffectiveDate && policy.OriginalEffectiveDate <= LastDate)
                                    {
                                        FollowUpUtill.RegisterIssueAgainstMissingPayment(policy, FollowUpIssueCategory.MissFirst, FirstDate.Value, LastDate.Value);
                                        ActionLogger.Logger.WriteFollowUpLog("PolicyID: " + FollowPolicy.PolicyId + ", Issue raised for MissFirst (1)");
                                    }
                                    else
                                    {
                                        FollowUpUtill.RegisterIssueAgainstMissingPayment(policy, FollowUpIssueCategory.MissInv, FirstDate.Value, LastDate.Value);
                                        ActionLogger.Logger.WriteFollowUpLog("PolicyID: " + FollowPolicy.PolicyId + ", Issue raised for MissInv (2)");
                                    }
                                }
                                if (noOfMissingCount != 0 && StoreFirstMissingMonth != null && IsAutoTrmDateUpadte)
                                {
                                    FollowUpUtill.AutoPolicyTerminateProcess(_PaymentMode, StoreFirstMissingMonth, policy.PolicyId, noOfMissingCount, null);
                                    ActionLogger.Logger.WriteFollowUpLog("PolicyID: " + FollowPolicy.PolicyId + ", AutoPolicyTerminateProcess run");
                                }
                            }
                            else
                            {
                                ActionLogger.Logger.WriteFollowUpLog("PolicyID: " + FollowPolicy.PolicyId + ", MissInv Issue skipped as per advance range value");
                            }
                        }
                        else
                        {
                            ActionLogger.Logger.WriteFollowUpLog("PolicyID: " + FollowPolicy.PolicyId + ", PolicyPaymentEntries > 0 ");
                            PolicySchedule _PolicySchedule = PostUtill.CheckForIncomingTypeOfSchedule(FollowPolicy.PolicyId);
                            //Acme -regster pmc issues `
                            if (FirstDate != null && LastDate != null)
                            {
                                ActionLogger.Logger.WriteFollowUpLog("PolicyID: " + FollowPolicy.PolicyId + ", processing for PMC, firstDate: " + FirstDate + ", LastDate: " + LastDate);
                                List<DisplayFollowupIssue> lstPMC = FollowupIssueLst.Where(p => (p.PolicyId == FollowPolicy.PolicyId) && (p.InvoiceDate >= FirstDate && p.InvoiceDate <= LastDate) && p.IsPMCVariance == true && p.IssueCategoryID == 3).ToList();

                                //Made a check that if policy is monthly/querter.y/semi-annuka, then check only when less than year old payment 
                                bool isVariancetoRaise = ((policy.PolicyModeId == 0 || policy.PolicyModeId == 1 || policy.PolicyModeId == 2) && FirstDate >= DateTime.Now.AddMonths(-12));

                                // Sep 03, 2019 - Logic added to handle advance payment
                                // If advance recieved and variance checked for first payment, total = total/advance to compare to PMC 
                                bool flagvarience = PostUtill.CheckForPMCVariance(_PolicyPaymentEntriesFormissing, policy.LearnedFields.PMC, Convert.ToDateTime(FirstDate), Convert.ToDateTime(LastDate), policy.OriginalEffectiveDate, policy.Advance, policy.PolicyModeId);
                              
                                if (flagvarience /* && isVariancetoRaise */)
                                {
                                    if (lstPMC != null && lstPMC.Count > 0 && lstPMC.FirstOrDefault().IssueStatusId != (int)FollowUpIssueStatus.Closed) //unclosed issue exsts , no need to raise
                                    {
                                        ActionLogger.Logger.WriteFollowUpLog("PolicyID: " + FollowPolicy.PolicyId + ", flagvarience true but issue already exists, no action");
                                    }
                                    else //raise issue as not existing
                                    {
                                        ActionLogger.Logger.WriteFollowUpLog("PolicyID: " + FollowPolicy.PolicyId + ", flagvarience true, adding issue");
                                        //FollowUpUtill.RegisterIssueAgainstScheduleVariance(ppe);
                                        //foreach (PolicyPaymentEntriesPost ppe in _PolicyPaymentEntriesFormissing)
                                        //{
                                        FollowUpUtill.RegisterIssueAgainstPMCVariance(FollowPolicy.PolicyId, FirstDate, LastDate, null);
                                        //    ActionLogger.Logger.WriteFollowUpLog("PolicyID: " + FollowPolicy.PolicyId + ", PaymentEntryID: " + ppe.PaymentEntryID + ", PMC issue raised");
                                        //}
                                    }
                                }
                                else
                                {
                                    ActionLogger.Logger.WriteFollowUpLog("PolicyID: " + FollowPolicy.PolicyId + ", flagvarience false, removing issue");
                                    DisplayFollowupIssue FollowupIssuetemp = lstPMC.FirstOrDefault();// FollowupIssueLst.Where(p => (p.PolicyId == FollowPolicy.PolicyId) && (p.InvoiceDate >= FirstDate && p.InvoiceDate <= LastDate) && p.IsPMCVariance == true).Where(p => p.IssueCategoryID == 3).FirstOrDefault();
                                    if (FollowupIssuetemp != null)
                                    {
                                        if (FollowupIssuetemp.IssueStatusId != (int)FollowUpIssueStatus.Closed)
                                        {
                                            FollowupIssuetemp.IssueStatusId = (int)FollowUpIssueStatus.Closed;
                                            FollowupIssuetemp.IssueResultId = (int)FollowUpResult.Resolved_CD;
                                            FollowupIssue.AddUpdate(FollowupIssuetemp);
                                        }
                                    }
                                }

                            }

                            StoreFirstMissingMonth = null;
                            noOfMissingCount = 0;
                            List<DisplayFollowupIssue> FollowupIssueMissingtoclose = FollowupIssueLst.Where(p => p.FromDate == FirstDate).Where(p => p.ToDate == LastDate).Where(p => (p.IssueCategoryID == (int)FollowUpIssueCategory.MissFirst || p.IssueCategoryID == (int)FollowUpIssueCategory.MissInv)).ToList();

                            if (FollowupIssueMissingtoclose != null && FollowupIssueMissingtoclose.Count > 0)
                            {
                                ActionLogger.Logger.WriteFollowUpLog("PolicyID: " + FollowPolicy.PolicyId + ", FollowupIssueMissingtoclose > 0 ");
                                foreach (DisplayFollowupIssue _fossu in FollowupIssueMissingtoclose)
                                {
                                    _fossu.IssueResultId = (int)FollowUpResult.Resolved_CD;
                                    _fossu.IssueStatusId = (int)FollowUpIssueStatus.Closed;
                                    FollowupIssue.AddUpdate(_fossu);
                                    ActionLogger.Logger.WriteFollowUpLog("PolicyID: " + FollowPolicy.PolicyId + ", Issue closed - " + _fossu.IssueId);
                                }
                            }
                            /*  else
                              {
                                  ActionLogger.Logger.WriteFollowUpLog("PolicyID: " + FollowPolicy.PolicyId + ", FollowupIssueMissingtoclose = 0 ");
                                  if (FirstDate <= policy.OriginalEffectiveDate && policy.OriginalEffectiveDate <= LastDate)
                                  {
                                      FollowUpUtill.RegisterIssueAgainstMissingPayment(policy, FollowUpIssueCategory.MissFirst, FirstDate.Value, LastDate.Value);
                                      ActionLogger.Logger.WriteFollowUpLog("PolicyID: " + FollowPolicy.PolicyId + ", Issue raised for MissFirst (1)");
                                  }
                                  else
                                  {
                                      FollowUpUtill.RegisterIssueAgainstMissingPayment(policy, FollowUpIssueCategory.MissInv, FirstDate.Value, LastDate.Value);
                                      ActionLogger.Logger.WriteFollowUpLog("PolicyID: " + FollowPolicy.PolicyId + ", Issue raised for MissInv (2)");
                                  }

                                  //Acme commented - FollowupIssueLst = FollowupIssue.GetIssues(policy.PolicyId); rdeundant
                                  //following will never execute if issfollist is null or count = 0
                                  //as FollowupIssueMissingtoclose1 is same as issfollist

                                  //if (FollowupIssueLst != null && FollowupIssueLst.Count > 0)
                                  //{
                                  //  List<DisplayFollowupIssue> FollowupIssueMissingtoclose1 = FollowupIssueLst.Where(p => p.FromDate == FirstDate).Where(p => p.ToDate == LastDate).Where(p => (p.IssueCategoryID == (int)FollowUpIssueCategory.MissFirst || p.IssueCategoryID == (int)FollowUpIssueCategory.MissInv)).ToList();
                                  //foreach (DisplayFollowupIssue _fossu in FollowupIssueMissingtoclose1) //
                                  //{
                                  //    _fossu.IssueResultId = (int)FollowUpResult.Resolved_CD;
                                  //    _fossu.IssueStatusId = (int)FollowUpIssueStatus.Closed;
                                  //    FollowupIssue.AddUpdate(_fossu);
                                  //}
                                 // }

                              }*/
                        }
                    }
                    
                    //      FollowupIssueLst = FollowupIssue.GetIssues(FollowPolicy.PolicyId);

                    //Code for manually resolved or closed the issue for commision dashboard
                    FollowupIssueLst = FollowupIssue.GetIssues(policy.PolicyId);
                    if (FollowupIssueLst != null && FollowupIssueLst.Count > 0)
                    {
                        ActionLogger.Logger.WriteFollowUpLog("PolicyID: " + FollowPolicy.PolicyId + ", FollowupIssueLst :" + FollowupIssueLst.ToStringDump());
                        foreach (PolicyPaymentEntriesPost ResolvedorClosedIssue in _AllResolvedorClosedIssueId)
                        {
                            DisplayFollowupIssue FollowupIssuetemp = FollowupIssueLst.Where(p => (p.PolicyId == ResolvedorClosedIssue.PolicyID) && (p.InvoiceDate == ResolvedorClosedIssue.InvoiceDate)).FirstOrDefault();
                            PolicyPaymentEntriesPost objPolicyPaymentEntriesPost = _AllResolvedorClosedIssueId.Where(p => (p.PolicyID == ResolvedorClosedIssue.PolicyID) && (p.InvoiceDate == ResolvedorClosedIssue.InvoiceDate)).Where(p => p.FollowUpIssueResolveOrClosed == 1).FirstOrDefault();

                            if (objPolicyPaymentEntriesPost != null && FollowupIssuetemp !=null)
                            {
                                if (objPolicyPaymentEntriesPost.PaymentEntryID != null)
                                {
                                    FollowupIssuetemp.IssueResultId = (int)FollowUpResult.Resolved_Brk;
                                    FollowupIssuetemp.IssueStatusId = (int)FollowUpIssueStatus.Closed;
                                    FollowupIssue.AddUpdate(FollowupIssuetemp);
                                    PolicyPaymentEntriesPost.UpdateVarPaymentIssueId(ResolvedorClosedIssue.PaymentEntryID, FollowupIssuetemp.IssueId);
                                    ActionLogger.Logger.WriteFollowUpLog("PolicyID: " + FollowPolicy.PolicyId + ", UpdateVarPaymentIssueId as closed" + FollowupIssuetemp.IssueId);
                                }
                            }
                            else
                            {
                                ActionLogger.Logger.WriteFollowUpLog("PolicyID: " + FollowPolicy.PolicyId + ", objPolicyPaymentEntriesPost null");
                            }
                        }

                     
                        //If policy settings. Track incoming %=. F. 
                        //Delete issues with (status=open or result =Resolved_CD) and (category=VarSchedule or category=VarCompDue)                    
                        if (policy.IsTrackIncomingPercentage == false)
                        {
                            ActionLogger.Logger.WriteFollowUpLog("PolicyID: " + FollowPolicy.PolicyId + ", IsTrackIncomingPercentage false");
                            //Acme -     List<DisplayFollowupIssue> AllIssueList = FollowupIssue.GetIssues(FollowPolicy.PolicyId);

                            //Fix on April 22, 2020 - Make sure PMC issues are not removed in this condition 
                            List<DisplayFollowupIssue> ForDeleteIssuelist = new List<DisplayFollowupIssue>(FollowupIssueLst.Where(p => p.IsPMCVariance != true && (p.IssueStatusId == (int)FollowUpIssueStatus.Open || p.IssueCategoryID == (int)FollowUpResult.Resolved_CD) && (p.IssueCategoryID == (int)FollowUpIssueCategory.VarCompDue || p.IssueCategoryID == (int)FollowUpIssueCategory.VarSchedule)));
                            foreach (var item in ForDeleteIssuelist)
                            {
                                if (item.IsResolvedFromCommDashboard != true)
                                {
                                    MyAgencyVault.BusinessLibrary.FollowUpUtill.UpdateIssueIdOfPaymentsForIssueId(item.IssueId, null);
                                    FollowupIssue.DeleteIssue(item.IssueId);
                                }
                            }
                        }
                        //If policy settings. Track missing month=. F. 
                        //Delete issues with (status=open or result =Resolved_CD) and (category=miss first or category=miss inv) 
                        if (policy.IsTrackMissingMonth == false)
                        {
                            ActionLogger.Logger.WriteFollowUpLog("PolicyID: " + FollowPolicy.PolicyId + ", IsTrackMissingMonth false");
                            //   List<DisplayFollowupIssue> AllIssueList = FollowupIssue.GetIssues(FollowPolicy.PolicyId);
                            //List<DisplayFollowupIssue> ForDeleteIssuelist = new List<DisplayFollowupIssue>(FollowupIssueLst.Where(p => (p.IssueStatusId == (int)FollowUpIssueStatus.Open || p.IssueCategoryID == (int)FollowUpResult.Resolved_CD) && (p.IssueCategoryID == (int)FollowUpIssueCategory.MissFirst || p.IssueCategoryID == (int)FollowUpIssueCategory.MissInv)));
                            
                            //Fix on April 22, 2020 - Make sure PMC issues are not removed in this condition 
                            List<DisplayFollowupIssue> ForDeleteIssuelist = new List<DisplayFollowupIssue>(FollowupIssueLst.Where(p => (p.IssueStatusId == (int)FollowUpIssueStatus.Open || p.IssueCategoryID == (int)FollowUpResult.Resolved_CD) && (p.IssueCategoryID == (int)FollowUpIssueCategory.VarCompDue || p.IssueCategoryID == (int)FollowUpIssueCategory.VarSchedule) && p.IsPMCVariance != true));

                            foreach (var item in ForDeleteIssuelist)
                            {
                                if (item.IsResolvedFromCommDashboard != true)
                                {
                                    MyAgencyVault.BusinessLibrary.FollowUpUtill.UpdateIssueIdOfPaymentsForIssueId(item.IssueId, null);
                                    FollowupIssue.DeleteIssue(item.IssueId);
                                }
                            }
                        }
                    }
                    //Checking variance issues moved to last by Acme 
                    //re -initialize issue list as some as closed/deleted above 
                    FollowupIssueLst = FollowupIssue.GetIssues(policy.PolicyId);
                    //if (policy.LastNoVarIssueDate != null)
                    //{
                    //    _AllPaymentEntriesOnPolicyFormissing = _AllPaymentEntriesOnPolicyFormissing.Where(x => x.CreatedOn >= policy.LastNoVarIssueDate).ToList();
                    //    ActionLogger.Logger.WriteFollowUpLog("Payment entries filtered based on creation date: " + policy.LastNoVarIssueDate);
                    //}
                    //else
                    //{
                    //    ActionLogger.Logger.WriteFollowUpLog("policy.LastNoVarIssueDate found null");
                    //}
                    foreach (PolicyPaymentEntriesPost ppepfv in _AllPaymentEntriesOnPolicyFormissing)
                    {
                        ActionLogger.Logger.WriteFollowUpLog("PolicyID: " + FollowPolicy.PolicyId + ", _AllPaymentEntriesOnPolicyFormissing processing ID - " + ppepfv.PaymentEntryID);
                        DisplayFollowupIssue FollowupIssuetemp = null;
                        if (FollowupIssueLst != null)
                        {
                            FollowupIssuetemp = FollowupIssueLst.Where(p => (p.PolicyId == ppepfv.PolicyID) && (p.InvoiceDate == ppepfv.InvoiceDate) && p.IsPMCVariance != true).Where(p => p.IssueCategoryID == 3).FirstOrDefault();
                        }
                        else
                        {
                            ActionLogger.Logger.WriteFollowUpLog("PolicyID: " + FollowPolicy.PolicyId + ", FollowupIssueLst null");
                        }
                        bool flagvarience = PostUtill.CheckForIncomingScheduleVariance(ppepfv, policy.ModeAvgPremium);
                        //Added check to skip the variance calculation in older data - sep 13, 2019
                        if (flagvarience)
                        {
                            ActionLogger.Logger.WriteFollowUpLog("PolicyID: " + FollowPolicy.PolicyId + ", flagvarience true");
                            if (FollowupIssuetemp == null)
                                FollowUpUtill.RegisterIssueAgainstScheduleVariance(ppepfv);
                            else
                                PolicyPaymentEntriesPost.UpdateVarPaymentIssueId(ppepfv.PaymentEntryID, FollowupIssuetemp.IssueId);
                        }
                        else
                        {
                            ActionLogger.Logger.WriteFollowUpLog("PolicyID: " + FollowPolicy.PolicyId + ", flagvarience false");
                            if (FollowupIssuetemp != null)
                            {
                                ActionLogger.Logger.WriteFollowUpLog("PolicyID: " + FollowPolicy.PolicyId + ",Followup Issue lIst true: " + FollowupIssuetemp.IssueId);
                                if (FollowupIssuetemp.IssueStatusId != (int)FollowUpIssueStatus.Closed)
                                {
                                    ActionLogger.Logger.WriteFollowUpLog("PolicyID: " + FollowPolicy.PolicyId + ",Variance isseu not closed ");
                                    FollowupIssuetemp.IssueStatusId = (int)FollowUpIssueStatus.Closed;
                                    FollowupIssuetemp.IssueResultId = (int)FollowUpResult.Resolved_CD;
                                    FollowupIssue.AddUpdate(FollowupIssuetemp);
                                    ActionLogger.Logger.WriteFollowUpLog("PolicyID: " + FollowPolicy.PolicyId + ",Variance isseu  closed ");
                                }
                            }
                        }
                    }

                    //Acme added to update the new variables from SP 
                    Policy.UpdateLastMissIssuesDate(FollowPolicy.PolicyId);
                    ActionLogger.Logger.WriteFollowUpLog("PolicyID: " + FollowPolicy.PolicyId + ", LastMissIssueDate updated ");
                    _FollowUpStatus.IsComplete = true;
                }
                else
                {
                    ActionLogger.Logger.WriteFollowUpLog("PolicyID: " + FollowPolicy.PolicyId + ", IsTrackPayment false ");
                }

                Policy.UpdateLastFollowupRunsWithTodayDate(FollowPolicy.PolicyId);
            }
            catch (Exception ex)
            {
                _FollowUpStatus.IsComplete = false;
                _FollowUpStatus.Actions += "--Error-";
                _FollowUpStatus.ex = ex;
                ActionLogger.Logger.WriteFollowUpErrorLog("PolicyID: " + FollowPolicy.PolicyId + ", Exception: " + ex.Message);
            }
            return _FollowUpStatus;
        }



        static bool IsIssueToBeRaised(int advance, DateTime from, DateTime? effDate)
        {
            ActionLogger.Logger.WriteFollowUpErrorLog("IsIssueToBeRaised: advance - " + advance + ", from - " + from  + ", effdate: " + effDate);

            if (advance > 0)
            {
                DateTime skipTillDate = Convert.ToDateTime(effDate).AddMonths(advance);
              
                if (from < skipTillDate)//Fixed on may 05, 2020 - to exclude the skiptillDate date   //means time period already received payment in advance '
                    return false;
            }

            return true;
        }

        private static int GetRangValue(PaymentMode _PaymentType)
        {
            int intVAlue = 0;
            switch (_PaymentType.ToString())
            {
                case "Monthly":
                    intVAlue = 1;
                    break;
                case "Quarterly":
                    intVAlue = 3;
                    break;
                case "HalfYearly":
                    intVAlue = 6;
                    break;
                case "Yearly":
                    intVAlue = 12;
                    break;
                case "OneTime":
                    intVAlue = 1;
                    break;
                case "Random":
                    intVAlue = 0;
                    break;
            }

            return intVAlue;
        }

    }
}
