using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MyAgencyVault.BusinessLibrary.Base;
using System.Runtime.Serialization;
using DLinq = DataAccessLayer.LinqtoEntity;
using MyAgencyVault.BusinessLibrary.Masters;
using System.Data.EntityClient;
using System.Data.SqlClient;
using System.Data;
using System.Transactions;

namespace MyAgencyVault.BusinessLibrary
{
    [DataContract]
    public class User : UserDetail
    {
        #region  "Data members i.e. public properties"
        [DataMember]
        public String UserName { get; set; }

        [DataMember]
        public static String testName { get; set; }

        [DataMember]
        public String RembStatus { get; set; }
        [DataMember]
        public String SStatus { get; set; }
        [DataMember]
        public string Password { get; set; }
        [DataMember]
        public string PasswordHintQ { get; set; }
        [DataMember]
        public string PasswordHintA { get; set; }
        [DataMember]
        public Guid UserCredentialID { get; set; }
        [DataMember]
        public bool IsHouseAccount { get; set; }
        //Added 13012016
        [DataMember]
        public bool? IsAccountExec { get; set; }
        //Added 04062014
        [DataMember]
        public bool? DisableAgentEditing { get; set; }
        //[DataMember]
        //public bool IsDeleted { get; set; }
        [DataMember]
        public bool? IsLicenseDeleted { get; set; }
        [DataMember]
        public Guid? LicenseeId { get; set; }
        [DataMember]
        public DateTime CreateOn { get; set; }
        [DataMember]
        public string LicenseeName { get; set; }
        [DataMember]
        public List<Client> UserClients { get; set; }
        [DataMember]
        public UserRole Role { get; set; }
        [DataMember]
        public Guid AttachedToLicensee { get; set; }
        [IgnoreDataMember]
        private List<UserPermissions> _Permissions;
        [DataMember]
        public List<UserPermissions> Permissions
        {
            get
            {
                using (DLinq.CommissionDepartmentEntities DataModel = Entity.DataModel)
                {
                    List<UserPermissions> _permissions = null;
                    _permissions = (from usr in DataModel.UserPermissions
                                    where (usr.UserCredential.UserCredentialId == this.UserCredentialID && usr.UserCredential.IsDeleted == false)
                                    select new UserPermissions
                                    {
                                        UserID = usr.UserCredential.UserCredentialId,
                                        Module = (MasterModule)usr.MasterModule.ModuleId,
                                        Permission = (ModuleAccessRight)usr.MasterAccessRight.AccessRightId,
                                        UserPermissionId = usr.UserPermissionId
                                    }).OrderBy(p => p.Module).ToList();
                    return _permissions ?? new List<UserPermissions>();
                }
            }
            set
            {
                _Permissions = value;
            }
        }
        /// <summary>
        /// all the users to which the current logged in user have the accessibility to see their data.
        /// </summary> 
        [IgnoreDataMember]
        private List<LinkedUser> _Linkedusers;
        [DataMember]
        public List<LinkedUser> LinkedUsers
        {
            get
            {
                using (DLinq.CommissionDepartmentEntities DataModel = Entity.DataModel)
                {
                    List<LinkedUser> LnkUsers = null;

                    //if (this.Role == UserRole.Agent && this.IsHouseAccount == false)
                    //{
                    //    List<DLinq.UserCredential> connectedLinkedUsers = DataModel.UserCredentials.FirstOrDefault(s => s.UserCredentialId == this.UserCredentialID).UserCredentials.Where(s => s.IsDeleted == false && s.UserCredentialId != this.UserCredentialID).ToList();

                    //    if (connectedLinkedUsers != null && connectedLinkedUsers.Count != 0)
                    //        LnkUsers = (from usr in connectedLinkedUsers
                    //                    select new LinkedUser
                    //                    {
                    //                        UserId = usr.UserCredentialId,
                    //                        LastName = usr.UserDetail.LastName,
                    //                        FirstName = usr.UserDetail.FirstName,
                    //                        NickName = usr.UserDetail.NickName,
                    //                        IsConnected = true,
                    //                        UserName = usr.UserName
                    //                    }).ToList();

                    //    List<Guid> connectedUserIds = null;
                    //    if (LnkUsers == null)
                    //        connectedUserIds = new List<Guid>();
                    //    else
                    //        connectedUserIds = LnkUsers.Select(s => s.UserId).ToList();

                    //    List<LinkedUser> notConnectedLinkedUsers = (from usr in DataModel.UserCredentials
                    //                                                where (!connectedUserIds.Contains(usr.UserCredentialId)) && usr.LicenseeId == this.LicenseeId && usr.RoleId == 3 && (usr.UserCredentialId != this.UserCredentialID) && (usr.IsDeleted == false)
                    //                                                select new LinkedUser
                    //                                                {
                    //                                                    UserId = usr.UserCredentialId,
                    //                                                    LastName = usr.UserDetail.LastName,
                    //                                                    FirstName = usr.UserDetail.FirstName,
                    //                                                    NickName = usr.UserDetail.NickName,
                    //                                                    IsConnected = false,
                    //                                                    UserName = usr.UserName
                    //                                                }).ToList();

                    //    if (LnkUsers != null)
                    //        LnkUsers.AddRange(notConnectedLinkedUsers);
                    //    else if (notConnectedLinkedUsers != null)
                    //        LnkUsers = notConnectedLinkedUsers;
                    //}
                    //else
                    //{
                    //    List<DLinq.UserCredential> allUsersExceptHO = DataModel.UserCredentials.Where(s => s.LicenseeId == this.LicenseeId && s.IsDeleted == false && s.RoleId == 3 && s.IsHouseAccount == false).ToList();
                    //    LnkUsers = (from usr in allUsersExceptHO
                    //                select new LinkedUser
                    //                {
                    //                    UserId = usr.UserCredentialId,
                    //                    LastName = usr.UserDetail.LastName,
                    //                    FirstName = usr.UserDetail.FirstName,
                    //                    NickName = usr.UserDetail.NickName,
                    //                    IsConnected = true,
                    //                    UserName = usr.UserName
                    //                }).ToList();
                    //}
                    
                    return LnkUsers;
                }
            }
            set
            {
                _Linkedusers = value;
            }
        }
        [DataMember]
        public bool IsNewsToFlash { get; set; }
        [DataMember]
        public Guid? HouseOwnerId { get; set; }
        [DataMember]
        public Guid? AdminId { get; set; }
        [DataMember]
        public string WebDavPath { get; set; }

        #endregion
        #region IEditable<User> Members
        //to do: need to add a addupdate/delete function for user detail separately.
        /// <summary>   
        /// to do: need to do save for user permission included in this code itself.
        /// </summary>
        public void AddUpdateUser()
        {
            try
            {
                Guid guidUserID;
                using (DLinq.CommissionDepartmentEntities DataModel = Entity.DataModel)
                {
                    var _dtUsers = DataModel.UserCredentials.FirstOrDefault(s => s.UserCredentialId == this.UserCredentialID);

                    if (_dtUsers == null)
                    {
                        _dtUsers = new DLinq.UserCredential
                        {
                            UserCredentialId = ((this.UserCredentialID == Guid.Empty) ? Guid.NewGuid() : this.UserCredentialID),
                            UserName = this.UserName,
                            Password = this.Password,
                            PasswordHintAnswer = this.PasswordHintA,
                            PasswordHintQuestion = this.PasswordHintQ,
                            RoleId = (int)this.Role,
                            LicenseeId = this.LicenseeId,
                            CreatedOn = DateTime.Now
                        };
                        DataModel.AddToUserCredentials(_dtUsers);
                    }
                    else
                    {
                        _dtUsers.UserName = this.UserName;
                        _dtUsers.Password = this.Password;
                        _dtUsers.PasswordHintAnswer = this.PasswordHintA;
                        _dtUsers.PasswordHintQuestion = this.PasswordHintQ;
                    }

                    var _dtUserDetail = DataModel.UserDetails.FirstOrDefault(s => s.UserCredentialId == this.UserCredentialID);

                    if (_dtUserDetail == null)
                    {
                        _dtUserDetail = new DLinq.UserDetail
                        {
                            UserCredentialId = _dtUsers.UserCredentialId,
                            FirstName = this.FirstName,
                            LastName = this.LastName,
                            Company = this.Company,
                            NickName = this.NickName,
                            Address = this.Address,
                            ZipCode = this.ZipCode.CustomParseToLong(),
                            City = this.City,
                            State = this.State,
                            Email = this.Email,
                            FirstYearDefault = this.FirstYearDefault,
                            RenewalDefault = this.RenewalDefault,
                            ReportForEntireAgency = this.ReportForEntireAgency,
                            ReportForOwnBusiness = this.ReportForOwnBusiness,
                            OfficePhone = this.OfficePhone,
                            CellPhone = this.CellPhone,
                            Fax = this.Fax,
                            AddPayeeOn = null
                        };
                        DataModel.AddToUserDetails(_dtUserDetail);
                    }
                    else
                    {
                        _dtUserDetail.FirstName = FirstName;
                        _dtUserDetail.LastName = LastName;
                        _dtUserDetail.Company = Company;
                        _dtUserDetail.NickName = NickName;
                        _dtUserDetail.Address = Address;
                        _dtUserDetail.ZipCode = ZipCode.CustomParseToLong();
                        _dtUserDetail.City = City;
                        _dtUserDetail.State = State;
                        _dtUserDetail.Email = Email;
                        _dtUserDetail.OfficePhone = OfficePhone;
                        _dtUserDetail.CellPhone = CellPhone;
                        _dtUserDetail.Fax = Fax;
                    }
                    DataModel.SaveChanges();

                    guidUserID = _dtUsers.UserCredentialId;
                }

                foreach (UserPermissions up in this._Permissions)
                {
                    if (up.UserID != null)
                    {
                        if (up.UserID == Guid.Empty)
                        {
                            up.UserID = guidUserID;
                        }
                    }
                    else
                    {
                        up.UserID = guidUserID;
                    }
                }
                //Add default permission for the user...
                UpdateUserPermission();
            }
            catch
            {
            }
        }

        public void AddUpdateUserPermission()
        {
            using (DLinq.CommissionDepartmentEntities DataModel = Entity.DataModel)
            {
                DLinq.UserDetail userDetail = DataModel.UserDetails.FirstOrDefault(s => s.UserCredentialId == this.UserCredentialID);

                if (userDetail != null)
                {
                    //added
                    userDetail.DisableAgentEditing = this.DisableAgentEditing;
                    userDetail.RenewalDefault = this.RenewalDefault;
                    userDetail.FirstYearDefault = this.FirstYearDefault;
                    userDetail.ReportForEntireAgency = this.ReportForEntireAgency;
                    userDetail.ReportForOwnBusiness = this.ReportForOwnBusiness;
                    DataModel.SaveChanges();
                }
            }

            bool bvalue = false;
            if (this.IsAccountExec == true)
            {
                bvalue = true;
            }
            try
            {
                UpdateAccountExec(this.UserCredentialID, bvalue);
            }
            catch
            {
            }

            try
            {
                UpdateUserPermission();
            }
            catch
            {
            }
            try
            {
                SaveLinkedUsers();
            }
            catch
            {
            }
            try
            {
                SaveLinkedUsers(this._Linkedusers, this.UserCredentialID);
            }
            catch
            {
            }
        }

        public bool Delete()
        {
            using (DLinq.CommissionDepartmentEntities DataModel = Entity.DataModel)
            {

                int associtedPolicyCount = DataModel.Policies.Where(s => s.CreatedBy == this.UserCredentialID && s.IsDeleted == false).Count();
                bool UserPresentAsPayee = OutGoingPayment.IsUserPresentAsPayee(this.UserCredentialID) | OutgoingSchedule.IsUserPresentAsPayee(this.UserCredentialID);

                if (associtedPolicyCount != 0 || UserPresentAsPayee)
                {
                    return false;
                }

                var _dtUsers = (from u in DataModel.UserCredentials
                                where (u.UserCredentialId == this.UserCredentialID)
                                select u).FirstOrDefault();

                if (_dtUsers != null)
                    _dtUsers.IsDeleted = true;

                DataModel.SaveChanges();

                return true;
            }
        }

        public void UpdateAccountExec(Guid userCredencialID, bool bvalue)
        {
            try
            {
                if (CheckAccoutExec(userCredencialID))
                {
                    return;
                }
                else
                {
                    using (DLinq.CommissionDepartmentEntities DataModel = Entity.DataModel)
                    {
                        DLinq.UserCredential UserCredential = DataModel.UserCredentials.FirstOrDefault(s => s.UserCredentialId == userCredencialID);

                        if (UserCredential != null)
                        {
                            //added
                            UserCredential.IsAccountExec = bvalue;
                            DataModel.SaveChanges();
                        }
                    }
                }
            }
            catch
            {
            }
        }

        public bool CheckAccoutExec(Guid userCredencialID, bool bvalue)
        {
            bool bValue = false;
            try
            {
                using (DLinq.CommissionDepartmentEntities DataModel = Entity.DataModel)
                {
                    DLinq.Policy objPolicy = DataModel.Policies.FirstOrDefault(s => s.UserCredentialId == userCredencialID);

                    if (objPolicy != null)
                    {
                        bValue = true;
                    }
                }
            }
            catch
            {
            }

            return bValue;
        }

        public bool CheckAccoutExec(Guid userCredencialID)
        {
            bool bValue = false;
            try
            {
                using (DLinq.CommissionDepartmentEntities DataModel = Entity.DataModel)
                {
                    DLinq.UserCredential UserCredential = DataModel.UserCredentials.FirstOrDefault(s => s.UserCredentialId == userCredencialID);

                    if (UserCredential != null)
                    {
                        UserCredential.IsAccountExec = true;
                        DataModel.SaveChanges();
                    }
                }
            }
            catch
            {
            }

            return bValue;
        }

        #endregion

        public User GetUserWithinLicensee(Guid userCredencialID, Guid LicenseeID)
        {
            User objPolicy = new User();
            try
            {
                using (DLinq.CommissionDepartmentEntities DataModel = Entity.DataModel)
                {
                    objPolicy = (from uc in DataModel.UserCredentials
                                 where (uc.UserCredentialId == userCredencialID) && (uc.IsDeleted == false) && (uc.LicenseeId == LicenseeID)
                                 select new User
                                 {
                                     UserCredentialID = uc.UserCredentialId

                                 }
                              ).FirstOrDefault();
                }

            }
            catch
            {
            }

            return objPolicy;
        }

        public static void HouseAccoutTransferProcess(User user)
        {
            try
            {
                using (DLinq.CommissionDepartmentEntities DataModel = Entity.DataModel)
                {
                    DLinq.UserCredential NewHOUser = DataModel.UserCredentials.FirstOrDefault(s => s.UserCredentialId == user.UserCredentialID);

                    if (NewHOUser == null)
                        throw new InvalidOperationException("Target user is not exist");

                    DLinq.UserCredential HOUser = DataModel.UserCredentials.First(s => s.LicenseeId == user.LicenseeId && s.IsDeleted == false && s.IsHouseAccount == true);
                    HOUser.IsHouseAccount = false;
                    NewHOUser.IsHouseAccount = true;

                    DataModel.Policies.Where(s => s.CreatedBy == HOUser.UserCredentialId && s.IsDeleted == false).ToList().ForEach(s => s.CreatedBy = NewHOUser.UserCredentialId);
                    DataModel.SaveChanges();
                }
            }
            catch
            {
            }
        }

        public static List<UserPermissions> GetCurrentPermission(Guid UserCredentialId)
        {

            using (DLinq.CommissionDepartmentEntities DataModel = Entity.DataModel)
            {
                List<UserPermissions> _permissions = null;
                _permissions = (from usr in DataModel.UserPermissions
                                where (usr.UserCredential.UserCredentialId == UserCredentialId && usr.UserCredential.IsDeleted == false)
                                select new UserPermissions
                                {
                                    UserID = usr.UserCredential.UserCredentialId,
                                    Module = (MasterModule)usr.MasterModule.ModuleId,
                                    Permission = (ModuleAccessRight)usr.MasterAccessRight.AccessRightId,
                                    UserPermissionId = usr.UserPermissionId
                                }).OrderBy(p => p.Module).ToList();
                return _permissions;
            }

        }
        /// <summary>
        /// invoke to change/update the various kind permission of the user.
        /// </summary>
        /// <returns>true if permissions updated successfully for this user, else false</returns>
        public void UpdateUserPermission()
        {
            try
            {
                if (_Permissions == null)
                    return;

                using (DLinq.CommissionDepartmentEntities DataModel = Entity.DataModel)
                {
                    DLinq.UserPermission _permision = null;

                    foreach (UserPermissions up in this._Permissions)
                    {
                        DLinq.MasterAccessRight _Right = ReferenceMaster.GetReferencedMasterAccessRight(up.Permission, DataModel);
                        DLinq.MasterModule _Module = ReferenceMaster.GetReferencedMasterModule(up.Module, DataModel);
                        DLinq.UserCredential _UC = ReferenceMaster.GetreferencedUserCredential(up.UserID.Value, DataModel);

                        _permision = (from p in DataModel.UserPermissions where p.UserPermissionId == up.UserPermissionId select p).FirstOrDefault();
                        if (_permision != null)
                        {
                            _permision.MasterAccessRightReference.Value = _Right;
                            _permision.UserCredentialReference.Value = _UC;
                            _permision.MasterModuleReference.Value = _Module;
                        }
                        else
                        {
                            _permision = new DLinq.UserPermission();
                            _permision.MasterAccessRightReference.Value = _Right;
                            _permision.UserCredentialReference.Value = _UC;
                            _permision.MasterModuleReference.Value = _Module;
                            _permision.UserPermissionId = up.UserPermissionId.Value;
                            DataModel.AddToUserPermissions(_permision);

                        }
                        DataModel.SaveChanges();
                    }
                }
            }
            catch
            {
            }
        }

        public static string getUserEmail(Guid UserID)
        {
            using (DLinq.CommissionDepartmentEntities DataModel = Entity.DataModel)
            {
                string userEmail = (from results in DataModel.UserDetails
                                    where results.UserCredentialId == UserID
                                    select results.Email).FirstOrDefault();
                return userEmail;
            }
        }

        public static User GetUserIdWise(Guid UserCredId)
        {
            using (DLinq.CommissionDepartmentEntities DataModel = Entity.DataModel)
            {
                User _user = (from uc in DataModel.UserCredentials

                              where (uc.UserCredentialId == UserCredId) && (uc.IsDeleted == false)
                              select new User
                              {
                                  UserCredentialID = uc.UserCredentialId,
                                  FirstName = uc.UserDetail.FirstName,
                                  LastName = uc.UserDetail.LastName,
                                  City = uc.UserDetail.City,
                                  Company = uc.UserDetail.Company,
                                  State = uc.UserDetail.State,
                                  NickName = uc.UserDetail.NickName,
                                  Email = uc.UserDetail.Email,
                                  Address = uc.UserDetail.Address,
                                  OfficePhone = uc.UserDetail.OfficePhone,
                                  CellPhone = uc.UserDetail.CellPhone,
                                  Fax = uc.UserDetail.Fax,
                                  LicenseeId = uc.Licensee.LicenseeId,
                                  LicenseeName = uc.Licensee.Company,
                                  UserName = uc.UserName,
                                  Password = uc.Password,
                                  PasswordHintQ = uc.PasswordHintQuestion,
                                  PasswordHintA = uc.PasswordHintAnswer,
                                  IsHouseAccount = uc.IsHouseAccount,
                                  IsAccountExec = uc.IsAccountExec,
                                  //Added
                                  DisableAgentEditing = uc.UserDetail.DisableAgentEditing,

                                  FirstYearDefault = uc.UserDetail.FirstYearDefault,
                                  RenewalDefault = uc.UserDetail.RenewalDefault,
                                  Role = (UserRole)uc.MasterRole.RoleId,
                                  IsNewsToFlash = uc.IsNewsToFlash
                              }
                         ).FirstOrDefault();


                //DLinq.UserDetail userDetail = DataModel.UserDetails.FirstOrDefault(s => s.UserCredentialId == _user.UserCredentialID);
                //if (userDetail.ZipCode != null)
                //    _user.ZipCode = userDetail.ZipCode.Value.ToString("D5");
                //else
                //    _user.ZipCode = null;

                return _user;
            }
        }

        public static Guid GetLicenseeUserCredentialId(Guid licId)
        {
            using (DLinq.CommissionDepartmentEntities DataModel = Entity.DataModel)
            {
                if (licId == null || licId == Guid.Empty)
                {
                    Guid userId = DataModel.UserCredentials.Where(s => s.MasterRole.RoleId == 1).First().UserCredentialId;
                    return userId;
                }
                else
                {
                    Guid userId = DataModel.UserCredentials.Where(s => s.MasterRole.RoleId == 2 && s.LicenseeId == licId).First().UserCredentialId;
                    return userId;
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// 
        private void SaveLinkedUsers()
        {
            DLinq.UserCredential linkedUser;
            using (DLinq.CommissionDepartmentEntities DataModel = Entity.DataModel)
            {
                DLinq.UserCredential user = DataModel.UserCredentials.FirstOrDefault(s => s.UserCredentialId == this.UserCredentialID && s.IsDeleted == false);
                user.UserCredentials.Clear();
                foreach (LinkedUser agent in this._Linkedusers)
                {
                    if (agent.IsConnected == true)
                    {
                        linkedUser = ReferenceMaster.GetreferencedUserCredential(agent.UserId, DataModel);
                        user.UserCredentials.Add(linkedUser);
                    }
                }
                DataModel.SaveChanges();
            }
        }

        ///<summary>
        /// 
        
        /// </summary>
        private void SaveLinkedUsers(List<LinkedUser> LinkedUser, Guid CurrentUserID)
        {
            DLinq.UserCredential linkedUser;
            using (DLinq.CommissionDepartmentEntities DataModel = Entity.DataModel)
            {
                DLinq.UserCredential user = DataModel.UserCredentials.FirstOrDefault(s => s.UserCredentialId == CurrentUserID && s.IsDeleted == false);
                user.UserCredentials.Clear();
                foreach (LinkedUser agent in LinkedUser)
                {
                    if (agent.IsConnected == true)
                    {
                        linkedUser = ReferenceMaster.GetreferencedUserCredential(agent.UserId, DataModel);
                        user.UserCredentials.Add(linkedUser);
                    }
                }
                DataModel.SaveChanges();
            }
        }
        /// <summary>
        /// for sign in purpose, validate the userid and password provided in the parameter.
        /// </summary>
        /// 
        public User GetValidIdentity(string UserName, string Password)
        {
            var pp = new User();


            using (DLinq.CommissionDepartmentEntities DataModel = Entity.DataModel)
            {
                DataModel.CommandTimeout = 600000000;

                pp = (from ud in DataModel.UserCredentials
                      where (ud.UserName == UserName && ud.Password == Password && ud.IsDeleted == false && ((ud.Licensee == null) || (ud.Licensee.LicenseStatusId != 1)))
                      select new User
                      {
                          UserName = ud.UserName,
                          Password = ud.Password,
                          PasswordHintA = ud.PasswordHintAnswer,
                          PasswordHintQ = ud.PasswordHintQuestion,
                          Role = (UserRole)ud.MasterRole.RoleId,
                          UserCredentialID = ud.UserCredentialId,
                          IsHouseAccount = ud.IsHouseAccount,
                          DisableAgentEditing = ud.UserDetail.DisableAgentEditing,
                          IsDeleted = ud.IsDeleted,
                          IsLicenseDeleted = ud.Licensee.IsDeleted,
                          LicenseeId = (Guid?)ud.Licensee.LicenseeId ?? Guid.Empty,
                          IsNewsToFlash = ud.IsNewsToFlash
                      }).FirstOrDefault();

                if (pp == null) return null;

                if (!pp.LicenseeId.IsNullOrEmpty())
                {
                    pp.HouseOwnerId = DataModel.UserCredentials.First(s => s.LicenseeId == pp.LicenseeId && s.IsHouseAccount == true && s.IsDeleted == false).UserCredentialId;
                    pp.AdminId = DataModel.UserCredentials.First(s => s.LicenseeId == pp.LicenseeId && s.RoleId == 2 && s.IsDeleted == false).UserCredentialId;
                }
                else
                {
                    pp.HouseOwnerId = Guid.Empty;
                    pp.AdminId = Guid.Empty;
                }

                if ((String.Compare(pp.UserName, UserName, true) != 0) || (String.Compare(pp.Password, Password, false) != 0))
                    return null;

                pp.Permissions = GetCurrentPermission(pp.UserCredentialID);
                pp.WebDavPath = SystemConstant.GetKeyValue("WebDevPath");

                if (pp.LicenseeId != Guid.Empty)
                    Licensee.SetLastLoginTime(pp.LicenseeId.Value);

                return pp;
            }
           
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="UserName"></param>
        /// <param name="Password"></param>
        /// <returns></returns>
        /// 
        public User GetForgetValidIdentity(string UserName)
        {
            using (DLinq.CommissionDepartmentEntities DataModel = Entity.DataModel)
            {
                var pp = (from ud in DataModel.UserCredentials
                          where (ud.UserName.ToLower() == UserName.ToLower() && ud.IsDeleted == false)
                          select ud).FirstOrDefault();
                User user = null;
                if (pp != null)
                {
                    user = new User
                          {
                              UserName = pp.UserName,
                              Password = pp.Password,
                              PasswordHintA = pp.PasswordHintAnswer,
                              PasswordHintQ = pp.PasswordHintQuestion,
                              Role = (UserRole)pp.MasterRole.RoleId,
                              UserCredentialID = pp.UserCredentialId,
                              IsHouseAccount = pp.IsHouseAccount,
                              IsDeleted = pp.IsDeleted,
                              IsLicenseDeleted = pp.Licensee.IsDeleted,
                              LicenseeId = (Guid?)(pp.Licensee != null ? pp.Licensee.LicenseeId : Guid.Empty)

                          };

                    user.Email = DataModel.UserDetails.Where(s => s.UserCredential.UserCredentialId == pp.UserCredentialId).Select(s => s.Email).FirstOrDefault();
                }
                return user;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        /// add by vinod 
        /// to get userlist by user email 
        public static List<User> GetAllUsers(string strEmailAddress)
        {
            using (DLinq.CommissionDepartmentEntities DataModel = Entity.DataModel)
            {
                DataModel.CommandTimeout = 600000000;

                List<User> _usr = null;
                if (!string.IsNullOrEmpty(strEmailAddress))
                {
                    strEmailAddress = strEmailAddress.ToUpper();
                }

                _usr = (from uc in DataModel.UserCredentials
                        where uc.IsDeleted == false && uc.UserDetail != null && uc.UserDetail.Email.ToUpper() == strEmailAddress
                        select new User
                        {
                            UserCredentialID = uc.UserCredentialId,
                            IsAccountExec = uc.IsAccountExec,
                            FirstName = uc.UserDetail.FirstName,
                            LastName = uc.UserDetail.LastName,
                            //City = uc.UserDetail.City,
                            //Company = uc.UserDetail.Company,
                            //State = uc.UserDetail.State,
                            NickName = uc.UserDetail.NickName,
                            Email = uc.UserDetail.Email,
                            //Address = uc.UserDetail.Address,
                            //OfficePhone = uc.UserDetail.OfficePhone,
                            //CellPhone = uc.UserDetail.CellPhone,
                            //Fax = uc.UserDetail.Fax,
                            LicenseeId = uc.Licensee.LicenseeId,
                            LicenseeName = uc.Licensee.Company,
                            UserName = uc.UserName,
                            Password = uc.Password,
                            //DisableAgentEditing = uc.UserDetail.DisableAgentEditing,
                            //PasswordHintQ = uc.PasswordHintQuestion,
                            //PasswordHintA = uc.PasswordHintAnswer,
                            IsHouseAccount = uc.IsHouseAccount,
                            //FirstYearDefault = uc.UserDetail.FirstYearDefault,
                            //RenewalDefault = uc.UserDetail.RenewalDefault,
                            Role = (UserRole)uc.MasterRole.RoleId
                            //IsNewsToFlash = uc.IsNewsToFlash
                        }
                        ).ToList();
                foreach (User usr in _usr)
                {
                    DLinq.UserDetail userDetail = DataModel.UserDetails.FirstOrDefault(s => s.UserCredentialId == usr.UserCredentialID);
                    if (userDetail.ZipCode != null)
                        usr.ZipCode = userDetail.ZipCode.Value.ToString("D5");
                    else
                        usr.ZipCode = null;

                    if (!usr.LicenseeId.IsNullOrEmpty())
                    {
                        usr.HouseOwnerId = DataModel.UserCredentials.First(s => s.LicenseeId == usr.LicenseeId && s.IsHouseAccount == true && s.IsDeleted == false).UserCredentialId;
                        usr.AdminId = DataModel.UserCredentials.First(s => s.LicenseeId == usr.LicenseeId && s.RoleId == 2 && s.IsDeleted == false).UserCredentialId;
                    }
                    else
                    {
                        usr.HouseOwnerId = Guid.Empty;
                        usr.AdminId = Guid.Empty;
                    }
                }
                return _usr;
            }
        }
        
        public static List<User> GetAllPayee()
        {
            using (DLinq.CommissionDepartmentEntities DataModel = Entity.DataModel)
            {
                List<User> _usr = null;

                _usr = (from uc in DataModel.UserCredentials
                        where uc.IsDeleted == false && uc.UserDetail != null && uc.RoleId == 3
                        select new User
                        {
                            IsHouseAccount = uc.IsHouseAccount,
                            IsAccountExec = uc.IsAccountExec,
                            UserCredentialID = uc.UserCredentialId,
                            FirstName = uc.UserDetail.FirstName,
                            LastName = uc.UserDetail.LastName,                           
                            //Company = uc.UserDetail.Company,                           
                            NickName = uc.UserDetail.NickName,
                            Email = uc.UserDetail.Email,                            
                            LicenseeId = uc.Licensee.LicenseeId,
                            //LicenseeName = uc.Licensee.Company,
                            UserName = uc.UserName,
                            Password = uc.Password,
                            //DisableAgentEditing = uc.UserDetail.DisableAgentEditing,
                            //PasswordHintQ = uc.PasswordHintQuestion,
                            //PasswordHintA = uc.PasswordHintAnswer,
                            //FirstYearDefault = uc.UserDetail.FirstYearDefault,
                            //RenewalDefault = uc.UserDetail.RenewalDefault,
                            Role = (UserRole)uc.MasterRole.RoleId
                            //IsNewsToFlash = uc.IsNewsToFlash
                        }
                        ).ToList();

                return _usr;
            }
        }

        public static List<User> GetAccountExecByLicencessID(Guid licensssID)
        {
            using (DLinq.CommissionDepartmentEntities DataModel = Entity.DataModel)
            {
                List<User> _usr = null;
                try
                {
                    DataModel.CommandTimeout = 600000000;

                    _usr = (from uc in DataModel.UserCredentials
                            where uc.IsDeleted == false && uc.UserDetail != null && uc.LicenseeId == licensssID & uc.IsAccountExec == true
                            select new User
                            {
                                IsHouseAccount = uc.IsHouseAccount,
                                IsAccountExec = uc.IsAccountExec,
                                UserCredentialID = uc.UserCredentialId,
                                FirstName = uc.UserDetail.FirstName,
                                LastName = uc.UserDetail.LastName,
                                Company = uc.UserDetail.Company,
                                NickName = uc.UserDetail.NickName,
                                Email = uc.UserDetail.Email,
                                LicenseeId = uc.Licensee.LicenseeId,
                                UserName = uc.UserName,
                                Password = uc.Password,
                                PasswordHintA = uc.PasswordHintAnswer,
                                Role = (UserRole)uc.MasterRole.RoleId

                            }
                            ).ToList();


                    _usr = _usr.OrderBy(s => s.NickName).ToList();
                }
                catch
                {
                }

                return _usr;
            }
        }

        public static int GetAccountExecCount(Guid licensssID)
        {
            int intCount = 0;
            try
            {
                using (DLinq.CommissionDepartmentEntities DataModel = Entity.DataModel)
                {
                    DataModel.CommandTimeout = 600000000;                   
                    intCount = (from uc in DataModel.UserCredentials where uc.IsDeleted == false && uc.UserDetail != null && uc.LicenseeId == licensssID & uc.IsAccountExec == true select uc).Count();
                }
            }
            catch
            {
            }
            return intCount;
        }

        public static int GetAllAccountExecCount()
        {
            int intCount = 0;
            try
            {
                using (DLinq.CommissionDepartmentEntities DataModel = Entity.DataModel)
                {
                    DataModel.CommandTimeout = 600000000;
                    intCount = (from uc in DataModel.UserCredentials where uc.IsDeleted == false && uc.UserDetail != null & uc.IsAccountExec == true select uc).Count();
                }
            }
            catch
            {
            }
            return intCount;
        }

        public int GetPayeeCount(Guid licensssID)
        {
            int intCount = 0;
            try
            {
                using (DLinq.CommissionDepartmentEntities DataModel = Entity.DataModel)
                {
                    DataModel.CommandTimeout = 600000000;
                    intCount = (from uc in DataModel.UserCredentials where uc.IsDeleted == false && uc.UserDetail != null && uc.LicenseeId == licensssID select uc).Count();
                }
            }
            catch
            {
            }
            return intCount;
        }

        public int GetAllPayeeCount()
        {
            int intCount = 0;
            try
            {
                using (DLinq.CommissionDepartmentEntities DataModel = Entity.DataModel)
                {
                    DataModel.CommandTimeout = 600000000;
                    intCount = (from uc in DataModel.UserCredentials where uc.IsDeleted == false && uc.UserDetail != null select uc).Count();
                }
            }
            catch
            {
            }
            return intCount;
        }

        public static List<User> GetAllPayeeByLicencessID(Guid licensssID)
        {
            using (DLinq.CommissionDepartmentEntities DataModel = Entity.DataModel)
            {
                List<User> _usr = null;
                DataModel.CommandTimeout = 600000000;

                _usr = (from uc in DataModel.UserCredentials
                        where uc.IsDeleted == false && uc.UserDetail != null && uc.LicenseeId == licensssID && uc.RoleId == 3 //acme 
                        select new User
                        {
                            IsHouseAccount = uc.IsHouseAccount,
                            IsAccountExec = uc.IsAccountExec,
                            UserCredentialID = uc.UserCredentialId,
                            FirstName = uc.UserDetail.FirstName,
                            LastName = uc.UserDetail.LastName,
                            //City = uc.UserDetail.City,
                            Company = uc.UserDetail.Company,
                            //State = uc.UserDetail.State,
                            NickName = uc.UserDetail.NickName,
                            Email = uc.UserDetail.Email,
                            Address = uc.UserDetail.Address,
                            //OfficePhone = uc.UserDetail.OfficePhone,
                            //CellPhone = uc.UserDetail.CellPhone,
                            //Fax = uc.UserDetail.Fax,
                            LicenseeId = uc.Licensee.LicenseeId,
                            LicenseeName = uc.Licensee.Company,
                            UserName = uc.UserName,
                            Password = uc.Password,
                            //DisableAgentEditing = uc.UserDetail.DisableAgentEditing,
                            //PasswordHintQ = uc.PasswordHintQuestion,
                            PasswordHintA = uc.PasswordHintAnswer,
                            //FirstYearDefault = uc.UserDetail.FirstYearDefault,
                            //RenewalDefault = uc.UserDetail.RenewalDefault,
                            Role = (UserRole)uc.MasterRole.RoleId
                            //IsNewsToFlash = uc.IsNewsToFlash
                        }
                        ).ToList();

                //foreach (User usr in _usr)
                //{
                //    DLinq.UserDetail userDetail = DataModel.UserDetails.FirstOrDefault(s => s.UserCredentialId == usr.UserCredentialID);
                //    if (userDetail.ZipCode != null)
                //        usr.ZipCode = userDetail.ZipCode.Value.ToString("D5");
                //    else
                //        usr.ZipCode = null;

                //    if (!usr.LicenseeId.IsNullOrEmpty())
                //    {
                //        usr.HouseOwnerId = DataModel.UserCredentials.First(s => s.LicenseeId == usr.LicenseeId && s.IsHouseAccount == true && s.IsDeleted == false).UserCredentialId;
                //        usr.AdminId = DataModel.UserCredentials.First(s => s.LicenseeId == usr.LicenseeId && s.RoleId == 2 && s.IsDeleted == false).UserCredentialId;
                //    }
                //    else
                //    {
                //        usr.HouseOwnerId = Guid.Empty;
                //        usr.AdminId = Guid.Empty;
                //    }
                //}
                return _usr;
            }
        }
        
        //public static List<User> GetAllPayeeByLicencessID(Guid licensssID)
        //{
        //    using (DLinq.CommissionDepartmentEntities DataModel = Entity.DataModel)
        //    {
        //        List<User> _usr = null;

        //        _usr = (from uc in DataModel.UserCredentials
        //                where uc.IsDeleted == false && uc.UserDetail != null && uc.IsHouseAccount == false && uc.RoleId == 3 && uc.LicenseeId == licensssID
        //                select new User
        //                {
        //                    IsHouseAccount = uc.IsHouseAccount,
        //                    UserCredentialID = uc.UserCredentialId,
        //                    FirstName = uc.UserDetail.FirstName,
        //                    LastName = uc.UserDetail.LastName,
        //                    City = uc.UserDetail.City,
        //                    Company = uc.UserDetail.Company,
        //                    State = uc.UserDetail.State,
        //                    NickName = uc.UserDetail.NickName,
        //                    Email = uc.UserDetail.Email,
        //                    Address = uc.UserDetail.Address,
        //                    OfficePhone = uc.UserDetail.OfficePhone,
        //                    CellPhone = uc.UserDetail.CellPhone,
        //                    Fax = uc.UserDetail.Fax,
        //                    LicenseeId = uc.Licensee.LicenseeId,
        //                    LicenseeName = uc.Licensee.Company,
        //                    UserName = uc.UserName,
        //                    Password = uc.Password,
        //                    DisableAgentEditing = uc.UserDetail.DisableAgentEditing,
        //                    PasswordHintQ = uc.PasswordHintQuestion,
        //                    PasswordHintA = uc.PasswordHintAnswer,
        //                    FirstYearDefault = uc.UserDetail.FirstYearDefault,
        //                    RenewalDefault = uc.UserDetail.RenewalDefault,
        //                    Role = (UserRole)uc.MasterRole.RoleId,
        //                    IsNewsToFlash = uc.IsNewsToFlash
        //                }
        //                ).ToList();

        //        //foreach (User usr in _usr)
        //        //{
        //        //    DLinq.UserDetail userDetail = DataModel.UserDetails.FirstOrDefault(s => s.UserCredentialId == usr.UserCredentialID);
        //        //    if (userDetail.ZipCode != null)
        //        //        usr.ZipCode = userDetail.ZipCode.Value.ToString("D5");
        //        //    else
        //        //        usr.ZipCode = null;

        //        //    if (!usr.LicenseeId.IsNullOrEmpty())
        //        //    {
        //        //        usr.HouseOwnerId = DataModel.UserCredentials.First(s => s.LicenseeId == usr.LicenseeId && s.IsHouseAccount == true && s.IsDeleted == false).UserCredentialId;
        //        //        usr.AdminId = DataModel.UserCredentials.First(s => s.LicenseeId == usr.LicenseeId && s.RoleId == 2 && s.IsDeleted == false).UserCredentialId;
        //        //    }
        //        //    else
        //        //    {
        //        //        usr.HouseOwnerId = Guid.Empty;
        //        //        usr.AdminId = Guid.Empty;
        //        //    }
        //        //}
        //        return _usr;
        //    }
        //}

        public static List<User> GetAllUsers()
        {
            using (DLinq.CommissionDepartmentEntities DataModel = Entity.DataModel)
            {
                DataModel.CommandTimeout = 600000000;

                List<User> _usr = null;

                _usr = (from uc in DataModel.UserCredentials
                        where uc.IsDeleted == false && uc.UserDetail != null
                        select new User
                        {
                            IsHouseAccount = uc.IsHouseAccount,
                            IsAccountExec = uc.IsAccountExec,
                            UserCredentialID = uc.UserCredentialId,
                            FirstName = uc.UserDetail.FirstName,
                            LastName = uc.UserDetail.LastName,
                            //City = uc.UserDetail.City,
                            Company = uc.UserDetail.Company,
                            //State = uc.UserDetail.State,
                            NickName = uc.UserDetail.NickName,
                            Email = uc.UserDetail.Email,
                            Address = uc.UserDetail.Address,
                            //OfficePhone = uc.UserDetail.OfficePhone,
                            //CellPhone = uc.UserDetail.CellPhone,
                            Fax = uc.UserDetail.Fax,
                            LicenseeId = uc.Licensee.LicenseeId,
                            LicenseeName = uc.Licensee.Company,
                            UserName = uc.UserName,
                            Password = uc.Password,
                            //Added 04062014    
                            //DisableAgentEditing = uc.UserDetail.DisableAgentEditing,
                            PasswordHintQ = uc.PasswordHintQuestion,
                            PasswordHintA = uc.PasswordHintAnswer,
                            //FirstYearDefault = uc.UserDetail.FirstYearDefault,
                            //RenewalDefault = uc.UserDetail.RenewalDefault,
                            Role = (UserRole)uc.MasterRole.RoleId
                            //IsNewsToFlash = uc.IsNewsToFlash
                        }
                        ).ToList();

                //foreach (User usr in _usr)
                //{
                //    DLinq.UserDetail userDetail = DataModel.UserDetails.FirstOrDefault(s => s.UserCredentialId == usr.UserCredentialID);
                //    if (userDetail.ZipCode != null)
                //        usr.ZipCode = userDetail.ZipCode.Value.ToString("D5");
                //    else
                //        usr.ZipCode = null;

                //    if (!usr.LicenseeId.IsNullOrEmpty())
                //    {
                //        usr.HouseOwnerId = DataModel.UserCredentials.First(s => s.LicenseeId == usr.LicenseeId && s.IsHouseAccount == true && s.IsDeleted == false).UserCredentialId;
                //        usr.AdminId = DataModel.UserCredentials.First(s => s.LicenseeId == usr.LicenseeId && s.RoleId == 2 && s.IsDeleted == false).UserCredentialId;
                //    }
                //    else
                //    {
                //        usr.HouseOwnerId = Guid.Empty;
                //        usr.AdminId = Guid.Empty;
                //    }
                //}
                return _usr;


            }
        }

        /// <summary>
        /// overloaded function be there to support various filter criteria.
        /// GetUsers()/all/all viewable to loggedinuser/all in the licensee/all of the given search criteria/of one userrole.
        /// </summary>
        /// <returns></returns>
        /// 

        public static IEnumerable<User> GetUsers(Guid? LicenseeId, UserRole RoleIdToView)
        {
            using (DLinq.CommissionDepartmentEntities DataModel = Entity.DataModel)
            {
                IEnumerable<User> userList = null;

                userList = (from uc in DataModel.UserCredentials
                            where uc.IsDeleted == false && uc.MasterRole.RoleId == (int)RoleIdToView && (uc.Licensee.IsDeleted == false) && (uc.Licensee.LicenseeId == LicenseeId)
                            select new User
                            {
                                UserCredentialID = uc.UserCredentialId,
                                IsAccountExec = uc.IsAccountExec,
                                FirstName = uc.UserDetail.FirstName,
                                LastName = uc.UserDetail.LastName,
                                City = uc.UserDetail.City,
                                Company = uc.UserDetail.Company,
                                State = uc.UserDetail.State,
                                NickName = uc.UserDetail.NickName,
                                Email = uc.UserDetail.Email,
                                Address = uc.UserDetail.Address,
                                OfficePhone = uc.UserDetail.OfficePhone,
                                CellPhone = uc.UserDetail.CellPhone,
                                Fax = uc.UserDetail.Fax,
                                LicenseeId = uc.Licensee.LicenseeId,
                                LicenseeName = uc.Licensee.Company,
                                UserName = uc.UserName,
                                Password = uc.Password,
                                DisableAgentEditing = uc.UserDetail.DisableAgentEditing,
                                PasswordHintQ = uc.PasswordHintQuestion,
                                PasswordHintA = uc.PasswordHintAnswer,
                                IsHouseAccount = uc.IsHouseAccount,
                                FirstYearDefault = uc.UserDetail.FirstYearDefault,
                                RenewalDefault = uc.UserDetail.RenewalDefault,                               
                                IsNewsToFlash = uc.IsNewsToFlash,
                                Role = (UserRole)uc.MasterRole.RoleId
                            }).ToList();

                //foreach (User usr in userList)
                //{
                //    DLinq.UserDetail userDetail = DataModel.UserDetails.FirstOrDefault(s => s.UserCredentialId == usr.UserCredentialID);
                //    if (userDetail.ZipCode != null)
                //        usr.ZipCode = userDetail.ZipCode.Value.ToString("D5");
                //    else
                //        usr.ZipCode = null;

                //    if (!usr.LicenseeId.IsNullOrEmpty())
                //    {
                //        usr.HouseOwnerId = DataModel.UserCredentials.First(s => s.LicenseeId == usr.LicenseeId && s.IsHouseAccount == true && s.IsDeleted == false).UserCredentialId;
                //        usr.AdminId = DataModel.UserCredentials.First(s => s.LicenseeId == usr.LicenseeId && s.RoleId == 2 && s.IsDeleted == false).UserCredentialId;
                //    }
                //    else
                //    {
                //        usr.HouseOwnerId = Guid.Empty;
                //        usr.AdminId = Guid.Empty;
                //    }
                //}

                return userList;
            }
        }

        public static List<User> GetUsers(UserRole RoleId)
        {
            using (DLinq.CommissionDepartmentEntities DataModel = Entity.DataModel)
            {
                List<User> _usrlist = null;

                _usrlist = (from s in DataModel.UserCredentials
                            where (s.RoleId == (int)RoleId) && (s.IsDeleted == false) && ((4 == (int)RoleId) || s.Licensee.IsDeleted == false)
                            select new User
                            {
                                UserCredentialID = s.UserCredentialId,
                                IsAccountExec = s.IsAccountExec,
                                UserName = s.UserName,
                                Password = s.Password,
                                FirstName = s.UserDetail.FirstName,
                                LastName = s.UserDetail.LastName,
                                Role = (UserRole)s.MasterRole.RoleId,
                                IsNewsToFlash = s.IsNewsToFlash
                            }
                        ).ToList();

                foreach (User usr in _usrlist)
                {
                    if (!usr.LicenseeId.IsNullOrEmpty())
                    {
                        usr.HouseOwnerId = DataModel.UserCredentials.First(s => s.LicenseeId == usr.LicenseeId && s.IsHouseAccount == true && s.IsDeleted == false).UserCredentialId;
                        usr.AdminId = DataModel.UserCredentials.First(s => s.LicenseeId == usr.LicenseeId && s.RoleId == 2 && s.IsDeleted == false).UserCredentialId;
                    }
                    else
                    {
                        usr.HouseOwnerId = Guid.Empty;
                        usr.AdminId = Guid.Empty;
                    }
                }
                return _usrlist;
            }
        }

        public List<User> GetHouseUsers(Guid LincessID, int intRoleID, bool IsHouseAccount)
        {

            int intHouseAccount = 0;

            if (IsHouseAccount)
            {
                intHouseAccount = 1;
            }

            List<User> lstUser = new List<User>();

            try
            {

                DLinq.CommissionDepartmentEntities ctx = new DLinq.CommissionDepartmentEntities(); //create your entity object here
                EntityConnection ec = (EntityConnection)ctx.Connection;
                SqlConnection sc = (SqlConnection)ec.StoreConnection; //get the SQLConnection that your entity object would use
                string adoConnStr = sc.ConnectionString;

                using (SqlConnection con = new SqlConnection(adoConnStr))
                {
                    using (SqlCommand cmd = new SqlCommand("Usp_GetUserList", con))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@LicenseeId", LincessID);
                        cmd.Parameters.AddWithValue("@RoleId", intRoleID);
                        cmd.Parameters.AddWithValue("@IsHouseAccount", intHouseAccount);
                        con.Open();

                        SqlDataReader reader = cmd.ExecuteReader();
                        while (reader.Read())
                        {
                            try
                            {
                                User objUser = new User();

                                try
                                {
                                    if (!string.IsNullOrEmpty(Convert.ToString(reader["UserCredentialID"])))
                                    {
                                        objUser.UserCredentialID = reader["UserCredentialID"] == null ? Guid.Empty : (Guid)reader["UserCredentialID"];
                                    }
                                }
                                catch
                                {
                                }

                                try
                                {
                                    if (!string.IsNullOrEmpty(Convert.ToString(reader["UserName"])))
                                    {
                                        objUser.UserName = reader["UserName"] == null ? string.Empty : Convert.ToString(reader["UserName"]);
                                    }
                                }
                                catch
                                {
                                }

                                try
                                {
                                    if (!string.IsNullOrEmpty(Convert.ToString(reader["Password"])))
                                    {
                                        objUser.Password = Convert.ToString(reader["Password"]);
                                    }
                                }
                                catch
                                {
                                }

                                try
                                {
                                    if (!string.IsNullOrEmpty(Convert.ToString(reader["FirstName"])))
                                    {
                                        objUser.FirstName = reader["FirstName"] == null ? string.Empty : Convert.ToString(reader["FirstName"]);
                                    }
                                }
                                catch
                                {
                                }

                                try
                                {
                                    if (!string.IsNullOrEmpty(Convert.ToString(reader["LastName"])))
                                    {
                                        objUser.LastName = reader["LastName"] == null ? string.Empty : Convert.ToString(reader["LastName"]);
                                    }
                                }
                                catch
                                {
                                }

                                try
                                {
                                    if (!string.IsNullOrEmpty(Convert.ToString(reader["RoleId"])))
                                    {
                                        if (IsHouseAccount)
                                        {
                                            objUser.HouseOwnerId = objUser.UserCredentialID;
                                            objUser.IsHouseAccount = true;
                                            objUser.AdminId = null;

                                        }
                                        else
                                        {
                                            objUser.HouseOwnerId = null;
                                            objUser.IsHouseAccount = false;
                                            objUser.AdminId = null;
                                        }
                                    }
                                }
                                catch
                                {
                                }

                                try
                                {
                                    if (!string.IsNullOrEmpty(Convert.ToString(reader["IsNewsToFlash"])))
                                    {
                                        objUser.IsNewsToFlash = Convert.ToBoolean(reader["IsNewsToFlash"]);
                                    }
                                }
                                catch
                                {
                                }

                                lstUser.Add(objUser);
                            }
                            catch
                            {
                            }

                        }
                        reader.Close();
                    }
                }
            }
            catch
            {
            }

            return lstUser;

        }

        public List<LinkedUser> GetLinkedUser(Guid UserCredentialId, UserRole RoleId, bool isHouseValue)
        {
            List<LinkedUser> LnkUsers = new List<LinkedUser>();
            try
            {
                using (DLinq.CommissionDepartmentEntities DataModel = Entity.DataModel)
                {


                    List<DLinq.UserCredential> connectedLinkedUsers = DataModel.UserCredentials.FirstOrDefault(s => s.UserCredentialId == UserCredentialId).UserCredentials.Where(s => s.IsDeleted == false && s.UserCredentialId != UserCredentialId).ToList();

                    if (connectedLinkedUsers != null && connectedLinkedUsers.Count != 0)
                        LnkUsers = (from usr in connectedLinkedUsers
                                    select new LinkedUser
                                    {
                                        UserId = usr.UserCredentialId,
                                        LastName = usr.UserDetail.LastName,
                                        FirstName = usr.UserDetail.FirstName,
                                        NickName = usr.UserDetail.NickName,
                                        IsConnected = true,
                                        UserName = usr.UserName
                                    }).ToList();

                }

            }

            catch
            {
            }

            return LnkUsers;

        }

        public List<LinkedUser> GetAllLinkedUser(List<User> objUser, Guid GuidLicID, Guid UserCredentialId, int intRole, bool boolHouseAccount)
        {
            using (DLinq.CommissionDepartmentEntities DataModel = Entity.DataModel)
            {
                List<LinkedUser> LnkUsers = null;

                foreach (var item in objUser)
                {
                    if (intRole == 3 && this.IsHouseAccount == boolHouseAccount)
                    {
                        List<DLinq.UserCredential> connectedLinkedUsers = DataModel.UserCredentials.FirstOrDefault(s => s.UserCredentialId == item.UserCredentialID).UserCredentials.Where(s => s.IsDeleted == false && s.UserCredentialId != item.UserCredentialID).ToList();

                        if (connectedLinkedUsers != null && connectedLinkedUsers.Count != 0)
                            LnkUsers = (from usr in connectedLinkedUsers
                                        select new LinkedUser
                                        {
                                            UserId = usr.UserCredentialId,
                                            LastName = usr.UserDetail.LastName,
                                            FirstName = usr.UserDetail.FirstName,
                                            NickName = usr.UserDetail.NickName,
                                            IsConnected = true,
                                            UserName = usr.UserName
                                        }).ToList();

                        List<Guid> connectedUserIds = null;
                        if (LnkUsers == null)
                            connectedUserIds = new List<Guid>();
                        else
                            connectedUserIds = LnkUsers.Select(s => s.UserId).ToList();

                        List<LinkedUser> notConnectedLinkedUsers = (from usr in DataModel.UserCredentials
                                                                    where (!connectedUserIds.Contains(usr.UserCredentialId)) && usr.LicenseeId == item.LicenseeId && usr.RoleId == 3 && (usr.UserCredentialId != item.UserCredentialID) && (usr.IsDeleted == false)
                                                                    select new LinkedUser
                                                                    {
                                                                        UserId = usr.UserCredentialId,
                                                                        LastName = usr.UserDetail.LastName,
                                                                        FirstName = usr.UserDetail.FirstName,
                                                                        NickName = usr.UserDetail.NickName,
                                                                        IsConnected = false,
                                                                        UserName = usr.UserName
                                                                    }).ToList();

                        if (LnkUsers != null)
                            LnkUsers.AddRange(notConnectedLinkedUsers);
                        else if (notConnectedLinkedUsers != null)
                            LnkUsers = notConnectedLinkedUsers;
                    }
                    else
                    {
                        List<DLinq.UserCredential> allUsersExceptHO = DataModel.UserCredentials.Where(s => s.LicenseeId == item.LicenseeId && s.IsDeleted == false && s.RoleId == 3 && s.IsHouseAccount == false).ToList();
                        LnkUsers = (from usr in allUsersExceptHO
                                    select new LinkedUser
                                    {
                                        UserId = usr.UserCredentialId,
                                        LastName = usr.UserDetail.LastName,
                                        FirstName = usr.UserDetail.FirstName,
                                        NickName = usr.UserDetail.NickName,
                                        IsConnected = true,
                                        UserName = usr.UserName
                                    }).ToList();
                    }

                }

                return LnkUsers;
            }
        }

        public static IEnumerable<User> GetAllUsersByLicChunck(Guid LicenseeId, int skip, int take)
        {
            using (DLinq.CommissionDepartmentEntities DataModel = Entity.DataModel)
            {
                IEnumerable<User> userList = null;

                try
                {
                    DataModel.CommandTimeout = 600000000;

                    userList = (from uc in DataModel.UserCredentials
                                where uc.IsDeleted == false && (uc.LicenseeId == LicenseeId)
                                select new User
                                {
                                    UserCredentialID = uc.UserCredentialId,
                                    IsAccountExec = uc.IsAccountExec,
                                    FirstName = uc.UserDetail.FirstName,
                                    LastName = uc.UserDetail.LastName,
                                    City = uc.UserDetail.City,
                                    Company = uc.UserDetail.Company,
                                    State = uc.UserDetail.State,
                                    NickName = uc.UserDetail.NickName,
                                    Email = uc.UserDetail.Email,
                                    Address = uc.UserDetail.Address,
                                    OfficePhone = uc.UserDetail.OfficePhone,
                                    CellPhone = uc.UserDetail.CellPhone,
                                    Fax = uc.UserDetail.Fax,
                                    LicenseeId = uc.Licensee.LicenseeId,
                                    LicenseeName = uc.Licensee.Company,
                                    UserName = uc.UserName,
                                    Password = uc.Password,
                                    DisableAgentEditing = uc.UserDetail.DisableAgentEditing,
                                    PasswordHintQ = uc.PasswordHintQuestion,
                                    PasswordHintA = uc.PasswordHintAnswer,
                                    IsHouseAccount = uc.IsHouseAccount,
                                    FirstYearDefault = uc.UserDetail.FirstYearDefault,
                                    RenewalDefault = uc.UserDetail.RenewalDefault,
                                    Role = (UserRole)uc.MasterRole.RoleId,
                                    IsNewsToFlash = uc.IsNewsToFlash
                                }).ToList();
                }
                catch
                {
                }

                return userList.OrderBy(d => d.NickName).Skip(skip).Take(take).ToList();
            }
        }

        public static List<User> GetAllUsersByChunck(int skip, int take)
        {
            using (DLinq.CommissionDepartmentEntities DataModel = Entity.DataModel)
            {
                DataModel.CommandTimeout = 600000000;

                List<User> _usr = null;

                try
                {

                    _usr = (from uc in DataModel.UserCredentials
                            where uc.IsDeleted == false && uc.UserDetail != null
                            select new User
                            {
                                IsHouseAccount = uc.IsHouseAccount,
                                IsAccountExec = uc.IsAccountExec,
                                UserCredentialID = uc.UserCredentialId,
                                FirstName = uc.UserDetail.FirstName,
                                LastName = uc.UserDetail.LastName,
                                City = uc.UserDetail.City,
                                Company = uc.UserDetail.Company,
                                State = uc.UserDetail.State,
                                NickName = uc.UserDetail.NickName,
                                Email = uc.UserDetail.Email,
                                Address = uc.UserDetail.Address,
                                OfficePhone = uc.UserDetail.OfficePhone,
                                CellPhone = uc.UserDetail.CellPhone,
                                Fax = uc.UserDetail.Fax,
                                LicenseeId = uc.Licensee.LicenseeId,
                                LicenseeName = uc.Licensee.Company,
                                UserName = uc.UserName,
                                Password = uc.Password,
                                DisableAgentEditing = uc.UserDetail.DisableAgentEditing,
                                PasswordHintQ = uc.PasswordHintQuestion,
                                PasswordHintA = uc.PasswordHintAnswer,
                                FirstYearDefault = uc.UserDetail.FirstYearDefault,
                                RenewalDefault = uc.UserDetail.RenewalDefault,
                                Role = (UserRole)uc.MasterRole.RoleId,
                                IsNewsToFlash = uc.IsNewsToFlash
                            }
                            ).ToList();
                }
                catch
                {
                }

                return _usr.OrderBy(d => d.NickName).Skip(skip).Take(take).ToList();
                
            }
        }

        public static IEnumerable<User> GetUsers(Guid LicenseeId)
        {
            using (DLinq.CommissionDepartmentEntities DataModel = Entity.DataModel)
            {
                DataModel.CommandTimeout = 600000000;

                IEnumerable<User> userList = null;
                userList = (from uc in DataModel.UserCredentials
                            where uc.IsDeleted == false && (uc.LicenseeId == LicenseeId)
                            select new User
                            {
                                UserCredentialID = uc.UserCredentialId,
                                IsAccountExec = uc.IsAccountExec,
                                FirstName = uc.UserDetail.FirstName,
                                LastName = uc.UserDetail.LastName,
                                City = uc.UserDetail.City,
                                Company = uc.UserDetail.Company,
                                State = uc.UserDetail.State,
                                NickName = uc.UserDetail.NickName,
                                Email = uc.UserDetail.Email,
                                Address = uc.UserDetail.Address,
                                OfficePhone = uc.UserDetail.OfficePhone,
                                CellPhone = uc.UserDetail.CellPhone,
                                Fax = uc.UserDetail.Fax,
                                LicenseeId = uc.Licensee.LicenseeId,
                                LicenseeName = uc.Licensee.Company,
                                UserName = uc.UserName,
                                Password = uc.Password,
                                DisableAgentEditing = uc.UserDetail.DisableAgentEditing,
                                PasswordHintQ = uc.PasswordHintQuestion,
                                PasswordHintA = uc.PasswordHintAnswer,
                                IsHouseAccount = uc.IsHouseAccount,
                                FirstYearDefault = uc.UserDetail.FirstYearDefault,
                                RenewalDefault = uc.UserDetail.RenewalDefault,
                                Role = (UserRole)uc.MasterRole.RoleId,
                                IsNewsToFlash = uc.IsNewsToFlash
                            }).ToList();
                //foreach (User usr in userList)
                //{
                //    DLinq.UserDetail userDetail = DataModel.UserDetails.FirstOrDefault(s => s.UserCredentialId == usr.UserCredentialID);
                //    if (userDetail.ZipCode != null)
                //        usr.ZipCode = userDetail.ZipCode.Value.ToString("D5");
                //    else
                //        usr.ZipCode = null;

                //    if (!usr.LicenseeId.IsNullOrEmpty())
                //    {
                //        usr.HouseOwnerId = DataModel.UserCredentials.First(s => s.LicenseeId == usr.LicenseeId && s.IsHouseAccount == true && s.IsDeleted == false).UserCredentialId;
                //        usr.AdminId = DataModel.UserCredentials.First(s => s.LicenseeId == usr.LicenseeId && s.RoleId == 2 && s.IsDeleted == false).UserCredentialId;
                //    }
                //    else
                //    {
                //        usr.HouseOwnerId = Guid.Empty;
                //        usr.AdminId = Guid.Empty;
                //    }
                //}
                return userList;
            }
        }

        public static IEnumerable<User> GetUsersForReports(Guid LicenseeId)
        {
            using (DLinq.CommissionDepartmentEntities DataModel = Entity.DataModel)
            {
                DataModel.CommandTimeout = 600000000;

                IEnumerable<User> userList = null;
                userList = (from uc in DataModel.UserCredentials
                            where uc.IsDeleted == false && (uc.LicenseeId == LicenseeId)
                            select new User
                            {
                                UserCredentialID = uc.UserCredentialId,                               
                                FirstName = uc.UserDetail.FirstName,
                                LastName = uc.UserDetail.LastName,
                                NickName = uc.UserDetail.NickName,
                                LicenseeId = uc.Licensee.LicenseeId,                               
                                Role = (UserRole)uc.MasterRole.RoleId
                                
                            }).ToList();
                
                return userList;
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="LoggedInRoleId"></param>
        /// <param name="RoleIdToView"></param>
        /// <returns></returns>
        /// 
        public static void TurnOnNewsToFlashBit()
        {
            using (DLinq.CommissionDepartmentEntities DataModel = Entity.DataModel)
            {
                List<DLinq.UserCredential> userList = DataModel.UserCredentials.Where(uc => uc.IsDeleted == false && (uc.RoleId == 2 || uc.RoleId == 3) && uc.Licensee.IsDeleted == false).ToList();
                foreach (DLinq.UserCredential usr in userList)
                {
                    usr.IsNewsToFlash = true;
                }
                DataModel.SaveChanges();
            }
        }

        public static void TurnOffNewsToFlashBit(Guid userId)
        {
            using (DLinq.CommissionDepartmentEntities DataModel = Entity.DataModel)
            {
                DLinq.UserCredential userCred = DataModel.UserCredentials.FirstOrDefault(s => s.UserCredentialId == userId);
                if (userCred != null)
                    userCred.IsNewsToFlash = false;

                DataModel.SaveChanges();
            }
        }

        public static bool IsUserNameExist(Guid userId, string userName)
        {
            bool userNameExist = false;
            using (DLinq.CommissionDepartmentEntities DataModel = Entity.DataModel)
            {
                if (DataModel.UserCredentials.Any(s => s.UserCredentialId == userId))
                    userNameExist = DataModel.UserCredentials.Any(s => s.UserName.ToUpper() == userName.ToUpper() && s.IsDeleted == false && s.UserCredentialId != userId);
                else
                    userNameExist = DataModel.UserCredentials.Any(s => s.UserName.ToUpper() == userName.ToUpper() && s.IsDeleted == false);
            }
            return userNameExist;
        }

        public void ImportHouseUsers(System.Data.DataTable TempTable, Guid LincessID)
        {
            try
            {
                string UserName = string.Empty;
                string Password = string.Empty;
                string Role = string.Empty;
                int RID = 3;
                string PasswordHintQuestion = string.Empty;
                string PasswordHintAnswer = string.Empty;
                bool IsHouseAccount = false;
                bool IsNewsToFlash = false;
                bool IsAccountExec = false;
                string FirstName = string.Empty;
                string LastName = string.Empty;
                string Company = string.Empty;
                string NickName = string.Empty;
                string Address = string.Empty;
                string ZipCode = string.Empty;
                string City = string.Empty;
                string State = string.Empty;
                string Email = string.Empty;
                string OfficePhone = string.Empty;
                string CellPhone = string.Empty;
                string Fax = string.Empty;
                string FirstYearDefault = string.Empty;
                string RenewalDefault = string.Empty;
                bool ReportForEntireAgency = false;
                bool ReportForOwnBusiness = false;
                string AddPayeeOn = string.Empty;
                bool DisableAgentEditing = false;
                string Moduletodisplay = string.Empty;
                int ModuleId = 11; //DEU
                int AccessRightId = 2;
                string AccessRight = string.Empty;

                if (TempTable != null)
                {
                    int intColIndex = TempTable.Columns.Count - 1;

                    for (int i = 0; i < TempTable.Rows.Count; i++)
                    {
                        if (intColIndex >= 0)
                        {
                            try
                            {
                                UserName = Convert.ToString(TempTable.Rows[i][0]);
                            }
                            catch
                            {
                            }
                        }

                        if (intColIndex >= 1)
                        {
                            try
                            {
                                Password = Convert.ToString(TempTable.Rows[i][1]);

                            }
                            catch
                            {
                            }
                        }

                        if (intColIndex >= 2)
                        {
                            try
                            {
                                Role = Convert.ToString(TempTable.Rows[i][2]);
                                RID = RoleID(Role);
                            }
                            catch
                            {
                            }
                        }

                        if (intColIndex >= 3)
                        {
                            try
                            {
                                PasswordHintQuestion = Convert.ToString(TempTable.Rows[i][3]);

                            }
                            catch
                            {
                            }
                        }

                        if (intColIndex >= 4)
                        {
                            try
                            {
                                PasswordHintAnswer = Convert.ToString(TempTable.Rows[i][4]);

                            }
                            catch
                            {
                            }
                        }

                        if (intColIndex >= 5)
                        {
                            try
                            {
                                IsHouseAccount = trueOrFalse(Convert.ToString(TempTable.Rows[i][5]));
                            }
                            catch
                            {
                            }
                        }

                        if (intColIndex >= 6)
                        {
                            try
                            {
                                IsNewsToFlash = trueOrFalse(Convert.ToString(TempTable.Rows[i][6]));

                            }
                            catch
                            {
                            }
                        }

                        if (intColIndex >= 7)
                        {
                            try
                            {
                                IsAccountExec = trueOrFalse(Convert.ToString(TempTable.Rows[i][7]));

                            }
                            catch
                            {
                            }
                        }

                        if (intColIndex >= 8)
                        {
                            try
                            {
                                FirstName = Convert.ToString(TempTable.Rows[i][8]);

                            }
                            catch
                            {
                            }
                        }

                        if (intColIndex >= 9)
                        {
                            try
                            {
                                LastName = Convert.ToString(TempTable.Rows[i][9]);

                            }
                            catch
                            {
                            }
                        }

                        if (intColIndex >= 10)
                        {
                            try
                            {
                                Company = Convert.ToString(TempTable.Rows[i][10]);

                            }
                            catch
                            {
                            }
                        }

                        if (intColIndex >= 11)
                        {
                            try
                            {
                                NickName = Convert.ToString(TempTable.Rows[i][11]);

                            }
                            catch
                            {
                            }
                        }

                        if (intColIndex >= 12)
                        {
                            try
                            {
                                Address = Convert.ToString(TempTable.Rows[i][12]);

                            }
                            catch
                            {
                            }
                        }

                        if (intColIndex >= 13)
                        {
                            try
                            {
                                ZipCode = Convert.ToString(TempTable.Rows[i][13]);

                            }
                            catch
                            {
                            }
                        }

                        if (intColIndex >= 14)
                        {
                            try
                            {
                                City = Convert.ToString(TempTable.Rows[i][14]);

                            }
                            catch
                            {
                            }
                        }

                        if (intColIndex >= 15)
                        {
                            try
                            {
                                State = Convert.ToString(TempTable.Rows[i][15]);

                            }
                            catch
                            {
                            }
                        }

                        if (intColIndex >= 16)
                        {
                            try
                            {
                                Email = Convert.ToString(TempTable.Rows[i][16]);

                            }
                            catch
                            {
                            }
                        }

                        if (intColIndex >= 17)
                        {
                            try
                            {
                                OfficePhone = Convert.ToString(TempTable.Rows[i][17]);

                            }
                            catch
                            {
                            }
                        }

                        if (intColIndex >= 18)
                        {
                            try
                            {
                                CellPhone = Convert.ToString(TempTable.Rows[i][18]);

                            }
                            catch
                            {
                            }
                        }

                        if (intColIndex >= 19)
                        {
                            try
                            {
                                Fax = Convert.ToString(TempTable.Rows[i][19]);

                            }
                            catch
                            {
                            }
                        }

                        if (intColIndex >= 20)
                        {
                            try
                            {
                                FirstYearDefault = Convert.ToString(TempTable.Rows[i][20]);

                            }
                            catch
                            {
                            }
                        }

                        if (intColIndex >= 21)
                        {
                            try
                            {
                                RenewalDefault = Convert.ToString(TempTable.Rows[i][21]);

                            }
                            catch
                            {
                            }
                        }

                        if (intColIndex >= 22)
                        {
                            try
                            {
                                ReportForEntireAgency = trueOrFalse(Convert.ToString(TempTable.Rows[i][22]));

                            }
                            catch
                            {
                            }
                        }

                        if (intColIndex >= 23)
                        {
                            try
                            {
                                ReportForOwnBusiness = trueOrFalse(Convert.ToString(TempTable.Rows[i][23]));

                            }
                            catch
                            {
                            }
                        }

                        if (intColIndex >= 24)
                        {
                            try
                            {
                                AddPayeeOn = Convert.ToString(TempTable.Rows[i][24]);

                            }
                            catch
                            {
                            }
                        }

                        if (intColIndex >= 25)
                        {
                            try
                            {
                                DisableAgentEditing = trueOrFalse(Convert.ToString(TempTable.Rows[i][25]));

                            }
                            catch
                            {
                            }
                        }

                        if (intColIndex >= 26)
                        {
                            try
                            {
                                Moduletodisplay = Convert.ToString(TempTable.Rows[i][26]);


                            }
                            catch
                            {
                            }
                        }

                        if (intColIndex >= 27)
                        {
                            try
                            {
                                AccessRight = Convert.ToString(TempTable.Rows[i][27]);

                            }
                            catch
                            {
                            }
                        }

                        //Check Nick name available or Not
                        bool bValue = IsNickNameExist(NickName);
                        if (!bValue)
                        {
                            AddUserCredentials(UserName, Password, RID, PasswordHintQuestion, PasswordHintAnswer, IsHouseAccount, IsNewsToFlash,
                            IsAccountExec, LincessID, FirstName, LastName, Company, NickName, Address, ZipCode, City, State, Email,
                            OfficePhone, CellPhone, Fax, FirstYearDefault, RenewalDefault, ReportForEntireAgency, ReportForOwnBusiness,
                            AddPayeeOn, DisableAgentEditing, Moduletodisplay, ModuleId, AccessRightId);
                        }
                    }

                }
            }
            catch
            {
            }

        }

        private int RoleID(string strRole)
        {
            int value = 3;//agent ID

            if (string.IsNullOrEmpty(strRole))
            {
                value = 3;
            }
            else if (strRole.ToLower() == "superadmin")
            {
                value = 1;
            }
            else if (strRole.ToLower() == "administrator")
            {
                value = 2;
            }
            else if (strRole.ToLower() == "agent")
            {
                value = 3;
            }
            else if (strRole.ToLower() == "dep")
            {
                value = 5;
            }
            return value;

        }

        private bool trueOrFalse(string strValue)
        {
            bool bvalue = false;//agent ID

            if (string.IsNullOrEmpty(strValue))
            {
                bvalue = false;
            }
            else if (strValue.ToLower() == "no")
            {
                bvalue = false;
            }
            else if (strValue.ToLower() == "yes")
            {
                bvalue = true;
            }

            return bvalue;

        }

        private bool IsNickNameExist(string strNickName)
        {
            bool bValue = false;

            try
            {
                using (DLinq.CommissionDepartmentEntities DataModel = Entity.DataModel)
                {
                    User _user = (from uc in DataModel.UserDetails

                                  where (uc.NickName.ToLower() == strNickName)
                                  select new User
                                  {
                                      UserCredentialID = uc.UserCredentialId,
                                      IsDeleted = uc.UserCredential.IsDeleted
                                  }
                             ).FirstOrDefault();

                    if (_user == null)
                    {
                        bValue = false;
                    }
                    else
                    {
                        if (_user.IsDeleted != null)
                        {
                            bValue = _user.IsDeleted;

                        }

                    }
                }
            }
            catch 
            {
                bValue = false;
            }
            return bValue;
        }


        public void AddUserCredentials(string UserName, string Password, int RID, string PasswordHintQuestion, string PasswordHintAnswer, bool IsHouseAccount, bool IsNewsToFlash, bool IsAccountExec, Guid LicenseeId, string FirstName,
            string LastName, string Company, string NickName, string Address, string ZipCode, string City, string State, string Email, string OfficePhone,
            string CellPhone, string Fax, string FirstYearDefault, string RenewalDefault, bool ReportForEntireAgency, bool ReportForOwnBusiness, string AddPayeeOn,
            bool DisableAgentEditing, string Moduletodisplay, int ModuleId, int AccessRightId)
        {
            //using (TransactionScope transactionScope = new TransactionScope())
            //{
                try
                {

                    using (DLinq.CommissionDepartmentEntities DataModel = Entity.DataModel)
                    {
                        Guid guidID = Guid.NewGuid();
                        //User credentials
                        try
                        {
                           
                            var _dtUsers = new DLinq.UserCredential();
                            _dtUsers.UserCredentialId = guidID;
                            _dtUsers.UserName = UserName;
                            _dtUsers.Password = Password;
                            _dtUsers.PasswordHintAnswer = PasswordHintQuestion;
                            _dtUsers.PasswordHintQuestion = PasswordHintAnswer;
                            _dtUsers.RoleId = RID;
                            _dtUsers.LicenseeId = LicenseeId;
                            _dtUsers.CreatedOn = DateTime.Now;
                            DataModel.AddToUserCredentials(_dtUsers);
                        }
                        catch
                        {
                        }

                        //User details
                        try
                        {

                            var _dtUserDetail = new DLinq.UserDetail();
                            _dtUserDetail.UserCredentialId = guidID;
                            _dtUserDetail.FirstName = FirstName;
                            _dtUserDetail.LastName = LastName;
                            _dtUserDetail.Company = Company;
                            _dtUserDetail.NickName = NickName;
                            if (Address.Length > 50)
                            {
                                string strAddress = Address;
                                strAddress = strAddress.Substring(0, 49);
                                _dtUserDetail.Address = strAddress;
                            }
                            else
                            {
                                _dtUserDetail.Address = Address;
                            }
                            if (!string.IsNullOrEmpty(ZipCode))
                            {
                                _dtUserDetail.ZipCode = Convert.ToInt32(ZipCode);
                            }

                            _dtUserDetail.City = City;
                            _dtUserDetail.State = State;
                            _dtUserDetail.Email = Email;
                            _dtUserDetail.OfficePhone = OfficePhone;
                            _dtUserDetail.CellPhone = CellPhone;
                            _dtUserDetail.Fax = Fax;
                            _dtUserDetail.FirstYearDefault = Convert.ToDouble(FirstYearDefault);
                            _dtUserDetail.RenewalDefault = Convert.ToDouble(RenewalDefault);
                            _dtUserDetail.ReportForEntireAgency = ReportForEntireAgency;
                            _dtUserDetail.ReportForOwnBusiness = ReportForOwnBusiness;
                            _dtUserDetail.AddPayeeOn = DateTime.Now;
                            _dtUserDetail.DisableAgentEditing = DisableAgentEditing;
                            DataModel.AddToUserDetails(_dtUserDetail);
                        }
                        catch
                        {
                        }
                        //permission
                        try
                        {
                            var _permision = new DLinq.UserPermission();
                            _permision.UserPermissionId = Guid.NewGuid();
                            _permision.UserCredentialId = guidID;
                            _permision.ModuleId = ModuleId;
                            _permision.AccessRightId = AccessRightId;
                            DataModel.AddToUserPermissions(_permision);
                        }
                        catch 
                        {
                        }
                        
                        DataModel.SaveChanges();

                        //transactionScope.Complete();
                    }
                }
                catch 
                {
                }
                
            //}
        }

    }
}