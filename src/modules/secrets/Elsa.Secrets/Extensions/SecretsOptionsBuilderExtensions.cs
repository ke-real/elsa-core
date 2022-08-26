using Elsa.Activities.Http.Contracts;
using Elsa.Activities.Sql.Contracts;
using Elsa.Options;
using Elsa.Secrets.Enrichers;
using Elsa.Secrets.Handlers;
using Elsa.Secrets.Manager;
using Elsa.Secrets.Persistence;
using Elsa.Secrets.Persistence.Decorators;
using Elsa.Secrets.Providers;
using Elsa.Secrets.ValueFormatters;
using Microsoft.Extensions.DependencyInjection;

namespace Elsa.Secrets.Extensions
{
    public static class SecretsOptionsBuilderExtensions
    {
        public static ElsaOptionsBuilder AddSecrets(this ElsaOptionsBuilder elsaOptions)
        {
            elsaOptions.Services
                .AddSingleton<ISecretValueFormatter, MsSqlSecretValueFormatter>()
                .AddSingleton<ISecretValueFormatter, PostgreSqlSecretValueFormatter>()
                .AddSingleton<ISecretValueFormatter, AuthorizationHeaderSecretValueFormatter>()
                .AddSingleton<ISecretValueFormatter, OAuth2SecretValueFormatter>()
                .AddScoped<SecretsValueHandler>()
                .AddScoped<ISqlConnectionStringHandler>(x => x.GetRequiredService<SecretsValueHandler>())
                .AddScoped<ISendHttpRequestAuthorizationHeaderHandler>(x => x.GetRequiredService<SecretsValueHandler>())
                .AddScoped<IActivityInputDescriptorEnricher, SendHttpRequestAuthorizationInputDescriptorEnricher>()
                .AddScoped<IActivityInputDescriptorEnricher, ExecuteSqlQueryConnectionStringInputDescriptorEnricher>()
                .AddScoped<IActivityInputDescriptorEnricher, ExecuteSqlCommandConnectionStringInputDescriptorEnricher>()
                .AddScoped<ISecretsManager, SecretsManager>()
                .AddScoped<ISecretsProvider, SecretsProvider>()
                .Decorate<ISecretsStore, EventPublishingSecretsStore>()
                .AddNotificationHandlersFrom<DescribingActivityTypeHandler>();

            return elsaOptions;
        }
    }
}
