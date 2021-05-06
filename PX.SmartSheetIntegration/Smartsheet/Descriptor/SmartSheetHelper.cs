using PX.Data;
using PX.Objects.PM;
using System;
using System.Collections.Generic;
using System.Linq;
using Smartsheet.Api;
using Smartsheet.Api.Models;

namespace SmartSheetIntegration
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

        public DateTime CalculateWorkingDays(DateTime date, int days)
        {
            days -= 1;
            if (days < 0)
            {
                throw new PXException(SmartsheetConstants.Messages.SMARTSHEET_TOKEN_MISSING);
            }

            if (days == 0) return date;

            if (date.DayOfWeek == DayOfWeek.Saturday)
            {
                date = date.AddDays(2);
                days -= 1;
            }
            else if (date.DayOfWeek == DayOfWeek.Sunday)
            {
                date = date.AddDays(1);
                days -= 1;
            }

            date = date.AddDays(days / 5 * 7);
            int extraDays = days % 5;

            if ((int)date.DayOfWeek + extraDays > 5)
            {
                extraDays += 2;
            }

            return date.AddDays(extraDays);

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
                                            SmartsheetHelper smartSheetHelperObject, PXResultset<PMSSMapping> templateMappingSet)
        {
            //Add newly created rows to Smartsheet
            List<Row> newRows = smartSheetHelperObject.InsertAcumaticaTasksInSS(projectEntryGraph, columnMap, null, false, templateMappingSet);
            IList<Row> ssRows = smartsheetClient.SheetResources.RowResources.AddRows((long)sheetSelected, newRows);

            int ssTaskIDPosition = 0;
            if (ssRows.Count > 0 && ssRows[0].Cells != null)
            {
                //"TaskCD" is the linking element between the Acumatica Project's Tasks and the cell being used as the "Task" equivalent in the mapping definition
                PMSSMapping mappingSS = templateMappingSet.Where(t => ((PMSSMapping)t).NameAcu.Trim().ToUpper() == SmartsheetConstants.ColumnMapping.TASKS_CD.Trim().ToUpper()).FirstOrDefault();

                ssTaskIDPosition = smartSheetHelperObject.GetSSTaskPosition(ssRows[0].Cells, columnMap[mappingSS.NameSS]);
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
                                                        bool firstSync, PXResultset<PMSSMapping> templateMappingSet)
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

                foreach (PMSSMapping row in templateMappingSet)
                {
                    if (!String.IsNullOrEmpty(row.NameAcu))
                    {
                        if (!String.IsNullOrEmpty(row.NameSS))
                        {
                            if (row.NameAcu == SmartsheetConstants.ColumnMapping.DURATION)
                            {
                                currentCell = new Cell.AddCellBuilder(originalColumnMap[row.NameSS], projectEntryGraph.GetValue(SmartsheetConstants.ViewName.TASK, taskRow, row.NameAcu).ToString()).Build();
                                currentCell.Format = SmartsheetConstants.CellFormat.LARGE_GRAY_BACKGROUND;
                            }
                            else
                            {
                                if (row.NameAcu == SmartsheetConstants.ColumnMapping.PCT_COMPLETE)
                                {
                                    decimal completePercent = Convert.ToDecimal(projectEntryGraph.GetValue(SmartsheetConstants.ViewName.TASK, taskRow, row.NameAcu)) / 100;
                                    currentCell = new Cell.AddCellBuilder(originalColumnMap[row.NameSS], completePercent).Build();
                                    currentCell.Format = SmartsheetConstants.CellFormat.LARGER_GRAY_BACKGROUND_PERCENTAGE;
                                }
                                else
                                {
                                    if (row.NameAcu == SmartsheetConstants.ColumnMapping.TASKS_CD) taskRow.TaskCD = taskRow.TaskCD.Trim();
                                    currentCell = new Cell.AddCellBuilder(originalColumnMap[row.NameSS], projectEntryGraph.GetValue(SmartsheetConstants.ViewName.TASK, taskRow, row.NameAcu)).Build();
                                    currentCell.Format = SmartsheetConstants.CellFormat.LARGE_GRAY_BACKGROUND;
                                }
                            }
                            newCells.Add(currentCell);
                        }
                    }
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
                                                int ssTaskIDPosition, 
                                                PXResultset<PMSSMapping> templateMappingSet)
        {
            if (projectEntryGraph.Project.Current != null
                    && projectEntryGraph.Project.Current.TemplateID != null)
            {
                PXResultset<PMTask> templateTasksSet = PXSelect<
                    PMTask,
                    Where<PMTask.projectID, Equal<Required<PMTask.projectID>>>>
                    .Select(projectEntryGraph, projectEntryGraph.Project.Current.TemplateID);

                foreach (PMTask templateTask in templateTasksSet)
                {
                    PMTask actualTask = PXSelect<
                        PMTask,
                        Where<PMTask.projectID, Equal<Required<PMTask.projectID>>,
                            And<PMTask.taskCD, Equal<Required<PMTask.taskCD>>>>>
                        .Select(projectEntryGraph, projectEntryGraph.Project.Current.ContractID, templateTask.TaskCD.Trim());

                    if (actualTask == null)
                    {
                        continue;
                    }

                    PMTaskSSExt pmTemplateTaskSSExtRow = PXCache<PMTask>.GetExtension<PMTaskSSExt>(templateTask);

                    PXResultset<PMSubTask> templateSubTasksSet = PXSelect<
                        PMSubTask,
                        Where<PMSubTask.projectID, Equal<Required<PMSubTask.projectID>>,
                            And<PMSubTask.taskID, Equal<Required<PMSubTask.taskID>>>>,
                        OrderBy<
                            Asc<PMSubTask.position>>>
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
                                dependencySibling = smartSheetHelperObject.AddSubTasks(smartsheetClient, projectEntryGraph, columnMap, sheet, actualTask, pmTemplateTaskSSExtRow, subTaskRow, ssRow.Id, dependencyStartDateOffset, dependencySibling, templateMappingSet);
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
                                    ProjectEntry projectEntryGraph,
                                    Dictionary<string, long> columnMap,
                                    Sheet sheet, PMTask taskRow,
                                    PMTaskSSExt pmTemplateTaskSSExtRow,
                                    PMSubTask subTaskRow,
                                    long? columnID,
                                    int dependencyStartDateOffset,
                                    long dependencySibling, 
                                    PXResultset<PMSSMapping> templateMappingSSRow)
        {
            List<Cell> newCells = new List<Cell>();
            Cell currentCell;

            ProjectEntry copyProjectEntryGraph = projectEntryGraph;
            ProjectEntrySmartsheetExt graphExtended = copyProjectEntryGraph.GetExtension<ProjectEntrySmartsheetExt>();

            if (taskRow != null)
            {
                taskRow.TaskCD = subTaskRow.SubTaskCD;
                taskRow.Description = subTaskRow.Description;
                if (pmTemplateTaskSSExtRow != null)
                {
                    if (pmTemplateTaskSSExtRow.UsrEnableSubtaskDependency == true)
                    {
                        taskRow.StartDate = taskRow.EndDate;
                    }
                }

                foreach (PMSSMapping row in templateMappingSSRow)
                {
                    if (!String.IsNullOrEmpty(row.NameAcu))
                    {
                        if (copyProjectEntryGraph.GetValue(SmartsheetConstants.ViewName.TASK, taskRow, row.NameAcu) is DateTime)
                        {
                            currentCell = new Cell.AddCellBuilder(columnMap[row.NameSS], (DateTime)copyProjectEntryGraph.GetValue(SmartsheetConstants.ViewName.TASK, taskRow, row.NameAcu)).Build();
                            currentCell.Format = SmartsheetConstants.CellFormat.LARGE_GRAY_BACKGROUND;
                        }
                        else
                        {
                            if (row.NameAcu == SmartsheetConstants.ColumnMapping.PCT_COMPLETE)
                            {
                                decimal completePercent = Convert.ToDecimal(copyProjectEntryGraph.GetValue(SmartsheetConstants.ViewName.TASK, taskRow, row.NameAcu)) / 100;
                                currentCell = new Cell.AddCellBuilder(columnMap[row.NameSS], completePercent).Build();
                                currentCell.Format = SmartsheetConstants.CellFormat.LARGER_GRAY_BACKGROUND_PERCENTAGE;
                            }
                            else
                            {
                                currentCell = new Cell.AddCellBuilder(columnMap[row.NameSS], copyProjectEntryGraph.GetValue(SmartsheetConstants.ViewName.TASK, taskRow, row.NameAcu).ToString()).Build();
                                currentCell.Format = SmartsheetConstants.CellFormat.LARGE_GRAY_BACKGROUND;
                            }
                        }
                        newCells.Add(currentCell);
                    }
                }
            }

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
                                                    Dictionary<string, int> columnPositionMap, 
                                                    PXResultset<PMSSMapping> templateMappingSet)
        {
            bool recordedInAcumatica = false;
            int primaryColumnPosition = 0;

            PMTask pmTaskNewEntry = new PMTask();

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
                                pmTaskNewEntry = new PMTask();
                                pmTaskNewEntry.ProjectID = pmProjectRow.ContractID;
                                pmTaskNewEntry.TaskCD = updatedSSRow.Cells[primaryColumnPosition].Value.ToString();

                                PMTask taskCDValidation = (PMTask)projectEntryGraph.Tasks.Select()
                                                        .Where(t => ((PMTask)t).TaskCD.Trim().ToUpper() == updatedSSRow.Cells[primaryColumnPosition].Value.ToString().Trim().ToUpper()).FirstOrDefault();

                                if (taskCDValidation == null)
                                {
                                    projectEntryGraph.Tasks.Cache.SetValueExt<PMTask.rateTableID>(pmTaskNewEntry, pmSetupSSExt.UsrDefaultRateTableID);
                                    projectEntryGraph.Tasks.Cache.SetValueExt<PMTask.status>(pmTaskNewEntry, SmartsheetConstants.SSConstants.ACTIVE);
                                    pmTaskNewEntry.Description = SmartsheetConstants.Messages.DEFAULT_TASK_DESCRIPTION;

                                    string durationVar = "";
                                    foreach (PMSSMapping row in templateMappingSet)
                                    {
                                        if (!String.IsNullOrEmpty(row.NameAcu))
                                        {
                                            SettingForSheets(row, columnPositionMap, updatedSSRow, pmTaskNewEntry, projectEntryGraph, durationVar);
                                        }
                                    }

                                    PMTaskSSExt pmTaskExtRow = PXCache<PMTask>.GetExtension<PMTaskSSExt>(pmTaskNewEntry);
                                    pmTaskExtRow.UsrSmartsheetTaskID = updatedSSRow.Id;
                                    //Insert() has to be invoked at the end as the order in which the values are assigned to the object depends on the iteration
                                    pmTaskNewEntry = projectEntryGraph.Tasks.Insert(pmTaskNewEntry);
                                }
                            }
                        }
                        else //Previously existing row in SS
                        {
                            pmTaskNewEntry = new PMTask();
                            //Fields updated: Description, Start Date, End Date, % complete.
                            if (updatedSSRow.Cells[primaryColumnPosition].Value != null)
                            {
                                if (currentTaskRow != null)
                                {
                                    // Find the Task to update it
                                    PMSSMapping mappingSS = templateMappingSet.Where(t => ((PMSSMapping)t).NameAcu.Trim().ToUpper() == SmartsheetConstants.ColumnMapping.TASKS_CD.Trim().ToUpper()).FirstOrDefault();

                                    pmTaskNewEntry = (PMTask)projectEntryGraph.Tasks.Select()
                                                        .Where(t => ((PMTask)t).TaskCD.Trim().ToUpper() == updatedSSRow.Cells[columnPositionMap[mappingSS.NameSS]].Value.ToString().Trim().ToUpper()).FirstOrDefault();
                                    string durationVar = "";

                                    foreach (PMSSMapping row in templateMappingSet)
                                    {
                                        if (!String.IsNullOrEmpty(row.NameAcu))
                                        {
                                            SettingForSheets(row, columnPositionMap, updatedSSRow, pmTaskNewEntry, projectEntryGraph, durationVar);
                                        }
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
        /// Behavior of data obtained from SmartSheet to insert or update in Acumatica
        /// </summary>
        /// <param name="row"></param>
        /// <param name="columnPositionMap"></param>
        /// <param name="updatedSSRow"></param>
        /// <param name="pmTaskNewEntry"></param>
        /// <param name="projectEntryGraph"></param>
        /// <param name="durationVar"></param>
        public void SettingForSheets(PMSSMapping row, Dictionary<string, int> columnPositionMap, Row updatedSSRow, PMTask pmTaskNewEntry, ProjectEntry projectEntryGraph, string durationVar) 
        {
            if (updatedSSRow.Cells[columnPositionMap[row.NameSS]].Value != null)
            {
                if (updatedSSRow.Cells[columnPositionMap[row.NameSS]].Value is DateTime)
                {
                    DateTime rowDate = (DateTime)updatedSSRow.Cells[columnPositionMap[row.NameSS]].Value;
                    projectEntryGraph.Tasks.Cache.SetValueExt(pmTaskNewEntry, row.NameAcu, rowDate);
                }
                else
                {
                    //Duration has to be assigned before StartDate
                    if (row.NameAcu == SmartsheetConstants.ColumnMapping.DURATION)
                    {
                        durationVar = updatedSSRow.Cells[columnPositionMap[row.NameSS]].Value.ToString().Replace("d", "");
                        projectEntryGraph.Tasks.Cache.SetValueExt(pmTaskNewEntry, row.NameAcu, durationVar);
                    }
                    else
                    {
                        if (row.NameAcu == SmartsheetConstants.ColumnMapping.PCT_COMPLETE)
                        {
                            decimal percent = Convert.ToDecimal(updatedSSRow.Cells[columnPositionMap[row.NameSS]].Value.ToString().Replace("%", "")) * 100;
                            projectEntryGraph.Tasks.Cache.SetValueExt(pmTaskNewEntry, row.NameAcu, percent);
                        }
                        else
                        {
                            projectEntryGraph.Tasks.Cache.SetValueExt(pmTaskNewEntry, row.NameAcu, updatedSSRow.Cells[columnPositionMap[row.NameSS]].Value.ToString());
                        }

                    }
                }

                if (row.NameAcu == SmartsheetConstants.ColumnMapping.START_DATE)
                {
                    if (!String.IsNullOrEmpty(durationVar))
                    {
                        int durationRow = Convert.ToInt32(durationVar);
                        DateTime rowDate = (DateTime)updatedSSRow.Cells[columnPositionMap[row.NameSS]].Value;
                        projectEntryGraph.Tasks.Cache.SetValueExt<PMTaskSSExt.duration>(pmTaskNewEntry, durationRow);
                    }
                }
            }
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