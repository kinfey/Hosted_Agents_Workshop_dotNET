using WorkshopLab.Core;

namespace WorkshopLab.Tests;

public class HostedAgentAdvisorTests
{
    private readonly HostedAgentAdvisor _advisor = new();

    [Fact]
    public void RecommendImplementationShape_ReturnsHostedAgent_WhenCustomLogicIsNeeded()
    {
        var result = _advisor.RecommendImplementationShape(
            "Onboard teams to Hosted Agents",
            "yes",
            "no",
            "no");

        Assert.Contains("Recommended implementation: Hosted agent", result);
        Assert.Contains("custom server-side logic", result);
    }

    [Fact]
    public void RecommendImplementationShape_ReturnsPromptAgent_WhenScenarioIsLightweight()
    {
        var result = _advisor.RecommendImplementationShape(
            "Prototype a simple FAQ assistant",
            "no",
            "no",
            "no");

        Assert.Contains("Recommended implementation: Prompt agent", result);
    }

    [Fact]
    public void BuildLaunchChecklist_IncludesCoreHostedAgentSteps()
    {
        var result = _advisor.BuildLaunchChecklist("triage-coach", "pilot");

        Assert.Contains("triage-coach", result);
        Assert.Contains("pilot", result);
        Assert.Contains("agent.yaml", result);
        Assert.Contains("linux/amd64", result);
    }

    [Theory]
    [InlineData("requests to /responses fail after startup", "/responses")]
    [InlineData("docker image fails on amd64", "linux/amd64")]
    [InlineData("credential login problem", "AZURE_AI_PROJECT_ENDPOINT")]
    public void TroubleshootHostedAgent_ReturnsTargetedGuidance(string symptom, string expected)
    {
        var result = _advisor.TroubleshootHostedAgent(symptom);

        Assert.Contains(expected, result);
    }
}