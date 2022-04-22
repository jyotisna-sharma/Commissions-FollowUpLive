using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MyAgencyVault.BusinessLibrary.Base;
using System.ServiceModel;
using MyAgencyVault.BusinessLibrary;

namespace MyAgencyVault.WcfService
{
     [ServiceContract] 
    interface IPolicyOutgoingDistribution
    {
         [OperationContract]
         
         bool IsEntryMarkPaid(Guid PaymentEntryId);
         [OperationContract]
         
         List<PolicyOutgoingDistribution> GetOutgoingPaymentByPoicyPaymentEntryId(Guid EntryId);
         [OperationContract]
         
         void DeleteByPolicyIncomingPaymentId(Guid PaymentEntryId);
         [OperationContract]
         
         void AddUpdateOutgoingPaymentEntry(PolicyOutgoingDistribution _PolicyOutgoingDistribution);
         [OperationContract]
         
         void AddUpdateOutGoingPaymentEntries(List<PolicyOutgoingDistribution> _PolicyOutgoingDistribution);
         [OperationContract]
        
         void DeleteOutGoingPaymentViaOutgoingPaymentId(Guid OutgoingPaymentid);
    }
     public partial class MavService : IPolicyOutgoingDistribution
     {
         #region IPolicyOutgoingDistribution Members

         
         public bool IsEntryMarkPaid(Guid PaymentEntryId)
         {
             return PolicyOutgoingDistribution.IsEntryMarkPaid(PaymentEntryId);
         }

         #endregion

         
         #region IPolicyOutgoingDistribution Members

         
         public List<PolicyOutgoingDistribution> GetOutgoingPaymentByPoicyPaymentEntryId(Guid EntryId)
         {
            return  PolicyOutgoingDistribution.GetOutgoingPaymentByPoicyPaymentEntryId(EntryId);
         }

         #endregion

         #region IPolicyOutgoingDistribution Members

         
         public void DeleteByPolicyIncomingPaymentId(Guid PaymentEntryId)
         {
             PolicyOutgoingDistribution.DeleteByPolicyIncomingPaymentId(PaymentEntryId);
         }

         #endregion

         #region IPolicyOutgoingDistribution Members

         
         public void AddUpdateOutgoingPaymentEntry(PolicyOutgoingDistribution _PolicyOutgoingDistribution)
         {
              PolicyOutgoingDistribution.AddUpdateOutgoingPaymentEntry(_PolicyOutgoingDistribution);
         }

         #endregion

         #region IPolicyOutgoingDistribution Members

         
         public void AddUpdateOutGoingPaymentEntries(List<PolicyOutgoingDistribution> _PolicyOutgoingDistribution)
         {
             foreach (PolicyOutgoingDistribution _PolicyOutg in _PolicyOutgoingDistribution)
             {
                 PolicyOutgoingDistribution.AddUpdateOutgoingPaymentEntry(_PolicyOutg);
             }
         }

         #endregion

         #region IPolicyOutgoingDistribution Members
         
         public void DeleteOutGoingPaymentViaOutgoingPaymentId(Guid OutgoingPaymentid)
         {
             PolicyOutgoingDistribution.DeleteById(OutgoingPaymentid);
         }

         #endregion
     }
}
