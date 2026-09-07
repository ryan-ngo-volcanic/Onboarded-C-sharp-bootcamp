# Wikipedia Search API Application — Learning Plan

## Overview

Build a .NET 10 ASP.NET Core API that:

- Authenticates users with token-based authentication.
- Accepts CSV uploads containing 1–100 Wikipedia search keywords.
- Processes uploaded keywords after the upload is accepted.
- Stores extracted Wikipedia search information in SQL Server.
- Lets authenticated users retrieve and search only their own data.
- Includes automated tests, CI/CD, and cloud deployment.

This plan intentionally describes the work and design decisions without providing implementation code.

## Current Workspace State

The workspace is currently empty except for VS Code settings.

Available local tools:

- .NET SDK `10.0.302`
- Docker `29.0.2`

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

- Accept registration information.
- Reject duplicate identities.
- Enforce a password policy.
- Return a suitable success response without exposing internal account data.

### Sign In

- Verify credentials.
- Return a bearer access token with a limited lifetime.
- Do not log credentials or tokens.

For a learning exercise, local token issuance can demonstrate JWT authentication. For a production system, use a standards-based identity provider instead of inventing an authorization server.

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

## Phase 4: Build the Authentication Vertical Slice

Implement registration and sign-in before business endpoints.

Verify that:

- Identity hashes passwords.
- Tokens expire.
- Invalid tokens receive unauthorized responses.
- Protected endpoints reject anonymous requests.
- Authentication errors do not reveal sensitive internal details.

Keep token-signing secrets outside source control. Use local secret storage during development and managed cloud secrets after deployment.

**Learning focus:** authentication versus authorization, claims, bearer middleware, and secret management.

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

## Phase 7: Build the Extraction Component

Create a component that converts a Wikipedia response into the internal search-capture model.

Plan for:

- Correct URL and query encoding
- A fixed external base address
- A descriptive `User-Agent`
- Timeouts and cancellation
- Retry handling for `429` and transient server errors
- DOM parsing instead of regular expressions
- Extraction invariant validation
- Explicit failure behavior when expected markup is absent

Keep HTTP transport separate from parsing so parser tests can use local fixtures.

## Phase 8: Add Durable Background Processing

1. Find pending keyword records.
2. Atomically mark a record as processing.
3. Call the Wikipedia component.
4. Save the capture and mark the record complete.
5. Record safe failure and retry information when processing fails.
6. Recover abandoned processing records after application restarts.
7. Enforce global throttling instead of starting 100 requests simultaneously.

Avoid relying solely on an in-memory queue if uploaded work must survive application restarts.

**Learning focus:** hosted services, dependency-injection scopes, concurrency, idempotency, retries, and durable work.

## Phase 9: Add Reporting Endpoints

Implement in this order:

1. List the user's uploads and keywords.
2. Retrieve one keyword's result.
3. Search and filter the user's stored records.
4. Add pagination and sorting.
5. Enforce ownership in every database query.
6. Avoid loading raw HTML unless requested.

Create two test users and confirm that neither can retrieve the other's records.

## Phase 10: Add Automated Tests

### Unit Tests

Prioritize tests for:

- CSV validation
- The 1-keyword boundary
- The 100-keyword boundary
- Rejection of 0 and 101 keywords
- Keyword normalization and duplicate policy
- Wikipedia fixture parsing
- Thumbnail classification
- Link counting
- Retry-decision rules
- Search and filter rules
- Processing-state transitions

### Integration Tests

Cover:

- Registration and sign-in
- Protected endpoint behavior
- Valid and invalid file uploads
- SQL persistence
- User ownership isolation
- Pagination
- Background processing with a fake Wikipedia HTTP server

Do not use live Wikipedia calls in automated tests because they would be slow, flaky, and inconsiderate.

## Phase 11: Add Operational Hardening

Add:

- Structured logging without tokens, passwords, or sensitive raw data
- Health endpoints
- Problem Details error responses
- Request and file-size limits
- Database indexes for user, normalized keyword, status, and timestamps
- Metrics for pending, completed, failed, and retried jobs
- Graceful shutdown and cancellation
- Startup configuration validation

## Phase 12: Add CI/CD and Cloud Deployment

Azure is a direct fit with SQL Server:

- API hosting: Azure App Service or Azure Container Apps
- Database: Azure SQL Database
- Secrets: Azure Key Vault or protected deployment settings
- Automation: GitHub Actions or Azure Pipelines

The automated pipeline should:

1. Restore dependencies.
2. Build in Release mode.
3. Run unit tests.
4. Run integration tests.
5. Publish test results and coverage.
6. Create the deployable artifact or container.
7. Deploy only after tests pass.
8. Apply migrations as a controlled deployment step.
9. Run a smoke or health check after deployment.

Do not have every production application instance automatically apply migrations during startup.

# Recommended Starting Milestone

The first coding session should stop after this milestone:

1. Establish the intended Git repository boundary.
2. Create the .NET solution and API/test projects.
3. Confirm `dotnet build` succeeds.
4. Confirm `dotnet test` succeeds.
5. Write down the exact CSV contract.
6. Write tests for the CSV validation boundaries.
7. Implement only enough CSV parsing to make those tests pass.

After that, implement authentication as the first complete API vertical slice. Leave Wikipedia integration until the technical spike is complete and the raw-HTML interpretation has been settled.
