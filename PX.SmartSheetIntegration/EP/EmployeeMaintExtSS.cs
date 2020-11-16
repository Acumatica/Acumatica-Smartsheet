using PX.Data;
using PX.Objects.EP;
using PX.SM;
using System;
using Smartsheet.Api;
using Smartsheet.Api.Models;
using Smartsheet.Api.OAuth;
using System.Linq;

namespace SmartSheetIntegration
{
    public class EmployeeMaintExtSS : PXGraphExtension<EmployeeMaint>
    {
        public override void Initialize()
        {
            base.Initialize();
            Base.Action.AddMenuAction(this.EmployeeUserSS);
        }

        #region DataMembers

        public PXSelect<EPUsersListSS> UserList;

        #endregion

        #region Methods
        /// <summary>
        /// Creates or updates users obtained from SmartSheet in Acumatica
        /// </summary>
        /// <param name="refreshedToken"></param>
        public void GetUsersSS(string refreshedToken = "")
        {
            // User connection with SmartSheet
            Users userRecord = PXSelect<
                Users,
                Where<Users.pKID, Equal<Required<AccessInfo.userID>>>>
                .Select(this.Base, this.Base.Accessinfo.UserID);

            if (userRecord == null)
            {
                throw new PXException(SmartsheetConstants.Messages.ERROR_USER);
            }
            UsersSSExt userRecordSSExt = PXCache<Users>.GetExtension<UsersSSExt>(userRecord);

            try
            {
                Token token = new Token();

                token.AccessToken = (String.IsNullOrEmpty(refreshedToken)) ? userRecordSSExt.UsrSmartsheetToken : refreshedToken;

                SmartsheetClient smartsheetClient = new SmartsheetBuilder().SetAccessToken(token.AccessToken).Build();
                PaginatedResult<User> smartsheetUserSet = smartsheetClient.UserResources.ListUsers(null, null);
                if (smartsheetUserSet.TotalCount > 0)
                {
                    PXCache<EPUsersListSS> usersListCache = this.Base.Caches<EPUsersListSS>();
                    foreach (User dataUsers in smartsheetUserSet.Data)
                    {
                        EPUsersListSS epUsersListSSRow = UserList.Select().Where(x => ((EPUsersListSS)x).Ssuserid == dataUsers.Id).FirstOrDefault();
                        if (epUsersListSSRow == null)
                        {
                            EPUsersListSS ci = (EPUsersListSS)usersListCache.Insert(new EPUsersListSS
                            {
                                Ssuserid = dataUsers.Id,
                                FirstName = dataUsers.FirstName,
                                LastName = dataUsers.LastName,
                                Email = dataUsers.Email
                            }); 
                        }
                        else
                        {
                            epUsersListSSRow.FirstName = dataUsers.FirstName;
                            epUsersListSSRow.LastName = dataUsers.LastName;
                            epUsersListSSRow.Email = dataUsers.Email;
                            usersListCache.Update(epUsersListSSRow);
                        }
                    }
                    this.Base.Persist();
                }
            }
            catch (Exception e)
            {
                if (e.Message.Contains(SmartsheetConstants.SSConstants.EXPIRED_TOKEN_MESSAGE))
                {
                    MyProfileMaint profileMaintGraph = PXGraph.CreateInstance<MyProfileMaint>();
                    MyProfileMaintExt graphExtended = profileMaintGraph.GetExtension<MyProfileMaintExt>();
                    string updatedToken = graphExtended.RefreshSmartsheetToken();
                    GetUsersSS(updatedToken);
                }
                else
                {
                    throw new PXException(e.Message);
                }
            }
        }
        #endregion

        #region Action
        public PXAction<EPEmployee> EmployeeUserSS;
        [PXUIField(DisplayName = SmartsheetConstants.ActionsNames.EMPLOYEE_USER_SMARTSHEET, MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
        [PXButton(CommitChanges = true)]
        public virtual void employeeUserSS() => GetUsersSS();
        #endregion

        #region Events
        protected virtual void EPEmployee_UsrSSUserID_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
        {
            if (e.Row == null)
            {
                return;
            }

            EPEmployee epEmployeeRow = (EPEmployee)e.Row;
            EPEmployeeSSExt epEmployeeExtRow = PXCache<EPEmployee>.GetExtension<EPEmployeeSSExt>(epEmployeeRow);

            if ((long?)e.OldValue != epEmployeeExtRow.UsrSSUserid)
            {
                PXCache<EPUsersListSS> usersListCache = this.Base.Caches<EPUsersListSS>();
                EPUsersListSS epUsersListSSRow = UserList.Select().Where(x => ((EPUsersListSS)x).Ssuserid == (long?)e.OldValue).FirstOrDefault();
                if (epUsersListSSRow != null)
                {
                    epUsersListSSRow.BAccountID = null;
                    usersListCache.Update(epUsersListSSRow);
                }

                if (epEmployeeExtRow.UsrSSUserid != null)
                {
                    epUsersListSSRow = UserList.Select().Where(x => ((EPUsersListSS)x).Ssuserid == epEmployeeExtRow.UsrSSUserid).FirstOrDefault();
                    if (epUsersListSSRow != null)
                    {
                        epUsersListSSRow.BAccountID = epEmployeeRow.BAccountID;
                        usersListCache.Update(epUsersListSSRow);
                    }
                }
            }
        }
        #endregion
    }
}
