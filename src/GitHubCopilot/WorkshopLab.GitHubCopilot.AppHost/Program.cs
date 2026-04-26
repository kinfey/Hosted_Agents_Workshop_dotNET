using System.ComponentModel;
using GitHub.Copilot.SDK;
using Microsoft.Agents.AI;
using Microsoft.Agents.AI.Hosting.AGUI.AspNetCore;
using Microsoft.Extensions.AI;
using WorkshopLab.GitHubCopilot.Core;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddHttpClient().AddLogging();
builder.Services.AddAGUI();

var app = builder.Build();

var modelName = builder.Configuration["GitHubCopilot:Model"]
    ?? Environment.GetEnvironmentVariable("GITHUB_COPILOT_MODEL")
    ?? "gpt-5.5";

var endpointPath = builder.Configuration["AGUI:EndpointPath"] ?? "/ag-ui";
var copilotCliPath = ResolveCopilotCliPath(
    builder.Configuration["GitHubCopilot:CliPath"]
    ?? Environment.GetEnvironmentVariable("COPILOT_CLI_PATH")) ?? "copilot";

var advisor = new GitHubCopilotAdvisor();

[Description("Recommend how to shape a GitHub Copilot powered agent with AG-UI.")]
string RecommendCopilotAgentShape(
    [Description("The business goal for the agent.")] string goal,
    [Description("Whether the team needs deterministic local .NET function tools. Use yes or no.")] string needsLocalTools,
    [Description("Whether the team needs an AG-UI client/server experience. Use yes or no.")] string needsAgui,
    [Description("Whether the team expects a Foundry deployment flow. Use yes or no.")] string needsFoundryDeployment)
{
    return advisor.RecommendCopilotAgentShape(goal, needsLocalTools, needsAgui, needsFoundryDeployment);
}

[Description("Create a launch checklist for a GitHub Copilot and AG-UI workshop pilot.")]
string BuildCopilotLaunchChecklist(
    [Description("The short agent name.")] string agentName,
    [Description("The AG-UI endpoint path such as /ag-ui.")] string endpointPath)
{
    return advisor.BuildCopilotLaunchChecklist(agentName, endpointPath);
}

[Description("Suggest troubleshooting guidance for a GitHub Copilot AG-UI symptom.")]
string TroubleshootCopilotAgui(
    [Description("A short symptom or error description from the team.")] string symptom)
{
    return advisor.TroubleshootCopilotAgui(symptom);
}

List<AIFunction> tools =
[
    AIFunctionFactory.Create(RecommendCopilotAgentShape),
    AIFunctionFactory.Create(BuildCopilotLaunchChecklist),
    AIFunctionFactory.Create(TroubleshootCopilotAgui)
];

var instructions =
    """
    You are a GitHub Copilot and AG-UI readiness coach for a .NET workshop.

    Help teams use GitHub Copilot with the Microsoft Agent Framework, choose an AG-UI client/server shape, and troubleshoot local setup issues.

    When a user asks for implementation guidance:
    1. Clarify the goal when it is vague.
    2. Use RecommendCopilotAgentShape for architecture decisions.
    3. Use BuildCopilotLaunchChecklist for concrete onboarding steps.
    4. Use TroubleshootCopilotAgui for setup or runtime symptoms.
    5. Keep answers practical, concise, and beginner-friendly.
    """;

var sessionConfig = new SessionConfig
{
    Model = modelName,
    Streaming = true,
    OnPermissionRequest = PermissionHandler.ApproveAll,
    Tools = tools,
    SystemMessage = new SystemMessageConfig
    {
        Mode = SystemMessageMode.Append,
        Content = instructions
    }
};

await using CopilotClient copilotClient = new(new CopilotClientOptions
{
    CliPath = copilotCliPath
});
AIAgent agent = copilotClient.AsAIAgent(
    sessionConfig,
    ownsClient: false,
    id: "github-copilot-readiness-coach",
    name: "GitHubCopilotReadinessCoach",
    description: "A GitHub Copilot backed readiness coach exposed through AG-UI.");

app.MapGet("/", () => Results.Ok(new
{
    name = agent.Name,
    model = modelName,
    copilotCliPath,
    aguiEndpoint = endpointPath,
    sampleClientEnvironment = $"AGUI_SERVER_URL=http://localhost:8888{endpointPath}"
}));

app.MapAGUI(endpointPath, agent);

Console.WriteLine($"GitHub Copilot AG-UI AppHost is listening. Model: {modelName}; endpoint: {endpointPath}");
Console.WriteLine($"Using Copilot CLI: {copilotCliPath}");
Console.WriteLine("Set AGUI_SERVER_URL to the full endpoint URL, for example http://localhost:8888/ag-ui");

await app.RunAsync();

static string ResolveCopilotCliPath(string? configuredPath)
{
    if (!string.IsNullOrWhiteSpace(configuredPath))
    {
        var expandedPath = Environment.ExpandEnvironmentVariables(configuredPath);
        if (File.Exists(expandedPath))
        {
            return expandedPath;
        }

        if (!Path.IsPathRooted(expandedPath) && TryFindOnPath(expandedPath, out var configuredCommandPath))
        {
            return configuredCommandPath;
        }

        throw new FileNotFoundException(
            $"Configured Copilot CLI path was not found: {configuredPath}. Set GitHubCopilot:CliPath or COPILOT_CLI_PATH to the full copilot executable path.");
    }

    if (TryFindOnPath("copilot", out var pathCopilotCliPath))
    {
        return pathCopilotCliPath;
    }

    throw new FileNotFoundException(
        "Copilot CLI was not found. Install and authenticate GitHub Copilot CLI, or set COPILOT_CLI_PATH to the full copilot executable path.");
}

static bool TryFindOnPath(string commandName, out string resolvedPath)
{
    var pathValue = Environment.GetEnvironmentVariable("PATH") ?? string.Empty;
    var candidateNames = OperatingSystem.IsWindows()
        ? new[] { commandName, $"{commandName}.exe", $"{commandName}.cmd", $"{commandName}.bat" }
        : new[] { commandName };

    foreach (var directory in pathValue.Split(Path.PathSeparator, StringSplitOptions.RemoveEmptyEntries))
    {
        foreach (var candidateName in candidateNames)
        {
            var candidatePath = Path.Combine(directory, candidateName);
            if (File.Exists(candidatePath))
            {
                resolvedPath = candidatePath;
                return true;
            }
        }
    }

    resolvedPath = string.Empty;
    return false;
}
