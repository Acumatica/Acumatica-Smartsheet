using PX.Data;
using PX.Objects.CS;
using PX.Objects.PM;
using System.Collections.Generic;
using System.Linq;

namespace SmartSheetIntegration
{
    public class ListPMTaskAttribute : PXStringListAttribute
    {
        public ListPMTaskAttribute()
            : base()
        {
        }

        private static string[] ignoredFields = new[]
        {
                    "NoteID",
                    "CreatedByID",
                    "CreatedByScreenID",
                    "CreatedDateTime",
                    "LastModifiedByID",
                    "LastModifiedByScreenID",
                    "LastModifiedDateTime",
                    "tstamp",
                    "VisibleInSO",
                    "VisibleInPO",
                    "VisibleInIN",
                    "VisibleInGL",
                    "VisibleInEA",
                    "VisibleInCR",
                    "VisibleInCA",
                    "VisibleInAR",
                    "VisibleInAP",
                    "WipAccountGroupID",
                    "Selected",
                    "IsDefault",
                    "TaskID",
                    "AutoIncludeInPrj",
                    "CompletedPctMethod",
                };

        public override void CacheAttached(PXCache sender)
        {
            base.CacheAttached(sender);

            var values = new List<string>() { " " };
            var labels = new List<string>() { " " };

            var type = typeof(PMTask);
            // Acuminator disable once PX1045 PXGraphCreateInstanceInEventHandlers Needed because the info is retrieved using a graph instance.
            if (PXAccess.FeatureInstalled<FeaturesSet.projectModule>())
            {
                ProjectEntry projectEntryGraph = PXGraph.CreateInstance<ProjectEntry>();
                foreach (var field in projectEntryGraph.GetFieldNames(SmartsheetConstants.ViewName.TASK))
                {
                    if (ignoredFields.Any(fieldName => field.Contains(fieldName) || field.Contains("_") || field.Contains("Note")))
                        continue;
                    if (!values.Contains(field))
                    {
                        PXFieldState fs = projectEntryGraph.Caches[type].GetStateExt(null, field) as PXFieldState;
                        values.Add(field);
                        labels.Add(fs != null ? fs.DisplayName : field);
                    }
                }
            }

            _AllowedValues = values.ToArray();
            _AllowedLabels = labels.ToArray();
        }
    }
}