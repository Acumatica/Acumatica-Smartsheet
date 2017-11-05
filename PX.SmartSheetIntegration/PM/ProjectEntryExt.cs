using System.Collections;
using PX.Data;
using PX.Objects.PM;
using System;
using System.Collections.Generic;
using Smartsheet.Api.Models;
using Smartsheet.Api;
using Smartsheet.Api.OAuth;
using PX.SM;

namespace PX.SmartSheetIntegration
{
    public class ProjectEntryExt : PXGraphExtension<ProjectEntry>
    {
        public override void Initialize()
        {
            this.Base.action.AddMenuAction(synGanttSmartsheetProject);
        }

        #region Events
        protected virtual void PMProject_RowSelected(PXCache sender, PXRowSelectedEventArgs e)
        {
            if (e.Row == null)
            {
                return;
            }

            PMProject pmProjectRow = (PMProject)e.Row;
            PMProjectSSExt pmProjectSSExtRow = PXCache<PMProject>.GetExtension<PMProjectSSExt>(pmProjectRow);

            pmProjectSSExtRow.SyncedInSmartsheet = pmProjectSSExtRow.UsrSmartsheetContractID != null;
        }
        #endregion

        #region Actions
        public PXAction<PMProject> synGanttSmartsheetProject;
        [PXProcessButton(CommitChanges = true)]
        [PXUIField(DisplayName = SmartsheetConstants.ActionsNames.SYNC_SMARTSHEET_PROJECT, MapViewRights = PXCacheRights.Update, MapEnableRights = PXCacheRights.Update)]
        protected virtual IEnumerable SynGanttSmartsheetProject(PXAdapter adapter)
        {
            PMProject pmProjectRow = this.Base.Project.Current;

            this.Base.Actions.PressSave();
            PXLongOperation.StartOperation(this.Base,
                delegate ()
                {
                    using (PXTransactionScope ts = new PXTransactionScope())
                    {
                        ProjectEntry projectEntryGraph = PXGraph.CreateInstance<ProjectEntry>();
                        ProjectEntryExt graphExtended = projectEntryGraph.GetExtension<ProjectEntryExt>();
                        graphExtended.CreateUpdateGanttProject(projectEntryGraph, pmProjectRow);

                        ts.Complete();
                    }
                });

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
        /// <param name="isMassProcess">Indicates if it's used in a processing page</param>
        public void CreateUpdateGanttProject(ProjectEntry projectEntryGraph, PMProject pmProjectRow, string refreshedToken = "", bool isMassProcess = false)
        {
            //Primary Data View is set
            projectEntryGraph.Project.Current = pmProjectRow;

            SmartsheetHelper smartSheetHelperObject = new SmartsheetHelper();

            string sheetName = pmProjectRow.ContractCD.Trim() + " - " + pmProjectRow.Description.Trim();
            sheetName = smartSheetHelperObject.Left(sheetName, SmartsheetConstants.SSConstants.SS_PROJECT_NAME_LENGTH);

            PMSetup setupRecord = projectEntryGraph.Setup.Select();
            PMSetupSSExt pmSetupSSExt = PXCache<PMSetup>.GetExtension<PMSetupSSExt>(setupRecord);
            PMProjectSSExt pmProjectSSExt = PXCache<PMProject>.GetExtension<PMProjectSSExt>(pmProjectRow);

            Users userRecord = PXSelect<Users, Where<Users.pKID, Equal<Current<AccessInfo.userID>>>>.Select(projectEntryGraph);
            UsersSSExt userRecordSSExt = PXCache<Users>.GetExtension<UsersSSExt>(userRecord);

            smartSheetHelperObject.SetupValidation(userRecordSSExt, pmSetupSSExt);
            smartSheetHelperObject.ProjectValidation(projectEntryGraph);

            try
            {
                Token token = new Token();

                token.AccessToken = (String.IsNullOrEmpty(refreshedToken)) ? userRecordSSExt.UsrSmartsheetToken : refreshedToken;

                SmartsheetClient smartsheetClient = new SmartsheetBuilder().SetAccessToken(token.AccessToken).Build();

                long? sheetSelected;
                Dictionary<string, long> currentColumnMap = new Dictionary<string, long>();

                //////////////////
                //Information from Acumatica is updated in Smartsheet
                //////////////////
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

                    smartSheetHelperObject.UpdateSSProject(smartsheetClient, currentColumnMap, projectEntryGraph, sheetSelected, smartSheetHelperObject);
                }
                else //Acumatica Project has not been linked to Smartsheet
                {
                    //Sheet is created from a Gantt Template available in SmartSheet
                    Sheet sheet = new Sheet.CreateSheetFromTemplateBuilder(sheetName, SmartsheetConstants.SSConstants.BASIC_SS_PROJECT_WITH_GANTT).Build();
                    sheet = smartsheetClient.SheetResources.CreateSheet(sheet);

                    Sheet newlyCreatedSheet = smartsheetClient.SheetResources.GetSheet((long)sheet.Id, null, null, null, null, null, null, null);

                    currentColumnMap = new Dictionary<string, long>();
                    foreach (Column currentColumn in newlyCreatedSheet.Columns)
                    {
                        currentColumnMap.Add(currentColumn.Title, (long)currentColumn.Id);
                    }

                    smartSheetHelperObject.AdjustGanttSheet(smartsheetClient, sheet, currentColumnMap);

                    //Acumatica Tasks are added as Smartsheet rows
                    List<Row> newSSRows = smartSheetHelperObject.InsertAcumaticaTasksInSS(projectEntryGraph, currentColumnMap, null, true);
                    IList<Row> ssRows = smartsheetClient.SheetResources.RowResources.AddRows((long)sheet.Id, newSSRows);

                    int ssTaskIDPosition = 0;
                    if (ssRows.Count > 0 && ssRows[0].Cells != null)
                    {
                        ssTaskIDPosition = smartSheetHelperObject.GetSSTaskPosition(ssRows[0].Cells, currentColumnMap[SmartsheetConstants.GanttTemplateMapping.TASK_NAME]);
                    }

                    smartSheetHelperObject.InsertAcumaticaSubTasks(projectEntryGraph, smartsheetClient, sheet, ssRows, smartSheetHelperObject, currentColumnMap, ssTaskIDPosition);

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

                smartSheetHelperObject.UpdateAcumaticaTasks(projectEntryGraph, pmProjectRow, pmSetupSSExt, updatedSheet, columnPositionMap);

                projectEntryGraph.Actions.PressSave();

                if (isMassProcess)
                {
                    PXProcessing.SetInfo(String.Format(SmartsheetConstants.Messages.SUCCESSFULLY_SYNCED, pmProjectRow.ContractCD));
                }
            }
            catch (Exception e)
            {
                if (e.Message.Contains(SmartsheetConstants.SSConstants.EXPIRED_TOKEN_MESSAGE))
                {
                    MyProfileMaint profileMaintGraph = PXGraph.CreateInstance<MyProfileMaint>();
                    MyProfileMaintExt graphExtended = profileMaintGraph.GetExtension<MyProfileMaintExt>();
                    string updatedToken = graphExtended.RefreshSmartsheetToken();
                    CreateUpdateGanttProject(projectEntryGraph, pmProjectRow, updatedToken, isMassProcess = false);
                }
                else
                {
                    throw new PXException(e.Message);
                }
            }
        }
        #endregion
    }
}