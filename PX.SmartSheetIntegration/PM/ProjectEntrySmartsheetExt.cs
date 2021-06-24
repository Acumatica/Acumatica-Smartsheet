using PX.Data;
using PX.Objects.EP;
using PX.Objects.PM;
using PX.SM;
using Smartsheet.Api;
using Smartsheet.Api.Models;
using Smartsheet.Api.OAuth;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace SmartSheetIntegration
{
    public class ProjectEntrySmartsheetExt : PXGraphExtension<ProjectEntry>
    {
        public override void Initialize()
        {
            base.Initialize();
            this.Base.action.AddMenuAction(synGanttSmartsheetProject);
        }

        #region DataMembers
        public PXSelect<EPUsersListSS> UserSSList;

        public PXSelect<
            PMSubTask,
            Where<PMSubTask.projectID, Equal<Current<PMTask.projectID>>,
                And<PMSubTask.taskID, Equal<Current<PMTask.taskID>>>>,
            OrderBy<
                Asc<PMSubTask.position>>>
            PMSubTask;
        #endregion

        #region Events

        #region PMProject
        protected virtual void PMProject_RowSelected(PXCache sender, PXRowSelectedEventArgs e)
        {
            if (e.Row == null)
            {
                return;
            }

            PMProject pmProjectRow = (PMProject)e.Row;
            PMProjectSSExt pmProjectSSExtRow = PXCache<PMProject>.GetExtension<PMProjectSSExt>(pmProjectRow);

            pmProjectSSExtRow.SyncedInSmartsheet = pmProjectSSExtRow.UsrSmartsheetContractID != null;

            Dictionary<string, string> fields = new Dictionary<string, string>();

            foreach (PMTemplateListSS pmTemplateListSSRow in PXSelectJoinGroupBy<
                PMTemplateListSS,
                InnerJoin<PMSSMapping,
                    On<PMSSMapping.templateSS, Equal<PMTemplateListSS.templateSS>,
                    And<PMSSMapping.nameAcu, IsNotNull,
                    And<PMSSMapping.nameAcu, NotEqual<Required<PMSSMapping.nameAcu>>>>>>,
                Aggregate<
                    GroupBy<PMTemplateListSS.templateSS>>>
                .Select(this.Base, ""))
            {
                fields.Add(pmTemplateListSSRow.TemplateSS, pmTemplateListSSRow.TemplateName);
            }
            PXStringListAttribute.SetList<PMProjectSSExt.usrTemplateSS>(sender, pmProjectRow, fields.Keys.ToArray(), fields.Values.ToArray());
            PXUIFieldAttribute.SetEnabled<PMProjectSSExt.usrTemplateSS>(sender, pmProjectRow, !(bool)pmProjectSSExtRow.SyncedInSmartsheet);
        }
        #endregion

        #region PMTask
        protected virtual void PMTask_Duration_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
        {
            if (e.Row == null)
            {
                return;
            }

            PMTask pmTaskRow = (PMTask)e.Row;
            PMTaskSSExt pmTaskExtRow = PXCache<PMTask>.GetExtension<PMTaskSSExt>(pmTaskRow);

            if (pmTaskExtRow.Duration != null
                    && pmTaskExtRow.Duration >= 1
                    && pmTaskRow.StartDate != null
                    && e.OldValue != null)
            {
                SmartsheetHelper smartSheetHelperObject = new SmartsheetHelper();
                DateTime endDateRow = smartSheetHelperObject.CalculateWorkingDays((DateTime)pmTaskRow.StartDate, (int)pmTaskExtRow.Duration);
                pmTaskRow.EndDate = endDateRow;
            }
        }

        protected virtual void PMTask_StartDate_FieldUpdating(PXCache sender, PXFieldUpdatingEventArgs e)
        {
            if (e.Row == null)
            {
                return;
            }

            PMTask pmTaskRow = (PMTask)e.Row;
            PMTaskSSExt pmTaskExtRow = PXCache<PMTask>.GetExtension<PMTaskSSExt>(pmTaskRow);

            if (pmTaskExtRow.Duration != null
                    && pmTaskExtRow.Duration >= 1
                    && e.NewValue != null)
            {
                SmartsheetHelper smartSheetHelperObject = new SmartsheetHelper();
                DateTime endDateRow = smartSheetHelperObject.CalculateWorkingDays((DateTime)e.NewValue, (int)pmTaskExtRow.Duration);
                pmTaskRow.EndDate = endDateRow;
            }
        }
        #endregion

        #endregion

        #region Actions

        public PXAction<PMProject> synGanttSmartsheetProject;
        [PXUIField(DisplayName = SmartsheetConstants.ActionsNames.SYNC_SMARTSHEET_PROJECT, MapEnableRights = PXCacheRights.Update, MapViewRights = PXCacheRights.Update)]
        [PXProcessButton(CommitChanges = true)]
        public virtual IEnumerable SynGanttSmartsheetProject(PXAdapter adapter)
        {
            this.Base.Actions.PressSave();
            PMProject pmProjectRow = this.Base.Project.Current;

            ProjectEntry projectEntryGraph = PXGraph.CreateInstance<ProjectEntry>();

            SmartsheetClient smartsheetClient = CheckTokenSheetSS(projectEntryGraph, pmProjectRow);

            if (smartsheetClient != null)
            {
                PXLongOperation.StartOperation(this.Base,
                () =>
                {
                    using (PXTransactionScope ts = new PXTransactionScope())
                    {
                        projectEntryGraph = this.Base;
                        ProjectEntrySmartsheetExt graphExtended = projectEntryGraph.GetExtension<ProjectEntrySmartsheetExt>();
                        CreateEmployeesAcuUserSS(projectEntryGraph, smartsheetClient);
                        graphExtended.CreateUpdateGanttProject(projectEntryGraph, pmProjectRow, smartsheetClient);
                        ts.Complete();
                    }

                });
            }

            return adapter.Get();
        }

        public PXAction<PMProject> addSSUser;
        [PXUIField(DisplayName = SmartsheetConstants.ActionsNames.SYNC_SMARTSHEET_EMPLOYEES, MapEnableRights = PXCacheRights.Update, MapViewRights = PXCacheRights.Update, Visible = false)]
        [PXButton(CommitChanges = true, OnClosingPopup = PXSpecialButtonType.Cancel)]
        public virtual IEnumerable AddSSUser(PXAdapter adapter)
        {
            PMProject pmProjectRow = this.Base.Project.Current;

            ProjectEntry projectEntryGraph = PXGraph.CreateInstance<ProjectEntry>();
            SmartsheetClient smartsheetClient = CheckTokenSheetSS(projectEntryGraph, pmProjectRow);

            PXLongOperation.StartOperation(this.Base, delegate ()
            {
                using (PXTransactionScope ts = new PXTransactionScope())
                {
                    CreateEmployeesAcuUserSS(projectEntryGraph, smartsheetClient);
                    ts.Complete();
                }
                PXLongOperation.SetCustomInfo(this.Base);
            });
            return adapter.Get();
        }

        public PXAction<PMProject> populateDates;
        [PXProcessButton(CommitChanges = true)]
        [PXUIField(DisplayName = SmartsheetConstants.ActionsNames.POPULATE_DATES, MapViewRights = PXCacheRights.Update, MapEnableRights = PXCacheRights.Update)]
        protected virtual IEnumerable PopulateDates(PXAdapter adapter)
        {
            PMSetup setupRecord = this.Base.Setup.Select();
            PMSetupSSExt setupRecordExt = PXCache<PMSetup>.GetExtension<PMSetupSSExt>(setupRecord);
            if (!String.IsNullOrEmpty(setupRecordExt.UsrTypeTaskDate) && setupRecordExt.UsrDurationTaskDate != null)
            {
                foreach (PMTask filter in this.Base.Tasks.Select())
                {
                    PMTaskSSExt pMTaskRecordExt = PXCache<PMTask>.GetExtension<PMTaskSSExt>(filter);

                    if (String.IsNullOrEmpty(filter.StartDate.ToString()))
                    {
                        filter.StartDate = DateTime.Today;
                    }
                    if (String.IsNullOrEmpty(filter.EndDate.ToString()))
                    {
                        switch (setupRecordExt.UsrTypeTaskDate)
                        {
                            case SmartsheetConstants.SSConstants.DAY:
                                SmartsheetHelper smartSheetHelperObject = new SmartsheetHelper();
                                filter.EndDate = smartSheetHelperObject.CalculateWorkingDays((DateTime)filter.StartDate, (int)setupRecordExt.UsrDurationTaskDate);
                                break;
                            case SmartsheetConstants.SSConstants.MONTH:
                                filter.EndDate = ((DateTime)filter.StartDate).AddMonths((int)setupRecordExt.UsrDurationTaskDate);
                                break;
                            case SmartsheetConstants.SSConstants.YEAR:
                                filter.EndDate = ((DateTime)filter.StartDate).AddYears((int)setupRecordExt.UsrDurationTaskDate);
                                break;
                        }
                    }
                    this.Base.Tasks.Cache.Update(filter);
                }
            }
            else
            {
                throw new PXException(SmartsheetConstants.Messages.DURATION_FIELDS_NOT_INDICATED);
            }

            return adapter.Get();
        }

        #endregion

        #region Methods

        /// <summary>
        /// Creates or updates the Smartsheet project with information from Acumatica.
        /// New Tasks created in Smartsheet will be updated back in Acumatica
        /// </summary>
        /// <param name="projectEntryGraph">Project Entry graph</param>
        /// <param name="pmProjectRow">PMProject record</param>
        /// <param name="smartsheetClient">Smartsheet's SDK Client</param>
        /// <param name="isMassProcess">Indicates if it's used in a processing page</param>
        public void CreateUpdateGanttProject(ProjectEntry projectEntryGraph, PMProject pmProjectRow, SmartsheetClient smartsheetClient, bool isMassProcess = false)
        {
            //Primary Data View is set
            projectEntryGraph.Project.Current = pmProjectRow;
            PMProjectSSExt pmProjectSSExt = PXCache<PMProject>.GetExtension<PMProjectSSExt>(pmProjectRow);

            PMSetup setupRecord = projectEntryGraph.Setup.Select();
            PMSetupSSExt pmSetupSSExt = PXCache<PMSetup>.GetExtension<PMSetupSSExt>(setupRecord);

            SmartsheetHelper smartSheetHelperObject = new SmartsheetHelper();
            if (String.IsNullOrEmpty(pmProjectSSExt.UsrTemplateSS))
            {
                throw new PXException(SmartsheetConstants.Messages.DEFAULT_TEMPLATE);
            }

            string sheetName = pmProjectRow.ContractCD.Trim() + " - " + pmProjectRow.Description.Trim();
            sheetName = smartSheetHelperObject.Left(sheetName, SmartsheetConstants.SSConstants.SS_PROJECT_NAME_LENGTH);

            try
            {
                long? sheetSelected;
                Dictionary<string, long> currentColumnMap = new Dictionary<string, long>();

                //////////////////
                //Information from Acumatica is updated in Smartsheet
                //////////////////

                PXResultset<PMSSMapping> templateMappingSet = PXSelect<
                    PMSSMapping,
                    Where<PMSSMapping.templateSS, Equal<Required<PMSSMapping.templateSS>>>>
                    .Select(projectEntryGraph, pmProjectSSExt.UsrTemplateSS);

                if (pmProjectSSExt != null
                        && pmProjectSSExt.UsrSmartsheetContractID != null) //Acumatica Project is already linked to SS
                {
                    sheetSelected = pmProjectSSExt.UsrSmartsheetContractID;

                    Sheet ssProjectSheet = smartsheetClient.SheetResources.GetSheet((long)sheetSelected, null, null, null, null, null, null, null);

                    //Columns ID Mapping
                    currentColumnMap = new Dictionary<string, long>();
                    foreach (Column currentColumn in ssProjectSheet.Columns)
                    {
                        currentColumnMap.Add(currentColumn.Title, (long)currentColumn.Id);
                    }

                    smartSheetHelperObject.UpdateSSProject(smartsheetClient, currentColumnMap, projectEntryGraph, sheetSelected, smartSheetHelperObject, templateMappingSet);
                }
                else //Acumatica Project has not been linked to Smartsheet
                {
                    //Sheet is created from a the template selected in the Project
                    Sheet sheet = new Sheet.CreateSheetFromTemplateBuilder(sheetName, Convert.ToInt64(pmProjectSSExt.UsrTemplateSS)).Build();
                    sheet = smartsheetClient.SheetResources.CreateSheet(sheet);

                    Sheet newlyCreatedSheet = smartsheetClient.SheetResources.GetSheet((long)sheet.Id, null, null, null, null, null, null, null);

                    currentColumnMap = new Dictionary<string, long>();
                    foreach (Column currentColumn in newlyCreatedSheet.Columns)
                    {
                        currentColumnMap.Add(currentColumn.Title, (long)currentColumn.Id);
                    }

                    //Acumatica Tasks are added as Smartsheet rows
                    List<Row> newSSRows = smartSheetHelperObject.InsertAcumaticaTasksInSS(projectEntryGraph, currentColumnMap, null, true, templateMappingSet);
                    IList<Row> ssRows = smartsheetClient.SheetResources.RowResources.AddRows((long)sheet.Id, newSSRows);

                    int ssTaskIDPosition = 0;
                    if (ssRows.Count > 0 && ssRows[0].Cells != null)
                    {
                        PMSSMapping mappingSS = templateMappingSet.Where(t => ((PMSSMapping)t).NameAcu.Trim().ToUpper() == SmartsheetConstants.ColumnMapping.TASKS_CD.Trim().ToUpper()).FirstOrDefault();

                        ssTaskIDPosition = smartSheetHelperObject.GetSSTaskPosition(ssRows[0].Cells, currentColumnMap[mappingSS.NameSS]);
                    }

                    // Add SubTasks
                    smartSheetHelperObject.InsertAcumaticaSubTasks(projectEntryGraph, smartsheetClient, sheet, ssRows, smartSheetHelperObject, currentColumnMap, ssTaskIDPosition, templateMappingSet);

                    foreach (Row currentRow in ssRows)
                    {
                        foreach (PMTask rowTask in projectEntryGraph.Tasks.Select())
                        {
                            if (currentRow.Cells[ssTaskIDPosition].Value != null
                                    && rowTask.TaskCD != null
                                    && string.Equals(currentRow.Cells[ssTaskIDPosition].Value.ToString().Trim(), rowTask.TaskCD.Trim(), StringComparison.OrdinalIgnoreCase))
                            {
                                PMTaskSSExt pmTaskSSExtRow = PXCache<PMTask>.GetExtension<PMTaskSSExt>(rowTask);
                                pmTaskSSExtRow.UsrSmartsheetTaskID = currentRow.Id;
                                projectEntryGraph.Tasks.Update(rowTask);
                                break;
                            }
                        }
                    }

                    sheetSelected = (long)sheet.Id;

                    PMProjectSSExt pmProjectSSExtRow = PXCache<PMProject>.GetExtension<PMProjectSSExt>(pmProjectRow);
                    pmProjectSSExtRow.UsrSmartsheetContractID = sheetSelected;
                    projectEntryGraph.Project.Update(pmProjectRow);
                }


                //////////////////
                //Information from Smartsheet is updated in Acumatica
                //////////////////
                Sheet updatedSheet = smartsheetClient.SheetResources.GetSheet((long)sheetSelected, null, null, null, null, null, null, null);

                int columnPosition = 0;
                Dictionary<string, int> columnPositionMap = new Dictionary<string, int>();
                foreach (Column currentColumn in updatedSheet.Columns)
                {
                    columnPositionMap.Add(currentColumn.Title, columnPosition);
                    columnPosition += 1;
                }

                smartSheetHelperObject.UpdateAcumaticaTasks(projectEntryGraph, pmProjectRow, pmSetupSSExt, updatedSheet, columnPositionMap, templateMappingSet);

                projectEntryGraph.Actions.PressSave();

                if (isMassProcess)
                {
                    PXProcessing.SetInfo(String.Format(SmartsheetConstants.Messages.SUCCESSFULLY_SYNCED, pmProjectRow.ContractCD));
                }
            }
            catch (Exception e)
            {
                throw new PXException(e.Message);
            }
        }

        /// <summary>
        /// Verifies the connection Token and the existence of the Sheet
        /// </summary>
        /// <param name="projectEntryGraph">Project's Graph</param>
        /// <param name="pmProjectRow">Current PMProject row</param>
        /// <param name="refreshedToken">Existing token</param>
        /// <param name="isMassProcess">Indicates if it's used in a processing page</param>
        /// <returns></returns>
        public SmartsheetClient CheckTokenSheetSS(ProjectEntry projectEntryGraph, PMProject pmProjectRow, string refreshedToken = "", bool isMassProcess = false)
        {
            //Primary Data View is set
            projectEntryGraph.Project.Current = pmProjectRow;

            SmartsheetHelper smartSheetHelperObject = new SmartsheetHelper();

            string sheetName = pmProjectRow.ContractCD.Trim() + " - " + pmProjectRow.Description.Trim();
            sheetName = smartSheetHelperObject.Left(sheetName, SmartsheetConstants.SSConstants.SS_PROJECT_NAME_LENGTH);

            PMSetup setupRecord = projectEntryGraph.Setup.Select();
            PMSetupSSExt pmSetupSSExt = PXCache<PMSetup>.GetExtension<PMSetupSSExt>(setupRecord);
            PMProjectSSExt pmProjectSSExt = PXCache<PMProject>.GetExtension<PMProjectSSExt>(pmProjectRow);

            Users userRecord = PXSelect<
                Users,
                Where<Users.pKID, Equal<Required<AccessInfo.userID>>>>
                .Select(projectEntryGraph, projectEntryGraph.Accessinfo.UserID);
            UsersSSExt userRecordSSExt = PXCache<Users>.GetExtension<UsersSSExt>(userRecord);

            smartSheetHelperObject.SetupValidation(userRecordSSExt, pmSetupSSExt);
            smartSheetHelperObject.ProjectValidation(projectEntryGraph);

            Token token = new Token();

            token.AccessToken = (String.IsNullOrEmpty(refreshedToken)) ? userRecordSSExt.UsrSmartsheetToken : refreshedToken;

            try
            {
                SmartsheetClient smartsheetClient = new SmartsheetBuilder()
                                                        .SetAccessToken(token.AccessToken)
                                                        .SetDateTimeFixOptOut(true)
                                                        .Build();

                long? sheetSelected;
                Dictionary<string, long> currentColumnMap = new Dictionary<string, long>();

                if (pmProjectSSExt != null
                        && pmProjectSSExt.UsrSmartsheetContractID != null) //Acumatica Project is already linked to SS
                {
                    sheetSelected = pmProjectSSExt.UsrSmartsheetContractID;

                    Sheet ssProjectSheet = smartsheetClient.SheetResources.GetSheet((long)sheetSelected, null, null, null, null, null, null, null);

                }

                return smartsheetClient;
            }
            catch (Exception ex)
            {
                if (ex.Message.Contains(SmartsheetConstants.SSConstants.EXPIRED_TOKEN_MESSAGE))
                {
                    MyProfileMaint profileMaintGraph = PXGraph.CreateInstance<MyProfileMaint>();
                    MyProfileMaintExt graphExtended = profileMaintGraph.GetExtension<MyProfileMaintExt>();
                    string updatedToken = graphExtended.RefreshSmartsheetToken();
                    CheckTokenSheetSS(projectEntryGraph, pmProjectRow, updatedToken);
                }

                if (ex.Message.Contains(SmartsheetConstants.SSConstants.NOTFOUND_PROJECT_MESSAGE))
                {
                    if (isMassProcess)
                    {
                        SmartsheetClient smartsheetClient = new SmartsheetBuilder()
                                                                .SetAccessToken(token.AccessToken)
                                                                .SetDateTimeFixOptOut(true)
                                                                .Build();
                        UnlinkSmartsheetProject(projectEntryGraph);
                        CreateUpdateGanttProject(projectEntryGraph, pmProjectRow, smartsheetClient, true);
                    }
                    else
                    {
                        UnlinkSmartsheetProject(projectEntryGraph);
                        throw new PXException(SmartsheetConstants.Messages.UNLINK_PROJECT);
                    }
                }
                else
                {
                    throw new PXException(ex.Message);
                }

                return null;
            }
        }

        /// <summary>
        /// Links the Smartsheet user information to the Acumatica's Employee
        /// </summary>
        /// <param name="projectEntryGraph">Project graph</param>
        /// <param name="smartsheetClient">Smartsheet SDK Client</param>
        public void CreateEmployeesAcuUserSS(ProjectEntry projectEntryGraph, SmartsheetClient smartsheetClient)
        {
            User userRow = new User();
            AccessUsers accessUsersGraph = PXGraph.CreateInstance<AccessUsers>();
            AccessUsersSSIExt graphExtended = accessUsersGraph.GetExtension<AccessUsersSSIExt>();

            try
            {
                EmployeeMaint employeeMaintGraph = PXGraph.CreateInstance<EmployeeMaint>();
                foreach (EPEmployeeContract filter in projectEntryGraph.EmployeeContract.Select())
                {
                    EPEmployee ePEmployeeRecord = employeeMaintGraph.Employee.Current = employeeMaintGraph.Employee.Search<EPEmployee.bAccountID>(filter.EmployeeID);
                    if (ePEmployeeRecord == null) { return; }
                    EPEmployeeSSExt ePEmployeeExtRecord = PXCache<EPEmployee>.GetExtension<EPEmployeeSSExt>(ePEmployeeRecord);
                    if (String.IsNullOrEmpty(Convert.ToString(ePEmployeeExtRecord.UsrSSUserid)))
                    {
                        userRow = graphExtended.CreateSmartsheetUser(filter.EmployeeID, smartsheetClient);
                        if (userRow != null)
                        {
                            PXCache<EPUsersListSS> usersListCache = employeeMaintGraph.Caches<EPUsersListSS>();

                            EPUsersListSS epUsersListSSRow = UserSSList.Select().Where(x => ((EPUsersListSS)x).Ssuserid == userRow.Id).FirstOrDefault();
                            if (epUsersListSSRow == null)
                            {
                                EPUsersListSS usersListSSRow = (EPUsersListSS)usersListCache.Insert(new EPUsersListSS
                                {
                                    Ssuserid = userRow.Id,
                                    FirstName = userRow.FirstName,
                                    LastName = userRow.LastName,
                                    Email = userRow.Email,
                                    BAccountID = filter.EmployeeID
                                });
                            }
                            else
                            {
                                epUsersListSSRow.BAccountID = filter.EmployeeID;
                                epUsersListSSRow.FirstName = userRow.FirstName;
                                epUsersListSSRow.LastName = userRow.LastName;
                                epUsersListSSRow.Email = userRow.Email;
                                usersListCache.Update(epUsersListSSRow);
                            }
                            employeeMaintGraph.Persist();

                            PXDatabase.Update<EPEmployee>(new PXDataFieldRestrict<EPEmployee.bAccountID>(filter.EmployeeID),
                                                            new PXDataFieldAssign<EPEmployeeSSExt.usrSSUserID>(userRow.Id));
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new PXException(ex.Message);
            }
        }

        /// <summary>
        /// Unlinks the Acumatica project from a Smartsheet reference
        /// </summary>
        /// <param name="projectEntryGraph"></param>
        public void UnlinkSmartsheetProject(ProjectEntry projectEntryGraph)
        {
            PMProject pmProjectRow = projectEntryGraph.Project.Current;

            if (pmProjectRow != null)
            {
                PMProjectSSExt pmProjectExtRow = PXCache<PMProject>.GetExtension<PMProjectSSExt>(pmProjectRow);
                pmProjectExtRow.UsrSmartsheetContractID = null;
                foreach (PMTask pmTaskRow in projectEntryGraph.Tasks.Select())
                {
                    PMTaskSSExt pmTaskRowExt = PXCache<PMTask>.GetExtension<PMTaskSSExt>(pmTaskRow);
                    pmTaskRowExt.UsrSmartsheetTaskID = null;
                    projectEntryGraph.Tasks.Cache.Update(pmTaskRow);
                }

                projectEntryGraph.Project.Cache.Update(pmProjectRow);
                projectEntryGraph.Persist();
            }
        }
        #endregion

    }
}
