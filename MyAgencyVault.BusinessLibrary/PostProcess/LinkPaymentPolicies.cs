////////////////////////////////////////////////////////////////////////////////////////
//   Description : It is a temp collection for LinkPayment Pending Policy
//   
//
//

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MyAgencyVault.BusinessLibrary.Base;
using System.Runtime.Serialization;
using System.Transactions;
using DLinq = DataAccessLayer.LinqtoEntity;
using System.Linq.Expressions;
using System.Threading;

namespace MyAgencyVault.BusinessLibrary
{
    [DataContract]
    public class LinkPaymentPolicies
    {
        [DataMember]
        public Guid PolicyId { get; set; }

        [DataMember]
        public Guid ClientId { get; set; }
        [DataMember]
        public string ClientName { get; set; }
        [DataMember]
        public string Insured { get; set; }
        [DataMember]
        public Guid PayorId { get; set; }
        [DataMember]
        public string PayorName { get; set; }
        [DataMember]
        public Guid CarrierId { get; set; }
        [DataMember]
        public string CarrierName { get; set; }
        [DataMember]
        public Guid ProductId { get; set; }
        [DataMember]
        public string ProductName { get; set; }
        [DataMember]
        public int? CompTypeId { get; set; }
        [DataMember]
        public string CompTypeName { get; set; }
        [DataMember]
        public int? CompScheduleTypeId { get; set; }
        [DataMember]
        public string CompScheduleTypeName { get; set; }
        //[DataMember]
        //public double FirstYear { get; set; }
        //[DataMember]
        //public double Renewal { get; set; }
        [DataMember]
        public List<LinkPaymentReciptRecords> Entries { get; set; }
        [DataMember]
        public LinkPaymentReciptRecords SelectedEntries { get; set; }
        [DataMember]
        public string PolicyNumber { get; set; }
        [DataMember]
        public int? StatusId { get; set; }
        [DataMember]
        public string StatusName { get; set; }

        [DataMember]
        public DateTime? OriginalEffDate { get; set; }

        [DataMember]
        public DateTime? CreatedOn { get; set; }

        [DataMember]
        public Guid LicenseId { get; set; }
        public void AddUpdate(LinkPaymentPolicies _LinkPaymentPolicies)
        {
        }

        public static List<LinkPaymentPolicies> GetPendingPoliciesForLinkedPolicy(Guid LicencessId)
        {
            List<LinkPaymentPolicies> _LinkedPaymentPendingPolicies = new List<LinkPaymentPolicies>();
            Dictionary<string, object> parameters = new Dictionary<string, object>();
            parameters.Add("PolicyLicenseeId", LicencessId);
            parameters.Add("IsDeleted", false);
            parameters.Add("PolicyStatusId", (int)_PolicyStatus.Pending);
            Expression<Func<DLinq.Policy, bool>> expressionParameters = p => p.PolicyPaymentEntries.Count > 0;
            List<PolicyDetailsData> _Policies = Policy.GetPolicyData(parameters, expressionParameters);
            _Policies = new List<PolicyDetailsData>(_Policies.Where(p => p.ClientId != Guid.Empty).ToList());

            foreach (PolicyDetailsData _Policy in _Policies)
            {
                try
                {
                    LinkPaymentPolicies _LinkPaymentPolicies = new LinkPaymentPolicies();
                    _LinkPaymentPolicies.PolicyId = _Policy.PolicyId;
                    _LinkPaymentPolicies.PolicyNumber = _Policy.PolicyNumber;
                    _LinkPaymentPolicies.ClientId = _Policy.ClientId.Value;
                    _LinkPaymentPolicies.ClientName = _Policy.ClientName;
                    _LinkPaymentPolicies.Insured = _Policy.Insured ?? "";
                    _LinkPaymentPolicies.CarrierId = _Policy.CarrierID ?? Guid.Empty;
                    _LinkPaymentPolicies.CarrierName = _Policy.CarrierName ?? "";
                    _LinkPaymentPolicies.ProductId = _Policy.CoverageId ?? Guid.Empty;
                    _LinkPaymentPolicies.PayorId = _Policy.PayorId ?? Guid.Empty;
                    _LinkPaymentPolicies.PayorName = _Policy.PayorName;
                    _LinkPaymentPolicies.ProductName = _Policy.CoverageName ?? "";
                    _LinkPaymentPolicies.CompTypeId = _Policy.IncomingPaymentTypeId ?? 0;
                    _LinkPaymentPolicies.OriginalEffDate = _Policy.OriginalEffectiveDate;
                    _LinkPaymentPolicies.LicenseId = _Policy.PolicyLicenseeId ?? Guid.Empty;
                    _LinkPaymentPolicies.CreatedOn = _Policy.CreatedOn;
                    _LinkPaymentPolicies.CompTypeName = CompType(_Policy.CompType);
                    _LinkPaymentPolicies.CompScheduleTypeName = _Policy.CompSchuduleType;

                    _LinkedPaymentPendingPolicies.Add(_LinkPaymentPolicies);

                    Payor _Payor = Payor.GetPayorByID(_Policy.PayorId ?? Guid.Empty);
                    _LinkedPaymentPendingPolicies.Where(p => p.PolicyId == _Policy.PolicyId).FirstOrDefault().PayorId = _Payor.PayorID;
                    _LinkedPaymentPendingPolicies.Where(p => p.PolicyId == _Policy.PolicyId).FirstOrDefault().PayorName = _Payor.PayorName;

                }
                catch (Exception)
                {
                }
            }

            return _LinkedPaymentPendingPolicies.OrderBy(p => p.CreatedOn).ToList();
        }


        private static string CompType(int? intVAlue)
        {
            string strReturn = string.Empty;

            switch (intVAlue)
            {
                case 0:
                    strReturn = "Other";
                    break;
                case 1:
                    strReturn = "Commission";
                    break;
                case 2:
                    strReturn = "Override";
                    break;
                case 3:
                    strReturn = "Bonus";
                    break;
                case 4:
                    strReturn = "Fee";
                    break;

                case 5:
                    strReturn = "Pending";
                    break;

                default:
                    strReturn = "Pending";
                    break;

            }
            return strReturn;
        }
       
        public static List<LinkPaymentPolicies> GetAllPoliciesForLinkedPolicy(Guid LicencessId)
        {
            List<LinkPaymentPolicies> _LinkedPaymentAllPolicies = new List<LinkPaymentPolicies>();
            Dictionary<string, object> parameters = new Dictionary<string, object>();
            parameters.Add("PolicyLicenseeId", LicencessId);
            parameters.Add("IsDeleted", false);

            List<PolicyDetailsData> _Policies = Policy.GetPolicyData(parameters);
            _Policies = new List<PolicyDetailsData>(_Policies.Where(p => p.ClientId != Guid.Empty).ToList());
            _Policies = _Policies.Where(p => p.PolicyStatusId != (int)_PolicyStatus.Pending).ToList<PolicyDetailsData>();

            for (int idx = 0; idx < _Policies.Count; idx++)
            {
                LinkPaymentPolicies lpp = new LinkPaymentPolicies();
                lpp.PolicyId = _Policies[idx].PolicyId;
                try
                {
                    if (_Policies[idx].ClientId != null)
                    {
                        lpp.ClientId = _Policies[idx].ClientId.Value;
                    }
                    
                }
                catch {
                }
                lpp.ClientName = _Policies[idx].ClientName;
                lpp.Insured = _Policies[idx].Insured;
                lpp.PayorId = _Policies[idx].PayorId ?? Guid.Empty;
                lpp.PayorName = _Policies[idx].PayorName;
                lpp.CarrierId = _Policies[idx].CarrierID ?? Guid.Empty;
                lpp.CarrierName = _Policies[idx].CarrierName;
                lpp.ProductId = _Policies[idx].CoverageId.Value;
                lpp.ProductName = _Policies[idx].CoverageName;
                lpp.CompTypeId = _Policies[idx].IncomingPaymentTypeId;


                lpp.PolicyNumber = _Policies[idx].PolicyNumber;
                lpp.StatusId = _Policies[idx].PolicyStatusId;
                lpp.StatusName = _Policies[idx].PolicyStatusName;
                lpp.OriginalEffDate = _Policies[idx].OriginalEffectiveDate;
                lpp.CompTypeName = CompType(_Policies[idx].CompType);
                lpp.CompScheduleTypeName = _Policies[idx].CompSchuduleType;
                lpp.LicenseId = _Policies[idx].PolicyLicenseeId ?? Guid.Empty;
                _LinkedPaymentAllPolicies.Add(lpp);
            }
            return _LinkedPaymentAllPolicies;
        }

        public static void MakePolicyActive(Guid PolicyId, Guid ClientId)
        {
            PolicyDetailsData _Policy = PostUtill.GetPolicy(PolicyId);
            _Policy.PolicyStatusId = (int?)_PolicyStatus.Active;
            Guid? Cliid = null;
            if (ClientId != Guid.Empty)
            {
                Cliid = _Policy.ClientId;
                _Policy.ClientId = ClientId;

            }
            Policy.AddUpdatePolicy(_Policy);
            Policy.AddUpdatePolicyHistoryNotCheckPayment(_Policy.PolicyId);
            PolicyLearnedFieldData _PolicyLearnedField = PolicyLearnedField.GetPolicyLearnedFieldsPolicyWise(_Policy.PolicyId);
            _PolicyLearnedField.ClientID = _Policy.ClientId.Value;
            _PolicyLearnedField.ClientName = _Policy.ClientName;

            PolicyLearnedField.AddUpdateLearned(_PolicyLearnedField, _PolicyLearnedField.ProductType);
            PolicyLearnedField.AddUpdateHistoryLearnedNotCheckPayment(_Policy.PolicyId);


            using (DLinq.CommissionDepartmentEntities DataModel = Entity.DataModel)
            {
                DataModel.EntriesByDEUs.Where(P => (P.PolicyID == _Policy.PolicyId) && (P.ClientID == Cliid)).ToList().ForEach(p => p.ClientID = _Policy.ClientId);
                DataModel.SaveChanges();
            }
            RemoveLinkPaymentToPolicy(Cliid);

            RemoveClient(Cliid.Value, _Policy.PolicyLicenseeId.Value);

        }

        public static void RemoveLinkPaymentToPolicy(Guid? Cliid)
        {
            LastViewPolicy.DeleteLastViewRecordClientIdWise(Cliid.Value);
        }

        public static void DoLinkPolicy(Guid LicenseeId, bool IsReverse, bool IsLinkWithExistingPolicy, Guid PendingPolicyId, Guid ClientId, Guid activePolicyId, Guid PolicyPaymentEntryId, Guid CurrentUser, Guid PendingPayorId,
            Guid ActivePayorId, bool IsAgencyVersion, bool IsPaidMarked, bool IsScheduleMatches, UserRole _UserRole)
        {
            var options = new TransactionOptions
                  {
                      IsolationLevel = IsolationLevel.ReadCommitted,
                      Timeout = TimeSpan.FromMinutes(60)
                  };
            using (TransactionScope ts = new TransactionScope(TransactionScopeOption.Required, options))
            {
                try
                {

                    int NonPendPolicyPaymentEntryCnt = PolicyPaymentEntriesPost.GetPolicyPaymentEntryPolicyIDWise(activePolicyId).ToList().Count;//Get record of policy via entryId
                    ChangeInPaymentIncomingEntries(PolicyPaymentEntryId, PendingPolicyId, activePolicyId);

                    if (IsAgencyVersion && IsPaidMarked && IsScheduleMatches && IsReverse && IsLinkWithExistingPolicy)
                    {
                        AddRecordWithMinusAmountInPaymentOutgoingEntries(PolicyPaymentEntryId, PendingPolicyId, activePolicyId);
                        DistributeAmountToPayee(PolicyPaymentEntryId);
                        UpdateBatchData(PolicyPaymentEntryId);
                    }
                    else if (IsAgencyVersion && !IsPaidMarked && IsScheduleMatches && IsLinkWithExistingPolicy)
                    {
                        //Same
                        RemoveRecordFromOutgoing(PolicyPaymentEntryId);
                        DistributeAmountToPayee(PolicyPaymentEntryId);
                        UpdateBatchData(PolicyPaymentEntryId);
                    }

                    //#region "Updated code"

                    //int PendPolicyPaymentEntryCnt = PolicyPaymentEntriesPost.GetPolicyPaymentEntryPolicyIDWise(PendingPolicyId).ToList().Count;//Get record of policy via entryId

                    //if (PendPolicyPaymentEntryCnt == 1)
                    //{
                    //    RunLearnedForPolicy(activePolicyId);
                    //    RunFollowp(activePolicyId, PolicyPaymentEntryId, _UserRole);//For Active
                    //}                    

                    //if (PendPolicyPaymentEntryCnt == 0)
                    //{
                    //    Client.DeleteCascadeClient(PendingPolicyId, ClientId, LicenseeId);
                    //}
                    //else//For Pending
                    //{
                    //    if (PendPolicyPaymentEntryCnt == 1)
                    //    {
                    //        FollowUpUtill.FollowUpProcedure(FollowUpRunModules.PaymentDeleted, null, PendingPolicyId, PostUtill.GetPolicy(PendingPolicyId).IsTrackPayment, true, _UserRole, null);
                    //    }
                    //}

                    //#endregion

                    #region "Commented code"

                    //RunLearnedForPolicy(activePolicyId);
                    //RunFollowp(activePolicyId, PolicyPaymentEntryId, _UserRole);//For Active policy

                    //Thread  ThFollowup=new Thread(() =>
                    //{

                    RunLearnedForPolicy(activePolicyId);
                    RunFollowp(activePolicyId, PolicyPaymentEntryId, _UserRole);//For Active policy
                    //});

                    //ThFollowup.IsBackground = true;
                    //ThFollowup.Start();


                    int PendPolicyPaymentEntryCnt = PolicyPaymentEntriesPost.GetPolicyPaymentEntryPolicyIDWise(PendingPolicyId).ToList().Count;//Get record of policy via entryId

                    if (PendPolicyPaymentEntryCnt == 0)
                    {
                        Client.DeleteCascadeClient(PendingPolicyId, ClientId, LicenseeId);
                    }
                    else//For Pending policy
                    {
                        //Thread ThFollowup1 = new Thread(() =>
                        //{
                        //    FollowUpUtill.FollowUpProcedure(FollowUpRunModules.PaymentDeleted, null, PendingPolicyId, PostUtill.GetPolicy(PendingPolicyId).IsTrackPayment, true, _UserRole, null);
                        //});

                        //ThFollowup1.IsBackground = true;
                        //ThFollowup1.Start();
                        FollowUpUtill.FollowUpProcedure(FollowUpRunModules.PaymentDeleted, null, PendingPolicyId, PostUtill.GetPolicy(PendingPolicyId).IsTrackPayment, true, _UserRole, null);
                    }
                    #endregion


                    //Update policy billing database...
                    using (DLinq.CommissionDepartmentEntities InnerDataModel = Entity.DataModel)
                    {
                        int totalUnits = (InnerDataModel.PolicyLevelBillingDetails.Where(s => s.PolicyId == PendingPolicyId).Sum(s => s.TotalUnits)) ?? 0;
                        DLinq.PolicyLevelBillingDetail policyDetailData = InnerDataModel.PolicyLevelBillingDetails.FirstOrDefault(s => s.PolicyId == activePolicyId);
                        if (policyDetailData != null && totalUnits > 0)
                        {
                            policyDetailData.TotalUnits += totalUnits;
                            InnerDataModel.SaveChanges();
                        }
                    }

                    if (PendPolicyPaymentEntryCnt == 0)
                    {
                        try
                        {
                            RemovePendingPolicy(PendingPolicyId);

                            RemoveEntryByDEU(PendingPolicyId, PolicyPaymentEntryId);

                            RemoveClient(ClientId, LicenseeId);
                        }
                        catch
                        {
                        }
                    }

                    ts.Complete();
                }
                catch
                {

                }
            }
        }

        private static void RemoveEntryByDEU(Guid PendingPolicyId, Guid paymentEntryID)
        {

            using (DLinq.CommissionDepartmentEntities DataModel = Entity.DataModel)
            {
                List<DLinq.PolicyPaymentEntry> PolicyPaymentEntryLst = DataModel.PolicyPaymentEntries.Where(p => p.PolicyId == PendingPolicyId && p.PaymentEntryId == paymentEntryID).ToList();
                foreach (DLinq.PolicyPaymentEntry ppe in PolicyPaymentEntryLst)
                {
                    List<DLinq.EntriesByDEU> EntriesByDEULst = DataModel.EntriesByDEUs.Where(p => p.PolicyID == PendingPolicyId && p.DEUEntryID == ppe.DEUEntryId).ToList();
                    foreach (DLinq.EntriesByDEU ebdeu in EntriesByDEULst)
                    {
                        DataModel.DeleteObject(ebdeu);

                    }
                    DataModel.SaveChanges();
                }
            }
        }

        private static void RemovePolicyPaymentEntry(Guid PendingPolicyId)
        {
            using (DLinq.CommissionDepartmentEntities DataModel = Entity.DataModel)
            {
                List<DLinq.PolicyPaymentEntry> PolicyPaymentEntryLst = DataModel.PolicyPaymentEntries.Where(p => p.PolicyId == PendingPolicyId).ToList();
                foreach (DLinq.PolicyPaymentEntry ppe in PolicyPaymentEntryLst)
                {
                    DataModel.DeleteObject(ppe);

                }
                DataModel.SaveChanges();
            }
        }

        private static void RemoveLastViewRecord(Guid PendingPolicyId)
        {
            LastViewPolicy.DeleteLastViewRecordPolicyIdWise(PendingPolicyId);
        }

        private static void UpdateDeuPolicyEntry(Guid _PolicyPaymentEntryId, Guid activePolicyId)
        {
            Guid _DeuEntryId = PolicyPaymentEntriesPost.GetPolicyPaymentEntry(_PolicyPaymentEntryId).DEUEntryId.Value;
            DEU _deu = DEU.GetDeuEntryidWise(_DeuEntryId);
            _deu.PolicyId = activePolicyId;           
            //DEU.AddupdateDeuEntry(_deu);

            //Create object
            DEU objDEU = new DEU();
            objDEU.AddupdateDeuEntry(_deu);
        }

        private static void RemoveRecordFromOutgoing(Guid PolicyPaymentEntryId)
        {
            List<PolicyOutgoingDistribution> _PolicyOutgoingDistribution = PolicyOutgoingDistribution.GetOutgoingPaymentByPoicyPaymentEntryId(PolicyPaymentEntryId);
            foreach (PolicyOutgoingDistribution POD in _PolicyOutgoingDistribution)
            {
                PolicyOutgoingDistribution.DeleteById(POD.OutgoingPaymentId);
            }
        }

        private static void RunFollowp(Guid PolicyId, Guid PolicyPaymentEntryId, UserRole _UserRole)
        {
            bool IsTrackPayment = PostUtill.GetPolicy(PolicyId).IsTrackPayment;
            Guid DeuEntryId = GetDEUEntryId(PolicyPaymentEntryId);
            DEU _DeuEntry = DEU.GetDeuEntryidWise(DeuEntryId);

            FollowUpUtill.FollowUpProcedure(FollowUpRunModules.PaymentEntered, _DeuEntry, PolicyId, IsTrackPayment, true, _UserRole, null);
        }

        // private static void RunLearnedForPolicy(Guid CurrentUser, Guid LicenseeId, Guid PolicyPaymentEntryId, Guid PolicyId)
        private static void RunLearnedForPolicy(Guid PolicyId)
        {
            //DEU _LatestDEUrecord = DEU.GetLatestInvoiceDateRecord(PolicyId);

            //instance method
            DEU objDeu = new DEU();
            DEU _LatestDEUrecord = objDeu.GetLatestInvoiceDateRecord(PolicyId);

            if (_LatestDEUrecord != null)
            {
                Guid PolicyIdDeuToLrn = DEULearnedPost.AddDataDeuToLearnedPost(_LatestDEUrecord);
                if (PolicyIdDeuToLrn != null)
                {
                    LearnedToPolicyPost.AddUpdateLearnedToPolicy(PolicyIdDeuToLrn);
                    PolicyToLearnPost.AddUpdatPolicyToLearn(PolicyIdDeuToLrn);
                }

            }
            //DEUFields _DEUFields = new DEUFields()
            //{
            //    CurrentUser = CurrentUser,
            //    LicenseeId = LicenseeId,
            //   DeuEntryId = GetDEUEntryId(PolicyPaymentEntryId),
            // };
            // DEU _DEU=DEU.GetDeuEntryidWise(GetDEUEntryId(PolicyPaymentEntryId));

            //DEULearnedPost.AddDataDeuToLearnedPost( _DEU);//Need to verify
            //LearnedToPolicyPost.AddUpdateLearnedToPolicy(_DEU.PolicyId);
            //PolicyToLearnPost.AddUpdatPolicyToLearn(_DEU.PolicyId);
        }

        private static Guid GetDEUEntryId(Guid PolicyPaymentEntryId)
        {
            Guid DEUEntryId = Guid.Empty;
            try
            {
                PolicyPaymentEntriesPost objPolicyPaymentEntriesPost = PolicyPaymentEntriesPost.GetPolicyPaymentEntry(PolicyPaymentEntryId);
                if (objPolicyPaymentEntriesPost != null)
                {
                    DEUEntryId = (Guid)objPolicyPaymentEntriesPost.DEUEntryId;
                }

                if (DEUEntryId == null)
                {
                    return Guid.Empty;
                }
            }
            catch
            {
            }
            return DEUEntryId;
        }

        private static void RemoveLearnedPolicy(Guid PolicyId)
        {
            try
            {
                PolicyLearnedField.DeleteLearnedHistory(PolicyId);
                PolicyLearnedField.DeleteByPolicy(PolicyId);
            }
            catch
            {
            }
        }

        private static void RemoveFollowUp(Guid PolicyId)
        {
            FollowupIssue.DeleteFollowupByPolicyId(PolicyId);
        }

        private static void AddRecordWithMinusAmountInPaymentOutgoingEntries(Guid PaymentEntryId, Guid PendingPolicyId, Guid ActivePolicyId)
        {
            List<PolicyOutgoingDistribution> _PolicyOutgoingDistributionLst = PolicyOutgoingDistribution.GetOutgoingPaymentByPoicyPaymentEntryId(PaymentEntryId);
            PolicyOutgoingDistribution _PolicyOutgoingDistribution = _PolicyOutgoingDistributionLst.First();
            PolicyOutgoingDistribution _NewPolicyOutgoingDistribution = new PolicyOutgoingDistribution()
            {
                OutgoingPaymentId = Guid.NewGuid(),
                PaymentEntryId = _PolicyOutgoingDistribution.PaymentEntryId,
                RecipientUserCredentialId = _PolicyOutgoingDistribution.RecipientUserCredentialId,
                PaidAmount = _PolicyOutgoingDistribution.PaidAmount * (-1),
                CreatedOn = _PolicyOutgoingDistribution.CreatedOn,
                // ReferencedOutgoingScheduleId = _PolicyOutgoingDistribution.ReferencedOutgoingScheduleId,
                // ReferencedOutgoingAdvancedScheduleId = _PolicyOutgoingDistribution.ReferencedOutgoingAdvancedScheduleId,
                IsPaid = false,

            };
            PolicyOutgoingDistribution.AddUpdateOutgoingPaymentEntry(_NewPolicyOutgoingDistribution);
        }

        private static void UpdateBatchData(Guid PolicyEntryId)
        {
            Batch objBatch = new Batch();
            //Batch _Batch = Batch.GetBatchEntryViaEntryId(PolicyEntryId);
            Batch _Batch = objBatch.GetBatchEntryViaEntryId(PolicyEntryId);
            if (_Batch.EntryStatus == EntryStatus.Paid)
            {
                _Batch.EntryStatus = EntryStatus.PartialUnpaid;
                _Batch.AddUpdate();
            }
        }

        /// <summary>
        /// Remove Client from id if it not associate from any policy--Client will never remove physically .only pne flag IsDelete is set for this
        /// </summary>
        /// <param name="ClientId"></param>
        private static void RemoveClient(Guid ClientId, Guid LicenseeId)
        {
            PostUtill.RemoveClient(ClientId, LicenseeId);
        }

        private static void RemovePendingPolicy(Guid PolicyId)
        {
            try
            {
                PolicyDetailsData _Policy = new PolicyDetailsData()
                {
                    PolicyId = PolicyId,
                };
                Policy.DeletePolicyHistoryPermanentById(_Policy);
                Policy.DeletePolicyFromDB(_Policy);
            }
            catch
            {

            }
        }

        private static void DistributeAmountToPayee(Guid PolicyPaymentEntryId)
        {
            PolicyPaymentEntriesPost _PolicyPaymentEntriesPost = GetPayemntEntry(PolicyPaymentEntryId);//Get record of policy via entryId
            Guid PolicyId = _PolicyPaymentEntriesPost.PolicyID;
            DateTime? InvoiceDt = _PolicyPaymentEntriesPost.InvoiceDate;
            Guid LicenseeId = Guid.Empty;//No need it becoz it is use for HO n for here HO should be false
            PostUtill.EntryInPolicyOutGoingPayment(false, PolicyPaymentEntryId, PolicyId, InvoiceDt, LicenseeId);
        }

        //private static void DetectedAmountFromWrongActive(Guid guid, Guid guid1)
        //{
        //    throw new NotImplementedException();
        //}

        private static PolicyPaymentEntriesPost GetPayemntEntry(Guid PolicyEntryId)
        {
            return PolicyPaymentEntriesPost.GetPolicyPaymentEntry(PolicyEntryId);
        }

        // private static void ChangeInPaymentOutGoingEntries(Guid PolicyEntryId, Guid PendingPolicyId, Guid ActivePolicyId,PolicySchedule _PolicySchedule)
        private static void ChangeInPaymentOutGoingEntries(Guid PolicyEntryId, Guid PendingPolicyId, Guid ActivePolicyId)
        {
            List<PolicyOutgoingDistribution> _PolicyOutgoingDistributionLst = PolicyOutgoingDistribution.GetOutgoingPaymentByPoicyPaymentEntryId(PolicyEntryId);
            PolicyOutgoingDistribution _PolicyOutgoingDistribution = _PolicyOutgoingDistributionLst.FirstOrDefault();

        }

        private static void ChangeInPaymentIncomingEntries(Guid PolicyEntryId, Guid PendingPolicyId, Guid ActivePolicyId)
        {
            try
            {
                PolicyPaymentEntriesPost _PolicyPaymentEntriesPost = PolicyPaymentEntriesPost.GetPolicyPaymentEntry(PolicyEntryId);

                _PolicyPaymentEntriesPost.PolicyID = ActivePolicyId;
                //Set true when link the payment from comp manager
                _PolicyPaymentEntriesPost.IsLinkPayment = true;
                PolicyPaymentEntriesPost.AddUpadate(_PolicyPaymentEntriesPost);
                //Update DEU--3-5-2011
                DEU _DEU = DEU.GetDeuEntryidWise(_PolicyPaymentEntriesPost.DEUEntryId.Value);
                _DEU.PolicyId = ActivePolicyId;
                _DEU.ClientID = PostUtill.GetPolicy(ActivePolicyId).ClientId;
                //DEU.AddupdateDeuEntry(_DEU);               
                DEU objDEU = new DEU();
                objDEU.AddupdateDeuEntry(_DEU);
            }
            catch
            {
            }
        }

        public static bool ScheduleMatches(Guid EntryId, Guid ActivePolicyId)
        {
            bool Flag = PostUtill.CheckForOutgoingScheduleVariance(EntryId, ActivePolicyId);
            return Flag;
        }
        
        private static bool PolicyPaidMarked()
        {
            throw new NotImplementedException();
        }

        private static bool GetAgencyVersion()
        {
            throw new NotImplementedException();
        }
    }


}
