﻿using System;
using System.Data;
using System.Threading.Tasks;
using Elsa.Models;
using Elsa.Scripting.Liquid.Services;
using Elsa.Serialization;
using Elsa.Services;
using Microsoft.Extensions.DependencyInjection;
using YesSql.Provider.Sqlite;

namespace Elsa.Samples.DeclarativeCompositeActivitiesConsole
{
    /// <summary>
    /// Demonstrates the use of declarative composite activities.
    /// </summary>
    class Program
    {
        static async Task Main()
        {
            // Create a service container with Elsa services.
            var services = new ServiceCollection()
                .AddElsa()
                .AddConsoleActivities()
                .BuildServiceProvider();

            // Run startup actions (not needed when registering Elsa with a Host).
            var startupRunner = services.GetRequiredService<IStartupRunner>();
            await startupRunner.StartupAsync();

            // Define a workflow.
            var workflowDefinition = new WorkflowDefinition
            {
                WorkflowDefinitionId = "SampleWorkflow",
                WorkflowDefinitionVersionId = "1",
                Version = 1,
                IsPublished = true,
                IsLatest = true,
                IsEnabled = true,
                PersistenceBehavior = WorkflowPersistenceBehavior.Suspended,
                Activities = new[]
                {
                    WriteLine("write-line-1", "==Composite Activities Demo=="),
                    new CompositeActivityDefinition
                    {
                        ActivityId = "composite-1",
                        Activities = new[]
                        {
                            WriteLine("write-line-2", "Line 1 of composite activity."),
                            WriteLine("write-line-3", "Line 2 of composite activity."),
                        },
                        Connections = new[]
                        {
                            new ConnectionDefinition("write-line-2", "write-line-3", OutcomeNames.Done)
                        }
                    },
                    WriteLine("write-line-4", "==End=="),
                },
                Connections = new[]
                {
                    new ConnectionDefinition("write-line-1", "composite-1", OutcomeNames.Done),
                    new ConnectionDefinition("composite-1", "write-line-4", OutcomeNames.Done),
                }
            };

            // Serialize workflow definition to JSON.
            var serializer = services.GetRequiredService<IContentSerializer>();
            var json = serializer.Serialize(workflowDefinition);

            // Deserialize workflow definition from JSON.
            var deserializedWorkflowDefinition = serializer.Deserialize<WorkflowDefinition>(json);

            // Materialize workflow.
            var materializer = services.GetRequiredService<IWorkflowBlueprintMaterializer>();
            var workflowBlueprint = materializer.CreateWorkflowBlueprint(deserializedWorkflowDefinition);

            // Execute workflow.
            var workflowRunner = services.GetRequiredService<IWorkflowRunner>();
            await workflowRunner.RunWorkflowAsync(workflowBlueprint);
        }

        private static ActivityDefinition WriteLine(string id, string text) =>
            new ActivityDefinition
            {
                ActivityId = id,
                Type = nameof(Activities.Console.WriteLine),
                Properties = new ActivityDefinitionProperties
                {
                    [nameof(Activities.Console.WriteLine.Text)] = new ActivityDefinitionPropertyValue
                    {
                        Syntax = LiquidExpressionHandler.SyntaxName,
                        Expression = text,
                        Type = typeof(string)
                    }
                }
            };
    }
}