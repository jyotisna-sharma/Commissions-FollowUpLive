using System;
using System.Collections.Generic;
using System.Linq;
using MyAgencyVault.BusinessLibrary.Masters;
using System.Runtime.Serialization;
using DLinq = DataAccessLayer.LinqtoEntity;
using System.Transactions;
using System.Globalization;
using System.Threading;
using System.Linq.Expressions;
using MyAgencyVault.BusinessLibrary.Base;
using System.Reflection;
using System.ComponentModel;
using System.Data.EntityClient;
using System.Data.SqlClient;
using System.Data;

namespace MyAgencyVault.BusinessLibrary
{
    [DataContract]
    public class PostProcessReturnStatus
    {
        [DataMember]
        public Guid DeuEntryId { get; set; }
        [DataMember]
        public Guid OldDeuEntryId { get; set; }
        [DataMember]
        public bool IsComplete { get; set; }
        [DataMember]
        public string ErrorMessage { get; set; }
        [DataMember]
        public PostEntryProcess PostEntryStatus { get; set; }
        [DataMember]
        public ModifiyableBatchStatementData BatchStatementData { get; set; }
        [DataMember]
        public bool IsClientDeleted { get; set; }
        [DataMember]
        public int ReferenceNo { get; set; }
    }

    [DataContract]
    public enum DataType
    {
        [EnumMember]
        Date = 1,
        [EnumMember]
        Numeric = 2,
        [EnumMember]
        Text = 3
    }

    public class BasicInformationForProcess
    {
        public PostStatus PostStatus { get; set; }
        public bool IsPaymentToHO { get; set; }
        public Guid PolicyId { get; set; }
        public string message { get; set; }
        public bool IsAgencVersion { get; set; }
    }

    [DataContract]
    public class DEUFields
    {
        [DataMember]
        public Guid DeuEntryId { get; set; }
        [DataMember]
        public List<DataEntryField> DeuFieldDataCollection { get; set; }//it is not used by PostProcess.it is only for DEU table datasave from DEU.
        [DataMember]
        public List<DeuSearchedPolicy> SearchedPolicyList { get; set; }
        [DataMember]
        public DeuSearchedPolicy SelectedPolicy { get; set; }
        [DataMember]
        public Guid? PayorId { get; set; }
        [DataMember]
        public Guid BatchId { get; set; }
        [DataMember]
        public Guid? LicenseeId { get; set; }
        [DataMember]
        public Guid StatementId { get; set; }
        [DataMember]
        public Guid CurrentUser { get; set; }//it is the ID of DEU.
        [DataMember]
        public DEU DeuData { get; set; }
        [DataMember]
        public int ReferenceNo { get; set; }
        [DataMember]
        public Guid? TemplateID { get; set; }

    }

    [DataContract]
    public class UniqueIdenitfier
    {
        [DataMember]
        public string ColumnName { get; set; }
        [DataMember]
        public string Text { get; set; }
        [DataMember]
        public string MaskedText { get; set; }

    }

    //deuFields have searchedPolicy
    public class PostUtill
    {
        //public static Dictionary<Guid, object> LockDic = new Dictionary<Guid, object>();
        public static DEU GetDeuCollection(PolicyPaymentEntriesPost PolicySelectedIncomingPaymentCommissionDashBoard, PolicyDetailsData SelectedPolicy)
        {
            PolicyLearnedFieldData _poliLrnd = PolicyLearnedField.GetPolicyLearnedFieldsPolicyWise(SelectedPolicy.PolicyId);
            DEU _DEU = new DEU();
            try
            {
                _DEU.DEUENtryID = Guid.NewGuid();
                //_DEU.OriginalEffectiveDate = SelectedPolicy.OriginalEffectiveDate;
                _DEU.OriginalEffectiveDate = _poliLrnd.Effective;
                _DEU.PaymentRecived = PolicySelectedIncomingPaymentCommissionDashBoard.PaymentRecived;
                _DEU.CommissionPercentage = PolicySelectedIncomingPaymentCommissionDashBoard.CommissionPercentage;
                _DEU.Insured = _poliLrnd.Insured;
                _DEU.PolicyNumber = SelectedPolicy.PolicyNumber;
                _DEU.Enrolled = SelectedPolicy.Enrolled;
                _DEU.Eligible = SelectedPolicy.Eligible;
                _DEU.SplitPer = PolicySelectedIncomingPaymentCommissionDashBoard.SplitPer;
                _DEU.PolicyMode = _poliLrnd.PolicyModeId;
                _DEU.TrackFromDate = _poliLrnd.TrackFrom;
                _DEU.CompTypeID = _poliLrnd.CompTypeId;
                _DEU.ClientID = _poliLrnd.ClientID;
                _DEU.StmtID = PolicySelectedIncomingPaymentCommissionDashBoard.StmtID;
                _DEU.PolicyId = SelectedPolicy.PolicyId;
                _DEU.InvoiceDate = PolicySelectedIncomingPaymentCommissionDashBoard.InvoiceDate;
                _DEU.PayorId = SelectedPolicy.PayorId;
                _DEU.NoOfUnits = PolicySelectedIncomingPaymentCommissionDashBoard.NumberOfUnits;
                _DEU.DollerPerUnit = PolicySelectedIncomingPaymentCommissionDashBoard.DollerPerUnit;
                _DEU.Fee = PolicySelectedIncomingPaymentCommissionDashBoard.Fee;
                _DEU.Bonus = PolicySelectedIncomingPaymentCommissionDashBoard.Bonus;
                _DEU.CommissionTotal = PolicySelectedIncomingPaymentCommissionDashBoard.TotalPayment;
                _DEU.CarrierID = _poliLrnd.CarrierId;
                _DEU.CoverageID = _poliLrnd.CoverageId;
                _DEU.IsEntrybyCommissiondashBoard = true;
                _DEU.CreatedBy = PolicySelectedIncomingPaymentCommissionDashBoard.CreatedBy;
                _DEU.PostCompleteStatus = 0;
            }
            catch (Exception ex)
            {
                ActionLogger.Logger.WriteLog("GetDeuCollection :" + ex.StackTrace.ToString(), true);
            }
            return _DEU;

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="_PostEntryProcess"></param>
        /// <param name="DeuEntryId"></param>
        /// <param name="RepostNewDeuEntryId"></param>
        /// <param name="_UserRole"></param>
        /// <returns></returns>
        //public static PostProcessReturnStatus PostStartWrapper(PostEntryProcess _PostEntryProcess, Guid DeuEntryId, Guid RepostNewDeuEntryId, UserRole _UserRole)
        //{
        //    bool lockObtained = false;

        //    DEUFields deuFields = null;
        //    deuFields = FillDEUFields(DeuEntryId);
        //    BasicInformationForProcess _BasicInformationForProcess = PostUtill.GetPolicyToProcess(deuFields);

        //    lockObtained = PolicyLocking.LockPolicy(_BasicInformationForProcess.PolicyId);

        //    while (!lockObtained)
        //    {
        //        Thread.Sleep(100);
        //        lockObtained = PolicyLocking.LockPolicy(_BasicInformationForProcess.PolicyId);
        //    }

        //    try
        //    {
        //        return PostStart(_PostEntryProcess, DeuEntryId, RepostNewDeuEntryId, _UserRole);
        //    }
        //    finally
        //    {
        //        PolicyLocking.UnlockPolicy(_BasicInformationForProcess.PolicyId);
        //    }
        //}

        /// <summary>
        /// From Here Post Process Start 
        /// this is called afetr DEU data is saved in DEU and validate
        /// </summary>
        /// <param name="_PostEntryProcess"></param>
        /// <param name="DeuEntryId"></param>
        /// <param name="RepostNewDeuEntryId">Use Incase of Repost</param>
        /// <returns>Return the Post Information after Post</returns>
        public static PostProcessReturnStatus PostStart(PostEntryProcess _PostEntryProcess, Guid DeuEntryId, Guid RepostNewDeuEntryId, Guid UserId, UserRole _UserRole, PostEntryProcess _ActualPostEntryStatus, string strUnlinkClient, string Operation)
        {
            ActionLogger.Logger.WriteLog("POstUtil.PostStart :_PostEntryProcess: " + _PostEntryProcess + ", DeuEntryId " + DeuEntryId, true);
            bool isException = false;
            PostProcessReturnStatus _PostProcessReturnStatus = new PostProcessReturnStatus() { DeuEntryId = DeuEntryId, IsComplete = false, ErrorMessage = null, PostEntryStatus = _PostEntryProcess, IsClientDeleted = false };
            DEUFields deuFields = null;
            //var options = new TransactionOptions
            //{
            //    IsolationLevel = IsolationLevel.ReadCommitted,
            //    Timeout = TimeSpan.FromMinutes(60)
            //};
            try
            {

                if (_PostEntryProcess == PostEntryProcess.FirstPost)
                {

                    DEU objDEU = new DEU();
                    deuFields = FillDEUFields(DeuEntryId);

                    BasicInformationForProcess _BasicInformationForProcess = PostUtill.GetPolicyToProcess(deuFields, strUnlinkClient);
                    if (_BasicInformationForProcess == null)
                    {
                        return _PostProcessReturnStatus;
                    }
                    PolicyDetailsData policytoDeu = GetPolicy(_BasicInformationForProcess.PolicyId);
                    deuFields.DeuData.PolicyId = _BasicInformationForProcess.PolicyId;
                    deuFields.DeuData.PostStatusID = (int)_BasicInformationForProcess.PostStatus;
                    deuFields.DeuData.ClientID = (deuFields.DeuData.ClientID == null || deuFields.DeuData.ClientID == Guid.Empty) ? policytoDeu.ClientId : deuFields.DeuData.ClientID;
                    Client objClient = Client.GetClient(deuFields.DeuData.ClientID.Value);
                    if (objClient != null)
                    {
                        deuFields.DeuData.ClientName = objClient.Name;
                    }
                    else
                    {
                        deuFields.DeuData.ClientName = strUnlinkClient;
                    }

                    //deuFields.DeuData.ClientName = Client.GetClient(deuFields.DeuData.ClientID.Value).Name;
                    deuFields.DeuData.PostCompleteStatus = (int)PostCompleteStatusEnum.InProgress;
                    deuFields.DeuData.Insured = string.IsNullOrEmpty(deuFields.DeuData.Insured) ? policytoDeu.Insured : deuFields.DeuData.Insured;

                    objDEU.AddupdateDeuEntry(deuFields.DeuData);
                    _PostProcessReturnStatus = ProcessSearchedPoilcy(deuFields, _BasicInformationForProcess, _PostProcessReturnStatus);
                    if (_PostProcessReturnStatus.IsComplete)
                    {
                       // ActionLogger.Logger.WriteLog("Sucess commited:", true);
                    }
                    else
                    {
                        if (deuFields != null)
                        {
                           // ActionLogger.Logger.WriteLog("Rollback id:" + deuFields.DeuEntryId.ToString(), true);
                        }
                    }

                    if (_PostProcessReturnStatus.ErrorMessage == "Payment is not Postable (Agency version and Outgoing Schedule is not Distributable)")
                    {
                        throw new Exception("Payment is not Postable");
                    }

                    if (deuFields.DeuData.IsEntrybyCommissiondashBoard)
                    {
                        DEU de = DEU.GetDeuEntryidWise(_PostProcessReturnStatus.DeuEntryId);
                        de.PostCompleteStatus = (int)PostCompleteStatusEnum.Unsuccessful;
                        DEU objdeu = new DEU();
                        objdeu.AddupdateDeuEntry(de);
                    }

                    _PostProcessReturnStatus.IsComplete = true;

                    if (deuFields.DeuData.IsEntrybyCommissiondashBoard == false)
                    {
                        bool IsTrackPayment = true;
                        PolicyDetailsData objPolicyDetailsData = GetPolicy(deuFields.DeuData.PolicyId);
                        if (objPolicyDetailsData != null)
                        {
                            IsTrackPayment = objPolicyDetailsData.IsTrackPayment;
                        }

                     //Acme   FollowUpUtill.FollowUpProcedure(FollowUpRunModules.PaymentEntered, deuFields.DeuData, deuFields.DeuData.PolicyId, IsTrackPayment, deuFields.DeuData.IsEntrybyCommissiondashBoard, _UserRole, null);
                        // count more then 50 payments
                        ////ansh //Need to update code and remove threading from here 
                        //List<PolicyPaymentEntriesPost> _AllPaymentEntriesOnPolicyFormissing = PolicyPaymentEntriesPost.GetPolicyPaymentEntryPolicyIDWise(deuFields.DeuData.PolicyId);

                        //if (_AllPaymentEntriesOnPolicyFormissing.Count < 50)
                        //{
                        //    FollowUpUtill.FollowUpProcedure(FollowUpRunModules.PaymentEntered, deuFields.DeuData, deuFields.DeuData.PolicyId, IsTrackPayment, deuFields.DeuData.IsEntrybyCommissiondashBoard, _UserRole, null);
                        //}
                        //else
                        //{
                        //    Thread ThreadfollowupProcedure = new Thread(() =>
                        //    {
                        //        FollowUpUtill.FollowUpProcedure(FollowUpRunModules.PaymentEntered, deuFields.DeuData, deuFields.DeuData.PolicyId, IsTrackPayment, deuFields.DeuData.IsEntrybyCommissiondashBoard, _UserRole, null);
                        //    });
                        //    ThreadfollowupProcedure.IsBackground = true;
                        //    ThreadfollowupProcedure.Start();
                        //}
                    }
                    else
                    {
                   //Acme     FollowUpUtill.FollowUpProcedure(FollowUpRunModules.PaymentEntered, deuFields.DeuData, deuFields.DeuData.PolicyId, GetPolicy(deuFields.DeuData.PolicyId).IsTrackPayment, deuFields.DeuData.IsEntrybyCommissiondashBoard, _UserRole, null);

                    }

                    if (UserId != Guid.Empty)
                    {
                        DEU objdeu = new DEU();
                        _PostProcessReturnStatus.BatchStatementData = objdeu.UpdateBatchStatementDataOnSuccessfullDeuPost(DeuEntryId, UserId);
                    }


                    try
                    {
                        if (deuFields != null)
                        {
                            //Thread ThreadUpdateEntry = new Thread(() =>
                            //{
                            //    DEU.UpdateTotalAmountAndEntry(deuFields.StatementId, deuFields.DeuEntryId);
                            //});
                            //ThreadUpdateEntry.Start();
                            DEU.UpdateTotalAmountAndEntry(deuFields.StatementId, deuFields.DeuEntryId);
                        }
                    }
                    catch { }


                }
                else if (_PostEntryProcess == PostEntryProcess.RePost)
                {
                    if (RepostNewDeuEntryId != null && RepostNewDeuEntryId != Guid.Empty)
                    {
                        PostProcessReturnStatus _tempPostProcessReturnStatus = PostStart(PostEntryProcess.Delete, DeuEntryId, RepostNewDeuEntryId, UserId, _UserRole, _ActualPostEntryStatus, string.Empty, string.Empty);
                        if (_tempPostProcessReturnStatus.IsComplete)
                        {
                            _tempPostProcessReturnStatus = PostStart(PostEntryProcess.FirstPost, RepostNewDeuEntryId, Guid.Empty, UserId, _UserRole, _ActualPostEntryStatus, string.Empty, string.Empty);
                            _PostProcessReturnStatus = _tempPostProcessReturnStatus;
                            _PostProcessReturnStatus.PostEntryStatus = PostEntryProcess.RePost;
                        }
                    }
                    else if (RepostNewDeuEntryId == null || RepostNewDeuEntryId == Guid.Empty)
                    {
                        _PostProcessReturnStatus.ErrorMessage = "Repost Id is Empty";
                        _PostProcessReturnStatus.IsComplete = false;
                        _PostProcessReturnStatus.DeuEntryId = RepostNewDeuEntryId;
                    }
                    if (DEU.GetDeuEntryidWise(_PostProcessReturnStatus.DeuEntryId).IsEntrybyCommissiondashBoard)
                    {
                        DEU de = DEU.GetDeuEntryidWise(_PostProcessReturnStatus.DeuEntryId);
                        de.PostCompleteStatus = (int)PostCompleteStatusEnum.Unsuccessful;
                        DEU objDEU = new DEU();
                        objDEU.AddupdateDeuEntry(de);

                        try
                        {
                            if (deuFields != null)
                            {
                                //Thread ThreadUpdateEntry = new Thread(() =>
                                //{
                                //    DEU.UpdateTotalAmountAndEntry(deuFields.StatementId, deuFields.DeuEntryId);
                                //});
                                //ThreadUpdateEntry.Start();

                                DEU.UpdateTotalAmountAndEntry(deuFields.StatementId, deuFields.DeuEntryId);
                            }

                        }
                        catch
                        {
                        }
                    }
                    _PostProcessReturnStatus.ErrorMessage = "Null";
                    _PostProcessReturnStatus.IsComplete = true;
                }
                else if (_PostEntryProcess == PostEntryProcess.Delete)
                {
                    bool flag = false;
                    PolicyPaymentEntriesPost PolicySelectedIncomingPaymenttemp = PolicyPaymentEntriesPost.GetPolicyPaymentEntryDEUEntryIdWise(DeuEntryId);
                    if (PolicySelectedIncomingPaymenttemp == null)//post enrty was unsuccessful
                    {
                        ActionLogger.Logger.WriteLog("before post enrty was unsuccessfull then need to delete DEUEntryID" + DeuEntryId.ToString(), true);

                        _PostProcessReturnStatus.BatchStatementData = DEU.DeleteDeuEntry(DeuEntryId);
                        _PostProcessReturnStatus.IsComplete = true;
                        _PostProcessReturnStatus.ErrorMessage = "DEU Entry Deleted";
                        ActionLogger.Logger.WriteLog("After post enrty was unsuccessfull then need to delete ", true);

                        DEU objDEU = new DEU();
                        objDEU.DeleteDeuEntryByID(DeuEntryId);
                        return _PostProcessReturnStatus;
                    }

                    deuFields = FillDEUFields(DeuEntryId);
                    flag = PolicyOutgoingDistribution.IsEntryMarkPaid(PolicySelectedIncomingPaymenttemp.PaymentEntryID);

                    if (flag == true)
                    {
                        _PostProcessReturnStatus.IsComplete = false;
                        _PostProcessReturnStatus.ErrorMessage = "Paid/Semi Paid Entry";
                        return _PostProcessReturnStatus;
                    }

                    //Delete Policy Outgoing payment
                    PolicyOutgoingDistribution.DeleteByPolicyIncomingPaymentId(PolicySelectedIncomingPaymenttemp.PaymentEntryID);
                    //Delete Policypayment entry and Followup
                    FollowupIssue.DeletePolicyPayment_And_FollowUpbyPolicyPaymentEntryId(PolicySelectedIncomingPaymenttemp.PaymentEntryID, _ActualPostEntryStatus);
                    //FollowupIssue.DeleteFollowUpbyPolicyPaymentEntryId(PolicySelectedIncomingPaymenttemp.PaymentEntryID);
                    //PolicyPaymentEntriesPost.DeletePolicyPayentIdWise(PolicySelectedIncomingPaymenttemp.PaymentEntryID);
                    //Open an missing issue
                    FollowUpUtill.OpenMissisngIssueIfAny(PolicySelectedIncomingPaymenttemp.InvoiceDate, PolicySelectedIncomingPaymenttemp.PolicyID);
                    //Delete Deu Entry
                    _PostProcessReturnStatus.BatchStatementData = DEU.DeleteDeuEntry(deuFields.DeuEntryId);
                    //it is for updating the smart field
                    //List<DEU> _DEULst = DEU.GetDEUPolicyIdWise(PolicySelectedIncomingPaymenttemp.PolicyID);
                    DEU objDeu = new DEU();
                    List<DEU> _DEULst = objDeu.GetDEUPolicyIdWise(PolicySelectedIncomingPaymenttemp.PolicyID);

                    if (_DEULst != null && _DEULst.Count != 0)
                    {
                        try
                        {
                            DEU _DEU = _DEULst.Where(p => p.InvoiceDate == _DEULst.Max(p1 => p1.InvoiceDate)).FirstOrDefault();
                            DEUFields _DEUFields = PostUtill.FillDEUFields(_DEU.DEUENtryID);
                            DEU _LatestDEUrecord = objDeu.GetLatestInvoiceDateRecord(deuFields.DeuData.PolicyId);
                            if (_LatestDEUrecord != null)
                            {
                                Guid PolicyId = DEULearnedPost.AddDataDeuToLearnedPost(_LatestDEUrecord);
                                LearnedToPolicyPost.AddUpdateLearnedToPolicy(PolicyId);
                                PolicyToLearnPost.AddUpdatPolicyToLearn(PolicyId);
                            }

                        }
                        catch (Exception ex)
                        {
                            //ActionLogger.Logger.WriteLog("Issue in AddDataDeuToLearnedPost,AddUpdateLearnedToPolicy,AddUpdatPolicyToLearn block in Poststart function", true);
                            ActionLogger.Logger.WriteLog(ex.StackTrace.ToString(), true);
                        }

                    }
                    else
                    {
                        try
                        {
                            PolicyDetailsData ppolicy = GetPolicy(PolicySelectedIncomingPaymenttemp.PolicyID);
                            if (ppolicy == null)
                            {
                                _PostProcessReturnStatus.IsClientDeleted = true;
                            }
                            else if (ppolicy.PolicyStatusId == (int)_PolicyStatus.Pending)
                            {
                                try
                                {
                                    Policy.DeletePolicyCascadeFromDBById(ppolicy.PolicyId);
                                }
                                catch (Exception ex)
                                {
                                    ActionLogger.Logger.WriteLog("issue in function DeletePolicyCascadeFromDBById :" + ex.StackTrace.ToString(), true);
                                }
                                RemoveClient(ppolicy.ClientId ?? Guid.Empty, ppolicy.PolicyLicenseeId ?? Guid.Empty);
                                _PostProcessReturnStatus.IsClientDeleted = true;

                            }
                            else
                            {
                                PolicyDetailsData Policyhistory = Policy.GetPolicyHistoryIdWise(PolicySelectedIncomingPaymenttemp.PolicyID);
                                if (Policyhistory != null)
                                    Policy.AddUpdatePolicy(Policyhistory);

                                PolicyLearnedFieldData _PolicyLearnedField = PolicyLearnedField.GetPolicyLearnedFieldsHistoryPolicyWise(PolicySelectedIncomingPaymenttemp.PolicyID);
                                if (_PolicyLearnedField != null)
                                    PolicyLearnedField.AddUpdateLearned(_PolicyLearnedField, _PolicyLearnedField.ProductType);
                            }
                        }
                        catch (Exception ex)
                        {
                            ActionLogger.Logger.WriteLog("1 :" + ex.StackTrace.ToString(), true);
                            ActionLogger.Logger.WriteLog("1 :" + ex.InnerException.ToString(), true);
                        }
                    }

                    try
                    {
                        PolicyDetailsData FollPolicy = GetPolicy(deuFields.DeuData.PolicyId);
                        if (FollPolicy != null)
                        {
                          //Acme  FollowUpUtill.FollowUpProcedure(FollowUpRunModules.PaymentDeleted, null, deuFields.DeuData.PolicyId, FollPolicy.IsTrackPayment, deuFields.DeuData.IsEntrybyCommissiondashBoard, _UserRole, null);
                        }

                        _PostProcessReturnStatus.ErrorMessage = "Null";

                        if (deuFields != null && deuFields.DeuData != null)
                            deuFields.DeuData.PostCompleteStatus = (int)PostCompleteStatusEnum.Successful;
                    }
                    catch (Exception ex)
                    {
                        ActionLogger.Logger.WriteLog("test :" + ex.StackTrace.ToString(), true);
                        ActionLogger.Logger.WriteLog("test :" + ex.InnerException.ToString(), true);
                    }

                    _PostProcessReturnStatus.IsComplete = true;

                }
            }
            catch (Exception ex)
            {
                FollowUpUtill.IsCallForDelete = false;
                isException = true;
                ActionLogger.Logger.WriteLog("Issue while posting payment in post start function", true);
                ActionLogger.Logger.WriteLog(ex.StackTrace.ToString(), true);
            }

            if (isException)
            {
                _PostProcessReturnStatus.IsComplete = true;
            }
            return _PostProcessReturnStatus;
        }

        public static void RemoveClient(Guid ClientId, Guid LicenseeId)
        {
            if (ClientId == Guid.Empty || LicenseeId == Guid.Empty) return;

            try
            {
                Dictionary<string, object> parameters = new Dictionary<string, object>();
                parameters.Add("PolicyLicenseeId", LicenseeId);
               // parameters.Add("IsDeleted", false);
                parameters.Add("PolicyClientId", ClientId);
                int num = Policy.GetPolicyData(parameters).ToList().Count;
                if (num > 0)
                    return;
                parameters = new Dictionary<string, object>();
                parameters.Add("PolicyLicenseeId", LicenseeId);
                parameters.Add("PolicyStatusId", (int)_PolicyStatus.Delete);
                parameters.Add("PolicyClientId", ClientId);
                List<PolicyDetailsData> deletedpolicy = Policy.GetPolicyData(parameters).ToList();
                deletedpolicy.ForEach(p => Policy.DeletePolicyCascadeFromDBById(p.PolicyId));
                Client _Client = new Client()
                {
                    ClientId = ClientId,
                };
                Client.DeleteClient(_Client);
            }
            catch (Exception ex)
            {
                ActionLogger.Logger.WriteLog("RemoveClient :" + ex.StackTrace.ToString(), true);
                ActionLogger.Logger.WriteLog("RemoveClient :" + ex.InnerException.ToString(), true);
            }
        }
        /// <summary>
        /// This fill DEU Data For Post Process
        /// </summary>
        /// <param name="DeuEntryId"></param>
        /// <returns>Return the DEU Collection</returns>
        public static DEUFields FillDEUFields(Guid DeuEntryId)
        {
            DEUFields deufields = new DEUFields();
            try
            {
                DEU _DEU = DEU.GetDeuEntryidWise(DeuEntryId);
                deufields.DeuEntryId = DeuEntryId;
                if (_DEU == null)
                    return deufields;

                deufields.StatementId = _DEU.StmtID ?? Guid.Empty;
                deufields.PayorId = _DEU.PayorId;
                deufields.TemplateID = _DEU.TemplateID;
                if (deufields.StatementId != Guid.Empty)
                {
                    deufields.BatchId = Statement.GetStatement(deufields.StatementId).BatchId;
                }

                if (deufields.BatchId != Guid.Empty)
                {
                    Batch objBatch = new Batch();
                    deufields.LicenseeId = objBatch.GetBatchViaBatchId(deufields.BatchId).LicenseeId;
                }

                deufields.CurrentUser = _DEU.CreatedBy ?? Guid.Empty;
                deufields.DeuData = _DEU;
                if (_DEU.IsEntrybyCommissiondashBoard)
                {
                    deufields.SearchedPolicyList = GetDeuSearchedPolicyviaPolicyId(_DEU.PolicyId);
                }
                else
                {
                    List<UniqueIdenitfier> _UniqueIndenties = GetUniqueIdentifierForPayor(deufields.PayorId, deufields.TemplateID, DeuEntryId);//It is to be check
                    if (_UniqueIndenties.Count > 0)
                    {
                        deufields.SearchedPolicyList = GetPoliciesFromUniqueIdentifier(_UniqueIndenties, deufields.LicenseeId ?? Guid.Empty, deufields.PayorId ?? Guid.Empty);
                        if (deufields.SearchedPolicyList.Count() == 1)
                        {
                            //Added
                            if (string.IsNullOrEmpty(deufields.SearchedPolicyList.FirstOrDefault().ClientName))
                            {

                                try
                                {
                                    if (deufields.DeuData.ClientID == Guid.Empty)
                                    {
                                        Guid clientID = Client.AddUpdateClient(Convert.ToString(deufields.DeuData.ClientName), (Guid)deufields.LicenseeId, Guid.NewGuid());
                                        Policy.UpdatePolicyClient(deufields.SearchedPolicyList.FirstOrDefault().PolicyId, clientID);
                                        Policy.UpdatePolicyClientLernedFields(deufields.SearchedPolicyList.FirstOrDefault().PolicyId, clientID);
                                    }

                                }
                                catch
                                {
                                }
                            }

                        }

                    }
                }
            }
            catch (Exception ex)
            {
                ActionLogger.Logger.WriteLog("Issue in FillDEUFields:  " + ex.StackTrace.ToString(), true);
                ActionLogger.Logger.WriteLog("Issue in FillDEUFields:  " + ex.InnerException.ToString(), true);
            }

            return deufields;
        }
        /// <summary>
        /// Get the DEU Searched Policy,it is called in the case of commission dashboard so it must have single policy
        /// </summary>
        /// <param name="PolicyId"></param>
        /// <returns>return the collection of DEUSearchedPolicy</returns>
        private static List<DeuSearchedPolicy> GetDeuSearchedPolicyviaPolicyId(Guid PolicyId)
        {
            List<DeuSearchedPolicy> _PolicyLst = new List<DeuSearchedPolicy>();
            try
            {
                PolicyDetailsData poli = GetPolicy(PolicyId);
                DeuSearchedPolicy dsp = new DeuSearchedPolicy();
                dsp.CarrierName = poli.CarrierName;
                dsp.ClientName = poli.ClientName;
                dsp.CompType = PolicyIncomingPaymentType.GetIncomingPaymentTypeList().Where(p => p.PaymentTypeId == poli.IncomingPaymentTypeId.Value).FirstOrDefault().PaymenProcedureName;
                dsp.Insured = poli.Insured;
                dsp.LastModifiedDate = poli.CreatedOn.Value;
                dsp.PaymentMode = poli.PolicyModeId;
                dsp.PolicyId = poli.PolicyId;
                dsp.PolicyNumber = poli.PolicyNumber;
                dsp.PolicyStatus = poli.PolicyStatusId ?? 0;
                dsp.ProductName = poli.CoverageName;
                _PolicyLst.Add(dsp);
            }
            catch (Exception ex)
            {
                ActionLogger.Logger.WriteLog("GetDeuSearchedPolicyviaPolicyId :" + ex.StackTrace.ToString(), true);
                ActionLogger.Logger.WriteLog("GetDeuSearchedPolicyviaPolicyId :" + ex.InnerException.ToString(), true);
            }
            return _PolicyLst;
        }

        /// <summary>
        /// Get the Unique Identifier int the DEU Form For Particuler Payor
        /// </summary>
        /// <param name="PayorId"></param>
        /// <param name="DeuEntryId"></param>
        /// <returns></returns>
        private static List<UniqueIdenitfier> GetUniqueIdentifierForPayor(Guid? PayorId, Guid? TemplateID, Guid DeuEntryId)
        {
            List<UniqueIdenitfier> _UniqueIndenties = new List<UniqueIdenitfier>();

            try
            {
                if (PayorId == null)
                {
                    return _UniqueIndenties;
                }

                PayorTool PayorTool = PayorTool.GetPayorToolMgr((Guid)PayorId, TemplateID);
                if (PayorTool != null)
                {
                    List<PayorToolField> _PayorToolField = PayorTool.ToolFields.Where(p => p.IsPartOfPrimaryKey == true).OrderBy(p => p.EquivalentDeuField).ToList();
                    DEU _DEU = DEU.GetDeuEntryidWise(DeuEntryId);
                    foreach (PayorToolField ptf in _PayorToolField)
                    {
                        UniqueIdenitfier _UniqueIdenitfier = new UniqueIdenitfier();

                        _UniqueIdenitfier.ColumnName = ptf.EquivalentDeuField;
                        _UniqueIdenitfier.MaskedText = ptf.MaskText;
                        if (_DEU != null)
                        {
                            _UniqueIdenitfier.Text = GetValueFromDEUTableForAColumn(_DEU, _UniqueIdenitfier.ColumnName);
                        }
                        if (_UniqueIdenitfier != null)
                        {
                            _UniqueIndenties.Add(_UniqueIdenitfier);
                        }
                    }
                }
                else
                {
                    List<ImportToolPaymentDataFieldsSettings> objImportToolPaymentDataSettings = new List<ImportToolPaymentDataFieldsSettings>();
                    PayorTemplate objPayorTemplateCode = new PayorTemplate();
                    if (PayorId != null && TemplateID != null)
                    {
                        objImportToolPaymentDataSettings = objPayorTemplateCode.LoadPaymentDataFieldsSetting((Guid)PayorId, (Guid)TemplateID).ToList();
                    }
                    DEU _DEU = DEU.GetDeuEntryidWise(DeuEntryId);
                    objImportToolPaymentDataSettings = new List<ImportToolPaymentDataFieldsSettings>(objImportToolPaymentDataSettings.Where(p => p.PartOfPrimaryKey == true).ToList());
                    foreach (ImportToolPaymentDataFieldsSettings ptf in objImportToolPaymentDataSettings)
                    {
                        UniqueIdenitfier _UniqueIdenitfier = new UniqueIdenitfier();
                        _UniqueIdenitfier.ColumnName = ptf.FieldsName;
                        if (_DEU != null)
                        {
                            _UniqueIdenitfier.Text = GetValueFromDEUTableForAColumn(_DEU, _UniqueIdenitfier.ColumnName);
                        }
                        if (_UniqueIdenitfier != null)
                        {
                            _UniqueIndenties.Add(_UniqueIdenitfier);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ActionLogger.Logger.WriteLog("GetUniqueIdentifierForPayor :" + ex.StackTrace.ToString(), true);
                ActionLogger.Logger.WriteLog("GetUniqueIdentifierForPayor :" + ex.InnerException.ToString(), true);
            }
            return _UniqueIndenties;
        }

        private static string GetValueFromDEUTableForAColumn(DEU DeuEntry, string ColumnName)
        {
            string value = null;
            // DEU DEUEntry = DEU.GetDeuEntryidWise(DeuEntryId);
            try
            {
                switch (ColumnName)
                {
                    case "Effective":
                        if (DeuEntry != null)
                        {
                            if (DeuEntry.OriginalEffectiveDate != null)
                            {
                                value = DeuEntry.OriginalEffectiveDate.Value.ToString("MM/dd/yyyy");
                            }
                        }

                        break;
                    case "Insured":
                        if (DeuEntry != null)
                        {
                            if (DeuEntry.Insured != null)
                            {
                                value = DeuEntry.Insured.ToString();
                            }
                        }
                        break;
                    case "PolicyNumber":
                        if (DeuEntry != null)
                        {
                            if (DeuEntry.PolicyNumber != null)
                            {
                                value = DeuEntry.PolicyNumber.ToString();
                            }
                        }
                        break;

                    case "PolicyMode":
                        if (DeuEntry != null)
                        {
                            if (DeuEntry.PolicyMode != null)
                            {
                                value = DeuEntry.PolicyMode.ToString();
                            }
                        }
                        break;
                    case "TrackFromDate":
                        if (DeuEntry != null)
                        {
                            if (DeuEntry.TrackFromDate != null)
                            {
                                value = DeuEntry.TrackFromDate.Value.ToString("MM/dd/yyyy");
                            }
                        }
                        break;

                    case "CompType":
                        if (DeuEntry != null)
                        {
                            if (DeuEntry.CompTypeID != null)
                            {
                                value = DeuEntry.CompTypeID.ToString();
                            }
                        }
                        break;
                    case "Client":
                        if (DeuEntry != null)
                        {
                            if (DeuEntry.ClientName != null)
                            {
                                value = DeuEntry.ClientName.ToString();
                            }
                        }
                        break;

                    case "Payor":
                        if (DeuEntry != null)
                        {
                            if (DeuEntry.PayorId != null)
                            {
                                value = DeuEntry.PayorId.ToString();
                            }
                        }
                        break;

                    case "Carrier":
                        if (DeuEntry != null)
                        {
                            value = Carrier.GetCarrierNickName(DeuEntry.PayorId ?? Guid.Empty, DeuEntry.CarrierID ?? Guid.Empty);
                            if (string.IsNullOrEmpty(value))
                            {
                                value = DeuEntry.CarrierName;
                            }
                        }
                        break;
                    case "Product":
                        if (DeuEntry != null)
                        {
                            //value = Coverage.GetCoverageNickName(DeuEntry.PayorId ?? Guid.Empty, DeuEntry.CarrierID ?? Guid.Empty, DeuEntry.CoverageID ?? Guid.Empty);
                            //if (string.IsNullOrEmpty(value))
                            //{
                            //    value = DeuEntry.ProductName;
                            //}

                            if (string.IsNullOrEmpty(DeuEntry.ProductName))
                            {
                                value = Coverage.GetCoverageNickName(DeuEntry.PayorId ?? Guid.Empty, DeuEntry.CarrierID ?? Guid.Empty, DeuEntry.CoverageID ?? Guid.Empty);
                            }
                            else
                            {
                                value = DeuEntry.ProductName;
                            }
                        }
                        break;
                    default:
                        PropertyInfo identifier = typeof(DEU).GetProperty(ColumnName);
                        if (identifier != null)
                        {
                            value = identifier.GetValue(DeuEntry, null).ToString();
                        }
                        break;
                }
            }
            catch (Exception ex)
            {
                ActionLogger.Logger.WriteLog("GetValueFromDEUTableForAColumn :" + ex.StackTrace.ToString(), true);
                ActionLogger.Logger.WriteLog("GetValueFromDEUTableForAColumn :" + ex.InnerException.ToString(), true);
            }
            
            return value;

        }

   
        public static decimal CalculatePMC(Guid PolicyId)
        {
            using (DLinq.CommissionDepartmentEntities DataModel = Entity.DataModel)
            {
                System.Data.Objects.ObjectParameter objParam = new System.Data.Objects.ObjectParameter("pMC", typeof(decimal));
                try
                {
                    DataModel.CommandTimeout = 600000000;
                    var PMC = (from s in DataModel.CalculatePMC(PolicyId)
                               select new
                               {
                                   s.PMC

                               }).Single();

                    return Convert.ToDecimal(PMC.PMC);

                }
                catch (Exception ex)
                {
                    ActionLogger.Logger.WriteLog("CalculatePMC :" + ex.StackTrace.ToString(), true);
                    ActionLogger.Logger.WriteLog("CalculatePMC :" + ex.InnerException.ToString(), true);

                    return 0;
                }

            }
        }

        public static decimal CalculatePAC(Guid PolicyId)
        {
            using (DLinq.CommissionDepartmentEntities DataModel = Entity.DataModel)
            {
                try
                {
                    DataModel.CommandTimeout = 600000000;
                    var PAC = (from s in DataModel.CalculatePAC(PolicyId)
                               select new
                               {
                                   s.PAC

                               }).Single();

                    return Convert.ToDecimal(PAC.PAC);

                }
                catch(Exception ex)
                {
                    ActionLogger.Logger.WriteLog("CalculatePAC :" + ex.StackTrace.ToString(), true);
                    ActionLogger.Logger.WriteLog("CalculatePAC :" + ex.InnerException.ToString(), true);
                    return 0;
                }
            }
        }

        //private Guid? GetCarrierID(Guid payorID)
        //{
        //    Guid? guidCarrierID = null;

        //    if (payorID != Guid.Empty)
        //    {
        //        List<Carrier> AllCarriersInPayor = new List<Carrier>();
        //        AllCarriersInPayor = Carrier.GetPayorCarriers(payorID).ToList();
        //        if (AllCarriersInPayor.Count == 1)
        //        {
        //            guidCarrierID = AllCarriersInPayor.FirstOrDefault().CarrierId;
        //        }
        //    }
        //    return guidCarrierID;

        //}

        public static List<DeuSearchedPolicy> GetPoliciesFromUniqueIdentifier(List<UniqueIdenitfier> _UniqueIndenties, Guid LicenseId, Guid PayorId)
        {
            List<PolicyDetailsData> _policy = null;
            List<DeuSearchedPolicy> _PolicyLst = new List<DeuSearchedPolicy>();
            List<DeuSearchedPolicy> _TempPolicyLst = new List<DeuSearchedPolicy>();
            try
            {
                Dictionary<string, object> parameters = new Dictionary<string, object>();
                Expression<Func<DLinq.Policy, bool>> expressionParameters = p => p.IsDeleted == false;
                parameters.Add("PayorId", PayorId);
                Expression<Func<DLinq.Policy, bool>> tempParameters = p => p.PolicyLicenseeId == LicenseId;

                Guid? carrierId = null, coverageId = null;
                Guid? tempcoverageId = null;
                string PolicyNumber = string.Empty;
                string strProductType = string.Empty;
                bool isPrdType = false;
                expressionParameters = expressionParameters.And(tempParameters);

                foreach (UniqueIdenitfier DFD in _UniqueIndenties)
                {
                    switch (DFD.ColumnName)
                    {
                        case "Effective":
                            DateTime time;
                            DateTime.TryParse(DFD.Text, DateTimeFormatInfo.InvariantInfo, DateTimeStyles.None, out time);
                            parameters.Add("OriginalEffectiveDate", time);
                            break;
                        case "Insured":
                            parameters.Add("Insured", DFD.Text);
                            break;
                        case "PolicyNumber":
                            PolicyNumber = DFD.Text;
                            parameters.Add("PolicyNumber", DFD.Text);
                            break;
                        case "Enrolled":
                            parameters.Add("PolicyNumber", DFD.Text);
                            break;
                        case "Eligible":
                            parameters.Add("Eligible", DFD.Text);
                            break;
                        case "SplitPercentage":
                            double? value = Convert.ToDouble(DFD.Text);
                            parameters.Add("SplitPercentage", value);
                            break;
                        case "PolicyMode":
                            Int32? PolicyMode = Convert.ToInt32(DFD.Text);
                            parameters.Add("PolicyModeId", PolicyMode);
                            break;
                        case "TrackFromDate":
                            DateTime TrackFrom = DateTime.ParseExact(DFD.Text, "MM/dd/yyyy", DateTimeFormatInfo.InvariantInfo);
                            parameters.Add("TrackFromDate", TrackFrom);
                            break;
                        case "CompType":
                            int? PaymentId = null;
                            try
                            {
                                PaymentId = Convert.ToInt32(DFD.Text);
                            }
                            catch
                            {
                                PaymentId = BLHelper.getCompTypeIdByName(DFD.Text);
                            }
                            parameters.Add("IncomingPaymentTypeId", PaymentId);
                            break;
                        case "Client":
                            Guid? clientId = BLHelper.GetClientId(DFD.Text, LicenseId);
                            if (clientId != null)
                                parameters.Add("PolicyClientId", clientId);
                            else
                            {
                                clientId = new Guid();
                                parameters.Add("PolicyClientId", clientId);
                            }
                            break;
                        case "Payor":
                            Guid? PayorIdval = Guid.Parse(DFD.Text);
                            parameters.Add("PayorId", PayorIdval);
                            break;
                        case "Carrier":
                            carrierId = BLHelper.GetCarrierId(DFD.Text, PayorId);
                            if (carrierId == null || carrierId == Guid.Empty)
                            {
                                string carriernickname = DFD.Text;
                                tempParameters = p => p.PolicyLearnedField.CarrierNickName == carriernickname;
                                expressionParameters = expressionParameters.And(tempParameters);
                            }
                            else
                                parameters.Add("CarrierId", carrierId);

                            break;
                        case "Product":

                            isPrdType = true;
                            strProductType = DFD.Text;
                            coverageId = BLHelper.GetProductId(DFD.Text, PayorId, carrierId);
                            if (coverageId == null || coverageId == Guid.Empty)
                            {
                                //coverageId = BLHelper.GetProductId(DFD.Text, PayorId);
                                tempcoverageId = BLHelper.GetProductIdByProductType(DFD.Text, PayorId, PolicyNumber);
                                if (tempcoverageId != null)
                                {
                                    parameters.Add("CoverageId", tempcoverageId);
                                }
                                else
                                {
                                    Guid? CoveragecoverageId = BLHelper.GetProductIdByCoverageNickName(DFD.Text, PayorId, PolicyNumber);
                                    if (CoveragecoverageId != null)
                                    {
                                        parameters.Add("CoverageId", CoveragecoverageId);
                                    }

                                    else
                                    {
                                        string ProductType = DFD.Text;
                                        tempParameters = p => p.PolicyLearnedField.ProductType == ProductType;
                                        expressionParameters = expressionParameters.And(tempParameters);
                                        parameters.Add("ProductType", ProductType);
                                    }
                                }

                            }
                            else
                                parameters.Add("CoverageId", coverageId);

                            break;
                        case "CompScheduleType":
                            //string compScheduleType = DFD.Text;
                            //tempParameters = p => p.PolicyLearnedField.CompScheduleType == compScheduleType;
                            //expressionParameters = expressionParameters.And(tempParameters);
                            //Get comp type name 
                            int? intCompTypeID = CompType.GetCompTypeName(DFD.Text);
                            tempParameters = p => p.PolicyLearnedField.CompTypeID == intCompTypeID;
                            expressionParameters = expressionParameters.And(tempParameters);
                            break;

                        case "ProductType":
                            break;

                    }
                }
                _policy = Policy.GetPolicyData(parameters, expressionParameters);
                foreach (PolicyDetailsData poli in _policy)
                {
                    DeuSearchedPolicy dsp = new DeuSearchedPolicy();
                    dsp.CarrierName = poli.CarrierName;
                    dsp.ClientName = poli.ClientName;
                    dsp.CompType = PolicyIncomingPaymentType.GetIncomingPaymentTypeList().Where(p => p.PaymentTypeId == (poli.IncomingPaymentTypeId ?? 1)).FirstOrDefault().PaymenProcedureName;
                    dsp.Insured = poli.Insured;
                    dsp.LastModifiedDate = poli.CreatedOn.Value;
                    dsp.PaymentMode = poli.PolicyModeId;
                    dsp.PolicyId = poli.PolicyId;
                    dsp.PolicyNumber = poli.PolicyNumber;
                    dsp.PolicyStatus = poli.PolicyStatusId ?? 0;
                    dsp.ProductName = poli.CoverageName;
                    dsp.ProductType = poli.ProductType;
                    _PolicyLst.Add(dsp);
                }

                if (isPrdType)
                {
                    foreach (var item in _PolicyLst)
                    {
                        if (!string.IsNullOrEmpty(item.ProductType))
                        {
                            _TempPolicyLst.Add(item);
                        }
                    }
                    _PolicyLst.Clear();
                    _PolicyLst = new List<DeuSearchedPolicy>();
                    _PolicyLst = _TempPolicyLst;
                    _PolicyLst = _PolicyLst.Where(s => s.ProductType.ToLower() == strProductType.ToLower()).ToList();
                }
            }
            catch (Exception ex)
            {
                ActionLogger.Logger.WriteLog("GetPoliciesFromUniqueIdentifier ex.StackTrace :" + ex.StackTrace.ToString(), true);
                ActionLogger.Logger.WriteLog("GetPoliciesFromUniqueIdentifier ex.InnerException :" + ex.InnerException.ToString(), true);

            }
            return _PolicyLst;
        }

        //public static List<DeuSearchedPolicy> GetPoliciesFromUniqueIdentifier(List<UniqueIdenitfier> _UniqueIndenties, Guid LicenseId, Guid PayorId)
        //{

        //    List<PolicyDetailsData> _policy = null;
        //    List<DeuSearchedPolicy> _PolicyLst = new List<DeuSearchedPolicy>();
        //    Dictionary<string, object> parameters = new Dictionary<string, object>();
        //    Expression<Func<DLinq.Policy, bool>> expressionParameters = p => p.IsDeleted == false;
        //    parameters.Add("PayorId", PayorId);
        //    Expression<Func<DLinq.Policy, bool>> tempParameters = p => p.PolicyLicenseeId == LicenseId;

        //    Guid? carrierId = null, coverageId = null;
        //    Guid? tempcoverageId = null;
        //    string PolicyNumber = string.Empty;
        //    expressionParameters = expressionParameters.And(tempParameters);

        //    foreach (UniqueIdenitfier DFD in _UniqueIndenties)
        //    {
        //        switch (DFD.ColumnName)
        //        {
        //            case "Effective":
        //                DateTime time;
        //                DateTime.TryParse(DFD.Text, DateTimeFormatInfo.InvariantInfo, DateTimeStyles.None, out time);
        //                parameters.Add("OriginalEffectiveDate", time);
        //                break;
        //            case "Insured":
        //                parameters.Add("Insured", DFD.Text);
        //                break;
        //            case "PolicyNumber":
        //                PolicyNumber = DFD.Text;
        //                parameters.Add("PolicyNumber", DFD.Text);
        //                break;
        //            case "Enrolled":
        //                parameters.Add("PolicyNumber", DFD.Text);
        //                break;
        //            case "Eligible":
        //                parameters.Add("Eligible", DFD.Text);
        //                break;
        //            case "SplitPercentage":
        //                double? value = Convert.ToDouble(DFD.Text);
        //                parameters.Add("SplitPercentage", value);
        //                break;
        //            case "PolicyMode":
        //                Int32? PolicyMode = Convert.ToInt32(DFD.Text);
        //                parameters.Add("PolicyModeId", PolicyMode);
        //                break;
        //            case "TrackFromDate":
        //                DateTime TrackFrom = DateTime.ParseExact(DFD.Text, "MM/dd/yyyy", DateTimeFormatInfo.InvariantInfo);
        //                parameters.Add("TrackFromDate", TrackFrom);
        //                break;
        //            case "CompType":
        //                int? PaymentId = null;
        //                try
        //                {
        //                    PaymentId = Convert.ToInt32(DFD.Text);
        //                }
        //                catch
        //                {
        //                    PaymentId = BLHelper.getCompTypeIdByName(DFD.Text);
        //                }
        //                parameters.Add("IncomingPaymentTypeId", PaymentId);
        //                break;
        //            case "Client":
        //                Guid? clientId = BLHelper.GetClientId(DFD.Text, LicenseId);
        //                if (clientId != null)
        //                    parameters.Add("PolicyClientId", clientId);
        //                else
        //                {
        //                    clientId = new Guid();
        //                    parameters.Add("PolicyClientId", clientId);
        //                }
        //                break;
        //            case "Payor":
        //                Guid? PayorIdval = Guid.Parse(DFD.Text);
        //                parameters.Add("PayorId", PayorIdval);
        //                break;
        //            case "Carrier":
        //                carrierId = BLHelper.GetCarrierId(DFD.Text, PayorId);
        //                if (carrierId == null || carrierId == Guid.Empty)
        //                {
        //                    string carriernickname = DFD.Text;
        //                    tempParameters = p => p.PolicyLearnedField.CarrierNickName == carriernickname;
        //                    expressionParameters = expressionParameters.And(tempParameters);
        //                }
        //                else
        //                    parameters.Add("CarrierId", carrierId);

        //                break;
        //            case "Product":
        //                coverageId = BLHelper.GetProductId(DFD.Text, PayorId, carrierId);

        //                if (coverageId == null || coverageId == Guid.Empty)
        //                {
        //                    //coverageId = BLHelper.GetProductId(DFD.Text, PayorId);
        //                    tempcoverageId = BLHelper.GetProductIdByProductType(DFD.Text, PayorId, PolicyNumber);
        //                    if (tempcoverageId != null)
        //                    {
        //                        parameters.Add("CoverageId", tempcoverageId);
        //                    }
        //                    else
        //                    {
        //                        Guid? CoveragecoverageId = BLHelper.GetProductIdByCoverageNickName(DFD.Text, PayorId, PolicyNumber);
        //                        if (CoveragecoverageId != null)
        //                        {
        //                            parameters.Add("CoverageId", CoveragecoverageId);
        //                        }

        //                        else
        //                        {
        //                            //Guid? CovoverageId = BLHelper.GetProductId(DFD.Text, PayorId);
        //                            //if (CovoverageId != null)
        //                            //{
        //                            //    parameters.Add("CoverageId", CovoverageId);
        //                            //}
        //                            string ProductType = DFD.Text;
        //                            tempParameters = p => p.PolicyLearnedField.ProductType == ProductType;

        //                            expressionParameters = expressionParameters.And(tempParameters);
        //                            parameters.Add("ProductType", ProductType);
        //                        }
        //                    }

        //                    //if (coverageId == null || coverageId == Guid.Empty)
        //                    //{
        //                    //    //string CoverageNickName = DFD.Text;
        //                    //    //tempParameters = p => p.PolicyLearnedField.CoverageNickName == CoverageNickName;
        //                    //    try
        //                    //    {
        //                    //        string CoverageNickName = DFD.Text;
        //                    //        tempParameters = p => p.PolicyLearnedField.CoverageNickName.ToLower() == CoverageNickName.ToLower();

        //                    //        string ProductType = DFD.Text;
        //                    //        tempParameters = p => p.PolicyLearnedField.ProductType == ProductType;

        //                    //        expressionParameters = expressionParameters.And(tempParameters);
        //                    //        parameters.Add("ProductType", ProductType);
        //                    //    }
        //                    //    catch
        //                    //    {
        //                    //    }
        //                    //}
        //                }
        //                else
        //                    parameters.Add("CoverageId", coverageId);

        //                break;
        //            case "CompScheduleType":
        //                //string compScheduleType = DFD.Text;
        //                //tempParameters = p => p.PolicyLearnedField.CompScheduleType == compScheduleType;
        //                //expressionParameters = expressionParameters.And(tempParameters);
        //                //Get comp type name 
        //                int? intCompTypeID = CompType.GetCompTypeName(DFD.Text);
        //                tempParameters = p => p.PolicyLearnedField.CompTypeID == intCompTypeID;
        //                expressionParameters = expressionParameters.And(tempParameters);
        //                break;

        //            case "ProductType":
        //                break;

        //        }
        //    }
        //    _policy = Policy.GetPolicyData(parameters, expressionParameters);
        //    foreach (PolicyDetailsData poli in _policy)
        //    {
        //        DeuSearchedPolicy dsp = new DeuSearchedPolicy();
        //        dsp.CarrierName = poli.CarrierName;
        //        dsp.ClientName = poli.ClientName;
        //        dsp.CompType = PolicyIncomingPaymentType.GetIncomingPaymentTypeList()
        //                        .Where(p => p.PaymentTypeId == (poli.IncomingPaymentTypeId ?? 1)).FirstOrDefault().PaymenProcedureName;
        //        dsp.Insured = poli.Insured;
        //        dsp.LastModifiedDate = poli.CreatedOn.Value;
        //        dsp.PaymentMode = poli.PolicyModeId;
        //        dsp.PolicyId = poli.PolicyId;
        //        dsp.PolicyNumber = poli.PolicyNumber;
        //        dsp.PolicyStatus = poli.PolicyStatusId ?? 0;
        //        dsp.ProductName = poli.CoverageName;
        //       
        //        _PolicyLst.Add(dsp);
        //    }
        //    return _PolicyLst;
        //}
        /// <summary>
        /// this will return the Policy which will be processed by Post
        /// </summary>
        /// <param name="deuFields"></param>
        /// <returns>Return the Basic Information needed to Peocess</returns>
        public static BasicInformationForProcess GetPolicyToProcess(DEUFields deuFields, string strUnlinkClient)
        {
            ActionLogger.Logger.WriteLog("GetPolicyToProcess deuFields: " + deuFields.DeuEntryId, true);
            BasicInformationForProcess _BasicInformationForProcess = new BasicInformationForProcess();
            try
            {
                if (deuFields == null)
                {
                    return _BasicInformationForProcess;
                }

                int SearchedPolicyCnt = 0;// SearchedPolicy.Count;
                bool IsAgencyVersionLicense = false;

                if (deuFields.SearchedPolicyList != null)
                    SearchedPolicyCnt = deuFields.SearchedPolicyList.Count;

                if (deuFields.LicenseeId != null)
                    IsAgencyVersionLicense = BillingLineDetail.IsAgencyVersionLicense(deuFields.LicenseeId.Value);

                _BasicInformationForProcess.IsAgencVersion = IsAgencyVersionLicense;
                if (SearchedPolicyCnt == 0)
                {
                    _BasicInformationForProcess.IsPaymentToHO = true;
                    _BasicInformationForProcess.PostStatus = PostStatus.NoLink;

                    if (IsAgencyVersionLicense || !IsAgencyVersionLicense)//if agency or not agency
                    {
                        _BasicInformationForProcess.PolicyId = CreateNewPendingPolicy(deuFields, strUnlinkClient);//Discuss on that  policy is to be created on under which agent or HO--it is created on HO
                    }
                    _BasicInformationForProcess.message = "-- Searched Policy Count is zero --";
                }
                else if (SearchedPolicyCnt > 1)
                {
                    int PendingPolicyCnt = deuFields.SearchedPolicyList.Where(p => p.PolicyStatus == (int)_PolicyStatus.Pending).ToList().Count;

                    if (PendingPolicyCnt == 1)
                    {

                        _BasicInformationForProcess.IsPaymentToHO = true;
                        if (!IsAgencyVersionLicense)//if agency or not agency
                        {
                            _BasicInformationForProcess.PostStatus = PostStatus.NoAgency;
                            _BasicInformationForProcess.PolicyId = deuFields.SearchedPolicyList.Where(p => p.PolicyStatus == (int)_PolicyStatus.Pending).FirstOrDefault().PolicyId;
                        }
                        else if (IsAgencyVersionLicense)
                        {
                            _BasicInformationForProcess.PostStatus = PostStatus.NoLink;
                            _BasicInformationForProcess.PolicyId = deuFields.SearchedPolicyList.Where(p => p.PolicyStatus == (int)_PolicyStatus.Pending).FirstOrDefault().PolicyId;
                        }
                        _BasicInformationForProcess.message = "-- Pending Policy Count is One --";

                    }
                    else if (PendingPolicyCnt == 0)
                    {

                        int Cnt = deuFields.SearchedPolicyList.Where(p => p.PolicyStatus == (int)_PolicyStatus.Active).ToList().Count;
                        if (Cnt > 1)
                        {
                            _BasicInformationForProcess = AddPaymentToActivePolicy(deuFields, IsAgencyVersionLicense);
                        }
                        else
                        {
                            _BasicInformationForProcess.IsPaymentToHO = true;
                            if (!IsAgencyVersionLicense)//if agency or not agency
                            {
                                _BasicInformationForProcess.PostStatus = PostStatus.NoAgency;
                                _BasicInformationForProcess.PolicyId = CreateNewPendingPolicy(deuFields, strUnlinkClient);//Discuss on that  policy is to be created on under which agent or HO
                            }
                            else
                            {
                                _BasicInformationForProcess.PostStatus = PostStatus.NoLink;
                                _BasicInformationForProcess.PolicyId = CreateNewPendingPolicy(deuFields, strUnlinkClient);//Discuss on that  policy is to be created on under which agent or HO
                            }
                            _BasicInformationForProcess.message = "-- Pending Policy Count is Zero --";
                        }

                    }
                    else if (PendingPolicyCnt > 1)//this case is need to confirm
                    {
                        _BasicInformationForProcess.IsPaymentToHO = true;
                        if (!IsAgencyVersionLicense)//if agency or not agency
                        {
                            _BasicInformationForProcess.PostStatus = PostStatus.NoAgency;
                            DateTime LatestPendingPolicyDate = deuFields.SearchedPolicyList.Where(p => p.PolicyStatus == (int)_PolicyStatus.Pending).Max(p => p.LastModifiedDate);
                            _BasicInformationForProcess.PolicyId = deuFields.SearchedPolicyList.Where(p => p.PolicyStatus == (int)_PolicyStatus.Pending).Where(p => p.LastModifiedDate == LatestPendingPolicyDate).FirstOrDefault().PolicyId;
                        }
                        else
                        {
                            _BasicInformationForProcess.PostStatus = PostStatus.NoLink;
                            DateTime LatestPendingPolicyDate = deuFields.SearchedPolicyList.Where(p => p.PolicyStatus == (int)_PolicyStatus.Pending).Max(p => p.LastModifiedDate);
                            _BasicInformationForProcess.PolicyId = deuFields.SearchedPolicyList.Where(p => p.PolicyStatus == (int)_PolicyStatus.Pending).Where(p => p.LastModifiedDate == LatestPendingPolicyDate).FirstOrDefault().PolicyId;
                        }
                        _BasicInformationForProcess.message = "-- Pending Policy Count is greater than One --";
                    }

                }
                else if (SearchedPolicyCnt == 1)
                {
                    int SearchPendingStatuscnt = deuFields.SearchedPolicyList.Where(p => p.PolicyStatus == (int)_PolicyStatus.Pending).ToList().Count;
                    if (SearchPendingStatuscnt == 1)
                    {
                        _BasicInformationForProcess.IsPaymentToHO = true;
                        if (!IsAgencyVersionLicense)//if agency or not agency
                        {
                            _BasicInformationForProcess.PostStatus = PostStatus.NoAgency;
                            _BasicInformationForProcess.PolicyId = deuFields.SearchedPolicyList.Where(p => p.PolicyStatus == (int)_PolicyStatus.Pending).FirstOrDefault().PolicyId;
                        }
                        else
                        {
                            _BasicInformationForProcess.PostStatus = PostStatus.NoLink;
                            _BasicInformationForProcess.PolicyId = deuFields.SearchedPolicyList.Where(p => p.PolicyStatus == (int)_PolicyStatus.Pending).FirstOrDefault().PolicyId;
                        }
                        _BasicInformationForProcess.message = "--Search Pending Policy Count is One --";

                    }

                    else//Not Pending (Either Active/Terminated)
                    {
                        if (IsAgencyVersionLicense)//if agency or not agency
                        {
                            FirstYrRenewalYr _FirstYrRenewalYr = FirstYrRenewalYr.None;
                            _BasicInformationForProcess.PolicyId = Guid.Empty;
                            _BasicInformationForProcess.PolicyId = deuFields.SearchedPolicyList.FirstOrDefault().PolicyId;
                            PolicyDetailsData pol = GetPolicy(_BasicInformationForProcess.PolicyId);
                            if (pol.IsOutGoingBasicSchedule ?? false)
                            {
                                if (pol.OriginalEffectiveDate == null)
                                {
                                    if (pol.TrackFromDate == null)
                                    {
                                        if (deuFields.DeuData.OriginalEffectiveDate == null)
                                        {
                                            _FirstYrRenewalYr = IsUseFirstYearForOutGoing(deuFields.DeuData.InvoiceDate, deuFields.DeuData.InvoiceDate, _BasicInformationForProcess.PolicyId);
                                        }
                                        else
                                        {
                                            if (deuFields.DeuData.OriginalEffectiveDate > deuFields.DeuData.InvoiceDate)
                                            {
                                                _FirstYrRenewalYr = IsUseFirstYearForOutGoing(deuFields.DeuData.InvoiceDate, deuFields.DeuData.InvoiceDate, _BasicInformationForProcess.PolicyId);
                                            }
                                            else
                                            {
                                                _FirstYrRenewalYr = IsUseFirstYearForOutGoing(deuFields.DeuData.InvoiceDate, deuFields.DeuData.OriginalEffectiveDate, _BasicInformationForProcess.PolicyId);
                                            }
                                        }
                                    }
                                    else
                                    {
                                        _FirstYrRenewalYr = IsUseFirstYearForOutGoing(deuFields.DeuData.InvoiceDate, pol.TrackFromDate, _BasicInformationForProcess.PolicyId);
                                    }
                                }
                                else
                                {
                                    _FirstYrRenewalYr = IsUseFirstYearForOutGoing(deuFields.DeuData.InvoiceDate, pol.OriginalEffectiveDate, _BasicInformationForProcess.PolicyId);

                                }
                                if (_FirstYrRenewalYr == FirstYrRenewalYr.None)
                                {
                                    _FirstYrRenewalYr = FirstYrRenewalYr.FirstYear;
                                }

                                if (_FirstYrRenewalYr == FirstYrRenewalYr.None)
                                {
                                    _BasicInformationForProcess.PolicyId =  CreateNewPendingPolicy(deuFields, strUnlinkClient);
                                    _BasicInformationForProcess.PostStatus = PostStatus.Ag_NoSplits;
                                    _BasicInformationForProcess.IsPaymentToHO = true;
                                    _BasicInformationForProcess.message = "-- First/Renewal not decidable --";
                                }
                                else
                                {
                                    _BasicInformationForProcess.PostStatus = PostStatus.Linked_Agency;
                                    _BasicInformationForProcess.IsPaymentToHO = false;
                                }
                            }
                            else
                            {
                                _BasicInformationForProcess.PostStatus = PostStatus.Linked_Agency;
                                _BasicInformationForProcess.IsPaymentToHO = false;
                            }
                        }
                        else
                        {
                            _BasicInformationForProcess.PostStatus = PostStatus.Linked_NoAg;
                            _BasicInformationForProcess.IsPaymentToHO = true;
                            _BasicInformationForProcess.PolicyId = deuFields.SearchedPolicyList.FirstOrDefault().PolicyId;
                            _BasicInformationForProcess.message = "-- No Agency Version --";
                        }
                    }

                }
            }
            catch (Exception ex)
            {
                ActionLogger.Logger.WriteLog("GetPolicyToProcess " + ex.StackTrace.ToString() + DateTime.Now.ToLongTimeString(), true);
                ActionLogger.Logger.WriteLog("GetPolicyToProcess " + ex.InnerException.ToString() + DateTime.Now.ToLongTimeString(), true);
            }

            return _BasicInformationForProcess;

        }

        public static BasicInformationForProcess AddPaymentToActivePolicy(DEUFields deuFields, bool IsAgencyVersionLicense)
        {
            BasicInformationForProcess _BasicInformationForProcess = new BasicInformationForProcess();
            try
            {
                if (IsAgencyVersionLicense)//if agency or not agency
                {
                    FirstYrRenewalYr _FirstYrRenewalYr = FirstYrRenewalYr.None;

                    _BasicInformationForProcess.PolicyId = Guid.Empty;

                    _BasicInformationForProcess.PolicyId = deuFields.SearchedPolicyList.FirstOrDefault().PolicyId;
                    PolicyDetailsData pol = GetPolicy(_BasicInformationForProcess.PolicyId);
                    if (pol.IsOutGoingBasicSchedule ?? false)
                    {
                        if (pol.OriginalEffectiveDate == null)
                        {
                            if (pol.TrackFromDate == null)
                            {
                                if (deuFields.DeuData.OriginalEffectiveDate == null)
                                {
                                    _FirstYrRenewalYr = IsUseFirstYearForOutGoing(deuFields.DeuData.InvoiceDate, deuFields.DeuData.InvoiceDate, _BasicInformationForProcess.PolicyId);
                                }
                                else
                                {
                                    //Check enter invoice date and effective date
                                    if (deuFields.DeuData.OriginalEffectiveDate > deuFields.DeuData.InvoiceDate)
                                    {
                                        _FirstYrRenewalYr = IsUseFirstYearForOutGoing(deuFields.DeuData.InvoiceDate, deuFields.DeuData.InvoiceDate, _BasicInformationForProcess.PolicyId);
                                    }
                                    else
                                    {
                                        _FirstYrRenewalYr = IsUseFirstYearForOutGoing(deuFields.DeuData.InvoiceDate, deuFields.DeuData.OriginalEffectiveDate, _BasicInformationForProcess.PolicyId);
                                    }
                                }
                            }
                            else
                            {
                                _FirstYrRenewalYr = IsUseFirstYearForOutGoing(deuFields.DeuData.InvoiceDate, pol.TrackFromDate, _BasicInformationForProcess.PolicyId);
                            }
                        }
                        else
                        {
                            _FirstYrRenewalYr = IsUseFirstYearForOutGoing(deuFields.DeuData.InvoiceDate, pol.OriginalEffectiveDate, _BasicInformationForProcess.PolicyId);

                        }

                        //if comes from serch then
                        if (_FirstYrRenewalYr == FirstYrRenewalYr.None)
                        {
                            _FirstYrRenewalYr = FirstYrRenewalYr.FirstYear;
                        }

                        if (_FirstYrRenewalYr == FirstYrRenewalYr.None)
                        {
                            _BasicInformationForProcess.PolicyId = CreateNewPendingPolicy(deuFields, string.Empty);
                            _BasicInformationForProcess.PostStatus = PostStatus.Ag_NoSplits;
                            _BasicInformationForProcess.IsPaymentToHO = true;
                            _BasicInformationForProcess.message = "-- First/Renewal not decidable --";
                        }
                        else
                        {
                            _BasicInformationForProcess.PostStatus = PostStatus.Linked_Agency;
                            _BasicInformationForProcess.IsPaymentToHO = false;
                        }
                    }
                    else
                    {
                        _BasicInformationForProcess.PostStatus = PostStatus.Linked_Agency;
                        _BasicInformationForProcess.IsPaymentToHO = false;
                    }
                }
                else
                {
                    _BasicInformationForProcess.PostStatus = PostStatus.Linked_NoAg;
                    _BasicInformationForProcess.IsPaymentToHO = true;
                    _BasicInformationForProcess.PolicyId = deuFields.SearchedPolicyList.FirstOrDefault().PolicyId;
                    _BasicInformationForProcess.message = "-- No Agency Version --";
                }
            }
            catch (Exception ex)
            {
                ActionLogger.Logger.WriteLog("Issue in AddPaymentToActivePolicy :" + ex.StackTrace.ToString(), true);
                ActionLogger.Logger.WriteLog("Issue in AddPaymentToActivePolicy :" + ex.InnerException.ToString(), true);
            }
            return _BasicInformationForProcess;
        }

        private static PolicyPaymentEntriesPost GetInMemoryCollectionOfPolicyPaymentEntriesFromDEUInMemoryCollection(DEUFields deuFields, Guid PolicyId)
        {
            PolicyPaymentEntriesPost _PolicyPaymentEntriesPost = new PolicyPaymentEntriesPost();
            try
            {
                _PolicyPaymentEntriesPost.PolicyID = PolicyId;
                _PolicyPaymentEntriesPost.InvoiceDate = deuFields.DeuData.InvoiceDate;
                _PolicyPaymentEntriesPost.PaymentRecived = deuFields.DeuData.PaymentRecived ?? 0;
                _PolicyPaymentEntriesPost.CommissionPercentage = deuFields.DeuData.CommissionPercentage ?? 0;
                _PolicyPaymentEntriesPost.SplitPer = deuFields.DeuData.SplitPer;
                _PolicyPaymentEntriesPost.ClientId = deuFields.DeuData.ClientID ?? Guid.Empty;
                _PolicyPaymentEntriesPost.NumberOfUnits = deuFields.DeuData.NoOfUnits ?? 0;
                _PolicyPaymentEntriesPost.DollerPerUnit = deuFields.DeuData.DollerPerUnit ?? 0;
                _PolicyPaymentEntriesPost.Fee = deuFields.DeuData.Fee ?? 0;
                _PolicyPaymentEntriesPost.Bonus = deuFields.DeuData.Bonus ?? 0;
                _PolicyPaymentEntriesPost.TotalPayment = deuFields.DeuData.CommissionTotal ?? 0;
                _PolicyPaymentEntriesPost.PostStatusID = deuFields.DeuData.PostStatusID;
            }
            catch (Exception ex)
            {
                ActionLogger.Logger.WriteLog("GetInMemoryCollectionOfPolicyPaymentEntriesFromDEUInMemoryCollection :" + ex.StackTrace.ToString(), true);
                ActionLogger.Logger.WriteLog("GetInMemoryCollectionOfPolicyPaymentEntriesFromDEUInMemoryCollection :" + ex.InnerException.ToString(), true);
            }

            return _PolicyPaymentEntriesPost;

        }

        /// <summary>
        /// This is the Mode Calculation 
        /// </summary>
        /// <param name="_policy"></param>
        /// <param name="deuenteredmode"></param>
        /// <returns></returns>
        public static MasterPolicyMode? ModeEntryFromDeu(PolicyDetailsData _policy, int? deuenteredmode, bool isEnterfromDEU)
        {
            MasterPolicyMode? _FollowUpMode = null;
            if (_policy == null) return _FollowUpMode;
            try
            {
                if (deuenteredmode == null && isEnterfromDEU == true)
                {
                    _FollowUpMode = CalculateModeFromInvoice(_policy.PolicyId);

                    if (_FollowUpMode != null)
                    {
                        PolicyLearnedField.UpdateLearnedFieldsMode(_policy.PolicyId, (int)_FollowUpMode);
                    }
                }

                PolicyLearnedFieldData PolicyLearnedFields = PolicyLearnedField.GetPolicyLearnedFieldsPolicyWise(_policy.PolicyId);

                if (PolicyLearnedFields != null)
                {
                    int? LearnedMode = PolicyLearnedFields.PolicyModeId;
                    if (LearnedMode != null)
                    {
                        _FollowUpMode = (MasterPolicyMode)LearnedMode;
                    }
                }
            }
            catch (Exception ex)
            {
               
                ActionLogger.Logger.WriteLog("ModeEntryFromDeu :" + ex.Message.ToString(), true);
            }
            return _FollowUpMode;

            #region "Commented code

            //No need to query the DB again
            //int? policyDetail = GetPolicy(_policy.PolicyId).PolicyModeId;



            //int? policyDetail = _policy.PolicyModeId;
            //List<PolicyPaymentEntriesPost> _PolicyPaymentEntries = PolicyPaymentEntriesPost.GetPolicyPaymentEntryPolicyIDWise(_policy.PolicyId);

            //if (_policy.PolicyStatusId == (int)_PolicyStatus.Pending && LearnedMode == null && policyDetail == null && _PolicyPaymentEntries.Count == 0)
            //{
            //    _FollowUpMode = MasterPolicyMode.Monthly;
            //}

            //else if (_policy.PolicyStatusId == (int)_PolicyStatus.Pending && LearnedMode == null && policyDetail == null && _PolicyPaymentEntries.Count == 1)
            //{
            //    _FollowUpMode = MasterPolicyMode.Monthly;
            //}

            //else if (_policy.PolicyStatusId == (int)_PolicyStatus.Pending && LearnedMode == null && policyDetail == null && _PolicyPaymentEntries.Count >= 2)
            //{
            //    //_FollowUpMode = GetPolicyModeFromOldRecipts(_policy.PolicyId);
            //}

            //else if (_policy.PolicyStatusId == (int)_PolicyStatus.Pending && deuenteredmode != null && LearnedMode != null && policyDetail != null && deuenteredmode == LearnedMode && LearnedMode != policyDetail)
            //{
            //    _FollowUpMode = (MasterPolicyMode)LearnedMode;
            //}

            //else if (_policy.PolicyStatusId != (int)_PolicyStatus.Pending && deuenteredmode != null && LearnedMode == deuenteredmode && policyDetail == null)
            //{
            //    _FollowUpMode = (MasterPolicyMode)LearnedMode;
            //}

            //else if (_policy.PolicyStatusId != (int)_PolicyStatus.Pending && deuenteredmode == null && LearnedMode == null && policyDetail == null)
            //{
            //   // _FollowUpMode = GetPolicyModeFromOldRecipts(_policy.PolicyId);
            //}

            //else if (_policy.PolicyStatusId != (int)_PolicyStatus.Pending && deuenteredmode == null && LearnedMode != null && LearnedMode == policyDetail)
            //{
            //    _FollowUpMode = (MasterPolicyMode)LearnedMode;
            //}

            //else if (_policy.PolicyStatusId != (int)_PolicyStatus.Pending && deuenteredmode != null && LearnedMode != null && policyDetail != null && deuenteredmode == LearnedMode && LearnedMode != policyDetail)
            //{
            //    //_FollowUpMode = GetPolicyModeFromOldRecipts(_policy.PolicyId);
            //}
            //else if (_policy.PolicyStatusId == (int)_PolicyStatus.Pending && deuenteredmode == null && LearnedMode != null && LearnedMode == policyDetail)
            //{
            //    _FollowUpMode = (MasterPolicyMode)LearnedMode;
            //}
            //return _FollowUpMode;

            #endregion


        }

        #region"Comment code"
        //public static MasterPolicyMode? GetPolicyModeFromOldRecipts(Guid _PolicyId)
        //{

        //    List<PolicyPaymentEntriesPost> _PolicyPaymentEntriesPost = PolicyPaymentEntriesPost.GetPolicyPaymentEntryPolicyIDWise(_PolicyId);

        //    List<DateTime?> UniqueDateTime = _PolicyPaymentEntriesPost.Select(c => c.InvoiceDate).Distinct().ToList();

        //    List<PolicyPaymentEntriesPost> _TempPolicyPaymentEntriesPost = new List<PolicyPaymentEntriesPost>(_PolicyPaymentEntriesPost);
        //    _PolicyPaymentEntriesPost.Clear();
        //    foreach (DateTime dt in UniqueDateTime)
        //    {
        //        _PolicyPaymentEntriesPost.Add(_TempPolicyPaymentEntriesPost.Where(p => p.InvoiceDate == dt).FirstOrDefault());
        //    }
        //    _PolicyPaymentEntriesPost = _PolicyPaymentEntriesPost.OrderBy(p => p.InvoiceDate).ToList<PolicyPaymentEntriesPost>();
        //    int minDiff = 12;
        //    if (_PolicyPaymentEntriesPost.Count == 0) return null;
        //    if (_PolicyPaymentEntriesPost.Count == 1) return MasterPolicyMode.Monthly;
        //    for (int idx = 0; idx < _PolicyPaymentEntriesPost.Count - 1; idx++)
        //    {

        //        int diff = (GetNumberOfMonthBetweenTwoDays(_PolicyPaymentEntriesPost[idx + 1].InvoiceDate.Value, _PolicyPaymentEntriesPost[idx].InvoiceDate.Value));
        //        if (minDiff == 1) break;
        //        if (minDiff > diff)
        //        {
        //            minDiff = diff;
        //        }


        //    }

        //    if (minDiff % 12 == 0)
        //    {
        //        return MasterPolicyMode.Annually;
        //    }
        //    else if (minDiff % 6 == 0)
        //    {
        //        return MasterPolicyMode.HalfYearly;
        //    }
        //    else if (minDiff % 3 == 0)
        //    {
        //        return MasterPolicyMode.Quarterly;
        //    }
        //    else if (minDiff % 1 == 0)
        //    {
        //        return MasterPolicyMode.Monthly;
        //    }


        //    return MasterPolicyMode.Monthly;
        //}
        #endregion

        public static MasterPolicyMode? CalculateModeFromInvoice(Guid _PolicyId)
        {
            MasterPolicyMode? Policymode = null;

            try
            {

                List<PolicyPaymentEntriesPost> _PolicyPaymentEntriesPost = PolicyPaymentEntriesPost.GetPolicyPaymentEntryPolicyIDWise(_PolicyId);

                if (_PolicyPaymentEntriesPost == null)
                {
                    return Policymode;
                }
                if (_PolicyPaymentEntriesPost.Count == 0)
                {
                    return Policymode;
                }
                #region"Get Unique Invoice date"
                //Get Unique invoice date
                List<DateTime?> UniqueDateTime = _PolicyPaymentEntriesPost.Select(c => c.InvoiceDate).Distinct().ToList();
                List<PolicyPaymentEntriesPost> _TempPolicyPaymentEntriesPost = new List<PolicyPaymentEntriesPost>(_PolicyPaymentEntriesPost);

                if (_PolicyPaymentEntriesPost != null)
                {
                    _PolicyPaymentEntriesPost.Clear();
                }

                if (_PolicyPaymentEntriesPost.Count > 0)
                {
                    _PolicyPaymentEntriesPost.Clear();
                }  


                foreach (DateTime dt in UniqueDateTime)
                {
                    _PolicyPaymentEntriesPost.Add(_TempPolicyPaymentEntriesPost.Where(p => p.InvoiceDate == dt).FirstOrDefault());
                }
                #endregion

                if (_PolicyPaymentEntriesPost == null)
                {
                    return Policymode;
                }

                _PolicyPaymentEntriesPost = _PolicyPaymentEntriesPost.OrderBy(p => p.InvoiceDate).Distinct().ToList<PolicyPaymentEntriesPost>();

                if (_PolicyPaymentEntriesPost.Count > 3)
                {
                    //Leave the mode as before                
                    return Policymode;
                }

                if (_PolicyPaymentEntriesPost.Count == 3)
                {
                    _PolicyPaymentEntriesPost = _PolicyPaymentEntriesPost.OrderBy(p => p.InvoiceDate).ToList<PolicyPaymentEntriesPost>();

                    int diffFirst = (GetNumberOfMonthBetweenTwoDays(Convert.ToDateTime(_PolicyPaymentEntriesPost[0].InvoiceDate),Convert.ToDateTime(_PolicyPaymentEntriesPost[1].InvoiceDate)));
                    int diffSecond = (GetNumberOfMonthBetweenTwoDays(Convert.ToDateTime(_PolicyPaymentEntriesPost[1].InvoiceDate), Convert.ToDateTime(_PolicyPaymentEntriesPost[2].InvoiceDate)));

                    if (diffFirst == diffSecond && diffFirst == 1)
                    {
                        Policymode = MasterPolicyMode.Monthly;
                    }
                    else if (diffFirst == diffSecond && diffFirst == 3)
                    {
                        Policymode = MasterPolicyMode.Quarterly;
                    }
                    else if (diffFirst == diffSecond && diffFirst == 6)
                    {
                        Policymode = MasterPolicyMode.HalfYearly;
                    }
                    else if (diffFirst == diffSecond && diffFirst == 12)
                    {
                        Policymode = MasterPolicyMode.Annually;
                    }

                }

                else if (_PolicyPaymentEntriesPost.Count == 2)
                {
                    _PolicyPaymentEntriesPost = _PolicyPaymentEntriesPost.OrderBy(p => p.InvoiceDate).ToList<PolicyPaymentEntriesPost>();

                    int diff = (GetNumberOfMonthBetweenTwoDays(Convert.ToDateTime(_PolicyPaymentEntriesPost[0].InvoiceDate), Convert.ToDateTime(_PolicyPaymentEntriesPost[1].InvoiceDate)));

                    if (diff == 1)
                    {
                        Policymode = MasterPolicyMode.Monthly;
                    }
                    else if (diff == 3)
                    {
                        Policymode = MasterPolicyMode.Quarterly;
                    }
                    else if (diff == 6)
                    {
                        Policymode = MasterPolicyMode.HalfYearly;
                    }
                    else if (diff == 12)
                    {
                        Policymode = MasterPolicyMode.Annually;
                    }
                }
            }
            catch (Exception ex)
            {
                ActionLogger.Logger.WriteLog("CalculateModeFromInvoice :" + ex.Message.ToString(), true);
            }
            return Policymode;
        }

        private static MasterPolicyMode? CalculateMonth(int firstdiff, int secondDiff)
        {

            MasterPolicyMode? mode = null;
            try
            {

                if (firstdiff == secondDiff && firstdiff == 1)
                {
                    mode = MasterPolicyMode.Monthly;
                }
                else if (firstdiff == secondDiff && firstdiff == 3)
                {
                    mode = MasterPolicyMode.Quarterly;
                }
                else if (firstdiff == secondDiff && firstdiff == 6)
                {
                    mode = MasterPolicyMode.HalfYearly;
                }
                else if (firstdiff == secondDiff && firstdiff == 12)
                {
                    mode = MasterPolicyMode.HalfYearly;
                }
            }
            catch (Exception ex)
            {
                ActionLogger.Logger.WriteLog("CalculateMonth :" + ex.StackTrace.ToString(), true);
                ActionLogger.Logger.WriteLog("CalculateMonth :" + ex.InnerException.ToString(), true);
            }

            return mode;
        }

        private static int GetNumberOfMonthBetweenTwoDays(DateTime startDate, DateTime endDate)
        {
            int monthsApart = 0;
            
            try
            {
                monthsApart = 12 * (startDate.Year - endDate.Year) + startDate.Month - endDate.Month;               
            }
            catch
            {
            }
            return Math.Abs(monthsApart);
        }

        /// <summary>
        /// It Process the search Policy
        /// </summary>
        /// <param name="deuFields"></param>
        /// <param name="_BasicInformationForProcess"></param>
        /// <param name="_PostProcessReturnStatus"></param>
        /// <returns></returns>
        public static PostProcessReturnStatus ProcessSearchedPoilcy(DEUFields deuFields, BasicInformationForProcess _BasicInformationForProcess, PostProcessReturnStatus _PostProcessReturnStatus)
        {
            Guid PaymentEntryID = Guid.Empty;
            try
            {
               // bool IsTrackPayment = false;               
                if (_BasicInformationForProcess != null)
                {
                    if (deuFields != null)
                    {
                        PostStatus poststatus = _BasicInformationForProcess.PostStatus;
                        bool IsPaymentToHO = _BasicInformationForProcess.IsPaymentToHO;// false;//Verify Check

                        PaymentEntryID = EntryInPoicyPamentEntries(deuFields, _BasicInformationForProcess);//remain to Complete--Completed
                        //DEU _LatestDEUrecord = DEU.GetLatestInvoiceDateRecord(deuFields.DeuData.PolicyId);
                        DEU _LatestDEUrecord = DEU.GetDEULatestInvoiceDateRecord(deuFields.DeuEntryId);
                       
                        //Add code to update policy mode 22/11/2012
                        #region"Change mode"

                        if (deuFields.DeuData != null)
                        {
                            _LatestDEUrecord.PolicyMode = deuFields.DeuData.PolicyMode;
                        }

                        #endregion

                        Guid PolicyIdDeuToLrn = DEULearnedPost.AddDataDeuToLearnedPost(_LatestDEUrecord);
                        if (PolicyIdDeuToLrn != null)
                        {
                            if (PolicyIdDeuToLrn != Guid.Empty)
                            {
                                LearnedToPolicyPost.AddUpdateLearnedToPolicy(PolicyIdDeuToLrn);
                            }
                        }


                        //PolicyDetailsData objPolicyDetails = new PolicyDetailsData();
                        //if (_BasicInformationForProcess.PolicyId != null)
                        //{
                        //    //Need to change this code after this build
                        //    //ankita
                        //   objPolicyDetails = GetPolicy(_BasicInformationForProcess.PolicyId);
                        //}

                        //if (objPolicyDetails != null)
                        //{
                        //    if (objPolicyDetails.IsTrackPayment != null)
                        //    {
                        //        IsTrackPayment = objPolicyDetails.IsTrackPayment;
                        //    }
                        //}

                        if (deuFields.DeuData != null)
                        {
                            if (deuFields.DeuData.IsEntrybyCommissiondashBoard != null)
                            {
                                if (deuFields.DeuData.IsEntrybyCommissiondashBoard && !_BasicInformationForProcess.IsAgencVersion)
                                {
                                    IsPaymentToHO = true;
                                }

                                else if ((!IsPaymentToHO && !deuFields.DeuData.IsEntrybyCommissiondashBoard) || (deuFields.DeuData.IsEntrybyCommissiondashBoard && _BasicInformationForProcess.IsAgencVersion))
                                {
                                    IsPaymentToHO = !CheckForOutgoingScheduleVariance(PaymentEntryID);
                                }

                                if (deuFields.DeuData.IsEntrybyCommissiondashBoard)
                                {
                                    if (IsPaymentToHO && _BasicInformationForProcess.IsAgencVersion)
                                    {
                                        _PostProcessReturnStatus.ErrorMessage = "Payment is not Postable (Agency version and Outgoing Schedule is not Distributable)";
                                        return _PostProcessReturnStatus;
                                    }
                                }
                            }
                            if (deuFields.DeuData.InvoiceDate == null)
                            {
                                deuFields.DeuData.InvoiceDate = DateTime.Now;
                            }
                            try
                            {
                                EntryInPolicyOutGoingPayment(IsPaymentToHO, PaymentEntryID, deuFields.DeuData.PolicyId, Convert.ToDateTime(deuFields.DeuData.InvoiceDate), (Guid)deuFields.LicenseeId);//Need to check with Column i recieved
                            }
                            catch
                            {
                            }
                            _PostProcessReturnStatus.ErrorMessage = "Completed";
                            _PostProcessReturnStatus.IsComplete = true;
                        }

                    }

                }

            }
            catch (Exception ex)
            {
                ActionLogger.Logger.WriteLog("*************************", true);
                ActionLogger.Logger.WriteLog("Issue in ProcessSearchedPoilcy ex.Message :" + ex.Message.ToString(), true);
                ActionLogger.Logger.WriteLog("Issue in ProcessSearchedPoilcy ex.StackTrace :" + ex.StackTrace.ToString(), true);
                ActionLogger.Logger.WriteLog("Issue in ProcessSearchedPoilcy PaymentEntryID :" + PaymentEntryID.ToString(), true);
                ActionLogger.Logger.WriteLog("*************************", true);
                _PostProcessReturnStatus.IsComplete = false;
            }

            return _PostProcessReturnStatus;
        }

        public static PaymentMode ConvertMode(MasterPolicyMode _MasterPolicyMode)
        {
            PaymentMode _PaymentMode = PaymentMode.Monthly;
            try
            {
                switch (_MasterPolicyMode)
                {
                    case MasterPolicyMode.Annually:
                        _PaymentMode = PaymentMode.Yearly;
                        break;
                    case MasterPolicyMode.HalfYearly:
                        _PaymentMode = PaymentMode.HalfYearly;
                        break;
                    case MasterPolicyMode.Monthly:
                        _PaymentMode = PaymentMode.Monthly;
                        break;
                    case MasterPolicyMode.OneTime:
                        _PaymentMode = PaymentMode.OneTime;
                        break;
                    case MasterPolicyMode.Quarterly:
                        _PaymentMode = PaymentMode.Quarterly;
                        break;
                    case MasterPolicyMode.Random:
                        _PaymentMode = PaymentMode.Random;
                        break;
                    default:
                        break;
                }
            }           
            catch (Exception ex)
            {
                ActionLogger.Logger.WriteLog("Issue in ConvertMode MasterPolicyMode parameter " + ex.InnerException.ToString(), true);
                ActionLogger.Logger.WriteLog("Issue in ConvertMode MasterPolicyMode parameter " + ex.Message.ToString(), true);
            }
            return _PaymentMode;
        }

        public static PaymentMode ConvertMode(int MonthMode)
        {
            PaymentMode _PaymentMode = PaymentMode.Monthly;
            try
            {
                switch (MonthMode)
                {
                    case 0:
                        _PaymentMode = PaymentMode.Monthly;
                        break;
                    case 1:
                        _PaymentMode = PaymentMode.Quarterly;
                        break;
                    case 2:
                        _PaymentMode = PaymentMode.HalfYearly;
                        break;
                    case 3:
                        _PaymentMode = PaymentMode.Yearly;
                        break;
                    case 4:
                        _PaymentMode = PaymentMode.OneTime;
                        break;
                    case 5:
                        _PaymentMode = PaymentMode.Random;
                        break;
                    default:
                        break;
                }
            }
            catch (Exception ex)
            {                
                ActionLogger.Logger.WriteLog("Issue in ConvertMode int parameter " + ex.InnerException.ToString(), true);
                ActionLogger.Logger.WriteLog("Issue in ConvertMode int parameter " + ex.Message.ToString(), true);
            }
            return _PaymentMode;
        }


        /// <summary>
        /// Acme created - to cross check if incoming/outgoing payment entry is equal 
        /// after payment entry is made.
        /// </summary>
        /// <param name="PaymentEntryID"></param>
        /// <returns></returns>
       static  bool CheckIsIncomingPaymentEqualsOutgoing(Guid PaymentEntryID)
        {
            try
            {
                DLinq.CommissionDepartmentEntities ctx = new DLinq.CommissionDepartmentEntities(); //create your entity object here
                EntityConnection ec = (EntityConnection)ctx.Connection;
                SqlConnection sc = (SqlConnection)ec.StoreConnection; //get the SQLConnection that your entity object would use
                string adoConnStr = sc.ConnectionString;

                using (SqlConnection con = new SqlConnection(adoConnStr))
                {
                    string sql = "select  po.paymentEntryID, SUM (paidAmount) as sumOutgoing, TotalPayment from PolicyOutgoingPayments po inner join PolicyPaymentEntries pe on po.PaymentEntryId = pe.PaymentEntryId where pe.PaymentEntryId = '" + PaymentEntryID + "' group by po.PaymentEntryId, TotalPayment ";
                    SqlCommand cmd = new SqlCommand(sql, con);
                    con.Open();
                    double TruncationError = Convert.ToDouble(Masters.SystemConstant.GetKeyValue("TruncationError"));
                    SqlDataReader dr = cmd.ExecuteReader();
                    while (dr.Read())
                    {
                        string strSum = Convert.ToString(dr["sumOutgoing"]);
                        string strTotal = Convert.ToString(dr["TotalPayment"]);
                        double sum = 0;
                        double total = 0;
                        double.TryParse(strSum, out sum);
                        double.TryParse(strTotal, out total);
                        if (Math.Abs(sum - total) > TruncationError)
                        {
                            ActionLogger.Logger.WriteLog(DateTime.Now.ToString() + "Incoming and outgoing mismatch, PaymentEntryID: " + PaymentEntryID + ", sumOutgoing - " + strSum + ", totalIncoming: " + total + ", truncationError: " + TruncationError, true);
                            return false;
                        }
                    }

                }
            }
            catch (Exception ex)
            {
                ActionLogger.Logger.WriteLog("CheckIsIncomingPaymentEqualsOutgoing ex.StackTrace :" + ex.StackTrace.ToString(), true);
            }
            return true;
        }

        /// <summary>
        ///Entry in Policy OutGoing Payment
        /// </summary>
        /// <param name="IsPaymentToHO"></param>
        /// <param name="PaymentEntryID"></param>
        /// <param name="PolicyId"></param>
        /// <param name="LicenseeId"></param>
        /// <param name="PaidAmount"></param>
        public static void EntryInPolicyOutGoingPayment(bool IsPaymentToHO, Guid PaymentEntryID, Guid PolicyId, DateTime? InvoiceDate, Guid LicenseeId)
        {
            try
            {
                ActionLogger.Logger.WriteLog("Start EntryInPolicyOutGoingPayment with PaymentEntryID: " + PaymentEntryID + ",  IsPaymentToHO: " + IsPaymentToHO, true);

                if (PolicyId == null || PolicyId == Guid.Empty)
                {
                    ActionLogger.Logger.WriteLog("PolicyID is null for PaymentEntryID: " + PaymentEntryID + ", returning ", true);
                    return;
                }

                 decimal Premium=0;
                 decimal DollerPerUnits = 0;

                PolicySchedule _PolicySchedule = CheckForOutgoingTypeOfSchedule(PolicyId);
                //PolicyDetailsData _Policy = GetPolicy(PolicyId);
                PolicyDetailsData _Policy = GetPolicyEffectiveDate(PolicyId);
                PolicyPaymentEntriesPost _PolicyPaymentEntriesPost = PolicyPaymentEntriesPost.GetPolicyPaymentEntry(PaymentEntryID);

                if (_PolicyPaymentEntriesPost == null)
                {
                    ActionLogger.Logger.WriteLog("PolicypaymententriesPost is nul,PaymentEntryID: " + PaymentEntryID + " returning :", true);
                    return;
                }               
                Premium = _PolicyPaymentEntriesPost.PaymentRecived;
                DollerPerUnits = _PolicyPaymentEntriesPost.DollerPerUnit;

                FirstYrRenewalYr IsFirstYr = FirstYrRenewalYr.None;

                List<OutGoingPayment> _outgoingSchedule = GetBasicOutGoingScheduleOfPolicy(PolicyId);
                if (_Policy != null)
                {
                    if (_Policy.OriginalEffectiveDate != null)
                    {
                        IsFirstYr = IsUseFirstYearForOutGoing(InvoiceDate, _Policy.OriginalEffectiveDate, PolicyId);
                    }
                }

                if (IsPaymentToHO)
                {
                    double Amount = Convert.ToDouble(_PolicyPaymentEntriesPost.TotalPayment);
                    Guid _HouseOwnerID = GetPolicyHouseOwner(LicenseeId);

                    //Instruction to add PolicyOutgoingPayments table .

                    //if PercentageOfCommission then enter into "Payment" column
                    //if PercentageOfPremium then enbter into "Premium" Column
                    //Enter value either "Payment" column Or "Premium" Column

                    PolicyOutgoingDistribution _PolicyOutgoingDistribution = new PolicyOutgoingDistribution();
                    _PolicyOutgoingDistribution.OutgoingPaymentId = Guid.NewGuid();
                    _PolicyOutgoingDistribution.PaymentEntryId = PaymentEntryID;
                    _PolicyOutgoingDistribution.RecipientUserCredentialId = _HouseOwnerID;


                    if (_outgoingSchedule.Count == 0)
                    {
                        //When creating pending policy then out going and incoming shedule is not there 
                        //in that case all the payment will distribute to house owner
                        //then out going payment percentage will be 100 %
                        double DefaultDbPaymentPercent = 100.0;
                        _PolicyOutgoingDistribution.Payment = DefaultDbPaymentPercent;

                        _PolicyOutgoingDistribution.PaidAmount = Amount;
                        _PolicyOutgoingDistribution.IsPaid = false;
                        _PolicyOutgoingDistribution.CreatedOn = DateTime.Now;
                        PolicyOutgoingDistribution.AddUpdateOutgoingPaymentEntry(_PolicyOutgoingDistribution);
                    }
                    else
                    {
                        //Added on March 28, Acme, to fix null paid amount issue
                        if (IsFirstYr == FirstYrRenewalYr.None)
                        {
                            IsFirstYr = IsUseFirstYear(InvoiceDate, _Policy.OriginalEffectiveDate, PolicyId);
                        }
                        if (_outgoingSchedule.FirstOrDefault().ScheduleTypeId == (int)MasterBasicOutgoinSchedule.PercentageOfCommission) //Payment column
                        {
                            foreach (OutGoingPayment _OutGoingPayment in _outgoingSchedule)
                            {
                                _PolicyOutgoingDistribution.OutgoingPaymentId = Guid.NewGuid();
                                _PolicyOutgoingDistribution.PaymentEntryId = PaymentEntryID;
                                _PolicyOutgoingDistribution.RecipientUserCredentialId = _OutGoingPayment.PayeeUserCredentialId;
                                if (IsFirstYr == FirstYrRenewalYr.FirstYear)
                                {
                                    _PolicyOutgoingDistribution.PaidAmount = Convert.ToDouble(_PolicyPaymentEntriesPost.TotalPayment) * _OutGoingPayment.FirstYearPercentage / 100;
                                    _PolicyOutgoingDistribution.Payment = _OutGoingPayment.FirstYearPercentage;

                                }
                                else if (IsFirstYr == FirstYrRenewalYr.Renewal)
                                {
                                    _PolicyOutgoingDistribution.PaidAmount = Convert.ToDouble(_PolicyPaymentEntriesPost.TotalPayment) * _OutGoingPayment.RenewalPercentage / 100;
                                    _PolicyOutgoingDistribution.Payment = _OutGoingPayment.RenewalPercentage;
                                }
                               
                              //  _PolicyOutgoingDistribution.PaidAmount = Amount;
                                _PolicyOutgoingDistribution.IsPaid = false;
                                _PolicyOutgoingDistribution.CreatedOn = DateTime.Now;
                                PolicyOutgoingDistribution.AddUpdateOutgoingPaymentEntry(_PolicyOutgoingDistribution);
                            }
                        }
                        else //premium column
                        {
                            foreach (OutGoingPayment _OutGoingPayment in _outgoingSchedule)
                            {
                                //Acme added to add all outgoing entries , bug - only one was going initially
                                     _PolicyOutgoingDistribution.OutgoingPaymentId = Guid.NewGuid();
                                     _PolicyOutgoingDistribution.PaymentEntryId = PaymentEntryID;
                                     _PolicyOutgoingDistribution.RecipientUserCredentialId = _OutGoingPayment.PayeeUserCredentialId;

                                     if (IsFirstYr == FirstYrRenewalYr.FirstYear)
                                     {
                                         _PolicyOutgoingDistribution.PaidAmount = Convert.ToDouble(Premium) * _OutGoingPayment.FirstYearPercentage.Value / 100;
                                         _PolicyOutgoingDistribution.Premium = _OutGoingPayment.FirstYearPercentage;
                                     }
                                     else
                                     {
                                         _PolicyOutgoingDistribution.PaidAmount = Convert.ToDouble(Premium) * _OutGoingPayment.RenewalPercentage.Value / 100;
                                         _PolicyOutgoingDistribution.Premium = _OutGoingPayment.RenewalPercentage;
                                     }
                               // _PolicyOutgoingDistribution.PaidAmount = Amount;
                                _PolicyOutgoingDistribution.IsPaid = false;
                                _PolicyOutgoingDistribution.CreatedOn = DateTime.Now;
                                PolicyOutgoingDistribution.AddUpdateOutgoingPaymentEntry(_PolicyOutgoingDistribution);
                            }
                        }
                    }

                    //_PolicyOutgoingDistribution.PaidAmount = Amount;
                    //_PolicyOutgoingDistribution.IsPaid = false;
                    //_PolicyOutgoingDistribution.CreatedOn = DateTime.Now;
                    //PolicyOutgoingDistribution.AddUpdateOutgoingPaymentEntry(_PolicyOutgoingDistribution);
                }
                else
                {
                    if (_PolicySchedule == PolicySchedule.Basic)
                    {
                        //Need to discuss on with eric
                        if (IsFirstYr == FirstYrRenewalYr.None)
                        {
                            IsFirstYr = IsUseFirstYear(InvoiceDate, _Policy.OriginalEffectiveDate, PolicyId);
                        }

                        if (_outgoingSchedule == null) return;
                        if (_outgoingSchedule.Count == 0) return;

                        if (_outgoingSchedule.FirstOrDefault().ScheduleTypeId == (int)MasterBasicOutgoinSchedule.PercentageOfCommission)
                        {
                            foreach (OutGoingPayment _OutGoingPayment in _outgoingSchedule)
                            {
                                PolicyOutgoingDistribution _PolicyOutgoingDistribution = new PolicyOutgoingDistribution();
                                _PolicyOutgoingDistribution.OutgoingPaymentId = Guid.NewGuid();
                                _PolicyOutgoingDistribution.PaymentEntryId = PaymentEntryID;
                                _PolicyOutgoingDistribution.RecipientUserCredentialId = _OutGoingPayment.PayeeUserCredentialId;

                                if (IsFirstYr == FirstYrRenewalYr.FirstYear)
                                {
                                    _PolicyOutgoingDistribution.PaidAmount = Convert.ToDouble(_PolicyPaymentEntriesPost.TotalPayment) * _OutGoingPayment.FirstYearPercentage / 100;
                                    _PolicyOutgoingDistribution.Payment = _OutGoingPayment.FirstYearPercentage;

                                }
                                else if (IsFirstYr == FirstYrRenewalYr.Renewal)
                                {
                                    _PolicyOutgoingDistribution.PaidAmount = Convert.ToDouble(_PolicyPaymentEntriesPost.TotalPayment) * _OutGoingPayment.RenewalPercentage / 100;
                                    _PolicyOutgoingDistribution.Payment = _OutGoingPayment.RenewalPercentage;
                                }
                                _PolicyOutgoingDistribution.CreatedOn = DateTime.Now;
                                _PolicyOutgoingDistribution.IsPaid = false;
                                //_PolicyOutgoingDistribution.CreatedOn = DateTime.Today;
                                //_PolicyOutgoingDistribution.ReferencedOutgoingScheduleId = _OutGoingPayment.OutgoingScheduleId;
                                PolicyOutgoingDistribution.AddUpdateOutgoingPaymentEntry(_PolicyOutgoingDistribution);

                            }

                        }
                        else if (_outgoingSchedule.FirstOrDefault().ScheduleTypeId == (int)MasterBasicOutgoinSchedule.PercentageOfPremium)
                        {

                            foreach (OutGoingPayment _OutGoingPayment in _outgoingSchedule)
                            {
                                PolicyOutgoingDistribution _PolicyOutgoingDistribution = new PolicyOutgoingDistribution();
                                _PolicyOutgoingDistribution.OutgoingPaymentId = Guid.NewGuid();
                                _PolicyOutgoingDistribution.PaymentEntryId = PaymentEntryID;

                                _PolicyOutgoingDistribution.RecipientUserCredentialId = _OutGoingPayment.PayeeUserCredentialId;

                                if (IsFirstYr == FirstYrRenewalYr.FirstYear)
                                {
                                    _PolicyOutgoingDistribution.PaidAmount = Convert.ToDouble(Premium) * _OutGoingPayment.FirstYearPercentage.Value / 100;
                                    _PolicyOutgoingDistribution.Premium = _OutGoingPayment.FirstYearPercentage;
                                }
                                else if (IsFirstYr == FirstYrRenewalYr.Renewal)
                                {
                                    _PolicyOutgoingDistribution.PaidAmount = Convert.ToDouble(Premium) * _OutGoingPayment.RenewalPercentage.Value / 100;
                                    _PolicyOutgoingDistribution.Premium = _OutGoingPayment.RenewalPercentage;

                                }
                                _PolicyOutgoingDistribution.CreatedOn = DateTime.Now;
                                _PolicyOutgoingDistribution.IsPaid = false;
                                //_PolicyOutgoingDistribution.ReferencedOutgoingScheduleId = _OutGoingPayment.OutgoingScheduleId;
                                PolicyOutgoingDistribution.AddUpdateOutgoingPaymentEntry(_PolicyOutgoingDistribution);

                            }
                        }

                    }
                    else if (_PolicySchedule == PolicySchedule.Advance)
                    {
                        PolicyOutgoingSchedule _AdvanceSchedule = GetAdvanceOutgoingScheduleOfPolicy(PolicyId);
                        _AdvanceSchedule = FillNullDateWithMaxSystemDate(_AdvanceSchedule);


                        int AdvanceOutgoingType = _AdvanceSchedule.ScheduleTypeId;

                        if (AdvanceOutgoingType == (int)MasterAdvanceScheduleType.PercentageofPremium_scale)
                        {
                            List<OutgoingScheduleEntry> _OutgoingShedule = _AdvanceSchedule.OutgoingScheduleList
                                .Where(p => p.EffectiveFromDate <= InvoiceDate)
                                .Where(p => p.EffectiveToDate >= InvoiceDate)
                               .Where(p => p.FromRange <= double.Parse(_PolicyPaymentEntriesPost.PaymentRecived.ToString())).ToList();

                            List<Guid> PayeeList = _OutgoingShedule.Select(p => p.PayeeUserCredentialId).Distinct().ToList();

                            foreach (Guid payee in PayeeList)
                            {

                                decimal TempPremiumAmt = Premium;
                                List<OutgoingScheduleEntry> _tempOutg = _OutgoingShedule.Where(p => p.PayeeUserCredentialId == payee).OrderBy(p => p.FromRange).ToList();
                                foreach (OutgoingScheduleEntry outg in _tempOutg)
                                {
                                    PolicyOutgoingDistribution _PolicyOutgoingDistribution = new PolicyOutgoingDistribution();
                                    _PolicyOutgoingDistribution.OutgoingPaymentId = Guid.NewGuid();
                                    _PolicyOutgoingDistribution.PaymentEntryId = PaymentEntryID;
                                    _PolicyOutgoingDistribution.RecipientUserCredentialId = outg.PayeeUserCredentialId;


                                    if (TempPremiumAmt >
                                   Convert.ToDecimal(outg.ToRange
                                   -
                                   (outg.FromRange + GetValidFromForcalCulation(outg.FromRange))
                                   ))
                                    {
                                        _PolicyOutgoingDistribution.PaidAmount = (outg.ToRange - (outg.FromRange + GetValidFromForcalCulation(outg.FromRange))) * (outg.Rate) / 100;
                                        _PolicyOutgoingDistribution.Premium = outg.Rate;
                                        TempPremiumAmt = TempPremiumAmt - Convert.ToDecimal(outg.ToRange.Value);
                                    }
                                    else
                                    {
                                        _PolicyOutgoingDistribution.PaidAmount = Convert.ToDouble(TempPremiumAmt) * outg.Rate / 100;
                                        _PolicyOutgoingDistribution.Premium = outg.Rate;
                                        TempPremiumAmt = 0;
                                        break;
                                    }
                                    _PolicyOutgoingDistribution.IsPaid = false;
                                    _PolicyOutgoingDistribution.CreatedOn = DateTime.Now;
                                    //_PolicyOutgoingDistribution.ReferencedOutgoingAdvancedScheduleId = outg.CoveragesScheduleId;
                                    PolicyOutgoingDistribution.AddUpdateOutgoingPaymentEntry(_PolicyOutgoingDistribution);
                                }
                            }
                        }
                        else if (AdvanceOutgoingType == (int)MasterAdvanceScheduleType.PercentageofPremium_target)
                        {
                            List<OutgoingScheduleEntry> _OutgoingShedule = _AdvanceSchedule.OutgoingScheduleList
                                .Where(p => p.EffectiveFromDate <= InvoiceDate)
                                .Where(p => p.EffectiveToDate >= InvoiceDate)
                                .Where(p => p.FromRange <= double.Parse(_PolicyPaymentEntriesPost.PaymentRecived.ToString()))
                                .Where(p => p.ToRange >= double.Parse(_PolicyPaymentEntriesPost.PaymentRecived.ToString())).ToList();

                            foreach (OutgoingScheduleEntry outg in _OutgoingShedule)
                            {
                                PolicyOutgoingDistribution _PolicyOutgoingDistribution = new PolicyOutgoingDistribution();
                                _PolicyOutgoingDistribution.OutgoingPaymentId = Guid.NewGuid();
                                _PolicyOutgoingDistribution.PaymentEntryId = PaymentEntryID;
                                _PolicyOutgoingDistribution.RecipientUserCredentialId = outg.PayeeUserCredentialId;
                                _PolicyOutgoingDistribution.PaidAmount = double.Parse(Premium.ToString()) * outg.Rate / 100;

                                _PolicyOutgoingDistribution.Premium = outg.Rate;

                                _PolicyOutgoingDistribution.CreatedOn = DateTime.Now;
                                //_PolicyOutgoingDistribution.ReferencedOutgoingAdvancedScheduleId = outg.CoveragesScheduleId;
                                _PolicyOutgoingDistribution.IsPaid = false;
                                PolicyOutgoingDistribution.AddUpdateOutgoingPaymentEntry(_PolicyOutgoingDistribution);
                            }

                        }
                        else if (AdvanceOutgoingType == (int)MasterAdvanceScheduleType.PerHeadFee_scale)
                        {
                            List<OutgoingScheduleEntry> _OutgoingShedule = _AdvanceSchedule.OutgoingScheduleList
                                .Where(p => p.EffectiveFromDate <= InvoiceDate)
                                .Where(p => p.EffectiveToDate >= InvoiceDate)
                                .Where(p => p.FromRange <= double.Parse(_PolicyPaymentEntriesPost.NumberOfUnits.ToString())).ToList();

                            int NumberOfHead = _PolicyPaymentEntriesPost.NumberOfUnits;
                            List<Guid> PayeeList = _OutgoingShedule.Select(p => p.PayeeUserCredentialId).Distinct().ToList();

                            foreach (Guid payee in PayeeList)
                            {
                                int temoNUmberOfHead = NumberOfHead;
                                List<OutgoingScheduleEntry> _tempOutg = _OutgoingShedule.Where(p => p.PayeeUserCredentialId == payee).OrderBy(p => p.FromRange).ToList();
                                foreach (OutgoingScheduleEntry outg in _tempOutg)
                                {
                                    PolicyOutgoingDistribution _PolicyOutgoingDistribution = new PolicyOutgoingDistribution();
                                    _PolicyOutgoingDistribution.OutgoingPaymentId = Guid.NewGuid();
                                    _PolicyOutgoingDistribution.PaymentEntryId = PaymentEntryID;
                                    _PolicyOutgoingDistribution.RecipientUserCredentialId = outg.PayeeUserCredentialId;
                                    if (temoNUmberOfHead > (outg.ToRange - (outg.FromRange + GetValidFromForcalCulation(outg.FromRange))))
                                    {
                                        _PolicyOutgoingDistribution.PaidAmount = ((outg.ToRange - (outg.FromRange + GetValidFromForcalCulation(outg.FromRange)))) * (outg.Rate);
                                        _PolicyOutgoingDistribution.OutGoingPerUnit = Convert.ToDecimal(outg.Rate);
                                        temoNUmberOfHead = temoNUmberOfHead - Convert.ToInt32(outg.ToRange);
                                    }
                                    else
                                    {
                                        _PolicyOutgoingDistribution.PaidAmount = temoNUmberOfHead * outg.Rate;
                                        _PolicyOutgoingDistribution.OutGoingPerUnit = Convert.ToDecimal(outg.Rate);

                                        temoNUmberOfHead = 0;
                                        break;
                                    }
                                    _PolicyOutgoingDistribution.IsPaid = false;
                                    _PolicyOutgoingDistribution.CreatedOn = DateTime.Now;
                                    //_PolicyOutgoingDistribution.ReferencedOutgoingAdvancedScheduleId = outg.CoveragesScheduleId;
                                    PolicyOutgoingDistribution.AddUpdateOutgoingPaymentEntry(_PolicyOutgoingDistribution);
                                }
                            }
                        }
                        else if (AdvanceOutgoingType == (int)MasterAdvanceScheduleType.PerHeadFee_target)
                        {

                            List<OutgoingScheduleEntry> _OutgoingShedule = _AdvanceSchedule.OutgoingScheduleList
                                .Where(p => p.EffectiveFromDate <= InvoiceDate)
                                .Where(p => p.EffectiveToDate >= InvoiceDate)
                                .Where(p => p.FromRange <= double.Parse(_PolicyPaymentEntriesPost.NumberOfUnits.ToString()))
                                .Where(p => p.ToRange >= double.Parse(_PolicyPaymentEntriesPost.NumberOfUnits.ToString())).ToList();
                            foreach (OutgoingScheduleEntry outg in _OutgoingShedule)
                            {
                                PolicyOutgoingDistribution _PolicyOutgoingDistribution = new PolicyOutgoingDistribution();
                                _PolicyOutgoingDistribution.OutgoingPaymentId = Guid.NewGuid();
                                _PolicyOutgoingDistribution.PaymentEntryId = PaymentEntryID;
                                _PolicyOutgoingDistribution.RecipientUserCredentialId = outg.PayeeUserCredentialId;
                                _PolicyOutgoingDistribution.PaidAmount = outg.Rate * _PolicyPaymentEntriesPost.NumberOfUnits;

                                _PolicyOutgoingDistribution.OutGoingPerUnit = Convert.ToDecimal(outg.Rate);
                                _PolicyOutgoingDistribution.IsPaid = false;
                                _PolicyOutgoingDistribution.CreatedOn = DateTime.Now;
                                // _PolicyOutgoingDistribution.ReferencedOutgoingAdvancedScheduleId = outg.CoveragesScheduleId;
                                PolicyOutgoingDistribution.AddUpdateOutgoingPaymentEntry(_PolicyOutgoingDistribution);
                            }

                        }
                        else if (AdvanceOutgoingType == (int)MasterAdvanceScheduleType.FlatDollar_modal)
                        {
                            List<OutgoingScheduleEntry> _OutgoingShedule = _AdvanceSchedule.OutgoingScheduleList
                                .Where(p => p.EffectiveFromDate <= InvoiceDate)
                                .Where(p => p.EffectiveToDate >= InvoiceDate).ToList();
                            foreach (OutgoingScheduleEntry outg in _OutgoingShedule)
                            {
                                PolicyOutgoingDistribution _PolicyOutgoingDistribution = new PolicyOutgoingDistribution();
                                _PolicyOutgoingDistribution.OutgoingPaymentId = Guid.NewGuid();
                                _PolicyOutgoingDistribution.PaymentEntryId = PaymentEntryID;
                                _PolicyOutgoingDistribution.RecipientUserCredentialId = outg.PayeeUserCredentialId;
                                _PolicyOutgoingDistribution.PaidAmount = outg.Rate;
                                _PolicyOutgoingDistribution.IsPaid = false;
                                _PolicyOutgoingDistribution.CreatedOn = DateTime.Now;
                                //_PolicyOutgoingDistribution.ReferencedOutgoingAdvancedScheduleId = outg.CoveragesScheduleId;
                                PolicyOutgoingDistribution.AddUpdateOutgoingPaymentEntry(_PolicyOutgoingDistribution);
                            }

                        }
                    }
                }

                //Acme added: Feb 02, 2017
                try
                {
                    if (!CheckIsIncomingPaymentEqualsOutgoing(PaymentEntryID))
                    {
                        string body = "Outgoing payments are not equal to incoming payment for following details : ";
                        body += "\nPolicyID: " + PolicyId;
                        body += "\nIsPaymentToHO: " + IsPaymentToHO;
                        body += "\nPaymentEntryID: " + PaymentEntryID;
                        body += "\nInvoiceDate: " + InvoiceDate;
                        body += "\nLicenseeId: " + LicenseeId;
                        MailServerDetail.sendMail("deudev@acmeminds.com", "Commissions Alert: Outgoing payments mismatch incoming payment", body);
                    }
                }
                catch (Exception ex)
                {
                    ActionLogger.Logger.WriteLog("EntryInPolicyOutGoingPayment exception sending mail :" + ex.Message, true);
                }

            }
            catch (Exception ex)
            {
                ActionLogger.Logger.WriteLog("******************Start EntryInPolicyOutGoingPayment**************", true);
                ActionLogger.Logger.WriteLog("EntryInPolicyOutGoingPayment ex.StackTrace :" + ex.StackTrace.ToString(), true);
                ActionLogger.Logger.WriteLog("EntryInPolicyOutGoingPayment ex.InnerException :" + ex.Message.ToString(), true);
                ActionLogger.Logger.WriteLog("******************End EntryInPolicyOutGoingPayment**************", true);
            }

        }

        private static PolicyOutgoingSchedule GetAllPayeeForDistributeAmount(Guid PolicyId)
        {
            return OutgoingSchedule.GetPolicyOutgoingSchedule(PolicyId);
        }

        public static Guid GetPolicyHouseOwner(Guid PolicyLicenID)
        {
            using (DLinq.CommissionDepartmentEntities DataModel = Entity.DataModel)
            {
                Guid UserCredID = (from f in DataModel.UserCredentials

                                   where (f.LicenseeId == PolicyLicenID)
                                   select f).Where(f => f.IsHouseAccount == true).FirstOrDefault().UserCredentialId;
                return UserCredID;

            }


        }

        private static Guid EntryInPoicyPamentEntries(DEUFields deuFields, BasicInformationForProcess _BasicInformationForProcess)
        {
            Guid PolicyEnteryID = Guid.NewGuid();
            Guid FollowUpID = Guid.Empty;
            try
            {
                using (DLinq.CommissionDepartmentEntities DataModel = Entity.DataModel)
                {                    
                    var _policyentry = (from p in DataModel.PolicyPaymentEntries where (p.PaymentEntryId == PolicyEnteryID) select p).FirstOrDefault();
                    if (_policyentry == null)
                    {
                        _policyentry = new DLinq.PolicyPaymentEntry();
                        _policyentry.PaymentEntryId = PolicyEnteryID;
                        _policyentry.StatementReference.Value = (from f in DataModel.Statements where f.StatementId == deuFields.StatementId select f).FirstOrDefault();
                        _policyentry.PolicyReference.Value = (from f in DataModel.Policies where f.PolicyId == _BasicInformationForProcess.PolicyId select f).FirstOrDefault();
                        _policyentry.InvoiceDate = deuFields.DeuData.InvoiceDate;
                        _policyentry.PaymentRecived = deuFields.DeuData.PaymentRecived;
                        _policyentry.CommissionPercentage = deuFields.DeuData.CommissionPercentage;
                        _policyentry.NumberOfUnits = deuFields.DeuData.NoOfUnits;
                        _policyentry.Fee = deuFields.DeuData.Fee;
                        _policyentry.SplitPercentage = deuFields.DeuData.SplitPer;
                        _policyentry.TotalPayment = deuFields.DeuData.CommissionTotal;
                        //_policyentry.CreatedOn = DateTime.Today;
                        _policyentry.CreatedOn = DateTime.Now;
                        _policyentry.CreatedBy = deuFields.CurrentUser;
                        _policyentry.MasterPostStatuReference.Value = (from f in DataModel.MasterPostStatus where f.PostStatusID == (int)_BasicInformationForProcess.PostStatus select f).FirstOrDefault();//Fill the Data in Master Post Status
                        _policyentry.Bonus = deuFields.DeuData.Bonus;
                        _policyentry.DollerPerUnit = deuFields.DeuData.DollerPerUnit;
                        _policyentry.DEUEntryId = deuFields.DeuData.DEUENtryID;

                        DataModel.AddToPolicyPaymentEntries(_policyentry);
                    }
                    DataModel.SaveChanges();
                   
                }
            }
            catch (Exception ex)
            {
                ActionLogger.Logger.WriteLog("EntryInPoicyPamentEntries :" + ex.Message.ToString(), true);
            }
            return PolicyEnteryID;
        }

        //public static Guid CreateNewPendingPolicy(DEUFields deuFields, string strUnlinkClient)
        //{
        //    Guid NewPendingPolicyID = Guid.NewGuid();
        //    try
        //    {

        //        using (DLinq.CommissionDepartmentEntities DataModel = Entity.DataModel)
        //        {
        //            var _policy = (from p in DataModel.Policies where (p.PolicyId == NewPendingPolicyID) select p).FirstOrDefault();
        //            if (_policy == null)
        //            {
        //                _policy = new DLinq.Policy();
        //                _policy.PolicyId = NewPendingPolicyID;
        //                _policy.PolicyStatusId = (int)_PolicyStatus.Pending;

        //                if (string.IsNullOrEmpty(strUnlinkClient))
        //                {
        //                    _policy.ClientReference.Value = CreateNewClientForPendingPolicy(deuFields.DeuData, DataModel, deuFields.LicenseeId.Value);
        //                }
        //                else
        //                {
        //                    //Add New client for unlink policy
        //                    _policy.ClientReference.Value = CreateNewClientForUnlikingPolicy(deuFields.DeuData, DataModel, deuFields.LicenseeId.Value, strUnlinkClient);
        //                }
        //                //_policy.ClientReference.Value = CreateNewClientForPendingPolicy(deuFields.DeuData, DataModel, deuFields.LicenseeId.Value);

        //                _policy.LicenseeReference.Value = (from f in DataModel.Licensees where f.LicenseeId == deuFields.LicenseeId select f).FirstOrDefault();

        //                //_policy.CreatedOn = DateTime.Today;
        //                _policy.CreatedOn = DateTime.Now;

        //                _policy.CreatedBy = GetPolicyHouseOwner((Guid)deuFields.LicenseeId);
        //                _policy.IsTrackPayment = true;
        //                _policy.PolicyType = "New";

        //                if (deuFields != null)
        //                {
        //                    if (deuFields.DeuData != null)
        //                    {
        //                        if (deuFields.DeuData.PolicyNumber != null)
        //                            _policy.PolicyNumber = deuFields.DeuData.PolicyNumber;

        //                        if (deuFields.DeuData.PolicyMode != null)
        //                            _policy.PolicyModeId = deuFields.DeuData.PolicyMode;

        //                        else
        //                            _policy.PolicyModeId = (int)MasterPolicyMode.Monthly;

        //                        if (deuFields.DeuData.CompTypeID != null)
        //                            _policy.IncomingPaymentTypeId = deuFields.DeuData.CompTypeID;
        //                        else
        //                            _policy.IncomingPaymentTypeId = (int)MasterIncoimgPaymentType.commission;

        //                        if (deuFields.DeuData.SplitPer != null)
        //                            _policy.SplitPercentage = deuFields.DeuData.SplitPer;

        //                        if (deuFields.DeuData.Insured != null)
        //                        {
        //                            if (string.IsNullOrEmpty(deuFields.DeuData.Insured))
        //                                _policy.Insured = _policy.Client.Name;
        //                            else
        //                                _policy.Insured = deuFields.DeuData.Insured;
        //                        }

        //                        if (deuFields.DeuData.PayorId != null)
        //                            _policy.PayorId = deuFields.DeuData.PayorId;

        //                        if (deuFields.DeuData.CarrierID != null)
        //                            _policy.CarrierId = deuFields.DeuData.CarrierID;

        //                        if (deuFields.DeuData.CoverageID != null)
        //                            _policy.CoverageId = deuFields.DeuData.CoverageID;

        //                        //Added product type
        //                        if (deuFields.DeuData.ProductName != null)
        //                            _policy.ProductType = deuFields.DeuData.ProductName;

        //                    }
        //                }
        //                Carrier carr = Carrier.GetPayorCarrier(_policy.PayorId.Value, _policy.CarrierId ?? Guid.Empty);
        //                if (carr != null)
        //                {
        //                    _policy.IsTrackIncomingPercentage = carr.IsTrackIncomingPercentage;
        //                    _policy.IsTrackMissingMonth = carr.IsTrackMissingMonth;
        //                }
        //                else
        //                {
        //                    _policy.IsTrackIncomingPercentage = true;
        //                    _policy.IsTrackMissingMonth = true;
        //                }
        //                _policy.IsIncomingBasicSchedule = true;
        //                _policy.IsOutGoingBasicSchedule = true;

        //                //Added 
        //                try
        //                {
        //                    if (string.IsNullOrEmpty(_policy.PolicyNumber))
        //                    {
        //                        _policy.PolicyNumber = _policy.Insured;
        //                    }

        //                    if ((_policy.PolicyClientId == null) || (_policy.PolicyClientId==Guid.Empty))
        //                    {
        //                        _policy.PolicyClientId = _policy.Client.ClientId;
        //                    }
                       
        //                }
        //                catch
        //                {
        //                }

        //                ActionLogger.Logger.WriteLog("CreateNewPendingPolicy _policy.PolicyClientId :" + _policy.PolicyClientId.ToString(), true);
        //                ActionLogger.Logger.WriteLog("CreateNewPendingPolicy _policy.PolicyNumber :" + _policy.PolicyNumber.ToString(), true);

        //                DataModel.AddToPolicies(_policy);
        //            }
        //            DataModel.SaveChanges();

        //            PolicyToolIncommingShedule _PolicyToolIncommingShedule = new PolicyToolIncommingShedule();
        //            _PolicyToolIncommingShedule.FirstYearPercentage = 0;
        //            _PolicyToolIncommingShedule.RenewalPercentage = 0;
        //            _PolicyToolIncommingShedule.ScheduleTypeId = 1;
        //            _PolicyToolIncommingShedule.IncomingScheduleId = Guid.NewGuid();
        //            _PolicyToolIncommingShedule.PolicyId = _policy.PolicyId;
        //            _PolicyToolIncommingShedule.AddUpdate();

        //            try
        //            {
        //                PolicyToLearnPost.AddUpdatPolicyToLearn(_policy, deuFields);
        //                LearnedToPolicyPost.AddUpdateLearnedToPolicy(_policy.PolicyId);
        //                DEULearnedPost.AddDataDeuToLearnedPost(deuFields.DeuData);
        //                Policy.AddUpdatePolicyHistory(_policy, DataModel);
        //                PolicyLearnedField.AddUpdateHistoryLearned(_policy.PolicyId);
        //            }
        //            catch
        //            {
        //            }
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        ActionLogger.Logger.WriteLog("CreateNewPendingPolicy :" + ex.InnerException.ToString(), true);
        //    }
        //    return NewPendingPolicyID;
        //}

        public static Guid CreateNewPendingPolicy(DEUFields deuFields, string strUnlinkClient)
        {
            Guid NewPendingPolicyID = Guid.NewGuid();
            try
            {

                using (DLinq.CommissionDepartmentEntities DataModel = Entity.DataModel)
                {
                    var _policy = (from p in DataModel.Policies where (p.PolicyId == NewPendingPolicyID) select p).FirstOrDefault();
                    if (_policy == null)
                    {
                        _policy = new DLinq.Policy();
                        _policy.PolicyId = NewPendingPolicyID;
                        _policy.PolicyStatusId = (int)_PolicyStatus.Pending;

                        if (string.IsNullOrEmpty(strUnlinkClient))
                        {
                            _policy.ClientReference.Value = CreateNewClientForPendingPolicy(deuFields.DeuData, DataModel, deuFields.LicenseeId.Value);
                        }
                        else
                        {
                            //Add New client for unlink policy
                            _policy.ClientReference.Value = CreateNewClientForUnlikingPolicy(deuFields.DeuData, DataModel, deuFields.LicenseeId.Value, strUnlinkClient);
                        }
                        //_policy.ClientReference.Value = CreateNewClientForPendingPolicy(deuFields.DeuData, DataModel, deuFields.LicenseeId.Value);

                        _policy.LicenseeReference.Value = (from f in DataModel.Licensees where f.LicenseeId == deuFields.LicenseeId select f).FirstOrDefault();

                        //_policy.CreatedOn = DateTime.Today;
                        _policy.CreatedOn = DateTime.Now;

                        _policy.CreatedBy = GetPolicyHouseOwner((Guid)deuFields.LicenseeId);
                        _policy.IsTrackPayment = true;
                        _policy.PolicyType = "New";

                        if (deuFields != null)
                        {
                            if (deuFields.DeuData != null)
                            {
                                if (deuFields.DeuData.PolicyNumber != null)
                                    _policy.PolicyNumber = deuFields.DeuData.PolicyNumber;

                                if (deuFields.DeuData.PolicyMode != null)
                                    _policy.PolicyModeId = deuFields.DeuData.PolicyMode;

                                else
                                    _policy.PolicyModeId = (int)MasterPolicyMode.Monthly;

                                if (deuFields.DeuData.CompTypeID != null)
                                    _policy.IncomingPaymentTypeId = deuFields.DeuData.CompTypeID;
                                else
                                    _policy.IncomingPaymentTypeId = (int)MasterIncoimgPaymentType.commission;

                                if (deuFields.DeuData.SplitPer != null)
                                    _policy.SplitPercentage = deuFields.DeuData.SplitPer;

                                if (deuFields.DeuData.Insured != null)
                                {
                                    if (string.IsNullOrEmpty(deuFields.DeuData.Insured))
                                        _policy.Insured = _policy.Client.Name;
                                    else
                                        _policy.Insured = deuFields.DeuData.Insured;
                                }

                                if (deuFields.DeuData.PayorId != null)
                                    _policy.PayorId = deuFields.DeuData.PayorId;

                                if (deuFields.DeuData.CarrierID != null)
                                    _policy.CarrierId = deuFields.DeuData.CarrierID;

                                if (deuFields.DeuData.CoverageID != null)
                                    _policy.CoverageId = deuFields.DeuData.CoverageID;

                                //Added product type
                                if (deuFields.DeuData.ProductName != null)
                                    _policy.ProductType = deuFields.DeuData.ProductName;

                            }
                        }
                        Carrier carr = Carrier.GetPayorCarrier(_policy.PayorId.Value, _policy.CarrierId ?? Guid.Empty);
                        if (carr != null)
                        {
                            _policy.IsTrackIncomingPercentage = carr.IsTrackIncomingPercentage;
                            _policy.IsTrackMissingMonth = carr.IsTrackMissingMonth;
                        }
                        else
                        {
                            _policy.IsTrackIncomingPercentage = true;
                            _policy.IsTrackMissingMonth = true;
                        }
                        _policy.IsIncomingBasicSchedule = true;
                        _policy.IsOutGoingBasicSchedule = true;

                        //Added 
                        try
                        {
                            if (string.IsNullOrEmpty(_policy.PolicyNumber))
                            {
                                _policy.PolicyNumber = _policy.Insured;
                            }

                            if ((_policy.PolicyClientId == null) || (_policy.PolicyClientId == Guid.Empty))
                            {
                                _policy.PolicyClientId = _policy.Client.ClientId;
                            }

                        }
                        catch(Exception ex)
                        {
                            ActionLogger.Logger.WriteLog("CreateNewPendingPolicy, exception in assigning number/client:" + ex.Message, true);
                        }

                        //ActionLogger.Logger.WriteLog("CreateNewPendingPolicy _policy.PolicyClientId :" + _policy.PolicyClientId.ToString(), true);
                        //ActionLogger.Logger.WriteLog("CreateNewPendingPolicy _policy.PolicyNumber :" + _policy.PolicyNumber.ToString(), true);

                        DataModel.AddToPolicies(_policy);
                    }
                    DataModel.SaveChanges();

                    PolicyToolIncommingShedule _PolicyToolIncommingShedule = new PolicyToolIncommingShedule();
                    _PolicyToolIncommingShedule.FirstYearPercentage = 0;
                    _PolicyToolIncommingShedule.RenewalPercentage = 0;
                    _PolicyToolIncommingShedule.ScheduleTypeId = 1;
                    _PolicyToolIncommingShedule.IncomingScheduleId = Guid.NewGuid();
                    _PolicyToolIncommingShedule.PolicyId = _policy.PolicyId;
                    _PolicyToolIncommingShedule.AddUpdate();

                    try
                    {
                        PolicyToLearnPost.AddUpdatPolicyToLearn(_policy, deuFields);
                        LearnedToPolicyPost.AddUpdateLearnedToPolicy(_policy.PolicyId);
                        DEULearnedPost.AddDataDeuToLearnedPost(deuFields.DeuData);
                        Policy.AddUpdatePolicyHistory(_policy, DataModel);
                        PolicyLearnedField.AddUpdateHistoryLearned(_policy.PolicyId);
                    }
                    catch (Exception ex)
                    {
                        ActionLogger.Logger.WriteLog("CreateNewPendingPolicy1 ex.InnerException :" + ex.Message, true);
                    }
                }
            }
            catch (Exception ex)
            {
                ActionLogger.Logger.WriteLog("CreateNewPendingPolicy ex.InnerException:" + ex.Message, true);
            }
            return NewPendingPolicyID;
        }

        private static Guid CreateNewClient(DEU dEU, Guid licenceId)
        {
            Guid ClientId;

            if (dEU.ClientID != null && dEU.ClientID != Guid.Empty)
            {
                ClientId = dEU.ClientID.Value;
            }
            else
            {
                Client _client = new Client();
                ClientId = Guid.NewGuid();
                _client.ClientId = ClientId;
                _client.Address = "";
                _client.Zip = "";
                _client.State = "";
                _client.Name = String.IsNullOrEmpty(dEU.ClientName) == true ? dEU.PolicyNumber : dEU.ClientName;
                _client.IsDeleted = false;
                _client.City = "";
                _client.Email = "";
                _client.LicenseeId = licenceId;
                _client.AddUpdate();
            }

            return ClientId;
            
        }

        private static DLinq.Client CreateNewClientForPendingPolicy(DEU Deu, DLinq.CommissionDepartmentEntities DataModel, Guid licenceId)
        {
            ActionLogger.Logger.WriteLog(DateTime.Now.ToString() + "  CreateNewClientForPendingPolicy started, Deu.ClientID - " + Deu.ClientID, true);
            Guid ClientId = Guid.Empty;
            if (Deu.ClientID != null && Deu.ClientID != Guid.Empty)
            {
                //ClientId = Deu.ClientID.Value;
                ClientId = (Guid)Deu.ClientID;
                //Mar 10 2017 Acme - fix for an issue in deu entries editing where clientID set null, afeter it got removed from DB on deletion
                if ((from f in DataModel.Clients where f.ClientId == ClientId select f).FirstOrDefault() == null)
                {
                    try
                    {
                        Client _client = new Client();
                       // ClientId = Guid.NewGuid();
                        _client.ClientId = ClientId;
                        _client.Address = "";
                        _client.Zip = "";
                        _client.State = "";
                        _client.Name = String.IsNullOrEmpty(Deu.ClientName) == true ? Deu.PolicyNumber : Deu.ClientName;
                        _client.IsDeleted = false;
                        _client.City = "";
                        _client.Email = "";
                        _client.LicenseeId = licenceId;
                        _client.AddUpdate();
                    }
                    catch (Exception ex)
                    {
                        ActionLogger.Logger.WriteLog("CreateNewClientForPendingPolicy:" + ex.Message.ToString(), true);
                    }
                }
            }
            else
            {
                try
                {
                    Client _client = new Client();
                    ClientId = Guid.NewGuid();
                    _client.ClientId = ClientId;
                    _client.Address = "";
                    _client.Zip = "";
                    _client.State = "";
                    _client.Name = String.IsNullOrEmpty(Deu.ClientName) == true ? Deu.PolicyNumber : Deu.ClientName;
                    _client.IsDeleted = false;
                    _client.City = "";
                    _client.Email = "";
                    _client.LicenseeId = licenceId;
                    _client.AddUpdate();
                }
                catch (Exception ex)
                {
                    ActionLogger.Logger.WriteLog("CreateNewClientForPendingPolicy:" + ex.Message.ToString(), true);
                }
            }
            return (from f in DataModel.Clients where f.ClientId == ClientId select f).FirstOrDefault();
        }

        private static DLinq.Client CreateNewClientForUnlikingPolicy(DEU Deu, DLinq.CommissionDepartmentEntities DataModel, Guid licenceId, string strUnlinkClient)
        {
            Guid ClientId = Guid.NewGuid();
            try
            {

                Client _client = new Client();              
                _client.ClientId = ClientId;
                _client.Address = "";
                _client.Zip = "";
                _client.State = "";
                _client.Name = String.IsNullOrEmpty(strUnlinkClient) == true ? Deu.PolicyNumber : strUnlinkClient;
                _client.IsDeleted = false;
                _client.City = "";
                _client.Email = "";
                _client.LicenseeId = licenceId;
                _client.AddUpdate();
            }
            catch (Exception ex)
            {
                ActionLogger.Logger.WriteLog("CreateNewClientForPendingPolicy:" + ex.Message.ToString(), true);
            }

            return (from f in DataModel.Clients where f.ClientId == ClientId select f).FirstOrDefault();
        }

        public static PolicyDetailsData GetFollowupPolicy(Guid SePolicyID)
        {
            PolicyDetailsData objPolicyDetailsData = new PolicyDetailsData();
            try
            {
                DLinq.CommissionDepartmentEntities ctx = new DLinq.CommissionDepartmentEntities(); //create your entity object here
                EntityConnection ec = (EntityConnection)ctx.Connection;
                SqlConnection sc = (SqlConnection)ec.StoreConnection; //get the SQLConnection that your entity object would use
                string adoConnStr = sc.ConnectionString;

                DateTime? nullDateTime = null;
                Decimal? dbDc = null;

                using (SqlConnection con = new SqlConnection(adoConnStr))
                {
                    using (SqlCommand cmd = new SqlCommand("Usp_GetFollowupProcPolicy", con))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@PolicyId", SePolicyID);
                        con.Open();

                        SqlDataReader reader = cmd.ExecuteReader();
                        // Call Read before accessing data. 
                        while (reader.Read())
                        {
                            try
                            {
                                try
                                {
                                    if (!string.IsNullOrEmpty(Convert.ToString(reader["PolicyId"])))
                                    {
                                        objPolicyDetailsData.PolicyId = reader["PolicyId"] == null ? Guid.Empty : (Guid)reader["PolicyId"];
                                    }
                                }
                                catch
                                {
                                }

                                try
                                {
                                    if (!string.IsNullOrEmpty(Convert.ToString(reader["PolicyNumber"])))
                                    {
                                        objPolicyDetailsData.PolicyNumber = reader["PolicyNumber"] == null ? string.Empty : Convert.ToString(reader["PolicyNumber"]);
                                    }
                                }
                                catch
                                {
                                }

                                try
                                {
                                    if (!string.IsNullOrEmpty(Convert.ToString(reader["OriginalEffectiveDate"])))
                                    {
                                        objPolicyDetailsData.OriginalEffectiveDate = reader["OriginalEffectiveDate"] == null ? nullDateTime : Convert.ToDateTime(reader["OriginalEffectiveDate"]);
                                    }
                                }
                                catch
                                {
                                }

                                try
                                {
                                    if (!string.IsNullOrEmpty(Convert.ToString(reader["ModeAvgPremium"])))
                                    {
                                        objPolicyDetailsData.ModeAvgPremium = reader["ModeAvgPremium"] == null ? dbDc : Convert.ToDecimal(reader["ModeAvgPremium"]);
                                    }
                                }
                                catch
                                {
                                }

                                try
                                {
                                    if (!string.IsNullOrEmpty(Convert.ToString(reader["Advance"])))
                                    {
                                        objPolicyDetailsData.Advance = reader["Advance"] == null ? 0 : Convert.ToInt32(reader["Advance"]);
                                    }
                                }
                                catch
                                {
                                }

                                try
                                {
                                    if (!string.IsNullOrEmpty(Convert.ToString(reader["PolicyTerminationDate"])))
                                    {
                                        objPolicyDetailsData.PolicyTerminationDate = reader["PolicyTerminationDate"] == null ? nullDateTime : Convert.ToDateTime(reader["PolicyTerminationDate"]);
                                    }
                                }
                                catch
                                {
                                }

                                try
                                {
                                    if (!string.IsNullOrEmpty(Convert.ToString(reader["IsTrackMissingMonth"])))
                                    {
                                        if (reader["IsTrackMissingMonth"] != null)
                                        {
                                            objPolicyDetailsData.IsTrackMissingMonth = Convert.ToBoolean(reader["IsTrackMissingMonth"]);
                                        }
                                    }

                                }
                                catch
                                {
                                }

                                try
                                {
                                    if (!string.IsNullOrEmpty(Convert.ToString(reader["IsTrackPayment"])))
                                    {
                                        if (reader["IsTrackPayment"] != null)
                                        {
                                            objPolicyDetailsData.IsTrackPayment = Convert.ToBoolean(reader["IsTrackPayment"]);
                                        }
                                    }

                                }
                                catch
                                {
                                }

                            }
                            catch
                            {
                            }

                        }

                        // Call Close when done reading.
                        reader.Close();
                    }
                }
            }
            catch
            {
            }

            return objPolicyDetailsData;
        }

        public static PolicyDetailsData GetPolicyEffectiveDate(Guid SePolicyID)
        {
            PolicyDetailsData objPolicyDetailsData = new PolicyDetailsData();
            DLinq.CommissionDepartmentEntities ctx = new DLinq.CommissionDepartmentEntities(); //create your entity object here
            EntityConnection ec = (EntityConnection)ctx.Connection;
            SqlConnection sc = (SqlConnection)ec.StoreConnection; //get the SQLConnection that your entity object would use
            string adoConnStr = sc.ConnectionString;

            using (SqlConnection con = new SqlConnection(adoConnStr))
            {
                using (SqlCommand cmd = new SqlCommand("Usp_GetFollowupProcPolicy", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@PolicyId", SePolicyID);
                    con.Open();

                    SqlDataReader reader = cmd.ExecuteReader();
                    // Call Read before accessing data. 
                    while (reader.Read())
                    {

                        try
                        {
                            if (!string.IsNullOrEmpty(Convert.ToString(reader["OriginalEffectiveDate"])))
                            {

                                objPolicyDetailsData.OriginalEffectiveDate = Convert.ToDateTime(reader["OriginalEffectiveDate"]);
                            }
                        }
                        catch
                        {
                        }

                    }

                }
            }
            return objPolicyDetailsData;
        }

        public static PolicyDetailsData GetPolicy(Guid SePolicyID)
        {
            Dictionary<string, object> parameters = new Dictionary<string, object>();
            if (!parameters.ContainsKey("PolicyId"))
                parameters.Add("PolicyId", SePolicyID);
            return Policy.GetPolicyData(parameters).FirstOrDefault();
        }


        static decimal GetTotalAsPerMode(int PolicyModeId, decimal total)
        {
            ActionLogger.Logger.WriteLog("GetTotalAsPerMode, PolicyModeId: " + PolicyModeId + ", total: " + total, true);
            decimal finalTotal = total;
            try
            {
                switch (PolicyModeId)
                {
                    case 1:
                        finalTotal = total / 3;
                        break;
                    case 2:
                        finalTotal = total / 6;
                        break;
                    case 3:
                        finalTotal = total / 12;
                        break;
                    default:
                        break;
                }

                ActionLogger.Logger.WriteLog("GetTotalAsPerMode, total as per mode: " + finalTotal, true);
            }
            catch(Exception ex)
            {
                ActionLogger.Logger.WriteLog("GetTotalAsPerMode, ex: " + ex.Message, true);
            }
            return finalTotal;
        }

        
        /// <summary>
        ///  Sep 03, 2019 - Logic added to handle advance payment and calculate total = total/mode
        ///  Because comparison on monthly basis with PMC
        ///  Then, if advance recieved and variance checked for first payment, total = total/advance to compare to PMC 
        /// </summary>
        /// <param name="lstPaymentEntriesPost"></param>
        /// <param name="strPMC"></param>
        /// <param name="StartDate"></param>
        /// <param name="EndDate"></param>
        /// <returns></returns>
        public static bool CheckForPMCVariance(List<PolicyPaymentEntriesPost> lstPaymentEntriesPost, string strPMC, DateTime? StartDate, DateTime? EndDate, DateTime? EffectiveDate, int? Advance, int? PolicyModeId)
        {
            ActionLogger.Logger.WriteLog("CheckForPMCVariance request: PMC - " + strPMC + ", Start: " + StartDate + ", EndDate: " + EndDate + ", EffDate: " + EffectiveDate + ", Advance: " + Advance + ", Mode: " + PolicyModeId, true);
            bool Flag = false;
            try
            {
                if (string.IsNullOrEmpty(strPMC)) return false;

                //strPMC = (!string.IsNullOrEmpty(strPMC)) ? strPMC.Split('$')[1] : "";
                strPMC.Replace("$", "");

                decimal PMC = 0;
                decimal.TryParse(strPMC, out PMC);
                List<PolicyPaymentEntriesPost> lstValid = lstPaymentEntriesPost.Where(x => x.InvoiceDate >= StartDate && x.InvoiceDate <= EndDate).ToList();
                decimal total = lstValid.Sum(x => x.TotalPayment); //Fix done to remove payment received and add total payment 
                ActionLogger.Logger.WriteLog("CheckForPMCVariance, total without mode: " + total, true);

                

                if(Advance > 0)
                {
                    ActionLogger.Logger.WriteLog("CheckForPMCVariance, advance > 0 ", true);
                    if(EffectiveDate != null && EffectiveDate >= StartDate && EffectiveDate <= EndDate)
                    {
                        total = total / (decimal) Advance;
                        ActionLogger.Logger.WriteLog("CheckForPMCVariance, EffDate falling in range, total: " + total, true);
                    }
                    else
                    {
                        total = (PolicyModeId > 0) ? GetTotalAsPerMode((int)PolicyModeId, total) : total;
                        ActionLogger.Logger.WriteLog("CheckForPMCVariance, EffDate NOT falling in range, total: " + total, true);
                    }
                }
                else
                {
                    total = (PolicyModeId > 0) ? GetTotalAsPerMode((int)PolicyModeId, total) : total;
                    ActionLogger.Logger.WriteLog("CheckForPMCVariance, Advance = 0 or null", true);
                }
                

                string result = "";
                using (SqlConnection con = new SqlConnection(DBConnection.GetConnectionString()))//("Data Source=localhost;Initial Catalog=CommisionDepartmentEricDB;Integrated Security=True;MultipleActiveResultSets=True"))
                {
                    con.Open();
                    SqlCommand scm = new SqlCommand();
                    scm.CommandText = "usp_GetPMCVariance";
                    scm.Connection = con;
                    scm.CommandType = CommandType.StoredProcedure;
                    scm.Parameters.AddWithValue("@total", total);
                    scm.Parameters.AddWithValue("@PMC", PMC);

                    result = scm.ExecuteScalar() as string;
                }
                Flag = (result == "1"); // true when 1
            }
            catch (Exception ex)
            {
                ActionLogger.Logger.WriteLog("CheckForPMCVariance exception: " + ex.Message,true);
            }
            return Flag;
            //usp_GetPMCVariance
        }

        //public static void HandlePMCVariance(List<PolicyPaymentEntriesPost> _PolicyPaymentEntriesFormissing, string PMC, DateTime FirstDate, DateTime LastDate, Guid PolicyID)
        //{

        //    bool flagvarience = CheckForPMCVariance(_PolicyPaymentEntriesFormissing, PMC, FirstDate, LastDate);
        //    List<DisplayFollowupIssue> FollowupIssueLst = FollowupIssue.GetIssues(PolicyID);
        //    //Update/Close issue against all payment entries 
        //    foreach (PolicyPaymentEntriesPost ppe in _PolicyPaymentEntriesFormissing)
        //    {
        //        if (flagvarience)
        //        {
        //            FollowUpUtill.RegisterIssueAgainstPMCVariance(PolicyID, FirstDate, LastDate, ppe);
        //        }
        //        else
        //        {
        //            DisplayFollowupIssue FollowupIssuetemp = FollowupIssueLst.Where(p => (p.PolicyId == PolicyID) && (p.InvoiceDate == ppe.InvoiceDate)).Where(p => p.IssueCategoryID == 3).FirstOrDefault();
        //            if (FollowupIssuetemp != null)
        //            {
        //                if (FollowupIssuetemp.IssueStatusId != (int)FollowUpIssueStatus.Closed)
        //                {
        //                    FollowupIssuetemp.IssueStatusId = (int)FollowUpIssueStatus.Closed;
        //                    FollowupIssuetemp.IssueResultId = (int)FollowUpResult.Resolved_CD;
        //                    FollowupIssue.AddUpdate(FollowupIssuetemp);
        //                }
        //            }
        //        }
        //    }
        //    // return flagvarience;
        //}


        /// <summary>
        /// Check for varience in Incoimng Schedule
        /// </summary>
        /// <param name="PolicyId"></param>
        /// <param name="InvoiceDate"></param>
        /// <param name="EntryId"></param>
        /// <param name="_TempPolicyPaymentEntriesPost"></param>
        /// <returns></returns>
        /// 
        public static bool CheckForIncomingScheduleVariance(PolicyPaymentEntriesPost _TempPolicyPaymentEntriesPost, decimal? Premium)
        {
            double TruncationError = Convert.ToDouble(Masters.SystemConstant.GetKeyValue("TruncationError"));

            PolicyPaymentEntriesPost.Expectedpayment = 0;
            bool Flag = false;

            try
            {
                PolicySchedule _PolicySchedule = CheckForIncomingTypeOfSchedule(_TempPolicyPaymentEntriesPost.PolicyID);
                PolicyDetailsData _Policy = GetPolicy(_TempPolicyPaymentEntriesPost.PolicyID);

                double SplitPer = _Policy.SplitPercentage ?? 100;
                PolicyPaymentEntriesPost _PolicyPaymentEntriesPost = _TempPolicyPaymentEntriesPost;

                // Get the Premium on the Policy to calculate the variance
                //decimal Premium = _PolicyPaymentEntriesPost.PaymentRecived;
                FirstYrRenewalYr IsFirstYr = FirstYrRenewalYr.None;

                if (_PolicySchedule == PolicySchedule.None)
                {
                    Flag = false;
                }
                //check schedule is basic or advances
                else if (_PolicySchedule == PolicySchedule.Basic)
                {
                    //get effective date from smart field
                    PolicyLearnedFieldData _PolicyLearnedField = PolicyLearnedField.GetPolicyLearnedFieldsPolicyWise(_TempPolicyPaymentEntriesPost.PolicyID);
                    //if effectve date of policy is null then issue will resolved ,no variance is genrated
                    if (_PolicyLearnedField.Effective == null)
                        return Flag = false;

                    //IsFirstYr = IsUseFirstYear(_PolicyPaymentEntriesPost.InvoiceDate, _Policy.OriginalEffectiveDate, _PolicyPaymentEntriesPost.PolicyID);
                    IsFirstYr = IsUseFirstYear(_PolicyPaymentEntriesPost.InvoiceDate, _PolicyLearnedField.Effective, _PolicyPaymentEntriesPost.PolicyID);
                    if (IsFirstYr == FirstYrRenewalYr.None)
                    {
                        Flag = true;
                        return Flag;
                    }
                    //old
                    //PolicyToolIncommingShedule _Schedule;
                    //_Schedule = GetBasicIncomingScheduleOfPolicy(_PolicyPaymentEntriesPost.PolicyID);
                    //PolicyToolIncommingShedule _incomingSchedule = _Schedule;
                    //new
                    PolicyToolIncommingShedule _incomingSchedule = PolicyToolIncommingShedule.GettingPolicyIncomingSchedule(_PolicyPaymentEntriesPost.PolicyID); //GetBasicIncomingScheduleOfPolicy(_PolicyPaymentEntriesPost.PolicyID);


                    if (_incomingSchedule == null || _incomingSchedule.IncomingScheduleID == Guid.Empty)
                    {
                        ActionLogger.Logger.WriteFollowUpLog("Incoming schedule is null, returning");
                        return Flag;//14-mar-2011 
                    }

                    if (_incomingSchedule.Mode == Mode.Custom) //Handle custom case 
                    {
                        if (_incomingSchedule.CustomType == CustomMode.Graded)
                        {
                            ActionLogger.Logger.WriteFollowUpLog("Incoming schedule is Custom Graded");
                            //Check as per payment date, what was the total from year beginnin
                            int year = Convert.ToDateTime(_PolicyPaymentEntriesPost.InvoiceDate).Year;
                            int month = Convert.ToDateTime(_PolicyPaymentEntriesPost.InvoiceDate).Month;

                            int effectiveMonth = Convert.ToDateTime(_PolicyLearnedField.Effective).Month;

                            if(month < effectiveMonth)
                            {
                                year = year - 1;
                            }

                            DateTime startDate = new DateTime(year, effectiveMonth, 1);
                            ActionLogger.Logger.WriteFollowUpLog("startDate found as : " + startDate);
                            DateTime endDate = Convert.ToDateTime(_PolicyPaymentEntriesPost.InvoiceDate);
                            ActionLogger.Logger.WriteFollowUpLog("endDate found as : " + endDate);

                            //Get sum of all payments in this range
                            double sum = PolicyPaymentEntriesPost.GetAllPaymentEntriesSumInRange(startDate, endDate, _PolicyPaymentEntriesPost.PolicyID);
                            ActionLogger.Logger.WriteFollowUpLog("endDate found as : " + endDate);

                            //Get expected% from graded table 
                            //double? expectedPercent = PayorIncomingSchedule.GetGradedPercentOnRange(sum);
                            if (_incomingSchedule.GradedSchedule != null && _incomingSchedule.GradedSchedule.Count > 0)
                            {
                                //check if sum exceeds max value , then use max value 
                                double maxVal = _incomingSchedule.GradedSchedule.OrderByDescending(x => x.From).FirstOrDefault().To;

                                var record = _incomingSchedule.GradedSchedule.Where(x => x.From <= sum && x.To >= sum).FirstOrDefault();
                                if (sum > maxVal)
                                {
                                    ActionLogger.Logger.WriteFollowUpLog("Sum is greated than maxValue, so using max Value ");
                                    record = _incomingSchedule.GradedSchedule.Where(x => x.To == maxVal).FirstOrDefault();
                                }
                                

                                if (record != null)
                                {
                                    double? expectedPercent = record.Percent;
                                    ActionLogger.Logger.WriteFollowUpLog("percent found for given sum: " + expectedPercent);
                                    if (expectedPercent != null)
                                    {
                                        try
                                        {
                                            double finalres = (Convert.ToDouble(_PolicyPaymentEntriesPost.CommissionPercentage) - expectedPercent).Value;

                                            if (_incomingSchedule.ScheduleTypeId == (int)MasterBasicIncomingSchedule.PerHead)
                                            {
                                                finalres = (Convert.ToDouble(_PolicyPaymentEntriesPost.DollerPerUnit) - expectedPercent).Value;
                                            }
                                            if ((Math.Abs(finalres) >= TruncationError) && expectedPercent != 0)
                                            {
                                                Flag = true;
                                            }
                                            ActionLogger.Logger.WriteFollowUpLog("finalres : " + finalres);
                                        }
                                        catch(Exception ex)
                                        {
                                            ActionLogger.Logger.WriteFollowUpLog("finalres exception: " + ex.Message);
                                        }
                                        ActionLogger.Logger.WriteFollowUpLog("Flag : " + Flag);
                                    }
                                }
                                else
                                {
                                    ActionLogger.Logger.WriteFollowUpLog("Graded schedule not found for given sum: " + sum);
                                }
                            }
                            else
                            {
                                ActionLogger.Logger.WriteFollowUpLog("Graded schedule list found null ");
                            }

                        }
                        else //Non-graded
                        {
                            ActionLogger.Logger.WriteFollowUpLog("Incoming schedule is Custom Non - Graded");
                            //Check which year policy is in
                            int year = 1;
                            try
                            {
                                if (_PolicyLearnedField.Effective != null) //if null, always assumed first year.
                                {
                                    year = (int)Math.Ceiling((Convert.ToDateTime(_PolicyPaymentEntriesPost.InvoiceDate) - Convert.ToDateTime(_PolicyLearnedField.Effective)).TotalDays / 365.25D);
                                    year = (year <= 0) ? 1 : year;
                                }
                            }
                            catch (Exception ex)
                            {
                                ActionLogger.Logger.WriteFollowUpLog("Exception in getting polic age: " + ex.Message);
                            }
                            ActionLogger.Logger.WriteFollowUpLog("policy year found as : " + year);
                            //Check if year present in schedule
                            if (_incomingSchedule.NonGradedSchedule != null)
                            {
                                //double Firstyrtotalsum = _incomingSchedule.NonGradedSchedule.Where(x => x.Year == 1).FirstOrDefault().Percent;
                                int maxYear = year;
                                var recYear = _incomingSchedule.NonGradedSchedule.OrderByDescending(x => x.Year).FirstOrDefault();
                                if(recYear != null)
                                     maxYear = recYear.Year;

                                year = (year > maxYear) ? maxYear : year;

                                ActionLogger.Logger.WriteFollowUpLog("year found from schedule as : " + year);

                                double? expectedPercent = 0;
                                var rec = _incomingSchedule.NonGradedSchedule.Where(x => x.Year == year).FirstOrDefault();
                                if(rec != null){
                                    expectedPercent = rec.Percent;
                                }
                                ActionLogger.Logger.WriteFollowUpLog("Percent found for year as : " + expectedPercent);

                                if (expectedPercent != null && expectedPercent != 0)
                                {
                                    try
                                    {
                                        double finalres = (Convert.ToDouble(_PolicyPaymentEntriesPost.CommissionPercentage) - expectedPercent).Value;

                                        if (_incomingSchedule.ScheduleTypeId == (int)MasterBasicIncomingSchedule.PerHead)
                                        {
                                            finalres = (Convert.ToDouble(_PolicyPaymentEntriesPost.DollerPerUnit) - expectedPercent).Value;
                                        }
                                        ActionLogger.Logger.WriteFollowUpLog("finalres : " + finalres);

                                        if ((Math.Abs(finalres) >= TruncationError) && expectedPercent != 0)
                                        {
                                            Flag = true;
                                        }
                                    }
                                    catch(Exception ex)
                                    {
                                        ActionLogger.Logger.WriteFollowUpLog("Exception in finalRes calculation : " + ex.Message);
                                    }
                                }
                                ActionLogger.Logger.WriteFollowUpLog("Flag : " + Flag);
                            }
                            else
                            {
                                ActionLogger.Logger.WriteFollowUpLog("POlicy non graded schedule is null: ");
                            }
                        }
                    }
                    else
                    {
                        ActionLogger.Logger.WriteFollowUpLog("Incoming schedule is Default");

                        if (_incomingSchedule.ScheduleTypeId == (int)MasterBasicIncomingSchedule.PercentageOfPremium)
                        {
                            if (IsFirstYr == FirstYrRenewalYr.FirstYear)
                            {
                                //old
                                //double? expectedIncoming = Convert.ToDouble(Premium) * _incomingSchedule.FirstYearPercentage * SplitPer / 10000;

                                double? expectedIncoming = _incomingSchedule.FirstYearPercentage;

                                if (expectedIncoming != null)
                                {
                                    PolicyPaymentEntriesPost.Expectedpayment = decimal.Parse(expectedIncoming.Value.ToString());
                                }
                                else
                                {
                                    expectedIncoming = 0;
                                }

                                //double finalres = (Convert.ToDouble(_PolicyPaymentEntriesPost.TotalPayment) - expectedIncoming).Value;
                                //new 
                                double finalres = (Convert.ToDouble(_PolicyPaymentEntriesPost.CommissionPercentage) - expectedIncoming).Value;

                                // generate Variance if the calculated value is greater than the tolerance value
                                if ((Math.Abs(finalres) >= TruncationError) && expectedIncoming != 0)
                                {
                                    //get data when variance is generated Means get all payment at that invoice ,and get variance
                                    // don't delete this code ,only write here to remove to calling from database every time
                                    //decimal dcValue = PolicyPaymentEntriesPost.GetTotalpaymentOnInvoiceDate(_TempPolicyPaymentEntriesPost.PolicyID, Convert.ToDateTime(_TempPolicyPaymentEntriesPost.InvoiceDate));
                                    //finalres = (Convert.ToDouble(dcValue) - expectedIncoming).Value;
                                    // generate Variance if the calculated value is greater than the tolerance value
                                    //if ((Math.Abs(finalres) >= TruncationError) && expectedIncoming != 0)
                                    //{
                                    Flag = true;
                                    //}
                                }
                            }
                            else //calculation of renewal percent
                            {
                                //double? expectedIncoming = Convert.ToDouble(Premium) * _incomingSchedule.RenewalPercentage * SplitPer / 10000;

                                double? expectedIncoming = _incomingSchedule.RenewalPercentage;
                                if (expectedIncoming != null)
                                {
                                    PolicyPaymentEntriesPost.Expectedpayment = decimal.Parse(expectedIncoming.Value.ToString());
                                }
                                else
                                {
                                    expectedIncoming = 0;
                                }
                                //double finalres = (Convert.ToDouble(_PolicyPaymentEntriesPost.TotalPayment) - expectedIncoming).Value;

                                double finalres = (Convert.ToDouble(_PolicyPaymentEntriesPost.CommissionPercentage) - expectedIncoming).Value;

                                if ((Math.Abs(finalres) >= TruncationError && expectedIncoming != 0))
                                {
                                    ////get data when variance is generated Means get all payment at that invoice ,and get variance
                                    //  // don't delete this code ,only write here to remove to calling from database every time
                                    //  decimal dcValue = PolicyPaymentEntriesPost.GetTotalpaymentOnInvoiceDate(_TempPolicyPaymentEntriesPost.PolicyID, Convert.ToDateTime(_TempPolicyPaymentEntriesPost.InvoiceDate));
                                    //  finalres = (Convert.ToDouble(dcValue) - expectedIncoming).Value;
                                    //   // generate Variance if the calculated value is greater than the tolerance value
                                    //  if ((Math.Abs(finalres) >= TruncationError) && expectedIncoming != 0)
                                    //  {
                                    Flag = true;
                                    //}
                                }
                            }
                        }
                        else if (_incomingSchedule.ScheduleTypeId == (int)MasterBasicIncomingSchedule.PerHead)
                        {
                            if (IsFirstYr == FirstYrRenewalYr.FirstYear)
                            {
                                //double? expectedIncoming = _PolicyPaymentEntriesPost.NumberOfUnits * _incomingSchedule.FirstYearPercentage * SplitPer / 100;

                                // comparasion with dolloer per unit
                                double? expectedFee = _incomingSchedule.FirstYearPercentage;

                                if (expectedFee != null)
                                {
                                    PolicyPaymentEntriesPost.Expectedpayment = decimal.Parse(expectedFee.Value.ToString());
                                }
                                else
                                {
                                    expectedFee = 0;
                                }
                                //double finalres = (Convert.ToDouble(_PolicyPaymentEntriesPost.TotalPayment) - expectedFee).Value;

                                double finalres = (Convert.ToDouble(_PolicyPaymentEntriesPost.DollerPerUnit) - expectedFee).Value;

                                if ((Math.Abs(finalres) >= TruncationError) && (expectedFee != 0))
                                {
                                    ////get data when variance is generated Means get all payment at that invoice ,and get variance
                                    //// don't delete this code ,only write here to remove to calling from database every time
                                    //decimal dcValue = PolicyPaymentEntriesPost.GetTotalpaymentOnInvoiceDate(_TempPolicyPaymentEntriesPost.PolicyID, Convert.ToDateTime(_TempPolicyPaymentEntriesPost.InvoiceDate));
                                    //finalres = (Convert.ToDouble(dcValue) - expectedFee).Value;
                                    // generate Variance if the calculated value is greater than the tolerance value
                                    //if ((Math.Abs(finalres) >= TruncationError) && expectedFee != 0)
                                    //{
                                    Flag = true;
                                    //}
                                }
                            }
                            else
                            {
                                //double? expectedIncoming = _PolicyPaymentEntriesPost.NumberOfUnits * _incomingSchedule.RenewalPercentage * SplitPer / 100;
                                //expected fee for renewal percentage
                                double? expectedFee = _incomingSchedule.RenewalPercentage;
                                if (expectedFee != null)
                                {
                                    PolicyPaymentEntriesPost.Expectedpayment = decimal.Parse(expectedFee.Value.ToString());
                                }
                                else
                                {
                                    expectedFee = 0;
                                }
                                //Un Comment on "26 august 2013"
                                //double finalres = (Convert.ToDouble(_PolicyPaymentEntriesPost.TotalPayment) - expectedFee).Value;

                                //comparasion with dolloer per unit
                                //Comment on "26 august 2013"
                                double finalres = (Convert.ToDouble(_PolicyPaymentEntriesPost.DollerPerUnit) - expectedFee).Value;

                                if ((Math.Abs(finalres) >= TruncationError) && (expectedFee != 0))
                                {
                                    ////get data when variance is generated Means get all payment at that invoice ,and get variance
                                    //// don't delete this code ,only write here to remove to calling from database every time
                                    //decimal dcValue = PolicyPaymentEntriesPost.GetTotalpaymentOnInvoiceDate(_TempPolicyPaymentEntriesPost.PolicyID, Convert.ToDateTime(_TempPolicyPaymentEntriesPost.InvoiceDate));
                                    //finalres = (Convert.ToDouble(dcValue) - expectedFee).Value;
                                    //// generate Variance if the calculated value is greater than the tolerance value
                                    //if ((Math.Abs(finalres) >= TruncationError) && expectedFee != 0)
                                    //{
                                    Flag = true;
                                    //}
                                }
                            }
                        }
                    }
                }
                else if (_PolicySchedule == PolicySchedule.Advance)
                {
                    decimal calAmount = 0;

                    PolicyIncomingSchedule _AdvanceSchedule = GetAdvanceIncomingScheduleOfPolicy(_PolicyPaymentEntriesPost.PolicyID);
                    if (_AdvanceSchedule == null || _AdvanceSchedule.IncomingScheduleList == null || _AdvanceSchedule.IncomingScheduleList.Count == 0) return Flag;
                    int AdvanceIncomingType = _AdvanceSchedule.ScheduleTypeId;
                    _AdvanceSchedule = FillNullDateWithMaxSystemDate(_AdvanceSchedule);

                    if (AdvanceIncomingType == (int)MasterAdvanceScheduleType.PercentageofPremium_scale)
                    {
                        double tempPremium = Convert.ToDouble(Premium);
                        double? incomingperimum = 0;
                        List<IncomingScheduleEntry> _InComingShedule = _AdvanceSchedule.IncomingScheduleList
                            .Where(p => p.EffectiveFromDate <= _PolicyPaymentEntriesPost.InvoiceDate)
                            .Where(p => p.EffectiveToDate >= _PolicyPaymentEntriesPost.InvoiceDate)
                            .Where(p => p.FromRange <= double.Parse(_PolicyPaymentEntriesPost.PaymentRecived.ToString())).OrderBy(p => p.FromRange).ToList();
                        foreach (IncomingScheduleEntry intg in _InComingShedule)
                        {
                            if (tempPremium > ((intg.ToRange - intg.FromRange) + GetValidFromForcalCulation(intg.FromRange)))
                            {
                                incomingperimum += Convert.ToDouble((intg.ToRange - intg.FromRange) + GetValidFromForcalCulation(intg.FromRange)) * intg.Rate * SplitPer / 10000;
                                tempPremium = tempPremium - Convert.ToDouble(intg.ToRange);
                            }
                            else
                            {
                                incomingperimum += tempPremium * intg.Rate * SplitPer / 10000;
                                tempPremium = 0;
                                break;
                            }
                        }

                        double? expectedIncoming = Convert.ToDouble(_PolicyPaymentEntriesPost.TotalPayment);
                        if (expectedIncoming != null)
                        {
                            PolicyPaymentEntriesPost.Expectedpayment = decimal.Parse(expectedIncoming.Value.ToString());
                        }
                        double finalres = (incomingperimum - expectedIncoming).Value;
                        if ((Math.Abs(finalres) <= TruncationError) || (expectedIncoming == 0))
                        {
                            Flag = true;
                        }
                    }

                    else if (AdvanceIncomingType == (int)MasterAdvanceScheduleType.PercentageofPremium_target)
                    {
                        double? perimumofper = 0;
                        List<IncomingScheduleEntry> _InComingShedule = _AdvanceSchedule.IncomingScheduleList
                            .Where(p => p.EffectiveFromDate <= _PolicyPaymentEntriesPost.InvoiceDate)
                            .Where(p => p.EffectiveToDate >= _PolicyPaymentEntriesPost.InvoiceDate)
                            .Where(p => p.FromRange <= double.Parse(_PolicyPaymentEntriesPost.PaymentRecived.ToString()))
                            .Where(p => p.ToRange >= double.Parse(_PolicyPaymentEntriesPost.PaymentRecived.ToString())).ToList();
                        foreach (IncomingScheduleEntry intg in _InComingShedule)
                        {
                            perimumofper += Convert.ToDouble(Premium) * intg.Rate * SplitPer / 10000;
                        }
                        double? expectedIncoming = Convert.ToDouble(_PolicyPaymentEntriesPost.TotalPayment);
                        if (expectedIncoming != null)
                        {
                            PolicyPaymentEntriesPost.Expectedpayment = decimal.Parse(expectedIncoming.Value.ToString());
                        }
                        double finalres = (perimumofper - expectedIncoming).Value;

                        if ((Math.Abs(finalres) >= TruncationError) || (expectedIncoming == 0))
                        {
                            Flag = true;
                        }
                    }

                    else if (AdvanceIncomingType == (int)MasterAdvanceScheduleType.PerHeadFee_scale)
                    {
                        List<IncomingScheduleEntry> _InComingShedule = _AdvanceSchedule.IncomingScheduleList
                            .Where(p => p.EffectiveFromDate <= _PolicyPaymentEntriesPost.InvoiceDate)
                            .Where(p => p.EffectiveToDate >= _PolicyPaymentEntriesPost.InvoiceDate)
                            .Where(p => p.FromRange <= Convert.ToDouble(_PolicyPaymentEntriesPost.NumberOfUnits)).ToList();
                        int TempHead = _PolicyPaymentEntriesPost.NumberOfUnits;

                        foreach (IncomingScheduleEntry intg in _InComingShedule)
                        {
                            if (TempHead > (intg.ToRange - (intg.FromRange + GetValidFromForcalCulation(intg.FromRange))))
                            {
                                calAmount += Convert.ToDecimal(intg.ToRange - (intg.FromRange + GetValidFromForcalCulation(intg.FromRange))) * Convert.ToDecimal(intg.Rate) * Convert.ToDecimal(SplitPer) / 100;
                                TempHead = TempHead - Convert.ToInt32(intg.ToRange);
                            }
                            else
                            {
                                calAmount += TempHead * Convert.ToDecimal(intg.Rate) * Convert.ToDecimal(SplitPer) / 100;
                                TempHead = 0;
                                break;
                            }

                        }

                        decimal? expectedIncoming = _PolicyPaymentEntriesPost.TotalPayment;
                        if (expectedIncoming != null)
                        {
                            PolicyPaymentEntriesPost.Expectedpayment = decimal.Parse(expectedIncoming.Value.ToString());
                        }
                        double finalres = Convert.ToDouble((calAmount - expectedIncoming));
                        if ((Math.Abs(finalres) <= TruncationError) || (expectedIncoming == 0))
                        {
                            Flag = true;
                        }
                    }
                    else if (AdvanceIncomingType == (int)MasterAdvanceScheduleType.PerHeadFee_target)
                    {
                        List<IncomingScheduleEntry> _InComingShedule = _AdvanceSchedule.IncomingScheduleList
                            .Where(p => p.EffectiveFromDate <= _PolicyPaymentEntriesPost.InvoiceDate)
                            .Where(p => p.EffectiveToDate >= _PolicyPaymentEntriesPost.InvoiceDate)
                            .Where(p => p.FromRange <= double.Parse(_PolicyPaymentEntriesPost.NumberOfUnits.ToString()))
                            .Where(p => p.ToRange >= double.Parse(_PolicyPaymentEntriesPost.NumberOfUnits.ToString())).ToList();
                        foreach (IncomingScheduleEntry outg in _InComingShedule)
                        {
                            calAmount += _PolicyPaymentEntriesPost.NumberOfUnits * Convert.ToDecimal(outg.Rate.ToString()) * Convert.ToDecimal(SplitPer) / 100;
                        }

                        decimal? expectedIncoming = _PolicyPaymentEntriesPost.TotalPayment;
                        if (expectedIncoming != null)
                        {
                            PolicyPaymentEntriesPost.Expectedpayment = decimal.Parse(expectedIncoming.Value.ToString());
                        }
                        double finalres = Convert.ToDouble((calAmount - expectedIncoming));
                        if ((Math.Abs(finalres) <= TruncationError) || (expectedIncoming == 0))
                        {
                            Flag = true;
                        }
                    }
                    else if (AdvanceIncomingType == (int)MasterAdvanceScheduleType.FlatDollar_modal)
                    {
                        List<IncomingScheduleEntry> _InComingShedule =
                            _AdvanceSchedule.IncomingScheduleList.Where(p => p.EffectiveFromDate <= _PolicyPaymentEntriesPost.InvoiceDate).
                            Where(p => p.EffectiveToDate >= _PolicyPaymentEntriesPost.InvoiceDate).ToList();
                        foreach (IncomingScheduleEntry outg in _InComingShedule)
                        {
                            calAmount += Convert.ToDecimal(outg.Rate) * Convert.ToDecimal(SplitPer) / 100;
                        }

                        decimal? expectedIncoming = _PolicyPaymentEntriesPost.TotalPayment;
                        if (expectedIncoming != null)
                        {
                            PolicyPaymentEntriesPost.Expectedpayment = decimal.Parse(expectedIncoming.Value.ToString());
                        }
                        double finalres = Convert.ToDouble((calAmount - expectedIncoming));
                        if ((Math.Abs(finalres) <= TruncationError) || (expectedIncoming == 0))
                        {
                            Flag = true;
                        }
                    }

                }
            }
            catch (Exception ex)
            {
                ActionLogger.Logger.WriteFollowUpLog("CheckForIncomingScheduleVariance :" + ex.StackTrace.ToString());
                ActionLogger.Logger.WriteFollowUpLog("CheckForIncomingScheduleVariance :" + ex.InnerException.ToString());
            }
            return Flag;
        }
        //public static bool CheckForIncomingScheduleVariance(PolicyPaymentEntriesPost _TempPolicyPaymentEntriesPost, decimal? Premium)
        //{
        //    double TruncationError = Convert.ToDouble(Masters.SystemConstant.GetKeyValue("TruncationError"));

        //    PolicyPaymentEntriesPost.Expectedpayment = 0;
        //    double dbPMC = 0;
        //    bool Flag = false;
        //    double finalres = 0;
        //    try
        //    {
        //        PolicySchedule _PolicySchedule = CheckForIncomingTypeOfSchedule(_TempPolicyPaymentEntriesPost.PolicyID);
        //        PolicyDetailsData _Policy = GetPolicy(_TempPolicyPaymentEntriesPost.PolicyID);

        //        double SplitPer = _Policy.SplitPercentage ?? 100;
        //        PolicyPaymentEntriesPost _PolicyPaymentEntriesPost = _TempPolicyPaymentEntriesPost;

        //        // Get the Premium on the Policy to calculate the variance
        //        //decimal Premium = _PolicyPaymentEntriesPost.PaymentRecived;
        //        FirstYrRenewalYr IsFirstYr = FirstYrRenewalYr.None;

        //        if (_PolicySchedule == PolicySchedule.None)
        //        {
        //            Flag = false;
        //        }
        //        //check schedule is basic or advances
        //        else if (_PolicySchedule == PolicySchedule.Basic)
        //        {
        //            //get effective date from smart field
        //            PolicyLearnedFieldData _PolicyLearnedField = PolicyLearnedField.GetPolicyLearnedFieldsPolicyWise(_TempPolicyPaymentEntriesPost.PolicyID);
        //            //if effectve date of policy is null then issue will resolved ,no variance is genrated
        //            if (_PolicyLearnedField.Effective == null)
        //                return Flag = false;

        //            //IsFirstYr = IsUseFirstYear(_PolicyPaymentEntriesPost.InvoiceDate, _Policy.OriginalEffectiveDate, _PolicyPaymentEntriesPost.PolicyID);
        //            IsFirstYr = IsUseFirstYear(_PolicyPaymentEntriesPost.InvoiceDate, _PolicyLearnedField.Effective, _PolicyPaymentEntriesPost.PolicyID);
        //            if (IsFirstYr == FirstYrRenewalYr.None)
        //            {
        //                Flag = true;
        //                return Flag;
        //            }
        //            //old
        //            //PolicyToolIncommingShedule _Schedule;
        //            //_Schedule = GetBasicIncomingScheduleOfPolicy(_PolicyPaymentEntriesPost.PolicyID);
        //            //PolicyToolIncommingShedule _incomingSchedule = _Schedule;
        //            //new
        //            PolicyToolIncommingShedule _incomingSchedule = GetBasicIncomingScheduleOfPolicy(_PolicyPaymentEntriesPost.PolicyID);

        //            if (_incomingSchedule == null || _incomingSchedule.IncomingScheduleId == Guid.Empty) return Flag;//14-mar-2011 

        //            if (_incomingSchedule.ScheduleTypeId == (int)MasterBasicIncomingSchedule.PercentageOfPremium)
        //            {
        //                //if (IsFirstYr == FirstYrRenewalYr.FirstYear)
        //                //{
        //                    //old
        //                    //double? expectedIncoming = Convert.ToDouble(Premium) * _incomingSchedule.FirstYearPercentage * SplitPer / 10000;
        //                    //vinod 14052015 added
        //                    if (_Policy != null)
        //                    {
        //                        dbPMC =Convert.ToDouble(CalculatePAC(_Policy.PolicyId));
        //                    }
        //                    //vinod 14052015 added
        //                    //Comment (15052015)
        //                    //double? expectedIncoming = _incomingSchedule.FirstYearPercentage;
        //                    double? expectedIncoming = Convert.ToDouble(_PolicyPaymentEntriesPost.TotalPayment);

        //                    //if (expectedIncoming != null)
        //                    //{
        //                    //    PolicyPaymentEntriesPost.Expectedpayment = decimal.Parse(expectedIncoming.Value.ToString());
        //                    //}

        //                    //double finalres = (Convert.ToDouble(_PolicyPaymentEntriesPost.TotalPayment) - expectedIncoming).Value;

        //                    //Comment (15052015)
        //                    //double finalres = (Convert.ToDouble(_PolicyPaymentEntriesPost.CommissionPercentage) - expectedIncoming).Value;

        //                    //new 
        //                    if (dbPMC > 0)
        //                    {
        //                        finalres = Convert.ToDouble(dbPMC - expectedIncoming);
        //                        finalres = finalres / Math.Abs(dbPMC);
        //                    }

        //                    // NO Variance if the calculated value is greater than the tolerance value
        //                    if (dbPMC <= 0)
        //                    {
        //                        return Flag = false;
        //                    }
        //                    // Variance if the calculated value is greater than the tolerance value
        //                    if (((Math.Abs(finalres) >= TruncationError) && expectedIncoming != 0))
        //                    {
        //                        //get data when variance is generated Means get all payment at that invoice ,and get variance
        //                        // don't delete this code ,only write here to remove to calling from database every time
        //                        //decimal dcValue = PolicyPaymentEntriesPost.GetTotalpaymentOnInvoiceDate(_TempPolicyPaymentEntriesPost.PolicyID, Convert.ToDateTime(_TempPolicyPaymentEntriesPost.InvoiceDate));
        //                        //finalres = (Convert.ToDouble(dcValue) - expectedIncoming).Value;
        //                        // generate Variance if the calculated value is greater than the tolerance value
        //                        //if ((Math.Abs(finalres) >= TruncationError) && expectedIncoming != 0)
        //                        //{
        //                        Flag = true;
        //                        //}
        //                    }
        //                //}
        //                //else //calculation of renewal percent
        //                //{
        //                //    //double? expectedIncoming = Convert.ToDouble(Premium) * _incomingSchedule.RenewalPercentage * SplitPer / 10000;

        //                //    double? expectedIncoming = _incomingSchedule.RenewalPercentage;
        //                //    if (expectedIncoming != null)
        //                //    {
        //                //        PolicyPaymentEntriesPost.Expectedpayment = decimal.Parse(expectedIncoming.Value.ToString());
        //                //    }

        //                //    //double finalres = (Convert.ToDouble(_PolicyPaymentEntriesPost.TotalPayment) - expectedIncoming).Value;

        //                //    double finalres = (Convert.ToDouble(_PolicyPaymentEntriesPost.CommissionPercentage) - expectedIncoming).Value;

        //                //    if ((Math.Abs(finalres) >= TruncationError && expectedIncoming != 0))
        //                //    {
        //                //        ////get data when variance is generated Means get all payment at that invoice ,and get variance
        //                //        //  // don't delete this code ,only write here to remove to calling from database every time
        //                //        //  decimal dcValue = PolicyPaymentEntriesPost.GetTotalpaymentOnInvoiceDate(_TempPolicyPaymentEntriesPost.PolicyID, Convert.ToDateTime(_TempPolicyPaymentEntriesPost.InvoiceDate));
        //                //        //  finalres = (Convert.ToDouble(dcValue) - expectedIncoming).Value;
        //                //        //   // generate Variance if the calculated value is greater than the tolerance value
        //                //        //  if ((Math.Abs(finalres) >= TruncationError) && expectedIncoming != 0)
        //                //        //  {
        //                //        Flag = true;
        //                //        //}
        //                //    }
        //                //}
        //            }
        //            else if (_incomingSchedule.ScheduleTypeId == (int)MasterBasicIncomingSchedule.PerHead)
        //            {
        //                if (IsFirstYr == FirstYrRenewalYr.FirstYear)
        //                {
        //                    //double? expectedIncoming = _PolicyPaymentEntriesPost.NumberOfUnits * _incomingSchedule.FirstYearPercentage * SplitPer / 100;

        //                    // comparasion with dolloer per unit
        //                    double? expectedFee = _incomingSchedule.FirstYearPercentage;

        //                    if (expectedFee != null)
        //                    {
        //                        PolicyPaymentEntriesPost.Expectedpayment = decimal.Parse(expectedFee.Value.ToString());
        //                    }
        //                    //double finalres = (Convert.ToDouble(_PolicyPaymentEntriesPost.TotalPayment) - expectedFee).Value;

        //                    finalres = (Convert.ToDouble(_PolicyPaymentEntriesPost.DollerPerUnit) - expectedFee).Value;

        //                    if ((Math.Abs(finalres) >= TruncationError) && (expectedFee != 0))
        //                    {
        //                        ////get data when variance is generated Means get all payment at that invoice ,and get variance
        //                        //// don't delete this code ,only write here to remove to calling from database every time
        //                        //decimal dcValue = PolicyPaymentEntriesPost.GetTotalpaymentOnInvoiceDate(_TempPolicyPaymentEntriesPost.PolicyID, Convert.ToDateTime(_TempPolicyPaymentEntriesPost.InvoiceDate));
        //                        //finalres = (Convert.ToDouble(dcValue) - expectedFee).Value;
        //                        // generate Variance if the calculated value is greater than the tolerance value
        //                        //if ((Math.Abs(finalres) >= TruncationError) && expectedFee != 0)
        //                        //{
        //                        Flag = true;
        //                        //}
        //                    }
        //                }
        //                else
        //                {
        //                    //double? expectedIncoming = _PolicyPaymentEntriesPost.NumberOfUnits * _incomingSchedule.RenewalPercentage * SplitPer / 100;
        //                    //expected fee for renewal percentage
        //                    double? expectedFee = _incomingSchedule.RenewalPercentage;
        //                    if (expectedFee != null)
        //                    {
        //                        PolicyPaymentEntriesPost.Expectedpayment = decimal.Parse(expectedFee.Value.ToString());
        //                    }

        //                    //Un Comment on "26 august 2013"
        //                    //double finalres = (Convert.ToDouble(_PolicyPaymentEntriesPost.TotalPayment) - expectedFee).Value;

        //                    //comparasion with dolloer per unit
        //                    //Comment on "26 august 2013"
        //                     finalres = (Convert.ToDouble(_PolicyPaymentEntriesPost.DollerPerUnit) - expectedFee).Value;

        //                    if ((Math.Abs(finalres) >= TruncationError) && (expectedFee != 0))
        //                    {
        //                        ////get data when variance is generated Means get all payment at that invoice ,and get variance
        //                        //// don't delete this code ,only write here to remove to calling from database every time
        //                        //decimal dcValue = PolicyPaymentEntriesPost.GetTotalpaymentOnInvoiceDate(_TempPolicyPaymentEntriesPost.PolicyID, Convert.ToDateTime(_TempPolicyPaymentEntriesPost.InvoiceDate));
        //                        //finalres = (Convert.ToDouble(dcValue) - expectedFee).Value;
        //                        //// generate Variance if the calculated value is greater than the tolerance value
        //                        //if ((Math.Abs(finalres) >= TruncationError) && expectedFee != 0)
        //                        //{
        //                        Flag = true;
        //                        //}
        //                    }
        //                }
        //            }
        //        }
        //        else if (_PolicySchedule == PolicySchedule.Advance)
        //        {
        //            decimal calAmount = 0;

        //            PolicyIncomingSchedule _AdvanceSchedule = GetAdvanceIncomingScheduleOfPolicy(_PolicyPaymentEntriesPost.PolicyID);
        //            if (_AdvanceSchedule == null || _AdvanceSchedule.IncomingScheduleList == null || _AdvanceSchedule.IncomingScheduleList.Count == 0) return Flag;
        //            int AdvanceIncomingType = _AdvanceSchedule.ScheduleTypeId;
        //            _AdvanceSchedule = FillNullDateWithMaxSystemDate(_AdvanceSchedule);

        //            if (AdvanceIncomingType == (int)MasterAdvanceScheduleType.PercentageofPremium_scale)
        //            {
        //                double tempPremium = Convert.ToDouble(Premium);
        //                double? incomingperimum = 0;
        //                List<IncomingScheduleEntry> _InComingShedule = _AdvanceSchedule.IncomingScheduleList
        //                    .Where(p => p.EffectiveFromDate <= _PolicyPaymentEntriesPost.InvoiceDate)
        //                    .Where(p => p.EffectiveToDate >= _PolicyPaymentEntriesPost.InvoiceDate)
        //                    .Where(p => p.FromRange <= double.Parse(_PolicyPaymentEntriesPost.PaymentRecived.ToString())).OrderBy(p => p.FromRange).ToList();
        //                foreach (IncomingScheduleEntry intg in _InComingShedule)
        //                {
        //                    if (tempPremium > ((intg.ToRange - intg.FromRange) + GetValidFromForcalCulation(intg.FromRange)))
        //                    {
        //                        incomingperimum += Convert.ToDouble((intg.ToRange - intg.FromRange) + GetValidFromForcalCulation(intg.FromRange)) * intg.Rate * SplitPer / 10000;
        //                        tempPremium = tempPremium - Convert.ToDouble(intg.ToRange);
        //                    }
        //                    else
        //                    {
        //                        incomingperimum += tempPremium * intg.Rate * SplitPer / 10000;
        //                        tempPremium = 0;
        //                        break;
        //                    }
        //                }

        //                double? expectedIncoming = Convert.ToDouble(_PolicyPaymentEntriesPost.TotalPayment);
        //                if (expectedIncoming != null)
        //                {
        //                    PolicyPaymentEntriesPost.Expectedpayment = decimal.Parse(expectedIncoming.Value.ToString());
        //                }
        //                finalres = (incomingperimum - expectedIncoming).Value;
        //                if ((Math.Abs(finalres) <= TruncationError) || (expectedIncoming == 0))
        //                {
        //                    Flag = true;
        //                }
        //            }

        //            else if (AdvanceIncomingType == (int)MasterAdvanceScheduleType.PercentageofPremium_target)
        //            {
        //                double? perimumofper = 0;
        //                List<IncomingScheduleEntry> _InComingShedule = _AdvanceSchedule.IncomingScheduleList
        //                    .Where(p => p.EffectiveFromDate <= _PolicyPaymentEntriesPost.InvoiceDate)
        //                    .Where(p => p.EffectiveToDate >= _PolicyPaymentEntriesPost.InvoiceDate)
        //                    .Where(p => p.FromRange <= double.Parse(_PolicyPaymentEntriesPost.PaymentRecived.ToString()))
        //                    .Where(p => p.ToRange >= double.Parse(_PolicyPaymentEntriesPost.PaymentRecived.ToString())).ToList();
        //                foreach (IncomingScheduleEntry intg in _InComingShedule)
        //                {
        //                    perimumofper += Convert.ToDouble(Premium) * intg.Rate * SplitPer / 10000;
        //                }
        //                double? expectedIncoming = Convert.ToDouble(_PolicyPaymentEntriesPost.TotalPayment);
        //                if (expectedIncoming != null)
        //                {
        //                    PolicyPaymentEntriesPost.Expectedpayment = decimal.Parse(expectedIncoming.Value.ToString());
        //                }
        //                 finalres = (perimumofper - expectedIncoming).Value;

        //                if ((Math.Abs(finalres) >= TruncationError) || (expectedIncoming == 0))
        //                {
        //                    Flag = true;
        //                }
        //            }

        //            else if (AdvanceIncomingType == (int)MasterAdvanceScheduleType.PerHeadFee_scale)
        //            {
        //                List<IncomingScheduleEntry> _InComingShedule = _AdvanceSchedule.IncomingScheduleList
        //                    .Where(p => p.EffectiveFromDate <= _PolicyPaymentEntriesPost.InvoiceDate)
        //                    .Where(p => p.EffectiveToDate >= _PolicyPaymentEntriesPost.InvoiceDate)
        //                    .Where(p => p.FromRange <= Convert.ToDouble(_PolicyPaymentEntriesPost.NumberOfUnits)).ToList();
        //                int TempHead = _PolicyPaymentEntriesPost.NumberOfUnits;

        //                foreach (IncomingScheduleEntry intg in _InComingShedule)
        //                {
        //                    if (TempHead > (intg.ToRange - (intg.FromRange + GetValidFromForcalCulation(intg.FromRange))))
        //                    {
        //                        calAmount += Convert.ToDecimal(intg.ToRange - (intg.FromRange + GetValidFromForcalCulation(intg.FromRange))) * Convert.ToDecimal(intg.Rate) * Convert.ToDecimal(SplitPer) / 100;
        //                        TempHead = TempHead - Convert.ToInt32(intg.ToRange);
        //                    }
        //                    else
        //                    {
        //                        calAmount += TempHead * Convert.ToDecimal(intg.Rate) * Convert.ToDecimal(SplitPer) / 100;
        //                        TempHead = 0;
        //                        break;
        //                    }

        //                }

        //                decimal? expectedIncoming = _PolicyPaymentEntriesPost.TotalPayment;
        //                if (expectedIncoming != null)
        //                {
        //                    PolicyPaymentEntriesPost.Expectedpayment = decimal.Parse(expectedIncoming.Value.ToString());
        //                }
        //                 finalres = Convert.ToDouble((calAmount - expectedIncoming));
        //                if ((Math.Abs(finalres) <= TruncationError) || (expectedIncoming == 0))
        //                {
        //                    Flag = true;
        //                }
        //            }
        //            else if (AdvanceIncomingType == (int)MasterAdvanceScheduleType.PerHeadFee_target)
        //            {
        //                List<IncomingScheduleEntry> _InComingShedule = _AdvanceSchedule.IncomingScheduleList
        //                    .Where(p => p.EffectiveFromDate <= _PolicyPaymentEntriesPost.InvoiceDate)
        //                    .Where(p => p.EffectiveToDate >= _PolicyPaymentEntriesPost.InvoiceDate)
        //                    .Where(p => p.FromRange <= double.Parse(_PolicyPaymentEntriesPost.NumberOfUnits.ToString()))
        //                    .Where(p => p.ToRange >= double.Parse(_PolicyPaymentEntriesPost.NumberOfUnits.ToString())).ToList();
        //                foreach (IncomingScheduleEntry outg in _InComingShedule)
        //                {
        //                    calAmount += _PolicyPaymentEntriesPost.NumberOfUnits * Convert.ToDecimal(outg.Rate.ToString()) * Convert.ToDecimal(SplitPer) / 100;
        //                }

        //                decimal? expectedIncoming = _PolicyPaymentEntriesPost.TotalPayment;
        //                if (expectedIncoming != null)
        //                {
        //                    PolicyPaymentEntriesPost.Expectedpayment = decimal.Parse(expectedIncoming.Value.ToString());
        //                }
        //                finalres = Convert.ToDouble((calAmount - expectedIncoming));
        //                if ((Math.Abs(finalres) <= TruncationError) || (expectedIncoming == 0))
        //                {
        //                    Flag = true;
        //                }
        //            }
        //            else if (AdvanceIncomingType == (int)MasterAdvanceScheduleType.FlatDollar_modal)
        //            {
        //                List<IncomingScheduleEntry> _InComingShedule =
        //                    _AdvanceSchedule.IncomingScheduleList.Where(p => p.EffectiveFromDate <= _PolicyPaymentEntriesPost.InvoiceDate).
        //                    Where(p => p.EffectiveToDate >= _PolicyPaymentEntriesPost.InvoiceDate).ToList();
        //                foreach (IncomingScheduleEntry outg in _InComingShedule)
        //                {
        //                    calAmount += Convert.ToDecimal(outg.Rate) * Convert.ToDecimal(SplitPer) / 100;
        //                }

        //                decimal? expectedIncoming = _PolicyPaymentEntriesPost.TotalPayment;
        //                if (expectedIncoming != null)
        //                {
        //                    PolicyPaymentEntriesPost.Expectedpayment = decimal.Parse(expectedIncoming.Value.ToString());
        //                }
        //                finalres = Convert.ToDouble((calAmount - expectedIncoming));
        //                if ((Math.Abs(finalres) <= TruncationError) || (expectedIncoming == 0))
        //                {
        //                    Flag = true;
        //                }
        //            }

        //        }
        //    }
        //    catch
        //    {
        //    }
        //    return Flag;
        //}

        public static PolicyIncomingSchedule FillNullDateWithMaxSystemDate(PolicyIncomingSchedule _AdvanceSchedule)
        {
            try
            {
                foreach (IncomingScheduleEntry ise in _AdvanceSchedule.IncomingScheduleList)
                {
                    if (ise.EffectiveToDate.HasValue == false || ise.EffectiveToDate == null)
                    {
                        ise.EffectiveToDate = DateTime.MaxValue;
                    }
                }
            }
            catch (Exception ex)
            {
                ActionLogger.Logger.WriteLog("FillNullDateWithMaxSystemDate :" + ex.StackTrace.ToString(), true);
                ActionLogger.Logger.WriteLog("FillNullDateWithMaxSystemDate :" + ex.InnerException.ToString(), true);
            }
            return _AdvanceSchedule;
        }

        public static double? GetValidFromForcalCulation(double? FromRange)
        {
            double result = 0;

            try
            {
                if (FromRange == 0)
                    result = 0;
                else
                    result = 1;
            }
            catch
            {
                result = 0;
            }
            return result;

        }
        /// <summary>
        /// Check for use First Year or Renewal
        /// </summary>
        /// <param name="_invoicedate"></param>
        /// <param name="PolicyId"></param>
        /// <returns></returns>
        public static FirstYrRenewalYr IsUseFirstYear(DateTime? _invoicedate, DateTime? OriginalEffectiveDate, Guid PolicyId)
        {
            FirstYrRenewalYr Flag = FirstYrRenewalYr.None;
            try
            {
                //Policy _policy = GetPolicy(PolicyId);
                // PolicyLearnedField _policyLrndFld = PolicyLearnedField.GetPolicyLearnedFieldsPolicyWise(PolicyId);
                PolicyToolIncommingShedule _PolicyToolIncommingShedule = GetBasicIncomingScheduleOfPolicy(PolicyId);

                //if (_PolicyToolIncommingShedule == null) return FirstYrRenewalYr.None;
                //double FirstYr = _PolicyToolIncommingShedule.FirstYearPercentage ?? 0;
                //double Renewal = _PolicyToolIncommingShedule.RenewalPercentage ?? 0;
                //DateTime? EffDate = _policy.OriginalEffectiveDate;
                DateTime? EffDate = OriginalEffectiveDate;

                //change according eric 

                if (EffDate != null)
                {
                    DateTime? AddEffDate = EffDate.Value.AddMonths(12);
                    //invoice date should be greater or equal to original effective date and greater of add year
                    if ((AddEffDate.Value <= _invoicedate) && _invoicedate >= EffDate.Value)
                    {
                        Flag = FirstYrRenewalYr.Renewal;
                    }
                    else
                    {
                        Flag = FirstYrRenewalYr.FirstYear;
                    }
                }
                else
                {
                    //  // Acme commented Nov 29, 2019, as following if is wrong 
                    //if (FirstYr != Renewal)
                    //{
                    Flag = FirstYrRenewalYr.None;
                    //}
                    //else
                    //{
                    //    Flag = FirstYrRenewalYr.FirstYear;//Use any here first year and renewal both are equal
                    //}
                }
            }
            catch (Exception ex)
            {
                ActionLogger.Logger.WriteLog("IsUseFirstYear :" + ex.StackTrace.ToString(), true);
                ActionLogger.Logger.WriteLog("IsUseFirstYear :" + ex.InnerException.ToString(), true);
            }

            return Flag;


            //if (EffDate != null)
            //{
            //  if ((_invoicedate >= EffDate.Value) && (_invoicedate <= EffDate.Value.AddYears(1)))
            //  {
            //    Flag = FirstYrRenewalYr.FirstYear;
            //  }
            //  else if (_invoicedate > EffDate.Value.AddYears(1))
            //  {
            //    Flag = FirstYrRenewalYr.Renewal;
            //  }
            //}
            //else
            //{
            //  if (FirstYr != Renewal)
            //  {
            //    Flag = FirstYrRenewalYr.None;
            //  }
            //  else
            //  {
            //    Flag = FirstYrRenewalYr.FirstYear;//Use any here first year and renewal both are equal
            //  }
            //}

            //return Flag;
        }

        public static FirstYrRenewalYr IsUseFirstYearForOutGoing(DateTime? _invoicedate, DateTime? OriginalEffectiveDate, Guid PolicyId)
        {
            FirstYrRenewalYr Flag = FirstYrRenewalYr.None;
            try
            {
                PolicyDetailsData _policy = GetPolicy(PolicyId);
                //PolicyLearnedField _policyLrndFld = PolicyLearnedField.GetPolicyLearnedFieldsPolicyWise(PolicyId);
                //List<OutGoingPayment> _OutGoingPayment = GetBasicOutGoingScheduleOfPolicy(PolicyId);
                //double FirstYr = _PolicyToolIncommingShedule.FirstYearPercentage ?? 0;
                //double Renewal = _PolicyToolIncommingShedule.RenewalPercentage ?? 0;
                //DateTime? EffDate = _policyLrndFld.Effective;
                //DateTime? EffDate = _policy.OriginalEffectiveDate;

                DateTime? EffDate = OriginalEffectiveDate;

                if (EffDate != null)
                {
                    //Changed by Acme on March 21 - 2014. as per Eric's feedback to correctthe logic
                    //if ((_invoicedate >= EffDate.Value) && (_invoicedate <= EffDate.Value.AddYears(1)))
                    if ((_invoicedate >= EffDate.Value) && (_invoicedate < EffDate.Value.AddYears(1)))
                    {
                        Flag = FirstYrRenewalYr.FirstYear;
                    }
                    //else if (_invoicedate > EffDate.Value.AddYears(1))
                    else if (_invoicedate >= EffDate.Value.AddYears(1))
                    {
                        Flag = FirstYrRenewalYr.Renewal;
                    }
                }
                else
                {
                    Flag = FirstYrRenewalYr.None;

                }
            }
            catch (Exception ex)
            {
                ActionLogger.Logger.WriteLog("IsUseFirstYearForOutGoing :" + ex.StackTrace.ToString(), true);
                ActionLogger.Logger.WriteLog("IsUseFirstYearForOutGoing :" + ex.InnerException.ToString(), true);
            }

            return Flag;
        }
        /// <summary>
        /// Check For Varience in Outgoing Schedule
        /// </summary>
        /// <param name="PaymentEntryId"></param>
        /// <returns></returns>
        public static bool CheckForOutgoingScheduleVariance(Guid PaymentEntryId)
        {
            double TruncationError = Convert.ToDouble(Masters.SystemConstant.GetKeyValue("TruncationError"));
            bool Flag = false;
            PolicyPaymentEntriesPost _PolicyPaymentEntriesPost = PolicyPaymentEntriesPost.GetPolicyPaymentEntry(PaymentEntryId);
            try
            {
                if (_PolicyPaymentEntriesPost == null)
                {
                    return false;
                }

                decimal Premium = _PolicyPaymentEntriesPost.PaymentRecived;
                decimal DollerPerUnits = _PolicyPaymentEntriesPost.DollerPerUnit;
                decimal TotalPayment = _PolicyPaymentEntriesPost.TotalPayment;

                if (_PolicyPaymentEntriesPost.InvoiceDate == null)
                {
                    _PolicyPaymentEntriesPost.InvoiceDate = System.DateTime.Now;
                }
                DateTime InvoiceDate = _PolicyPaymentEntriesPost.InvoiceDate.Value;

                Guid PolicyId = _PolicyPaymentEntriesPost.PolicyID;
                PolicySchedule _PolicySchedule = CheckForOutgoingTypeOfSchedule(PolicyId);
                PolicyDetailsData _Policy = GetPolicy(PolicyId);

                FirstYrRenewalYr IsFirstYr = FirstYrRenewalYr.None;
                if (_PolicySchedule == PolicySchedule.None)
                {
                    Flag = false;
                }
                else if (_PolicySchedule == PolicySchedule.Basic)
                {
                    IsFirstYr = IsUseFirstYearForOutGoing(InvoiceDate, _Policy.OriginalEffectiveDate, PolicyId);
                    if (IsFirstYr == FirstYrRenewalYr.None)
                    {
                        Flag = false;
                        return Flag;
                    }
                    List<OutGoingPayment> _Schedule;

                    _Schedule = GetBasicOutGoingScheduleOfPolicy(PolicyId);
                    if (_Schedule.Count == 0) return Flag;
                    List<OutGoingPayment> _outgoingSchedule = _Schedule;
                    if (_outgoingSchedule.FirstOrDefault().ScheduleTypeId == (int)MasterBasicOutgoinSchedule.PercentageOfCommission)
                    {
                        Flag = true;//After dicuss


                    }
                    else if (_outgoingSchedule.FirstOrDefault().ScheduleTypeId == (int)MasterBasicOutgoinSchedule.PercentageOfPremium)
                    {
                        decimal calAmount = 0;
                        foreach (OutGoingPayment _OutGoingPayment in _outgoingSchedule)
                        {
                            if (IsFirstYr == FirstYrRenewalYr.FirstYear)
                            {
                                calAmount += Premium * decimal.Parse(_OutGoingPayment.FirstYearPercentage.Value.ToString()) / 100;
                            }
                            else if (IsFirstYr == FirstYrRenewalYr.Renewal)
                            {
                                calAmount += Premium * decimal.Parse(_OutGoingPayment.RenewalPercentage.Value.ToString()) / 100;

                            }
                        }

                        double finalres = Convert.ToDouble((calAmount - _PolicyPaymentEntriesPost.TotalPayment));
                        if (Math.Abs(finalres) <= TruncationError)
                        {
                            Flag = true;
                        }
                    }

                }
                else if (_PolicySchedule == PolicySchedule.Advance)
                {
                    PolicyOutgoingSchedule _AdvanceSchedule = GetAdvanceOutgoingScheduleOfPolicy(PolicyId);

                    if (_AdvanceSchedule.OutgoingScheduleList != null && _AdvanceSchedule.OutgoingScheduleList.Count != 0)
                    {
                        int AdvanceOutgoingType = _AdvanceSchedule.ScheduleTypeId;
                        decimal calAmount = 0;

                        _AdvanceSchedule = FillNullDateWithMaxSystemDate(_AdvanceSchedule);

                        if (AdvanceOutgoingType == (int)MasterAdvanceScheduleType.PercentageofPremium_scale)
                        {
                            List<OutgoingScheduleEntry> _OutgoingShedule = _AdvanceSchedule.OutgoingScheduleList.Where(p => p.EffectiveFromDate <= InvoiceDate)
                                .Where(p => p.EffectiveToDate >= InvoiceDate)
                                .Where(p => p.FromRange <= double.Parse(_PolicyPaymentEntriesPost.PaymentRecived.ToString())).ToList();

                            List<Guid> PayeeList = _OutgoingShedule.Select(p => p.PayeeUserCredentialId).Distinct().ToList();
                            calAmount = 0;
                            foreach (Guid payee in PayeeList)
                            {

                                decimal TempPremiumAmt = Premium;
                                List<OutgoingScheduleEntry> _tempOutg = _OutgoingShedule.Where(p => p.PayeeUserCredentialId == payee).OrderBy(p => p.FromRange).ToList();
                                foreach (OutgoingScheduleEntry outg in _tempOutg)
                                {

                                    if (TempPremiumAmt >
                                        Convert.ToDecimal(outg.ToRange
                                        -
                                        (outg.FromRange + GetValidFromForcalCulation(outg.FromRange))
                                        ))
                                    {
                                        calAmount += Convert.ToDecimal(outg.ToRange - (outg.FromRange + GetValidFromForcalCulation(outg.FromRange))) * Convert.ToDecimal(outg.Rate) / 100;
                                        TempPremiumAmt = TempPremiumAmt - Convert.ToDecimal(outg.ToRange.Value);
                                    }
                                    else
                                    {
                                        calAmount += TempPremiumAmt * decimal.Parse(outg.Rate.ToString()) / 100;
                                        TempPremiumAmt = 0;
                                        break;

                                    }
                                }
                            }

                            double finalres = Convert.ToDouble((calAmount - Convert.ToDecimal(_PolicyPaymentEntriesPost.TotalPayment)));
                            if (Math.Abs(finalres) <= TruncationError)
                            {
                                Flag = true;
                            }
                        }
                        else if (AdvanceOutgoingType == (int)MasterAdvanceScheduleType.PercentageofPremium_target)
                        {
                            calAmount = 0;
                            List<OutgoingScheduleEntry> _OutgoingShedule = _AdvanceSchedule.OutgoingScheduleList
                                .Where(p => p.EffectiveFromDate <= InvoiceDate)
                                .Where(p => p.EffectiveToDate >= InvoiceDate)
                                .Where(p => p.FromRange <= double.Parse(_PolicyPaymentEntriesPost.PaymentRecived.ToString()))
                                .Where(p => p.ToRange >= double.Parse(_PolicyPaymentEntriesPost.PaymentRecived.ToString())).ToList();
                            foreach (OutgoingScheduleEntry outg in _OutgoingShedule)
                            {
                                calAmount += Premium * decimal.Parse(outg.Rate.ToString()) / 100;
                            }

                            double finalres = Convert.ToDouble(calAmount - _PolicyPaymentEntriesPost.TotalPayment);
                            if (Math.Abs(finalres) <= TruncationError)
                            {
                                Flag = true;
                            }
                        }
                        else if (AdvanceOutgoingType == (int)MasterAdvanceScheduleType.PerHeadFee_scale)
                        {

                            List<OutgoingScheduleEntry> _OutgoingShedule = _AdvanceSchedule.OutgoingScheduleList
                                .Where(p => p.EffectiveFromDate <= InvoiceDate)
                                .Where(p => p.EffectiveToDate >= InvoiceDate)
                                .Where(p => p.FromRange <= double.Parse(_PolicyPaymentEntriesPost.NumberOfUnits.ToString())).ToList();
                            int NumberOfHead = _PolicyPaymentEntriesPost.NumberOfUnits;
                            List<Guid> PayeeList = _OutgoingShedule.Select(p => p.PayeeUserCredentialId).Distinct().ToList();
                            calAmount = 0;
                            foreach (Guid payee in PayeeList)
                            {

                                int temoNUmberOfHead = NumberOfHead;
                                List<OutgoingScheduleEntry> _tempOutg = _OutgoingShedule.Where(p => p.PayeeUserCredentialId == payee).OrderBy(p => p.FromRange).ToList();
                                foreach (OutgoingScheduleEntry outg in _tempOutg)
                                {

                                    if (temoNUmberOfHead > (outg.ToRange - (outg.FromRange + GetValidFromForcalCulation(outg.FromRange))))
                                    {
                                        calAmount += Convert.ToDecimal(outg.ToRange - (outg.FromRange + GetValidFromForcalCulation(outg.FromRange))) * Convert.ToDecimal(outg.Rate);
                                        temoNUmberOfHead = temoNUmberOfHead - Convert.ToInt32(outg.ToRange);
                                    }
                                    else
                                    {
                                        calAmount += temoNUmberOfHead * Convert.ToDecimal(outg.Rate);
                                        temoNUmberOfHead = 0;
                                        break;
                                    }

                                }
                            }

                            double finalres = Convert.ToDouble((calAmount - _PolicyPaymentEntriesPost.TotalPayment));
                            if (Math.Abs(finalres) <= TruncationError)
                            {
                                Flag = true;
                            }
                        }
                        else if (AdvanceOutgoingType == (int)MasterAdvanceScheduleType.PerHeadFee_target)
                        {
                            calAmount = 0;
                            List<OutgoingScheduleEntry> _OutgoingShedule = _AdvanceSchedule.OutgoingScheduleList
                                .Where(p => p.EffectiveFromDate <= InvoiceDate)
                                .Where(p => p.EffectiveToDate >= InvoiceDate)
                                .Where(p => p.FromRange <= double.Parse(_PolicyPaymentEntriesPost.NumberOfUnits.ToString()))
                                .Where(p => p.ToRange >= double.Parse(_PolicyPaymentEntriesPost.NumberOfUnits.ToString())).ToList();
                            foreach (OutgoingScheduleEntry outg in _OutgoingShedule)
                            {
                                calAmount += Convert.ToDecimal(outg.Rate) * _PolicyPaymentEntriesPost.NumberOfUnits;
                            }
                            double finalres = Convert.ToDouble((calAmount - _PolicyPaymentEntriesPost.TotalPayment));
                            if (Math.Abs(finalres) <= TruncationError)
                            {
                                Flag = true;
                            }
                        }
                        else if (AdvanceOutgoingType == (int)MasterAdvanceScheduleType.FlatDollar_modal)
                        {
                            calAmount = 0;
                            List<OutgoingScheduleEntry> _OutgoingShedule = _AdvanceSchedule.OutgoingScheduleList
                                .Where(p => p.EffectiveFromDate <= InvoiceDate)
                                .Where(p => p.EffectiveToDate >= InvoiceDate).ToList();
                            foreach (OutgoingScheduleEntry outg in _OutgoingShedule)
                            {
                                calAmount += decimal.Parse(outg.Rate.ToString());
                            }
                            double finalres = Convert.ToDouble(calAmount - _PolicyPaymentEntriesPost.TotalPayment);
                            if (Math.Abs(finalres) <= TruncationError)
                            {
                                Flag = true;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ActionLogger.Logger.WriteLog("CheckForOutgoingScheduleVariance :" + ex.StackTrace.ToString(), true);
                ActionLogger.Logger.WriteLog("CheckForOutgoingScheduleVariance :" + ex.InnerException.ToString(), true);
            }
            return Flag;
        }
        /// <summary>
        /// Check for OutGoing Schedule Varience For the Active Policy against a Previous Payment
        /// </summary>
        /// <param name="PaymentEntryId"></param>
        /// <param name="ActivePolicyId"></param>
        /// <returns></returns>
        public static bool CheckForOutgoingScheduleVariance(Guid PaymentEntryId, Guid ActivePolicyId)
        {

            double TruncationError = Convert.ToDouble(Masters.SystemConstant.GetKeyValue("TruncationError"));

            bool Flag = false;
            try
            {
                PolicyPaymentEntriesPost _PolicyPaymentEntriesPost = PolicyPaymentEntriesPost.GetPolicyPaymentEntry(PaymentEntryId);

                decimal Premium = _PolicyPaymentEntriesPost.PaymentRecived;
                decimal DollerPerUnits = _PolicyPaymentEntriesPost.DollerPerUnit;
                decimal TotalPayment = _PolicyPaymentEntriesPost.TotalPayment;
                DateTime InvoiceDate = _PolicyPaymentEntriesPost.InvoiceDate.Value;

                PolicySchedule _PolicySchedule = CheckForOutgoingTypeOfSchedule(ActivePolicyId);
                PolicyDetailsData _Policy = GetPolicy(ActivePolicyId);

                FirstYrRenewalYr IsFirstYr = FirstYrRenewalYr.None;
                if (_PolicySchedule == PolicySchedule.None)
                {
                    Flag = false;
                }
                else if (_PolicySchedule == PolicySchedule.Basic)
                {
                    IsFirstYr = PostUtill.IsUseFirstYear(InvoiceDate, _Policy.OriginalEffectiveDate, ActivePolicyId);
                    if (IsFirstYr == FirstYrRenewalYr.None)
                    {
                        Flag = false;
                        return Flag;
                    }
                    List<OutGoingPayment> _Schedule;
                    _Schedule = PostUtill.GetBasicOutGoingScheduleOfPolicy(ActivePolicyId);
                    if (_Schedule == null || _Schedule.Count == 0)
                    {
                        return false;
                    }
                    List<OutGoingPayment> _outgoingSchedule = _Schedule;
                    if (_outgoingSchedule.FirstOrDefault().ScheduleTypeId == (int)MasterBasicOutgoinSchedule.PercentageOfCommission)
                    {
                        Flag = true;//After dicuss

                    }
                    else if (_outgoingSchedule.FirstOrDefault().ScheduleTypeId == (int)MasterBasicOutgoinSchedule.PercentageOfPremium)
                    {
                        decimal calAmount = 0;
                        foreach (OutGoingPayment _OutGoingPayment in _outgoingSchedule)
                        {
                            if (IsFirstYr == FirstYrRenewalYr.FirstYear)
                            {
                                calAmount += Premium * decimal.Parse(_OutGoingPayment.FirstYearPercentage.Value.ToString()) / 100;
                            }
                            else if (IsFirstYr == FirstYrRenewalYr.Renewal)
                            {
                                calAmount += Premium * decimal.Parse(_OutGoingPayment.RenewalPercentage.Value.ToString()) / 100;

                            }
                        }

                        double finalres = Convert.ToDouble((calAmount - _PolicyPaymentEntriesPost.TotalPayment));
                        if (Math.Abs(finalres) <= TruncationError)
                        {
                            Flag = true;
                        }
                    }

                }
                else if (_PolicySchedule == PolicySchedule.Advance)
                {
                    PolicyOutgoingSchedule _AdvanceSchedule = PostUtill.GetAdvanceOutgoingScheduleOfPolicy(ActivePolicyId);
                    if (_AdvanceSchedule == null || _AdvanceSchedule.OutgoingScheduleList == null || _AdvanceSchedule.OutgoingScheduleList.Count == 0)
                    {
                        return false;
                    }
                    int AdvanceOutgoingType = _AdvanceSchedule.ScheduleTypeId;
                    decimal calAmount = 0;

                    _AdvanceSchedule = PostUtill.FillNullDateWithMaxSystemDate(_AdvanceSchedule);

                    if (AdvanceOutgoingType == (int)MasterAdvanceScheduleType.PercentageofPremium_scale)
                    {
                        List<OutgoingScheduleEntry> _OutgoingShedule = _AdvanceSchedule.OutgoingScheduleList.Where(p => p.EffectiveFromDate <= InvoiceDate)
                            .Where(p => p.EffectiveToDate >= InvoiceDate)
                            .Where(p => p.FromRange <= double.Parse(_PolicyPaymentEntriesPost.PaymentRecived.ToString())).ToList();

                        List<Guid> PayeeList = _OutgoingShedule.Select(p => p.PayeeUserCredentialId).Distinct().ToList();
                        calAmount = 0;
                        foreach (Guid payee in PayeeList)
                        {

                            decimal TempPremiumAmt = Premium;
                            List<OutgoingScheduleEntry> _tempOutg = _OutgoingShedule.Where(p => p.PayeeUserCredentialId == payee).OrderBy(p => p.FromRange).ToList();
                            foreach (OutgoingScheduleEntry outg in _tempOutg)
                            {
                                if (TempPremiumAmt >
                                    Convert.ToDecimal(outg.ToRange
                                    -
                                    (outg.FromRange + PostUtill.GetValidFromForcalCulation(outg.FromRange))
                                    ))
                                {
                                    calAmount += Convert.ToDecimal(outg.ToRange - (outg.FromRange + GetValidFromForcalCulation(outg.FromRange))) * Convert.ToDecimal(outg.Rate) / 100;
                                    TempPremiumAmt = TempPremiumAmt - Convert.ToDecimal(outg.ToRange.Value);
                                }
                                else
                                {
                                    calAmount += TempPremiumAmt * decimal.Parse(outg.Rate.ToString()) / 100;
                                    TempPremiumAmt = 0;
                                    break;

                                }
                            }
                        }

                        double finalres = Convert.ToDouble((calAmount - Convert.ToDecimal(_PolicyPaymentEntriesPost.TotalPayment)));
                        if (Math.Abs(finalres) <= TruncationError)
                        {
                            Flag = true;
                        }
                    }
                    else if (AdvanceOutgoingType == (int)MasterAdvanceScheduleType.PercentageofPremium_target)
                    {
                        calAmount = 0;
                        List<OutgoingScheduleEntry> _OutgoingShedule = _AdvanceSchedule.OutgoingScheduleList
                            .Where(p => p.EffectiveFromDate <= InvoiceDate)
                            .Where(p => p.EffectiveToDate >= InvoiceDate)
                            .Where(p => p.FromRange <= double.Parse(_PolicyPaymentEntriesPost.PaymentRecived.ToString()))
                            .Where(p => p.ToRange >= double.Parse(_PolicyPaymentEntriesPost.PaymentRecived.ToString())).ToList();
                        foreach (OutgoingScheduleEntry outg in _OutgoingShedule)
                        {
                            calAmount += Premium * decimal.Parse(outg.Rate.ToString()) / 100;
                        }

                        double finalres = Convert.ToDouble(calAmount - _PolicyPaymentEntriesPost.TotalPayment);
                        if (Math.Abs(finalres) <= TruncationError)
                        {
                            Flag = true;
                        }
                    }
                    else if (AdvanceOutgoingType == (int)MasterAdvanceScheduleType.PerHeadFee_scale)
                    {

                        List<OutgoingScheduleEntry> _OutgoingShedule = _AdvanceSchedule.OutgoingScheduleList
                            .Where(p => p.EffectiveFromDate <= InvoiceDate)
                            .Where(p => p.EffectiveToDate >= InvoiceDate)
                            .Where(p => p.FromRange <= double.Parse(_PolicyPaymentEntriesPost.NumberOfUnits.ToString())).ToList();
                        int NumberOfHead = _PolicyPaymentEntriesPost.NumberOfUnits;
                        List<Guid> PayeeList = _OutgoingShedule.Select(p => p.PayeeUserCredentialId).Distinct().ToList();
                        calAmount = 0;
                        foreach (Guid payee in PayeeList)
                        {

                            int temoNUmberOfHead = NumberOfHead;
                            List<OutgoingScheduleEntry> _tempOutg = _OutgoingShedule.Where(p => p.PayeeUserCredentialId == payee).OrderBy(p => p.FromRange).ToList();
                            foreach (OutgoingScheduleEntry outg in _tempOutg)
                            {

                                if (temoNUmberOfHead > (outg.ToRange - (outg.FromRange + GetValidFromForcalCulation(outg.FromRange))))
                                {
                                    calAmount += Convert.ToDecimal(outg.ToRange - (outg.FromRange + GetValidFromForcalCulation(outg.FromRange))) * Convert.ToDecimal(outg.Rate);
                                    temoNUmberOfHead = temoNUmberOfHead - Convert.ToInt32(outg.ToRange);
                                }
                                else
                                {
                                    calAmount += temoNUmberOfHead * Convert.ToDecimal(outg.Rate);
                                    temoNUmberOfHead = 0;
                                    break;
                                }

                            }
                        }

                        double finalres = Convert.ToDouble((calAmount - _PolicyPaymentEntriesPost.TotalPayment));
                        if (Math.Abs(finalres) <= TruncationError)
                        {
                            Flag = true;
                        }
                    }
                    else if (AdvanceOutgoingType == (int)MasterAdvanceScheduleType.PerHeadFee_target)
                    {
                        calAmount = 0;
                        List<OutgoingScheduleEntry> _OutgoingShedule = _AdvanceSchedule.OutgoingScheduleList
                            .Where(p => p.EffectiveFromDate <= InvoiceDate)
                            .Where(p => p.EffectiveToDate >= InvoiceDate)
                            .Where(p => p.FromRange <= double.Parse(_PolicyPaymentEntriesPost.NumberOfUnits.ToString()))
                            .Where(p => p.ToRange >= double.Parse(_PolicyPaymentEntriesPost.NumberOfUnits.ToString())).ToList();
                        foreach (OutgoingScheduleEntry outg in _OutgoingShedule)
                        {
                            calAmount += Convert.ToDecimal(outg.Rate) * _PolicyPaymentEntriesPost.NumberOfUnits;
                        }
                        double finalres = Convert.ToDouble((calAmount - _PolicyPaymentEntriesPost.TotalPayment));
                        if (Math.Abs(finalres) <= TruncationError)
                        {
                            Flag = true;
                        }
                    }
                    else if (AdvanceOutgoingType == (int)MasterAdvanceScheduleType.FlatDollar_modal)
                    {
                        calAmount = 0;
                        List<OutgoingScheduleEntry> _OutgoingShedule = _AdvanceSchedule.OutgoingScheduleList
                            .Where(p => p.EffectiveFromDate <= InvoiceDate)
                            .Where(p => p.EffectiveToDate >= InvoiceDate).ToList();
                        foreach (OutgoingScheduleEntry outg in _OutgoingShedule)
                        {
                            calAmount += decimal.Parse(outg.Rate.ToString());
                        }
                        double finalres = Convert.ToDouble(calAmount - _PolicyPaymentEntriesPost.TotalPayment);
                        if (Math.Abs(finalres) <= TruncationError)
                        {
                            Flag = true;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ActionLogger.Logger.WriteLog("CheckForOutgoingScheduleVariance :" + ex.StackTrace.ToString(), true);
                ActionLogger.Logger.WriteLog("CheckForOutgoingScheduleVariance :" + ex.InnerException.ToString(), true);
            }
            return Flag;
        }

        private static PolicyOutgoingSchedule FillNullDateWithMaxSystemDate(PolicyOutgoingSchedule _AdvanceSchedule)
        {
            foreach (OutgoingScheduleEntry ise in _AdvanceSchedule.OutgoingScheduleList)
            {
                if (ise.EffectiveToDate.HasValue == false || ise.EffectiveToDate == null)
                {
                    ise.EffectiveToDate = DateTime.MaxValue;
                }
            }
            return _AdvanceSchedule;
        }


        public static PolicySchedule CheckForIncomingTypeOfSchedule(Guid policyid)
        {
            if (policyid == new Guid()) return PolicySchedule.None;

            bool? flag = GetPolicy(policyid).IsIncomingBasicSchedule;
            if (flag == null)
                return PolicySchedule.None;

            return flag.Value ? PolicySchedule.Basic : PolicySchedule.Advance;
        }

        public static PolicySchedule CheckForOutgoingTypeOfSchedule(Guid PolicyId)
        {
            return PolicySchedule.Basic;

            //Commented by vinod 03082016
            //Only basic schudule is running & advance schule is not implemented

            //if (PolicyId == new Guid()) return PolicySchedule.None;

            //bool? flag = GetPolicy(PolicyId).IsOutGoingBasicSchedule;
            //if (flag == null)
            //    return PolicySchedule.None;

            //return flag.Value ? PolicySchedule.Basic : PolicySchedule.Advance;

        }

        public static PolicyToolIncommingShedule GetBasicIncomingScheduleOfPolicy(Guid PolicyID)
        {
            PolicyToolIncommingShedule PolicyIncomSche = null;
            try
            {
                using (DLinq.CommissionDepartmentEntities DataModel = Entity.DataModel)
                {
                    DLinq.PolicyIncomingSchedule _policyincomingschedule = (from f in DataModel.PolicyIncomingSchedules where f.PolicyId.Value == PolicyID select f).FirstOrDefault();

                    if (_policyincomingschedule != null)
                    {
                        PolicyIncomSche = new PolicyToolIncommingShedule();
                        PolicyIncomSche.FirstYearPercentage = _policyincomingschedule.FirstYearPercentage;
                        PolicyIncomSche.IncomingScheduleId = _policyincomingschedule.IncomingScheduleId;
                        PolicyIncomSche.PolicyId = _policyincomingschedule.PolicyId.Value;
                        PolicyIncomSche.RenewalPercentage = _policyincomingschedule.RenewalPercentage;
                        PolicyIncomSche.ScheduleTypeId = _policyincomingschedule.ScheduleTypeId.Value;
                    }
                    
                }
            }
            catch (Exception ex)
            {
                ActionLogger.Logger.WriteLog("GetBasicIncomingScheduleOfPolicy :" + ex.StackTrace.ToString(), true);
                ActionLogger.Logger.WriteLog("GetBasicIncomingScheduleOfPolicy :" + ex.InnerException.ToString(), true);
            }
            return PolicyIncomSche;
        }

        public static List<OutGoingPayment> GetBasicOutGoingScheduleOfPolicy(Guid PolicyId)
        {
            return OutGoingPayment.GetOutgoingSheduleForPolicy(PolicyId);
        }

        private static PolicyIncomingSchedule GetAdvanceIncomingScheduleOfPolicy(Guid PolicyId)
        {
            PolicyIncomingSchedule _IncomingSchedule = IncomingSchedule.GetPolicyIncomingSchedule(PolicyId);
            return _IncomingSchedule;

        }

        public static PolicyOutgoingSchedule GetAdvanceOutgoingScheduleOfPolicy(Guid PolicyId)
        {
            PolicyOutgoingSchedule _OutGoingSchedule = OutgoingSchedule.GetPolicyOutgoingSchedule(PolicyId);
            return _OutGoingSchedule;

        }

        public static DateTime? GetOldestInvoiceDate(Guid PolicyId)
        {
            DateTime? dtTime = null;
            try
            {
                List<PolicyPaymentEntriesPost> _PolicyPaymentEntriesPost = PolicyPaymentEntriesPost.GetPolicyPaymentEntryPolicyIDWise(PolicyId);
                if (_PolicyPaymentEntriesPost == null || _PolicyPaymentEntriesPost.Count == 0)
                {
                    dtTime = null;
                }
                else
                {
                    dtTime = _PolicyPaymentEntriesPost.Min(p => p.InvoiceDate);
                }
            }
            catch
            {
            }
            return dtTime;
        }

        public static DateTime? GetGreaterInvoiceDate(Guid PolicyId)
        {
            DateTime? dtTime = null;

            try
            {

                List<PolicyPaymentEntriesPost> _PolicyPaymentEntriesPost = PolicyPaymentEntriesPost.GetPolicyGreatestInvoiceDate(PolicyId);
                if (_PolicyPaymentEntriesPost == null || _PolicyPaymentEntriesPost.Count == 0)
                {
                    dtTime = null;
                }
                else
                {
                    dtTime = _PolicyPaymentEntriesPost.Max(p => p.InvoiceDate);
                }
            }
            catch (Exception)
            {
            }
            return dtTime;
        }

        public static DateTime? DateComparer(DateTime? date1, DateTime? date2)
        {
            DateTime? dtTime = null;
            try
            {
                if (date1 != null && date2 != null)
                {
                    dtTime = DateTime.Compare(date1.Value, date2.Value) <= 0 ? date1 : date2;
                }
                else if (date1 == null)
                {
                    dtTime = date2;
                }
                else if (date2 == null)
                {
                    dtTime = date1;
                }
            }
            catch
            {
            }

            return dtTime;
        }

        public static DateTime? GetGreaterDate(DateTime? date1, DateTime? date2)
        {
            if (date1 != null && date2 != null)
            {
                return DateTime.Compare(date1.Value, date2.Value) >= 0 ? date1 : date2;
            }
            else if (date1 == null)
            {
                return date2;
            }
            else if (date2 == null)
            {
                return date1;
            }
            return null;
        }
        /// <summary>
        /// Calculate The Track From Date
        /// </summary>
        /// ------- Comment #4 From Pankaj 2011-06-20 11:06:13 [reply] -------
        //After discussion, final date to update "smart track from date" as minimum date
        //from following 3
        //1. Stored Smart Track Date
        //2. Oldest Invoice Date
        //3. Policy Detail Track From Date
        /// <param name="PolicyId"></param>
        /// <returns></returns>
        public static DateTime? CalculateTrackFromDate(PolicyDetailsData _Policy)
        {
            DateTime? TFdate = null;
            try
            {
                if (_Policy != null)
                {
                    if (_Policy.TrackFromDate != null)
                    {
                        DateTime? PolicyTrackFrmDate = _Policy.TrackFromDate;
                        PolicyLearnedFieldData _PolicyLearnedField = PolicyLearnedField.GetPolicyLearnedFieldsPolicyWise(_Policy.PolicyId);

                        if (_PolicyLearnedField != null)
                        {
                            if (_PolicyLearnedField.TrackFrom != null)
                            {
                                DateTime? LearnedFieldTrackFrmDate = _PolicyLearnedField == null ? null : _PolicyLearnedField.TrackFrom;
                                DateTime? olderstInvoiceDate = GetOldestInvoiceDate(_Policy.PolicyId);

                                TFdate = DateComparer(DateComparer(PolicyTrackFrmDate, LearnedFieldTrackFrmDate), olderstInvoiceDate);
                            }
                        }
                    }
                }                
             
            }
            catch (Exception ex)
            {
                ActionLogger.Logger.WriteLog("*************Start CalculateTrackFromDate*************", true);
                ActionLogger.Logger.WriteLog("CalculateTrackFromDate ex.StackTrace :" + ex.StackTrace.ToString(), true);
                ActionLogger.Logger.WriteLog("CalculateTrackFromDate ex.InnerException :" + ex.Message.ToString(), true);
                ActionLogger.Logger.WriteLog("*************End CalculateTrackFromDate*************", true);
            }

            return TFdate;

        }

        public static DateTime? CalculateTrackFromDate(Guid PolicyId)
        {
            DateTime? TFdate = null;
            try
            {
                //Dictionary<string, object> parameters = new Dictionary<string, object>();
                //if (!parameters.ContainsKey("PolicyId"))
                //    parameters.Add("PolicyId", PolicyId);
                //DateTime? PolicyTrackFrmDate = Policy.GetPolicyData(parameters).FirstOrDefault().TrackFromDate;

                DateTime? PolicyTrackFrmDate =Policy.GetPolicyTrackDate(PolicyId);

                PolicyLearnedFieldData _PolicyLearnedField = PolicyLearnedField.GetPolicyLearnedFieldsPolicyWise(PolicyId);  
              
                DateTime? LearnedFieldTrackFrmDate = _PolicyLearnedField == null ? null : _PolicyLearnedField.TrackFrom;

                DateTime? olderstInvoiceDate = GetOldestInvoiceDate(PolicyId);

                TFdate = DateComparer(DateComparer(PolicyTrackFrmDate, LearnedFieldTrackFrmDate), olderstInvoiceDate);
            }
            catch (Exception ex)
            {
                ActionLogger.Logger.WriteLog("*************Start CalculateTrackFromDate1*************", true);
                ActionLogger.Logger.WriteLog("CalculateTrackFromDate1 ex.StackTrace:" + ex.StackTrace.ToString(), true);
                ActionLogger.Logger.WriteLog("CalculateTrackFromDate1 ex.InnerException:" + ex.Message.ToString(), true);
                ActionLogger.Logger.WriteLog("*************End CalculateTrackFromDate1*************", true);
            }

            return TFdate;

        }

        public static void RemoveLinkPaymentToPolicy(Guid? Cliid)
        {

        }
    }

}
