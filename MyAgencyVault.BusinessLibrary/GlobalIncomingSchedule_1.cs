using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;
using MyAgencyVault.BusinessLibrary.Base;
using DLinq = DataAccessLayer.LinqtoEntity;

namespace MyAgencyVault.BusinessLibrary
{
    [DataContract]
    public class IncomingScheduleEntry
    {
        [DataMember]
        public Guid CoveragesScheduleId { get; set; }
        [DataMember]
        public double? FromRange { get; set; }
        [DataMember]
        public double? ToRange { get; set; }
        [DataMember]
        public double? Rate { get; set; }
        [DataMember]
        public DateTime? EffectiveFromDate { get; set; }
        [DataMember]
        public DateTime? EffectiveToDate { get; set; }
        [DataMember]
        public bool IsDeleted { get; set; }
    }

    [DataContract]
    public class OutgoingScheduleEntry
    {
        [DataMember]
        public Guid CoveragesScheduleId { get; set; }
        [DataMember]
        public double? FromRange { get; set; }
        [DataMember]
        public double? ToRange { get; set; }
        [DataMember]
        public double? Rate { get; set; }
        [DataMember]
        public DateTime? EffectiveFromDate { get; set; }
        [DataMember]
        public DateTime? EffectiveToDate { get; set; }
        [DataMember]
        public Guid PayeeUserCredentialId { get; set; }
        [DataMember]
        public string PayeeName { get; set; }
        [DataMember]
        public bool IsPrimaryAgent { get; set; }
    }

    [DataContract]
    public class GlobalIncomingSchedule
    {
        [DataMember]
        public Guid CarrierId { get; set; }
        [DataMember]
        public Guid CoverageId { get; set; }
        [DataMember]
        public string ScheduleTypeName { get; set; }
        [DataMember]
        public string CarrierName { get; set; }
        [DataMember]
        public string ProductName { get; set; }
        [DataMember]
        public int ScheduleTypeId { get; set; }
        [DataMember]
        public List<IncomingScheduleEntry> IncomingScheduleList { get; set; }
        [DataMember]
        public bool IsModified { get; set; }
    }

    [DataContract]
    public class PolicyIncomingSchedule
    {
        [DataMember]
        public Guid PolicyId { get; set; }
        [DataMember]
        public string ScheduleTypeName { get; set; }
        [DataMember]
        public int ScheduleTypeId { get; set; }
        [DataMember]
        public List<IncomingScheduleEntry> IncomingScheduleList { get; set; }
        [DataMember]
        public bool IsModified { get; set; }
    }

    [DataContract]
    public class PolicyIncomingPayType
    {
        [DataMember]
        public int IncomingPaymentTypeId { get; set; }
        [DataMember]
        public string Name { get; set; }       
       
    }

    [DataContract]
    public class PolicyOutgoingSchedule
    {
        [DataMember]
        public Guid PolicyId { get; set; }
        [DataMember]
        public string ScheduleTypeName { get; set; }
        [DataMember]
        public int ScheduleTypeId { get; set; }
        [DataMember]
        public List<OutgoingScheduleEntry> OutgoingScheduleList { get; set; }
    }

    [DataContract]
    public class IncomingSchedule
    {
        #region IEditable<IncomingSchedule> Members

        public static void AddUpdateGlobalSchedule(GlobalIncomingSchedule globalSchedule)
        {
            using (DLinq.CommissionDepartmentEntities DataModel = Entity.DataModel)
            {
                DLinq.GlobalCoveragesSchedule gcs = null;

                if (globalSchedule.IncomingScheduleList == null || globalSchedule.IncomingScheduleList.Count == 0)
                {
                    DeleteGlobalSchedule(globalSchedule.CarrierId, globalSchedule.CoverageId, DataModel);
                    return;
                }

                List<DLinq.GlobalCoveragesSchedule> schedule = (from e in DataModel.GlobalCoveragesSchedules
                                                                where e.CarrierId == globalSchedule.CarrierId && e.CoverageId == globalSchedule.CoverageId && e.IsDeleted == false
                       select e).ToList();

                foreach (DLinq.GlobalCoveragesSchedule entry in schedule)
                {
                    IncomingScheduleEntry tmpEntry = globalSchedule.IncomingScheduleList.FirstOrDefault(s => s.CoveragesScheduleId == entry.CoveragesScheduleId);
                    if(tmpEntry == null)
                        entry.IsDeleted = true;
                }

                foreach (IncomingScheduleEntry _globalCoveragesShedule in globalSchedule.IncomingScheduleList)
                {
                    DLinq.MasterScheduleType _objSheduleType = new DLinq.MasterScheduleType();
                    
                    gcs = (from e in DataModel.GlobalCoveragesSchedules
                           where e.CoveragesScheduleId == _globalCoveragesShedule.CoveragesScheduleId
                           select e).FirstOrDefault();

                    if (gcs == null)
                    {
                        gcs = new DLinq.GlobalCoveragesSchedule
                        {
                            CoveragesScheduleId = _globalCoveragesShedule.CoveragesScheduleId,
                            FromRange = _globalCoveragesShedule.FromRange,
                            ToRange = _globalCoveragesShedule.ToRange,
                            EffectiveToDate = _globalCoveragesShedule.EffectiveToDate,
                            EffectiveFromDate = _globalCoveragesShedule.EffectiveFromDate,
                            Rate = _globalCoveragesShedule.Rate,
                            ScheduleTypeId = globalSchedule.ScheduleTypeId,
                            CoverageId = globalSchedule.CoverageId,
                            CarrierId = globalSchedule.CarrierId,
                            IsDeleted = false
                        };
                        DataModel.AddToGlobalCoveragesSchedules(gcs);
                    }
                    else
                    {
                        gcs.FromRange = _globalCoveragesShedule.FromRange;
                        gcs.ToRange = _globalCoveragesShedule.ToRange;
                        gcs.EffectiveToDate = _globalCoveragesShedule.EffectiveToDate;
                        gcs.EffectiveFromDate = _globalCoveragesShedule.EffectiveFromDate;
                        gcs.Rate = _globalCoveragesShedule.Rate;
                        gcs.ScheduleTypeId = globalSchedule.ScheduleTypeId;
                        gcs.CoverageId = globalSchedule.CoverageId;
                        gcs.CarrierId = globalSchedule.CarrierId;
                    }
                }

                DataModel.SaveChanges();
            }

        }

        private static void DeleteGlobalSchedule(Guid carrierId, Guid coverageId, DLinq.CommissionDepartmentEntities DataModel)
        {
            List<DLinq.GlobalCoveragesSchedule> schedule = DataModel.GlobalCoveragesSchedules.Where(s => s.CarrierId == carrierId && s.CoverageId == coverageId).ToList();
            foreach (DLinq.GlobalCoveragesSchedule entry in schedule)
                entry.IsDeleted = true;
            DataModel.SaveChanges();
        }

        public static void AddUpdatePolicySchedule(PolicyIncomingSchedule policySchedule)
        {
            using (DLinq.CommissionDepartmentEntities DataModel = Entity.DataModel)
            {
                DLinq.PolicyIncomingAdvancedSchedule gcs = null;

                if (policySchedule.IncomingScheduleList == null || policySchedule.IncomingScheduleList.Count == 0)
                {
                    DeletePolicySchedule(policySchedule.PolicyId);
                    return;
                }

                List<DLinq.PolicyIncomingAdvancedSchedule> schedule = (from e in DataModel.PolicyIncomingAdvancedSchedules
                                                                where e.PolicyId == policySchedule.PolicyId
                                                                select e).ToList();

                foreach (DLinq.PolicyIncomingAdvancedSchedule entry in schedule)
                {
                    IncomingScheduleEntry tmpEntry = policySchedule.IncomingScheduleList.FirstOrDefault(s => s.CoveragesScheduleId == entry.IncomingAdvancedScheduleId);
                    if (tmpEntry == null)
                        DataModel.DeleteObject(entry);
                }

                foreach (IncomingScheduleEntry _policyIncominShedule in policySchedule.IncomingScheduleList)
                {
                    DLinq.MasterScheduleType _objSheduleType = new DLinq.MasterScheduleType();

                    gcs = (from e in DataModel.PolicyIncomingAdvancedSchedules
                           where e.IncomingAdvancedScheduleId == _policyIncominShedule.CoveragesScheduleId
                           select e).FirstOrDefault();

                    if (gcs == null)
                    {
                        gcs = new DLinq.PolicyIncomingAdvancedSchedule
                        {
                            PolicyId = policySchedule.PolicyId,
                            IncomingAdvancedScheduleId = _policyIncominShedule.CoveragesScheduleId,
                            FromRange = _policyIncominShedule.FromRange,
                            ToRange = _policyIncominShedule.ToRange,
                            EffectiveToDate = _policyIncominShedule.EffectiveToDate,
                            EffectiveFromDate = _policyIncominShedule.EffectiveFromDate,
                            Rate = _policyIncominShedule.Rate,
                            ScheduleTypeId = policySchedule.ScheduleTypeId
                        };
                        DataModel.AddToPolicyIncomingAdvancedSchedules(gcs);
                    }
                    else
                    {
                        gcs.FromRange = _policyIncominShedule.FromRange;
                        gcs.ToRange = _policyIncominShedule.ToRange;
                        gcs.EffectiveToDate = _policyIncominShedule.EffectiveToDate;
                        gcs.EffectiveFromDate = _policyIncominShedule.EffectiveFromDate;
                        gcs.Rate = _policyIncominShedule.Rate;
                        gcs.ScheduleTypeId = policySchedule.ScheduleTypeId;
                    }
                }

                DataModel.SaveChanges();
            }
        }

        public static void DeletePolicySchedule(Guid PolicyId)
        {
            using (DLinq.CommissionDepartmentEntities DataModel = Entity.DataModel)
            {
                List<DLinq.PolicyIncomingAdvancedSchedule> schedule = DataModel.PolicyIncomingAdvancedSchedules.Where(s => s.PolicyId == PolicyId).ToList();
                foreach (DLinq.PolicyIncomingAdvancedSchedule entry in schedule)
                {
                    DataModel.DeleteObject(entry);
                    DataModel.SaveChanges();
                }
            }
        }

        #endregion

        #region  "Data members aka - public properties"
        
        [DataMember]
        public Guid CarrierId { get; set; }
        [DataMember]
        public Guid CoverageId { get; set; }
        [DataMember]
        public string ScheduleTypeName { get; set; }
        [DataMember]
        public string CarrierName { get; set; }
        [DataMember]
        public string ProductName { get; set; }
        [DataMember]
        public int ScheduleTypeId { get; set; }
        [DataMember]
        public List<IncomingScheduleEntry> IncomingScheduleList { get; set; }
        [DataMember]
        public bool IsModified { get; set; }
        #endregion

        public static GlobalIncomingSchedule GetGlobalIncomingSchedule(Guid carrierId, Guid coverageId)
        {
            using (DLinq.CommissionDepartmentEntities DataModel = Entity.DataModel)
            {
                GlobalIncomingSchedule globalIncomingSchedule = new GlobalIncomingSchedule { CarrierId = carrierId, CoverageId = coverageId };
                foreach (DLinq.GlobalCoveragesSchedule sch in DataModel.GlobalCoveragesSchedules)
                {
                    if (sch.CarrierId == carrierId && sch.CoverageId == coverageId && sch.IsDeleted == false)
                    {
                        IncomingScheduleEntry scheduleEntry = new IncomingScheduleEntry
                        {
                            CoveragesScheduleId = sch.CoveragesScheduleId,
                            FromRange = sch.FromRange,
                            ToRange = sch.ToRange,
                            EffectiveFromDate = sch.EffectiveFromDate,
                            EffectiveToDate = sch.EffectiveToDate,
                            Rate = sch.Rate,
                        };

                        if (globalIncomingSchedule.IncomingScheduleList == null)
                        {
                            globalIncomingSchedule.IncomingScheduleList = new List<IncomingScheduleEntry>();
                            
                            globalIncomingSchedule.CoverageId = coverageId;
                            globalIncomingSchedule.CarrierId = carrierId;
                            globalIncomingSchedule.CarrierName = sch.Carrier.CarrierName;
                            globalIncomingSchedule.ProductName = sch.Coverage.ProductName;
                            globalIncomingSchedule.ScheduleTypeId = sch.ScheduleTypeId;
                            globalIncomingSchedule.ScheduleTypeName = sch.MasterScheduleType.Name;
                        }

                        globalIncomingSchedule.IncomingScheduleList.Add(scheduleEntry);
                    }
                }
                return globalIncomingSchedule;
            }
        }

        public static PolicyIncomingSchedule GetPolicyIncomingSchedule(Guid PolicyId)
        {
            using (DLinq.CommissionDepartmentEntities DataModel = Entity.DataModel)
            {
                PolicyIncomingSchedule policyIncomingSchedule = new PolicyIncomingSchedule { PolicyId = PolicyId };
                var PolicyIncomingSchedules = from PIS in DataModel.PolicyIncomingAdvancedSchedules
                                              where PIS.PolicyId == PolicyId
                                              select PIS;
                foreach (DLinq.PolicyIncomingAdvancedSchedule sch in PolicyIncomingSchedules)
                {
                    if (sch.PolicyId == PolicyId)
                    {
                        IncomingScheduleEntry scheduleEntry = new IncomingScheduleEntry
                        {
                            CoveragesScheduleId = sch.IncomingAdvancedScheduleId,
                            FromRange = sch.FromRange,
                            ToRange = sch.ToRange,
                            EffectiveFromDate = sch.EffectiveFromDate,
                            EffectiveToDate = sch.EffectiveToDate,
                            Rate = sch.Rate,
                        };

                        if (policyIncomingSchedule.IncomingScheduleList == null)
                        {
                            policyIncomingSchedule.IncomingScheduleList = new List<IncomingScheduleEntry>();

                            policyIncomingSchedule.PolicyId = PolicyId;
                            policyIncomingSchedule.ScheduleTypeId = sch.ScheduleTypeId.Value;
                            policyIncomingSchedule.ScheduleTypeName = sch.MasterScheduleType.Name;
                        }

                        policyIncomingSchedule.IncomingScheduleList.Add(scheduleEntry);
                    }
                }
                return policyIncomingSchedule;
            }
        }

        public static void ChangeScheduleType(Guid carrierId,Guid coverageId,int scheduleType)
        {
            using (DLinq.CommissionDepartmentEntities DataModel = Entity.DataModel)
            {
                List<DLinq.GlobalCoveragesSchedule> gcs = null;

                gcs = (from e in DataModel.GlobalCoveragesSchedules
                       where e.CoverageId == coverageId && e.CarrierId == carrierId
                       select e).ToList();

                foreach (DLinq.GlobalCoveragesSchedule gc in gcs)
                {
                    gc.ScheduleTypeId = scheduleType;
                    gc.MasterScheduleType = DataModel.MasterScheduleTypes.FirstOrDefault(s => s.ScheduleTypeId == scheduleType);
                }
                DataModel.SaveChanges();
            }
        }
    }

    [DataContract]
    public class OutgoingSchedule
    {
        public static void AddUpdatePolicyOutgoingSchedule(PolicyOutgoingSchedule policySchedule)
        {
            using (DLinq.CommissionDepartmentEntities DataModel = Entity.DataModel)
            {
                DLinq.PolicyOutgoingAdvancedSchedule gcs = null;

                if (policySchedule.OutgoingScheduleList == null || policySchedule.OutgoingScheduleList.Count == 0)
                {
                    DeleteOutgoingPolicySchedule(policySchedule.PolicyId);
                    return;
                }

                List<DLinq.PolicyOutgoingAdvancedSchedule> schedule = (from e in DataModel.PolicyOutgoingAdvancedSchedules
                                                                       where e.PolicyId == policySchedule.PolicyId
                                                                       select e).ToList();

                foreach (DLinq.PolicyOutgoingAdvancedSchedule entry in schedule)
                {
                    OutgoingScheduleEntry tmpEntry = policySchedule.OutgoingScheduleList.FirstOrDefault(s => s.CoveragesScheduleId == entry.OutgoingAdvancedScheduleId);
                    if (tmpEntry == null)
                        DataModel.DeleteObject(entry);
                }

                foreach (OutgoingScheduleEntry _policyOutgoingShedule in policySchedule.OutgoingScheduleList)
                {
                    DLinq.MasterScheduleType _objSheduleType = new DLinq.MasterScheduleType();

                    gcs = (from e in DataModel.PolicyOutgoingAdvancedSchedules
                           where e.OutgoingAdvancedScheduleId == _policyOutgoingShedule.CoveragesScheduleId
                           select e).FirstOrDefault();

                    if (gcs == null)
                    {
                        gcs = new DLinq.PolicyOutgoingAdvancedSchedule
                        {
                            PolicyId = policySchedule.PolicyId,
                            IsPrimaryAgent = _policyOutgoingShedule.IsPrimaryAgent,
                            PayeeUserCredentialId = _policyOutgoingShedule.PayeeUserCredentialId,
                            PayeeName = _policyOutgoingShedule.PayeeName,
                            OutgoingAdvancedScheduleId = _policyOutgoingShedule.CoveragesScheduleId,
                            FromRange = _policyOutgoingShedule.FromRange,
                            ToRange = _policyOutgoingShedule.ToRange,
                            EffectiveToDate = _policyOutgoingShedule.EffectiveToDate,
                            EffectiveFromDate = _policyOutgoingShedule.EffectiveFromDate,
                            Rate = _policyOutgoingShedule.Rate,
                            ScheduleTypeId = policySchedule.ScheduleTypeId,
                            ModifiedOn = DateTime.Now
                        };

                        DLinq.UserDetail userDetail = DataModel.UserDetails.FirstOrDefault(s => s.UserCredentialId == _policyOutgoingShedule.PayeeUserCredentialId);
                        if (userDetail.AddPayeeOn == null)
                            userDetail.AddPayeeOn = DateTime.Now;

                        DataModel.AddToPolicyOutgoingAdvancedSchedules(gcs);
                    }
                    else
                    {
                        gcs.IsPrimaryAgent = _policyOutgoingShedule.IsPrimaryAgent;
                        gcs.PayeeUserCredentialId = _policyOutgoingShedule.PayeeUserCredentialId;
                        gcs.PayeeName = _policyOutgoingShedule.PayeeName;
                        gcs.FromRange = _policyOutgoingShedule.FromRange;
                        gcs.ToRange = _policyOutgoingShedule.ToRange;
                        gcs.EffectiveToDate = _policyOutgoingShedule.EffectiveToDate;
                        gcs.EffectiveFromDate = _policyOutgoingShedule.EffectiveFromDate;
                        gcs.Rate = _policyOutgoingShedule.Rate;
                        gcs.ScheduleTypeId = policySchedule.ScheduleTypeId;
                        gcs.ModifiedOn = DateTime.Now;
                    }
                }

                DataModel.SaveChanges();
            }
        }
        
        public static void DeleteOutgoingPolicySchedule(Guid PolicyId)
        {
            using (DLinq.CommissionDepartmentEntities DataModel = Entity.DataModel)
            {
                List<DLinq.PolicyIncomingAdvancedSchedule> schedule = DataModel.PolicyIncomingAdvancedSchedules.Where(s => s.PolicyId == PolicyId).ToList();
                foreach (DLinq.PolicyIncomingAdvancedSchedule entry in schedule)
                    DataModel.DeleteObject(entry);
                DataModel.SaveChanges();
            }
        }
        
        public static PolicyOutgoingSchedule GetPolicyOutgoingSchedule(Guid PolicyId)
        {
            using (DLinq.CommissionDepartmentEntities DataModel = Entity.DataModel)
            {
                PolicyOutgoingSchedule policyOutgoingSchedule = new PolicyOutgoingSchedule { PolicyId = PolicyId };
                foreach (DLinq.PolicyOutgoingAdvancedSchedule sch in DataModel.PolicyOutgoingAdvancedSchedules)
                {
                    if (sch.PolicyId == PolicyId)
                    {
                        OutgoingScheduleEntry scheduleEntry = new OutgoingScheduleEntry
                        {
                            IsPrimaryAgent = sch.IsPrimaryAgent,
                            PayeeUserCredentialId = sch.PayeeUserCredentialId.Value,
                            PayeeName = sch.PayeeName,
                            CoveragesScheduleId = sch.OutgoingAdvancedScheduleId,
                            FromRange = sch.FromRange,
                            ToRange = sch.ToRange,
                            EffectiveFromDate = sch.EffectiveFromDate,
                            EffectiveToDate = sch.EffectiveToDate,
                            Rate = sch.Rate,
                        };

                        if (policyOutgoingSchedule.OutgoingScheduleList == null)
                        {
                            policyOutgoingSchedule.OutgoingScheduleList = new List<OutgoingScheduleEntry>();

                            policyOutgoingSchedule.PolicyId = PolicyId;
                            policyOutgoingSchedule.ScheduleTypeId = sch.ScheduleTypeId.Value;
                            policyOutgoingSchedule.ScheduleTypeName = sch.MasterScheduleType.Name;
                        }

                        policyOutgoingSchedule.OutgoingScheduleList.Add(scheduleEntry);
                    }
                }
                return policyOutgoingSchedule;
            }
        }

        public static List<Guid> GetAllPoliciesForUser(Guid userCredId)
        {
            using (DLinq.CommissionDepartmentEntities DataModel = Entity.DataModel)
            {
                 List<DLinq.PolicyOutgoingAdvancedSchedule> schedule = (from e in DataModel.PolicyOutgoingAdvancedSchedules
                                                                        where e.PayeeUserCredentialId == userCredId
                                                                       select e).OrderBy(s => s.PolicyId).ToList();

                 List<Guid> policies = new List<Guid>();
                 foreach (DLinq.PolicyOutgoingAdvancedSchedule entry in schedule)
                 {
                     Guid policyId = policies.FirstOrDefault(s => s == entry.PolicyId);
                     if (policyId == null)
                         policies.Add(entry.PolicyId.Value);
                 }
                 return policies;
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
