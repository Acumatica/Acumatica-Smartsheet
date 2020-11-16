using PX.Data;
using PX.Objects.PM;
using System;

namespace SmartSheetIntegration
{
    [System.SerializableAttribute()]
    [PXCacheName(SmartsheetConstants.TableNames.PMSUBTASK)]
    [PXPrimaryGraph(typeof(TemplateTaskMaint))]
    public class PMSubTask : PX.Data.IBqlTable
    {
        #region ProjectID
        public abstract class projectID : PX.Data.IBqlField
        {
        }
        [PXDBInt(IsKey = true)]
        [PXDBDefault(typeof(PMTask.projectID))]
        [PXParent(typeof(Select<PMProject,
            Where<PMProject.contractID,
            Equal<Current<PMSubTask.projectID>>>>))]
        public virtual int? ProjectID { get; set; }
        #endregion
        #region TaskID
        public abstract class taskID : PX.Data.IBqlField
        {
        }
        [PXDBInt(IsKey = true)]
        [PXDBDefault(typeof(PMTask.taskID))]
        [PXParent(typeof(Select<PMTask,
            Where<PMTask.taskID,
            Equal<Current<PMSubTask.taskID>>>>))]
        public virtual int? TaskID { get; set; }
        #endregion
        #region SubTaskCD
        public abstract class subTaskCD : PX.Data.IBqlField
        {
        }
        [PXDBString(30, IsUnicode = true, IsKey = true, InputMask = ">CCCCCCCCCCCCCCCCCCCCCCCCCCCCCC")]
        [PXDefault()]
        [PXUIField(DisplayName = "SubTask ID", Visibility = PXUIVisibility.SelectorVisible)]
        public virtual string SubTaskCD { get; set; }
        #endregion
        #region Description
        public abstract class description : PX.Data.IBqlField
        {
        }
        [PXDBString(250, IsUnicode = true)]
        [PXUIField(DisplayName = "Description")]
        public virtual string Description { get; set; }
        #endregion
        #region Position
        public abstract class position : PX.Data.IBqlField
        {
        }
        [PXUIField(DisplayName = "Position")]
        [PXDBInt()]
        public virtual int? Position { get; set; }
        #endregion
        #region tstamp
        public abstract class Tstamp : PX.Data.IBqlField
        {
        }
        [PXDBTimestamp()]
        public virtual byte[] tstamp { get; set; }
        #endregion
        #region CreatedByID
        public abstract class createdByID : PX.Data.IBqlField
        {
        }
        [PXDBCreatedByID()]
        public virtual Guid? CreatedByID { get; set; }
        #endregion
        #region CreatedByScreenID
        public abstract class createdByScreenID : PX.Data.IBqlField
        {
        }
        [PXDBCreatedByScreenID()]
        public virtual string CreatedByScreenID { get; set; }
        #endregion
        #region CreatedDateTime
        public abstract class createdDateTime : PX.Data.IBqlField
        {
        }
        [PXDBCreatedDateTime()]
        public virtual DateTime? CreatedDateTime { get; set; }
        #endregion
        #region LastModifiedByID
        public abstract class lastModifiedByID : PX.Data.IBqlField
        {
        }
        [PXDBLastModifiedByID()]
        public virtual Guid? LastModifiedByID { get; set; }
        #endregion
        #region LastModifiedByScreenID
        public abstract class lastModifiedByScreenID : PX.Data.IBqlField
        {
        }
        [PXDBLastModifiedByScreenID()]
        public virtual string LastModifiedByScreenID { get; set; }
        #endregion
        #region LastModifiedDateTime
        public abstract class lastModifiedDateTime : PX.Data.IBqlField
        {
        }
        [PXDBLastModifiedDateTime()]
        public virtual DateTime? LastModifiedDateTime { get; set; }
        #endregion
        #region NoteID
        public abstract class noteID : PX.Data.IBqlField
        {
        }
        [PXNote()]
        public virtual Guid? NoteID { get; set; }
        #endregion
    }
}