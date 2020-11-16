using PX.Data;
using PX.Data.BQL;
using PX.SM;

namespace SmartSheetIntegration
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

    public class SSConnected : BqlType<IBqlString, string>.Constant<SSConnected>
    {
        public SSConnected() : base(SmartsheetConstants.Messages.SS_CONNECTED) { }
    }

    public class SSDisConnected : BqlType<IBqlString, string>.Constant<SSDisConnected>
    {
        public SSDisConnected() : base(SmartsheetConstants.Messages.SS_DISCONNECTED) { }
    }
}