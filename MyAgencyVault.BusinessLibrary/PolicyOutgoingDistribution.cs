using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MyAgencyVault.BusinessLibrary.Base;
using MyAgencyVault.BusinessLibrary.Masters;
using System.Runtime.Serialization;
using DLinq = DataAccessLayer.LinqtoEntity;
using System.Data.EntityClient;
using System.Data.SqlClient;
using System.Data;

namespace MyAgencyVault.BusinessLibrary
{
    public class PolicyOutgoingDistribution
    {
        [DataMember]
        public Guid OutgoingPaymentId { get; set; }
        [DataMember]

        public Guid? PaymentEntryId { get; set; }
        [DataMember]

        public Guid? RecipientUserCredentialId { get; set; }
        [DataMember]

        public double? PaidAmount { get; set; } //it is TotalDueToPayee
        [DataMember]

        public DateTime? CreatedOn { get; set; }
       // [DataMember]

       // public Guid? ReferencedOutgoingScheduleId { get; set; }
       // [DataMember]

      //  public Guid? ReferencedOutgoingAdvancedScheduleId { get; set; }

        [DataMember]
        public bool? IsPaid { get; set; }

      
        [DataMember]
        public double? Premium { get; set; }  //It is %of Premium
        [DataMember]
        public decimal? OutGoingPerUnit { get; set; } //it is OutgoingPerunit
      
        [DataMember]
        public double? Payment { get; set; }//It is % of commission
       
        /// <summary>
        /// 
        /// </summary>
        /// <param name="PaymentEntryId"></param>
        /// <returns></returns>
        public static bool IsEntryMarkPaid(Guid PaymentEntryId)
        {
            List<PolicyOutgoingDistribution>_PolicyOutgoingDistributionLst
                = GetOutgoingPaymentByPoicyPaymentEntryId(PaymentEntryId);
            if (_PolicyOutgoingDistributionLst == null || _PolicyOutgoingDistributionLst.Count == 0) return false;
            return _PolicyOutgoingDistributionLst.Count(p => p.IsPaid == true) == 
                _PolicyOutgoingDistributionLst.Count ? true : false;
        }


        public static bool AddUpdateOutgoingPaymentEntry(PolicyOutgoingDistribution _PolicyOutgoingDistribution)
        {
            bool bValue = true;
            try
            {
                using (DLinq.CommissionDepartmentEntities DataModel = Entity.DataModel)
                {
                    Guid _OutGoingPayment = Guid.NewGuid();
                    DLinq.PolicyOutgoingPayment _ObjPolicyOutgoingPayment = (from m in DataModel.PolicyOutgoingPayments where m.OutgoingPaymentId == _PolicyOutgoingDistribution.OutgoingPaymentId select m).FirstOrDefault();

                    if (_ObjPolicyOutgoingPayment == null)
                    {
                        _ObjPolicyOutgoingPayment = new DLinq.PolicyOutgoingPayment
                        {
                            OutgoingPaymentId = _PolicyOutgoingDistribution.OutgoingPaymentId,
                            PaidAmount = _PolicyOutgoingDistribution.PaidAmount,
                            CreatedOn = _PolicyOutgoingDistribution.CreatedOn,
                            IsPaid = _PolicyOutgoingDistribution.IsPaid,
                            Premium = _PolicyOutgoingDistribution.Premium,
                            OutgoingPerUnit = _PolicyOutgoingDistribution.OutGoingPerUnit,
                            Payment = _PolicyOutgoingDistribution.Payment,

                        };


                        _ObjPolicyOutgoingPayment.RecipientUserCredentialId = _PolicyOutgoingDistribution.RecipientUserCredentialId;
                        _ObjPolicyOutgoingPayment.PolicyPaymentEntryReference.Value = (from f in DataModel.PolicyPaymentEntries where f.PaymentEntryId == _PolicyOutgoingDistribution.PaymentEntryId select f).FirstOrDefault();
                        // _ObjPolicyOutgoingPayment.ReferencedOutgoingScheduleId= _PolicyOutgoingDistribution.ReferencedOutgoingScheduleId ;
                        // _ObjPolicyOutgoingPayment.ReferencedOutgoingAdvancedScheduleId =  _PolicyOutgoingDistribution.ReferencedOutgoingAdvancedScheduleId ;

                        DataModel.AddToPolicyOutgoingPayments(_ObjPolicyOutgoingPayment);

                    }
                    else
                    {
                        _ObjPolicyOutgoingPayment.OutgoingPaymentId = _PolicyOutgoingDistribution.OutgoingPaymentId;
                        _ObjPolicyOutgoingPayment.PaidAmount = _PolicyOutgoingDistribution.PaidAmount;
                        _ObjPolicyOutgoingPayment.CreatedOn = _PolicyOutgoingDistribution.CreatedOn;
                        _ObjPolicyOutgoingPayment.IsPaid = _PolicyOutgoingDistribution.IsPaid;
                        _ObjPolicyOutgoingPayment.Premium = _PolicyOutgoingDistribution.Premium;
                        _ObjPolicyOutgoingPayment.OutgoingPerUnit = _PolicyOutgoingDistribution.OutGoingPerUnit;
                        _ObjPolicyOutgoingPayment.Payment = _PolicyOutgoingDistribution.Payment;
                        _ObjPolicyOutgoingPayment.RecipientUserCredentialId = _PolicyOutgoingDistribution.RecipientUserCredentialId;
                        _ObjPolicyOutgoingPayment.PolicyPaymentEntryReference.Value = (from f in DataModel.PolicyPaymentEntries where f.PaymentEntryId == _PolicyOutgoingDistribution.PaymentEntryId select f).FirstOrDefault();
                        // _ObjPolicyOutgoingPayment.ReferencedOutgoingScheduleId = _PolicyOutgoingDistribution.ReferencedOutgoingScheduleId;
                        // _ObjPolicyOutgoingPayment.ReferencedOutgoingAdvancedScheduleId = _PolicyOutgoingDistribution.ReferencedOutgoingAdvancedScheduleId;

                    }
                    DataModel.SaveChanges();
                    bValue = true;
                }
            }
            catch (Exception)
            {
                bValue = false;
            }

            return bValue;

        }

        public static PolicyOutgoingDistribution GetOutgoingPaymentById(Guid OutgoingPaymentId)
        {
            PolicyOutgoingDistribution _PolicyOutgoingDistribution = null;
            using (DLinq.CommissionDepartmentEntities DataModel = Entity.DataModel)
            {
                _PolicyOutgoingDistribution = (from f in DataModel.PolicyOutgoingPayments
                                               where (f.OutgoingPaymentId == OutgoingPaymentId)
                                               select new PolicyOutgoingDistribution
                                               {
                                                   OutgoingPaymentId = f.OutgoingPaymentId,
                                                   PaymentEntryId = f.PaymentEntryId,
                                                   RecipientUserCredentialId = f.RecipientUserCredentialId,
                                                   PaidAmount = f.PaidAmount,
                                                   CreatedOn = f.CreatedOn,
                                                  // ReferencedOutgoingScheduleId = f.ReferencedOutgoingScheduleId,
                                                  // ReferencedOutgoingAdvancedScheduleId = f.ReferencedOutgoingAdvancedScheduleId,
                                                   IsPaid = f.IsPaid,
                                                   //18-Apr-2011
                                                   Premium = f.Premium??0,
                                                   OutGoingPerUnit = f.OutgoingPerUnit??0,
                                                   Payment = f.Payment??0,
                                                   
                                                   
                                               }
                                                ).FirstOrDefault();
            }
            return _PolicyOutgoingDistribution;

        }

        public static List<PolicyOutgoingDistribution> GetOutgoingPaymentByPoicyPaymentEntryId(Guid EntryId,Guid recipientCredentials)
        {
          List<PolicyOutgoingDistribution> _PolicyOutgoingDistribution = null;
          using (DLinq.CommissionDepartmentEntities DataModel = Entity.DataModel)
          {
            _PolicyOutgoingDistribution = (from f in DataModel.PolicyOutgoingPayments
                                           where (f.PaymentEntryId == EntryId && f.RecipientUserCredentialId== recipientCredentials)
                                           select new PolicyOutgoingDistribution
                                           {
                                             OutgoingPaymentId = f.OutgoingPaymentId,
                                             PaymentEntryId = f.PaymentEntryId,
                                             RecipientUserCredentialId = f.RecipientUserCredentialId,
                                             PaidAmount = f.PaidAmount,
                                             CreatedOn = f.CreatedOn,
                                             IsPaid = f.IsPaid,
                                             Premium = f.Premium ?? 0,
                                             OutGoingPerUnit = f.OutgoingPerUnit ?? 0,
                                             Payment = f.Payment ?? 0,
                                           }
                                            ).ToList();
          }
          return _PolicyOutgoingDistribution;
        }

        //public static List<PolicyOutgoingDistribution> GetOutgoingPaymentByPoicyPaymentEntryId(Guid EntryId)
        //{
        //    List<PolicyOutgoingDistribution> _PolicyOutgoingDistribution = null;
        //    using (DLinq.CommissionDepartmentEntities DataModel = Entity.DataModel)
        //    {
        //        _PolicyOutgoingDistribution = (from f in DataModel.PolicyOutgoingPayments
        //                                       where (f.PaymentEntryId == EntryId)
        //                                       select new PolicyOutgoingDistribution
        //                                       {
        //                                           OutgoingPaymentId = f.OutgoingPaymentId,
        //                                           PaymentEntryId = f.PaymentEntryId,
        //                                           RecipientUserCredentialId = f.RecipientUserCredentialId,
        //                                           PaidAmount = f.PaidAmount,
        //                                           CreatedOn = f.CreatedOn,
        //                                           IsPaid = f.IsPaid,
        //                                           Premium = f.Premium??0,
        //                                           OutGoingPerUnit = f.OutgoingPerUnit??0,
        //                                           Payment = f.Payment??0,
        //                                       }
        //                                        ).ToList();
        //    }
        //    return _PolicyOutgoingDistribution;
        //}

        public static List<PolicyOutgoingDistribution> GetOutgoingPaymentByPoicyPaymentEntryId(Guid EntryId)
        {
            List<PolicyOutgoingDistribution> _PolicyOutgoingDistribution = new List<PolicyOutgoingDistribution>();
            DLinq.CommissionDepartmentEntities ctx = new DLinq.CommissionDepartmentEntities(); //create your entity object here
            EntityConnection ec = (EntityConnection)ctx.Connection;
            SqlConnection sc = (SqlConnection)ec.StoreConnection; //get the SQLConnection that your entity object would use
            string adoConnStr = sc.ConnectionString;
            using (SqlConnection con = new SqlConnection(adoConnStr))
            {
                using (SqlCommand cmd = new SqlCommand("Usp_GetOutgoingPayment", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@PaymentEntryId", EntryId);
                    con.Open();
                    SqlDataReader reader = cmd.ExecuteReader();
                    // Call Read before accessing data. 
                    while (reader.Read())
                    {
                        try
                        {
                            PolicyOutgoingDistribution objPolicyDetailsData = new PolicyOutgoingDistribution();


                            if (!string.IsNullOrEmpty(Convert.ToString(reader["OutgoingPaymentId"])))
                            {
                                objPolicyDetailsData.OutgoingPaymentId = reader["OutgoingPaymentId"] == null ? Guid.Empty : (Guid)reader["OutgoingPaymentId"];
                            }
                            if (!string.IsNullOrEmpty(Convert.ToString(reader["PaymentEntryId"])))
                            {
                                objPolicyDetailsData.PaymentEntryId = reader["PaymentEntryId"] == null ? Guid.Empty : (Guid)reader["PaymentEntryId"];
                            }
                            if (!string.IsNullOrEmpty(Convert.ToString(reader["RecipientUserCredentialId"])))
                            {
                                objPolicyDetailsData.RecipientUserCredentialId = reader["RecipientUserCredentialId"] == null ? Guid.Empty : (Guid)reader["RecipientUserCredentialId"];
                            }
                            if (!string.IsNullOrEmpty(Convert.ToString(reader["PaidAmount"])))
                            {
                                objPolicyDetailsData.PaidAmount = reader["PaidAmount"] == null ? 0 : Convert.ToDouble(reader["PaidAmount"]);
                            }
                            if (!string.IsNullOrEmpty(Convert.ToString(reader["CreatedOn"])))
                            {
                                objPolicyDetailsData.CreatedOn = Convert.ToDateTime(reader["CreatedOn"]);
                            }

                            if (!string.IsNullOrEmpty(Convert.ToString(reader["IsPaid"])))
                            {
                                objPolicyDetailsData.IsPaid = (bool)reader["IsPaid"];
                            }
                            if (!string.IsNullOrEmpty(Convert.ToString(reader["Premium"])))
                            {
                                objPolicyDetailsData.Premium = Convert.ToDouble(reader["Premium"]);
                            }

                            if (!string.IsNullOrEmpty(Convert.ToString(reader["OutGoingPerUnit"])))
                            {
                                objPolicyDetailsData.OutGoingPerUnit = Convert.ToDecimal(reader["OutGoingPerUnit"]);
                            }
                            if (!string.IsNullOrEmpty(Convert.ToString(reader["Payment"])))
                            {
                                objPolicyDetailsData.Payment = Convert.ToDouble(reader["Payment"]);
                            }

                            _PolicyOutgoingDistribution.Add(objPolicyDetailsData);
                        }
                        catch
                        {
                        }

                    }
                    // Call Close when done reading.
                    reader.Close();
                }
            }
            return _PolicyOutgoingDistribution;
        }

        public static void DeleteByPolicyIncomingPaymentId(Guid PaymentEntryId)
        {

            List<PolicyOutgoingDistribution> _PolicyOutgoingDistribution = GetOutgoingPaymentByPoicyPaymentEntryId(PaymentEntryId);
            foreach (PolicyOutgoingDistribution _po in _PolicyOutgoingDistribution)
            {
                DeleteById(_po.OutgoingPaymentId);
            }

        }

        public static void DeleteById(Guid OutgoingPaymentid)
        {
            using (DLinq.CommissionDepartmentEntities DataModel = Entity.DataModel)
            {
                var _OutgoingDes = (from f in DataModel.PolicyOutgoingPayments where f.OutgoingPaymentId == OutgoingPaymentid select f).FirstOrDefault();
                if (_OutgoingDes != null)
                {
                    DataModel.DeleteObject(_OutgoingDes);
                    DataModel.SaveChanges();
                }
            }

        }
    }
}
