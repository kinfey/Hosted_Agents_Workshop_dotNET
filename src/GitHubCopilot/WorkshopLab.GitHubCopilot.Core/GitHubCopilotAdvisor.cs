using System.Text;

namespace WorkshopLab.GitHubCopilot.Core;

public sealed class GitHubCopilotAdvisor
{
    public string RecommendCopilotAgentShape(
        string goal,
        string needsLocalTools,
        string needsAgui,
        string needsFoundryDeployment)
    {
        var requiresTools = IsAffirmative(needsLocalTools);
        var requiresAgui = IsAffirmative(needsAgui);
        var requiresFoundry = IsAffirmative(needsFoundryDeployment);

        var shape = requiresAgui
            ? "GitHub Copilot agent with AG-UI endpoint"
            : "GitHub Copilot agent console prototype";

        var reasons = new List<string>();

        if (requiresTools)
        {
            reasons.Add("deterministic .NET tools should stay in the Core project and be exposed through Agent Framework function tools");
        }

        if (requiresAgui)
        {
            reasons.Add("AG-UI gives the client a protocol-level stream instead of a custom Blazor Server chat page");
        }

        if (requiresFoundry)
        {
            reasons.Add("Foundry deployment should be handled as a separate packaging and registration concern");
        }

        if (reasons.Count == 0)
        {
            reasons.Add("the scenario can start as a small local Copilot-backed prototype before adding hosting concerns");
        }

        return string.Join(
            Environment.NewLine,
            [
                $"Recommended shape: {shape}",
                $"Scenario goal: {goal}",
                $"Why: {string.Join("; ", reasons)}.",
                "Suggested next step: run the AppHost locally, connect with the AGUI client, then add deployment automation once the AG-UI contract is stable."
            ]);
    }

    public string BuildCopilotLaunchChecklist(string agentName, string endpointPath)
    {
        var normalizedAgentName = string.IsNullOrWhiteSpace(agentName) ? "github-copilot-readiness-coach" : agentName.Trim();
        var normalizedEndpointPath = string.IsNullOrWhiteSpace(endpointPath) ? "/ag-ui" : endpointPath.Trim();

        var checklist = new[]
        {
            $"1. Confirm the Copilot CLI is installed and authenticated before running '{normalizedAgentName}'.",
            "2. Set GITHUB_COPILOT_MODEL to gpt-5.5 unless the lab step asks for a different model.",
            $"3. Start the AppHost and verify that the AG-UI endpoint is available at '{normalizedEndpointPath}'.",
            "4. Run the AGUI client and send one prompt that exercises a Core function tool.",
            "5. Keep deterministic advisor logic in WorkshopLab.GitHubCopilot.Core and avoid placing business rules in the UI client.",
            "6. Package the AppHost only after local Copilot and AG-UI validation succeeds.",
            "7. Document one sample prompt, one expected response shape, and one troubleshooting path for workshop users."
        };

        return string.Join(Environment.NewLine, checklist);
    }

    public string TroubleshootCopilotAgui(string symptom)
    {
        var normalized = symptom?.Trim() ?? string.Empty;

        if (normalized.Contains("copilot", StringComparison.OrdinalIgnoreCase) || normalized.Contains("login", StringComparison.OrdinalIgnoreCase) || normalized.Contains("auth", StringComparison.OrdinalIgnoreCase))
        {
            return "Check that the GitHub Copilot CLI is installed, available on PATH, and authenticated with an account that has Copilot access.";
        }

        if (normalized.Contains("ag-ui", StringComparison.OrdinalIgnoreCase) || normalized.Contains("agui", StringComparison.OrdinalIgnoreCase) || normalized.Contains("stream", StringComparison.OrdinalIgnoreCase))
        {
            return "Confirm the AppHost is running, AGUI_SERVER_URL points to the AppHost base URL, and the client endpoint path matches the server mapping, usually /ag-ui.";
        }

        if (normalized.Contains("model", StringComparison.OrdinalIgnoreCase) || normalized.Contains("gpt", StringComparison.OrdinalIgnoreCase))
        {
            return "Set GITHUB_COPILOT_MODEL to gpt-5.5 and restart the AppHost so the Copilot session uses the expected model.";
        }

        return "Start with the local path: verify Copilot CLI authentication, run the AppHost, connect the AGUI client, then test a simple prompt before adding deployment steps.";
    }

    private static bool IsAffirmative(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return false;
        }

        return value.Trim().Equals("yes", StringComparison.OrdinalIgnoreCase)
            || value.Trim().Equals("true", StringComparison.OrdinalIgnoreCase)
            || value.Trim().Equals("y", StringComparison.OrdinalIgnoreCase);
    }
}
