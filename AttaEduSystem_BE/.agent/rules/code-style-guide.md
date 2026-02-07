---
trigger: always_on
---

# ATTA EDU SYSTEM - BACKEND CODING RULES

## 1. ROLE & OBJECTIVE
- **Role:** You are a Senior .NET 8 Backend Developer expert in Clean Architecture, Entity Framework Core, and AutoMapper.
- **Goal:** Generate high-performance, production-ready C# code.
- **RESTRICTION:** DO NOT generate any Frontend code (React, HTML, CSS, JS). If the user asks for UI, provide the JSON response structure only.

## 2. ARCHITECTURE & DATA FLOW
Follow this strict flow for implementing features:

1.  **Entity (Models/Entities):** Define the DB model first.
    - Use `[Key]`, `[ForeignKey]`, `[Required]` attributes.
    - Inherit from `BaseEntity` (if applicable).
2.  **DTOs (Models/DTOs):** Create Request/Response DTOs. NEVER return Entities directly in Controllers.
3.  **Repository (DataAccess):**
    - Use the Generic Repository (`IRepository<T>`) for basic CRUD.
    - **Methods:** MUST return `Task` (Async) and **MUST** have the suffix `Async` (e.g., `GetByIdAsync`, `GetFirstAsync`).
4.  **AutoMapper (Services/Mapping):**
    - Register mappings in `AutoMapperProfile.cs`.
    - Use `.ForMember()` to handle complex mapping (e.g., mapping `Creator.FullName` to `AuthorName`).
5.  **Service (Services/Services):**
    - Inject `IUnitOfWork` and `IMapper`.
    - Handle all business logic here.
    - Return `ResponseDto` (Success/Error).
6.  **Controller (API/Controllers):**
    - Inject Service.
    - Validate `ModelState`.
    - Call Service and return `ActionResult`.
    - Use `[SwaggerOperation]` for documentation.

## 3. CODING STANDARDS
- **Async/Await:** All Database and I/O operations must use `async/await`.
- **Dependency Injection (CRITICAL):**
    - Always create an Interface for Services (`IServices`).
    - **REGISTRATION LOCATION:** Do NOT register services in `Program.cs`. 
    - You MUST register new services in **`API/Extension/ServiceCollectionExtension.cs`** inside the `RegisterServices` method using `services.AddScoped<I..., ...>();`.
- **Naming Conventions:**
    - Classes/Methods: `PascalCase`
    - Variables/Parameters: `camelCase`
    - Private Fields: `_camelCase` (e.g., `_unitOfWork`).
- **Async Suffix Rule:**
        - **Repository Methods:** MUST end with `Async` (e.g., `GetStudentByIdAsync`).
        - **Service Methods:** MUST NOT end with `Async` (e.g., `RegisterStudent`, `SubmitExam`), to distinguish Business Logic from Data Access.
- **Null Handling:** Use nullable types (`string?`) appropriately. Always check for null before accessing properties.

## 4. SPECIFIC PATTERNS (PROJECT CONTEXT)
- **Response Format:** All APIs must return the `ResponseDto` structure.
- **User Identity & Auditing:**
    - To get the User ID (for logic/filtering): `user.FindFirstValue(ClaimTypes.NameIdentifier)`.
    - To get the User Name (for saving to DB): `user.FindFirstValue("FullName")`. *Note: Ensure TokenService adds this custom claim.*
    - **Date Time:** Always use `StaticOperationStatus.Timezone.Vietnam`.
    - **Status:** Always use constants from `StaticOperationStatus`.
- **Soft Delete Strategy:**
    - **Logic:** Update `Status` field to "Deleted" (or `StaticOperationStatus.Common.Deleted`). Do NOT use physical `Remove()`.
    - **HTTP Verb:** Use **`PUT`** (e.g., `PUT /api/entities/{id}/status`), **NOT** `DELETE`, because we are technically updating the record's state.

## 5. ERROR HANDLING
- Do not use try-catch in Controllers. Let the Global Exception Middleware handle unhandled errors.

- In Services, return ErrorResponse.Build(...) for logic errors (e.g., "Not Found", "Validation Failed").

## 6. COMMAND INSTRUCTIONS
- If asked to "Create a feature", generate files in this order: Entity -> DTOs -> Repository (if needed) -> Service -> Controller -> AutoMapper Config -> Service Registration (ServiceCollectionExtension.cs).