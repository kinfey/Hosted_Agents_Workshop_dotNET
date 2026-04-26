# GitHub Copilot Hosted Agent Deployment

This guide documents the deployment flow for the GitHub Copilot variant in `src/GitHubCopilot`.

It combines the hosted-agent deployment shape from the .NET samples with the GitHub Copilot hosted-agent ideas from the Python bring-your-own invocations sample, while matching this repository's current .NET implementation.

## What This Project Runs

`src/GitHubCopilot` contains four projects:

- `WorkshopLab.GitHubCopilot.Core` - deterministic readiness guidance and troubleshooting logic.
- `WorkshopLab.GitHubCopilot.AppHost` - ASP.NET Core host that creates a GitHub Copilot-backed `AIAgent` and exposes it through AG-UI at `/ag-ui`.
- `WorkshopLab.GitHubCopilot.AGUI` - console client that connects to the AG-UI endpoint.
- `WorkshopLab.GitHubCopilot.FoundryDeployment` - helper CLI for applying a hosted-agent definition to Microsoft Foundry.

The AppHost uses:

- `GitHub.Copilot.SDK` with an installed Copilot CLI executable.
- `Microsoft.Agents.AI.GitHub.Copilot`.
- `Microsoft.Agents.AI.Hosting.AGUI.AspNetCore`.
- model `gpt-5.5` by default.

## Important Difference From The Python Sample

The Python GitHub Copilot sample uses `azure-ai-agentserver-invocations`, accepts `POST /invocations`, and authenticates with `GITHUB_TOKEN`.

This .NET workshop variant uses the GitHub Copilot SDK through a Copilot CLI executable and exposes AG-UI with `MapAGUI`. It still keeps `GITHUB_TOKEN` in the deployment environment so the hosted-agent configuration stays compatible with the GitHub Copilot sample and future token-based auth changes. Because of that:

- Configure `GITHUB_TOKEN` as a user-supplied secret, even though the current AppHost path primarily resolves Copilot through the CLI.
- Configure `COPILOT_CLI_PATH` when the `copilot` executable is not already on `PATH`.
- A hosted container must include or otherwise provide the Copilot CLI executable before the AppHost can start successfully.

## Deployment Files

The GitHub Copilot deployment files are split between the app source and an isolated manifest directory:

```text
hosted_agent/manifests/GitHubCopilot/
└── agent.manifest.yaml

src/GitHubCopilot/
├── Dockerfile
├── .dockerignore
├── .env.example
├── agent.yaml
├── WorkshopLab.GitHubCopilot.AppHost/
├── WorkshopLab.GitHubCopilot.AGUI/
├── WorkshopLab.GitHubCopilot.Core/
└── WorkshopLab.GitHubCopilot.FoundryDeployment/
```

Keep `hosted_agent/manifests/GitHubCopilot` separate from the generated agent project. `azd ai agent init` fails when the generated target is inside the manifest directory. Use `src/GitHubCopilot/agent.yaml` with the repository's .NET deployment helper after you have a container image URI.

## Local Validation

From the repository root:

```bash
dotnet build WorkshopLab.sln
```

Set the local configuration. If `copilot` is already on `PATH`, omit `COPILOT_CLI_PATH`.

```bash
export GITHUB_COPILOT_MODEL="gpt-5.5"
export AGUI__EndpointPath="/ag-ui"
export GITHUB_TOKEN="github_pat_..."
export COPILOT_CLI_PATH="/full/path/to/copilot"
```

Start the AppHost:

```bash
dotnet run --project src/GitHubCopilot/WorkshopLab.GitHubCopilot.AppHost --urls http://localhost:8888
```

In another terminal, connect the AG-UI client:

```bash
export AGUI_SERVER_URL="http://localhost:8888/ag-ui"
dotnet run --project src/GitHubCopilot/WorkshopLab.GitHubCopilot.AGUI
```

Expected result: the client accepts prompts and streams responses from the GitHub Copilot readiness coach.

## azd Flow

The .NET hosted-agent samples use `azd ai agent init -m <manifest>` to generate deployment scaffolding from the manifest, then deploy with `azd up` or `azd deploy`.

From the repository root, create or reuse an output directory that is not inside the manifest directory:

```bash
mkdir -p hosted_agent/GitHubCopilot
cd hosted_agent/GitHubCopilot
azd ai agent init -m ../manifests/GitHubCopilot/agent.manifest.yaml
```

Set the values used by the hosted AppHost:

```bash
azd env set GITHUB_COPILOT_MODEL "gpt-5.5"
azd env set AGUI__EndpointPath "/ag-ui"
azd env set GITHUB_TOKEN "github_pat_..."
azd env set COPILOT_CLI_PATH "/usr/local/bin/copilot"
```

Then provision and deploy:

```bash
azd provision
azd deploy
```

Use the Foundry extension or the Foundry portal to start the hosted container and inspect status.

## ACR Remote Build Flow

ACR remote build is recommended, especially on Apple Silicon, because Microsoft Foundry hosted containers must run as `linux/amd64`.

From the repository root:

```bash
tag=$(date -u +%Y%m%d%H%M%S)
az acr build \
  --registry <your-acr-name> \
  --image github-copilot-readiness-coach:$tag \
  --platform linux/amd64 \
  --source-acr-auth-id "[caller]" \
  --file src/GitHubCopilot/Dockerfile \
  src/GitHubCopilot
```

The image URI will look like this:

```text
<your-acr-name>.azurecr.io/github-copilot-readiness-coach:<tag>
```

## Apply The Agent Definition With The Helper

After the image exists in ACR, apply `src/GitHubCopilot/agent.yaml` through the helper project.

```bash
export AZURE_AI_PROJECT_ENDPOINT="https://<account>.services.ai.azure.com/api/projects/<project>"
export AGENT_IMAGE="<your-acr-name>.azurecr.io/github-copilot-readiness-coach:<tag>"

dotnet run \
  --project src/GitHubCopilot/WorkshopLab.GitHubCopilot.FoundryDeployment/WorkshopLab.GitHubCopilot.FoundryDeployment.csproj \
  -- \
  --project-endpoint "$AZURE_AI_PROJECT_ENDPOINT" \
  --manifest src/GitHubCopilot/agent.yaml \
  --set AGENT_IMAGE="$AGENT_IMAGE" \
  --set GITHUB_TOKEN="github_pat_..." \
  --set GITHUB_COPILOT_MODEL="gpt-5.5" \
  --set AGUI_ENDPOINT_PATH="/ag-ui" \
  --set COPILOT_CLI_PATH="/usr/local/bin/copilot"
```

If you are updating an existing agent, set `FOUNDRY_AGENT_ID` or pass `--agent-id <agent-id>`.

## Invoke And Monitor

For local validation, use the AG-UI client:

```bash
export AGUI_SERVER_URL="http://localhost:8888/ag-ui"
dotnet run --project src/GitHubCopilot/WorkshopLab.GitHubCopilot.AGUI
```

For a deployed hosted agent, use the Foundry portal, Foundry extension, or Foundry MCP tools to start the container, inspect logs, and verify status.

## Production Notes

- The current .NET AppHost requires a Copilot CLI executable. The Dockerfile builds the .NET app but does not download or authenticate Copilot CLI for you.
- If your target Foundry environment only accepts `responses`, `invocations`, `mcp`, `a2a`, or `activity` protocols, add an invocations bridge before production deployment. The Python GitHub Copilot sample is the closest reference for that shape.
- Keep deterministic business rules in `WorkshopLab.GitHubCopilot.Core`; the AppHost should only wire model, tools, and protocol hosting.
- Keep image tags unique. Use timestamp tags rather than `latest`.
- Do not publish local Copilot credentials, personal paths, tenant IDs, subscription IDs, or registry names.

## Quick Troubleshooting

| Symptom | Check |
|---|---|
| `Copilot CLI was not found` | Set `COPILOT_CLI_PATH` or put `copilot` on `PATH`. |
| AG-UI client cannot connect | Confirm AppHost is running and `AGUI_SERVER_URL` points to `http://localhost:8888/ag-ui`. |
| Model error | Confirm `GITHUB_COPILOT_MODEL=gpt-5.5` or use a model available to the Copilot account. |
| Container fails after deploy | Check that the image includes the Copilot CLI path configured by `COPILOT_CLI_PATH`. |
| ACR build works locally but container fails in Foundry | Confirm the image was built with `--platform linux/amd64`. |