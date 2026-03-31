# Lab Instructions

These labs walk you through building, testing, deploying, and integrating a Microsoft Foundry Hosted Agent.

You will start by getting the environment ready and running the agent locally, then move through Copilot customization, implementation work, CI validation, deployment, and UI integration.

## Before You Start

Before you begin the labs, make sure the learner has access to the accounts, tools, and permissions the workshop assumes.

- An Azure subscription. A trial subscription is fine if it can create and use Azure AI Foundry resources, or you can bring your own subscription.
- Permission to sign in to Azure CLI and Azure Developer CLI with the account you will use for the workshop.
- Sufficient Azure access to work with the target resource group and Azure AI Foundry project. At minimum, the learner should be able to view resources, use the project endpoint, and deploy or use a chat model.
- A GitHub account, since later labs use GitHub Actions and repository-based workflow automation.
- Permission to create or update GitHub Actions workflows in the repo you are using for the workshop.

If you are running this in a shared enterprise environment, confirm the learner knows which subscription, resource group, Foundry project, and model deployment they are expected to use before starting Lab 0.

## Labs

| Lab | Topic | Outcome |
| --- | --- | --- |
| Lab 0 | Foundry setup | Dev container, local run, and `/responses` validation |
| Lab 1 | Copilot config | Repo-specific instructions and a Hosted Agent review skill |
| Lab 2 | Implementation shape | Add or improve a local tool in the hosted agent |
| Lab 3 | CI | Build, test, and container validation in GitHub Actions |
| Lab 4 | Deploy | Prepare and publish a deployable hosted-agent bundle |
| Lab 5 | UI | Build a chat UI and validate end-to-end agent responses |

Each lab is a guided exercise written for beginners and intended to be completed in sequence.