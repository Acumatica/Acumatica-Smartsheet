using System;
using PX.Data;

namespace SmartSheetIntegration
{
    [System.SerializableAttribute()]
    [PXCacheName(SmartsheetConstants.TableNames.EPUSERSLISTSS)]
    public class EPUsersListSS : IBqlTable
    {
        #region Ssuserid
        public abstract class ssuserid : PX.Data.BQL.BqlLong.Field<ssuserid> { }
        [PXDBLong(IsKey = true)]
        [PXUIField(DisplayName = "Ssuserid")]
        public virtual long? Ssuserid { get; set; }
        #endregion

        #region FirstName
        public abstract class firstName : PX.Data.BQL.BqlString.Field<firstName> { }
        [PXDBString(255, IsUnicode = true)]
        [PXUIField(DisplayName = "First Name")]
        public virtual string FirstName { get; set; }
        #endregion

        #region LastName
        public abstract class lastName : PX.Data.BQL.BqlString.Field<lastName> { }
        [PXDBString(255, IsUnicode = true)]
        [PXUIField(DisplayName = "Last Name")]
        public virtual string LastName { get; set; }
        #endregion

        #region Email
        public abstract class email : PX.Data.BQL.BqlString.Field<email> { }
        [PXDBString(128)]
        [PXUIField(DisplayName = "Email")]
        public virtual string Email { get; set; }
        #endregion

        #region BAccountID
        public abstract class bAccountID : PX.Data.BQL.BqlInt.Field<bAccountID> { }
        [PXDBInt()]
        [PXUIField(DisplayName = "BAccount ID")]
        public virtual int? BAccountID { get; set; }
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
    }
}