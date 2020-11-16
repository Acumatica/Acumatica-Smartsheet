using System;
using PX.SM;
using PX.Data;
using Smartsheet.Api;
using Smartsheet.Api.Models;
using PX.Objects.CR;

namespace SmartSheetIntegration
{
	public class AccessUsersSSIExt : PXGraphExtension<AccessUsers>
    {
		/// <summary>
		/// Creates the Acumatica Employee as a Smartsheet User
		/// </summary>
		/// <param name="bAccountID">Employee ID</param>
		/// <param name="smartsheetClient">Smartsheet SDK Client</param>
		/// <returns></returns>
		public User CreateSmartsheetUser(int? bAccountID, SmartsheetClient smartsheetClient)
		{
			BAccount bAccountRecord = PXSelect<BAccount, 
										Where<BAccount.bAccountID, Equal<Required<BAccount.bAccountID>>>>
									.Select(Base, bAccountID);
			PX.Objects.CR.Contact contactRecord = PXSelect<PX.Objects.CR.Contact, 
										Where<PX.Objects.CR.Contact.contactID, Equal<Required<PX.Objects.CR.Contact.contactID>>>>
									.Select(Base, bAccountRecord.DefContactID);

			if (contactRecord == null)
			{
				throw new PXException(string.Format(SmartsheetConstants.Messages.ERROR_CONTACT, bAccountRecord.AcctName));
			}

			try
			{
				User ssUserNew = new User();
				ssUserNew.Email = contactRecord.EMail;
				ssUserNew.FirstName = contactRecord.FirstName;
				ssUserNew.LastName = contactRecord.LastName;
				ssUserNew.Admin = false;
				ssUserNew.LicensedSheetCreator = false;
				User updatedUser = smartsheetClient.UserResources.AddUser(ssUserNew, false, false);

				return updatedUser;

			}
			catch (Exception e)
			{
				PXTrace.WriteError(e.Message);
				return null;
			}
		}
	}
}
