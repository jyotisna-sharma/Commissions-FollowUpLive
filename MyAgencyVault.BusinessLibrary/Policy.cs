using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MyAgencyVault.BusinessLibrary.Base;
using MyAgencyVault.BusinessLibrary.Masters;
using System.Runtime.Serialization;
using DLinq = DataAccessLayer.LinqtoEntity;
using System.Transactions;
using System.Data;
using System.Threading;
using System.Linq.Expressions;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Data.EntityClient;
using System.Data.SqlClient;

namespace MyAgencyVault.BusinessLibrary
{
    [DataContract]
    public class PolicySavedStatus
    {
        [DataMember]
        public bool IsError { get; set; }
        [DataMember]
        public string ErrorMessage { get; set; }
    }

    public class Policy
    {
        #region Save and Delete Methods

        public static void AddUpdatePolicy(PolicyDetailsData _PolicyRecord)
        {
            using (DLinq.CommissionDepartmentEntities DataModel = Entity.DataModel)
            {
                var _policy = (from p in DataModel.Policies where (p.PolicyId == _PolicyRecord.PolicyId) select p).FirstOrDefault();
                if (_policy == null)
                {
                    _policy = new DLinq.Policy
                    {
                        PolicyId = _PolicyRecord.PolicyId,
                        PolicyNumber = _PolicyRecord.PolicyNumber,
                        PolicyType = _PolicyRecord.PolicyType,
                        Insured = _PolicyRecord.Insured,
                        OriginalEffectiveDate = _PolicyRecord.OriginalEffectiveDate,
                        TrackFromDate = _PolicyRecord.TrackFromDate,
                        MonthlyPremium = _PolicyRecord.ModeAvgPremium,
                        SubmittedThrough = _PolicyRecord.SubmittedThrough,
                        Enrolled = _PolicyRecord.Enrolled,
                        Eligible = _PolicyRecord.Eligible,
                        PolicyTerminationDate = _PolicyRecord.PolicyTerminationDate,
                        IsTrackMissingMonth = _PolicyRecord.IsTrackMissingMonth,
                        IsTrackIncomingPercentage = _PolicyRecord.IsTrackIncomingPercentage,
                        IsTrackPayment = _PolicyRecord.IsTrackPayment,
                        IsDeleted = false,
                        ReplacedBy = _PolicyRecord.ReplacedBy,
                        DuplicateFrom = _PolicyRecord.DuplicateFrom,
                        CreatedOn = DateTime.Now,
                        IsIncomingBasicSchedule = _PolicyRecord.IsIncomingBasicSchedule,
                        IsOutGoingBasicSchedule = _PolicyRecord.IsOutGoingBasicSchedule,
                        SplitPercentage = _PolicyRecord.SplitPercentage,
                        Advance=_PolicyRecord.Advance,
                        ProductType = _PolicyRecord.ProductType,
                        UserCredentialId = _PolicyRecord.UserCredentialId,
                        AccoutExec = _PolicyRecord.AccoutExec
                    };
                    _policy.MasterPolicyStatuReference.Value = (from m in DataModel.MasterPolicyStatus where m.PolicyStatusId == _PolicyRecord.PolicyStatusId select m).FirstOrDefault();
                    _policy.ClientReference.Value = (from s in DataModel.Clients where s.ClientId == _PolicyRecord.ClientId select s).FirstOrDefault();
                    _policy.LicenseeReference.Value = (from l in DataModel.Licensees where l.LicenseeId == _PolicyRecord.PolicyLicenseeId select l).FirstOrDefault();

                    //_policy.MasterPolicyModeReference.Value = (from m in DataModel.MasterPolicyModes where m.PolicyModeId == _PolicyRecord.PolicyModeId select m).FirstOrDefault();
                    _policy.PolicyModeId = _PolicyRecord.PolicyModeId;

                    _policy.CoverageReference.Value = (from s in DataModel.Coverages where s.CoverageId == _PolicyRecord.CoverageId select s).FirstOrDefault();
                    _policy.MasterPolicyTerminationReasonReference.Value = (from s in DataModel.MasterPolicyTerminationReasons where s.PTReasonId == _PolicyRecord.TerminationReasonId select s).FirstOrDefault();
                    _policy.MasterIncomingPaymentTypeReference.Value = (from m in DataModel.MasterIncomingPaymentTypes where m.IncomingPaymentTypeId == _PolicyRecord.IncomingPaymentTypeId select m).FirstOrDefault();
                    _policy.PayorReference.Value = (from m in DataModel.Payors where m.PayorId == _PolicyRecord.PayorId select m).FirstOrDefault();
                    _policy.UserCredentialReference.Value = (from s in DataModel.UserCredentials where s.UserCredentialId == _PolicyRecord.CreatedBy select s).FirstOrDefault();
                    _policy.CarrierReference.Value = (from m in DataModel.Carriers where m.CarrierId == _PolicyRecord.CarrierID select m).FirstOrDefault();

                    if (_policy.PolicyStatusId == ((int)_PolicyStatus.Active))
                        _policy.ActivatedOn = _policy.CreatedOn;
                    else
                        _policy.ActivatedOn = null;

                    DataModel.AddToPolicies(_policy);

                }
                else
                {
                    _policy.PolicyId = _PolicyRecord.PolicyId;
                    _policy.PolicyNumber = _PolicyRecord.PolicyNumber;
                    _policy.PolicyType = _PolicyRecord.PolicyType;
                    _policy.Insured = _PolicyRecord.Insured;
                    _policy.OriginalEffectiveDate = _PolicyRecord.OriginalEffectiveDate;
                    _policy.TrackFromDate = _PolicyRecord.TrackFromDate;
                    _policy.MonthlyPremium = _PolicyRecord.ModeAvgPremium;
                    _policy.SubmittedThrough = _PolicyRecord.SubmittedThrough;
                    _policy.Enrolled = _PolicyRecord.Enrolled;
                    _policy.Eligible = _PolicyRecord.Eligible;
                    _policy.PolicyTerminationDate = _PolicyRecord.PolicyTerminationDate;
                    _policy.IsTrackMissingMonth = _PolicyRecord.IsTrackMissingMonth;
                    _policy.IsTrackIncomingPercentage = _PolicyRecord.IsTrackIncomingPercentage;
                    _policy.IsTrackPayment = _PolicyRecord.IsTrackPayment;
                    _policy.IsDeleted = _PolicyRecord.IsDeleted;
                    _policy.ReplacedBy = _PolicyRecord.ReplacedBy;
                    _policy.DuplicateFrom = _PolicyRecord.DuplicateFrom;
                    _policy.IsIncomingBasicSchedule = _PolicyRecord.IsIncomingBasicSchedule;
                    _policy.IsOutGoingBasicSchedule = _PolicyRecord.IsOutGoingBasicSchedule;
                    _policy.SplitPercentage = _PolicyRecord.SplitPercentage;

                    //recently added
                    _policy.Advance = _PolicyRecord.Advance;
                    _policy.ProductType = _PolicyRecord.ProductType;

                    //added 15012016
                    _policy.UserCredentialId = _PolicyRecord.UserCredentialId;
                    _policy.AccoutExec = _PolicyRecord.AccoutExec;


                    _policy.ClientReference.Value = (from s in DataModel.Clients where s.ClientId == _PolicyRecord.ClientId select s).FirstOrDefault();
                    _policy.LicenseeReference.Value = (from l in DataModel.Licensees where l.LicenseeId == _PolicyRecord.PolicyLicenseeId select l).FirstOrDefault();

                    if ((_policy.PolicyStatusId == ((int)_PolicyStatus.Pending)) && (_PolicyRecord.PolicyStatusId == ((int)_PolicyStatus.Active)))
                    {
                        if (_policy.ActivatedOn == null)
                        {
                            _policy.ActivatedOn = DateTime.Now;
                        }
                    }
                   
                    _policy.MasterPolicyStatuReference.Value = (from m in DataModel.MasterPolicyStatus where m.PolicyStatusId == _PolicyRecord.PolicyStatusId select m).FirstOrDefault();
                   
                    if (_PolicyRecord.PolicyModeId != null)
                        _policy.PolicyModeId = _PolicyRecord.PolicyModeId;
                    else
                        _policy.PolicyModeId = new System.Nullable<int>();

                    if (_PolicyRecord.CoverageId == Guid.Empty)
                    {
                        _policy.CoverageId = null;
                    }
                    else
                    {
                        _policy.CoverageId = _PolicyRecord.CoverageId;
                    }

                    //Add /Update termination date
                    if ((_PolicyRecord.PolicyStatusId == ((int)_PolicyStatus.Pending)) || (_PolicyRecord.PolicyStatusId == ((int)_PolicyStatus.Active)))
                    {
                        _policy.TerminationReasonId = null;
                    }
                    else
                    {
                        if (_PolicyRecord.TerminationReasonId != null)
                        {
                            _policy.MasterPolicyTerminationReasonReference.Value = (from s in DataModel.MasterPolicyTerminationReasons where s.PTReasonId == _PolicyRecord.TerminationReasonId select s).FirstOrDefault();
                        }
                        else
                        {
                            _policy.TerminationReasonId = null;
                        }
                    }
                   // _policy.MasterPolicyTerminationReasonReference.Value = (from s in DataModel.MasterPolicyTerminationReasons where s.PTReasonId == _PolicyRecord.TerminationReasonId select s).FirstOrDefault();
                    _policy.MasterIncomingPaymentTypeReference.Value = (from m in DataModel.MasterIncomingPaymentTypes where m.IncomingPaymentTypeId == _PolicyRecord.IncomingPaymentTypeId select m).FirstOrDefault();

                    if (_PolicyRecord.PayorId == Guid.Empty)
                    {
                        _policy.PayorId = null;
                    }
                    else
                    {
                        _policy.PayorId = _PolicyRecord.PayorId;
                    }
                    // _policy.PayorReference.Value = (from m in DataModel.Payors where m.PayorId == _PolicyRecord.PayorId select m).FirstOrDefault();

                    _policy.UserCredentialReference.Value = (from s in DataModel.UserCredentials where s.UserCredentialId == _PolicyRecord.CreatedBy select s).FirstOrDefault();
                    if (_PolicyRecord.CarrierID == Guid.Empty)
                    {
                        _policy.CarrierId = null;
                    }
                    else
                    {
                        _policy.CarrierId = _PolicyRecord.CarrierID;
                    }
                    // _policy.CarrierReference.Value = (from m in DataModel.Carriers where m.CarrierId == _PolicyRecord.CarrierID select m).FirstOrDefault();

                }
                //DataModel.Policies.Where(p => p.PolicyId == _PolicyRecord.PolicyId).FirstOrDefault().CoverageId = null;
                DataModel.SaveChanges();
            }
        }

        public static void UpdatePendingPolicy(DEU _DEU)
        {
            using (DLinq.CommissionDepartmentEntities DataModel = Entity.DataModel)
            {
                var _policy = (from p in DataModel.Policies where (p.PolicyId == _DEU.PolicyId) select p).FirstOrDefault();
                if (_policy != null)
                {
                    // in case of pending policy
                    //Update the following details
                    if (_policy.PolicyStatusId==2)
                    {
                        if (_DEU.SplitPer != null)
                            _policy.SplitPercentage = _DEU.SplitPer;

                        if (_DEU.PolicyMode != null)
                            _policy.PolicyModeId = _DEU.PolicyMode;

                        if (_DEU.OriginalEffectiveDate != null)
                            _policy.OriginalEffectiveDate = _DEU.OriginalEffectiveDate;

                        //if (_DEU.TrackFromDate != null)
                        //    _policy.TrackFromDate = _DEU.TrackFromDate;

                        if (_DEU.Eligible != null)                       
                            _policy.Eligible = _DEU.Eligible;                        

                        if (_DEU.Enrolled != null)                      
                            _policy.Enrolled = _DEU.Enrolled;

                        if (_DEU.CompTypeID != null)
                            _policy.IncomingPaymentTypeId = _DEU.CompTypeID;                        
                        
                      
                        DataModel.SaveChanges();
                    }
                    
                }
            }
        }

        public static void UpdatePolicyClient(Guid policyId,Guid clientID)
        {
            try
            {
                using (DLinq.CommissionDepartmentEntities DataModel = Entity.DataModel)
                {
                    var _policy = (from p in DataModel.Policies where (p.PolicyId == policyId) select p).FirstOrDefault();
                    if (_policy != null)
                    {
                        _policy.PolicyClientId = clientID;
                        DataModel.SaveChanges();


                    }
                }
            }
            catch
            {
            }
        }

        public static void UpdatePolicyClientLernedFields(Guid policyId, Guid clientID)
        {
            try
            {
                using (DLinq.CommissionDepartmentEntities DataModel = Entity.DataModel)
                {
                    var _policy = (from p in DataModel.PolicyLearnedFields where (p.PolicyId == policyId) select p).FirstOrDefault();
                    if (_policy != null)
                    {
                        _policy.ClientID = clientID;
                         DataModel.SaveChanges();


                    }
                }
            }
            catch
            {
            }
        }

        public static void UpdateMode(Guid _PolicyID, int PolicyMode)
        {
            using (DLinq.CommissionDepartmentEntities DataModel = Entity.DataModel)
            {
                var _policy = (from p in DataModel.Policies where (p.PolicyId == _PolicyID) select p).FirstOrDefault();
                if (_policy != null)
                {
                    // in case of pending policy
                    if (_policy.PolicyStatusId == 2)
                    {
                        _policy.PolicyModeId = PolicyMode;
                        DataModel.SaveChanges();
                    }

                }
            }
        }

        public static PolicySavedStatus SavePolicyData(PolicyDetailsData OriginalPolicy, PolicyDetailsData ReplacedPolicy)
        {
            PolicySavedStatus status = new PolicySavedStatus();

            TransactionOptions options = new TransactionOptions
            {
                IsolationLevel = System.Transactions.IsolationLevel.ReadCommitted,
                Timeout = TimeSpan.FromMinutes(15)
            };

            if (OriginalPolicy != null || ReplacedPolicy != null)
            {
                try
                {
                    using (TransactionScope transaction = new TransactionScope(TransactionScopeOption.Required, options))
                    {
                        if (ReplacedPolicy != null)
                        {
                            Policy.AddUpdatePolicy(ReplacedPolicy);
                            Policy.AddUpdatePolicyHistory(ReplacedPolicy.PolicyId);
                            PolicyLearnedField.AddUpdateHistoryLearned(ReplacedPolicy.PolicyId);
                        }
                        if (OriginalPolicy != null)
                        {
                            Policy.AddUpdatePolicy(OriginalPolicy);
                            PolicyToLearnPost.AddUpdatPolicyToLearn(OriginalPolicy.PolicyId);
                            Policy.AddUpdatePolicyHistory(OriginalPolicy.PolicyId);
                            PolicyLearnedField.AddUpdateHistoryLearned(OriginalPolicy.PolicyId);
                        }
                        transaction.Complete();
                    }
                }
                catch
                {
                    status.IsError = true;
                    status.ErrorMessage = "Policy " + OriginalPolicy.PolicyNumber + " is not saved succesfully.";
                }
            }

            return status;
        }

        public static PolicySavedStatus SavePolicy(PolicyDetailsData OriginalPolicy, PolicyDetailsData ReplacedPolicy, string strRenewal,string strCoverageNickName)
        {
            PolicySavedStatus status = new PolicySavedStatus();

            TransactionOptions options = new TransactionOptions
            {
                IsolationLevel = System.Transactions.IsolationLevel.ReadCommitted,
                Timeout = TimeSpan.FromMinutes(60)
            };

            if (OriginalPolicy != null || ReplacedPolicy != null)
            {
                try
                {
                    using (TransactionScope transaction = new TransactionScope(TransactionScopeOption.Required, options))
                    {
                        if (ReplacedPolicy != null)
                        {
                            Policy.AddUpdatePolicy(ReplacedPolicy);
                            Policy.AddUpdatePolicyHistory(ReplacedPolicy.PolicyId);
                            PolicyLearnedField.AddUpdateHistoryLearned(ReplacedPolicy.PolicyId);
                        }
                        if (OriginalPolicy != null)
                        {
                         
                            Policy.AddUpdatePolicy(OriginalPolicy);                                       
                            PolicyToLearnPost.AddPolicyToLearn(OriginalPolicy.PolicyId, strRenewal, strCoverageNickName, OriginalPolicy.ProductType);
                           
                        }
                        transaction.Complete();
                    }
                }
                catch
                {
                    status.IsError = true;
                    status.ErrorMessage = "Policy " + OriginalPolicy.PolicyNumber + " is not saved succesfully.";
                }
            }

            return status;
        }

        public static void UpdateRPolicyStatus(PolicyDetailsData _policyr)
        {
            using (DLinq.CommissionDepartmentEntities DataModel = Entity.DataModel)
            {
                var _policy = (from p in DataModel.Policies where (p.PolicyId == _policyr.PolicyId) select p).FirstOrDefault();
                if (_policy != null)
                {
                    _policy.MasterPolicyStatuReference.Value = (from m in DataModel.MasterPolicyStatus where m.PolicyStatusId == (int)_PolicyStatus.Terminated select m).FirstOrDefault();
                    DataModel.SaveChanges();
                }
            }
        }

        public void UpdatePolicyTermDate(Guid policyID, DateTime? dtTermReson)
        {
            using (DLinq.CommissionDepartmentEntities DataModel = Entity.DataModel)
            {
                var _policy = (from p in DataModel.Policies where (p.PolicyId == policyID) select p).FirstOrDefault();
                if (_policy != null)
                {
                    _policy.MasterPolicyStatuReference.Value = (from m in DataModel.MasterPolicyStatus where m.PolicyStatusId == (int)_PolicyStatus.Terminated select m).FirstOrDefault();
                    // _policy.PolicyStatusId=1;
                    //Term reson "Per Carrier
                    _policy.TerminationReasonId = 5;
                    _policy.PolicyTerminationDate = dtTermReson;
                    DataModel.SaveChanges();
                }
            }
        }

        public static void UpdatePolicySetting(PolicyDetailsData policy)
        {
            using (DLinq.CommissionDepartmentEntities DataModel = Entity.DataModel)
            {
                DataModel.Policies.Where(p => p.PolicyId == policy.PolicyId).FirstOrDefault().IsTrackMissingMonth = policy.IsTrackMissingMonth;
                DataModel.Policies.Where(p => p.PolicyId == policy.PolicyId).FirstOrDefault().IsTrackIncomingPercentage = policy.IsTrackIncomingPercentage;
                DataModel.SaveChanges();
            }
        }

        public static string GetPolicyProductType(Guid policyID, Guid PayorID, Guid CarrierID, Guid CoverageID)
        {
            string strNickName = string.Empty;

            try
            {
                using (DLinq.CommissionDepartmentEntities DataModel = Entity.DataModel)
                {
                    var strproductType = from p in DataModel.Policies
                                              where (p.PolicyId == policyID && p.PayorId == PayorID && p.CarrierId==CarrierID && p.CoverageId==CoverageID)
                                              select p.ProductType;

                    foreach (var item in strproductType)
                    {
                        if (!string.IsNullOrEmpty(item))
                        {
                            strNickName = Convert.ToString(item);
                        }

                    }

                }

            }
            catch
            {
            }
            return strNickName;
        }

        //Acme - Method to Update the last miss invoice issue date 
        public static void UpdateLastMissIssuesDate(Guid PolicyId)
        {
            try
            {
                if (PolicyId == Guid.Empty || PolicyId == null) return;

                DLinq.CommissionDepartmentEntities ctx = new DLinq.CommissionDepartmentEntities(); //create your entity object here
                EntityConnection ec = (EntityConnection)ctx.Connection;
                SqlConnection sc = (SqlConnection)ec.StoreConnection; //get the SQLConnection that your entity object would use
                string adoConnStr = sc.ConnectionString;

                using (SqlConnection con = new SqlConnection(adoConnStr))
                {
                    using (SqlCommand cmd = new SqlCommand("[Usp_UpdateLastMissIssueDate]", con))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@policyID", PolicyId);
                        con.Open();
                        cmd.ExecuteNonQuery();
                    }
                }
               
            }
            catch(Exception ex)
            {
                ActionLogger.Logger.WriteLog("UpdateLastMissIssuesDate: PolicyID " + PolicyId + ", error: " + ex.Message, true);
            }
        }

        public static void UpdateLastFollowupRunsWithTodayDate(Guid PolicyId)
        {
            try
            {
                if (PolicyId == Guid.Empty) return;
                if (PolicyId == null) return;
                using (DLinq.CommissionDepartmentEntities DataModel = Entity.DataModel)
                {
                    DataModel.UpdatelastFollowupRuns(PolicyId);
                }
            }
            catch
            {
            }

        }

        public static void MarkPolicyDeleted(PolicyDetailsData _policyrecord)
        {
            using (DLinq.CommissionDepartmentEntities DataModel = Entity.DataModel)
            {
                try
                {
                    var _policy = (from p in DataModel.Policies where (p.PolicyId == _policyrecord.PolicyId) select p).FirstOrDefault();
                    if (_policy != null)
                    {
                        _policy.IsDeleted = true;
                        DataModel.SaveChanges();
                    }
                }
                catch
                {
                }
            }
        }

        public static void MarkPolicyDeletedById(Guid PolicyId)
        {
            using (DLinq.CommissionDepartmentEntities DataModel = Entity.DataModel)
            {
                try
                {
                    var _policy = (from p in DataModel.Policies where (p.PolicyId == PolicyId) select p).FirstOrDefault();
                    if (_policy != null)
                    {
                        _policy.IsDeleted = true;
                        DataModel.SaveChanges();
                    }
                   
                }
                catch
                {
                }
            }
        }

        public static void DeletePolicyFromDB(PolicyDetailsData _Policy)
        {
            using (DLinq.CommissionDepartmentEntities DataModel = Entity.DataModel)
            {
                var _policy = (from p in DataModel.Policies where (p.PolicyId == _Policy.PolicyId) select p).FirstOrDefault();
                //Check null before going to delete on the basis of Policy ID
                if (_policy == null) return;
                DataModel.DeleteObject(_policy);
                DataModel.SaveChanges();

            }
        }

        public static void DeletePolicyFromDBById(Guid PolicyId)
        {
            using (DLinq.CommissionDepartmentEntities DataModel = Entity.DataModel)
            {
                var _policy = (from p in DataModel.Policies where (p.PolicyId == PolicyId) select p).FirstOrDefault();
                DataModel.DeleteObject(_policy);
                DataModel.SaveChanges();
            }
        }

        public static void DeletePolicyCascadeFromDBById(Guid PolicyId)
        {
            using (DLinq.CommissionDepartmentEntities DataModel = Entity.DataModel)
            {
                //PolicyOutgoingSchedule
                List<DLinq.PolicyOutgoingSchedule> _PolicyOutgoingSchedulelst = DataModel.PolicyOutgoingSchedules.Where(p => p.PolicyId == PolicyId).ToList();
                if (_PolicyOutgoingSchedulelst != null)
                {
                    foreach (DLinq.PolicyOutgoingSchedule pos in _PolicyOutgoingSchedulelst)
                        DataModel.DeleteObject(pos);
                }

                //PolicyOutgoingAdvancedSchedule 
                List<DLinq.PolicyOutgoingAdvancedSchedule> _PolicyOutgoingAdvancedScheduleLst = DataModel.PolicyOutgoingAdvancedSchedules
                                                                                                .Where(p => p.PolicyId == PolicyId).ToList();
                if (_PolicyOutgoingAdvancedScheduleLst != null)
                {
                    foreach (DLinq.PolicyOutgoingAdvancedSchedule poas in _PolicyOutgoingAdvancedScheduleLst)
                    {
                        DataModel.DeleteObject(poas);
                    }

                }
                //PolicyNote
                List<DLinq.PolicyNote> _PolicyNoteLst = DataModel.PolicyNotes.Where(p => p.PolicyId == PolicyId).ToList();
                if (_PolicyNoteLst != null)
                {
                    foreach (DLinq.PolicyNote pn in _PolicyNoteLst)
                    {
                        DataModel.DeleteObject(pn);
                    }
                }

                //PolicyLevelBillingDetail
                List<DLinq.PolicyLevelBillingDetail> _PolicyLevelBillingDetailLst = DataModel.PolicyLevelBillingDetails.Where(p => p.PolicyId == PolicyId).ToList();
                if (_PolicyLevelBillingDetailLst != null)
                {
                    foreach (DLinq.PolicyLevelBillingDetail plbd in _PolicyLevelBillingDetailLst)
                    {
                        DataModel.DeleteObject(plbd);
                    }
                }

                //PolicyLearnedFieldsHistory
                List<DLinq.PolicyLearnedFieldsHistory> _PolicyLearnedFieldsHistoryLst = DataModel.PolicyLearnedFieldsHistories.Where(p => p.PolicyId == PolicyId).ToList();
                if (_PolicyLearnedFieldsHistoryLst != null)
                {
                    foreach (DLinq.PolicyLearnedFieldsHistory plfh in _PolicyLearnedFieldsHistoryLst)
                    {
                        DataModel.DeleteObject(plfh);
                    }
                }

                //PolicyLearnedField
                List<DLinq.PolicyLearnedField> _PolicyLearnedFieldLst = DataModel.PolicyLearnedFields.Where(p => p.PolicyId == PolicyId).ToList();
                if (_PolicyLearnedFieldLst != null)
                {
                    foreach (DLinq.PolicyLearnedField plf in _PolicyLearnedFieldLst)
                    {
                        DataModel.DeleteObject(plf);
                    }
                }

                //PolicyIncomingAdvancedSchedule
                List<DLinq.PolicyIncomingAdvancedSchedule> _PolicyIncomingAdvancedScheduleLst = DataModel.PolicyIncomingAdvancedSchedules.Where(p => p.PolicyId == PolicyId).ToList();
                if (_PolicyIncomingAdvancedScheduleLst != null)
                {
                    foreach (DLinq.PolicyIncomingAdvancedSchedule pias in _PolicyIncomingAdvancedScheduleLst)
                    {
                        DataModel.DeleteObject(pias);
                    }
                }

                //PolicyIncomingSchedule
                List<DLinq.PolicyIncomingSchedule> _PolicyIncomingScheduleLst = DataModel.PolicyIncomingSchedules
                                                                                    .Where(p => p.PolicyId == PolicyId).ToList();
                if (_PolicyIncomingScheduleLst != null)
                {
                    foreach (DLinq.PolicyIncomingSchedule pis in _PolicyIncomingScheduleLst)
                    {
                        DataModel.DeleteObject(pis);
                    }
                }

                //LastPolicyViewed
                List<DLinq.LastPolicyViewed> _LastPolicyViewedLst = DataModel.LastPolicyVieweds
                                                                    .Where(p => p.PolicyId == PolicyId).ToList();

                if (_LastPolicyViewedLst != null)
                {
                    foreach (DLinq.LastPolicyViewed lpv in _LastPolicyViewedLst)
                    {

                        DataModel.DeleteObject(lpv);
                    }
                }

                //FollowupIssue
                List<DLinq.FollowupIssue> _FollowupIssueLst = DataModel.FollowupIssues
                                                            .Where(p => p.PolicyId == PolicyId).ToList();
                if (_FollowupIssueLst != null)
                {
                    foreach (DLinq.FollowupIssue fil in _FollowupIssueLst)
                    {
                        DataModel.DeleteObject(fil);
                    }
                }
                
                //PolicyPaymentEntry
                List<DLinq.PolicyPaymentEntry> _PolicyPaymentEntryLst = DataModel.PolicyPaymentEntries.Where(p => p.PolicyId == PolicyId).ToList();
                if (_PolicyPaymentEntryLst != null)
                {
                    foreach (DLinq.PolicyPaymentEntry ppel in _PolicyPaymentEntryLst)
                    {
                        List<DLinq.PolicyOutgoingPayment> _PolicyOutgoingPaymentLst = DataModel.PolicyOutgoingPayments
                                                        .Where(p => p.PaymentEntryId == ppel.PaymentEntryId).ToList();
                        foreach (DLinq.PolicyOutgoingPayment popl in _PolicyOutgoingPaymentLst)
                        {
                            DataModel.DeleteObject(popl);
                        }

                    }
                    foreach (DLinq.PolicyPaymentEntry ppel in _PolicyPaymentEntryLst)
                    {
                        var payment_followup = DataModel.FollowupIssues.Where(pf => pf.IssueId == ppel.FollowUpVarIssueId).AsEnumerable();
                        foreach (var pay_follow in payment_followup)
                        {
                            DataModel.DeleteObject(pay_follow);
                        }
                        DataModel.DeleteObject(ppel);
                    }
                }

                List<DLinq.EntriesByDEU> _entryByDEU = DataModel.EntriesByDEUs.Where(p => p.PolicyID == PolicyId).ToList();
                if (_entryByDEU != null)
                {
                    foreach (DLinq.EntriesByDEU ebd in _entryByDEU)
                    {
                        DataModel.DeleteObject(ebd);
                    }
                }

                var _policyHis = (from p in DataModel.PoliciesHistories where (p.PolicyId == PolicyId) select p).FirstOrDefault();
                if (_policyHis != null)
                {
                    DataModel.DeleteObject(_policyHis);
                }

                var _policy = (from p in DataModel.Policies where (p.PolicyId == PolicyId) select p).FirstOrDefault();
                DataModel.DeleteObject(_policy);
                DataModel.SaveChanges();
            }
        }

        public static bool CheckForPolicyPaymentExists(Guid Policyid)
        {
            bool flag = false;

            Dictionary<string, object> parameters = new Dictionary<string, object>();
            parameters.Add("PolicyId", Policyid);
            PolicyDetailsData _policylst = Policy.GetPolicyData(parameters).FirstOrDefault();           
            if (_policylst != null)
            {
                if (_policylst.policyPaymentEntries == null)
                {
                    _policylst.policyPaymentEntries = PolicyPaymentEntriesPost.GetPolicyPaymentEntryPolicyIDWise(Policyid);
                }
                List<PolicyPaymentEntriesPost> _PolicyPaymentEntriesPost = _policylst.policyPaymentEntries;
                foreach (PolicyPaymentEntriesPost popy in _PolicyPaymentEntriesPost)
                {
                    List<PolicyOutgoingDistribution> _PolicyOutgoingDistributionLst = PolicyOutgoingDistribution.GetOutgoingPaymentByPoicyPaymentEntryId(popy.PaymentEntryID);
                    if (_PolicyOutgoingDistributionLst != null && _PolicyOutgoingDistributionLst.Count != 0)
                    {
                        flag = true;
                        return flag;
                    }
                }
            }
           
            return flag;
        }

        #endregion

        #region Get Methods

        public static int? GetPolicyStatusID(Guid PolicyId)
        {
            using (DLinq.CommissionDepartmentEntities DataModel = Entity.DataModel)
            {
                var policyStatusId = from pl in DataModel.Policies
                                     where (pl.IsDeleted == false) && (pl.PolicyId == PolicyId)
                                     select new { pl.PolicyStatusId }.PolicyStatusId;
                return (int?)policyStatusId.SingleOrDefault();
            }
        }

        public static bool FollowUpRunsRequired(Guid PolicyId)
        {
            bool flag = true;
            try
            {
                using (DLinq.CommissionDepartmentEntities DataModel = Entity.DataModel)
                {
                    DateTime? LastFollowUpRuns = DataModel.Policies.Where(p => p.PolicyId == PolicyId).FirstOrDefault().LastFollowUpRuns;
                    int DaysCnt = Convert.ToInt32(SystemConstant.GetKeyValue(MasterSystemConst.NextFollowUpRunDaysCount.ToString()));
                    if (LastFollowUpRuns.HasValue)
                    {
                        DateTime finaldatetorun = LastFollowUpRuns.Value.AddDays(DaysCnt);
                        if (DateTime.Today > finaldatetorun)
                            flag = true;
                        else
                            flag = false;
                    }
                }
            }
            catch (Exception ex)
            {
                ActionLogger.Logger.WriteLog("FollowUpRunsRequired for PolicyID: " + PolicyId + ", ex: " + ex.Message, true);
            }
            return flag;
        }

        public static bool IsTrackPaymentChecked(Guid PolicyId)
        {
            using (DLinq.CommissionDepartmentEntities DataModel = Entity.DataModel)
            {
                var isTrackPaymentchecked = (from p in DataModel.Policies
                                             where p.PolicyId == PolicyId
                                             select new { p.IsTrackPayment }.IsTrackPayment).FirstOrDefault();
                return isTrackPaymentchecked;
            }

        }

        public DateTime ? GetFollowUpDate(Guid PolicyID)
        {
            DateTime? dtFollowupDate = null;
            using (DLinq.CommissionDepartmentEntities DataModel = Entity.DataModel)
            {
                dtFollowupDate = (from pl in DataModel.Policies.Where(p => p.PolicyId == PolicyID)
                                  select pl.LastFollowUpRuns).FirstOrDefault();

                return dtFollowupDate;

            }

        }

        public static List<PolicyDetailsData> GetPolicyDataForWindowService(Dictionary<string, object> Parameters, Expression<Func<DLinq.Policy, bool>> ParameterExpression = null)
        {
            Expression<Func<DLinq.Policy, bool>> parametersFromHelperClass = HelperClass.getWhereClause<DLinq.Policy>(Parameters);
            if (ParameterExpression != null)
            {
                parametersFromHelperClass = parametersFromHelperClass.And(ParameterExpression);
            }
            return FillinAllDataForWindowService(parametersFromHelperClass, ParameterExpression);
        }

        private static List<PolicyDetailsData> FillinAllDataForWindowService(Expression<Func<DLinq.Policy, bool>> parameters, Expression<Func<DLinq.Policy, bool>> ParameterExpression = null)
        {
            List<DLinq.Policy> policies;
            using (DLinq.CommissionDepartmentEntities DataModel = Entity.DataModel)
            {
                policies = (from pl in DataModel.Policies
                            .Where(parameters)
                            select pl).ToList();
                List<PolicyDetailsData> endList = new List<PolicyDetailsData>();

                if (policies == null)
                {
                    return endList;
                }
                try
                {
                    endList = (from pl in policies
                               select new PolicyDetailsData
                               {
                                   //PolicyId = pl.PolicyId == null ? Guid.Empty : pl.PolicyId,
                                   PolicyId = pl.PolicyId,
                                   PolicyNumber = pl.PolicyNumber == null ? string.Empty : pl.PolicyNumber,
                                   PolicyStatusId = pl.MasterPolicyStatu.PolicyStatusId,
                                   PolicyStatusName = pl.MasterPolicyStatu.Name,
                                   PolicyType = pl.PolicyType == null ? string.Empty : pl.PolicyType,
                                   PolicyLicenseeId = pl.Licensee.LicenseeId,
                                   Insured = pl.Insured == null ? string.Empty : pl.Insured,
                                   OriginalEffectiveDate = pl.OriginalEffectiveDate,
                                   TrackFromDate = pl.TrackFromDate,
                                   PolicyModeId = pl.MasterPolicyMode.PolicyModeId,
                                   ModeAvgPremium = pl.MonthlyPremium,
                                   SubmittedThrough = pl.SubmittedThrough == null ? string.Empty : pl.SubmittedThrough,
                                   Enrolled = pl.Enrolled == null ? string.Empty : pl.Enrolled,
                                   Eligible = pl.Eligible == null ? string.Empty : pl.Eligible,
                                   PolicyTerminationDate = pl.PolicyTerminationDate,
                                   TerminationReasonId = pl.TerminationReasonId,
                                   IsTrackMissingMonth = pl.IsTrackMissingMonth,
                                   IsTrackIncomingPercentage = pl.IsTrackIncomingPercentage,
                                   IsTrackPayment = pl.IsTrackPayment,
                                   IsDeleted = pl.IsDeleted,
                                   CarrierID = pl.Carrier == null ? Guid.Empty : pl.Carrier.CarrierId,
                                   CarrierName = pl.Carrier == null ? string.Empty : pl.Carrier.CarrierName,
                                   CoverageId = pl.Coverage == null ? Guid.Empty : pl.Coverage.CoverageId,
                                   CoverageName = pl.Coverage == null ? string.Empty : pl.Coverage.ProductName,
                                   ClientId = pl.Client == null ? Guid.Empty : pl.Client.ClientId,
                                   ClientName = pl.Client == null ? string.Empty : pl.Client.Name,
                                   ReplacedBy = pl.ReplacedBy,
                                   DuplicateFrom = pl.DuplicateFrom,
                                   IsIncomingBasicSchedule = pl.IsIncomingBasicSchedule,
                                   IsOutGoingBasicSchedule = pl.IsOutGoingBasicSchedule,
                                   PayorId = pl.Payor == null ? Guid.Empty : pl.Payor.PayorId,
                                   PayorName = pl.Payor == null ? string.Empty : pl.Payor.PayorName,
                                   PayorNickName = pl.Payor == null ? string.Empty : pl.Payor.NickName,
                                   SplitPercentage = pl.SplitPercentage,
                                   IncomingPaymentTypeId = pl.IncomingPaymentTypeId,
                                   PolicyIncomingPayType = pl.MasterIncomingPaymentType.Name,
                                   CreatedOn = pl.CreatedOn,
                                   RowVersion = pl.RowVersion,
                                   CreatedBy = pl.CreatedBy.Value,//--always check it will never null                                   
                                   IsSavedPolicy = true,
                                   CompType = pl.PolicyLearnedField == null ? null : pl.PolicyLearnedField.CompTypeID == null ? null : pl.PolicyLearnedField.CompTypeID,
                                   CompSchuduleType = pl.PolicyLearnedField == null ? string.Empty : pl.PolicyLearnedField.CompScheduleType == null ? string.Empty : pl.PolicyLearnedField.CompScheduleType,
                                   LastFollowUpRuns = pl.LastFollowUpRuns,

                               }).ToList();
                }
                catch
                {
                }

                endList = new List<PolicyDetailsData>(endList.Where(p => p.PolicyNumber != string.Empty)).ToList();
                endList = new List<PolicyDetailsData>(endList.Where(p => p.ClientId != Guid.Empty)).ToList();

               // endList.ForEach(p => p.PolicyPreviousData = FillPolicyDetailPreviousData(p));       
         
                return endList;
            }

        }

        public static List<PolicyDetailsData> GetAllPolicyForFollowupservice()
        {
            List<PolicyDetailsData> PolicyList = new List<PolicyDetailsData>();

            DLinq.CommissionDepartmentEntities ctx = new DLinq.CommissionDepartmentEntities(); //create your entity object here
            EntityConnection ec = (EntityConnection)ctx.Connection;
            SqlConnection sc = (SqlConnection)ec.StoreConnection; //get the SQLConnection that your entity object would use
            string adoConnStr = sc.ConnectionString;

            using (SqlConnection con = new SqlConnection(adoConnStr))
            {
                using (SqlCommand cmd = new SqlCommand("Usp_GetFollowupServicePolicy", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;

                    con.Open();

                    SqlDataReader reader = cmd.ExecuteReader();
                    // Call Read before accessing data. 
                    //Policies.PolicyId,Policies.PolicyNumber,Policies.LastFollowUpRuns,Policies.PolicyLicenseeId
                    while (reader.Read())
                    {
                        PolicyDetailsData objPolicyList = new PolicyDetailsData();
                        try
                        {
                            if (!string.IsNullOrEmpty(Convert.ToString(reader["PolicyId"])))
                            {
                                objPolicyList.PolicyId = (Guid)(reader["PolicyId"]);
                            }
                        }
                        catch
                        {
                        }

                        //try
                        //{
                        //    if (!string.IsNullOrEmpty(Convert.ToString(reader["PolicyNumber"])))
                        //    {
                                objPolicyList.PolicyNumber = Convert.ToString(reader["PolicyNumber"]);
                        //    }
                        //}
                        //catch
                        //{
                        //}

                        try
                        {
                            if (!string.IsNullOrEmpty(Convert.ToString(reader["LastFollowUpRuns"])))
                            {
                                objPolicyList.LastFollowUpRuns = Convert.ToDateTime(reader["LastFollowUpRuns"]);
                            }
                        }
                        catch
                        {
                        }

                        try
                        {
                            if (!string.IsNullOrEmpty(Convert.ToString(reader["PolicyLicenseeId"])))
                            {
                                objPolicyList.PolicyLicenseeId = (Guid)(reader["PolicyLicenseeId"]);
                            }
                        }
                        catch
                        {
                        }

                        try
                        {
                            if (!string.IsNullOrEmpty(Convert.ToString(reader["IsTrackPayment"])))
                            {
                                objPolicyList.IsTrackPayment = (bool)(reader["IsTrackPayment"]);
                            }
                        }
                        catch
                        {
                        }

                        PolicyList.Add(objPolicyList);
                    }
                }
            }

            return PolicyList;


        }

        public static DateTime? GetPolicyTrackDate(Guid PolicyID)
        {

            DateTime? dtTime = null;
            DLinq.CommissionDepartmentEntities ctx = new DLinq.CommissionDepartmentEntities(); //create your entity object here
            EntityConnection ec = (EntityConnection)ctx.Connection;
            SqlConnection sc = (SqlConnection)ec.StoreConnection; //get the SQLConnection that your entity object would use
            string adoConnStr = sc.ConnectionString;

            using (SqlConnection con = new SqlConnection(adoConnStr))
            {
                using (SqlCommand cmd = new SqlCommand("usp_GetPolicyTrackFromDate", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@PolicyId", PolicyID);
                    con.Open();

                    SqlDataReader reader = cmd.ExecuteReader();
                    // Call Read before accessing data. 
                    while (reader.Read())
                    {
                        try
                        {
                            if (!string.IsNullOrEmpty(Convert.ToString(reader["TrackFromDate"])))
                            {
                                dtTime = Convert.ToDateTime(reader["TrackFromDate"]);
                            }
                        }
                        catch
                        {
                        }
                    }
                }
            }
            return dtTime;
            
        }
       
        public static List<PolicyDetailsData> GetPolicyData(Dictionary<string, object> Parameters, Expression<Func<DLinq.Policy, bool>> ParameterExpression = null)
        {
            Expression<Func<DLinq.Policy, bool>> parametersFromHelperClass = HelperClass.getWhereClause<DLinq.Policy>(Parameters);
            if (ParameterExpression != null)
            {
                parametersFromHelperClass = parametersFromHelperClass.And(ParameterExpression);
            }
            return FillinAllData(parametersFromHelperClass, ParameterExpression);
        }

        //Acme - New method to fetch policy details
        public static PolicyDetailsData FillPolicyDataOnPolicyID(Guid PolicyId)
        {
            PolicyDetailsData data = null;
            List<PolicyDetailsData> endList = new List<PolicyDetailsData>();
            try
            {
                DLinq.CommissionDepartmentEntities ctx = new DLinq.CommissionDepartmentEntities(); //create your entity object here
                EntityConnection ec = (EntityConnection)ctx.Connection;
                SqlConnection sc = (SqlConnection)ec.StoreConnection; //get the SQLConnection that your entity object would use
                string adoConnStr = sc.ConnectionString;

                using (SqlConnection con = sc)
                {
                    con.Open();
                    SqlCommand scm = new SqlCommand();
                    scm.Connection = con;
                    scm.CommandText = "USP_GetPolicyAllDetails";
                    scm.Parameters.AddWithValue("@PolicyID", PolicyId);
                    scm.CommandType = CommandType.StoredProcedure;
                    SqlDataReader dr = scm.ExecuteReader();
                    while (dr.Read())
                    {
                        PolicyDetailsData obj = new PolicyDetailsData();
                                     obj.PolicyId =  (Guid) (dr["PolicyId"]);
                                    /*  PolicyNumber = pl.PolicyNumber == null ? string.Empty : pl.PolicyNumber;
                                      PolicyStatusId = pl.MasterPolicyStatu.PolicyStatusId;
                                      PolicyStatusName = pl.MasterPolicyStatu.Name;
                                      PolicyType = pl.PolicyType == null ? string.Empty : pl.PolicyType;
                                      PolicyLicenseeId = pl.Licensee.LicenseeId;
                                      Insured = pl.Insured == null ? string.Empty : pl.Insured;
                                      OriginalEffectiveDate = pl.OriginalEffectiveDate;
                                      TrackFromDate = pl.TrackFromDate;
                                      PolicyModeId = pl.MasterPolicyMode.PolicyModeId;
                                      ModeAvgPremium = pl.MonthlyPremium;
                                      SubmittedThrough = pl.SubmittedThrough == null ? string.Empty : pl.SubmittedThrough;
                                      Enrolled = pl.Enrolled == null ? string.Empty : pl.Enrolled;
                                      Eligible = pl.Eligible == null ? string.Empty : pl.Eligible;
                                      PolicyTerminationDate = pl.PolicyTerminationDate;
                                      TerminationReasonId = pl.TerminationReasonId;
                                      IsTrackMissingMonth = pl.IsTrackMissingMonth;
                                      IsTrackIncomingPercentage = pl.IsTrackIncomingPercentage;
                                      IsTrackPayment = pl.IsTrackPayment;
                                      IsDeleted = pl.IsDeleted;
                                      CarrierID = pl.Carrier == null ? Guid.Empty : pl.Carrier.CarrierId;
                                      CarrierName = pl.Carrier == null ? string.Empty : pl.Carrier.CarrierName;
                                      CoverageId = pl.Coverage == null ? Guid.Empty : pl.Coverage.CoverageId;
                                      CoverageName = pl.Coverage == null ? string.Empty : pl.Coverage.ProductName;

                                      ClientId = pl.Client == null ? Guid.Empty : pl.Client.ClientId;
                                      ClientName = pl.Client == null ? string.Empty : pl.Client.Name;

                                      ReplacedBy = pl.ReplacedBy;
                                      DuplicateFrom = pl.DuplicateFrom;
                                      IsIncomingBasicSchedule = pl.IsIncomingBasicSchedule;
                                      IsOutGoingBasicSchedule = pl.IsOutGoingBasicSchedule;
                                      PayorId = pl.Payor == null ? Guid.Empty : pl.Payor.PayorId;
                                      PayorName = pl.Payor == null ? string.Empty : pl.Payor.PayorName;
                                      PayorNickName = pl.Payor == null ? string.Empty : pl.Payor.NickName;
                                      SplitPercentage = pl.SplitPercentage == null ? 0.0 : pl.SplitPercentage;
                                      IncomingPaymentTypeId = pl.IncomingPaymentTypeId;
                                      PolicyIncomingPayType = pl.MasterIncomingPaymentType == null ? string.Empty : pl.MasterIncomingPaymentType.Name;
                                      CreatedOn = pl.CreatedOn;
                                      RowVersion = pl.RowVersion;
                                      CreatedBy = pl.CreatedBy.Value;
                                      IsSavedPolicy = true;
                                      CompType = pl.PolicyLearnedField == null ? null : pl.PolicyLearnedField.CompTypeID == null ? null : pl.PolicyLearnedField.CompTypeID;
                                      CompSchuduleType = pl.PolicyLearnedField == null ? string.Empty : pl.PolicyLearnedField.CompScheduleType == null ? string.Empty : pl.PolicyLearnedField.CompScheduleType;
                                      LastFollowUpRuns = pl.LastFollowUpRuns;
                                      Advance = pl.Advance == null ? null : pl.Advance;
                                      ProductType = pl.ProductType;
                                       //added
                                       AccoutExec = pl.AccoutExec;*/
                        endList.Add(obj);
                    }
                }
                if (endList != null)
                {
                    data = endList.FirstOrDefault();
                }
            }
            catch (Exception ex)
            {
              //  throw new Exception("Exception in getting reasons. ", new Exception("FAIL"));
            }
            return data;
        }


        private static List<PolicyDetailsData> FillinAllData(Expression<Func<DLinq.Policy, bool>> parameters, Expression<Func<DLinq.Policy, bool>> ParameterExpression = null)
        {
            List<DLinq.Policy> policies;
            List<PolicyDetailsData> endList = new List<PolicyDetailsData>();
            using (DLinq.CommissionDepartmentEntities DataModel = Entity.DataModel)
            {

                DataModel.CommandTimeout = 600000000;

                policies = (from pl in DataModel.Policies.Where(parameters) select pl).ToList();

                if (policies == null)
                {
                    return endList;
                }

                endList = (from pl in policies

                           select new PolicyDetailsData
                           {
                               PolicyId = pl.PolicyId,
                               PolicyNumber = pl.PolicyNumber == null ? string.Empty : pl.PolicyNumber,
                               PolicyStatusId = pl.MasterPolicyStatu.PolicyStatusId,
                               PolicyStatusName = pl.MasterPolicyStatu.Name,
                               PolicyType = pl.PolicyType == null ? string.Empty : pl.PolicyType,
                               PolicyLicenseeId = pl.Licensee.LicenseeId,
                               Insured = pl.Insured == null ? string.Empty : pl.Insured,
                               OriginalEffectiveDate = pl.OriginalEffectiveDate,
                               TrackFromDate = pl.TrackFromDate,
                               PolicyModeId = pl.MasterPolicyMode.PolicyModeId,
                               ModeAvgPremium = pl.MonthlyPremium,
                               SubmittedThrough = pl.SubmittedThrough == null ? string.Empty : pl.SubmittedThrough,
                               Enrolled = pl.Enrolled == null ? string.Empty : pl.Enrolled,
                               Eligible = pl.Eligible == null ? string.Empty : pl.Eligible,
                               PolicyTerminationDate = pl.PolicyTerminationDate,
                               TerminationReasonId = pl.TerminationReasonId,
                               IsTrackMissingMonth = pl.IsTrackMissingMonth,
                               IsTrackIncomingPercentage = pl.IsTrackIncomingPercentage,
                               IsTrackPayment = pl.IsTrackPayment,
                               IsDeleted = pl.IsDeleted,
                               CarrierID = pl.Carrier == null ? Guid.Empty : pl.Carrier.CarrierId,
                               CarrierName = pl.Carrier == null ? string.Empty : pl.Carrier.CarrierName,
                               CoverageId = pl.Coverage == null ? Guid.Empty : pl.Coverage.CoverageId,
                               CoverageName = pl.Coverage == null ? string.Empty : pl.Coverage.ProductName,

                               ClientId = pl.Client == null ? Guid.Empty : pl.Client.ClientId,
                               ClientName = pl.Client == null ? string.Empty : pl.Client.Name,

                               ReplacedBy = pl.ReplacedBy,
                               DuplicateFrom = pl.DuplicateFrom,
                               IsIncomingBasicSchedule = pl.IsIncomingBasicSchedule,
                               IsOutGoingBasicSchedule = pl.IsOutGoingBasicSchedule,
                               PayorId = pl.Payor == null ? Guid.Empty : pl.Payor.PayorId,
                               PayorName = pl.Payor == null ? string.Empty : pl.Payor.PayorName,
                               PayorNickName = pl.Payor == null ? string.Empty : pl.Payor.NickName,
                               SplitPercentage = pl.SplitPercentage == null ? 0.0 : pl.SplitPercentage,
                               IncomingPaymentTypeId = pl.IncomingPaymentTypeId,
                               PolicyIncomingPayType = pl.MasterIncomingPaymentType == null ? string.Empty : pl.MasterIncomingPaymentType.Name,
                               CreatedOn = pl.CreatedOn,
                               RowVersion = pl.RowVersion,
                               CreatedBy = pl.CreatedBy.Value,
                               IsSavedPolicy = true,
                              // CompType = pl.PolicyLearnedField == null ? null : pl.PolicyLearnedField.CompTypeID == null ? null : pl.PolicyLearnedField.CompTypeID,
                            //   CompSchuduleType = pl.PolicyLearnedField == null ? string.Empty : pl.PolicyLearnedField.CompScheduleType == null ? string.Empty : pl.PolicyLearnedField.CompScheduleType,
                               LastFollowUpRuns = pl.LastFollowUpRuns,
                               Advance = pl.Advance == null ? null : pl.Advance,
                               ProductType = pl.ProductType,
                               //added
                               AccoutExec = pl.AccoutExec,
                               //Acme
                               LastNoMissIssueDate = pl.LastNoMissIssueDate,
                               LastNoVarIssueDate = pl.LastNoVarIssueDate

                           }).ToList();


            }

            //endList.ForEach(p => p.PolicyPreviousData = FillPolicyDetailPreviousData(p));

            endList = new List<PolicyDetailsData>(endList.Where(p => p.ClientId != Guid.Empty)).ToList();
            endList = new List<PolicyDetailsData>(endList.Where(p => p.PolicyNumber != string.Empty)).ToList();

            return endList.OrderBy(p => p.PolicyNumber).ToList();

        }

        public static List<PolicyDetailsData> GetPolicyClientWise(Guid LicenseeId, Guid ClientId)
        {
            List<PolicyDetailsData> lstPolicyDetailsData = new List<PolicyDetailsData>();

            DLinq.CommissionDepartmentEntities ctx = new DLinq.CommissionDepartmentEntities(); //create your entity object here
            EntityConnection ec = (EntityConnection)ctx.Connection;
            SqlConnection sc = (SqlConnection)ec.StoreConnection; //get the SQLConnection that your entity object would use
            string adoConnStr = sc.ConnectionString;

            DateTime? nullDateTime = null;
            int? nullint = null;
            bool? nullBool = null;
            Guid? nullGuid = null;
            decimal? Nulldecimal = null;

            using (SqlConnection con = new SqlConnection(adoConnStr))
            {
                using (SqlCommand cmd = new SqlCommand("Usp_GetPolicyClientWise", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@LicenseeId", LicenseeId);
                    cmd.Parameters.AddWithValue("@PolicyClientId", ClientId);
                    con.Open();

                    SqlDataReader reader = cmd.ExecuteReader();

                    while (reader.Read())
                    {

                        PolicyDetailsData objPolicyDetailsData = new PolicyDetailsData();

                        if (!string.IsNullOrEmpty(Convert.ToString(reader["PolicyId"])))
                        {
                            objPolicyDetailsData.PolicyId = reader["PolicyId"] == null ? Guid.Empty : (Guid)reader["PolicyId"];
                        }

                        if (!string.IsNullOrEmpty(Convert.ToString(reader["PolicyNumber"])))
                        {
                            objPolicyDetailsData.PolicyNumber = reader["PolicyNumber"] == null ? string.Empty : Convert.ToString(reader["PolicyNumber"]);
                        }

                        if (!string.IsNullOrEmpty(Convert.ToString(reader["PolicyStatusId"])))
                        {
                            objPolicyDetailsData.PolicyStatusId = reader["PolicyStatusId"] == null ? nullint : Convert.ToInt32(reader["PolicyStatusId"]);
                        }

                        if (!string.IsNullOrEmpty(Convert.ToString(reader["PolicyStatusName"])))
                        {
                            objPolicyDetailsData.PolicyStatusName = reader["PolicyStatusName"] == null ? string.Empty : Convert.ToString(reader["PolicyStatusName"]);
                        }

                        if (!string.IsNullOrEmpty(Convert.ToString(reader["PolicyType"])))
                        {
                            objPolicyDetailsData.PolicyType = reader["PolicyType"] == null ? string.Empty : Convert.ToString(reader["PolicyType"]);
                        }

                        if (!string.IsNullOrEmpty(Convert.ToString(reader["LicenseeId"])))
                        {
                            objPolicyDetailsData.PolicyLicenseeId = reader["LicenseeId"] == null ? nullGuid : (Guid)reader["LicenseeId"];
                        }

                        if (!string.IsNullOrEmpty(Convert.ToString(reader["Insured"])))
                        {
                            objPolicyDetailsData.Insured = reader["Insured"] == null ? string.Empty : Convert.ToString(reader["Insured"]);
                        }

                        if (!string.IsNullOrEmpty(Convert.ToString(reader["OriginalEffectiveDate"])))
                        {
                            objPolicyDetailsData.OriginalEffectiveDate = reader["OriginalEffectiveDate"] == null ? nullDateTime : Convert.ToDateTime(reader["OriginalEffectiveDate"]);
                        }

                        if (!string.IsNullOrEmpty(Convert.ToString(reader["TrackFromDate"])))
                        {
                            objPolicyDetailsData.TrackFromDate = reader["TrackFromDate"] == null ? nullDateTime : Convert.ToDateTime(reader["TrackFromDate"]);
                        }

                        if (!string.IsNullOrEmpty(Convert.ToString(reader["PolicyModeId"])))
                        {
                            objPolicyDetailsData.PolicyModeId = reader["PolicyModeId"] == null ? nullint : Convert.ToInt32(reader["PolicyModeId"]);
                        }

                        if (!string.IsNullOrEmpty(Convert.ToString(reader["MonthlyPremium"])))
                        {
                            objPolicyDetailsData.ModeAvgPremium = reader["MonthlyPremium"] == null ? Nulldecimal : Convert.ToDecimal(reader["MonthlyPremium"]);
                        }

                        if (!string.IsNullOrEmpty(Convert.ToString(reader["SubmittedThrough"])))
                        {
                            objPolicyDetailsData.SubmittedThrough = reader["SubmittedThrough"] == null ? string.Empty : Convert.ToString(reader["SubmittedThrough"]);
                        }

                        if (!string.IsNullOrEmpty(Convert.ToString(reader["Enrolled"])))
                        {
                            objPolicyDetailsData.Enrolled = reader["Enrolled"] == null ? string.Empty : Convert.ToString(reader["Enrolled"]);
                        }

                        if (!string.IsNullOrEmpty(Convert.ToString(reader["Eligible"])))
                        {
                            objPolicyDetailsData.Eligible = reader["Eligible"] == null ? string.Empty : Convert.ToString(reader["Eligible"]);
                        }

                        if (!string.IsNullOrEmpty(Convert.ToString(reader["PolicyTerminationDate"])))
                        {
                            objPolicyDetailsData.PolicyTerminationDate = reader["PolicyTerminationDate"] == null ? nullDateTime : Convert.ToDateTime(reader["PolicyTerminationDate"]);
                        }

                        if (!string.IsNullOrEmpty(Convert.ToString(reader["TerminationReasonId"])))
                        {
                            objPolicyDetailsData.TerminationReasonId = reader["TerminationReasonId"] == null ? nullint : Convert.ToInt32(reader["TerminationReasonId"]);
                        }

                        if (!string.IsNullOrEmpty(Convert.ToString(reader["IsTrackMissingMonth"])))
                        {
                            if (reader["IsTrackMissingMonth"] != null)
                            {
                                objPolicyDetailsData.IsTrackMissingMonth = Convert.ToBoolean(reader["IsTrackMissingMonth"]);
                            }
                        }

                        if (!string.IsNullOrEmpty(Convert.ToString(reader["IsTrackIncomingPercentage"])))
                        {
                            if (reader["IsTrackIncomingPercentage"] != null)
                            {
                                objPolicyDetailsData.IsTrackIncomingPercentage = Convert.ToBoolean(reader["IsTrackIncomingPercentage"]);
                            }
                        }

                        if (!string.IsNullOrEmpty(Convert.ToString(reader["IsTrackPayment"])))
                        {
                            if (reader["IsTrackPayment"] != null)
                            {
                                objPolicyDetailsData.IsTrackPayment = Convert.ToBoolean(reader["IsTrackPayment"]);
                            }
                        }

                        if (!string.IsNullOrEmpty(Convert.ToString(reader["IsDeleted"])))
                        {
                            if (reader["IsDeleted"] != null)
                            {
                                objPolicyDetailsData.IsDeleted = Convert.ToBoolean(reader["IsDeleted"]);
                            }
                        }

                        if (!string.IsNullOrEmpty(Convert.ToString(reader["CarrierID"])))
                        {
                            objPolicyDetailsData.CarrierID = reader["CarrierID"] == null ? nullGuid : (Guid)(reader["CarrierID"]);
                        }

                        if (!string.IsNullOrEmpty(Convert.ToString(reader["CarrierName"])))
                        {
                            objPolicyDetailsData.CarrierName = reader["CarrierName"] == null ? string.Empty : Convert.ToString(reader["CarrierName"]);
                        }

                        if (!string.IsNullOrEmpty(Convert.ToString(reader["CoverageId"])))
                        {
                            objPolicyDetailsData.CoverageId = reader["CoverageId"] == null ? nullGuid : (Guid)(reader["CoverageId"]);
                        }

                        if (!string.IsNullOrEmpty(Convert.ToString(reader["ProductName"])))
                        {
                            objPolicyDetailsData.CoverageName = reader["ProductName"] == null ? string.Empty : Convert.ToString(reader["ProductName"]);
                        }

                        if (!string.IsNullOrEmpty(Convert.ToString(reader["ClientId"])))
                        {
                            objPolicyDetailsData.ClientId = reader["ClientId"] == null ? nullGuid : (Guid)(reader["ClientId"]);
                        }

                        if (!string.IsNullOrEmpty(Convert.ToString(reader["ClientsName"])))
                        {
                            objPolicyDetailsData.ClientName = reader["ClientsName"] == null ? string.Empty : Convert.ToString(reader["ClientsName"]);
                        }

                        if (!string.IsNullOrEmpty(Convert.ToString(reader["ReplacedBy"])))
                        {
                            objPolicyDetailsData.ReplacedBy = reader["ReplacedBy"] == null ? nullGuid : (Guid)(reader["ReplacedBy"]);
                        }

                        if (!string.IsNullOrEmpty(Convert.ToString(reader["DuplicateFrom"])))
                        {
                            objPolicyDetailsData.DuplicateFrom = reader["DuplicateFrom"] == null ? nullGuid : (Guid)(reader["DuplicateFrom"]);
                        }

                        if (!string.IsNullOrEmpty(Convert.ToString(reader["IsIncomingBasicSchedule"])))
                        {
                            objPolicyDetailsData.IsIncomingBasicSchedule = reader["IsIncomingBasicSchedule"] == null ? nullBool : Convert.ToBoolean(reader["IsIncomingBasicSchedule"]);
                        }

                        if (!string.IsNullOrEmpty(Convert.ToString(reader["IsOutGoingBasicSchedule"])))
                        {
                            objPolicyDetailsData.IsOutGoingBasicSchedule = reader["IsOutGoingBasicSchedule"] == null ? nullBool : Convert.ToBoolean(reader["IsOutGoingBasicSchedule"]);
                        }

                        if (!string.IsNullOrEmpty(Convert.ToString(reader["CarrierID"])))
                        {
                            objPolicyDetailsData.CarrierID = reader["CarrierID"] == null ? nullGuid : (Guid)(reader["CarrierID"]);
                        }

                        if (!string.IsNullOrEmpty(Convert.ToString(reader["PayorID"])))
                        {
                            objPolicyDetailsData.PayorId = reader["PayorID"] == null ? nullGuid : (Guid)(reader["PayorID"]);
                        }

                        if (!string.IsNullOrEmpty(Convert.ToString(reader["PayorName"])))
                        {
                            objPolicyDetailsData.PayorName = reader["PayorName"] == null ? string.Empty : Convert.ToString(reader["PayorName"]);
                        }

                        if (!string.IsNullOrEmpty(Convert.ToString(reader["SplitPercentage"])))
                        {
                            objPolicyDetailsData.PayorNickName = reader["PayorNickName"] == null ? string.Empty : Convert.ToString(reader["PayorNickName"]);
                        }

                        if (!string.IsNullOrEmpty(Convert.ToString(reader["SplitPercentage"])))
                        {
                            objPolicyDetailsData.SplitPercentage = reader["SplitPercentage"] == null ? 0 : Convert.ToDouble(reader["SplitPercentage"]);
                        }

                        if (!string.IsNullOrEmpty(Convert.ToString(reader["IncomingPaymentTypeId"])))
                        {
                            objPolicyDetailsData.IncomingPaymentTypeId = reader["IncomingPaymentTypeId"] == null ? nullint : Convert.ToInt32(reader["IncomingPaymentTypeId"]);
                        }

                        if (!string.IsNullOrEmpty(Convert.ToString(reader["Name"])))
                        {
                            objPolicyDetailsData.PolicyIncomingPayType = reader["Name"] == null ? string.Empty : Convert.ToString(reader["Name"]);
                        }

                        if (!string.IsNullOrEmpty(Convert.ToString(reader["CreatedOn"])))
                        {
                            objPolicyDetailsData.CreatedOn = reader["CreatedOn"] == null ? nullDateTime : Convert.ToDateTime(reader["CreatedOn"]);
                        }

                        if (!string.IsNullOrEmpty(Convert.ToString(reader["RowVersion"])))
                        {
                            objPolicyDetailsData.RowVersion = (Byte[])(reader["RowVersion"]);
                        }

                        if (!string.IsNullOrEmpty(Convert.ToString(reader["CreatedBy"])))
                        {
                            objPolicyDetailsData.CreatedBy = (Guid)(reader["CreatedBy"]);
                        }


                        objPolicyDetailsData.IsSavedPolicy = true;


                        if (!string.IsNullOrEmpty(Convert.ToString(reader["CompTypeID"])))
                        {
                            objPolicyDetailsData.CompSchuduleType = reader["CompTypeID"] == null ? string.Empty : Convert.ToString(reader["CompTypeID"]);
                        }

                        if (!string.IsNullOrEmpty(Convert.ToString(reader["CompScheduleType"])))
                        {
                            objPolicyDetailsData.CompSchuduleType = reader["CompScheduleType"] == null ? string.Empty : Convert.ToString(reader["CompScheduleType"]);
                        }

                        if (!string.IsNullOrEmpty(Convert.ToString(reader["LastFollowUpRuns"])))
                        {
                            objPolicyDetailsData.LastFollowUpRuns = reader["LastFollowUpRuns"] == null ? nullDateTime : Convert.ToDateTime(reader["LastFollowUpRuns"]);
                        }

                        if (!string.IsNullOrEmpty(Convert.ToString(reader["Advance"])))
                        {
                            objPolicyDetailsData.Advance = reader["Advance"] == null ? nullint : Convert.ToInt32(reader["Advance"]);
                        }

                        //Acme commented as don;t exist in result set from SP
                        
                        //if (!string.IsNullOrEmpty(Convert.ToString(reader["AccoutExec"])))
                        //{
                        //    objPolicyDetailsData.AccoutExec = reader["AccoutExec"] == null ? null : Convert.ToString(reader["AccoutExec"]);
                        //}

                        //if (!string.IsNullOrEmpty(Convert.ToString(reader["UserCredentialId"])))
                        //{
                        //    objPolicyDetailsData.UserCredentialId = reader["UserCredentialId"] == null ? Guid.Empty : (Guid)(reader["UserCredentialId"]);
                        //}


                        lstPolicyDetailsData.Add(objPolicyDetailsData);

                    }
                    reader.Close();
                }
            }
            return lstPolicyDetailsData;
        }
        #endregion

        public PolicyDetailsData GetPolicyStting(Guid policyID)
        {

            using (DLinq.CommissionDepartmentEntities DataModel = Entity.DataModel)
            {
                PolicyDetailsData _Policy = (from pl in DataModel.Policies
                                             where (pl.IsDeleted == false) && (pl.PolicyId == policyID)
                                             select new PolicyDetailsData
                                          {
                                              PolicyId = pl.PolicyId,                                              
                                              IsTrackMissingMonth = pl.IsTrackMissingMonth,
                                              IsTrackIncomingPercentage = pl.IsTrackIncomingPercentage,
                                          }).FirstOrDefault();

                return _Policy;
            }
        }
    
        #region Loading Releations for Policy
        private static void MapPolicyIncomingPayment(List<DLinq.PolicyPaymentEntry> Source, PolicyDetailsData Target)
        {
            List<PolicyPaymentEntriesPost> payments = (from u in Source
                                                       where u.PolicyId == Target.PolicyId
                                                       select new PolicyPaymentEntriesPost
                                                                    {
                                                                        PaymentEntryID = u.PaymentEntryId,
                                                                        StmtID = u.StatementId.Value,
                                                                        PolicyID = u.PolicyId.Value,
                                                                        // IssueID = u.IssueID.Value,
                                                                        InvoiceDate = u.InvoiceDate,
                                                                        PaymentRecived = u.PaymentRecived ?? 0,
                                                                        CommissionPercentage = u.CommissionPercentage ?? 0,
                                                                        NumberOfUnits = u.NumberOfUnits ?? 0,
                                                                        DollerPerUnit = u.DollerPerUnit ?? 0,
                                                                        Fee = u.Fee.Value,
                                                                        SplitPer = u.SplitPercentage ?? 0,
                                                                        TotalPayment = u.TotalPayment ?? 0,
                                                                        CreatedOn = u.CreatedOn.Value,
                                                                        CreatedBy = u.CreatedBy ?? Guid.Empty,
                                                                        PostStatusID = u.PostStatusID,
                                                                        DEUEntryId = u.DEUEntryId ?? Guid.Empty,
                                                                        StmtNumber = u.Statement.StatementNumber,
                                                                        FollowUpVarIssueId = u.FollowUpVarIssueId,
                                                                    }).ToList();

            Target.policyPaymentEntries = payments;
        }

        private static void MapPolicyOutGoingPayments(List<DLinq.PolicyOutgoingPayment> Source, PolicyPaymentEntriesPost Target)
        {
            List<PolicyOutgoingDistribution> policyIncomingSchedule = (from u in Source
                                                                       where u.PaymentEntryId == Target.PaymentEntryID
                                                                       select new PolicyOutgoingDistribution
                                                                       {
                                                                           PaymentEntryId = u.PaymentEntryId,
                                                                           CreatedOn = u.CreatedOn,
                                                                           IsPaid = u.IsPaid,
                                                                           OutgoingPaymentId = u.OutgoingPaymentId,
                                                                           OutGoingPerUnit = u.OutgoingPerUnit,
                                                                           RecipientUserCredentialId = u.RecipientUserCredentialId,
                                                                           PaidAmount = u.PaidAmount,
                                                                           Payment = u.Payment,
                                                                           Premium = u.Premium
                                                                       }).ToList();

        }

        private static void MapLearnedFields(List<PolicyDetailsData> policies, List<DLinq.Policy> Source)
        {
            var policyLearnedFields = (from plearned in Source
                                       select new PolicyLearnedFieldData
                                       {
                                           PolicyId = plearned.PolicyLearnedField.PolicyId,
                                           Insured = plearned.PolicyLearnedField.Insured,
                                           PolicyNumber = plearned.PolicyLearnedField.PolicyNumber,
                                           Effective = plearned.PolicyLearnedField.Effective,
                                           TrackFrom = plearned.PolicyLearnedField.TrackFrom,
                                           Renewal = plearned.PolicyLearnedField.Renewal,
                                           CarrierId = plearned.PolicyLearnedField.CarrierId,
                                           CoverageId = plearned.PolicyLearnedField.CoverageId,
                                           PAC =plearned.PolicyLearnedField.PAC,
                                           PMC =plearned.PolicyLearnedField.PMC,
                                           ModalAvgPremium = plearned.PolicyLearnedField.ModalAvgPremium,
                                           PolicyModeId = plearned.PolicyLearnedField.PolicyModeId,
                                           Enrolled = plearned.PolicyLearnedField.Enrolled,
                                           Eligible = plearned.PolicyLearnedField.Eligible,
                                           AutoTerminationDate = plearned.PolicyLearnedField.AutoTerminationDate,
                                           Link1 = plearned.PolicyLearnedField.Link1,
                                           PayorSysId = plearned.PolicyLearnedField.PayorSysID,
                                           LastModifiedOn = plearned.PolicyLearnedField.LastModifiedOn,
                                           LastModifiedUserCredentialId = plearned.PolicyLearnedField.LastModifiedUserCredentialid,
                                           CompTypeId = plearned.PolicyLearnedField.CompTypeID,
                                           CompScheduleType = plearned.PolicyLearnedField.CompScheduleType,
                                           PayorId = plearned.PolicyLearnedField.PayorId,
                                           PreviousEffectiveDate = plearned.PolicyLearnedField.PreviousEffectiveDate,
                                           PreviousPolicyModeid = plearned.PolicyLearnedField.PreviousPolicyModeId
                                       }).ToList();
            policyLearnedFields.ForEach(plf => plf.CoverageNickName = Coverage.GetCoverageNickName(plf.PayorId ?? Guid.Empty, plf.CarrierId ?? Guid.Empty, plf.CoverageId ?? Guid.Empty));
            policyLearnedFields.ForEach(plf => plf.CarrierNickName = Carrier.GetCarrierNickName(plf.PayorId ?? Guid.Empty, plf.CarrierId ?? Guid.Empty));
            policies.ForEach(p => p.LearnedFields = policyLearnedFields.Where(pl => pl.PolicyId == p.PolicyId).FirstOrDefault());
        }
        #endregion

        public static decimal GetPMC(Guid policyID)
        {
            return PostUtill.CalculatePMC(policyID);
        }

        public static decimal GetPAC(Guid policyID)
        {
            return PostUtill.CalculatePAC(policyID);
        }
        
        #region Batch and Others
        public static Batch GenerateBatch(PolicyDetailsData _policy)
        {
            bool IsBatchPaid = false;
            Batch _batch = null;
            Batch objBatch = new Batch();
            //List<Batch> tempBatchlst = Batch.GetBatchList(UploadStatus.Automatic);
            List<Batch> tempBatchlst = objBatch.GetBatchList(UploadStatus.Automatic);

            tempBatchlst = tempBatchlst == null ? null : tempBatchlst
                   .Where(p => p.PayorId == _policy.PayorId).Where(p => p.LicenseeId == _policy.PolicyLicenseeId).ToList();

            foreach (Batch bth in tempBatchlst)
            {               
                IsBatchPaid = objBatch.GetBatchPaidStatus(bth.BatchId);
                if (!IsBatchPaid)
                {
                    _batch = bth;
                    break;
                }
            }

            if (_batch == null)
            {
                _batch = new Batch();
                _batch.BatchId = Guid.NewGuid();
                _batch.LicenseeId = _policy.PolicyLicenseeId.Value;
                _batch.PayorId = _policy.PayorId;
                _batch.CreatedDate = DateTime.Today;
                _batch.FileType = null;
                _batch.UploadStatus = UploadStatus.Automatic;
                _batch.EntryStatus = EntryStatus.BatchCompleted;
                _batch.AssignedDeuUserName = null;
                _batch.CreatedFromUpload = null;
                _batch.FileName = null;
                _batch.LastModifiedDate = DateTime.Today;
                _batch.IsManuallyUploaded = null;
                _batch.SiteId = null;

                _batch.AddUpdate();
            }
            else
            {
                _batch.LastModifiedDate = DateTime.Today;
                _batch.AddUpdate();
            }
            return _batch;
        }

        public static Statement GenerateStatment(Guid BatchId, Guid PayorId, decimal PaymentRecived, Guid CreatedBy)
        {
            Statement _Statement = null;
            //List<Statement> StmtLst = Statement.GetStatementList(BatchId);
            //Added statement object
            Statement objStatement = new Statement();
            List<Statement> StmtLst = objStatement.GetStatementList(BatchId);
            
            if (StmtLst != null)
                _Statement = StmtLst.Where(p => p.PayorId == PayorId).FirstOrDefault();

            if (_Statement == null)
            {
                _Statement = new Statement();
                _Statement.StatementID = Guid.NewGuid();
                _Statement.BatchId = BatchId;
                _Statement.PayorId = PayorId;
                _Statement.CheckAmount = PaymentRecived;
                _Statement.StatusId = 2;
                _Statement.Entries = 1;
                _Statement.CreatedDate = DateTime.Now;
                _Statement.LastModified = DateTime.Now;
                _Statement.CreatedBy = CreatedBy;
                _Statement.BalanceForOrAdjustment = null;
                _Statement.EnteredAmount = PaymentRecived;
                _Statement.StatementDate = null;
                _Statement.AddUpdate();
            }
            else
            {
                _Statement.StatusId = 1;
                _Statement.CheckAmount += PaymentRecived;
                _Statement.EnteredAmount += PaymentRecived;
                _Statement.Entries += 1;
                _Statement.LastModified = DateTime.Now;
                _Statement.AddUpdate();
            }
            return _Statement;
        }

        public static void AddUpdatePolicyHistory(Guid PolicyId)
        {
            List<PolicyPaymentEntriesPost> _PolicyPaymentEntriesPost = PolicyPaymentEntriesPost.GetPolicyPaymentEntryPolicyIDWise(PolicyId);
            if (_PolicyPaymentEntriesPost.Count != 0) return;
            Dictionary<string, object> parameters = new Dictionary<string, object>();
            parameters.Add("PolicyId", PolicyId);
            PolicyDetailsData _PolicyRecord = GetPolicyData(parameters).FirstOrDefault();
            using (DLinq.CommissionDepartmentEntities DataModel = Entity.DataModel)
            {
                var _policy = (from p in DataModel.PoliciesHistories where (p.PolicyId == _PolicyRecord.PolicyId) select p).FirstOrDefault();
                if (_policy == null)
                {
                    _policy = new DLinq.PoliciesHistory
                    {
                        PolicyId = _PolicyRecord.PolicyId,
                        PolicyNumber = _PolicyRecord.PolicyNumber,
                        PolicyType = _PolicyRecord.PolicyType,
                        Insured = _PolicyRecord.Insured,
                        OriginalEffectiveDate = _PolicyRecord.OriginalEffectiveDate,
                        TrackFromDate = _PolicyRecord.TrackFromDate,
                        MonthlyPremium = _PolicyRecord.ModeAvgPremium,
                        SubmittedThrough = _PolicyRecord.SubmittedThrough,
                        Enrolled = _PolicyRecord.Enrolled,
                        Eligible = _PolicyRecord.Eligible,
                        PolicyTerminationDate = _PolicyRecord.PolicyTerminationDate,
                        IsTrackMissingMonth = _PolicyRecord.IsTrackMissingMonth,
                        IsTrackIncomingPercentage = _PolicyRecord.IsTrackIncomingPercentage,
                        IsTrackPayment = _PolicyRecord.IsTrackPayment,
                        IsDeleted = false,
                        ReplacedBy = _PolicyRecord.ReplacedBy,
                        DuplicateFrom = _PolicyRecord.DuplicateFrom,
                        CreatedOn = DateTime.Now,
                        IsIncomingBasicSchedule = _PolicyRecord.IsIncomingBasicSchedule,
                        IsOutGoingBasicSchedule = _PolicyRecord.IsOutGoingBasicSchedule,
                        SplitPercentage = _PolicyRecord.SplitPercentage,
                        Advance = _PolicyRecord.Advance,
                        ProductType = _PolicyRecord.ProductType,
                    };
                    _policy.MasterPolicyStatuReference.Value = (from m in DataModel.MasterPolicyStatus where m.PolicyStatusId == _PolicyRecord.PolicyStatusId select m).FirstOrDefault();
                    _policy.ClientReference.Value = (from s in DataModel.Clients where s.ClientId == _PolicyRecord.ClientId select s).FirstOrDefault();
                    _policy.LicenseeReference.Value = (from l in DataModel.Licensees where l.LicenseeId == _PolicyRecord.PolicyLicenseeId select l).FirstOrDefault();
                    _policy.MasterPolicyModeReference.Value = (from m in DataModel.MasterPolicyModes where m.PolicyModeId == _PolicyRecord.PolicyModeId select m).FirstOrDefault();
                    _policy.CoverageReference.Value = (from s in DataModel.Coverages where s.CoverageId == _PolicyRecord.CoverageId select s).FirstOrDefault();
                    _policy.MasterPolicyTerminationReasonReference.Value = (from s in DataModel.MasterPolicyTerminationReasons where s.PTReasonId == _PolicyRecord.TerminationReasonId select s).FirstOrDefault();
                    _policy.MasterIncomingPaymentTypeReference.Value = (from m in DataModel.MasterIncomingPaymentTypes where m.IncomingPaymentTypeId == _PolicyRecord.IncomingPaymentTypeId select m).FirstOrDefault();
                    _policy.PayorReference.Value = (from m in DataModel.Payors where m.PayorId == _PolicyRecord.PayorId select m).FirstOrDefault();
                    _policy.UserCredentialReference.Value = (from s in DataModel.UserCredentials where s.UserCredentialId == _PolicyRecord.CreatedBy select s).FirstOrDefault();
                    _policy.CarrierReference.Value = (from m in DataModel.Carriers where m.CarrierId == _PolicyRecord.CarrierID select m).FirstOrDefault();
                    DataModel.AddToPoliciesHistories(_policy);

                }
                else
                {
                    _policy.PolicyId = _PolicyRecord.PolicyId;
                    _policy.PolicyNumber = _PolicyRecord.PolicyNumber;
                    _policy.PolicyType = _PolicyRecord.PolicyType;
                    _policy.Insured = _PolicyRecord.Insured;
                    _policy.OriginalEffectiveDate = _PolicyRecord.OriginalEffectiveDate;
                    _policy.TrackFromDate = _PolicyRecord.TrackFromDate;
                    _policy.MonthlyPremium = _PolicyRecord.ModeAvgPremium;
                    _policy.SubmittedThrough = _PolicyRecord.SubmittedThrough;
                    _policy.Enrolled = _PolicyRecord.Enrolled;
                    _policy.Eligible = _PolicyRecord.Eligible;
                    _policy.PolicyTerminationDate = _PolicyRecord.PolicyTerminationDate;
                    _policy.IsTrackMissingMonth = _PolicyRecord.IsTrackMissingMonth;
                    _policy.IsTrackIncomingPercentage = _PolicyRecord.IsTrackIncomingPercentage;
                    _policy.IsTrackPayment = _PolicyRecord.IsTrackPayment;
                    _policy.IsDeleted = _PolicyRecord.IsDeleted;
                    _policy.ReplacedBy = _PolicyRecord.ReplacedBy;
                    _policy.DuplicateFrom = _PolicyRecord.DuplicateFrom;
                    _policy.CreatedOn = DateTime.Now;
                    _policy.IsIncomingBasicSchedule = _PolicyRecord.IsIncomingBasicSchedule;
                    _policy.IsOutGoingBasicSchedule = _PolicyRecord.IsOutGoingBasicSchedule;
                    _policy.SplitPercentage = _PolicyRecord.SplitPercentage;

                    _policy.ProductType = _PolicyRecord.ProductType;

                    _policy.MasterPolicyStatuReference.Value = (from m in DataModel.MasterPolicyStatus where m.PolicyStatusId == _PolicyRecord.PolicyStatusId select m).FirstOrDefault();
                    _policy.ClientReference.Value = (from s in DataModel.Clients where s.ClientId == _PolicyRecord.ClientId select s).FirstOrDefault();
                    _policy.LicenseeReference.Value = (from l in DataModel.Licensees where l.LicenseeId == _PolicyRecord.PolicyLicenseeId select l).FirstOrDefault();
                    _policy.MasterPolicyModeReference.Value = (from m in DataModel.MasterPolicyModes where m.PolicyModeId == _PolicyRecord.PolicyModeId select m).FirstOrDefault();
                    _policy.CoverageReference.Value = (from s in DataModel.Coverages where s.CoverageId == _PolicyRecord.CoverageId select s).FirstOrDefault();
                    _policy.MasterPolicyTerminationReasonReference.Value = (from s in DataModel.MasterPolicyTerminationReasons where s.PTReasonId == _PolicyRecord.TerminationReasonId select s).FirstOrDefault();
                    _policy.MasterIncomingPaymentTypeReference.Value = (from m in DataModel.MasterIncomingPaymentTypes where m.IncomingPaymentTypeId == _PolicyRecord.IncomingPaymentTypeId select m).FirstOrDefault();
                    _policy.PayorReference.Value = (from m in DataModel.Payors where m.PayorId == _PolicyRecord.PayorId select m).FirstOrDefault();
                    _policy.UserCredentialReference.Value = (from s in DataModel.UserCredentials where s.UserCredentialId == _PolicyRecord.CreatedBy select s).FirstOrDefault();
                    _policy.CarrierReference.Value = (from m in DataModel.Carriers where m.CarrierId == _PolicyRecord.CarrierID select m).FirstOrDefault();

                }
                DataModel.SaveChanges();
            }
        }

        public static void AddUpdatePolicyHistory(DLinq.Policy _PolicyRecord, DLinq.CommissionDepartmentEntities DataModel)
        {
            List<PolicyPaymentEntriesPost> _PolicyPaymentEntriesPost = PolicyPaymentEntriesPost.GetPolicyPaymentEntryPolicyIDWise(_PolicyRecord.PolicyId);
            if (_PolicyPaymentEntriesPost.Count != 0) return;

            var _policy = (from p in DataModel.PoliciesHistories where (p.PolicyId == _PolicyRecord.PolicyId) select p).FirstOrDefault();
            if (_policy == null)
            {
                _policy = new DLinq.PoliciesHistory
                {
                    PolicyId = _PolicyRecord.PolicyId,
                    PolicyNumber = _PolicyRecord.PolicyNumber,
                    PolicyType = _PolicyRecord.PolicyType,
                    Insured = _PolicyRecord.Insured,
                    OriginalEffectiveDate = _PolicyRecord.OriginalEffectiveDate,
                    TrackFromDate = _PolicyRecord.TrackFromDate,
                    MonthlyPremium = _PolicyRecord.MonthlyPremium,
                    SubmittedThrough = _PolicyRecord.SubmittedThrough,
                    Enrolled = _PolicyRecord.Enrolled,
                    Eligible = _PolicyRecord.Eligible,
                    PolicyTerminationDate = _PolicyRecord.PolicyTerminationDate,
                    IsTrackMissingMonth = _PolicyRecord.IsTrackMissingMonth,
                    IsTrackIncomingPercentage = _PolicyRecord.IsTrackIncomingPercentage,
                    IsTrackPayment = _PolicyRecord.IsTrackPayment,
                    IsDeleted = false,
                    ReplacedBy = _PolicyRecord.ReplacedBy,
                    DuplicateFrom = _PolicyRecord.DuplicateFrom,
                    CreatedOn = DateTime.Now,
                    IsIncomingBasicSchedule = _PolicyRecord.IsIncomingBasicSchedule,
                    IsOutGoingBasicSchedule = _PolicyRecord.IsOutGoingBasicSchedule,
                    SplitPercentage = _PolicyRecord.SplitPercentage,
                    ProductType = _PolicyRecord.ProductType,
                };
                _policy.MasterPolicyStatuReference.Value = _PolicyRecord.MasterPolicyStatu;//(from m in DataModel.MasterPolicyStatus where m.PolicyStatusId == _PolicyRecord.PolicyStatusId select m).FirstOrDefault();
                _policy.ClientReference.Value = _PolicyRecord.Client;// (from s in DataModel.Clients where s.ClientId == _PolicyRecord.ClientId select s).FirstOrDefault();
                _policy.LicenseeReference.Value = _PolicyRecord.Licensee;// (from l in DataModel.Licensees where l.LicenseeId == _PolicyRecord.PolicyLicenseeId select l).FirstOrDefault();
                _policy.MasterPolicyModeReference.Value = _PolicyRecord.MasterPolicyMode;// (from m in DataModel.MasterPolicyModes where m.PolicyModeId == _PolicyRecord.PolicyModeId select m).FirstOrDefault();
                _policy.CoverageReference.Value = _PolicyRecord.Coverage;// (from s in DataModel.Coverages where s.CoverageId == _PolicyRecord.CoverageId select s).FirstOrDefault();
                _policy.MasterPolicyTerminationReasonReference.Value = _PolicyRecord.MasterPolicyTerminationReason;// (from s in DataModel.MasterPolicyTerminationReasons where s.PTReasonId == _PolicyRecord.TerminationReasonId select s).FirstOrDefault();
                _policy.MasterIncomingPaymentTypeReference.Value = _PolicyRecord.MasterIncomingPaymentType;// (from m in DataModel.MasterIncomingPaymentTypes where m.IncomingPaymentTypeId == _PolicyRecord.IncomingPaymentTypeId select m).FirstOrDefault();
                _policy.PayorReference.Value = _PolicyRecord.Payor;// (from m in DataModel.Payors where m.PayorId == _PolicyRecord.PayorId select m).FirstOrDefault();
                _policy.UserCredentialReference.Value = _PolicyRecord.UserCredential;// (from s in DataModel.UserCredentials where s.UserCredentialId == _PolicyRecord.CreatedBy select s).FirstOrDefault();
                _policy.CarrierReference.Value = _PolicyRecord.Carrier;// (from m in DataModel.Carriers where m.CarrierId == _PolicyRecord.CarrierID select m).FirstOrDefault();
                DataModel.AddToPoliciesHistories(_policy);

            }
            else
            {
                _policy.PolicyId = _PolicyRecord.PolicyId;
                _policy.PolicyNumber = _PolicyRecord.PolicyNumber;
                _policy.PolicyType = _PolicyRecord.PolicyType;
                _policy.Insured = _PolicyRecord.Insured;
                _policy.OriginalEffectiveDate = _PolicyRecord.OriginalEffectiveDate;
                _policy.TrackFromDate = _PolicyRecord.TrackFromDate;
                _policy.MonthlyPremium = _PolicyRecord.MonthlyPremium;
                _policy.SubmittedThrough = _PolicyRecord.SubmittedThrough;
                _policy.Enrolled = _PolicyRecord.Enrolled;
                _policy.Eligible = _PolicyRecord.Eligible;
                _policy.PolicyTerminationDate = _PolicyRecord.PolicyTerminationDate;
                _policy.IsTrackMissingMonth = _PolicyRecord.IsTrackMissingMonth;
                _policy.IsTrackIncomingPercentage = _PolicyRecord.IsTrackIncomingPercentage;
                _policy.IsTrackPayment = _PolicyRecord.IsTrackPayment;
                _policy.IsDeleted = _PolicyRecord.IsDeleted;
                _policy.ReplacedBy = _PolicyRecord.ReplacedBy;
                _policy.DuplicateFrom = _PolicyRecord.DuplicateFrom;
                _policy.CreatedOn = DateTime.Now;
                _policy.IsIncomingBasicSchedule = _PolicyRecord.IsIncomingBasicSchedule;
                _policy.IsOutGoingBasicSchedule = _PolicyRecord.IsOutGoingBasicSchedule;
                _policy.SplitPercentage = _PolicyRecord.SplitPercentage;

                _policy.ProductType = _PolicyRecord.ProductType;

                _policy.MasterPolicyStatuReference.Value = _PolicyRecord.MasterPolicyStatu;//(from m in DataModel.MasterPolicyStatus where m.PolicyStatusId == _PolicyRecord.PolicyStatusId select m).FirstOrDefault();
                _policy.ClientReference.Value = _PolicyRecord.Client;// (from s in DataModel.Clients where s.ClientId == _PolicyRecord.ClientId select s).FirstOrDefault();
                _policy.LicenseeReference.Value = _PolicyRecord.Licensee;// (from l in DataModel.Licensees where l.LicenseeId == _PolicyRecord.PolicyLicenseeId select l).FirstOrDefault();
                _policy.MasterPolicyModeReference.Value = _PolicyRecord.MasterPolicyMode;// (from m in DataModel.MasterPolicyModes where m.PolicyModeId == _PolicyRecord.PolicyModeId select m).FirstOrDefault();
                _policy.CoverageReference.Value = _PolicyRecord.Coverage;// (from s in DataModel.Coverages where s.CoverageId == _PolicyRecord.CoverageId select s).FirstOrDefault();
                _policy.MasterPolicyTerminationReasonReference.Value = _PolicyRecord.MasterPolicyTerminationReason;// (from s in DataModel.MasterPolicyTerminationReasons where s.PTReasonId == _PolicyRecord.TerminationReasonId select s).FirstOrDefault();
                _policy.MasterIncomingPaymentTypeReference.Value = _PolicyRecord.MasterIncomingPaymentType;// (from m in DataModel.MasterIncomingPaymentTypes where m.IncomingPaymentTypeId == _PolicyRecord.IncomingPaymentTypeId select m).FirstOrDefault();
                _policy.PayorReference.Value = _PolicyRecord.Payor;// (from m in DataModel.Payors where m.PayorId == _PolicyRecord.PayorId select m).FirstOrDefault();
                _policy.UserCredentialReference.Value = _PolicyRecord.UserCredential;// (from s in DataModel.UserCredentials where s.UserCredentialId == _PolicyRecord.CreatedBy select s).FirstOrDefault();
                _policy.CarrierReference.Value = _PolicyRecord.Carrier;// (from m in DataModel.Carriers where m.CarrierId == _PolicyRecord.CarrierID select m).FirstOrDefault();
            }
            DataModel.SaveChanges();


        }
        
        public static void AddUpdatePolicyHistoryNotCheckPayment(Guid PolicyId)
        {
            Dictionary<string, object> parameters = new Dictionary<string, object>();
            parameters.Add("PolicyId", PolicyId);
            PolicyDetailsData _PolicyRecord = GetPolicyData(parameters).FirstOrDefault();
            using (DLinq.CommissionDepartmentEntities DataModel = Entity.DataModel)
            {
                var _policy = (from p in DataModel.PoliciesHistories where (p.PolicyId == _PolicyRecord.PolicyId) select p).FirstOrDefault();
                if (_policy == null)
                {
                    _policy = new DLinq.PoliciesHistory
                    {
                        PolicyId = _PolicyRecord.PolicyId,
                        PolicyNumber = _PolicyRecord.PolicyNumber,
                        PolicyType = _PolicyRecord.PolicyType,
                        Insured = _PolicyRecord.Insured,
                        OriginalEffectiveDate = _PolicyRecord.OriginalEffectiveDate,
                        TrackFromDate = _PolicyRecord.TrackFromDate,
                        MonthlyPremium = _PolicyRecord.ModeAvgPremium,
                        SubmittedThrough = _PolicyRecord.SubmittedThrough,
                        Enrolled = _PolicyRecord.Enrolled,
                        Eligible = _PolicyRecord.Eligible,
                        PolicyTerminationDate = _PolicyRecord.PolicyTerminationDate,
                        IsTrackMissingMonth = _PolicyRecord.IsTrackMissingMonth,
                        IsTrackIncomingPercentage = _PolicyRecord.IsTrackIncomingPercentage,
                        IsTrackPayment = _PolicyRecord.IsTrackPayment,
                        IsDeleted = false,
                        ReplacedBy = _PolicyRecord.ReplacedBy,
                        DuplicateFrom = _PolicyRecord.DuplicateFrom,
                        CreatedOn = DateTime.Now,
                        IsIncomingBasicSchedule = _PolicyRecord.IsIncomingBasicSchedule,
                        IsOutGoingBasicSchedule = _PolicyRecord.IsOutGoingBasicSchedule,
                        SplitPercentage = _PolicyRecord.SplitPercentage,
                    };
                    _policy.MasterPolicyStatuReference.Value = (from m in DataModel.MasterPolicyStatus where m.PolicyStatusId == _PolicyRecord.PolicyStatusId select m).FirstOrDefault();
                    _policy.ClientReference.Value = (from s in DataModel.Clients where s.ClientId == _PolicyRecord.ClientId select s).FirstOrDefault();
                    _policy.LicenseeReference.Value = (from l in DataModel.Licensees where l.LicenseeId == _PolicyRecord.PolicyLicenseeId select l).FirstOrDefault();
                    _policy.MasterPolicyModeReference.Value = (from m in DataModel.MasterPolicyModes where m.PolicyModeId == _PolicyRecord.PolicyModeId select m).FirstOrDefault();
                    _policy.CoverageReference.Value = (from s in DataModel.Coverages where s.CoverageId == _PolicyRecord.CoverageId select s).FirstOrDefault();
                    _policy.MasterPolicyTerminationReasonReference.Value = (from s in DataModel.MasterPolicyTerminationReasons where s.PTReasonId == _PolicyRecord.TerminationReasonId select s).FirstOrDefault();
                    _policy.MasterIncomingPaymentTypeReference.Value = (from m in DataModel.MasterIncomingPaymentTypes where m.IncomingPaymentTypeId == _PolicyRecord.IncomingPaymentTypeId select m).FirstOrDefault();
                    _policy.PayorReference.Value = (from m in DataModel.Payors where m.PayorId == _PolicyRecord.PayorId select m).FirstOrDefault();
                    _policy.UserCredentialReference.Value = (from s in DataModel.UserCredentials where s.UserCredentialId == _PolicyRecord.CreatedBy select s).FirstOrDefault();
                    _policy.CarrierReference.Value = (from m in DataModel.Carriers where m.CarrierId == _PolicyRecord.CarrierID select m).FirstOrDefault();
                    DataModel.AddToPoliciesHistories(_policy);

                }
                else
                {
                    _policy.PolicyId = _PolicyRecord.PolicyId;
                    _policy.PolicyNumber = _PolicyRecord.PolicyNumber;
                    _policy.PolicyType = _PolicyRecord.PolicyType;
                    _policy.Insured = _PolicyRecord.Insured;
                    _policy.OriginalEffectiveDate = _PolicyRecord.OriginalEffectiveDate;
                    _policy.TrackFromDate = _PolicyRecord.TrackFromDate;
                    _policy.MonthlyPremium = _PolicyRecord.ModeAvgPremium;
                    _policy.SubmittedThrough = _PolicyRecord.SubmittedThrough;
                    _policy.Enrolled = _PolicyRecord.Enrolled;
                    _policy.Eligible = _PolicyRecord.Eligible;
                    _policy.PolicyTerminationDate = _PolicyRecord.PolicyTerminationDate;
                    _policy.IsTrackMissingMonth = _PolicyRecord.IsTrackMissingMonth;
                    _policy.IsTrackIncomingPercentage = _PolicyRecord.IsTrackIncomingPercentage;
                    _policy.IsTrackPayment = _PolicyRecord.IsTrackPayment;
                    _policy.IsDeleted = _PolicyRecord.IsDeleted;
                    _policy.ReplacedBy = _PolicyRecord.ReplacedBy;
                    _policy.DuplicateFrom = _PolicyRecord.DuplicateFrom;
                    _policy.CreatedOn = DateTime.Now;
                    _policy.IsIncomingBasicSchedule = _PolicyRecord.IsIncomingBasicSchedule;
                    _policy.IsOutGoingBasicSchedule = _PolicyRecord.IsOutGoingBasicSchedule;
                    _policy.SplitPercentage = _PolicyRecord.SplitPercentage;
                    _policy.MasterPolicyStatuReference.Value = (from m in DataModel.MasterPolicyStatus where m.PolicyStatusId == _PolicyRecord.PolicyStatusId select m).FirstOrDefault();
                    _policy.ClientReference.Value = (from s in DataModel.Clients where s.ClientId == _PolicyRecord.ClientId select s).FirstOrDefault();
                    _policy.LicenseeReference.Value = (from l in DataModel.Licensees where l.LicenseeId == _PolicyRecord.PolicyLicenseeId select l).FirstOrDefault();
                    _policy.MasterPolicyModeReference.Value = (from m in DataModel.MasterPolicyModes where m.PolicyModeId == _PolicyRecord.PolicyModeId select m).FirstOrDefault();
                    _policy.CoverageReference.Value = (from s in DataModel.Coverages where s.CoverageId == _PolicyRecord.CoverageId select s).FirstOrDefault();
                    _policy.MasterPolicyTerminationReasonReference.Value = (from s in DataModel.MasterPolicyTerminationReasons where s.PTReasonId == _PolicyRecord.TerminationReasonId select s).FirstOrDefault();
                    _policy.MasterIncomingPaymentTypeReference.Value = (from m in DataModel.MasterIncomingPaymentTypes where m.IncomingPaymentTypeId == _PolicyRecord.IncomingPaymentTypeId select m).FirstOrDefault();
                    _policy.PayorReference.Value = (from m in DataModel.Payors where m.PayorId == _PolicyRecord.PayorId select m).FirstOrDefault();
                    _policy.UserCredentialReference.Value = (from s in DataModel.UserCredentials where s.UserCredentialId == _PolicyRecord.CreatedBy select s).FirstOrDefault();
                    _policy.CarrierReference.Value = (from m in DataModel.Carriers where m.CarrierId == _PolicyRecord.CarrierID select m).FirstOrDefault();

                }
                DataModel.SaveChanges();
            }
        }

        public static PolicyDetailsData GetPolicyHistoryIdWise(Guid PolicyId)
        {
            using (DLinq.CommissionDepartmentEntities DataModel = Entity.DataModel)
            {
                PolicyDetailsData _Policy = (from pl in DataModel.PoliciesHistories
                                             where (pl.IsDeleted == false) && (pl.PolicyId == PolicyId)
                                             select new PolicyDetailsData
                                          {
                                              PolicyId = pl.PolicyId,
                                              PolicyNumber = pl.PolicyNumber,
                                              PolicyStatusId = pl.MasterPolicyStatu.PolicyStatusId,
                                              PolicyStatusName = pl.MasterPolicyStatu.Name,
                                              PolicyType = pl.PolicyType,
                                              PolicyLicenseeId = pl.Licensee.LicenseeId,
                                              Insured = pl.Insured,
                                              OriginalEffectiveDate = pl.OriginalEffectiveDate,
                                              TrackFromDate = pl.TrackFromDate,
                                              PolicyModeId = pl.MasterPolicyMode.PolicyModeId,
                                              ModeAvgPremium = pl.MonthlyPremium,
                                              SubmittedThrough = pl.SubmittedThrough,
                                              Enrolled = pl.Enrolled,
                                              Eligible = pl.Eligible,
                                              PolicyTerminationDate = pl.PolicyTerminationDate,
                                              TerminationReasonId = pl.TerminationReasonId,
                                              IsTrackMissingMonth = pl.IsTrackMissingMonth,
                                              IsTrackIncomingPercentage = pl.IsTrackIncomingPercentage,
                                              IsTrackPayment = pl.IsTrackPayment,
                                              IsDeleted = pl.IsDeleted,
                                              CarrierID = pl.Carrier.CarrierId == null ? Guid.Empty : pl.Carrier.CarrierId,
                                              CarrierName = pl.Carrier.CarrierName,
                                              CoverageId = pl.Coverage.CoverageId == null ? Guid.Empty : pl.Coverage.CoverageId,
                                              CoverageName = pl.Coverage.ProductName,
                                              ClientId = pl.Client.ClientId,
                                              ClientName = pl.Client.Name,
                                              ReplacedBy = pl.ReplacedBy,
                                              DuplicateFrom = pl.DuplicateFrom,
                                              IsIncomingBasicSchedule = pl.IsIncomingBasicSchedule,
                                              IsOutGoingBasicSchedule = pl.IsOutGoingBasicSchedule,
                                              PayorId = pl.Payor.PayorId == null ? Guid.Empty : pl.Payor.PayorId,
                                              PayorName = pl.Payor.PayorName,
                                              SplitPercentage = pl.SplitPercentage,
                                              IncomingPaymentTypeId = pl.IncomingPaymentTypeId,
                                              CreatedOn = pl.CreatedOn,
                                              CreatedBy = pl.CreatedBy.Value,//--always check it will never null
                                              IsSavedPolicy = true,

                                          }).FirstOrDefault();
                if (_Policy != null)
                {
                    _Policy.PolicyPreviousData = FillPolicyDetailPreviousData(_Policy);
                }
                
                return _Policy;
            }
        }

        public static void DeletePolicyHistory(PolicyDetailsData _policyrecord)
        {
            using (DLinq.CommissionDepartmentEntities DataModel = Entity.DataModel)
            {
                var _policy = (from p in DataModel.PoliciesHistories where (p.PolicyId == _policyrecord.PolicyId) select p).FirstOrDefault();
                if (_policy == null) return;
                _policy.IsDeleted = true;
                DataModel.SaveChanges();
            }
        }

        public static void DeletePolicyHistoryPermanentById(PolicyDetailsData _Policy)
        {
            using (DLinq.CommissionDepartmentEntities DataModel = Entity.DataModel)
            {
                var _policy = (from p in DataModel.PoliciesHistories where (p.PolicyId == _Policy.PolicyId) select p).FirstOrDefault();
                if (_policy == null) return;
                DataModel.DeleteObject(_policy);
                DataModel.SaveChanges();
            }
        }

        public static PolicyDetailPreviousData FillPolicyDetailPreviousData(PolicyDetailsData _Policy)
        {
            PolicyDetailPreviousData _PolicyDetailPreviousData = new PolicyDetailPreviousData();
            try
            {
                if (_Policy != null)
                {

                    _PolicyDetailPreviousData.OriginalEffectiveDate = _Policy.OriginalEffectiveDate;
                    _PolicyDetailPreviousData.PolicyModeId = _Policy.PolicyModeId;
                    _PolicyDetailPreviousData.TrackFromDate = _Policy.TrackFromDate;
                    _PolicyDetailPreviousData.PolicyTermdateDate = _Policy.PolicyTerminationDate;
                }
            }
            catch (Exception)
            {
            }
           
          
            return _PolicyDetailPreviousData;
        }

        public static PolicyDetailMasterData GetPolicyDetailMasterData()
        {
            PolicyDetailMasterData pdMasterData = new PolicyDetailMasterData();
            pdMasterData.Statuses = PolicyStatus.GetPolicyStatusList();
            pdMasterData.Modes = PolicyMode.GetPolicyModeListWithBlankAdded();
            pdMasterData.TerminationReasons = PolicyTerminationReason.GetTerminationReasonListWithBlankAdded();
            pdMasterData.IncomingPaymentTypes = PolicyIncomingPaymentType.GetIncomingPaymentTypeList();
            pdMasterData.IssueCategories = IssueCategory.GetAllCategory();
            pdMasterData.IssueReasons = IssueReasons.GetAllReason();
            pdMasterData.IssueResults = IssueResults.GetAllResults();
            pdMasterData.IssueStatuses = IssueStatus.GetAllStatus();
            pdMasterData.LearnedMasterIncomingPaymentTypes = PolicyIncomingPaymentType.GetIncomingPaymentTypeList(); ;
            pdMasterData.LearnedMasterPaymentsModes = PolicyMode.GetPolicyModeListWithBlankAdded();
            pdMasterData.IncomingAdvanceScheduleTypes = PolicyIncomingScheduleType.GetIncomingScheduleTypeList();
            pdMasterData.OutgoingAdvanceScheduleTypes = PolicyOutgoingScheduleType.GetOutgoingScheduleTypeList();

            return pdMasterData;
        }

        #endregion
    }


}
