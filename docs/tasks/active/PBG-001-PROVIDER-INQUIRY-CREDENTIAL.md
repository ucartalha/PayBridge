# PBG-001 — Provider Inquiry Credential Propagation

## Status

ACTIVE

## Priority

CRITICAL

## Read First

Before changing any code, read these files in order:

1. `/AGENTS.md`
2. `/docs/architecture/SYSTEM_ARCHITECTURE.md`
3. `/docs/architecture/TRANSACTION_BOUNDARIES.md`
4. `/docs/architecture/CQRS_PIPELINE.md`
5. `/docs/flows/PAYMENT_EXECUTION_FLOW.md`
6. `/docs/flows/PROVIDER_INQUIRY_FLOW.md`
7. `/docs/infrastructure/PROVIDER_INTEGRATION.md`

These documents define the architectural constraints of this task.

Do not begin implementation before reviewing them.

## Repository Verification

Documentation is guidance, but the repository is the final implementation source.

Before editing:

1. Pull/read the current branch.
2. Locate the actual implementation files.
3. Verify that the documented behavior still matches the code.
4. If documentation and code differ, stop that part of the implementation and report the mismatch as `DOCUMENTATION DRIFT`.
5. Do not silently implement against stale documentation.