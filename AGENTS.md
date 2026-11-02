# Repository Agent Instructions

## App watch command

- Start the app watcher from the `app/` directory:
  - `./fake.sh Watch`
- Preferred flow:
  - `cd app && ./fake.sh Watch`

## Pre-PR checklist

Before creating a PR, always run:

1. `cd app && ./fake.sh Test` — all tests must pass
2. `cd pulumi && npm ci && npm run check` — Pulumi TypeScript must compile

These local checks require no Pulumi credentials. Pulumi previews run in CI, not locally. CI runs `pulumi up` on merge to main, so do **not** run `pulumi up` manually.
