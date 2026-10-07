# Playwright integration testing

Configuration and conventions for frontend integration testing via the Playwright MCP server.

## Setup

To be configured per project. Reference: https://playwright.dev/docs/intro

## Conventions

- Test files live alongside features or in a dedicated tests/ directory.
- Each unit of work with a UI component should include at least one integration test.
- Tests should run quickly and produce a clear pass/fail result. Long-running test suites should be split into focused test groups.

## MCP server

Playwright MCP server configuration goes here once set up.
