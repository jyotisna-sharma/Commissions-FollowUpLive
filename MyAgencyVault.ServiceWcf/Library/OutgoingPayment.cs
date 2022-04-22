using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MyAgencyVault.BusinessLibrary;
using System.ServiceModel;

namespace MyAgencyVault.WcfService
{
    [ServiceContract]
    interface IOutgoingPayment
    {
        [OperationContract]
        void AddUpdateOutgoingPayment(List<OutGoingPayment> GisgoingSchedule);
        
        [OperationContract]
        void DeleteOutgoingPayment(OutGoingPayment OutGoingPymnt);
        
        [OperationContract]
        List<OutGoingPayment> GetOutgoingPayments();

        [OperationContract]
        List<OutGoingPayment> GetOutgoingSheduleForPolicy(Guid PolicyId);
    }

    public partial class MavService : IOutgoingPayment
    {
        public void AddUpdateOutgoingPayment(List<OutGoingPayment> GisgoingSchedule)
        {
            OutGoingPayment.AddUpdate(GisgoingSchedule);
        }

        public void DeleteOutgoingPayment(OutGoingPayment OutGoingPymnt)
        {
            OutGoingPymnt.Delete();
        }

        public List<OutGoingPayment> GetOutgoingPayments()
        {
            return OutGoingPayment.GetOutgoingShedule();
        }

        public List<OutGoingPayment> GetOutgoingSheduleForPolicy(Guid PolicyId)
        {
            return OutGoingPayment.GetOutgoingSheduleForPolicy(PolicyId);
        }
    }
}
