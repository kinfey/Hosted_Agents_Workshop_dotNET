param(
    [string]$ProjectEndpoint = $env:AZURE_AI_PROJECT_ENDPOINT,
    [string]$Manifest = "src/Foundry/WorkshopLab.AgentHost/agent.yaml",
    [string]$AgentId = $env:FOUNDRY_AGENT_ID,
    [string]$ModelDeploymentName = $env:MODEL_DEPLOYMENT_NAME
)

if ([string]::IsNullOrWhiteSpace($ProjectEndpoint)) {
    throw "AZURE_AI_PROJECT_ENDPOINT must be set before applying the Foundry manifest."
}

if ([string]::IsNullOrWhiteSpace($ModelDeploymentName)) {
    throw "MODEL_DEPLOYMENT_NAME must be set before applying the Foundry manifest."
}

$args = @(
    "run",
    "--project", "src/Foundry/WorkshopLab.FoundryDeployment/WorkshopLab.FoundryDeployment.csproj",
    "--",
    "--project-endpoint", $ProjectEndpoint,
    "--manifest", $Manifest,
    "--set", "AZURE_AI_PROJECT_ENDPOINT=$ProjectEndpoint",
    "--set", "chat=$ModelDeploymentName"
)

if (-not [string]::IsNullOrWhiteSpace($AgentId)) {
    $args += @("--agent-id", $AgentId)
}

dotnet @args