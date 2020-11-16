using System;
using System.Threading.Tasks;
using System.Web;
using System.Web.SessionState;
using IdentityModel.Client;
using Microsoft.Owin;
using Owin;
using PX.Data;
using PX.Owin;
using PX.SM;

namespace SmartSheetIntegration
{
    public class SSAuthenticationHandler : IConditionalOwinConfigurationPart, ISessionDependentOwinConfigurationPart
    {
        public const string Prefix = "OAuthAuthenticationHandlerSS";
        private static readonly PathString PathPrefix = new PathString($"/{Prefix}");

        public static string ReturnUrl
        {
            get
            {
                var applicationpath = string.IsNullOrEmpty(HttpContext.Current.Request.ApplicationPath)
                    ? string.Empty
                    : HttpContext.Current.Request.ApplicationPath + "/";
                return HttpContext.Current.Request.Url.GetLeftPart(UriPartial.Authority) + applicationpath
                        + Prefix;
            }
        }

        public void Configure(IAppBuilder app)
        {
            app.Run(ProcessAuthorizationCode);
        }

        private Task ProcessAuthorizationCode(IOwinContext ctx)
        {
            var authorizeResponse = new AuthorizeResponse(ctx.Request.QueryString.Value);
            if (String.IsNullOrEmpty(authorizeResponse.State))
                throw new InvalidOperationException(SmartsheetConstants.Messages.SMARTSHEET_INVALID_RESPONSE);

            string[] stateinfo = HttpUtility.UrlDecode(authorizeResponse.State).Split(',');

            if ((stateinfo == null) || (stateinfo.Length != 3))
                throw new InvalidOperationException(SmartsheetConstants.Messages.SMARTSHEET_INVALID_RESPONSE);

            string currentScope = !String.IsNullOrEmpty(stateinfo[2]) ?
                                    String.Format("{0}@{1}", stateinfo[1], stateinfo[2]) : stateinfo[1];

            using (new PXLoginScope(currentScope))
            {
                var graph = PXGraph.CreateInstance<MyProfileMaint>();
                var graphExt = graph.GetExtension<MyProfileMaintExt>();
                graphExt.GetSmartsheetToken(ctx.Request.Uri.AbsoluteUri);
                ctx.Response.Write(CloseWindow());
                return Task.FromResult(0);
            }
        }

        private static string CloseWindow()
        {
            return @"
                        <html>
                            <script type='text/javascript'>
                                window.close();
                            </script>
                        </html>
                                    ";
        }

        public SessionStateBehavior GetRequiredSessionBehavior(IOwinContext ctx) => Predicate(ctx) ? SessionStateBehavior.Required : SessionStateBehavior.Default;

        public bool Predicate(IOwinContext ctx)
        {
            PathString remaining;
            return ctx.Request.Path.StartsWithSegments(PathPrefix, out remaining);
        }
    }
}