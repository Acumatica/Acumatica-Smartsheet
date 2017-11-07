using PX.Data;
using PX.Objects.PM;
using System;

namespace PX.SmartSheetIntegration
{
    public class SetupMaintExt : PXGraphExtension<SetupMaint>
    {
        #region Events
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
        #endregion
    }
}