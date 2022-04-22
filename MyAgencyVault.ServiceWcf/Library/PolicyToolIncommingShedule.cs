using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MyAgencyVault.BusinessLibrary;
using System.ServiceModel;

namespace MyAgencyVault.WcfService
{

    [ServiceContract]
    interface IPolicyToolIncommingShedule
    {
        #region IEditable<PolicyToolIncommingShedule> Members
        [OperationContract]
        void AddUpdatePolicyToolIncommingShedule(PolicyToolIncommingShedule PInc);
        [OperationContract]
        void Delete(PolicyToolIncommingShedule PInc);
       [OperationContract]
        List<PolicyToolIncommingShedule> GetPolicyToolIncommingSheduleList();
       [OperationContract]
       PolicyToolIncommingShedule GetPolicyToolIncommingSheduleListPolicyWise(Guid PolicyId);
        #endregion
    }

    public partial class MavService : IPolicyToolIncommingShedule
    {
        public void AddUpdatePolicyToolIncommingShedule(PolicyToolIncommingShedule PInc)
        {
             PInc.AddUpdate();
        }

        public void Delete(PolicyToolIncommingShedule PInc)
        {
            PInc.Delete();
        }


        public List<PolicyToolIncommingShedule> GetPolicyToolIncommingSheduleList()
        {
            return PolicyToolIncommingShedule.GetPolicyToolIncommingSheduleList();
        }




        #region IPolicyToolIncommingShedule Members


        public PolicyToolIncommingShedule GetPolicyToolIncommingSheduleListPolicyWise(Guid PolicyId)
        {
            return PolicyToolIncommingShedule.GetPolicyToolIncommingSheduleListPolicyWise(PolicyId);
        }

        #endregion
    }
}