using PX.Common;
using PX.Data;
using PX.Objects.PM;
using PX.SM;
using Smartsheet.Api;
using Smartsheet.Api.Models;
using Smartsheet.Api.OAuth;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SmartSheetIntegration
{
    public class SetupMaintExt : PXGraphExtension<SetupMaint>
    {
        public override void Initialize()
        {
            base.Initialize();
            MappingSetup.Cache.AllowDelete = false;
            MappingSetup.Cache.AllowInsert = false;
            TemplateSetup.Cache.AllowDelete = false;
            TemplateSetup.Cache.AllowInsert = false;
        }

        #region DataMembers

        public PXSelect<PMTemplateListSS>
            TemplateSetup;

        public PXSelect<
            PMSSMapping, 
            Where<PMSSMapping.templateSS, Equal<Current<PMTemplateListSS.templateSS>>>> 
            MappingSetup;

        #endregion

        #region Events

        #region PMSetup

        protected virtual void PMSetup_RowPersisting(PXCache sender, PXRowPersistingEventArgs e)
        {
            if (e.Row == null)
            {
                return;
            }

            PMSetup pmSetupRow = (PMSetup)e.Row;
            PMSetupSSExt pmSetupSSExtRow = PXCache<PMSetup>.GetExtension<PMSetupSSExt>(pmSetupRow);

            if (pmSetupSSExtRow.UsrSmartsheetClientID != null
                    && String.IsNullOrEmpty(pmSetupSSExtRow.UsrDefaultRateTableID))
            {
                throw new PXRowPersistingException(typeof(PMSetupSSExt.usrDefaultRateTableID).Name, null,
                                                          ErrorMessages.FieldIsEmpty);
            }
        }
        protected virtual void PMSetup_RowSelected(PXCache cache, PXRowSelectedEventArgs e)
        {
            if (e.Row == null)
            {
                return;
            }

            PMSetup pmSetup = e.Row as PMSetup;

            Dictionary<string, string> fields = new Dictionary<string, string>();
            foreach (PMTemplateListSS templateRow in TemplateSetup.Select())
            {
                fields.Add(templateRow.TemplateSS, templateRow.TemplateName);
            }
            PXStringListAttribute.SetList<PMSetupSSExt.usrSSTemplate>(cache, pmSetup, fields.Keys.ToArray(), fields.Values.ToArray());
        }

        #endregion

        #region PMSMapping

        protected void PMSSMapping_RowInserted(PXCache cache, PXRowInsertedEventArgs e)
        {
            if (e.Row == null)
            {
                return;
            }

            PMSSMapping mappingRow = (PMSSMapping)e.Row;
            PMTemplateListSS pmTemplateListSSRecord = this.TemplateSetup.Current;
            mappingRow.TemplateSS = pmTemplateListSSRecord.TemplateSS;
        }

        #endregion

        #region PMTemplateListSS

        protected virtual void PMTemplateListSS_TemplateDefault_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
        {
            PMTemplateListSS row = e.Row as PMTemplateListSS;
            if (row == null) return;
            object oldValue = sender.GetValue<PMTemplateListSS.templateDefault>(row);
            if (row.TemplateDefault != null && (bool)e.NewValue == true)
            {
                PXResultset<PMSSMapping> pmSSMappingRows = this.MappingSetup.Select();
                if (pmSSMappingRows.Count() > 0)
                {
                    PMSSMapping pmSSMappingRecord = pmSSMappingRows.RowCast<PMSSMapping>().Where(_ => !String.IsNullOrEmpty(_.NameAcu)).FirstOrDefault();
                    if (pmSSMappingRecord != null)
                    {
                        return;
                    }
                }
                e.NewValue = oldValue;
                sender.RaiseExceptionHandling<PMTemplateListSS.templateDefault>(e.Row, oldValue, new PXSetPropertyException(SmartsheetConstants.Messages.ERROR_TEMPLATE_DEFAULT, PXErrorLevel.RowError));
            }
        }

        protected virtual void PMTemplateListSS_TemplateDefault_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
        {
            PMTemplateListSS row = e.Row as PMTemplateListSS;
            if (row == null || row.TemplateDefault == null) return;

            PMSetup setupRecord = this.Base.Setup.Current as PMSetup;
            PMSetupSSExt pmSetupSSExt = PXCache<PMSetup>.GetExtension<PMSetupSSExt>(setupRecord);

            if ((bool)row.TemplateDefault)
            {
                PMTemplateListSS pmSSMappingRecord = this.TemplateSetup.Select().RowCast<PMTemplateListSS>().Where(_ => (bool)_.TemplateDefault == true && _.TemplateSS != row.TemplateSS).FirstOrDefault();
                if (pmSSMappingRecord != null)
                {
                    pmSSMappingRecord.TemplateDefault = false;
                }
                pmSetupSSExt.UsrSSTemplate = row.TemplateSS;
            }
            else
            {
                pmSetupSSExt.UsrSSTemplate = null;

            }
            this.Base.Setup.Update(setupRecord);
            TemplateSetup.View.RequestRefresh();
        }

        #endregion

        #endregion

        #region Methods
        /// <summary>
        /// Gets the columns of the Sheet in SmartSheet
        /// </summary>
        /// <param name="refreshedToken">Smartsheet Token</param>
        /// <returns></returns>
        public Dictionary<string, string> GetColumnSheet(string refreshedToken = "")
        {
            PMTemplateListSS pmTemplateListSSRecord = this.TemplateSetup.Current;
            if (pmTemplateListSSRecord == null)
            {
                throw new PXException(SmartsheetConstants.Messages.ERROR_SETUP);
            }

            Users userRecord = PXSelect<
                Users,
                Where<Users.pKID, Equal<Required<AccessInfo.userID>>>>
                .Select(this.Base, this.Base.Accessinfo.UserID);
            if (userRecord == null)
            {
                throw new PXException(SmartsheetConstants.Messages.ERROR_USER);
            }

            UsersSSExt userRecordSSExt = PXCache<Users>.GetExtension<UsersSSExt>(userRecord);
            if (userRecordSSExt == null)
            {
                throw new PXException(SmartsheetConstants.Messages.ERROR_USEREXT);
            }

            Dictionary<string, string> currentColumnMap = new Dictionary<string, string>();

            try
            {
                Token token = new Token();

                token.AccessToken = (String.IsNullOrEmpty(refreshedToken)) ? userRecordSSExt.UsrSmartsheetToken : refreshedToken;

                SmartsheetClient smartsheetClient = new SmartsheetBuilder()
                                                        .SetAccessToken(token.AccessToken)
                                                        .SetDateTimeFixOptOut(true) //See NOTE ON 2.93.0 RELEASE on https://github.com/smartsheet-platform/smartsheet-csharp-sdk
                                                        .Build();

                if (!String.IsNullOrEmpty(pmTemplateListSSRecord.TemplateSS))
                {
                    Sheet sheet = new Sheet.CreateSheetFromTemplateBuilder(SmartsheetConstants.Messages.NAME_PROJECT_TEMP_SMARTSHEET, Convert.ToInt64(pmTemplateListSSRecord.TemplateSS)).Build();

                    //A temporary project is created in order to obtain the columns content
                    sheet = smartsheetClient.SheetResources.CreateSheet(sheet);

                    Sheet newlyCreatedSheet = smartsheetClient.SheetResources.GetSheet((long)sheet.Id, null, null, null, null, null, null, null);

                    currentColumnMap = new Dictionary<string, string>();
                    foreach (Column currentColumn in newlyCreatedSheet.Columns)
                    {
                        currentColumnMap.Add(currentColumn.Title, currentColumn.Type.ToString());
                    }

                    smartsheetClient.SheetResources.DeleteSheet((long)sheet.Id);
                }
                return currentColumnMap;
            }
            catch (Exception e)
            {
                if (e.Message.Contains(SmartsheetConstants.SSConstants.EXPIRED_TOKEN_MESSAGE))
                {
                    MyProfileMaint profileMaintGraph = PXGraph.CreateInstance<MyProfileMaint>();
                    MyProfileMaintExt graphExtended = profileMaintGraph.GetExtension<MyProfileMaintExt>();
                    string updatedToken = graphExtended.RefreshSmartsheetToken();
                    return GetColumnSheet(updatedToken);
                }
                else
                {
                    throw new PXException(e.Message);
                }
            }

        }

        /// <summary>
        /// Gets the SmartSheet templates
        /// </summary>
        /// <param name="refreshedToken">Smartsheet Token</param>
        /// <returns></returns>
        public Dictionary<string, string> GetTemplateSS(string refreshedToken = "")
        {
            Dictionary<string, string> templateSS = new Dictionary<string, string>();
            Users userRecord = PXSelect<
                Users,
                Where<Users.pKID, Equal<Required<AccessInfo.userID>>>>
                .Select(this.Base, this.Base.Accessinfo.UserID);
            if (userRecord == null)
            {
                throw new PXException(SmartsheetConstants.Messages.ERROR_USER);
            }
            UsersSSExt userRecordSSExt = PXCache<Users>.GetExtension<UsersSSExt>(userRecord);

            try
            {
                Token token = new Token();

                token.AccessToken = (String.IsNullOrEmpty(refreshedToken)) ? userRecordSSExt.UsrSmartsheetToken : refreshedToken;

                SmartsheetClient smartsheetClient = new SmartsheetBuilder()
                                                        .SetAccessToken(token.AccessToken)
                                                        .SetDateTimeFixOptOut(true) //See NOTE ON 2.93.0 RELEASE on https://github.com/smartsheet-platform/smartsheet-csharp-sdk
                                                        .Build();
                PaginatedResult<Template> templates = smartsheetClient.TemplateResources.ListPublicTemplates(null);
                if (templates.TotalCount > 0)
                {
                    foreach (Template dataTemplate in templates.Data)
                    {
                        templateSS.Add(dataTemplate.Id.ToString(), dataTemplate.Name);
                    }
                }
                return templateSS;
            }
            catch (Exception e)
            {
                if (e.Message.Contains(SmartsheetConstants.SSConstants.EXPIRED_TOKEN_MESSAGE))
                {
                    MyProfileMaint profileMaintGraph = PXGraph.CreateInstance<MyProfileMaint>();
                    MyProfileMaintExt graphExtended = profileMaintGraph.GetExtension<MyProfileMaintExt>();
                    string updatedToken = graphExtended.RefreshSmartsheetToken();
                    return GetTemplateSS(updatedToken);
                }
                else
                {
                    throw new PXException(e.Message);
                }
            }
        }

        /// <summary>
        /// Inserts the columns that the SmartSheet Template has in Acumatica
        /// </summary>
        /// <param name="mappingCache"> </param>
        public void InsertTemplate(PXCache<PMSSMapping> mappingCache)
        {
            Dictionary<string, string> columnSheetSS = GetColumnSheet();
            this.Base.Caches<PMSSMapping>().Clear();

            int cont = 0;
            foreach (var sheetRow in columnSheetSS)
            {
                if (mappingCache.GetValue<PMSSMapping.cellSSID>(sheetRow.Value.ToString()) == null)
                {
                    PMSSMapping mappingRow = (PMSSMapping)mappingCache.Insert(new PMSSMapping
                    {
                        CellSSID = cont++,
                        NameSS = sheetRow.Key,
                        NameAcu = null,
                        CellFormat = sheetRow.Value
                    });
                }
            }
        }
        #endregion

        #region Actions
        public PXAction<PMSetup> LoadTemplateSS;
        [PXUIField(DisplayName = SmartsheetConstants.ActionsNames.LOAD_TEMPLATE_SMARTSHEET, MapEnableRights = PXCacheRights.Update, MapViewRights = PXCacheRights.Update)]
        [PXButton(CommitChanges = true)]
        public virtual void loadTemplateSS()
        {
            PMSetup pmSetupRow = this.Base.Setup.Current;
            Dictionary<string, string> templateSS = GetTemplateSS();
            PXCache<PMTemplateListSS> pmtemplateListCache = this.Base.Caches<PMTemplateListSS>();

            foreach (PMTemplateListSS item in TemplateSetup.Select())
            {
                pmtemplateListCache.Delete(item);
            }

            foreach (var templateRow in templateSS)
            {
                PMTemplateListSS ci = (PMTemplateListSS)pmtemplateListCache.Insert(new PMTemplateListSS
                {
                    TemplateSS = templateRow.Key.ToString(),
                    TemplateName = templateRow.Value.ToString(),
                    TemplateDefault = false
                });
            }
        }

        public PXAction<PMSetup> LoadTemplateColumnsSS;
        [PXUIField(DisplayName = SmartsheetConstants.ActionsNames.LOAD_TEMPLATE_COLUMNS_SMARTSHEET, MapEnableRights = PXCacheRights.Update, MapViewRights = PXCacheRights.Update)]
        [PXButton(CommitChanges = true)]
        public virtual void loadTemplateColumnsSS()
        {
            PMSetup pmSetupRow = this.Base.Setup.Current;
            if (pmSetupRow == null) 
            {
                throw new PXException(SmartsheetConstants.Messages.ERROR_SETUP);
            }

            PXCache<PMSSMapping> mappingCache = this.Base.Caches<PMSSMapping>();

            if (MappingSetup.Select().Count() == 0)
            {
                InsertTemplate(mappingCache);
            }
            else
            {
                WebDialogResult result = this.Base.Setup.View.Ask(this.Base.Setup.Current, SmartsheetConstants.Messages.CONFIRM_HEADER, SmartsheetConstants.Messages.CONFIRM_RELOAD_VALUES, MessageButtons.YesNoCancel, MessageIcon.Warning);

                if (result == WebDialogResult.Yes)
                {
                    foreach (PMSSMapping item in MappingSetup.Select())
                    {
                        mappingCache.Delete(item);
                    }
                    mappingCache.Persist(PXDBOperation.Delete);
                    InsertTemplate(mappingCache);
                }
            }
        }
        #endregion
    }
}