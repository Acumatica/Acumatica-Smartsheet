using PX.Data;
using PX.Objects.PM;

namespace PX.SmartSheetIntegration
{
    public class TemplateTaskMaintExt : PXGraphExtension<TemplateTaskMaint>
    {
        #region Additional DataMembers
        public PXSelect<PMSubTask,
                        Where<PMSubTask.projectID, Equal<Current<PMTask.projectID>>,
                            And<PMSubTask.taskID, Equal<Current<PMTask.taskID>>>>,
                        OrderBy<Asc<PMSubTask.position>>> SelectedSubTasks;
        #endregion

        #region Actions
        public PXAction<PMTask> ButtonDown;
        [PXButton(CommitChanges = true)]
        [PXUIField(DisplayName = " ", MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
        protected virtual void buttonDown()
        {
            PMSubTask currentSubTaskRow = SelectedSubTasks.Current;

            PMSubTask nextSubTask = PXSelect<PMSubTask,
                            Where<PMSubTask.projectID, Equal<Required<PMSubTask.projectID>>,
                                And<PMSubTask.taskID, Equal<Required<PMSubTask.taskID>>,
                                And<PMSubTask.position, Greater<Required<PMSubTask.position>>>>>,
                            OrderBy<Asc<PMSubTask.position>>>
                            .SelectWindowed(this.Base, 0, 1, currentSubTaskRow.ProjectID, currentSubTaskRow.TaskID, currentSubTaskRow.Position);

            if (nextSubTask != null)
            {
                int swapPosition = (int)nextSubTask.Position;
                nextSubTask.Position = currentSubTaskRow.Position;
                currentSubTaskRow.Position = swapPosition;

                this.SelectedSubTasks.Update(currentSubTaskRow);
                this.SelectedSubTasks.Update(nextSubTask);
            }

            this.Base.Actions.PressSave();
        }

        public PXAction<PMTask> ButtonUp;
        [PXButton(CommitChanges = true)]
        [PXUIField(DisplayName = " ", MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
        protected virtual void buttonUp()
        {
            PMSubTask currentSubTaskRow = SelectedSubTasks.Current;

            PMSubTask prevSubTask = PXSelect<PMSubTask,
                            Where<PMSubTask.projectID, Equal<Required<PMSubTask.projectID>>,
                                And<PMSubTask.taskID, Equal<Required<PMSubTask.taskID>>,
                                And<PMSubTask.position, Less<Required<PMSubTask.position>>>>>,
                            OrderBy<Desc<PMSubTask.position>>>
                            .SelectWindowed(this.Base, 0, 1, currentSubTaskRow.ProjectID, currentSubTaskRow.TaskID, currentSubTaskRow.Position);

            if (prevSubTask != null)
            {
                int swapPosition = (int)prevSubTask.Position;
                prevSubTask.Position = currentSubTaskRow.Position;
                currentSubTaskRow.Position = swapPosition;

                this.SelectedSubTasks.Update(currentSubTaskRow);
                this.SelectedSubTasks.Update(prevSubTask);
            }

            this.Base.Actions.PressSave();
        }

        #endregion

        #region Events
        protected virtual void PMSubTask_Position_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
        {
            if (e.Row == null)
            {
                return;
            }

            PMSubTask pmSubTaskRow = (PMSubTask)e.Row;

            PMSubTask subTasksSet = PXSelect<PMSubTask,
                            Where<PMSubTask.projectID, Equal<Required<PMSubTask.projectID>>,
                            And<PMSubTask.taskID, Equal<Required<PMSubTask.taskID>>>>,
                            OrderBy<Desc<PMSubTask.position>>>
                            .SelectWindowed(this.Base, 0, 1, pmSubTaskRow.ProjectID, pmSubTaskRow.TaskID);

            if (subTasksSet != null
                    && subTasksSet.Position > 0)
            {
                pmSubTaskRow.Position = subTasksSet.Position + 1;
            }
            else
            {
                pmSubTaskRow.Position = 1;
            }
        }
        #endregion
    }
}