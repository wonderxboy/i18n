﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using i18n.Helpers;

namespace i18n
{
    /// <summary>
    /// HTTP module responsible for:
    /// 1. Implementing early URL localization
    /// 2. Installing our ResponseFilter into the ASP.NET pipeline.
    /// </summary>
    /// <remarks>
    ///     LocalizingModule can be installed like this:
    /// 
    ///         IIS7+ Integrated mode:
    /// 
    ///           &lt;system.webServer&gt;
    ///             &lt;modules&gt;
    ///               &lt;add name="i18n.LocalizingModule" type="i18n.LocalizingModule, i18n" /&gt;
    ///             &lt;/modules&gt;
    ///           &lt;/system.webServer&gt;
    /// 
    ///         IIS7 Classic mode and IIS6:
    /// 
    ///           &lt;system.web&gt;
    ///             &lt;httpModules&gt;
    ///               &lt;add name="i18n.LocalizingModule" type="i18n.LocalizingModule, i18n" /&gt; &lt;!-- #37 --&gt;
    ///             &lt;/httpModules&gt;
    ///           &lt;/system.web&gt;
    /// </remarks>
    public class LocalizingModule : IHttpModule
    {
        private IRootServices m_rootServices;

        public LocalizingModule()
        :   this(LocalizedApplication.Current)
        {}
        public LocalizingModule(
            IRootServices rootServices)
        {
            m_rootServices = rootServices;
        }

    #region [IHttpModule]

        public void Init(HttpApplication application)
        {
            DebugHelpers.WriteLine("LocalizingModule::Init -- application: {0}", application);
            
            // Wire up our event handlers into the ASP.NET pipeline.
            application.BeginRequest        += OnBeginRequest;
            application.ReleaseRequestState += OnReleaseRequestState;
        }
        public void Dispose() {}

    #endregion

    // Implementation

    // Events handlers

        /// <summary>
        /// Handler for the BeginRequest ASP.NET request pipeline event, where we inject our
        /// Early URL Localization logic.
        /// </summary>
        private void OnBeginRequest(object sender, EventArgs e)
        {
            HttpContextBase context = HttpContext.Current.GetHttpContextBase();
            DebugHelpers.WriteLine("LocalizingModule::OnBeginRequest -- sender: {0}, e:{1}, ContentType: {2},\n+++>Url: {3}\n+++>RawUrl:{4}", sender, e, context.Response.ContentType, context.Request.Url, context.Request.RawUrl);

            // Establish the language for the request. That is, we need to call
            // context.SetPrincipalAppLanguageForRequest with a language, got from the URL,
            // the i18n.langtag cookie, the Accept-Language header, or failing all that the
            // default application language.
            // · If early URL localizer configured, allow it to do it.
            if (m_rootServices.EarlyUrlLocalizerForApp != null) {
                m_rootServices.EarlyUrlLocalizerForApp.ProcessIncoming(context); }
            // · Otherwise skip the URL aspect and detemrine from the other (inferred) attributes.
            else {
                context.SetPrincipalAppLanguageForRequest(context.GetInferredLanguage()); }
        }

        /// <summary>
        /// Handler for the ReleaseRequestState ASP.NET request pipeline event.
        /// This event occurs late on in the pipeline but prior to the response being filtered.
        /// We take the opportunity to inject our i8n post-processing of the response.
        /// </summary>
        private void OnReleaseRequestState(object sender, EventArgs e)
        {
        //
            HttpContextBase context = HttpContext.Current.GetHttpContextBase();
            DebugHelpers.WriteLine("LocalizingModule::OnReleaseRequestState -- sender: {0}, e:{1}, ContentType: {2},\n+++>Url: {3}\n+++>RawUrl:{4}", sender, e, context.Response.ContentType, context.Request.Url, context.Request.RawUrl);

            // If the content type of the entity is eligible for processing...wire up our filter
            // to do the processing. The entity data will be run through the filter a bit later on
            // in the pipeline.
            if (LocalizedApplication.Current.ContentTypesToLocalize != null
                && LocalizedApplication.Current.ContentTypesToLocalize.Match(context.Response.ContentType).Success) {
                DebugHelpers.WriteLine("LocalizingModule::OnReleaseRequestState -- Installing filter");
                context.Response.Filter = new ResponseFilter(
                    context, 
                    context.Response.Filter,
                    m_rootServices.EarlyUrlLocalizerForApp,
                    m_rootServices.NuggetLocalizerForApp);
            }
        }
    }
}
