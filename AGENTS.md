# Repository Agent Instructions

## App watch command

- Start the app watcher from the `app/` directory:
  - `./fake.sh Watch`
- Preferred flow:
  - `cd app && ./fake.sh Watch`

## Pre-PR checklist

Before creating a PR, always run:

1. `cd app && ./fake.sh Test` — all tests must pass
2. `cd pulumi && pulumi preview` — must show expected changes with no errors

CI runs `pulumi up` on merge to main, so do **not** run `pulumi up` manually.
