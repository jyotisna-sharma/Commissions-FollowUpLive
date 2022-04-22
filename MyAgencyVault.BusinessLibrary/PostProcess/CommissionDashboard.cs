using System;
using System.Transactions;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MyAgencyVault.BusinessLibrary.Base;
using MyAgencyVault.BusinessLibrary.Masters;
using System.Runtime.Serialization;
using DLinq = DataAccessLayer.LinqtoEntity;
using System.Threading;

namespace MyAgencyVault.BusinessLibrary
{
    public class CommissionDashboard
    {
        public static List<PolicyPaymentEntriesPost> GetPolicyPaymentEntry(Guid PolicyId)
        {
            List<PolicyPaymentEntriesPost> _PolicyPaymentEntriesPost = PolicyPaymentEntriesPost.GetPolicyPaymentEntryPolicyIDWise(PolicyId);
            return _PolicyPaymentEntriesPost;
        }

        public static List<PolicyOutgoingDistribution> GetPolicyOutgoingPayment(Guid PolicyPaymentEntryId)
        {
            List<PolicyOutgoingDistribution> _PolicyOutgoingDistribution = PolicyOutgoingDistribution.GetOutgoingPaymentByPoicyPaymentEntryId(PolicyPaymentEntryId);
            return _PolicyOutgoingDistribution;
        }

        public static List<DisplayFollowupIssue> GetPolicyCommissionIssues(Guid PolicyId)
        {
            List<DisplayFollowupIssue> _FollowupIssue = FollowupIssue.GetIssues(PolicyId);
            return _FollowupIssue;
        }

        public static PolicyPaymentEntriesPost GetPolicyPaymentPaymentEntryEntryIdWise(Guid PolicyEntryid)
        {
            return PolicyPaymentEntriesPost.GetPolicyPaymentEntry(PolicyEntryid);
        }

        public static PostProcessReturnStatus CommissionDashBoardPostStartClienVMWrapper(PolicyDetailsData SelectedPolicy, PolicyPaymentEntriesPost PaymentEntry, PostEntryProcess _PostEntryProcess, UserRole _UserRole)
        {
            #region Process
            try
            {
                if (_PostEntryProcess == PostEntryProcess.FirstPost)
                {
                    Batch batch = Policy.GenerateBatch(SelectedPolicy);
                    return CommissionDashBoardPostStart(batch.BatchId, PaymentEntry, _PostEntryProcess, _UserRole);
                }
                else
                    return CommissionDashBoardPostStart(Guid.Empty, PaymentEntry, _PostEntryProcess, _UserRole);
            }
            finally
            {
                PolicyLocking.UnlockPolicy(SelectedPolicy.PolicyId);
            }

            #endregion
        }

        public static PostProcessReturnStatus CommissionDashBoardPostStart(Guid BatchId, PolicyPaymentEntriesPost PaymentEntry, PostEntryProcess _PostEntryProcess, UserRole _UserRole)
        {
            var options = new TransactionOptions
            {
                IsolationLevel = IsolationLevel.ReadCommitted,
                Timeout = TimeSpan.FromMinutes(60)
            };

            using (TransactionScope ts = new TransactionScope(TransactionScopeOption.Required, options))
            {
                PostProcessReturnStatus _PostProcessReturnStatus = new PostProcessReturnStatus() { DeuEntryId = Guid.Empty, IsComplete = false, ErrorMessage = null, PostEntryStatus = _PostEntryProcess };

                if (_PostEntryProcess == PostEntryProcess.FirstPost)
                {
                    PolicyDetailsData _Policy = PostUtill.GetPolicy(PaymentEntry.PolicyID);
                    Guid PayorId = _Policy.PayorId ?? Guid.Empty;
                    decimal PaymentRecived = PaymentEntry.TotalPayment;
                    Guid CreatedBy = PaymentEntry.CreatedBy;
                    Statement _Statement = Policy.GenerateStatment(BatchId, PayorId, PaymentRecived, CreatedBy);
                    PaymentEntry.StmtID = _Statement.StatementID;
                    DEU _DEU = PostUtill.GetDeuCollection(PaymentEntry, _Policy);
                    DEU objDEU = new DEU();
                    objDEU.AddupdateDeuEntry(_DEU);
                    _PostProcessReturnStatus = PostUtill.PostStart(_PostEntryProcess, _DEU.DEUENtryID, Guid.Empty, Guid.Empty, _UserRole, _PostEntryProcess, string.Empty, string.Empty);
                }
                else if (_PostEntryProcess == PostEntryProcess.RePost)
                {
                    PolicyDetailsData _Policy = PostUtill.GetPolicy(PaymentEntry.PolicyID);
                    DEU _DEU = PostUtill.GetDeuCollection(PaymentEntry, _Policy);
                    DEU objDEU = new DEU();
                    objDEU.AddupdateDeuEntry(_DEU);
                    _PostProcessReturnStatus = PostUtill.PostStart(_PostEntryProcess, PaymentEntry.DEUEntryId.Value, _DEU.DEUENtryID, Guid.Empty, _UserRole, _PostEntryProcess, string.Empty, string.Empty);

                }
                if (_PostProcessReturnStatus.IsComplete)
                {
                    ts.Complete();
                }
                return _PostProcessReturnStatus;
            }
        }

        public static PostProcessReturnStatus RemoveCommissiondashBoardIncomingPayment(PolicyPaymentEntriesPost PolicySelectedIncomingPaymentCommissionDashBoard, UserRole _UserRole)
        {
            try
            {
                var options = new TransactionOptions
                {
                    IsolationLevel = IsolationLevel.ReadCommitted,
                    Timeout = TimeSpan.FromMinutes(60)
                };
                using (TransactionScope ts = new TransactionScope(TransactionScopeOption.Required, options))
                {
                    PostProcessReturnStatus _PostProcessReturnStatus = PostUtill.PostStart(PostEntryProcess.Delete,
                        PolicySelectedIncomingPaymentCommissionDashBoard.DEUEntryId.Value, Guid.Empty, Guid.Empty, _UserRole, PostEntryProcess.Delete, string.Empty, "Delete");

                    if (_PostProcessReturnStatus.IsComplete)
                    {
                        ts.Complete();
                    }
                    return _PostProcessReturnStatus;
                }
            }
            finally
            {
                //PolicyLocking.UnlockPolicy(PolicySelectedIncomingPaymentCommissionDashBoard.PolicyID);
            }
        }

        public static PostProcessReturnStatus UnlinkCommissiondashBoardIncomingPayment(PolicyPaymentEntriesPost PolicySelectedIncomingPaymentCommissionDashBoard, UserRole _UserRole)
        {
            PostProcessReturnStatus _PostProcessReturnStatus = null;
            try
            {
                var options = new TransactionOptions
                {
                    IsolationLevel = IsolationLevel.ReadCommitted,
                    Timeout = TimeSpan.FromMinutes(60)
                };
                using (TransactionScope ts = new TransactionScope(TransactionScopeOption.Required, options))
                {

                    string strUnlink = string.Empty;
                    DEU _DEU = DEU.GetDeuEntryidWiseForUnlikingPolicy(PolicySelectedIncomingPaymentCommissionDashBoard.DEUEntryId ?? Guid.Empty);
                    PostUtill.PostStart(PostEntryProcess.Delete, _DEU.DEUENtryID, Guid.Empty, Guid.Empty, _UserRole, PostEntryProcess.FirstPost, strUnlink, string.Empty);

                    if (_DEU != null)
                    {
                        strUnlink = Convert.ToString(_DEU.UnlinkClientName);

                    }
                    DEU objDEU = new DEU();
                    objDEU.AddupdateUnlinkDeuEntry(_DEU, strUnlink);

                    _PostProcessReturnStatus = PostUtill.PostStart(PostEntryProcess.FirstPost, _DEU.DEUENtryID, Guid.Empty, Guid.Empty, _UserRole, PostEntryProcess.FirstPost, strUnlink, string.Empty);

                    if (_PostProcessReturnStatus.IsComplete)
                    {
                        ts.Complete();
                    }
                    return _PostProcessReturnStatus;
                }
            }
            finally
            {
            }
        }

    }
}
