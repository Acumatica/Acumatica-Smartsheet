using PX.Data;
using PX.Objects.PM;

namespace PX.SmartSheetIntegration
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
    }
}