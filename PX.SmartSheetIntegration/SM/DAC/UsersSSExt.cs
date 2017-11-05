using PX.Data;
using PX.SM;

namespace PX.SmartSheetIntegration
{
    public class UsersSSExt : PXCacheExtension<Users>
    {
        #region UsrSmartsheetToken
        public abstract class usrSmartsheetToken : PX.Data.IBqlField
        {
        }
        [PXDBString(150, IsUnicode = true)]
        [PXUIField(DisplayName = "Smartsheet Token", Visible = false)]
        public virtual string UsrSmartsheetToken { get; set; }
        #endregion

        #region UsrSmartsheetRefreshToken
        public abstract class usrSmartsheetRefreshToken : PX.Data.IBqlField
        {
        }
        [PXDBString(150, IsUnicode = true)]
        [PXUIField(DisplayName = "Smartsheet Refresh Token", Visible = false)]
        public virtual string UsrSmartsheetRefreshToken { get; set; }
        #endregion

        #region UsrSmartSheetStatus
        public abstract class usrSmartSheetStatus : IBqlField { }

        [PXString(IsUnicode = true)]
        [PXUIField(DisplayName = "Status", IsReadOnly = true)]
        [PXDependsOnFields(typeof(usrSmartsheetToken))]
        [PXFormula(typeof(Switch<Case<Where<usrSmartsheetToken, IsNull>, SSDisConnected>, SSConnected>))]
        public virtual string UsrSmartSheetStatus { get; set; }
        #endregion
    }

    public class SSConnected : Constant<string>
    {
        public SSConnected() : base(SmartsheetConstants.Messages.SS_CONNECTED) { }
    }

    public class SSDisConnected : Constant<string>
    {
        public SSDisConnected() : base(SmartsheetConstants.Messages.SS_DISCONNECTED) { }
    }
}