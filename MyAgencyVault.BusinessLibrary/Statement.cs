using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MyAgencyVault.BusinessLibrary.Base;
using System.Runtime.Serialization;
using DLinq = DataAccessLayer.LinqtoEntity;
using DataAccessLayer.LinqtoEntity;
using MyAgencyVault.BusinessLibrary.Masters;
using System.Transactions;
namespace MyAgencyVault.BusinessLibrary
{
    [DataContract]
    public class ModifiableStatementData
    {
        [DataMember]
        public Guid StatementId{ get; set; }
        [DataMember]
        public Guid? PayorId { get; set; }
        [DataMember]
        public decimal EnteredAmount { get; set; }
        [DataMember]
        public double CompletePercentage { get; set; }
        [DataMember]
        public int Entries { get; set; }
        [DataMember]
        public int StatusId { get; set; }
        [DataMember]
        public DateTime LastModified { get; set; }
    }

    /// <summary>
    /// 
    /// </summary>
    [DataContract]
    public class Statement
    {
        #region "Data Members aka - public properties "
        [DataMember]
        public Guid StatementID { get; set; }
        [DataMember]
        public Guid BatchId { get; set; }
        [DataMember]
        public Guid? PayorId { get; set; }
        [DataMember]
        public int StatementNumber { get; set; }
        [DataMember]
        public DateTime? StatementDate { get; set; }
        [DataMember]
        public decimal? CheckAmount { get; set; }
        [DataMember]
        public decimal? BalanceForOrAdjustment { get; set; }
        [DataMember]
        public decimal? NetAmount { get; set; }
        [DataMember]
        public decimal EnteredAmount { get; set; }
        [DataMember]
        public double CompletePercentage { get; set; }
        [DataMember]
        public int Entries { get; set; }
        [DataMember]
        public string Payor { get; set; }
        [DataMember]
        public int? StatusId { get; set; }
        [DataMember]
        public DateTime CreatedDate { get; set; }
        [DataMember]
        public DateTime LastModified { get; set; }
        [DataMember]
        public Guid CreatedBy { get; set; }
        [DataMember]
        public string CreatedByDEU { get; set; }
        [DataMember]
        public string CarrierName { get; set; }
        [DataMember]
        public string ProductName { get; set; }
        [DataMember]
        public DateTime? EntryDate { get; set; }
       
        [DataMember]
        public Guid? TemplateID { get; set; }

        [DataMember]
        public string FromPage { get; set; }

        [DataMember]
        public string ToPage { get; set; }

        #endregion
        #region  "functionss"
        /// <summary>
        /// hold all the commission entries made through this statement.
        /// </summary>
        [DataMember]
        public List<ExposedDEU> DeuEntries
        {
            get;
            set;
        }

        public static List<ExposedDEU> GetDeuEntriesforStatement(Guid StatementID)
        {
                using (DLinq.CommissionDepartmentEntities DataModel = Entity.DataModel)
                {
                  
                    #region comment for Sp 
                    List<DLinq.EntriesByDEU> deuEntries = DataModel.EntriesByDEUs.Where(s => s.StatementID == StatementID).ToList();
                    List<ExposedDEU> exposedDeuEntries = new List<ExposedDEU>();
                    DEU objdeu = new DEU();
                    foreach (DLinq.EntriesByDEU deu in deuEntries)
                    {
                        ExposedDEU exposedDeu = objdeu.CreateExposedDEU(deu);
                        exposedDeuEntries.Add(exposedDeu);
                    }
                    return exposedDeuEntries;
                    #endregion
                    //List<ExposedDEU> exposedDeuEntries = new List<ExposedDEU>();
                    //var result = DataModel.GetDeuEntriesforStatement(StatementID);

                    //exposedDeuEntries = (from p in result
                    //                     select new ExposedDEU
                    //                     {
                    //                         DEUENtryID = p.DEUEntryID,
                    //                         ClientName = p.ClientValue,
                    //                         UnlinkClientName = p.ClientValue,
                    //                         Insured = p.Insured,
                    //                         PaymentRecived = p.PaymentReceived,
                    //                         PolicyNumber = p.PolicyNumber,
                    //                         Units = p.NumberOfUnits,
                    //                         InvoiceDate = p.InvoiceDate,
                    //                         CommissionTotal = p.CommissionTotal,
                    //                         Fee = p.Fee,
                    //                         SplitPercentage = p.SplitPer,
                    //                         EntryDate = p.EntryDate,
                    //                         CarrierNickName = p.CarrierNickName,
                    //                         CoverageNickName = p.CoverageNickName,
                    //                         CarrierName = p.CarrierName,
                    //                         ProductName = p.ProductName,
                    //                         PostStatus = (PostCompleteStatusEnum)(p.PostCompleteStatus ?? 0),
                    //                         CommissionPercentage = p.CommissionPercentage,
                    //                         CreatedDate = p.CreatedOn,
                    //                     }).ToList();

                    //return exposedDeuEntries;

                   
                }

        }
        /// <summary>
        /// Add/Update the statement.
        /// </summary>
        public int AddUpdate()
        {
            using(DLinq.CommissionDepartmentEntities DataModel = Entity.DataModel)
            {
                var _statement = (from pn in DataModel.Statements where pn.StatementId == this.StatementID select pn).FirstOrDefault();
                if (_statement == null)
                {
                    DLinq.Batch batch = DataModel.Batches.FirstOrDefault(s => s.BatchId == this.BatchId);
                    _statement = new DLinq.Statement
                    {
                        BatchId = this.BatchId,
                        Batch = batch,
                        PayorId = this.PayorId,
                        CheckAmount = this.CheckAmount,
                        CreatedBy = this.CreatedBy,
                        CreatedOn = DateTime.Now,
                        LastModified = DateTime.Now,
                        Entries = this.Entries,
                        EnteredAmount = this.EnteredAmount,
                        StatementId = this.StatementID,
                        StatementDate = this.StatementDate,
                        StatementStatusId = this.StatusId,
                        BalAdj = this.BalanceForOrAdjustment,
                        TemplateID = this.TemplateID,
                        FromPage = this.FromPage,
                        ToPage = this.ToPage,
                    };
                    DataModel.AddToStatements(_statement);

                    if (batch.Statements.Count == 1)
                        batch.AssignedUserCredentialId = this.CreatedBy;

                }
                else
                {
                    _statement.BatchId = this.BatchId;
                    _statement.CheckAmount = this.CheckAmount;
                    _statement.EnteredAmount = this.EnteredAmount;//For Commission board
                    _statement.Entries = this.Entries;
                    _statement.CreatedBy = this.CreatedBy;
                    _statement.PayorId = this.PayorId;
                    _statement.LastModified = DateTime.Now;
                    _statement.StatementDate = this.StatementDate;
                    _statement.StatementStatusId = this.StatusId;
                    _statement.BalAdj = this.BalanceForOrAdjustment;
                    _statement.StatementStatusId = this.StatusId;
                    _statement.CreatedOn = System.DateTime.Now;
                    _statement.TemplateID = this.TemplateID;
                    _statement.FromPage = this.FromPage;
                    _statement.ToPage = this.ToPage;
                }

                DataModel.SaveChanges();
                return _statement.StatementNumber;
            }
        }

        public int AddStatementNumber(Statement objStatement)
        {
            int intStatementNumber = 0;
            try
            {
                using (DLinq.CommissionDepartmentEntities DataModel = Entity.DataModel)
                {
                    var _statement = (from pn in DataModel.Statements where pn.StatementId == objStatement.StatementID select pn).FirstOrDefault();
                    if (_statement == null)
                    {
                        DLinq.Batch batch = DataModel.Batches.FirstOrDefault(s => s.BatchId == objStatement.BatchId);
                        _statement = new DLinq.Statement
                        {
                            StatementId = objStatement.StatementID,
                            BatchId = objStatement.BatchId,
                            Batch = batch,
                            PayorId = objStatement.PayorId,
                            CheckAmount = objStatement.CheckAmount,
                            CreatedBy = objStatement.CreatedBy,
                            CreatedOn = DateTime.Now,
                            LastModified = DateTime.Now,
                            Entries = objStatement.Entries,
                            EnteredAmount = objStatement.EnteredAmount,                           
                            StatementDate = objStatement.StatementDate,
                            StatementStatusId = objStatement.StatusId,
                            BalAdj = objStatement.BalanceForOrAdjustment,
                            TemplateID = objStatement.TemplateID,
                            

                        };
                        DataModel.AddToStatements(_statement);

                        if (batch.Statements.Count == 1)
                            batch.AssignedUserCredentialId = this.CreatedBy;
                    }
                    else
                    {
                        _statement.BatchId = objStatement.BatchId;
                        _statement.CheckAmount = objStatement.CheckAmount;
                        _statement.EnteredAmount = objStatement.EnteredAmount;//For Commission board
                        _statement.Entries = objStatement.Entries;
                        _statement.CreatedBy = objStatement.CreatedBy;
                        _statement.PayorId = objStatement.PayorId;
                        _statement.LastModified = DateTime.Now;
                        _statement.StatementDate = objStatement.StatementDate;                       
                        _statement.BalAdj = objStatement.BalanceForOrAdjustment;
                        _statement.StatementStatusId = objStatement.StatusId;
                        _statement.CreatedOn = System.DateTime.Now;
                        _statement.TemplateID = objStatement.TemplateID;
                    }

                    DataModel.SaveChanges();
                    intStatementNumber = _statement.StatementNumber;
                }
            }
            catch
            {
            }
            return intStatementNumber;
        }

        public static ModifiableStatementData UpdateCheckAmount(Guid statementId,decimal dcCheckAmount, decimal dcNetAmount, decimal dcAdjustment)
        {
            ModifiableStatementData statementData = new ModifiableStatementData();
            try
            {
                using (DLinq.CommissionDepartmentEntities DataModel = Entity.DataModel)
                {
                    DLinq.Statement statement = DataModel.Statements.FirstOrDefault(s => s.StatementId == statementId);

                    if (statement != null)
                    {
                        statement.CheckAmount = dcCheckAmount;
                        statement.BalAdj = dcAdjustment;
                        DataModel.SaveChanges();

                        statementData.StatementId = statement.StatementId;
                        //statementData.CompletePercentage = (double)((statement.EnteredAmount ?? 0) / ((netAmount + statement.BalAdj ?? 0) == 0 ? int.MaxValue : (netAmount + statement.BalAdj ?? 0))) * 100;
                        if (dcNetAmount == 0)
                            statementData.CompletePercentage = 0;
                        else
                            statementData.CompletePercentage = ((double)(dcNetAmount - statement.EnteredAmount) * 100) / ((double)(dcNetAmount));

                        statementData.LastModified = statement.LastModified.Value;
                        statementData.Entries = statement.Entries.Value;
                        statementData.StatusId = statement.MasterStatementStatu.StatementStatusId;
                        statementData.EnteredAmount = statement.EnteredAmount.Value;
                    }
                  
                }
            }
            catch
            {
            }

            return statementData;

        }
        public void UpdateImporttoolStatementData(int intStatementNumber, decimal? dbCheckAmount, decimal? dbBalAdj,decimal? enteredAmount,int? intEntries,DateTime ?dtSatementDate)
        {
            using (DLinq.CommissionDepartmentEntities DataModel = Entity.DataModel)
            {
                try
                {
                    DLinq.Statement statement = DataModel.Statements.FirstOrDefault(s => s.StatementNumber == intStatementNumber);

                    if (statement != null)
                    {
                        statement.CheckAmount = dbCheckAmount;
                        statement.BalAdj = dbBalAdj;
                        //statement.EnteredAmount = enteredAmount;                  
                        statement.StatementDate = dtSatementDate;
                        DataModel.SaveChanges();
                    }
                }
                catch
                {
                }
            }
        }

        public void UpdateCheckAmount(int intStatementNumber, decimal? dbCheckAmount, decimal? dbBalAdj)
        {
            try
            {
                using (DLinq.CommissionDepartmentEntities DataModel = Entity.DataModel)
                {
                    DLinq.Statement statement = DataModel.Statements.FirstOrDefault(s => s.StatementNumber == intStatementNumber);

                    if (statement != null)
                    {
                        statement.CheckAmount = dbCheckAmount;
                        statement.BalAdj = dbBalAdj;
                        DataModel.SaveChanges();
                    }
                }
            }
            catch
            {
            }
        }

        public static ModifiableStatementData CreateModifiableStatementData(DLinq.Statement statement)
        {
            ModifiableStatementData statementData = new ModifiableStatementData();

            if (statement != null)
            {
                statementData.StatementId = statement.StatementId;
                statementData.CompletePercentage = (double)((statement.EnteredAmount ?? 0) / ((statement.CheckAmount ?? 0 + statement.BalAdj ?? 0) == 0 ? int.MaxValue : (statement.CheckAmount ?? 0 + statement.BalAdj ?? 0))) * 100;
                statementData.LastModified = statement.LastModified.Value;
                statementData.Entries = statement.Entries.Value;
                statementData.StatusId = statement.MasterStatementStatu.StatementStatusId;
                statementData.EnteredAmount = statement.EnteredAmount.Value;
                statementData.PayorId = statement.PayorId;
            }
            return statementData;
        }       /// <summary>
        /// just insert the statument . call AddUpdate()
        /// with all required properties filled.
        /// </summary>
        public void PostStatement()
        {
            this.AddUpdate();
        }

        /// <summary>
        /// update the status of the statement to be closed.
        /// </summary>    

        public bool CloseStatement()
        {
            bool StatementEntriesSuccessfull = false;
            try
            {
                using (DLinq.CommissionDepartmentEntities DataModel = Entity.DataModel)
                {
                    DLinq.Statement currentStatement = (from p in DataModel.Statements where p.StatementId == this.StatementID select p).FirstOrDefault();
                    if (currentStatement != null)
                    {
                        if (Math.Abs(Convert.ToDecimal(currentStatement.EnteredAmount) - Convert.ToDecimal(this.NetAmount)) <= 1)
                            StatementEntriesSuccessfull = true;

                        if (StatementEntriesSuccessfull)
                        {
                            currentStatement.StatementStatusId = 2;
                            currentStatement.MasterStatementStatu = DataModel.MasterStatementStatus.FirstOrDefault(s => s.StatementStatusId == 2);
                            DataModel.SaveChanges();
                        }
                    }
                }
            }
            catch
            {
            }
            return StatementEntriesSuccessfull;
        }

        public bool CloseStatementFromDeu(Statement objStatement)
        {
            using (DLinq.CommissionDepartmentEntities DataModel = Entity.DataModel)
            {
                bool StatementEntriesSuccessfull = false;

                if (objStatement != null)
                {
                    //List<DLinq.EntriesByDEU> DeuEntries = (from p in DataModel.EntriesByDEUs where p.StatementID == this.StatementID select p).ToList();

                    //decimal TotalMoney = 0;
                    //foreach (DLinq.EntriesByDEU entry in DeuEntries)
                    //{
                    //    TotalMoney += entry.CommissionPaid.Value + entry.CommissionTotal.Value;
                    //}


                    //Needs to be a +/- of $1.00 for right now to allow a statement to close "Eric requirement"
                    //if (currentStatement.EnteredAmount == this.NetAmount)

                    if (Math.Abs(Convert.ToDecimal(objStatement.EnteredAmount) - Convert.ToDecimal(objStatement.NetAmount)) <= 1)
                        StatementEntriesSuccessfull = true;

                    if (StatementEntriesSuccessfull)
                    {
                        DLinq.Statement currentStatement = (from p in DataModel.Statements where p.StatementId == this.StatementID select p).FirstOrDefault();
                        if (currentStatement != null)
                        {
                            currentStatement.StatementStatusId = 2;
                            currentStatement.MasterStatementStatu = DataModel.MasterStatementStatus.FirstOrDefault(s => s.StatementStatusId == 2);
                            currentStatement.EnteredAmount = objStatement.EnteredAmount;
                            DataModel.SaveChanges();
                            StatementEntriesSuccessfull = true;
                        }
                        else
                        {
                            StatementEntriesSuccessfull = false;
                        }
                    }
                }

                return StatementEntriesSuccessfull;
            }
        }

        public bool OpenStatement()
        {
            using (DLinq.CommissionDepartmentEntities DataModel = Entity.DataModel)
            {
                bool StatementEntriesSuccessfull = false;
                try
                {
                    DLinq.Statement currentStatement = (from p in DataModel.Statements where p.StatementId == this.StatementID select p).FirstOrDefault();
                    if (currentStatement != null)
                    {

                        currentStatement.StatementStatusId = 1;
                        currentStatement.MasterStatementStatu = DataModel.MasterStatementStatus.FirstOrDefault(s => s.StatementStatusId == 1);
                        DataModel.SaveChanges();
                        StatementEntriesSuccessfull = true;

                    }
                }
                catch
                {
                    StatementEntriesSuccessfull = false;                   
                }
                return StatementEntriesSuccessfull;
            }
        }

        public static Statement GetFindStatement(int statementNumber)
        {

            using (DLinq.CommissionDepartmentEntities DataModel = Entity.DataModel)
            {
                Statement statement = (from dv in DataModel.Statements
                                       where dv.StatementNumber == statementNumber
                                       select new Statement
                                       {
                                           StatementID = dv.StatementId,
                                           StatementNumber = dv.StatementNumber,
                                           StatementDate = dv.StatementDate.Value,
                                           CheckAmount = dv.CheckAmount,
                                           BalanceForOrAdjustment = dv.BalAdj,
                                           NetAmount = dv.CheckAmount ?? 0 + dv.BalAdj ?? 0,
                                           CompletePercentage = (double)((dv.EnteredAmount ?? 0) / ((dv.CheckAmount ?? 0 + dv.BalAdj ?? 0) == 0 ? int.MaxValue : (dv.CheckAmount ?? 0 + dv.BalAdj ?? 0))) * 100,
                                           BatchId = dv.BatchId.Value,
                                           StatusId = dv.MasterStatementStatu.StatementStatusId,
                                           Entries = dv.Entries ?? 0,
                                           EnteredAmount = dv.EnteredAmount.Value,
                                           CreatedDate = dv.CreatedOn.Value,
                                           LastModified = dv.LastModified.Value,
                                           PayorId = dv.PayorId.Value,
                                           TemplateID=dv.TemplateID,
                                           CreatedBy = dv.CreatedBy.Value,
                                           FromPage=dv.FromPage,
                                           ToPage=dv.ToPage

                                       }).FirstOrDefault();

                return statement;
            }

        }
        /// <summary>
        /// of a batch
        /// <param name="batch id"/>
        /// </summary>
        /// <returns></returns>
        /// 
        //public static List<Statement> GetStatementList(Guid BatchID)
        public List<Statement> GetStatementList(Guid BatchID)
        {
            using (DLinq.CommissionDepartmentEntities DataModel = Entity.DataModel)
            {
                List<Statement> statements = (from dv in DataModel.Statements
                                              where dv.Batch.BatchId == BatchID
                                              select new Statement
                                              {
                                                  StatementID = dv.StatementId,
                                                  StatementNumber = dv.StatementNumber,
                                                  StatementDate = dv.StatementDate.Value,
                                                  CheckAmount = dv.CheckAmount,
                                                  BalanceForOrAdjustment = dv.BalAdj,
                                                  NetAmount = ((dv.CheckAmount ?? 0) + (dv.BalAdj ?? 0)),
                                                  CompletePercentage = (double)((dv.EnteredAmount ?? 0) / ((dv.CheckAmount ?? 0 + dv.BalAdj ?? 0) == 0 ? int.MaxValue : (dv.CheckAmount ?? 0 + dv.BalAdj ?? 0))) * 100,
                                                  BatchId = dv.BatchId.Value,
                                                  StatusId = dv.MasterStatementStatu.StatementStatusId,
                                                  Entries = dv.Entries.Value,
                                                  EnteredAmount = dv.EnteredAmount.Value,
                                                  CreatedDate = dv.CreatedOn.Value,
                                                  LastModified = dv.LastModified.Value,
                                                  PayorId = dv.PayorId.Value,
                                                  CreatedBy = dv.CreatedBy.Value,
                                                  FromPage = dv.FromPage,
                                                  ToPage = dv.ToPage,
                                                  CreatedByDEU = DataModel.UserDetails.FirstOrDefault(p => p.UserCredentialId == dv.CreatedBy).LastName ?? "Super"
                                              }).ToList();

                //DLinq.UserDetail ud = null;
                //statements.ForEach(s => s.CreatedByDEU = ((ud = DataModel.UserDetails.FirstOrDefault(p => p.UserCredentialId == s.CreatedBy)) == null ? "Super" : ud.LastName));
                return statements;
            }
        }

        public static Statement GetStatement(Guid StatementID)
        {
            using (DLinq.CommissionDepartmentEntities DataModel = Entity.DataModel)
            {
                Statement statement = (from dv in DataModel.Statements
                                       where dv.StatementId == StatementID
                                       select new Statement
                                       {
                                           StatementID = dv.StatementId,
                                           StatementNumber = dv.StatementNumber,
                                           StatementDate = dv.StatementDate.Value,
                                           CheckAmount = dv.CheckAmount,
                                           BalanceForOrAdjustment = dv.BalAdj,
                                           NetAmount = dv.CheckAmount ?? 0 + dv.BalAdj ?? 0,
                                           CompletePercentage = (double)((dv.EnteredAmount ?? 0) / ((dv.CheckAmount ?? 0 + dv.BalAdj ?? 0) == 0 ? int.MaxValue : (dv.CheckAmount ?? 0 + dv.BalAdj ?? 0))) * 100,
                                           BatchId = dv.BatchId.Value,
                                           StatusId = dv.MasterStatementStatu.StatementStatusId,
                                           Entries = dv.Entries ?? 0,
                                           EnteredAmount = dv.EnteredAmount.Value,
                                           CreatedDate = dv.CreatedOn.Value,
                                           LastModified = dv.LastModified.Value,
                                           PayorId = dv.PayorId.Value,
                                           CreatedBy = dv.CreatedBy.Value,
                                           FromPage = dv.FromPage,
                                           ToPage = dv.ToPage

                                       }).FirstOrDefault();

                DLinq.UserDetail _UD = DataModel.UserDetails.FirstOrDefault(s => s.UserCredentialId == statement.CreatedBy);
                statement.CreatedByDEU = _UD == null ? "Super" : _UD.LastName;
                return statement;
            }
        }

        #endregion
        #region IEditable<Statement> Members

        public static bool DeleteStatement(Guid StatemetnId, UserRole _UserRole,string strOperation)
        {
            using (DLinq.CommissionDepartmentEntities DataModel = Entity.DataModel)
            {
                DLinq.Statement statement = DataModel.Statements.FirstOrDefault(s => s.StatementId == StatemetnId);
                //bool DeleteStatementSuccessfull = true;
                bool DeleteStatementSuccessfull = false;

                if (statement != null && statement.EntriesByDEUs != null && statement.EntriesByDEUs.Count != 0)
                {
                    TransactionOptions options = new TransactionOptions
                    {
                        IsolationLevel = IsolationLevel.ReadCommitted,
                        //Timeout = TimeSpan.FromMinutes(statement.EntriesByDEUs.Count * 1)
                        Timeout = TimeSpan.FromMinutes(statement.EntriesByDEUs.Count * 60)
                    };
                    try
                    {
                        using (TransactionScope transaction = new TransactionScope(TransactionScopeOption.Required,options))
                        {
                            List<Guid> entryIds = statement.EntriesByDEUs.Select(s => s.DEUEntryID).ToList();

                            for (int index = 0; index < entryIds.Count; index++)
                            {
                                PostUtill.PostStart(PostEntryProcess.Delete, entryIds[index], Guid.Empty, Guid.Empty, _UserRole, PostEntryProcess.Delete, string.Empty, strOperation);
                            }

                            using (DLinq.CommissionDepartmentEntities InnerDataModel = Entity.DataModel)
                            {
                                DLinq.Statement deletableStatement = InnerDataModel.Statements.FirstOrDefault(s => s.StatementId == StatemetnId);
                                InnerDataModel.DeleteObject(deletableStatement);
                                InnerDataModel.SaveChanges();
                                DeleteStatementSuccessfull = true;
                            }
                            transaction.Complete();
                        }
                    }
                    catch
                    {
                        DeleteStatementSuccessfull = false;
                       
                    }
                }
                else
                {
                    try
                    {
                        if (statement != null && statement.EntriesByDEUs != null && statement.EntriesByDEUs.Count == 0)
                        {
                            using (DLinq.CommissionDepartmentEntities InnerDataModel = Entity.DataModel)
                            {
                                DLinq.Statement deletableStatement = InnerDataModel.Statements.FirstOrDefault(s => s.StatementId == StatemetnId);
                                InnerDataModel.DeleteObject(deletableStatement);
                                InnerDataModel.SaveChanges();
                                DeleteStatementSuccessfull = true;
                            }
                        }
                    }
                    catch
                    { 
                        DeleteStatementSuccessfull = false;
                    }

                }
                return DeleteStatementSuccessfull;
            }
        }

        public bool IsStatementPartiallyOrFullyPaid()
        {
            using (DLinq.CommissionDepartmentEntities DataModel = Entity.DataModel)
            {
                bool IsPaid = false;
                List<PolicyPaymentEntry> Entries = DataModel.PolicyPaymentEntries.Where(s => s.StatementId == this.StatementID).ToList();
                if (Entries != null)
                {

                    foreach (PolicyPaymentEntry Entry in Entries)
                    {
                        bool Flag = PolicyOutgoingDistribution.GetOutgoingPaymentByPoicyPaymentEntryId(Entry.PaymentEntryId).Any(p => p.IsPaid == false);
                        if (Flag)
                        {
                            IsPaid = true;
                            break;
                        }
                    }
                }
                return IsPaid;
            }
        }

        #endregion
    }
}
