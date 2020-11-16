using PX.Data;
using PX.Objects.PM;

namespace SmartSheetIntegration
{
    public class PMProjectSSExt : PXCacheExtension<PMProject>
    {
        #region UsrSmartsheetContractID
        public abstract class usrSmartsheetContractID : PX.Data.IBqlField
        {
        }
        [PXDBLong()]
        [PXUIField(DisplayName = "Smartsheet Contract ID")]
        public virtual long? UsrSmartsheetContractID { get; set; }
        #endregion

        #region TemplateSS
        public abstract class usrTemplateSS : PX.Data.BQL.BqlString.Field<usrTemplateSS> 
        {
        }
        [PXDBString(20, IsUnicode = true)]
        [PXDefault(typeof(PMSetupSSExt.usrSSTemplate), PersistingCheck = PXPersistingCheck.Nothing)]
        [PXStringList()]
        [PXUIField(DisplayName = "Template SmartSheet")]
        public virtual string UsrTemplateSS { get; set; }
        #endregion

        #region Unbounded Fields

        #region Selected
        public abstract class selected : IBqlField
        {
        }
        [PXBool()]
        [PXUIField(DisplayName = "Selected")]
        public virtual bool? Selected { get; set; }
        #endregion

        #region SyncedInSmartsheet
        public abstract class syncedInSmartsheet : IBqlField
        {
        }
        [PXBool()]
        [PXUIField(DisplayName = "Synced in Smartsheet", Enabled = false)]
        public virtual bool? SyncedInSmartsheet { get; set; }
        #endregion

        #endregion
    }
}