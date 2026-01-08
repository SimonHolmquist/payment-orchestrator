# Architecture & Flows (Mermaid Diagrams)

This folder contains Mermaid diagrams that document the Payment Orchestrator design and its key distributed flows (Outbox, Webhooks/Inbox, Reconciliation).

## How to view

### Option A — Mermaid Chart / Online renderer
Open any `.mmd` file in a Mermaid-compatible renderer (e.g., Mermaid Chart) and preview it.

### Option B — VS Code
Install a Mermaid preview extension (e.g., “Markdown Preview Mermaid Support” or a dedicated Mermaid extension), then open the `.mmd` file and preview.

### Option C — Render to SVG/PNG (CLI)
If you want deterministic artifacts (for PRs/docs), you can render diagrams locally:

1. Install Mermaid CLI:
   ```bash
   npm i -g @mermaid-js/mermaid-cli
   ```
2. Render a diagram:
   ```bash
   mmdc -i docs/charts/system-architecture.mmd -o docs/charts/_rendered/system-architecture.svg
   ```

> Tip: keep `_rendered/` out of git unless you intentionally want committed images.

## Diagram index (recommended reading order)

1. **`system-architecture.mmd`**  
   High-level view of services, layers, workers, and infrastructure dependencies (SQL Server, RabbitMQ, MongoDB future).

2. **`api-surface.mmd`**  
   Public REST endpoints map and how they connect to Application use cases and the Domain.

3. **`domain-model.mmd`**  
   Domain model class diagram: `Payment` aggregate, `Money` value object, typed IDs, and domain events.

4. **`payment-state-machine.mmd`**  
   Payment state machine with main transitions (Created → Authorized → Captured, partial capture/refund, Unknown + reconciliation).

5. **`transactional-outbox-flow.mmd`**  
   Transactional outbox sequence: write state + outbox in the same transaction, then publish asynchronously.

6. **`webhook-inbox-flow.mmd`**  
   Webhook ingestion flow with inbox-based deduplication and safe state transitions + outbox emission.

7. **`reconciliation-flow.mmd`**  
   Reconciliation worker flow: resolve Unknown/stuck payments by querying the PSP and updating state.

8. **`sql-data-model.mmd`**  
   SQL conceptual ERD: Payments + Outbox (+ Inbox/Idempotency/Ledger as roadmap targets).

9. **`observability-correlation.mmd`**  
   Correlation/trace propagation across HTTP, DB, broker, and workers (logs/traces/metrics).

## ADR mapping (where these decisions are documented)

- Clean Architecture + DDD: ADR-0001
- Domain Events: ADR-0002
- Transactional Outbox: ADR-0003
- Reconciliation & Unknown state: ADR-0004
- Money VO: ADR-0005
- Strongly Typed IDs: ADR-0006
- Secrets & configuration: ADR-0007
- Docker Compose dev env: ADR-0008

## Maintenance guidelines

- Keep diagrams aligned with the code (especially state transitions and messaging semantics).
- Prefer “truthy” diagrams over “aspirational” ones; if something is a future step, label it explicitly.
- When adding a new major behavior (idempotency, DLQ strategy, auth), add/update both an ADR and a diagram.
