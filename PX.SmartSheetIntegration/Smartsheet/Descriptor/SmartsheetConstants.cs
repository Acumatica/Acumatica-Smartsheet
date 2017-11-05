using PX.Common;

namespace PX.SmartSheetIntegration
{
    public static class SmartsheetConstants
    {
        #region SSConstants
        public static class SSConstants
        {
            public const int SS_PROJECT_NAME_LENGTH = 50;
            public const long BASIC_SS_PROJECT_WITH_GANTT = 5066554783098756; //"Basic Project with Gantt & Dependencies"
            public const string SS_TEXT = "Smartsheet";
            public const string ACTIVE = "A";
            public const string AMPERSAND = "&";
            public const string PIPE = "|";
            public const string EXPIRED_TOKEN_MESSAGE = "expired";
        }
        #endregion

        #region ActionNames
        [PXLocalizable(ActionsNames.PREFIX)]
        public static class ActionsNames
        {
            public const string PREFIX = "Smartsheet Integration";
            public const string SYNC_SMARTSHEET_PROJECT = "Sync Smartsheet Project";
            public const string REQUEST_SS_TOKEN = "Get Smartsheet Token";
            public const string REFRESH_SS_TOKEN = "Refresh Smartsheet Token";
        }
        #endregion

        #region Messages
        [PXLocalizable(Messages.PREFIX)]
        public static class Messages
        {
            public const string PREFIX = "Smartsheet Integration";
            public const string SMARTSHEET_ID_MISSING = "Smartsheet Settings has to be specified in Project Preferences screen";
            public const string SMARTSHEET_TOKEN_MISSING = "Smartsheet Token has to be requested in User Profile screen";
            public const string SMARTSHEET_RATE_TABLE_MISSING = "The Default Rate Table has to be defined in the Project Setup page";
            public const string ALL_DATES_MUST_BE_SET = "Dates must be assigned in all tasks in order to Sync with Smartsheet";
            public const string SUCCESSFULLY_SYNCED = "Project {0} has been successfully synced with Smartsheet";
            public const string DEFAULT_TASK_DESCRIPTION = "Smartsheet Task";
            public const string SS_CONNECTED = "Connected";
            public const string SS_DISCONNECTED = "Disconnected";
            public const string SMARTSHEET_INVALID_RESPONSE = "Response is not valid";
        }
        #endregion

        #region TableNames
        [PXLocalizable(Messages.PREFIX)]
        public static class TableNames
        {
            public const string PREFIX = "Smartsheet Integration";
            public const string PMSUBTASK = "Project Template Subtasks";
        }
        #endregion

        #region ColumnMapping
        [PXLocalizable(ColumnMapping.PREFIX)]
        public static class ColumnMapping
        {
            public const string PREFIX = "Smartsheet Integration";
            public const string TASK_ID = "Task ID";
            public const string START_DATE = "Start Date";
            public const string END_DATE = "End Date";
            public const string DURATION = "Duration";
            public const string PCT_COMPLETE = "% Complete";
            public const string DESCRIPTION = "Description";
            public const string ASSIGNED_TO = "Assigned To";
            public const string PREDECESSORS = "Predecessors";
        }
        #endregion

        #region GanttTemplateMapping
        [PXLocalizable(GanttTemplateMapping.PREFIX)]
        public static class GanttTemplateMapping
        {
            public const string PREFIX = "Smartsheet Integration";
            public const string TASK_NAME = "Task Name";
            public const string START = "Start";
            public const string FINISH = "Finish";
            public const string DURATION = "Duration";
            public const string PCT_COMPLETE = "% Complete";
            public const string ASSIGNED_TO = "Assigned To";
            public const string COMMENTS = "Comments";
            public const string PREDECESSORS = "Predecessors";
            public const string STATUS = "Status";
        }
        #endregion

        #region CellFormat
        public static class CellFormat
        {
            public const string LARGE_BOLD_GRAY_BACKGROUND = ",1,1,,,,,,,18,,,,,,";
            public const string LARGER_GRAY_BACKGROUND = ",2,,,,,,,,18,,,,,,";
            public const string LARGER_GRAY_BACKGROUND_PERCENTAGE = ",2,,,,,,,,18,,,,,3,";
            public const string LARGE_GRAY_BACKGROUND = ",1,,,,,,,,18,,,,,,";
            public const string GRAY_BACKGROUND = ",,,,,,,,,18,,,,,,";
        }
        #endregion

        #region SSCodeRequest
        public static class SSCodeRequest
        {
            public const string ENDPOINT = "https://app.smartsheet.com/b/authorize?";
            public const string RESPONSE_TYPE = "response_type=code";
            public const string CLIENT_ID = "&client_id=";
            public const string SCOPE = "&scope=ADMIN_SHEETS,ADMIN_WORKSPACES,CREATE_SHEETS,READ_SHEETS,WRITE_SHEETS";
            public const string STATE = "&state=ACUMATICA";
        }
        #endregion

        #region SSTokenPOSTRequest
        public static class SSTokenPOSTRequest
        {
            public const string ENDPOINT = "https://api.smartsheet.com/2.0/token";
            public const string CODE_PREFIX = "code=";
            public const string CODE_PREFIX_W_AMPERSAND = "&" + CODE_PREFIX;
            public const string GRANT_TYPE = "grant_type=authorization_code";
            public const string GRANT_TYPE_REFRESH = "grant_type=refresh_token";
            public const string CLIENT_ID = "&client_id=";
            public const string REFRESH_TOKEN = "&refresh_token=";
            public const string HASH = "&hash=";
            public const string POST_REQUEST = "POST";
            public const string CONTENT_TYPE = "application/x-www-form-urlencoded";
            public const string JSON_ACCESS_TOKEN = "access_token";
            public const string JSON_REFRESH_TOKEN = "refresh_token";
            public const string BAD_REQUEST = "Bad Request";
        }
        #endregion
    }
}