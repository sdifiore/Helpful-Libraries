using Lombiq.HelpfulLibraries.AspNetCore.Security;
using Lombiq.HelpfulLibraries.OrchardCore.DependencyInjection;
using Lombiq.HelpfulLibraries.OrchardCore.Security;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.StaticFiles;
using OrchardCore.Environment.Shell;
using OrchardCore.Environment.Shell.Models;
using System.Linq;

namespace Microsoft.Extensions.DependencyInjection;

public static class SecurityOrchardCoreBuilderExtensions
{
    /// <summary>
    /// Configures the anti-forgery token to be always secure. With this configuration the token won't work in an HTTP
    /// environment so make sure that HTTPS redirection is enabled.
    /// </summary>
    public static OrchardCoreBuilder ConfigureAntiForgeryAlwaysSecure(this OrchardCoreBuilder builder) =>
        builder.ConfigureServices((services, _) =>
            services.Configure<AntiforgeryOptions>(options => options.Cookie.SecurePolicy = CookieSecurePolicy.Always));

    /// <summary>
    /// Provides some default security configuration for Orchard Core.
    /// </summary>
    /// <remarks>
    /// <para>This extension method configures the application as listed below.</para>
    /// <list type="bullet">
    ///     <item>
    ///         <description>
    ///             Add <see cref="CdnContentSecurityPolicyProvider"/> to permit using script and style resources from
    ///             some common CDNs.
    ///         </description>
    ///     </item>
    ///     <item>
    ///         <description>
    ///             Add <see cref="VueContentSecurityPolicyProvider"/> to permit script evaluation when the <c>vuejs</c>
    ///             resource is included.
    ///         </description>
    ///     </item>
    ///     <item>
    ///         <description>
    ///             Add <see cref="ContentSecurityPolicyAttributeContentSecurityPolicyProvider"/> to amend the content
    ///             security policy using the <see cref="ContentSecurityPolicyAttribute"/>.
    ///         </description>
    ///     </item>
    ///     <item>
    ///         <description>
    ///             Add <see cref="SkipContentSecurityPolicyProvider"/> to skip declaring a content security policy on
    ///             responses where it makes no sense.
    ///         </description>
    ///     </item>
    ///     <item>
    ///         <description>
    ///             Add <see cref="BrowserLinkContentSecurityPolicyProvider"/> to permit accessing other ports on
    ///             <c>localhost</c> during local development.
    ///         </description>
    ///     </item>
    ///     <item>
    ///         <description>
    ///             Make the session token's cookie always secure.
    ///         </description>
    ///     </item>
    ///     <item>
    ///         <description>
    ///             Make the anti-forgery token's cookie always secure.
    ///         </description>
    ///     </item>
    ///     <item>
    ///         <description>
    ///             Enable the <c>OrchardCore.Diagnostics</c> feature to provide custom error screens in production and
    ///             don't leak error information.
    ///         </description>
    ///     </item>
    ///     <item>
    ///         <description>
    ///            Adds <see cref="EmbeddedMediaContentSecurityPolicyProvider"/> that provides permitted hosts for the
    ///            <c>frame-src</c> directive of the <c>Content-Security-Policy</c> header, covering usual media
    ///            embedding sources like YouTube.
    ///         </description>
    ///     </item>
    ///     <item>
    ///         <description>
    ///            Adds <see cref="ExternalLoginContentSecurityPolicyProvider"/> that provides permitted hosts for the
    ///            <c>form-action</c> directive of the <c>Content-Security-Policy</c> header, covering external login
    ///            providers that require this (like Microsoft and GitHub).
    ///         </description>
    ///     </item>
    ///     <item>
    ///         <description>
    ///            Adds <see cref="ReCaptchaContentSecurityPolicyProvider"/> that provides various directives for the
    ///            <c>Content-Security-Policy</c> header, allowing using ReCaptcha captchas.
    ///         </description>
    ///     </item>
    ///     <item>
    ///         <description>
    ///            Adds <see cref="GoogleAnalyticsContentSecurityPolicyProvider"/> that provides various directives for
    ///            the <c>Content-Security-Policy</c> header, allowing using Google Analytics tracking.
    ///         </description>
    ///     </item>
    ///     <item>
    ///         <description>
    ///            Adds a middleware that supplies the <c>Content-Security-Policy</c> header.
    ///         </description>
    ///     </item>
    ///     <item>
    ///         <description>
    ///             Adds a middleware that supplies the <c>X-Content-Type-Options: nosniff</c> header.
    ///         </description>
    ///     </item>
    /// </list>
    /// <para>
    /// If you also need static file support, consider using <see cref="ConfigureSecurityDefaultsWithStaticFiles"/>
    /// instead. Alternatively, make sure to put the <c>app.UseStaticFiles()</c> call at the very end of your app
    /// configuration chain so it won't short-circuit prematurely and miss adding security headers to your static files.
    /// </para>
    /// </remarks>
    public static OrchardCoreBuilder ConfigureSecurityDefaults(
        this OrchardCoreBuilder builder,
        bool allowInlineScript = true,
        bool allowInlineStyle = false) =>
        builder.ConfigureSecurityDefaultsInner(allowInlineScript, allowInlineStyle, useStaticFiles: false);

    /// <summary>
    /// The same as <see cref="ConfigureSecurityDefaults"/>, but also registers the <see cref="StaticFileMiddleware"/>
    /// at the end of the chain, so <c>app.UseStaticFiles()</c> should not be called when this is used. This is helpful
    /// because <see cref="StaticFileMiddleware"/> short-circuits the call chain when delivering static files, so later
    /// middlewares are not executed (e.g. the <c>X-Content-Type-Options: nosniff</c> header wouldn't be added).
    /// </summary>
    public static OrchardCoreBuilder ConfigureSecurityDefaultsWithStaticFiles(
        this OrchardCoreBuilder builder,
        bool allowInlineScript = true,
        bool allowInlineStyle = false) =>
        builder.ConfigureSecurityDefaultsInner(allowInlineScript, allowInlineStyle, useStaticFiles: true);

    private static OrchardCoreBuilder ConfigureSecurityDefaultsInner(
        this OrchardCoreBuilder builder,
        bool allowInlineScript,
        bool allowInlineStyle,
        bool useStaticFiles)
    {
        builder.ApplicationServices.AddInlineStartup(
            services => services
                .AddContentSecurityPolicyProvider<CdnContentSecurityPolicyProvider>()
                .AddContentSecurityPolicyProvider<VueContentSecurityPolicyProvider>()
                .AddContentSecurityPolicyProvider<EmbeddedMediaContentSecurityPolicyProvider>()
                .AddContentSecurityPolicyProvider<ExternalLoginContentSecurityPolicyProvider>()
                .AddContentSecurityPolicyProvider<ContentSecurityPolicyAttributeContentSecurityPolicyProvider>()
                .AddContentSecurityPolicyProvider<SkipContentSecurityPolicyProvider>()
                .AddContentSecurityPolicyProvider<BrowserLinkContentSecurityPolicyProvider>()
                .AddContentSecurityPolicyProvider<ReCaptchaContentSecurityPolicyProvider>()
                .AddContentSecurityPolicyProvider<GoogleAnalyticsContentSecurityPolicyProvider>()
                .ConfigureSessionCookieAlwaysSecure(),
            (app, _, serviceProvider) =>
            {
                // Don't add any middlewares if the site is in setup mode.
                var shellSettings = serviceProvider
                    .GetRequiredService<IShellHost>()
                    .GetAllSettings()
                    .FirstOrDefault(settings => settings.Name == ShellSettings.DefaultShellName);
                if (shellSettings?.State == TenantState.Uninitialized) return;

                app
                    .UseContentSecurityPolicyHeader(allowInlineScript, allowInlineStyle)
                    .UseNosniffContentTypeOptionsHeader()
                    .UseStrictAndSecureCookies();

                if (useStaticFiles) app.UseStaticFiles();
            },
            order: 99); // Makes this service load fairly late. This should make the setup detection more accurate.

        return builder
            .ConfigureAntiForgeryAlwaysSecure()
            .AddTenantFeatures("OrchardCore.Diagnostics");
    }
}
