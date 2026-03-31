# Changelog

All notable changes to this repository are documented in this file.

The format is based on Keep a Changelog principles.

## [Unreleased] - 2026-03-31

### Added

- Added Lab 5 UI guide at `labs/lab-5-ui/2-core-guided.md`.
- Added new UI project `src/WorkshopLab.ChatUI` to the solution.
- Added screenshot automation scripts:
  - `scripts/capture-ui-chrome.mjs`
  - `scripts/capture-ui-chrome.ps1`
- Added npm scripts for screenshot capture in `package.json`:
  - `capture:screenshots`
  - `capture:screenshots:chrome`
- Added repository-level agent guidance file `AGENTS.md`.

### Changed

- Updated course maps and lab indexes to include Lab 5 in:
  - `README.md`
  - `labs/README.md`
- Updated Lab 5 docs to include screenshot references and beginner-friendly usage notes.
- Standardized screenshot naming for final response state:
  - `03-chat-ui-response-hd.png`

### Fixed

- Corrected Docker build context guidance for CI and lab instructions to use `./src` with explicit Dockerfile path.
- Added missing Authorization header guidance for Foundry production requests in `src/WorkshopLab.AgentHost/run-requests.http`.
- Added explicit local `/responses` validation command to Lab 0.

### Security and Public Sharing

- Sanitized environment-specific identifiers and replaced with placeholders across docs and metadata.
- Updated `.gitignore` for public sharing hygiene (`node_modules/`, IDE/OS artifacts, local debug outputs).
- Removed legacy screenshot naming artifact (`03-chat-ui-mobile.png`) in favor of `03-chat-ui-response-hd.png`.
