using PX.Common;

namespace SmartSheetIntegration
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
            public const string EXPIRED_TOKEN_MESSAGE = "Token Expired";
            public const string NOTFOUND_PROJECT_MESSAGE = "Not Found";
            public const string YEAR = "Y";
            public const string DAY = "D";
            public const string MONTH = "M";
            public const string LABEL_YEAR = "Year";
            public const string LABEL_DAY = "Day";
            public const string LABEL_MONTH = "Month";
        }
        #endregion

        #region ActionNames
        [PXLocalizable(ActionsNames.PREFIX)]
        public static class ActionsNames
        {
            public const string PREFIX = "Smartsheet Integration";
            public const string SYNC_SMARTSHEET_PROJECT = "Sync Smartsheet Project";
            public const string UNLINK_SMARTSHEET_PROJECT = "Unlink Smartsheet Project";
            public const string SYNC_SMARTSHEET_EMPLOYEES= "Sync SmartSheet Employees";
            public const string POPULATE_DATES= "Populate Dates";
            public const string REQUEST_SS_TOKEN = "Get Smartsheet Token";
            public const string REFRESH_SS_TOKEN = "Refresh Smartsheet Token";
            public const string EMPLOYEE_USER_SMARTSHEET = "Refresh SmartSheet Users";
            public const string LOAD_TEMPLATE_SMARTSHEET = "Load Smartsheet Templates";
            public const string LOAD_TEMPLATE_COLUMNS_SMARTSHEET = "Load Template Columns";
            
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
            public const string DEFAULT_TEMPLATE = "A Smartsheet Template must be assigned to the Project";
            public const string CONFIRM_RELOAD_VALUES = "There is a Smartsheet mapping available. Do you want to reload the values? (this will override the current content)";
            public const string CONFIRM_UNLINK_PROJECT = "The current Project does not exist in Smartsheet. Do you want to create and sync this Project?";
            public const string UNLINK_PROJECT = "The Smartsheet reference associated to this Project does not exist. Repeat the sync process in order to create a new link.";
            public const string CONFIRM_HEADER = "Confirmation";
            public const string NAME_PROJECT_TEMP_SMARTSHEET = "Smartsheet Template column";
            public const string ERROR_SETUP = "Error getting the setup fields";
            public const string ERROR_USER = "The user cannot be found";
            public const string ERROR_USEREXT = "Required fields for the connection in the user table cannot be null";
            public const string ERROR_CONTACT = "The employee {0} cannot be found in the Contacts table";
            public const string ERROR_DAYS = "Days cannot be negative";
            public const string ERROR_TEMPLATE_DEFAULT = "The template cannot be used by default because it does not have a mapping available";
            public const string DURATION_FIELDS_NOT_INDICATED = "Duration fields are not assigned in Projects Preferences";


        }
        #endregion

        #region TableNames
        [PXLocalizable(Messages.PREFIX)]
        public static class TableNames
        {
            public const string PREFIX = "Smartsheet Integration";
            public const string PMSUBTASK = "Project Template Subtasks";
            public const string PMTEMPLATELIST = "PMTemplateListSS";
            public const string PMMAPPING = "PMSSMapping";
            public const string EPUSERSLISTSS = "EPUsersListSS";
        }
        #endregion

        #region TableNames
        [PXLocalizable(Messages.PREFIX)]
        public static class ViewName
        {
            public const string TASK = "Tasks";
        }
        #endregion

        #region ColumnMapping
        [PXLocalizable(ColumnMapping.PREFIX)]
        public static class ColumnMapping
        {
            public const string TASKS_CD = "TaskCD";
            public const string PREFIX = "Smartsheet Integration";
            public const string START_DATE = "StartDate";
            public const string DURATION = "Duration";
            public const string PCT_COMPLETE = "CompletedPercent";
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
            public const string SCOPE = "&scope=ADMIN_SHEETS,ADMIN_WORKSPACES,ADMIN_USERS,CREATE_SHEETS,READ_SHEETS,READ_USERS,WRITE_SHEETS, DELETE_SHEETS";
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