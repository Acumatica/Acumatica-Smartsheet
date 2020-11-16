using PX.Data;
using PX.Objects.PM;

namespace SmartSheetIntegration
{
    public class PMSetupSSExt : PXCacheExtension<PMSetup>
    {
        #region UsrSmartsheetClientID
        public abstract class usrSmartsheetClientID : PX.Data.IBqlField
        {
        }
        [PXRSACryptString(255, IsUnicode = true)]
        [PXUIField(DisplayName = "App Client ID")]
        public virtual string UsrSmartsheetClientID { get; set; }
        #endregion

        #region UsrSmartsheetAppSecret
        public abstract class usrSmartsheetAppSecret : PX.Data.IBqlField
        {
        }
        [PXRSACryptString(255, IsUnicode = true)]
        [PXUIField(DisplayName = "App Secret")]
        public virtual string UsrSmartsheetAppSecret { get; set; }
        #endregion

        #region UsrDefaultRateTableID
        public abstract class usrDefaultRateTableID : PX.Data.IBqlField
        {
        }
        [PXDBString(PMRateTable.rateTableID.Length, IsUnicode = true)]
        [PXUIField(DisplayName = "Tasks Rate Table")]
        [PXSelector(typeof(PMRateTable.rateTableID), DescriptionField = typeof(PMRateTable.description))]
        public virtual string UsrDefaultRateTableID { get; set; }
        #endregion

        #region UsrSSTemplate
        public abstract class usrSSTemplate : PX.Data.BQL.BqlString.Field<usrSSTemplate> { }
        [PXDBString()]
        [PXStringList()]
        [PXUIField(DisplayName = "Smartsheet Template")]
        public virtual string UsrSSTemplate { get; set; }
        #endregion

        #region UsrTypeTaskDate
        public abstract class usrTypeTaskDate : PX.Data.BQL.BqlString.Field<usrTypeTaskDate> { }
        [PXDBString(1)]
        [PXStringList(new string[] { SmartsheetConstants.SSConstants.DAY, SmartsheetConstants.SSConstants.MONTH, SmartsheetConstants.SSConstants.YEAR }, new string[] { SmartsheetConstants.SSConstants.LABEL_DAY, SmartsheetConstants.SSConstants.LABEL_MONTH, SmartsheetConstants.SSConstants.LABEL_YEAR })]
        [PXUIField(DisplayName = "Task Duration")]
        public virtual string UsrTypeTaskDate { get; set; }
        #endregion

        #region UsrDurationTaskDate
        public abstract class usrDurationTaskDate : PX.Data.BQL.BqlInt.Field<usrDurationTaskDate> { }
        [PXDBInt]
        [PXUIField(DisplayName = "")]
        public virtual int? UsrDurationTaskDate { get; set; }
        #endregion        
    }
}