using PX.Data;
using PX.Objects.PM;
using PX.SM;
using System;
using System.IO;
using System.Net;
using Newtonsoft.Json.Linq;
using System.Text;
using Newtonsoft.Json;
using System.Security.Cryptography;
using System.Collections.Generic;
using PX.Data.DependencyInjection;
using Microsoft.AspNet.SignalR.Infrastructure;

namespace SmartSheetIntegration
{
    public class MyProfileMaintExt : PXGraphExtension<MyProfileMaint>, IGraphWithInitialization
    {
        [InjectDependency]
        private IConnectionManager _signalRConnectionManager { get; set; }

        #region Actions

        public PXAction<Users> requestSSToken;
        [PXButton(CommitChanges = true)]
        [PXUIField(DisplayName = SmartsheetConstants.ActionsNames.REQUEST_SS_TOKEN, MapViewRights = PXCacheRights.Update, MapEnableRights = PXCacheRights.Update)]
        protected virtual void RequestSSToken()
        {
            PMSetup pmSetupRow = PXSelect<PMSetup>.Select(Base);
            PMSetupSSExt pmSetupSSExtRow = PXCache<PMSetup>.GetExtension<PMSetupSSExt>(pmSetupRow);
            string loginScopeCompany = PXDatabase.Companies.Length > 0 ? PXAccess.GetCompanyName() : String.Empty;
            string currentScope = String.Format(",{0},{1}", Base.Accessinfo.UserName, loginScopeCompany);
            if (pmSetupSSExtRow != null && pmSetupSSExtRow.UsrSmartsheetClientID != null)
            {
                string smartsheetRedirect = SmartsheetConstants.SSCodeRequest.ENDPOINT;
                smartsheetRedirect += SmartsheetConstants.SSCodeRequest.RESPONSE_TYPE;
                smartsheetRedirect += SmartsheetConstants.SSCodeRequest.CLIENT_ID + pmSetupSSExtRow.UsrSmartsheetClientID;
                smartsheetRedirect += SmartsheetConstants.SSCodeRequest.SCOPE;
                smartsheetRedirect += SmartsheetConstants.SSCodeRequest.STATE + currentScope;

                throw new PXRedirectToUrlException(smartsheetRedirect, PXBaseRedirectException.WindowMode.InlineWindow,
                                                   string.Empty, false);
            }
            else
                throw new PXException(SmartsheetConstants.Messages.SMARTSHEET_ID_MISSING);
        }
        #endregion

        #region SmartsheetToken methods
        public void GetSmartsheetToken(string currentURL)
        {
            Guid loggedUser = Base.Accessinfo.UserID;

            Users usersRow = PXSelect<Users,
                                    Where<Users.pKID, Equal<Required<Users.pKID>>>>
                                    .Select(this.Base, loggedUser);
            UsersSSExt usersSSExtRow = PXCache<Users>.GetExtension<UsersSSExt>(usersRow);

            this.Base.UserProfile.Current = this.Base.UserProfile.Search<Users.pKID>(usersRow.PKID);

            PMSetup setupRow = PXSelect<PMSetup>.Select(this.Base);
            PMSetupSSExt setupSSExtRow = PXCache<PMSetup>.GetExtension<PMSetupSSExt>(setupRow);

            if (currentURL.IndexOf(SmartsheetConstants.SSCodeRequest.STATE, StringComparison.CurrentCultureIgnoreCase) > 0)
            {
                string ssCode = getSSCodeString(currentURL);

                if (ssCode.Trim().Length > 0)
                {
                    List<string> ssToken = getSSToken(setupSSExtRow.UsrSmartsheetClientID, ssCode, setupSSExtRow.UsrSmartsheetAppSecret);

                    if (ssToken.Count == 2)
                    {
                        usersSSExtRow.UsrSmartsheetToken = ssToken[0];
                        usersSSExtRow.UsrSmartsheetRefreshToken = ssToken[1];
                        this.Base.UserProfile.Update(usersRow);
                        this.Base.Actions.PressSave();
                    }
                }
            }
            SendRefreshCall();

            return;
        }

        /// <summary>
        /// Returns the code read from the URL parameter
        /// </summary>
        /// <param name="uRL"></param>
        /// <returns></returns>
        private string getSSCodeString(string uRL)
        {
            int codePosition = uRL.IndexOf(SmartsheetConstants.SSTokenPOSTRequest.CODE_PREFIX, StringComparison.CurrentCultureIgnoreCase);

            if (codePosition > 0)
            {
                int endOfCodePosition = uRL.IndexOf(SmartsheetConstants.SSConstants.AMPERSAND, codePosition);

                if (endOfCodePosition > 0)
                {
                    return uRL.Substring(codePosition + 5, endOfCodePosition - codePosition - 5);
                }
                else
                {
                    return uRL.Substring(codePosition + 5);
                }
            }
            return string.Empty;
        }

        public string RefreshSmartsheetToken()
        {
            Guid loggedUser = PXAccess.GetUserID();

            Users usersRow = PXSelect<Users,
                                    Where<Users.pKID, Equal<Required<Users.pKID>>>>
                                    .Select(this.Base, loggedUser);
            UsersSSExt usersSSExtRow = PXCache<Users>.GetExtension<UsersSSExt>(usersRow);

            PMSetup setupRow = PXSelect<PMSetup>.Select(this.Base);
            PMSetupSSExt setupSSExtRow = PXCache<PMSetup>.GetExtension<PMSetupSSExt>(setupRow);

            if (usersSSExtRow != null && usersSSExtRow.UsrSmartsheetRefreshToken != null)
            {
                List<string> ssToken = refreshToken(setupSSExtRow.UsrSmartsheetClientID, setupSSExtRow.UsrSmartsheetAppSecret, usersSSExtRow.UsrSmartsheetRefreshToken);

                if (ssToken.Count == 2)
                {
                    usersSSExtRow.UsrSmartsheetToken = ssToken[0];
                    usersSSExtRow.UsrSmartsheetRefreshToken = ssToken[1];
                    this.Base.UserProfile.Update(usersRow);
                    this.Base.Actions.PressSave();

                    return usersSSExtRow.UsrSmartsheetToken;
                }
            }

            return "";
        }

        private List<string> getSSToken(string smartSheetClientID, string smartSheetcode, string smartSheetAppSecret)
        {
            string hashSeedFirstPart = smartSheetAppSecret.Trim() + SmartsheetConstants.SSConstants.PIPE;
            string hashSeedSecondpart = hashSeedFirstPart + smartSheetcode.Trim();
            string sha256 = sha256Hash(hashSeedSecondpart);

            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(SmartsheetConstants.SSTokenPOSTRequest.ENDPOINT);

            string postData = SmartsheetConstants.SSTokenPOSTRequest.GRANT_TYPE;
            postData += SmartsheetConstants.SSTokenPOSTRequest.CODE_PREFIX_W_AMPERSAND + smartSheetcode.Trim();
            postData += SmartsheetConstants.SSTokenPOSTRequest.CLIENT_ID + smartSheetClientID.Trim();
            postData += SmartsheetConstants.SSTokenPOSTRequest.HASH + sha256;
            byte[] byteArray = Encoding.UTF8.GetBytes(postData);

            request.Method = SmartsheetConstants.SSTokenPOSTRequest.POST_REQUEST;
            request.ContentType = SmartsheetConstants.SSTokenPOSTRequest.CONTENT_TYPE;
            request.ContentLength = byteArray.Length;

            try
            {
                Stream requestStream = request.GetRequestStream();
                requestStream.Write(byteArray, 0, byteArray.Length);

                HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                requestStream = response.GetResponseStream();

                StreamReader reader = new StreamReader(requestStream);
                string responseFromServer = reader.ReadToEnd();

                JObject jsonDataArray = (JObject)JsonConvert.DeserializeObject(responseFromServer);
                List<string> tokenStrings = new List<string>();
                string ssAccessToken = "";
                string ssRefreshToken = "";

                if (jsonDataArray[SmartsheetConstants.SSTokenPOSTRequest.JSON_ACCESS_TOKEN].Value<string>() != null
                        && jsonDataArray[SmartsheetConstants.SSTokenPOSTRequest.JSON_ACCESS_TOKEN].Value<string>() != "")
                {
                    ssAccessToken = jsonDataArray[SmartsheetConstants.SSTokenPOSTRequest.JSON_ACCESS_TOKEN].Value<string>();
                    ssRefreshToken = jsonDataArray[SmartsheetConstants.SSTokenPOSTRequest.JSON_REFRESH_TOKEN].Value<string>();
                    tokenStrings.Add(ssAccessToken);
                    tokenStrings.Add(ssRefreshToken);
                }

                requestStream.Close();
                reader.Close();
                response.Close();

                return tokenStrings;
            }
            catch (Exception e)
            {
                throw new PXException(e.Message);
            }
        }

        private List<string> refreshToken(string smartSheetClientID, string smartSheetAppSecret, string refreshTokenValue)
        {
            string hashSeedFirstPart = smartSheetAppSecret.Trim() + SmartsheetConstants.SSConstants.PIPE;
            string hashSeedSecondpart = hashSeedFirstPart + refreshTokenValue.Trim();
            string sha256 = sha256Hash(hashSeedSecondpart);

            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(SmartsheetConstants.SSTokenPOSTRequest.ENDPOINT);

            string postData = SmartsheetConstants.SSTokenPOSTRequest.GRANT_TYPE_REFRESH;
            postData += SmartsheetConstants.SSTokenPOSTRequest.CLIENT_ID + smartSheetClientID.Trim();
            postData += SmartsheetConstants.SSTokenPOSTRequest.REFRESH_TOKEN + refreshTokenValue.Trim(); ;
            postData += SmartsheetConstants.SSTokenPOSTRequest.HASH + sha256;
            byte[] byteArray = Encoding.UTF8.GetBytes(postData);

            request.Method = SmartsheetConstants.SSTokenPOSTRequest.POST_REQUEST;
            request.ContentType = SmartsheetConstants.SSTokenPOSTRequest.CONTENT_TYPE;
            request.ContentLength = byteArray.Length;

            try
            {
                Stream requestStream = request.GetRequestStream();
                requestStream.Write(byteArray, 0, byteArray.Length);

                HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                requestStream = response.GetResponseStream();

                StreamReader reader = new StreamReader(requestStream);
                string responseFromServer = reader.ReadToEnd();

                JObject jsonDataArray = (JObject)JsonConvert.DeserializeObject(responseFromServer);

                List<string> tokenStrings = new List<string>();
                string ssAccessToken = "";
                string ssRefreshToken = "";
                if (jsonDataArray[SmartsheetConstants.SSTokenPOSTRequest.JSON_ACCESS_TOKEN].Value<string>() != null
                        && jsonDataArray[SmartsheetConstants.SSTokenPOSTRequest.JSON_ACCESS_TOKEN].Value<string>() != "")
                {
                    ssAccessToken = jsonDataArray[SmartsheetConstants.SSTokenPOSTRequest.JSON_ACCESS_TOKEN].Value<string>();
                    ssRefreshToken = jsonDataArray[SmartsheetConstants.SSTokenPOSTRequest.JSON_REFRESH_TOKEN].Value<string>();
                    tokenStrings.Add(ssAccessToken);
                    tokenStrings.Add(ssRefreshToken);
                }

                requestStream.Close();
                reader.Close();
                response.Close();

                return tokenStrings;

            }
            catch (Exception e)
            {
                throw new PXException(e.Message);
            }
        }

        /// <summary>
        /// Hash generator
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        private string sha256Hash(String value)
        {
            StringBuilder Sb = new StringBuilder();

            using (SHA256 hash = SHA256Managed.Create())
            {
                Encoding enc = Encoding.UTF8;
                Byte[] result = hash.ComputeHash(enc.GetBytes(value));

                foreach (Byte b in result)
                    Sb.Append(b.ToString("x2"));
            }

            return Sb.ToString();
        }
        #endregion

        private void SendRefreshCall()
        {
            this.Base.Actions.PressCancel();
            this.Base.UserProfile.Cache.Clear();
            this.Base.UserProfile.Cache.ClearQueryCache();
        }
    }
}