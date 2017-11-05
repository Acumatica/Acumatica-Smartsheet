using PX.Data;
using PX.Objects.PM;
using System;

namespace PX.SmartSheetIntegration
{
    public class PMTaskSSExt : PXCacheExtension<PMTask>
    {
        #region UsrSmartsheetTaskID
        public abstract class usrSmartsheetTaskID : PX.Data.IBqlField
        {
        }
        [PXDBLong()]
        [PXUIField(DisplayName = "Smartsheet Task ID")]
        public virtual long? UsrSmartsheetTaskID { get; set; }
        #endregion

        #region UsrEnableSubtaskDependency
        public abstract class usrEnableSubtaskDependency : PX.Data.IBqlField
        {
        }
        [PXDBBool]
        [PXUIField(DisplayName = "Enable Subtask Dependency")]
        [PXDefault(false, PersistingCheck = PXPersistingCheck.Nothing)]
        public virtual Boolean? UsrEnableSubtaskDependency { get; set; }
        #endregion
    }
}