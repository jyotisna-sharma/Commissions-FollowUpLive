using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MyAgencyVault.BusinessLibrary.Base;
using System.Runtime.Serialization;
using DLinq = DataAccessLayer.LinqtoEntity;


namespace MyAgencyVault.BusinessLibrary
{
    public class OutGoingPayment
    {
        #region "Data members aka - public properties"
        [DataMember]
        public Guid OutgoingScheduleId { get; set; }
        [DataMember]
        public Guid PolicyId { get; set; }
        [DataMember]
        public string Payor { get; set; }
        [DataMember]
        public Guid PayeeUserCredentialId { get; set; }
        [DataMember]
        public double? FirstYearPercentage { get; set; }
        [DataMember]
        public double? RenewalPercentage { get; set; }
        [DataMember]
        public bool IsPrimaryAgent { get; set; }

        [DataMember]
        public bool IsEditDisable { get; set; }

        
        [DataMember]
        public int ScheduleTypeId { get; set; }
        [DataMember]
        public DateTime? CreatedOn { get; set; }

        #endregion

        #region IEditable<OutgoingPayment> Members
        public void AddUpdate()
        {
            throw new NotImplementedException();
        }

        public static void AddUpdate(List<OutGoingPayment> GlobalOutPayment)
        {
            using (DLinq.CommissionDepartmentEntities DataModel = Entity.DataModel)
            {
                DLinq.PolicyOutgoingSchedule gcs = null;

                foreach (OutGoingPayment _globalCoveragesShedule in GlobalOutPayment)
                {

                    //DLinq.Policy _objPolicy = new DLinq.Policy();
                    //_objPolicy = (from l in DataModel.Policies where l.PolicyId == _globalCoveragesShedule.PolicyId select l).FirstOrDefault();

                    //DLinq.UserCredential _objPayee = new DLinq.UserCredential();
                    //_objPayee = (from l in DataModel.UserCredentials where l.UserCredentialId == _globalCoveragesShedule.PayeeUserCredentialId select l).FirstOrDefault();

                    gcs = (from e in DataModel.PolicyOutgoingSchedules
                           where e.OutgoingScheduleId == _globalCoveragesShedule.OutgoingScheduleId
                           select e).FirstOrDefault();
                    if (gcs == null)
                    {
                        gcs = new DLinq.PolicyOutgoingSchedule
                        {

                            OutgoingScheduleId = _globalCoveragesShedule.OutgoingScheduleId,
                            FirstYearPercentage = _globalCoveragesShedule.FirstYearPercentage,
                            RenewalPercentage = _globalCoveragesShedule.RenewalPercentage,
                            IsPrimaryAgent = _globalCoveragesShedule.IsPrimaryAgent,
                            ScheduleTypeId = _globalCoveragesShedule.ScheduleTypeId,
                            CreatedOn=DateTime.Today,
                            
                        };

                        gcs.PolicyReference.Value = (from f in DataModel.Policies where f.PolicyId == _globalCoveragesShedule.PolicyId select f).FirstOrDefault();
                        //gcs.UserDetailReference.Value = _objPayee;
                        gcs.UserCredentialReference.Value = (from f in DataModel.UserCredentials where f.UserCredentialId == _globalCoveragesShedule.PayeeUserCredentialId select f).FirstOrDefault();
                        DataModel.AddToPolicyOutgoingSchedules(gcs);

                    }
                    else
                    {
                        gcs.FirstYearPercentage = _globalCoveragesShedule.FirstYearPercentage;
                        gcs.RenewalPercentage = _globalCoveragesShedule.RenewalPercentage;
                        gcs.IsPrimaryAgent = _globalCoveragesShedule.IsPrimaryAgent;
                        gcs.ScheduleTypeId = _globalCoveragesShedule.ScheduleTypeId;
                    }
                    DataModel.SaveChanges();
                }
              

            }

        }

        public void Delete()
        {
            using (DLinq.CommissionDepartmentEntities DataModel = Entity.DataModel)
            {
                DLinq.PolicyOutgoingSchedule _policyOutgoingSchedule = (from n in DataModel.PolicyOutgoingSchedules
                                                                        where (n.OutgoingScheduleId == this.OutgoingScheduleId)
                                                                        select n).FirstOrDefault();

                if (_policyOutgoingSchedule != null)
                {
                    DataModel.DeleteObject(_policyOutgoingSchedule);
                    DataModel.SaveChanges();
                }
            }
        }

        #endregion

        public static List<OutGoingPayment> GetOutgoingShedule()
        {
            using (DLinq.CommissionDepartmentEntities DataModel = Entity.DataModel)
            {
                return (from gc in DataModel.PolicyOutgoingSchedules
                        select new OutGoingPayment
                        {
                            RenewalPercentage = gc.RenewalPercentage,
                            FirstYearPercentage = gc.FirstYearPercentage,
                            IsPrimaryAgent = gc.IsPrimaryAgent,
                            OutgoingScheduleId = gc.OutgoingScheduleId,
                            PolicyId = gc.Policy.PolicyId,
                            PayeeUserCredentialId = gc.UserCredential.UserCredentialId,
                            ScheduleTypeId = gc.ScheduleTypeId.Value,
                            CreatedOn = gc.CreatedOn,
                            Payor = gc.UserCredential.UserDetail.NickName,                           
                        }).ToList();
            }
        }

        public static List<OutGoingPayment> GetOutgoingSheduleForPolicy(Guid PolicyId)
        {
            using (DLinq.CommissionDepartmentEntities DataModel = Entity.DataModel)
            {
                return (from gc in DataModel.PolicyOutgoingSchedules
                        where (gc.PolicyId == PolicyId)
                        select new OutGoingPayment
                        {
                            RenewalPercentage = gc.RenewalPercentage,
                            FirstYearPercentage = gc.FirstYearPercentage,
                            IsPrimaryAgent = gc.IsPrimaryAgent,
                            OutgoingScheduleId = gc.OutgoingScheduleId,
                            PolicyId = gc.Policy.PolicyId,
                            PayeeUserCredentialId = gc.UserCredential.UserCredentialId,
                            ScheduleTypeId = gc.ScheduleTypeId.Value,
                            CreatedOn = gc.CreatedOn,
                            Payor = gc.UserCredential.UserDetail.NickName,                            
                        }).ToList();
            }
        }

        public void DeleteSchedule(List<OutGoingPayment> DeleteOutPayment)
        {
            using (DLinq.CommissionDepartmentEntities DataModel = Entity.DataModel)
            {
                DLinq.PolicyOutgoingSchedule gcs = null;
                foreach (OutGoingPayment _out in DeleteOutPayment)
                {
                    gcs = (from e in DataModel.PolicyOutgoingSchedules
                           where e.OutgoingScheduleId == _out.OutgoingScheduleId
                           select e).FirstOrDefault();
                    if (gcs != null)
                    {
                        DataModel.DeleteObject(gcs);
                        DataModel.SaveChanges();
                    }
                }

            }
        }

        public static void DeletePolicyOutGoingSchedulebyPolicyId(Guid PolicyId)
        {
            using (DLinq.CommissionDepartmentEntities DataModel = Entity.DataModel)
            {
               List< DLinq.PolicyOutgoingSchedule> gcs = DataModel.PolicyOutgoingSchedules.Where(p => p.PolicyId == PolicyId).ToList <DLinq.PolicyOutgoingSchedule>();
                foreach (DLinq.PolicyOutgoingSchedule _out in gcs)
                {
                  
                    DataModel.DeleteObject(_out);
                        DataModel.SaveChanges();
                }
            }
        }

        public static bool IsUserPresentAsPayee(Guid userId)
        {
            using (DLinq.CommissionDepartmentEntities DataModel = Entity.DataModel)
            {
                int count = DataModel.PolicyOutgoingAdvancedSchedules.Where(s => s.PayeeUserCredentialId == userId).Count();
                if (count != 0)
                    return true;
                else
                    return false;
            }
        }
    }
}
