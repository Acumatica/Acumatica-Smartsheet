using PX.Data;
using PX.Objects.PM;
using Smartsheet.Api;

namespace SmartSheetIntegration
{
    public class SmartsheetSyncProcess : PXGraph<SmartsheetSyncProcess>
    {
        public PXCancel<PMProject> Cancel;
        public PXProcessing<PMProject,
                                Where<PMProjectSSExt.usrSmartsheetContractID, IsNotNull,
                                     And<Match<Current<AccessInfo.userName>>>>> Projects;

        public SmartsheetSyncProcess()
        {
            Projects.SetProcessCaption("Sync");
            Projects.SetProcessAllCaption("Sync All");
            Projects.SetProcessDelegate<ProjectEntry>(
            delegate (ProjectEntry graph, PMProject projectRow)
            {
                graph.Clear();
                ProjectEntryExt projectEntryExtGraph = graph.GetExtension<ProjectEntryExt>();
                SmartsheetClient smartsheetClient = projectEntryExtGraph.CheckTokenSheetSS(graph, projectRow, "", true);
                projectEntryExtGraph.CreateEmployeesAcuUserSS(graph, smartsheetClient);
                projectEntryExtGraph.CreateUpdateGanttProject(graph, projectRow, smartsheetClient, true);
            });
        }
    }
}