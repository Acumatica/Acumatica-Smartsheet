using System;
using PX.Data;

namespace SmartSheetIntegration
{
    [Serializable]
    [PXCacheName(SmartsheetConstants.TableNames.PMTEMPLATELIST)]
    public class PMTemplateListSS : IBqlTable
    {
        #region TemplateSS
        public abstract class templateSS : PX.Data.BQL.BqlString.Field<templateSS> { }
        [PXDBString(20, IsKey = true, IsUnicode = true)]
        [PXUIField(DisplayName = "Template SS")]
        public virtual string TemplateSS { get; set; }
        #endregion

        #region TemplateName
        public abstract class templateName : PX.Data.BQL.BqlString.Field<templateName> { }
        [PXDBString(255, IsKey = true, IsUnicode = true)]
        [PXUIField(DisplayName = "Template Name", Enabled = false)]
        public virtual string TemplateName { get; set; }
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

        #region Tstamp
        public abstract class tstamp : PX.Data.BQL.BqlByteArray.Field<tstamp> { }
        [PXDBTimestamp()]
        [PXUIField(DisplayName = "Tstamp")]
        public virtual byte[] Tstamp { get; set; }
        #endregion

        #region Unbounded Fields
        #region TemplateDefault
        public abstract class templateDefault : PX.Data.BQL.BqlBool.Field<templateDefault> { }
        [PXBool()]
        [PXDefault(false, PersistingCheck = PXPersistingCheck.Nothing)]
        [PXFormula(typeof(Switch<Case<Where<templateSS, Equal<Current<PMSetupSSExt.usrSSTemplate>>>, True>, False>))]
        [PXUIField(DisplayName = "Default Template")]
        public virtual bool? TemplateDefault { get; set; }
        #endregion
        #endregion
    }
}
