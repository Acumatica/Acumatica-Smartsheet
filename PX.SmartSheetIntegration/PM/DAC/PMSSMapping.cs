using System;
using System.Collections.Generic;
using PX.Data;
using PX.Objects.PM;
using System.Linq;
using static SmartSheetIntegration.PMSSMapping.nameAcu;

namespace SmartSheetIntegration
{
    [Serializable]
    [PXCacheName(SmartsheetConstants.TableNames.PMMAPPING)]
    public class PMSSMapping : IBqlTable
    {
        #region TemplateSS
        public abstract class templateSS : PX.Data.BQL.BqlString.Field<templateSS> { }
        [PXDBString(20, IsKey = true, IsUnicode = true)]
        [PXParent(typeof(Select<PMTemplateListSS,
        Where<PMTemplateListSS.templateSS, Equal<Current<PMSSMapping.templateSS>>>>))]
        [PXUIField(DisplayName = "Template SS")]
        public virtual string TemplateSS { get; set; }
        #endregion

        #region Cellssid
        public abstract class cellSSID : PX.Data.BQL.BqlInt.Field<cellSSID> { }
        [PXDBInt(IsKey = true)]
        [PXUIField(DisplayName = "Cell ID")]
        public virtual int? CellSSID { get; set; }
        #endregion

        #region NameSS
        public abstract class nameSS : PX.Data.BQL.BqlString.Field<nameSS> { }
        [PXDBString(255, IsKey = true, IsUnicode = true)]
        [PXUIField(DisplayName = "Smartsheet Field", Enabled = false)]
        public virtual string NameSS { get; set; }
        #endregion

        #region NameAcu
        public abstract class nameAcu : PX.Data.BQL.BqlString.Field<nameAcu>
        {
            #region List
            public class ListPMTaskAttribute : PXStringListAttribute
            {
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
                    "CompletedPctMethod"
                };

                private static readonly string[] _values;
                private static readonly string[] _labels;

                static ListPMTaskAttribute()
                {
                    var values = new List<string>() {" "};
                    var labels = new List<string>() {" "};

                    var type = typeof(PMTask);
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
                    _values = values.ToArray();
                    _labels = labels.ToArray();
                }
                public ListPMTaskAttribute()
                            : base(_values, _labels) { }
            }
            #endregion

        }        
        [PXDBString(255, IsUnicode = true)]
        [ListPMTask]
        [PXDefault(PersistingCheck = PXPersistingCheck.Nothing)]
        [PXUIField(DisplayName = "Acumatica Field")]
        public virtual string NameAcu { get; set; }
        #endregion

        #region CellFormat
        public abstract class cellFormat : PX.Data.BQL.BqlString.Field<cellFormat> { }
        [PXDBString(255, IsUnicode = true)]
        [PXDefault(PersistingCheck = PXPersistingCheck.Nothing)]
        [PXUIField(DisplayName = "Cell Field Format", Enabled = false)]
        public virtual string CellFormat { get; set; }
        #endregion

        #region Tstamp
        public abstract class tstamp : PX.Data.BQL.BqlByteArray.Field<tstamp> { }
        [PXDBTimestamp()]
        [PXUIField(DisplayName = "Tstamp")]
        public virtual byte[] Tstamp { get; set; }
        #endregion

        #region CreatedByID
        public abstract class createdByID : PX.Data.BQL.BqlGuid.Field<createdByID> { }
        [PXDBCreatedByID()]
        public virtual Guid? CreatedByID { get; set; }
        #endregion

        #region CreatedByScreenID
        public abstract class createdByScreenID : PX.Data.BQL.BqlString.Field<createdByScreenID> { }
        [PXDBCreatedByScreenID()]
        public virtual string CreatedByScreenID { get; set; }
        #endregion

        #region CreatedDateTime
        public abstract class createdDateTime : PX.Data.BQL.BqlDateTime.Field<createdDateTime> { }
        [PXDBCreatedDateTime()]
        public virtual DateTime? CreatedDateTime { get; set; }
        #endregion

        #region LastModifiedByID
        public abstract class lastModifiedByID : PX.Data.BQL.BqlGuid.Field<lastModifiedByID> { }
        [PXDBLastModifiedByID()]
        public virtual Guid? LastModifiedByID { get; set; }
        #endregion

        #region LastModifiedByScreenID
        public abstract class lastModifiedByScreenID : PX.Data.BQL.BqlString.Field<lastModifiedByScreenID> { }
        [PXDBLastModifiedByScreenID()]
        public virtual string LastModifiedByScreenID { get; set; }
        #endregion

        #region LastModifiedDateTime
        public abstract class lastModifiedDateTime : PX.Data.BQL.BqlDateTime.Field<lastModifiedDateTime> { }
        [PXDBLastModifiedDateTime()]
        public virtual DateTime? LastModifiedDateTime { get; set; }
        #endregion
    }
}