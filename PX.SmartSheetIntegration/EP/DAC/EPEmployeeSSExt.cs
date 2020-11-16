using PX.Data;
using PX.Objects.EP;

namespace SmartSheetIntegration
{
    public class EPEmployeeSSExt : PXCacheExtension<EPEmployee>
    {
        #region UsrSSUserID
        public abstract class usrSSUserID : PX.Data.BQL.BqlLong.Field<usrSSUserID> { }
        [PXDBLong()]
        [PXUIField(DisplayName = "SmartSheet User")]
        [PXSelector(typeof(Search<
            EPUsersListSS.ssuserid,
            Where<EPUsersListSS.bAccountID, IsNull>>),
                    typeof(EPUsersListSS.firstName),
                    typeof(EPUsersListSS.lastName),
                    typeof(EPUsersListSS.email),
            SubstituteKey = typeof(EPUsersListSS.email))]
        public virtual long? UsrSSUserid { get; set; }
        #endregion
    }
}