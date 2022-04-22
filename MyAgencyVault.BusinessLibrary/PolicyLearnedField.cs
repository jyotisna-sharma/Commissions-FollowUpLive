using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MyAgencyVault.BusinessLibrary.Base;
using MyAgencyVault.BusinessLibrary.Masters;
using System.Runtime.Serialization;
using DLinq = DataAccessLayer.LinqtoEntity;
using System.Data.SqlClient;
using System.Data.EntityClient;
using System.Data;

namespace MyAgencyVault.BusinessLibrary
{
  [DataContract]
  public class PolicyLearnedFieldData
  {
    #region "data members aka - public properties"
    [DataMember]
    public Guid PolicyId { get; set; }
    [DataMember]
    public string Insured { get; set; }
    [DataMember]
    public string PolicyNumber { get; set; }
    [DataMember]
    public DateTime? Effective { get; set; }
    [DataMember]
    public DateTime? TrackFrom { get; set; }
    [DataMember]
    public string Renewal { get; set; }
    [DataMember]
    public Guid? CarrierId { get; set; }
    [DataMember]
    public Guid? CoverageId { get; set; }
    [DataMember]
    public string PAC { get; set; }
    [DataMember]
    public string PMC { get; set; }
    [DataMember]
    public decimal? ModalAvgPremium { get; set; }
    [DataMember]
    public int? PolicyModeId { get; set; }
    [DataMember]
    public string Enrolled { get; set; }
    [DataMember]
    public string Eligible { get; set; }
    [DataMember]
    public DateTime? AutoTerminationDate { get; set; }
    [DataMember]
    public string PayorSysId { get; set; }
    [DataMember]
    public string Link1 { get; set; }
    [DataMember]
    public decimal? Link2 { get; set; }
    [DataMember]
    public DateTime? LastModifiedOn { get; set; }
    [DataMember]
    public Guid? LastModifiedUserCredentialId { get; set; }
    [DataMember]
    public Guid ClientID { get; set; }
    [DataMember]
    public string ClientName { get; set; }
    [DataMember]
    public string CarrierNickName { get; set; }
    [DataMember]
    public string CoverageNickName { get; set; }
    [DataMember]
    public int? CompTypeId { get; set; }
    [DataMember]
    public string CompScheduleType { get; set; }
    [DataMember]
    public Guid? PayorId { get; set; }
    [DataMember]
    public DateTime? PreviousEffectiveDate { get; set; }
    [DataMember]
    public int? PreviousPolicyModeid { get; set; }
    [DataMember]
    public DateTime? PrevoiusTrackFromDate { get; set; }
    [DataMember]
    public int? Advance { get; set; }

    //Added new property
    //Seleted product type for policy manager
    [DataMember]
    public string ProductType { get; set; }

    #endregion
  }
  public class PolicyLearnedField : IEditable<PolicyLearnedFieldData>
  {
    #region IEditable<PolicyLearnedField> Members
    public void AddUpdate()
    {
    }

    public static void AddUpdateLearned(PolicyLearnedFieldData PolicyLernField, string strProductType)
    {
        using (DLinq.CommissionDepartmentEntities DataModel = Entity.DataModel)
        {
            // var _policyLearnedField = (from p in DataModel.PolicyLearnedFields where (p.PolicyLearnedFieldId == PolicyLernField.PolicyLearnedFieldId) select p).FirstOrDefault();
            var _policyLearnedField = (from p in DataModel.PolicyLearnedFields where (p.PolicyId == PolicyLernField.PolicyId) select p).FirstOrDefault();
            // List<PolicyLearnedField> _sPl = GetPolicyLearnedFieldsPolicyWise(this.PolicyId);
            if (_policyLearnedField == null)
            {
                _policyLearnedField = new DLinq.PolicyLearnedField
                {
                    // PolicyLearnedFieldId = (PolicyLernField.PolicyLearnedFieldId == null || PolicyLernField.PolicyLearnedFieldId==Guid.Empty) ? Guid.NewGuid() : PolicyLernField.PolicyLearnedFieldId,
                    Insured = PolicyLernField.Insured,
                    PolicyNumber = PolicyLernField.PolicyNumber,
                    Effective = PolicyLernField.Effective,
                    TrackFrom = PolicyLernField.TrackFrom,
                    Renewal = PolicyLernField.Renewal,
                    //Calculate PAC And PMC
                    PAC = "$" + Convert.ToString(PostUtill.CalculatePAC(PolicyLernField.PolicyId)),
                    PMC = "$" + Convert.ToString(PostUtill.CalculatePMC(PolicyLernField.PolicyId)),
                    ModalAvgPremium = PolicyLernField.ModalAvgPremium,
                    PolicyModeId = PolicyLernField.PolicyModeId,
                    Enrolled = PolicyLernField.Enrolled,
                    Eligible = PolicyLernField.Eligible,
                    AutoTerminationDate = PolicyLernField.AutoTerminationDate,
                    PayorSysID = PolicyLernField.PayorSysId,
                    Link1 = PolicyLernField.Link1,
                    SplitPercentage = PolicyLernField.Link2,
                    LastModifiedOn = PolicyLernField.LastModifiedOn,
                    LastModifiedUserCredentialid = PolicyLernField.LastModifiedUserCredentialId,
                    CompScheduleType = PolicyLernField.CompScheduleType,
                    //Save carier nick Name
                    CarrierNickName = PolicyLernField.CarrierNickName,
                    //Save Coverage nick Name
                    CoverageNickName = PolicyLernField.CoverageNickName,
                    ProductType = PolicyLernField.ProductType,

                };
                _policyLearnedField.PolicyReference.Value = (from s in DataModel.Policies where s.PolicyId == PolicyLernField.PolicyId select s).FirstOrDefault();
                _policyLearnedField.MasterPolicyModeReference.Value = (from s in DataModel.MasterPolicyModes where s.PolicyModeId == PolicyLernField.PolicyModeId select s).FirstOrDefault();
                _policyLearnedField.PayorReference.Value = (from s in DataModel.Payors where s.PayorId == PolicyLernField.PayorId select s).FirstOrDefault();
                _policyLearnedField.CarrierReference.Value = (from s in DataModel.Carriers where s.CarrierId == PolicyLernField.CarrierId select s).FirstOrDefault();
                _policyLearnedField.CoverageReference.Value = (from s in DataModel.Coverages where s.CoverageId == PolicyLernField.CoverageId select s).FirstOrDefault();
                _policyLearnedField.ClientReference.Value = (from s in DataModel.Clients where s.ClientId == PolicyLernField.ClientID select s).FirstOrDefault();

                _policyLearnedField.MasterIncomingPaymentTypeReference.Value = (from s in DataModel.MasterIncomingPaymentTypes where s.IncomingPaymentTypeId == PolicyLernField.CompTypeId select s).FirstOrDefault();
                DataModel.AddToPolicyLearnedFields(_policyLearnedField);

            }
            else
            {

                _policyLearnedField.PreviousEffectiveDate = _policyLearnedField.Effective;
                _policyLearnedField.PreviousPolicyModeId = _policyLearnedField.PolicyModeId;
                _policyLearnedField.Insured = PolicyLernField.Insured;
                _policyLearnedField.PolicyNumber = PolicyLernField.PolicyNumber;
                _policyLearnedField.Effective = PolicyLernField.Effective;
                _policyLearnedField.TrackFrom = PolicyLernField.TrackFrom;
                _policyLearnedField.Renewal = PolicyLernField.Renewal;
                _policyLearnedField.Effective = PolicyLernField.Effective;
                _policyLearnedField.PAC = "$" + Convert.ToString(PostUtill.CalculatePAC(PolicyLernField.PolicyId));
                _policyLearnedField.PMC = "$" + Convert.ToString(PostUtill.CalculatePMC(PolicyLernField.PolicyId));
                _policyLearnedField.ModalAvgPremium = PolicyLernField.ModalAvgPremium;
                _policyLearnedField.PolicyModeId = PolicyLernField.PolicyModeId;
                _policyLearnedField.Enrolled = PolicyLernField.Enrolled;
                _policyLearnedField.Eligible = PolicyLernField.Eligible;
                _policyLearnedField.AutoTerminationDate = PolicyLernField.AutoTerminationDate;
                _policyLearnedField.PayorSysID = PolicyLernField.PayorSysId;
                _policyLearnedField.Link1 = PolicyLernField.Link1;
                _policyLearnedField.SplitPercentage = PolicyLernField.Link2;
                _policyLearnedField.CompScheduleType = PolicyLernField.CompScheduleType;
                _policyLearnedField.LastModifiedOn = PolicyLernField.LastModifiedOn;
                _policyLearnedField.LastModifiedUserCredentialid = PolicyLernField.LastModifiedUserCredentialId;
                //update carrier nick name
                _policyLearnedField.CarrierNickName = PolicyLernField.CarrierNickName;
                //update coverage nick name          
                _policyLearnedField.CoverageNickName = PolicyLernField.CoverageNickName;

                _policyLearnedField.ProductType = PolicyLernField.ProductType;

                _policyLearnedField.PolicyReference.Value = (from s in DataModel.Policies where s.PolicyId == PolicyLernField.PolicyId select s).FirstOrDefault();
                _policyLearnedField.PayorReference.Value = (from s in DataModel.Payors where s.PayorId == PolicyLernField.PayorId select s).FirstOrDefault();
                _policyLearnedField.CarrierReference.Value = (from s in DataModel.Carriers where s.CarrierId == PolicyLernField.CarrierId select s).FirstOrDefault();
                _policyLearnedField.CoverageReference.Value = (from s in DataModel.Coverages where s.CoverageId == PolicyLernField.CoverageId select s).FirstOrDefault();
                _policyLearnedField.ClientReference.Value = (from s in DataModel.Clients where s.ClientId == PolicyLernField.ClientID select s).FirstOrDefault();
                _policyLearnedField.MasterIncomingPaymentTypeReference.Value = (from s in DataModel.MasterIncomingPaymentTypes where s.IncomingPaymentTypeId == PolicyLernField.CompTypeId select s).FirstOrDefault();

            }
            DataModel.SaveChanges();
        }
    }

    public static void UpdatePACPMC(Guid PolicyId)
    {
        ActionLogger.Logger.WriteLog("UpdatePACPMC :" + PolicyId, true);
        using (DLinq.CommissionDepartmentEntities DataModel = Entity.DataModel)
        {
            //System.Data.Objects.ObjectParameter objParam = new System.Data.Objects.ObjectParameter("pMC", typeof(decimal));
            try
            {
               /* DataModel.CommandTimeout = 600000000;
                var PMC = (from s in DataModel.CalculatePMC(PolicyId)
                           select new
                           {
                               s.PMC

                           }).Single();

                decimal pmc = Convert.ToDecimal(PMC.PMC);
                ActionLogger.Logger.WriteLog("UpdatePACPMC PMC found as :" + pmc, true);

                var PAC = (from s in DataModel.CalculatePAC(PolicyId)
                           select new
                           {
                               s.PAC

                           }).Single();
                decimal pac = Convert.ToDecimal(PAC.PAC);
                ActionLogger.Logger.WriteLog("UpdatePACPMC PAC found as :" + pmc, true);
                */
                var policy = (from p in DataModel.PolicyLearnedFields where p.PolicyId == PolicyId select p).FirstOrDefault();
                if (policy != null)
                {
                    ActionLogger.Logger.WriteLog("UpdatePACPMC policyID for update found as :" + policy.PolicyId, true);
                    policy.PAC = "$" + Convert.ToString(PostUtill.CalculatePAC(PolicyId));
                    policy.PMC = "$" + Convert.ToString(PostUtill.CalculatePMC(PolicyId));
                    DataModel.SaveChanges();
                    ActionLogger.Logger.WriteLog("UpdatePACPMC values saved :", true);
                }
                else
                {
                    ActionLogger.Logger.WriteLog("UpdatePACPMC policy object null, no save :", true);
                }
            }
            catch (Exception ex)
            {
                ActionLogger.Logger.WriteLog("UpdatePACPMC :" + ex.StackTrace.ToString(), true);
                ActionLogger.Logger.WriteLog("UpdatePACPMC :" + ex.InnerException.ToString(), true);

            }
        }
    }


    public static void UpdateLearnedFieldsMode(Guid _PolicyID,int PolicyMode)
    {
        using (DLinq.CommissionDepartmentEntities DataModel = Entity.DataModel)
        {
            var _poLearFiel = (from p in DataModel.PolicyLearnedFields where (p.PolicyId == _PolicyID) select p).FirstOrDefault();

            if (_poLearFiel != null)
            {
                //update smart mode
                _poLearFiel.PolicyModeId = PolicyMode;
                DataModel.SaveChanges();
                //Update Policy
                Policy.UpdateMode(_PolicyID, PolicyMode);
            }

        }      

    }

    public static PolicyLearnedFieldData GetPolicyLearnedFieldsPolicyWise(Guid _PolicyID)
    {
        PolicyLearnedFieldData _poLearFiel = null;
        using (DLinq.CommissionDepartmentEntities DataModel = Entity.DataModel)
        {
//            Acme added the following assignment as was missing in original code *******
            _poLearFiel = (from ss in DataModel.PolicyLearnedFields
                           where (ss.PolicyId == _PolicyID)
                           select new PolicyLearnedFieldData
                           {
                               // PolicyLearnedFieldId = ss.PolicyLearnedFieldId,
                               PolicyId = ss.Policy.PolicyId,
                               // ClientName=ss.ClientName,
                               Insured = ss.Insured,
                               PolicyNumber = ss.PolicyNumber,
                               Effective = ss.Effective.Value,
                               TrackFrom = ss.TrackFrom.Value,
                               PAC = ss.PAC,
                               PMC = ss.PMC,
                               ModalAvgPremium = ss.ModalAvgPremium,
                               Renewal = ss.Renewal,
                               CarrierId = ss.Carrier.CarrierId,
                               CoverageId = ss.Coverage.CoverageId,
                               // CompScheduleTypeId = ss.MasterScheduleType.ScheduleTypeId,
                               CompTypeId = ss.MasterIncomingPaymentType.IncomingPaymentTypeId,
                               //  CompSchedule=ss.CompSchedule,
                               //  CompType=ss.CompType,
                               //Add to get carrier nick name
                               CarrierNickName = ss.CarrierNickName,
                               //Add to get covrage nick name
                               CoverageNickName = ss.CoverageNickName,
                               PolicyModeId = ss.MasterPolicyMode.PolicyModeId,
                               Enrolled = ss.Enrolled,
                               Eligible = ss.Eligible,
                               AutoTerminationDate = ss.AutoTerminationDate,
                               PayorSysId = ss.PayorSysID,
                               Link1 = ss.Link1,
                               Link2 = ss.SplitPercentage,
                               LastModifiedOn = ss.LastModifiedOn,
                               LastModifiedUserCredentialId = ss.LastModifiedUserCredentialid,
                               ClientID = ss.Client.ClientId,
                               CompScheduleType = ss.CompScheduleType,
                               PayorId = ss.Payor.PayorId,
                               PreviousEffectiveDate = ss.PreviousEffectiveDate,
                               PreviousPolicyModeid = ss.PreviousPolicyModeId,
                           }
              ).ToList().FirstOrDefault();

            //*****************************ends here 
                if (_poLearFiel == null)
                    return _poLearFiel;

                if (_poLearFiel.ClientID != Guid.Empty)
                {
                    DLinq.Client client = DataModel.Clients.FirstOrDefault(s => s.ClientId == _poLearFiel.ClientID);
                    if (client != null)
                        _poLearFiel.ClientName = client.Name;
                }

                if (_poLearFiel.PayorId == null || _poLearFiel.PayorId == Guid.Empty || _poLearFiel.CarrierId == null || _poLearFiel.CarrierId == Guid.Empty) return _poLearFiel;
                {
                    _poLearFiel.CarrierNickName = Carrier.GetCarrierNickName(_poLearFiel.PayorId ?? Guid.Empty, _poLearFiel.CarrierId ?? Guid.Empty);
                }

                if (_poLearFiel.PayorId == null || _poLearFiel.PayorId == Guid.Empty || _poLearFiel.CarrierId == null || _poLearFiel.CarrierId == Guid.Empty || _poLearFiel.CoverageId == null || _poLearFiel.CoverageId == Guid.Empty)
                    return _poLearFiel;

                //Need to change yesterday
                //_poLearFiel.CoverageNickName = Coverage.GetCoverageNickName(_poLearFiel.PayorId ?? Guid.Empty, _poLearFiel.CarrierId ?? Guid.Empty, _poLearFiel.CoverageId ?? Guid.Empty);
                _poLearFiel.CoverageNickName = GetPolicyLearnedCoverageNickName((Guid)_PolicyID, (Guid)_poLearFiel.PayorId, (Guid)_poLearFiel.CarrierId, (Guid)_poLearFiel.CoverageId);
                _poLearFiel.PrevoiusTrackFromDate = _poLearFiel.TrackFrom;

           
        }
        return _poLearFiel;

    }

    public static DateTime? GetPolicyLearnedFieldsTrackDate(Guid _PolicyID)
    {
        DateTime? dtGetTime = null;

        PolicyLearnedFieldData _poLearFiel = null;
        using (DLinq.CommissionDepartmentEntities DataModel = Entity.DataModel)
        {
            _poLearFiel = (from ss in DataModel.PolicyLearnedFields
                           where (ss.PolicyId == _PolicyID)
                           select new PolicyLearnedFieldData
                           {
                               PolicyId = ss.Policy.PolicyId,
                               TrackFrom = ss.TrackFrom,
                           }).ToList().FirstOrDefault();


            if (_poLearFiel != null)
            {
                if (_poLearFiel.TrackFrom != null)
                {
                    dtGetTime = Convert.ToDateTime(_poLearFiel.TrackFrom);
                }
                else
                {
                    dtGetTime = null;
                }
            }

            else
            {
                dtGetTime = null;
            }

            return dtGetTime ;

        }

    }

    public static DateTime? GetPolicyLearnedFieldAutoTerminationDate(Guid _PolicyID)
    {
        DateTime? _poLearFielDateTime = null;
        PolicyLearnedFieldData _poLearFiel = null;
        string adoConnStr = DBConnection.GetConnectionString();
        SqlConnection con = null;

        try
        {
            using (con = new SqlConnection(adoConnStr))
            {
                using (SqlCommand cmd = new SqlCommand("Usp_GetAutoterminaationDate", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@PolicyID", _PolicyID);
                    con.Open();
                    SqlDataReader reader = cmd.ExecuteReader();
                    // Call Read before accessing data. 
                    while (reader.Read())
                    {
                        try
                        {
                            _poLearFiel = new PolicyLearnedFieldData();
                            if (!string.IsNullOrEmpty(Convert.ToString(reader["AutoTerminationDate"])))
                            {
                                _poLearFiel.AutoTerminationDate = Convert.ToDateTime(reader["AutoTerminationDate"]);
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
        finally
        {
            if (con != null)
            {
                con.Close();
            }

        }
        return _poLearFielDateTime;

    }
  
    //public static DateTime? GetPolicyLearnedFieldAutoTerminationDate(Guid _PolicyID)
    //{
    //    DateTime? _poLearFielDateTime = null;

    //    PolicyLearnedFieldData _poLearFiel = null;
    //    using (DLinq.CommissionDepartmentEntities DataModel = Entity.DataModel)
    //    {
    //        _poLearFiel = (from ss in DataModel.PolicyLearnedFields
    //                       where (ss.PolicyId == _PolicyID)
    //                       select new PolicyLearnedFieldData
    //                       {
    //                           PolicyId = ss.Policy.PolicyId,
    //                           AutoTerminationDate = ss.AutoTerminationDate,
    //                       }).ToList().FirstOrDefault();


    //        if (_poLearFiel != null)
    //        {
    //            if (_poLearFiel.AutoTerminationDate != null)
    //            {
    //                _poLearFielDateTime = Convert.ToDateTime(_poLearFiel.AutoTerminationDate);
    //            }
    //            else
    //            {
    //                _poLearFielDateTime = null;
    //            }
    //        }

    //        else
    //        {
    //            _poLearFielDateTime = null;
    //        }

    //        return _poLearFielDateTime;

    //    }
    //}

    public static string GetPolicyLearnedCoverageNickName(Guid policyID, Guid PayorID, Guid CarrierID, Guid CoverageID)
    {
        string strNickName = string.Empty;
       
        try
        {
            using (DLinq.CommissionDepartmentEntities DataModel = Entity.DataModel)
            {
               var strCovergeNickNaick = from p in DataModel.PolicyLearnedFields
                                         where (p.PolicyId == policyID && p.PayorId == PayorID && p.CarrierId == CarrierID && p.CoverageId == CoverageID)
                                      select p.CoverageNickName;

               foreach (var item in strCovergeNickNaick)
               {
                   if (!string.IsNullOrEmpty(item))
                   {
                       strNickName = Convert.ToString(item);
                   }
                   
               }
               
            }
            
        }
        catch
        {
        }
        return strNickName;
    }

    public void Delete()
    {
    }

    public static void Delete(PolicyLearnedFieldData policyLearnedFieldData)
    {
      using (DLinq.CommissionDepartmentEntities DataModel = Entity.DataModel)
      {
        var _policylearnedrecord = (from p in DataModel.PolicyLearnedFields where (p.PolicyId == policyLearnedFieldData.PolicyId) select p).FirstOrDefault();
        if (_policylearnedrecord == null) return;
        DataModel.DeleteObject(_policylearnedrecord);
        DataModel.SaveChanges();
      }
    }

    public static void DeleteByPolicy(Guid PolicyId)
    {
      using (DLinq.CommissionDepartmentEntities DataModel = Entity.DataModel)
      {
        var _policylearnedrecord = (from p in DataModel.PolicyLearnedFields where (p.PolicyId == PolicyId) select p).FirstOrDefault();
        if (_policylearnedrecord == null) return;
        DataModel.DeleteObject(_policylearnedrecord);
        DataModel.SaveChanges();
      }
    }
    
    public static bool IsValid(PolicyLearnedFieldData policy)
    {
      return true;
    }

    public static void AddUpdateHistoryLearned(Guid PolicyId)
    {
        List<PolicyPaymentEntriesPost> _PolicyPaymentEntriesPost = PolicyPaymentEntriesPost.GetPolicyPaymentEntryPolicyIDWise(PolicyId);
        if (_PolicyPaymentEntriesPost.Count != 0) return;
        PolicyLearnedFieldData PolicyLernField = GetPolicyLearnedFieldsPolicyWise(PolicyId);
        using (DLinq.CommissionDepartmentEntities DataModel = Entity.DataModel)
        {
            // var _policyLearnedField = (from p in DataModel.PolicyLearnedFields where (p.PolicyLearnedFieldId == PolicyLernField.PolicyLearnedFieldId) select p).FirstOrDefault();
            var _policyLearnedField = (from p in DataModel.PolicyLearnedFieldsHistories where (p.PolicyId == PolicyLernField.PolicyId) select p).FirstOrDefault();
            // List<PolicyLearnedField> _sPl = GetPolicyLearnedFieldsPolicyWise(this.PolicyId);
            if (_policyLearnedField == null)
            {
                _policyLearnedField = new DLinq.PolicyLearnedFieldsHistory
                {
                    // PolicyLearnedFieldId = (PolicyLernField.PolicyLearnedFieldId == null || PolicyLernField.PolicyLearnedFieldId==Guid.Empty) ? Guid.NewGuid() : PolicyLernField.PolicyLearnedFieldId,
                    Insured = PolicyLernField.Insured,
                    PolicyNumber = PolicyLernField.PolicyNumber,
                    Effective = PolicyLernField.Effective,
                    TrackFrom = PolicyLernField.TrackFrom,
                    Renewal = PolicyLernField.Renewal,
                    //CompSchedule = this.CompSchedule,
                    // CompType = this.CompType,
                    PAC = Convert.ToString(PolicyLernField.PAC),
                    PMC = Convert.ToString(PolicyLernField.PMC),
                    ModalAvgPremium = PolicyLernField.ModalAvgPremium,
                    PolicyModeId = PolicyLernField.PolicyModeId,//
                    Enrolled = PolicyLernField.Enrolled,
                    Eligible = PolicyLernField.Eligible,
                    AutoTerminationDate = PolicyLernField.AutoTerminationDate,
                    PayorSysID = PolicyLernField.PayorSysId,
                    Link1 = PolicyLernField.Link1,
                    SplitPercentage = PolicyLernField.Link2,
                    LastModifiedOn = PolicyLernField.LastModifiedOn,
                    LastModifiedUserCredentialid = PolicyLernField.LastModifiedUserCredentialId,//
                    CompScheduleType = PolicyLernField.CompScheduleType,
                    PreviousEffectiveDate = PolicyLernField.PreviousEffectiveDate,
                    PreviousPolicyModeId = PolicyLernField.PreviousPolicyModeid,
                    //03/03/2012
                    //Save carier nick Name
                    CarrierNickName = PolicyLernField.CarrierNickName,
                    //Save Coverage nick Name
                    CoverageNickName = PolicyLernField.CoverageNickName,
                    Advance = PolicyLernField.Advance,
                    ProductType = PolicyLernField.ProductType,

                };
                _policyLearnedField.PolicyReference.Value = (from s in DataModel.Policies where s.PolicyId == PolicyLernField.PolicyId select s).FirstOrDefault();
                _policyLearnedField.MasterPolicyModeReference.Value = (from s in DataModel.MasterPolicyModes where s.PolicyModeId == PolicyLernField.PolicyModeId select s).FirstOrDefault();
                _policyLearnedField.PayorReference.Value = (from s in DataModel.Payors where s.PayorId == PolicyLernField.PayorId select s).FirstOrDefault();
                _policyLearnedField.CarrierReference.Value = (from s in DataModel.Carriers where s.CarrierId == PolicyLernField.CarrierId select s).FirstOrDefault();
                _policyLearnedField.CoverageReference.Value = (from s in DataModel.Coverages where s.CoverageId == PolicyLernField.CoverageId select s).FirstOrDefault();
                _policyLearnedField.ClientReference.Value = (from s in DataModel.Clients where s.ClientId == PolicyLernField.ClientID select s).FirstOrDefault();

                _policyLearnedField.MasterIncomingPaymentTypeReference.Value = (from s in DataModel.MasterIncomingPaymentTypes where s.IncomingPaymentTypeId == PolicyLernField.CompTypeId select s).FirstOrDefault();
                // _policyLearnedField.MasterScheduleTypeReference.Value = (from s in DataModel.MasterScheduleTypes where s.ScheduleTypeId == PolicyLernField.CompScheduleTypeId select s).FirstOrDefault();
                DataModel.AddToPolicyLearnedFieldsHistories(_policyLearnedField);

            }
            else
            {
                //_policyLearnedField.PolicyLearnedFieldId = Guid.NewGuid();
                //_policyLearnedField.ClientName = this.ClientName;
                _policyLearnedField.Insured = PolicyLernField.Insured;
                _policyLearnedField.PolicyNumber = PolicyLernField.PolicyNumber;
                _policyLearnedField.Effective = PolicyLernField.Effective;
                _policyLearnedField.TrackFrom = PolicyLernField.TrackFrom;
                _policyLearnedField.Renewal = PolicyLernField.Renewal;
                // _policyLearnedField.CompSchedule = this.CompSchedule;
                // _policyLearnedField.CompType = this.CompType;
                _policyLearnedField.PAC = Convert.ToString(PolicyLernField.PAC);
                _policyLearnedField.PMC = Convert.ToString(PolicyLernField.PMC);
                _policyLearnedField.ModalAvgPremium = PolicyLernField.ModalAvgPremium;
                _policyLearnedField.PolicyModeId = PolicyLernField.PolicyModeId;
                _policyLearnedField.Enrolled = PolicyLernField.Enrolled;
                _policyLearnedField.Eligible = PolicyLernField.Eligible;
                _policyLearnedField.AutoTerminationDate = PolicyLernField.AutoTerminationDate;
                _policyLearnedField.PayorSysID = PolicyLernField.PayorSysId;
                _policyLearnedField.Link1 = PolicyLernField.Link1;
                _policyLearnedField.SplitPercentage = PolicyLernField.Link2;
                _policyLearnedField.CompScheduleType = PolicyLernField.CompScheduleType;
                _policyLearnedField.LastModifiedOn = PolicyLernField.LastModifiedOn;
                _policyLearnedField.LastModifiedUserCredentialid = PolicyLernField.LastModifiedUserCredentialId;
                _policyLearnedField.PolicyReference.Value = (from s in DataModel.Policies where s.PolicyId == PolicyLernField.PolicyId select s).FirstOrDefault();
                _policyLearnedField.PayorReference.Value = (from s in DataModel.Payors where s.PayorId == PolicyLernField.PayorId select s).FirstOrDefault();
                _policyLearnedField.CarrierReference.Value = (from s in DataModel.Carriers where s.CarrierId == PolicyLernField.CarrierId select s).FirstOrDefault();
                _policyLearnedField.CoverageReference.Value = (from s in DataModel.Coverages where s.CoverageId == PolicyLernField.CoverageId select s).FirstOrDefault();
                _policyLearnedField.ClientReference.Value = (from s in DataModel.Clients where s.ClientId == PolicyLernField.ClientID select s).FirstOrDefault();
                _policyLearnedField.MasterIncomingPaymentTypeReference.Value = (from s in DataModel.MasterIncomingPaymentTypes where s.IncomingPaymentTypeId == PolicyLernField.CompTypeId select s).FirstOrDefault();
                //  _policyLearnedField.MasterScheduleTypeReference.Value = (from s in DataModel.MasterScheduleTypes where s.ScheduleTypeId == PolicyLernField.CompScheduleTypeId select s).FirstOrDefault();
                _policyLearnedField.PreviousEffectiveDate = PolicyLernField.PreviousEffectiveDate;
                _policyLearnedField.PreviousPolicyModeId = PolicyLernField.PreviousPolicyModeid;
                //update carrier nick name
                _policyLearnedField.CarrierNickName = PolicyLernField.CarrierNickName;
                //update coverage nick name
                _policyLearnedField.CoverageNickName = PolicyLernField.CoverageNickName;
                _policyLearnedField.ProductType = PolicyLernField.ProductType;
                _policyLearnedField.Advance = PolicyLernField.Advance;
            }
            DataModel.SaveChanges();
            // LearnedToPolicyPost.AddUpdateLearnedToPolicy(GetPolicyLearnedFieldsPolicyWise(_policyLearnedField.PolicyId));
        }

    }

    public static void AddUpdateHistoryLearnedNotCheckPayment(Guid PolicyId)
    {
      //List<PolicyPaymentEntriesPost> _PolicyPaymentEntriesPost = PolicyPaymentEntriesPost.GetPolicyPaymentEntryPolicyIDWise(PolicyId);
      //if (_PolicyPaymentEntriesPost.Count != 0) return;
      PolicyLearnedFieldData PolicyLernField = GetPolicyLearnedFieldsPolicyWise(PolicyId);
      using (DLinq.CommissionDepartmentEntities DataModel = Entity.DataModel)
      {
        // var _policyLearnedField = (from p in DataModel.PolicyLearnedFields where (p.PolicyLearnedFieldId == PolicyLernField.PolicyLearnedFieldId) select p).FirstOrDefault();
        var _policyLearnedField = (from p in DataModel.PolicyLearnedFieldsHistories where (p.PolicyId == PolicyLernField.PolicyId) select p).FirstOrDefault();
        // List<PolicyLearnedField> _sPl = GetPolicyLearnedFieldsPolicyWise(this.PolicyId);
        if (_policyLearnedField == null)
        {
          _policyLearnedField = new DLinq.PolicyLearnedFieldsHistory
          {
            // PolicyLearnedFieldId = (PolicyLernField.PolicyLearnedFieldId == null || PolicyLernField.PolicyLearnedFieldId==Guid.Empty) ? Guid.NewGuid() : PolicyLernField.PolicyLearnedFieldId,
            Insured = PolicyLernField.Insured,
            PolicyNumber = PolicyLernField.PolicyNumber,
            Effective = PolicyLernField.Effective,
            TrackFrom = PolicyLernField.TrackFrom,
            Renewal = PolicyLernField.Renewal,
            //   CompSchedule = this.CompSchedule,
            // CompType = this.CompType,
            PAC = Convert.ToString(PolicyLernField.PAC),
            PMC = Convert.ToString(PolicyLernField.PMC),
            ModalAvgPremium = PolicyLernField.ModalAvgPremium,
            PolicyModeId = PolicyLernField.PolicyModeId,//
            Enrolled = PolicyLernField.Enrolled,
            Eligible = PolicyLernField.Eligible,
            AutoTerminationDate = PolicyLernField.AutoTerminationDate,
            PayorSysID = PolicyLernField.PayorSysId,
            Link1 = PolicyLernField.Link1,
            SplitPercentage = PolicyLernField.Link2,
            LastModifiedOn = PolicyLernField.LastModifiedOn,
            LastModifiedUserCredentialid = PolicyLernField.LastModifiedUserCredentialId,//
            CompScheduleType = PolicyLernField.CompScheduleType,
            PreviousEffectiveDate = PolicyLernField.PreviousEffectiveDate,
            PreviousPolicyModeId = PolicyLernField.PreviousPolicyModeid,
          };
          _policyLearnedField.PolicyReference.Value = (from s in DataModel.Policies where s.PolicyId == PolicyLernField.PolicyId select s).FirstOrDefault();
          _policyLearnedField.MasterPolicyModeReference.Value = (from s in DataModel.MasterPolicyModes where s.PolicyModeId == PolicyLernField.PolicyModeId select s).FirstOrDefault();
          _policyLearnedField.PayorReference.Value = (from s in DataModel.Payors where s.PayorId == PolicyLernField.PayorId select s).FirstOrDefault();
          _policyLearnedField.CarrierReference.Value = (from s in DataModel.Carriers where s.CarrierId == PolicyLernField.CarrierId select s).FirstOrDefault();
          _policyLearnedField.CoverageReference.Value = (from s in DataModel.Coverages where s.CoverageId == PolicyLernField.CoverageId select s).FirstOrDefault();
          _policyLearnedField.ClientReference.Value = (from s in DataModel.Clients where s.ClientId == PolicyLernField.ClientID select s).FirstOrDefault();

          _policyLearnedField.MasterIncomingPaymentTypeReference.Value = (from s in DataModel.MasterIncomingPaymentTypes where s.IncomingPaymentTypeId == PolicyLernField.CompTypeId select s).FirstOrDefault();
          // _policyLearnedField.MasterScheduleTypeReference.Value = (from s in DataModel.MasterScheduleTypes where s.ScheduleTypeId == PolicyLernField.CompScheduleTypeId select s).FirstOrDefault();
          DataModel.AddToPolicyLearnedFieldsHistories(_policyLearnedField);

        }
        else
        {
          // _policyLearnedField.PolicyLearnedFieldId = Guid.NewGuid();
          // _policyLearnedField.ClientName = this.ClientName;
          _policyLearnedField.Insured = PolicyLernField.Insured;
          _policyLearnedField.PolicyNumber = PolicyLernField.PolicyNumber;
          _policyLearnedField.Effective = PolicyLernField.Effective;
          _policyLearnedField.TrackFrom = PolicyLernField.TrackFrom;
          _policyLearnedField.Renewal = PolicyLernField.Renewal;
          // _policyLearnedField.CompSchedule = this.CompSchedule;
          // _policyLearnedField.CompType = this.CompType;
          _policyLearnedField.PAC = Convert.ToString(PolicyLernField.PAC);
          _policyLearnedField.PMC = Convert.ToString(PolicyLernField.PMC);
          _policyLearnedField.ModalAvgPremium = PolicyLernField.ModalAvgPremium;
          _policyLearnedField.PolicyModeId = PolicyLernField.PolicyModeId;
          _policyLearnedField.Enrolled = PolicyLernField.Enrolled;
          _policyLearnedField.Eligible = PolicyLernField.Eligible;
          _policyLearnedField.AutoTerminationDate = PolicyLernField.AutoTerminationDate;
          _policyLearnedField.PayorSysID = PolicyLernField.PayorSysId;
          _policyLearnedField.Link1 = PolicyLernField.Link1;
          _policyLearnedField.SplitPercentage = PolicyLernField.Link2;
          _policyLearnedField.CompScheduleType = PolicyLernField.CompScheduleType;
          _policyLearnedField.LastModifiedOn = PolicyLernField.LastModifiedOn;
          _policyLearnedField.LastModifiedUserCredentialid = PolicyLernField.LastModifiedUserCredentialId;
          _policyLearnedField.PolicyReference.Value = (from s in DataModel.Policies where s.PolicyId == PolicyLernField.PolicyId select s).FirstOrDefault();
          _policyLearnedField.PayorReference.Value = (from s in DataModel.Payors where s.PayorId == PolicyLernField.PayorId select s).FirstOrDefault();
          _policyLearnedField.CarrierReference.Value = (from s in DataModel.Carriers where s.CarrierId == PolicyLernField.CarrierId select s).FirstOrDefault();
          _policyLearnedField.CoverageReference.Value = (from s in DataModel.Coverages where s.CoverageId == PolicyLernField.CoverageId select s).FirstOrDefault();
          _policyLearnedField.ClientReference.Value = (from s in DataModel.Clients where s.ClientId == PolicyLernField.ClientID select s).FirstOrDefault();

          _policyLearnedField.MasterIncomingPaymentTypeReference.Value = (from s in DataModel.MasterIncomingPaymentTypes where s.IncomingPaymentTypeId == PolicyLernField.CompTypeId select s).FirstOrDefault();
          //  _policyLearnedField.MasterScheduleTypeReference.Value = (from s in DataModel.MasterScheduleTypes where s.ScheduleTypeId == PolicyLernField.CompScheduleTypeId select s).FirstOrDefault();
          _policyLearnedField.PreviousEffectiveDate = PolicyLernField.PreviousEffectiveDate;
          _policyLearnedField.PreviousPolicyModeId = PolicyLernField.PreviousPolicyModeid;
        }
        DataModel.SaveChanges();
       
      }
    }

    public static PolicyLearnedFieldData GetPolicyLearnedFieldsHistoryPolicyWise(Guid _PolicyID)
    {
      PolicyLearnedFieldData _poLearFiel = null;
      using (DLinq.CommissionDepartmentEntities DataModel = Entity.DataModel)
      {
        _poLearFiel = (from ss in DataModel.PolicyLearnedFieldsHistories
                       where (ss.PolicyId == _PolicyID)
                       select new PolicyLearnedFieldData
                       {
                         // PolicyLearnedFieldId = ss.PolicyLearnedFieldId,
                         PolicyId = ss.Policy.PolicyId,
                         // ClientName=ss.ClientName,
                         Insured = ss.Insured,
                         PolicyNumber = ss.PolicyNumber,
                         Effective = ss.Effective.Value,
                         TrackFrom = ss.TrackFrom.Value,
                         PAC =ss.PAC,
                         PMC =ss.PMC,
                         ModalAvgPremium = ss.ModalAvgPremium,
                         Renewal = ss.Renewal,
                         CarrierId = ss.Carrier.CarrierId,
                         CoverageId = ss.Coverage.CoverageId,
                         // CompScheduleTypeId = ss.MasterScheduleType.ScheduleTypeId,
                         CompTypeId = ss.MasterIncomingPaymentType.IncomingPaymentTypeId,
                         //  CompSchedule=ss.CompSchedule,
                         //  CompType=ss.CompType,
                           //Add to get carrier nick name
                         CarrierNickName = ss.CarrierNickName,
                           //Add to get covrage nick name
                         CoverageNickName = ss.CoverageNickName,
                         PolicyModeId = ss.MasterPolicyMode.PolicyModeId,
                         Enrolled = ss.Enrolled,
                         Eligible = ss.Eligible,
                         AutoTerminationDate = ss.AutoTerminationDate,
                         PayorSysId = ss.PayorSysID,
                         Link1 = ss.Link1,
                         Link2 = ss.SplitPercentage,
                         LastModifiedOn = ss.LastModifiedOn,
                         LastModifiedUserCredentialId = ss.LastModifiedUserCredentialid,
                         ClientID = ss.Client.ClientId,
                         CompScheduleType = ss.CompScheduleType,
                         PayorId = ss.Payor.PayorId,
                         PreviousEffectiveDate = ss.PreviousEffectiveDate,
                         PreviousPolicyModeid = ss.PreviousPolicyModeId,
                       }
               ).ToList().FirstOrDefault();
        if (_poLearFiel == null) return _poLearFiel;
        if (_poLearFiel.PayorId == null || _poLearFiel.PayorId == Guid.Empty || _poLearFiel.CarrierId == null || _poLearFiel.CarrierId == Guid.Empty) return _poLearFiel;
        {
          _poLearFiel.CarrierNickName = Carrier.GetCarrierNickName(_poLearFiel.PayorId ?? Guid.Empty, _poLearFiel.CarrierId ?? Guid.Empty);
        }

        if (_poLearFiel.PayorId == null || _poLearFiel.PayorId == Guid.Empty || _poLearFiel.CarrierId == null || _poLearFiel.CarrierId == Guid.Empty || _poLearFiel.CoverageId == null || _poLearFiel.CoverageId == Guid.Empty) return _poLearFiel;
        _poLearFiel.CoverageNickName = Coverage.GetCoverageNickName(_poLearFiel.PayorId ?? Guid.Empty, _poLearFiel.CarrierId ?? Guid.Empty, _poLearFiel.CoverageId ?? Guid.Empty);

      }
      _poLearFiel.PrevoiusTrackFromDate = _poLearFiel.TrackFrom;
      return _poLearFiel;

    }

    public static void DeleteLearnedHistory(Guid PolicyId)
    {
      using (DLinq.CommissionDepartmentEntities DataModel = Entity.DataModel)
      {
        var _policylearnedrecord = (from p in DataModel.PolicyLearnedFieldsHistories where (p.PolicyId == PolicyId) select p).FirstOrDefault();
        if (_policylearnedrecord == null) return;
        DataModel.DeleteObject(_policylearnedrecord);
        DataModel.SaveChanges();
      }
    }   

    #endregion

    PolicyLearnedFieldData IEditable<PolicyLearnedFieldData>.GetOfID()
    {
      return null;
    }
  }
}
