# Wikipedia Search API Application — Learning Plan

## Overview

Build a .NET 10 ASP.NET Core API that:

- Authenticates users with token-based authentication.
- Accepts CSV uploads containing 1–100 Wikipedia search keywords.
- Processes uploaded keywords after the upload is accepted.
- Stores extracted Wikipedia search information in SQL Server.
- Lets authenticated users retrieve and search only their own data.
- Includes automated tests, CI/CD, and cloud deployment.

This plan prioritizes a small, complete self-assignment over production-scale architecture. Build the required behavior first, then use the future-enhancement list to continue learning if desired.

## Scope Decision

### MVP required for the assignment

- Email/password sign-up and sign-in using ASP.NET Core Identity.
- One signed JWT access token; no refresh-token lifecycle in the MVP.
- Authenticated CSV upload with the existing 1–100 keyword validation.
- SQL Server persistence for users, uploads, keywords, processing status, and Wikipedia results.
- A simple database-backed `BackgroundService` that processes one pending keyword at a time.
- The required keyword list, detail, and search endpoints with user ownership checks.
- Focused unit and integration tests, one CI/CD workflow, and one cloud deployment.

### Deliberately deferred

Refresh-token rotation, token revocation, distributed queues, multiple architecture projects, CQRS/MediatR, generic repositories, advanced metrics, infrastructure as code, and elaborate retry administration are not required for the first complete version.

## Current Workspace State

Completed foundations:

- .NET 10 API and xUnit test projects.
- CSV parsing and validation tests.
- SQL Server, EF Core, ASP.NET Core Identity, and the initial persistence schema.
- JWT bearer validation and secure local configuration through user secrets.

Current focus: finish simple access-token issuance and the sign-up/sign-in endpoints. Refresh-token work on the current branch is experimental and should be removed when the simplified scope is implemented.

## Core Use Cases

### 1. Authentication

- Register a user.
- Sign in with credentials.
- Return an access token.
- Require the token on all keyword and reporting endpoints.

### 2. CSV Upload

- Accept a multipart CSV upload.
- Validate that the file contains 1–100 keywords.
- Store the upload and its keywords.
- Schedule the keywords for processing after accepting the upload.

### 3. Wikipedia Processing

For each keyword:

- Search Wikipedia.
- Capture the total number of matching results.
- Count displayed results with thumbnails.
- Count displayed results without thumbnails.
- Count links according to a clearly defined rule.
- Preserve the required first-page HTML.
- Track processing state and errors.

### 4. Authenticated Reporting

- List the authenticated user's uploaded keywords.
- Retrieve one keyword and its captured result.
- Search and filter data belonging to the authenticated user.
- Never expose records belonging to another user.

## Requirements That Need Clarification

Resolve these questions before finalizing the schema or extraction logic.

### What counts as a link?

“Total number of links on the page” could mean:

- Every `<a>` element in the complete HTML document.
- Links inside the main search-results area.
- One primary article link per displayed search result.

These interpretations produce different results. Choose one and make it part of the acceptance criteria.

### What counts as a thumbnail?

Possible interpretations include:

- A search-result item containing an `<img>` element.
- A result with a valid thumbnail URL.
- Any image, including icons and placeholders.

A useful rule is to classify every displayed result exactly once as either having or not having a thumbnail. Therefore:

```text
with-thumbnail count + without-thumbnail count = displayed result count
```

The displayed result count will usually be smaller than the total number of matches reported by Wikipedia.

### What does “HTML code of the first page” mean?

Determine whether the requirement expects:

- The complete HTML response from Wikipedia's first search-results page.
- Only the search-results container.
- HTML snippets returned by an official API.
- A locally generated HTML representation of API results.

This affects the Wikipedia integration strategy because Wikimedia recommends supported APIs rather than crawling dynamic search pages.

### What does “search across all keywords” mean?

Possible interpretations include:

- Search stored keyword text.
- Filter by processing status, result totals, or dates.
- Full-text search through stored Wikipedia HTML.
- Return aggregate reporting statistics.

Unless clarified otherwise, begin with keyword-text search plus metadata filters. Do not begin with full-text searching of raw HTML.

### How should duplicates behave?

Decide what happens when:

- One CSV contains duplicate keywords.
- A user uploads the same keyword multiple times.
- Different users upload the same keyword.
- Capitalization or whitespace differs.

Preserving each user's upload while optionally reusing a recent search capture is more flexible than globally rejecting duplicate keywords.

## Suggested Architecture

Start with a modular monolith rather than multiple deployable services.

### ASP.NET Core API

Responsible for:

- Authentication endpoints
- Upload endpoint
- Reporting endpoints
- Request validation
- Authorization

### Application Logic

Responsible for:

- Use-case orchestration
- Processing-state transitions
- Interfaces for persistence, Wikipedia access, and background work
- Business rules that are independent of HTTP and SQL Server

### Infrastructure

Responsible for:

- Entity Framework Core
- SQL Server
- ASP.NET Core Identity
- Wikipedia HTTP communication
- HTML parsing
- Background processing

### Background Worker

Responsible for:

- Finding pending keyword jobs
- Processing them with controlled concurrency
- Saving successful captures
- Recording failures and retry information

### Tests

Use separate test projects or clearly separated test folders for:

- Unit tests
- Integration tests

Avoid creating abstractions that are not needed by an actual use case. A small number of well-defined layers is enough for this exercise.

## Suggested Data Concepts

Design these relationships on paper before creating migrations.

### User

Use ASP.NET Core Identity for user persistence and password hashing. Do not design a custom password-hashing mechanism.

### Upload Batch

Represents one uploaded CSV file and may contain:

- Owner/user identifier
- Original filename
- Upload timestamp
- Number of keywords
- Overall processing status

### Uploaded Keyword

Represents one row submitted by a user and may contain:

- Parent upload identifier
- Original keyword
- Normalized keyword
- Processing status
- Attempt count
- Created, started, and completed timestamps
- Safe failure information

Useful processing states include:

- Pending
- Processing
- Completed
- Failed

### Search Capture

Stores the extracted information:

- Total matching results
- Displayed results with thumbnails
- Displayed results without thumbnails
- Link count
- Raw HTML
- Source request or URL identity
- Capture timestamp

Raw HTML can be large. Plan for an appropriate SQL Server large-text column, and do not include it in ordinary list responses.

## Responsible Wikipedia Access

“Be creative” should not mean bypassing protections with rotating proxies, spoofed identities, or CAPTCHA evasion.

Use a defensible approach:

- Prefer the official MediaWiki search API for supported structured data.
- Clarify the raw-HTML requirement before choosing an additional retrieval mechanism.
- Send a meaningful `User-Agent` containing application and contact information.
- Process requests serially or with very low concurrency.
- Respect `429 Too Many Requests` and `Retry-After`.
- Use exponential backoff with jitter for transient failures.
- Set HTTP timeouts and support cancellation.
- Cache or reuse recent captures for duplicate normalized keywords where appropriate.
- Do not retry permanent validation failures indefinitely.
- Keep the Wikipedia hostname fixed instead of accepting arbitrary target URLs from users.

Wikimedia currently recommends no more than three concurrent API requests and encourages serial, well-identified requests.

The first Wikipedia task should be a small technical spike comparing the official search API with the exact required fields. Do not build the full processing system until the HTML requirement is understood.

## Planned API Behavior

Exact route names can be selected later, but define the contracts before implementing them.

### Sign Up

- Accept email and password.
- Reject duplicate email addresses.
- Let ASP.NET Core Identity enforce the password policy and hash the password.
- Return one bearer access token and its expiration time.

### Sign In

- Verify email and password through ASP.NET Core Identity.
- Return one bearer access token with a limited lifetime.
- Do not persist, log, or return password data or signing keys.

Refresh tokens are intentionally excluded from the MVP. They can be added later as a separate security exercise covering hashing, rotation, revocation, reuse detection, and cleanup. For a real production system, prefer a standards-based identity provider instead of building an authorization server.

### Upload Keyword File

- Require authentication.
- Accept one CSV file.
- Validate file type, size, encoding, row structure, and keyword count.
- Parse with a proper CSV parser rather than manually splitting lines on commas.
- Save the batch and keyword records transactionally.
- Return an accepted response containing a batch identifier and processing state.

Returning before Wikipedia processing finishes avoids long-running upload requests and HTTP timeouts.

### List Keywords

- Require authentication.
- Filter by the authenticated user at the database-query level.
- Paginate the response.
- Return summary fields rather than raw HTML.
- Optionally filter by upload or processing state.

### Get One Keyword Result

- Require authentication.
- Return not found when the record does not exist or belongs to another user.
- Include the current processing state.
- Include captured information when processing is complete.
- Consider returning raw HTML only when explicitly requested.

### Search Stored Data

Begin with filters such as:

- Keyword text
- Processing state
- Upload date range
- Minimum or maximum total results
- Pagination
- Sorting

Do not add SQL full-text search over raw HTML unless the requirement explicitly calls for it.

# Staged Implementation Plan

## Phase 1: Define Acceptance Rules

Before writing application classes:

1. Resolve the link-count definition.
2. Resolve the thumbnail definition.
3. Resolve the raw-HTML definition.
4. Define what searching stored data means.
5. Define a valid CSV:
   - Is there a header?
   - Is there one keyword per row?
   - Are blank rows ignored?
   - What is the maximum keyword length?
   - Are duplicate rows preserved?
6. Define the expected Wikipedia result-page size.
7. Decide whether failed jobs can be manually retried.
8. Choose the cloud and CI/CD platform.

**Deliverable:** concise acceptance examples written in plain English.

## CSV Contract

- The first row must have a `keyword` header.
- The CSV must contain one keyword column.
- Leading and trailing whitespace is removed.
- Blank rows are ignored.
- Duplicate keywords are removed case-insensitively.
- The spelling of the first duplicate is preserved.
- After removing blanks and duplicates, the file must contain 1–100 keywords.
- Each keyword must contain 1–100 characters.
- If any rule fails, the entire file is rejected.

## Phase 2: Scaffold the Solution

Create:

1. An ASP.NET Core Web API targeting .NET 10.
2. Application/domain separation if desired.
3. SQL Server infrastructure.
4. A unit-test project.
5. An integration-test project if time permits.

Verify that the untouched scaffold can restore, build, and test successfully.

**Learning focus:** solutions, project references, dependency injection, configuration, and the ASP.NET Core request pipeline.

## Phase 3: Model Persistence

1. Sketch entity relationships.
2. Add Entity Framework Core and SQL Server support.
3. Configure ASP.NET Core Identity.
4. Define user ownership relationships.
5. Create the initial database migration.
6. Run SQL Server locally through Docker.
7. Verify that migrations create the intended schema.

Avoid adding a generic repository layer only because some tutorials use one.

**Learning focus:** EF Core relationships, migrations, indexes, constraints, and transactions.

## Phase 4: Build Simple Access-Token Authentication

Implement registration and sign-in before business endpoints.

MVP steps:

1. Create sign-up, sign-in, and access-token response models.
2. Generate an HMAC-signed JWT containing `sub`, `email`, `jti`, issuer, audience, and expiration.
3. Create sign-up and sign-in controller actions using `UserManager<ApplicationUser>`.
4. Add one protected endpoint or integration test to prove bearer validation works.

Verify that Identity hashes passwords, tokens expire, invalid tokens receive `401 Unauthorized`, duplicate registration fails safely, and protected endpoints reject anonymous requests.

Keep the signing key outside source control. Do not implement refresh tokens, token rotation, revocation storage, or logout-token blacklists in the MVP.

**Later enhancement:** replace local token issuance with a standards-based identity provider, or add secure rotating refresh tokens as a separate exercise.

## Phase 5: Build the CSV Upload Vertical Slice

1. Define the upload contract.
2. Validate multipart file input.
3. Parse CSV input.
4. Enforce the 1–100 usable-keyword rule.
5. Save one upload batch and its keywords in a transaction.
6. Initially leave keyword records in a pending state.
7. Return the batch identifier and processing state.

Test boundary cases before connecting the application to Wikipedia.

**Learning focus:** streams, uploads, validation, SQL transactions, and HTTP response semantics.

## Phase 6: Run a Wikipedia Technical Spike

Use a small disposable experiment to inspect:

1. The official search API response.
2. How total result counts are represented.
3. How thumbnails can be identified.
4. What HTML content is available.
5. Whether the required link count can be derived without crawling a prohibited page.
6. Behavior for no results, Unicode terms, redirects, and unusual searches.

Save representative responses as test fixtures. Automated tests should not call live Wikipedia.

**Learning focus:** `HttpClient`, JSON, HTML DOM parsing, external API etiquette, and unstable third-party markup.

## Phase 7: Build the Wikipedia Client and Extractor

Create one `HttpClient`-based service that searches a fixed Wikimedia endpoint and converts the response into the stored result model.

MVP requirements:

- Correct query encoding.
- A descriptive `User-Agent`.
- One request at a time.
- A timeout and cancellation token.
- Respect `429 Too Many Requests` and `Retry-After` with one bounded retry.
- Parse saved response fixtures in tests; never call live Wikipedia from automated tests.
- Document exactly what the stored HTML field represents.

**Later enhancement:** separate transport from parsing, add exponential backoff with jitter, caching, richer diagnostics, and resilience policies.

## Phase 8: Add Simple Database-Backed Processing

Use one ASP.NET Core `BackgroundService`; do not introduce Hangfire, Service Bus, or another queue for the MVP.

1. Find the oldest pending keyword.
2. Mark it processing and save.
3. Call the Wikipedia client sequentially.
4. Save the result and mark it completed, or record a safe failure and mark it failed.
5. Wait briefly, then process the next item.

Pending work already lives in SQL Server, so application restarts do not lose uploads. Keep concurrency at `1` to reduce rate-limit risk.

**Later enhancement:** atomic job leasing, abandoned-job recovery, configurable retry schedules, distributed workers, and queue-based processing.

## Phase 9: Add Reporting Endpoints

Implement in this order:

1. List the user's uploads and keywords.
2. Retrieve one keyword's result.
3. Search and filter the user's stored records.
4. Add pagination and sorting.
5. Enforce ownership in every database query.
6. Avoid loading raw HTML unless requested.

Create two test users and confirm that neither can retrieve the other's records.

## Phase 10: Add Focused Automated Tests

Keep tests proportional to the assignment.

### Unit tests

- Retain the completed CSV parser coverage.
- Test access-token expiration and required JWT claims.
- Test Wikipedia extraction with a few saved fixtures.
- Test only business transformations that can run without I/O.

### Integration tests

- Sign-up, sign-in, and anonymous access rejection.
- One valid and one invalid upload.
- User ownership isolation for reporting.
- One successful background-processing path with a fake HTTP response.

Do not call live Wikipedia from automated tests. Advanced retry matrices, token-rotation tests, load tests, and exhaustive filtering combinations are future enhancements.

## Phase 11: Add Minimum Operational Safety

Required for the MVP:

- Never log tokens, passwords, signing keys, or raw connection strings.
- Return consistent validation errors.
- Enforce upload and request-size limits.
- Keep startup configuration validation.
- Support cancellation and graceful worker shutdown.

**Later enhancement:** health dashboards, metrics, tracing, alerting, administrative retries, rate-limit telemetry, and automatic cleanup jobs.

## Phase 12: Add Minimal CI/CD and Cloud Deployment

Use one GitHub Actions or Azure Pipelines workflow:

1. Restore.
2. Build in Release mode.
3. Run tests.
4. Publish and deploy only after tests pass.
5. Apply migrations as a controlled deployment step.
6. Run one API smoke check.

Deploy to one ASP.NET-compatible cloud service and Azure SQL. Store connection strings and the JWT signing key in protected cloud configuration. Infrastructure as code, deployment slots, multiple environments, and Key Vault integration can be added later.

Do not have every production application instance automatically apply migrations during startup.

# Future Enhancement Backlog

After the assignment is complete, optional learning exercises include:

- Rotating refresh tokens with hashed storage, revocation, reuse detection, and cleanup.
- OAuth 2.0/OpenID Connect through a managed identity provider.
- Distributed queues and horizontally scaled workers.
- Atomic job leasing and abandoned-job recovery.
- Exponential backoff, jitter, caching, and circuit breakers.
- Full-text search over stored HTML and richer report filters.
- OpenTelemetry, dashboards, alerting, and audit history.
- Infrastructure as code, deployment slots, and multi-environment promotion.

# Current Next Milestone

1. Remove the experimental refresh-token entity, migration, contracts, hashing code, and tests.
2. Keep the existing JWT bearer validation.
3. Implement one access-token generator with a 24-hour lifetime for this exercise.
4. Implement email/password sign-up and sign-in.
5. Prove that a protected endpoint accepts a valid token and rejects anonymous access.
6. Then implement the authenticated CSV upload endpoint.

Do not begin Wikipedia integration until the API interpretation and stored HTML definition are settled.
