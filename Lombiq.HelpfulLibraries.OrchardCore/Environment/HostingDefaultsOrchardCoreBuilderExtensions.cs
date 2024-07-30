using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

namespace Microsoft.Extensions.DependencyInjection;

public static class HostingDefaultsOrchardCoreBuilderExtensions
{
    /// <summary>
    /// Lombiq-recommended opinionated default configuration for features of a standard Orchard Core application. If
    /// any of the configuration values exist, they won't be overridden, so e.g. appsettings.json configuration will
    /// take precedence.
    /// </summary>
    /// <param name="webApplicationBuilder">The <see cref="WebApplicationBuilder"/> instance of the app.</param>
    /// <param name="hostingConfiguration">Configuration for the hosting defaults.</param>
    public static OrchardCoreBuilder ConfigureHostingDefaults(
        this OrchardCoreBuilder builder,
        WebApplicationBuilder webApplicationBuilder,
        HostingConfiguration hostingConfiguration = null)
    {
        hostingConfiguration ??= new HostingConfiguration();

        // Not using static type references for the names here because those practically never change, but we'd need to
        // add project/package references to all the affected projects.

        var ocSection = webApplicationBuilder.Configuration.GetSection("OrchardCore");

        ocSection.GetSection("OrchardCore_Tenants").AddValueIfKeyNotExists("TenantRemovalAllowed", "true");

        var logLevelSection = webApplicationBuilder.Configuration.GetSection("Logging:LogLevel");

        if (webApplicationBuilder.Environment.IsDevelopment())
        {
            logLevelSection
                .AddValueIfKeyNotExists("Default", "Debug")
                .AddValueIfKeyNotExists("System", "Information")
                .AddValueIfKeyNotExists("Microsoft", "Information");

            // Orchard Core 1.8 and prior, this can be removed after an Orchard Core upgrade to 2.0.
            // OrchardCore_Email_Smtp below is 2.0+.
            var oc18SmtpSection = ocSection.GetSection("SmtpSettings");

            if (oc18SmtpSection["Host"] == null)
            {
                oc18SmtpSection["Host"] = "127.0.0.1";
                oc18SmtpSection["RequireCredentials"] = "false";
                oc18SmtpSection["Port"] = "25";
            }

            oc18SmtpSection.AddValueIfKeyNotExists("DefaultSender", "sender@example.com");

            var smtpSection = ocSection.GetSection("OrchardCore_Email_Smtp");

            if (smtpSection["Host"] == null)
            {
                smtpSection["Host"] = "127.0.0.1";
                smtpSection["RequireCredentials"] = "false";
                smtpSection["Port"] = "25";
            }

            smtpSection.AddValueIfKeyNotExists("DefaultSender", "sender@example.com");
        }
        else
        {
            logLevelSection
                .AddValueIfKeyNotExists("Default", "Warning")
                .AddValueIfKeyNotExists("Microsoft.AspNetCore", "Warning");
        }

        if (hostingConfiguration.EnableHealthChecksInProduction && webApplicationBuilder.Environment.IsProduction())
        {
            builder.AddTenantFeatures("OrchardCore.HealthChecks");
        }

        builder
            .AddDatabaseShellsConfigurationIfAvailable(webApplicationBuilder.Configuration)
            .ConfigureSmtpSettings(overrideAdminSettings: false)
            .ConfigureSecurityDefaultsWithStaticFiles(allowInlineStyle: true);

        return builder;
    }

    /// <summary>
    /// Lombiq-recommended opinionated default configuration for features of an Orchard Core application hosted in
    /// Azure. If any of the configuration values exist, they won't be overridden, so e.g. appsettings.json
    /// configuration will take precedence.
    /// </summary>
    /// <param name="webApplicationBuilder">The <see cref="WebApplicationBuilder"/> instance of the app.</param>
    /// <param name="hostingConfiguration">Configuration for the hosting defaults.</param>
    public static OrchardCoreBuilder ConfigureAzureHostingDefaults(
        this OrchardCoreBuilder builder,
        WebApplicationBuilder webApplicationBuilder,
        AzureHostingConfiguration hostingConfiguration = null)
    {
        hostingConfiguration ??= new AzureHostingConfiguration();

        builder.ConfigureHostingDefaults(webApplicationBuilder, hostingConfiguration);

        var ocSection = webApplicationBuilder.Configuration.GetSection("OrchardCore");

        if (webApplicationBuilder.Configuration.IsAzureHosting())
        {
            builder
                .AddTenantFeatures(
                    "OrchardCore.DataProtection.Azure",
                    "Lombiq.Hosting.BuildVersionDisplay")
                .DisableResourceDebugMode();

            if (hostingConfiguration.EnableAzureMediaStorage)
            {
                // Azure Media Storage and its dependencies. Keep this updated with Orchard upgrades.
                builder.AddTenantFeatures(
                    "OrchardCore.Contents",
                    "OrchardCore.ContentTypes",
                    "OrchardCore.Liquid",
                    "OrchardCore.Media",
                    "OrchardCore.Media.Azure.Storage",
                    "OrchardCore.Media.Cache",
                    "OrchardCore.Settings");
            }
        }

        var mediaSection = ocSection.GetSection("OrchardCore_Media_Azure");

        mediaSection.AddValueIfKeyNotExists("ContainerName", "media");
        mediaSection.AddValueIfKeyNotExists("BasePath", "{{ ShellSettings.Name }}");

        if (webApplicationBuilder.Environment.IsDevelopment())
        {
            var dataProtectionSection = ocSection.GetSection("OrchardCore_DataProtection_Azure");

            dataProtectionSection.AddValueIfKeyNotExists("CreateContainer", "true");
            dataProtectionSection.AddValueIfKeyNotExists("ConnectionString", "UseDevelopmentStorage=true");

            mediaSection.AddValueIfKeyNotExists("CreateContainer", "true");
            mediaSection.AddValueIfKeyNotExists("ConnectionString", "UseDevelopmentStorage=true");
        }

        return builder;
    }
}

public class HostingConfiguration
{
    /// <summary>
    /// Gets or sets a value indicating whether to enable <c>OrchardCore.HealthChecks</c> in the Production environment.
    /// </summary>
    public bool EnableHealthChecksInProduction { get; set; } = true;
}

public class AzureHostingConfiguration : HostingConfiguration
{
    /// <summary>
    /// Gets or sets a value indicating whether to enable <c>OrchardCore.Media.Azure.Storage</c> and its
    /// dependencies when hosted in Azure.
    /// </summary>
    public bool EnableAzureMediaStorage { get; set; } = true;
}
