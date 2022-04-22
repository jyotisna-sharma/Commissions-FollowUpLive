using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;
using MyAgencyVault.BusinessLibrary.Base;
using DLinq = DataAccessLayer.LinqtoEntity;
using System.Data.SqlClient;

namespace MyAgencyVault.BusinessLibrary
{
    [DataContract]
    public class PolicyToolIncommingShedule : PayorIncomingSchedule, IEditable<PolicyToolIncommingShedule>
    {
        public PolicyToolIncommingShedule()
        {
            this.CustomType = CustomMode.Graded;
        }
        #region "data members aka - public properties"
        [DataMember]
        public Guid IncomingScheduleId { get; set; }
        [DataMember]
        public Guid PolicyId { get; set; }
        // [DataMember]
        //public double? FirstYearPercentage { get; set; }
        //[DataMember]
        //public double? RenewalPercentage { get; set; }
        //[DataMember]
        //public double? SplitPercentage { get; set; }
        //[DataMember]
        //public int ScheduleTypeId { get; set; }
        #endregion
        #region IEditable<IncomingSchedule> Members

        public static PolicyToolIncommingShedule GettingPolicyIncomingSchedule(Guid policyId)
        {
            PolicyToolIncommingShedule policyData = new PolicyToolIncommingShedule();
            try
            {
                ActionLogger.Logger.WriteLog("GettingPolicyIncomingSchedule:Processing begins with policyId" + policyId, true);
                using (SqlConnection con = new SqlConnection(DBConnection.GetConnectionString()))
                {
                    using (SqlCommand cmd = new SqlCommand("Usp_gettingincomingschedule", con))
                    {
                        cmd.CommandType = System.Data.CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@policyId", policyId);
                        con.Open();
                        SqlDataReader reader = cmd.ExecuteReader();
                        while (reader.Read())
                        {
                            policyData.IncomingScheduleID = (Guid)reader["IncomingScheduleId"];
                            policyData.PolicyId = (Guid)reader["PolicyId"];
                            policyData.FirstYearPercentage = reader.IsDBNull("FirstYearPercentage") ? 0.00 : Convert.ToDouble(reader["FirstYearPercentage"]);
                            policyData.RenewalPercentage = reader.IsDBNull("RenewalPercentage") ? 0.00 : Convert.ToDouble(reader["RenewalPercentage"]);
                            policyData.ScheduleTypeId = reader.IsDBNull("ScheduleTypeId") ? 1 : (int)(reader["ScheduleTypeId"]);
                            string mod = Convert.ToString(reader["Mode"]);
                            int scheduleMode = 0;
                            int.TryParse(mod, out scheduleMode);
                            policyData.Mode = (Mode)scheduleMode;

                            string strType = Convert.ToString(reader["CustomType"]);
                            int intType = 0;
                            int.TryParse(strType, out intType);
                            intType = (intType == 0) ? 1 : intType;
                            policyData.CustomType = (CustomMode)intType;


                        }
                    }
                    con.Close();
                }
                if (policyData.Mode == Mode.Custom)
                {
                    if (policyData.CustomType == CustomMode.Graded)
                    {
                        policyData.GradedSchedule = GradedScheduleList(policyData.IncomingScheduleID, policyData.PolicyId);
                    }
                    else
                    {
                        policyData.NonGradedSchedule = NonGradedScheduleList(policyData.IncomingScheduleID, policyData.PolicyId);
                    }
                }
            }
            catch (Exception ex)
            {
                ActionLogger.Logger.WriteLog("GettingPolicyIncomingSchedule:Exception occurs while processing with policyId" + policyId + " " + ex.Message, true);
                throw ex;
            }
            return policyData;
        }
        public void AddUpdate()
        {
            try
            {
                if (this != null)
                {
                    ActionLogger.Logger.WriteLog(DateTime.Now.ToString() + " AddUpdate Incoming schedule request: " + this.ToStringDump(), true);
                }
                using (DLinq.CommissionDepartmentEntities DataModel = Entity.DataModel)
                {

                    DLinq.PolicyIncomingSchedule PolicyIncomingScheduleDetails = (from e in DataModel.PolicyIncomingSchedules
                                                                                  where e.PolicyId == this.PolicyId
                                                                                  select e).FirstOrDefault();

                    if (PolicyIncomingScheduleDetails == null)
                    {
                        PolicyIncomingScheduleDetails = new DLinq.PolicyIncomingSchedule
                        {
                            FirstYearPercentage = this.FirstYearPercentage,
                            RenewalPercentage = this.RenewalPercentage,
                            IncomingScheduleId = this.IncomingScheduleId
                        };
                        PolicyIncomingScheduleDetails.PolicyReference.Value = (from inc in DataModel.Policies where inc.PolicyId == this.PolicyId select inc).FirstOrDefault();
                        PolicyIncomingScheduleDetails.MasterBasicIncomingScheduleReference.Value = (from inc in DataModel.MasterBasicIncomingSchedules where inc.ScheduleId == this.ScheduleTypeId select inc).FirstOrDefault();
                        DataModel.AddToPolicyIncomingSchedules(PolicyIncomingScheduleDetails);


                    }
                    else
                    {
                        PolicyIncomingScheduleDetails.FirstYearPercentage = this.FirstYearPercentage;
                        PolicyIncomingScheduleDetails.RenewalPercentage = this.RenewalPercentage;
                        PolicyIncomingScheduleDetails.MasterBasicIncomingScheduleReference.Value = (from inc in DataModel.MasterBasicIncomingSchedules where inc.ScheduleId == this.ScheduleTypeId select inc).FirstOrDefault();
                    }

                    DataModel.SaveChanges();
                }
            }
            catch (Exception ex)
            {
                ActionLogger.Logger.WriteLog(DateTime.Now.ToString() + " AddUpdate Incoming schedule error: " + ex.Message, true);
            }
        }

        public void Delete()
        {
            using (DLinq.CommissionDepartmentEntities DataModel = Entity.DataModel)
            {
                DLinq.PolicyIncomingSchedule _PInc = (from n in DataModel.PolicyIncomingSchedules
                                                      where (n.IncomingScheduleId == this.IncomingScheduleId)
                                                      select n).FirstOrDefault();
                DataModel.DeleteObject(_PInc);
                DataModel.SaveChanges();
            }

        }
        public static void DeleteSchedule(Guid PolicyId)
        {
            try
            {
                PolicyToolIncommingShedule _PolicyIncomingSchedule = GetPolicyToolIncommingSheduleListPolicyWise(PolicyId);
                if (_PolicyIncomingSchedule == null) return;
                using (DLinq.CommissionDepartmentEntities DataModel = Entity.DataModel)
                {
                    DLinq.PolicyIncomingSchedule _PInc = (from n in DataModel.PolicyIncomingSchedules
                                                          where (n.IncomingScheduleId == _PolicyIncomingSchedule.IncomingScheduleId)
                                                          select n).FirstOrDefault();
                    if (_PInc == null) return;
                    DataModel.DeleteObject(_PInc);
                    DataModel.SaveChanges();
                }
            }
            catch (Exception ex)
            {
                ActionLogger.Logger.WriteLog("Exception in delete incomng schedule : " + ex.Message, true);
            }
        }

        public PolicyToolIncommingShedule GetOfID()
        {
            throw new NotImplementedException();
        }

        public static List<PolicyToolIncommingShedule> GetPolicyToolIncommingSheduleList(Guid? PolicyId)
        {
            using (DLinq.CommissionDepartmentEntities DataModel = Entity.DataModel)
            {
                List<PolicyToolIncommingShedule> _PolicyToolIncommingShedule;
                _PolicyToolIncommingShedule = (from hd in DataModel.PolicyIncomingSchedules
                                               where hd.PolicyId == PolicyId
                                               select new PolicyToolIncommingShedule
                                               {
                                                   FirstYearPercentage = hd.FirstYearPercentage,
                                                   RenewalPercentage = hd.RenewalPercentage,
                                                   PolicyId = hd.Policy.PolicyId,
                                                   ScheduleTypeId = hd.MasterBasicIncomingSchedule.ScheduleId,
                                                   IncomingScheduleId = hd.IncomingScheduleId,
                                               }).ToList();
                return _PolicyToolIncommingShedule;
            }
        }
        public static List<PolicyToolIncommingShedule> GetPolicyToolIncommingSheduleList()
        {
            using (DLinq.CommissionDepartmentEntities DataModel = Entity.DataModel)
            {
                List<PolicyToolIncommingShedule> _PolicyToolIncommingShedule;
                _PolicyToolIncommingShedule = (from hd in DataModel.PolicyIncomingSchedules
                                               select new PolicyToolIncommingShedule
                                               {
                                                   FirstYearPercentage = hd.FirstYearPercentage,
                                                   RenewalPercentage = hd.RenewalPercentage,
                                                   PolicyId = hd.Policy.PolicyId,
                                                   ScheduleTypeId = hd.MasterBasicIncomingSchedule.ScheduleId,
                                                   IncomingScheduleId = hd.IncomingScheduleId,
                                               }).ToList();
                return _PolicyToolIncommingShedule;
            }
        }

        public static PolicyToolIncommingShedule GetPolicyToolIncommingSheduleListPolicyWise(Guid PolicyId)
        {
            PolicyToolIncommingShedule PolicyToolIncommingSheduleLst = GetPolicyToolIncommingSheduleList(PolicyId).FirstOrDefault();
            return PolicyToolIncommingSheduleLst;
        }
        public bool IsValid()
        {
            throw new NotImplementedException();
        }
        #endregion

    }

}
