# Lab 2 — Core Guided: Improve a Local Hosted Agent Tool

**Goal:** Extend the hosted agent by improving one deterministic local tool and verifying the change through tests and a local `/responses` call.

**Time:** 20 minutes

**You will need:** Lab 1 completed.

## Steps

1. Open `src/WorkshopLab.Core/HostedAgentAdvisor.cs`.
2. Review the existing tools:
   - `RecommendImplementationShape`
   - `BuildLaunchChecklist`
   - `TroubleshootHostedAgent`
3. Pick one tool to improve. Suggested option: add a stronger recommendation path for scenarios that require custom code, tool access, and workflow orchestration at the same time.
4. Update the deterministic logic in `HostedAgentAdvisor`.
5. Add or update tests in `tests/WorkshopLab.Tests/HostedAgentAdvisorTests.cs`.
6. Run:

   ```powershell
   dotnet test
   ```

7. Start the hosted agent locally.
8. Send a request to `/responses` that exercises your updated tool logic.
9. Confirm the response matches the new deterministic behavior.

**Expected result:** A real feature change lands in the hosted agent and is covered by tests.