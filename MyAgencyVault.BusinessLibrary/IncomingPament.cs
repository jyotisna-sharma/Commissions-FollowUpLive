using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MyAgencyVault.BusinessLibrary.Base;
using System.Runtime.Serialization;

namespace MyAgencyVault.BusinessLibrary
{
    [DataContract]
    public class IncomingPament : IEditable<IncomingPament>
    {
        #region IEditable<IncomingPament> Members

        public void AddUpdate()
        {
            throw new NotImplementedException();
        }

        public void Delete()
        {
            throw new NotImplementedException();
        }

        public IncomingPament GetOfID()
        {
            throw new NotImplementedException();
        }

        public bool IsValid()
        {
            throw new NotImplementedException();
        }

        #endregion

        /// <summary>
        /// unlink the payment entry from the policy, to which currently it is associated to.
        /// </summary>
        /// <returns>return true on successfull attempts, else false</returns>
        public bool UnlinkIncomingPayments()
        {
            throw new NotImplementedException();
        }
        /// <summary>
        /// link this payment entry to and existing policy.
        /// </summary>
        /// <returns>return true on successfull attempts, else false</returns>
        public bool LinkIncomingPaymentsToAnExistingPolicy(Guid policyId)
        {
            throw new NotImplementedException();
        }
        /// <summary>
        /// make the linked policy to be active. 
        /// </summary>
        /// <returns>return true on successfull attempts, else false</returns>
        public bool ActivateNewPolicy()
        {
            throw new NotImplementedException();
        }
        /// <summary>
        /// developer need to recheck and think of the requirement of this funciton.
        /// </summary>
        /// <param name="policyID"></param>
        /// <returns>returns all the incoming payments related to a policyid given in the parameter.</returns>
        public static List<IncomingPament> GetIncomingPayments(Guid policyID)
        {
            throw new NotImplementedException();
        }
    }
}
