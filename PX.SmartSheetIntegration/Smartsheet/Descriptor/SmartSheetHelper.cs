using PX.Data;
using PX.Objects.PM;
using System;
using System.Collections.Generic;
using System.Linq;
using Smartsheet.Api;
using Smartsheet.Api.Models;

namespace PX.SmartSheetIntegration
{
    public class SmartsheetHelper
    {

        /// <summary>
        /// Verifies the definition of Smartsheet Setup parameters and the Token retrieval
        /// </summary>
        /// <param name="pmSetupSSExt"></param>
        public void SetupValidation(UsersSSExt usersSSExt, PMSetupSSExt pmSetupSSExt)
        {
            if (usersSSExt == null
                    || String.IsNullOrEmpty(usersSSExt.UsrSmartsheetToken))
            {
                throw new PXException(SmartsheetConstants.Messages.SMARTSHEET_TOKEN_MISSING);
            }

            if (pmSetupSSExt == null
                    || String.IsNullOrEmpty(pmSetupSSExt.UsrDefaultRateTableID))
            {
                throw new PXException(SmartsheetConstants.Messages.SMARTSHEET_RATE_TABLE_MISSING);
            }
            return;
        }

        /// <summary>
        /// Verifies the content of the Acumatica Project
        /// </summary>
        /// <param name="projectEntryGraph"></param>
        public void ProjectValidation(ProjectEntry projectEntryGraph)
        {
            PMTask pmTaskRow = (PMTask)projectEntryGraph.Tasks.Select()
                                    .Where(t => ((PMTask)t).StartDate == null || ((PMTask)t).EndDate == null).FirstOrDefault();

            if (pmTaskRow != null)
            {
                throw new PXException(SmartsheetConstants.Messages.ALL_DATES_MUST_BE_SET);
            }

            return;
        }


        /// <summary>
        /// Updates SS Project with new Acumatica tasks not yet synced
        /// </summary>
        /// <param name="smartsheetClient"></param>
        /// <param name="columnMap"></param>
        /// <param name="projectEntryGraph"></param>
        /// <param name="sheetSelected"></param>
        /// <param name="smartSheetHelperObject"></param>
        public void UpdateSSProject(SmartsheetClient smartsheetClient,
                                            Dictionary<string, long> columnMap,
                                            ProjectEntry projectEntryGraph,
                                            long? sheetSelected,
                                            SmartsheetHelper smartSheetHelperObject)
        {
            //Add newly created rows to Smartsheet
            List<Row> newRows = smartSheetHelperObject.InsertAcumaticaTasksInSS(projectEntryGraph, null, columnMap, false);
            IList<Row> ssRows = smartsheetClient.SheetResources.RowResources.AddRows((long)sheetSelected, newRows);

            int ssTaskIDPosition = 0;
            if (ssRows.Count > 0 && ssRows[0].Cells != null)
            {
                ssTaskIDPosition = smartSheetHelperObject.GetSSTaskPosition(ssRows[0].Cells, columnMap[SmartsheetConstants.ColumnMapping.TASK_ID]);
            }

            foreach (Row currentRow in ssRows)
            {
                foreach (PMTask acumaticaTask in projectEntryGraph.Tasks.Select())
                {
                    PMTaskSSExt pmTaskSSExtRow = PXCache<PMTask>.GetExtension<PMTaskSSExt>(acumaticaTask);

                    if (pmTaskSSExtRow != null
                            && pmTaskSSExtRow.UsrSmartsheetTaskID != null)
                    {
                        continue;
                    }

                    if (currentRow.Cells[ssTaskIDPosition].Value != null
                            && acumaticaTask.TaskCD != null
                            && string.Equals(currentRow.Cells[ssTaskIDPosition].Value.ToString().Trim(), acumaticaTask.TaskCD.Trim(), StringComparison.OrdinalIgnoreCase))
                    {
                        pmTaskSSExtRow.UsrSmartsheetTaskID = currentRow.Id;
                        projectEntryGraph.Tasks.Update(acumaticaTask);
                        break;
                    }
                }
            }
            return;
        }

        /// <summary>
        /// Inserts Acumatica Tasks in Smartsheet
        /// </summary>
        /// <param name="projectEntryGraph"></param>
        /// <param name="originalColumnMap"></param>
        /// <param name="modifiedColumnMap"></param>
        /// <param name="firstSync"></param>
        /// <returns></returns>
        public List<Row> InsertAcumaticaTasksInSS(ProjectEntry projectEntryGraph,
                                                        Dictionary<string, long> originalColumnMap,
                                                        Dictionary<string, long> modifiedColumnMap,
                                                        bool firstSync)
        {
            List<Row> newSSRows = new List<Row>();
            Row blankRow = new Row();

            if (firstSync)
            {
                blankRow = new Row.AddRowBuilder(null, true, null, null, null).Build();
                newSSRows.Add(blankRow);
            }

            foreach (PMTask taskRow in projectEntryGraph.Tasks.Select())
            {
                PMTaskSSExt pmTaskSSExtRow = PXCache<PMTask>.GetExtension<PMTaskSSExt>(taskRow);

                if (pmTaskSSExtRow != null
                        && pmTaskSSExtRow.UsrSmartsheetTaskID != null)
                {
                    continue;
                }

                List<Cell> newCells = new List<Cell>();
                Cell currentCell = new Cell();

                //Task
                if (firstSync)
                {
                    currentCell = new Cell.AddCellBuilder(originalColumnMap[SmartsheetConstants.GanttTemplateMapping.TASK_NAME], taskRow.TaskCD).Build();
                }
                else
                {
                    currentCell = new Cell.AddCellBuilder(modifiedColumnMap[SmartsheetConstants.ColumnMapping.TASK_ID], taskRow.TaskCD).Build();
                }
                currentCell.Format = SmartsheetConstants.CellFormat.LARGE_BOLD_GRAY_BACKGROUND;
                newCells.Add(currentCell);

                //Dates
                if (taskRow.StartDate != null && taskRow.EndDate != null)
                {
                    if (firstSync)
                    {
                        currentCell = new Cell.AddCellBuilder(originalColumnMap[SmartsheetConstants.GanttTemplateMapping.START], taskRow.StartDate).Build();
                    }
                    else
                    {
                        currentCell = new Cell.AddCellBuilder(modifiedColumnMap[SmartsheetConstants.ColumnMapping.START_DATE], taskRow.StartDate).Build();
                    }
                    currentCell.Format = SmartsheetConstants.CellFormat.LARGER_GRAY_BACKGROUND;
                    newCells.Add(currentCell);

                    TimeSpan dateDifference = (TimeSpan)(taskRow.EndDate - taskRow.StartDate);

                    if (firstSync)
                    {
                        currentCell = new Cell.AddCellBuilder(originalColumnMap[SmartsheetConstants.GanttTemplateMapping.DURATION], (dateDifference.Days + 1).ToString()).Build();
                    }
                    else
                    {
                        currentCell = new Cell.AddCellBuilder(modifiedColumnMap[SmartsheetConstants.ColumnMapping.DURATION], (dateDifference.Days + 1).ToString()).Build();
                    }
                    currentCell.Format = SmartsheetConstants.CellFormat.LARGER_GRAY_BACKGROUND;
                    newCells.Add(currentCell);
                }

                //Completed %
                if (taskRow.CompletedPercent != null)
                {
                    if (firstSync)
                    {
                        currentCell = new Cell.AddCellBuilder(originalColumnMap[SmartsheetConstants.GanttTemplateMapping.PCT_COMPLETE], (double)taskRow.CompletedPercent / 100).Build();
                    }
                    else
                    {
                        currentCell = new Cell.AddCellBuilder(modifiedColumnMap[SmartsheetConstants.ColumnMapping.PCT_COMPLETE], (double)taskRow.CompletedPercent / 100).Build();
                    }
                    currentCell.Format = SmartsheetConstants.CellFormat.LARGER_GRAY_BACKGROUND_PERCENTAGE;
                    newCells.Add(currentCell);
                }

                if (taskRow.Description != null)
                {
                    if (firstSync)
                    {
                        currentCell = new Cell.AddCellBuilder(originalColumnMap[SmartsheetConstants.GanttTemplateMapping.COMMENTS], taskRow.Description).Build();
                    }
                    else
                    {
                        currentCell = new Cell.AddCellBuilder(modifiedColumnMap[SmartsheetConstants.ColumnMapping.DESCRIPTION], taskRow.Description).Build();
                    }
                    currentCell.Format = SmartsheetConstants.CellFormat.LARGE_GRAY_BACKGROUND;
                    newCells.Add(currentCell);
                }

                Row currentRow = new Row.AddRowBuilder(null, true, null, null, null).SetCells(newCells).Build();
                currentRow.Format = SmartsheetConstants.CellFormat.GRAY_BACKGROUND;
                newSSRows.Add(currentRow);

                blankRow = new Row.AddRowBuilder(null, true, null, null, null).Build();
                newSSRows.Add(blankRow);
            }

            return newSSRows;
        }


        /// <summary>
        /// Inserts in the Smartsheet project Subtasks defined in the Project Template Task from Acumatica
        /// </summary>
        /// <param name="projectEntryGraph"></param>
        /// <param name="smartsheetClient"></param>
        /// <param name="sheet"></param>
        /// <param name="ssRowSet"></param>
        /// <param name="smartSheetHelperObject"></param>
        /// <param name="columnMap"></param>
        /// <param name="ssTaskIDPosition"></param>
        public void InsertAcumaticaSubTasks(ProjectEntry projectEntryGraph,
                                                SmartsheetClient smartsheetClient,
                                                Sheet sheet,
                                                IList<Row> ssRowSet,
                                                SmartsheetHelper smartSheetHelperObject,
                                                Dictionary<string, long> columnMap,
                                                int ssTaskIDPosition)
        {
            if (projectEntryGraph.Project.Current != null
                    && projectEntryGraph.Project.Current.TemplateID != null)
            {
                PXResultset<PMTask> templateTasksSet = PXSelect<PMTask,
                                                Where<PMTask.projectID, Equal<Required<PMTask.projectID>>>>
                                                .Select(projectEntryGraph, projectEntryGraph.Project.Current.TemplateID);

                foreach (PMTask templateTask in templateTasksSet)
                {
                    PMTask actualTask = PXSelect<PMTask,
                                                Where<PMTask.projectID, Equal<Required<PMTask.projectID>>,
                                                And<PMTask.taskCD, Equal<Required<PMTask.taskCD>>>>>
                                                .Select(projectEntryGraph, projectEntryGraph.Project.Current.ContractID, templateTask.TaskCD.Trim());

                    if (actualTask == null)
                    {
                        continue;
                    }

                    PMTaskSSExt pmTemplateTaskSSExtRow = PXCache<PMTask>.GetExtension<PMTaskSSExt>(templateTask);

                    PXResultset<PMSubTask> templateSubTasksSet = PXSelect<PMSubTask,
                                                                    Where<PMSubTask.projectID, Equal<Required<PMSubTask.projectID>>,
                                                                    And<PMSubTask.taskID, Equal<Required<PMSubTask.taskID>>>>,
                                                                    OrderBy<Asc<PMSubTask.position>>>
                                                                    .Select(projectEntryGraph, templateTask.ProjectID, templateTask.TaskID);

                    int dependencyStartDateOffset = 0;
                    long dependencySibling = 0;
                    foreach (Row ssRow in ssRowSet)
                    {
                        if (ssRow.Cells[ssTaskIDPosition].Value != null
                                && string.Equals(ssRow.Cells[ssTaskIDPosition].Value.ToString().Trim(), templateTask.TaskCD.Trim(), StringComparison.OrdinalIgnoreCase))
                        {
                            foreach (PMSubTask subTaskRow in templateSubTasksSet)
                            {
                                dependencySibling = smartSheetHelperObject.AddSubTasks(smartsheetClient, columnMap, sheet, actualTask, pmTemplateTaskSSExtRow, subTaskRow, ssRow.Id, dependencyStartDateOffset, dependencySibling);
                                dependencyStartDateOffset += (int)subTaskRow.Duration;
                            }
                        }
                    }
                }
            }

            return;
        }

        /// <summary>
        /// Adds subtasks to the Smartsheet project
        /// </summary>
        /// <param name="smartsheetClient"></param>
        /// <param name="columnMap"></param>
        /// <param name="sheet"></param>
        /// <param name="taskRow"></param>
        /// <param name="pmTemplateTaskSSExtRow"></param>
        /// <param name="subTaskRow"></param>
        /// <param name="columnID"></param>
        /// <param name="dependencyStartDateOffset"></param>
        /// <param name="dependencySibling"></param>
        /// <returns></returns>
        public long AddSubTasks(SmartsheetClient smartsheetClient,
                                    Dictionary<string, long> columnMap,
                                    Sheet sheet, PMTask taskRow,
                                    PMTaskSSExt pmTemplateTaskSSExtRow,
                                    PMSubTask subTaskRow,
                                    long? columnID,
                                    int dependencyStartDateOffset,
                                    long dependencySibling)
        {
            List<Cell> newCells = new List<Cell>();

            Cell currentCell = new Cell.AddCellBuilder(columnMap[SmartsheetConstants.GanttTemplateMapping.TASK_NAME], subTaskRow.SubTaskCD).Build();
            currentCell.Format = SmartsheetConstants.CellFormat.LARGE_GRAY_BACKGROUND;
            newCells.Add(currentCell);

            if (pmTemplateTaskSSExtRow.UsrEnableSubtaskDependency == true)
            {
                DateTime adjustedStartDate = taskRow.StartDate.Value.AddDays((double)dependencyStartDateOffset);
                currentCell = new Cell.AddCellBuilder(columnMap[SmartsheetConstants.GanttTemplateMapping.START], adjustedStartDate).Build();
                newCells.Add(currentCell);
            }
            else
            {
                currentCell = new Cell.AddCellBuilder(columnMap[SmartsheetConstants.GanttTemplateMapping.START], taskRow.StartDate).Build();
                newCells.Add(currentCell);
            }

            currentCell = new Cell.AddCellBuilder(columnMap[SmartsheetConstants.GanttTemplateMapping.DURATION], subTaskRow.Duration.ToString()).Build();
            newCells.Add(currentCell);
            currentCell = new Cell.AddCellBuilder(columnMap[SmartsheetConstants.GanttTemplateMapping.COMMENTS], subTaskRow.Description).Build();
            newCells.Add(currentCell);

            Row currentRow = new Row.AddRowBuilder(null, true, null, null, null).SetCells(newCells).Build();
            currentRow.ParentId = (long)columnID;

            currentRow.Format = SmartsheetConstants.CellFormat.GRAY_BACKGROUND;

            List<Row> newSSRows = new List<Row>();
            newSSRows.Add(currentRow);

            IList<Row> ssRows = smartsheetClient.SheetResources.RowResources.AddRows((long)sheet.Id, newSSRows);
            return (long)ssRows[0].Id;
        }


        /// <summary>
        /// Creates/Updates Acumatica Tasks with the Smartsheet modifications 
        /// </summary>
        /// <param name="projectEntryGraph"></param>
        /// <param name="pmProjectRow"></param>
        /// <param name="pmSetupSSExt"></param>
        /// <param name="updatedSheet"></param>
        /// <param name="columnPositionMap"></param>
        public void UpdateAcumaticaTasks(ProjectEntry projectEntryGraph,
                                                    PMProject pmProjectRow,
                                                    PMSetupSSExt pmSetupSSExt,
                                                    Sheet updatedSheet,
                                                    Dictionary<string, int> columnPositionMap)
        {
            bool recordedInAcumatica = false;
            int primaryColumnPosition = 0;

            foreach (Column updatedColumn in updatedSheet.Columns)
            {
                if (updatedColumn != null
                        && updatedColumn.Primary != null
                            && updatedColumn.Primary == true)
                {
                    foreach (Row updatedSSRow in updatedSheet.Rows)
                    {
                        if (updatedSSRow != null &&
                                updatedSSRow.ParentId != null) //Subtasks are not synced back to Acumatica
                        {
                            continue;
                        }

                        PMTask currentTaskRow = null;
                        PMTaskSSExt pmTaskSSExtRow = null;

                        foreach (PMTask taskRow in projectEntryGraph.Tasks.Select())
                        {
                            recordedInAcumatica = false;
                            pmTaskSSExtRow = PXCache<PMTask>.GetExtension<PMTaskSSExt>(taskRow);
                            if (pmTaskSSExtRow != null
                                    && pmTaskSSExtRow.UsrSmartsheetTaskID == updatedSSRow.Id)
                            {
                                recordedInAcumatica = true;
                                currentTaskRow = taskRow;
                                break;
                            }
                        }

                        if (recordedInAcumatica == false) //New Row in Smartsheet not yet added to Acumatica
                        {
                            //Fields retrieved: Task, Description, Start Date, End Date, % Complete,
                            if (updatedSSRow.Cells[primaryColumnPosition].Value != null)
                            {
                                PMTask pmTaskNewEntry = new PMTask();
                                pmTaskNewEntry.ProjectID = pmProjectRow.ContractID;
                                pmTaskNewEntry.TaskCD = updatedSSRow.Cells[primaryColumnPosition].Value.ToString();

                                PMTask taskCDValidation = (PMTask)projectEntryGraph.Tasks.Select()
                                                        .Where(t => ((PMTask)t).TaskCD.Trim().ToUpper() == updatedSSRow.Cells[primaryColumnPosition].Value.ToString().Trim().ToUpper()).FirstOrDefault();

                                if (taskCDValidation == null)
                                {
                                    pmTaskNewEntry = projectEntryGraph.Tasks.Insert(pmTaskNewEntry);

                                    if (updatedSSRow.Cells[columnPositionMap[SmartsheetConstants.ColumnMapping.DESCRIPTION]] != null
                                            && updatedSSRow.Cells[columnPositionMap[SmartsheetConstants.ColumnMapping.DESCRIPTION]].Value != null)
                                    {
                                        projectEntryGraph.Tasks.Cache.SetValueExt<PMTask.description>(pmTaskNewEntry, updatedSSRow.Cells[columnPositionMap[SmartsheetConstants.ColumnMapping.DESCRIPTION]].Value.ToString());
                                    }
                                    else
                                    {
                                        projectEntryGraph.Tasks.Cache.SetValueExt<PMTask.description>(pmTaskNewEntry, SmartsheetConstants.Messages.DEFAULT_TASK_DESCRIPTION);
                                    }

                                    projectEntryGraph.Tasks.Cache.SetValueExt<PMTask.rateTableID>(pmTaskNewEntry, pmSetupSSExt.UsrDefaultRateTableID);
                                    projectEntryGraph.Tasks.Cache.SetValueExt<PMTask.status>(pmTaskNewEntry, SmartsheetConstants.SSConstants.ACTIVE);

                                    if (updatedSSRow.Cells[columnPositionMap[SmartsheetConstants.ColumnMapping.START_DATE]] != null
                                            && updatedSSRow.Cells[columnPositionMap[SmartsheetConstants.ColumnMapping.START_DATE]].Value != null)
                                    {
                                        if (updatedSSRow.Cells[columnPositionMap[SmartsheetConstants.ColumnMapping.START_DATE]].Value is DateTime)
                                        {
                                            DateTime startDate = (DateTime)updatedSSRow.Cells[columnPositionMap[SmartsheetConstants.ColumnMapping.START_DATE]].Value;
                                            projectEntryGraph.Tasks.Cache.SetValueExt<PMTask.startDate>(pmTaskNewEntry, startDate);
                                            projectEntryGraph.Tasks.Cache.SetValueExt<PMTask.plannedStartDate>(pmTaskNewEntry, startDate);
                                        }
                                    }

                                    if (updatedSSRow.Cells[columnPositionMap[SmartsheetConstants.ColumnMapping.END_DATE]] != null
                                            && updatedSSRow.Cells[columnPositionMap[SmartsheetConstants.ColumnMapping.END_DATE]].Value != null)
                                    {
                                        if (updatedSSRow.Cells[columnPositionMap[SmartsheetConstants.ColumnMapping.END_DATE]].Value is DateTime)
                                        {
                                            DateTime endDate = (DateTime)updatedSSRow.Cells[columnPositionMap[SmartsheetConstants.ColumnMapping.END_DATE]].Value;
                                            projectEntryGraph.Tasks.Cache.SetValueExt<PMTask.endDate>(pmTaskNewEntry, endDate);
                                            projectEntryGraph.Tasks.Cache.SetValueExt<PMTask.plannedEndDate>(pmTaskNewEntry, endDate);
                                        }
                                    }

                                    if (updatedSSRow.Cells[columnPositionMap[SmartsheetConstants.ColumnMapping.PCT_COMPLETE]] != null
                                            && updatedSSRow.Cells[columnPositionMap[SmartsheetConstants.ColumnMapping.PCT_COMPLETE]].Value != null
                                                && pmTaskNewEntry.Status == SmartsheetConstants.SSConstants.ACTIVE)
                                    {
                                        projectEntryGraph.Tasks.Cache.SetValueExt<PMTask.completedPercent>(pmTaskNewEntry, (decimal)((double)updatedSSRow.Cells[columnPositionMap[SmartsheetConstants.ColumnMapping.PCT_COMPLETE]].Value * 100));
                                    }

                                    PMTaskSSExt pmTaskExtRow = PXCache<PMTask>.GetExtension<PMTaskSSExt>(pmTaskNewEntry);
                                    pmTaskExtRow.UsrSmartsheetTaskID = updatedSSRow.Id;

                                    pmTaskNewEntry = projectEntryGraph.Tasks.Update(pmTaskNewEntry);
                                }
                            }
                        }
                        else //Previously existing row in SS
                        {
                            //Fields updated: Description, Start Date, End Date, % complete.
                            if (updatedSSRow.Cells[primaryColumnPosition].Value != null)
                            {
                                if (currentTaskRow != null)
                                {
                                    if (updatedSSRow.Cells[columnPositionMap[SmartsheetConstants.ColumnMapping.DESCRIPTION]] != null
                                            && updatedSSRow.Cells[columnPositionMap[SmartsheetConstants.ColumnMapping.DESCRIPTION]].Value != null)
                                    {
                                        projectEntryGraph.Tasks.Cache.SetValueExt<PMTask.description>(currentTaskRow, updatedSSRow.Cells[columnPositionMap[SmartsheetConstants.ColumnMapping.DESCRIPTION]].Value.ToString());
                                    }

                                    if (updatedSSRow.Cells[columnPositionMap[SmartsheetConstants.ColumnMapping.START_DATE]] != null
                                            && updatedSSRow.Cells[columnPositionMap[SmartsheetConstants.ColumnMapping.START_DATE]].Value != null)
                                    {
                                        if (updatedSSRow.Cells[columnPositionMap[SmartsheetConstants.ColumnMapping.START_DATE]].Value is DateTime)
                                        {
                                            DateTime startDate = (DateTime)updatedSSRow.Cells[columnPositionMap[SmartsheetConstants.ColumnMapping.START_DATE]].Value;
                                            projectEntryGraph.Tasks.Cache.SetValueExt<PMTask.startDate>(currentTaskRow, startDate);
                                        }
                                    }

                                    if (updatedSSRow.Cells[columnPositionMap[SmartsheetConstants.ColumnMapping.END_DATE]] != null
                                            && updatedSSRow.Cells[columnPositionMap[SmartsheetConstants.ColumnMapping.END_DATE]].Value != null)
                                    {
                                        if (updatedSSRow.Cells[columnPositionMap[SmartsheetConstants.ColumnMapping.END_DATE]].Value is DateTime)
                                        {
                                            DateTime endDate = (DateTime)updatedSSRow.Cells[columnPositionMap[SmartsheetConstants.ColumnMapping.END_DATE]].Value;
                                            projectEntryGraph.Tasks.Cache.SetValueExt<PMTask.endDate>(currentTaskRow, endDate);
                                        }
                                    }

                                    if (updatedSSRow.Cells[columnPositionMap[SmartsheetConstants.ColumnMapping.PCT_COMPLETE]] != null
                                            && updatedSSRow.Cells[columnPositionMap[SmartsheetConstants.ColumnMapping.PCT_COMPLETE]].Value != null
                                                && currentTaskRow.Status == SmartsheetConstants.SSConstants.ACTIVE)
                                    {
                                        projectEntryGraph.Tasks.Cache.SetValueExt<PMTask.completedPercent>(currentTaskRow, (decimal)((double)updatedSSRow.Cells[columnPositionMap[SmartsheetConstants.ColumnMapping.PCT_COMPLETE]].Value * 100));
                                    }

                                    currentTaskRow = projectEntryGraph.Tasks.Update(currentTaskRow);
                                }
                            }
                        }
                    }
                    break;
                }
                else
                {
                    primaryColumnPosition += 1;
                }
            }

            return;
        }

        /// <summary>
        /// Returns the 0-based position of the TaskID column in smartsheet
        /// </summary>
        /// <param name="ssCellsSet">List of cells</param>
        /// <param name="ssTaskID">Task column identifier in Smartsheet</param>
        /// <returns></returns>
        public int GetSSTaskPosition(IList<Cell> ssCellsSet, long ssTaskID)
        {
            int ssTaskIDPosition = 0;

            foreach (Cell ssCell in ssCellsSet)
            {
                if (ssCell.ColumnId == ssTaskID)
                {
                    break;
                }
                ssTaskIDPosition++;
            }

            return ssTaskIDPosition;
        }

        /// <summary>
        /// Renames and reorganizes the Gantt Chart columns created from Smartsheet's Gantt template
        /// </summary>
        /// <param name="smartsheetClient"></param>
        /// <param name="sheet"></param>
        /// <param name="columnMap"></param>
        public void AdjustGanttSheet(SmartsheetClient smartsheetClient, Sheet sheet, Dictionary<string, long> columnMap)
        {
            Column updatedColumn = smartsheetClient.SheetResources.ColumnResources.UpdateColumn
                                    ((long)sheet.Id, new Column.UpdateColumnBuilder(columnMap[SmartsheetConstants.GanttTemplateMapping.TASK_NAME], SmartsheetConstants.ColumnMapping.TASK_ID, 0).Build());

            updatedColumn = smartsheetClient.SheetResources.ColumnResources.UpdateColumn
                                    ((long)sheet.Id, new Column.UpdateColumnBuilder(columnMap[SmartsheetConstants.GanttTemplateMapping.START], SmartsheetConstants.ColumnMapping.START_DATE, 1).Build());

            updatedColumn = smartsheetClient.SheetResources.ColumnResources.UpdateColumn
                                    ((long)sheet.Id, new Column.UpdateColumnBuilder(columnMap[SmartsheetConstants.GanttTemplateMapping.FINISH], SmartsheetConstants.ColumnMapping.END_DATE, 2).Build());

            updatedColumn = smartsheetClient.SheetResources.ColumnResources.UpdateColumn
                                    ((long)sheet.Id, new Column.UpdateColumnBuilder(columnMap[SmartsheetConstants.GanttTemplateMapping.ASSIGNED_TO], SmartsheetConstants.ColumnMapping.ASSIGNED_TO, 8).Build());

            updatedColumn = smartsheetClient.SheetResources.ColumnResources.UpdateColumn
                                    ((long)sheet.Id, new Column.UpdateColumnBuilder(columnMap[SmartsheetConstants.GanttTemplateMapping.PCT_COMPLETE], SmartsheetConstants.ColumnMapping.PCT_COMPLETE, 5).Build());

            updatedColumn = smartsheetClient.SheetResources.ColumnResources.UpdateColumn
                                    ((long)sheet.Id, new Column.UpdateColumnBuilder(columnMap[SmartsheetConstants.GanttTemplateMapping.COMMENTS], SmartsheetConstants.ColumnMapping.DESCRIPTION, 6).SetWidth(350).Build());

            updatedColumn = smartsheetClient.SheetResources.ColumnResources.UpdateColumn
                                    ((long)sheet.Id, new Column.UpdateColumnBuilder(columnMap[SmartsheetConstants.GanttTemplateMapping.PREDECESSORS], SmartsheetConstants.ColumnMapping.PREDECESSORS, 7).Build());

            //Deleted column
            smartsheetClient.SheetResources.ColumnResources.DeleteColumn((long)sheet.Id, columnMap[SmartsheetConstants.GanttTemplateMapping.STATUS]);

            return;
        }

        /// <summary>
        /// Returns the Left section of a string
        /// </summary>
        /// <param name="value">String to reduce</param>
        /// <param name="maxLength">Length</param>
        /// <returns></returns>
        public string Left(string value, int maxLength)
        {
            if (string.IsNullOrEmpty(value))
            {
                return value;
            }

            maxLength = Math.Abs(maxLength);

            return (value.Length <= maxLength
                   ? value
                   : value.Substring(0, maxLength)
                   );
        }
    }
}