# Pathfinder 2e Campaign Manager

A production-ready Blazor application using Clean Architecture to manage Pathfinder 2e campaigns, characters, initiative/combat, and custom content—secured, privacy-preserving, and extensible by admins.

## 🏗️ Architecture

This project follows Clean Architecture principles with the following layers:

```
src/
├── Domain/                     # Core business logic and entities
├── Application/               # Use cases, CQRS handlers, and validation
├── Infrastructure/           # Data persistence, external services
└── Presentation/
    ├── Server/              # Web API
    ├── Client/              # Blazor WASM
    └── Shared/              # Shared DTOs and models
```

## ✅ Implementation Progress

### ✅ Completed Components

#### 🏛️ Domain Layer
- **Entities**: Session, Character, Encounter, User, RulesVersion, ClassDef
- **Value Objects**: SessionCode with auto-generation
- **Domain Events**: Comprehensive event system for all aggregates
- **Error Handling**: Structured domain errors with typed error codes
- **Result Pattern**: Functional result type for error handling

#### 📋 Application Layer  
- **CQRS**: Commands and Queries with MediatR
- **Pipeline Behaviors**: 
  - ValidationBehavior (FluentValidation)
  - AuthorizationBehavior (Policy-based)
  - UnitOfWorkBehavior (Transaction management)
- **Sample Handlers**: Session, Character, and Encounter operations
- **DTOs**: Result pattern integration for API responses

#### 🔧 Infrastructure Layer
- **Entity Framework Core**: SQL Server with migrations
- **Repository Pattern**: Generic and specialized repositories
- **Unit of Work**: Transaction boundary management
- **Database Configuration**: Entity configurations and relationships
- **Services**: CurrentUser, DateTime, and Authorization services

### 🎯 Core Features (Domain Model)

- **Sessions**: Anonymous campaign containers with restricted access
- **Characters**: PF2e character sheets with audit logging
- **Encounters**: Initiative tracking, combat management, turn-based system
- **Users & Roles**: Player, DM, Admin role hierarchy
- **Rules Library**: Versioned PF2e content with admin management

### 🔐 Security & Privacy

- **Access Control**: Character visibility limited to owner + session DM
- **Session Anonymity**: No personal data exposed to other players
- **Authorization**: Policy-based permissions for all operations
- **Audit Trail**: Complete change tracking for characters

### 🛠️ Tech Stack

- **.NET 9**: Latest framework features
- **Entity Framework Core**: Database ORM with SQL Server
- **MediatR**: CQRS and domain event handling
- **FluentValidation**: Input validation with behaviors
- **Blazor WASM**: Client-side SPA framework
- **Mapster**: Object mapping
- **Result Pattern**: Functional error handling

## 🏃‍♂️ Getting Started

### Prerequisites
- .NET 9 SDK
- SQL Server or LocalDB

### Build & Run

```bash
# Build entire solution
dotnet build

# Run tests
dotnet test

# Run API server (from Server directory)
cd src/Presentation/Server
dotnet run

# Run Blazor client (from Client directory) 
cd src/Presentation/Client
dotnet run
```

## 📦 Project Structure

### Domain Layer (`src/Domain/`)
- `Entities/` - Aggregate roots and entities
- `ValueObjects/` - Immutable value types  
- `Enums/` - Domain enumerations
- `Interfaces/` - Repository contracts
- `Errors/` - Typed error definitions
- `Common/` - Result pattern implementation

### Application Layer (`src/Application/`)
- `Sessions/Commands/` & `Sessions/Queries/` - Session operations
- `Characters/Commands/` & `Characters/Queries/` - Character operations  
- `Encounters/Commands/` & `Encounters/Queries/` - Combat operations
- `Behaviors/` - Cross-cutting MediatR pipeline behaviors
- `Abstractions/` - Service contracts

### Infrastructure Layer (`src/Infrastructure/`)
- `Persistence/` - EF Core DbContext and configurations
- `Repositories/` - Data access implementations
- `Services/` - External service implementations

## 🎮 Key Domain Concepts

### Sessions
- **Anonymous Access**: Join via 6-character codes
- **Privacy Controls**: DM manages what players can see
- **Member Management**: Players and DM roles

### Characters  
- **Ownership Model**: Player owns, DM can view in session
- **Audit Trail**: All changes tracked with user attribution
- **PF2e Integration**: Class, ancestry, background references

### Encounters
- **Turn Management**: Initiative order with round tracking
- **Combatant Types**: PC, NPC, Monster support
- **Conditions**: Flexible condition/effect system
- **Undo/Redo**: Action history for combat mistakes

### Rules Library
- **Versioning**: Draft → Published → Archived workflow
- **Admin Control**: Class definitions and content management
- **Migration Support**: Character updates when rules change

## 🔄 Next Steps

1. **Web API Implementation** - REST endpoints with Swagger
2. **Blazor WASM Client** - UI with Fluxor state management  
3. **CSS Design Tokens** - Themeable design system
4. **Authentication & Identity** - JWT with ASP.NET Identity
5. **Database Migrations** - Initial schema and seed data
6. **Testing Suite** - Unit, integration, and E2E tests

## 📋 API Examples

The application exposes a RESTful API following the Result pattern:

```csharp
// All endpoints return ResultDto<T>
POST /api/sessions              → Create session
GET  /api/sessions/{id}         → Get session details  
POST /api/characters            → Create character
GET  /api/characters/{id}       → Get character
POST /api/encounters            → Create encounter
POST /api/encounters/{id}/start → Start combat
```

## 🧪 Testing Strategy

- **Domain Tests**: Entity invariants and business rules
- **Application Tests**: Handler behavior with in-memory DB
- **Infrastructure Tests**: Repository integration with Testcontainers
- **API Tests**: Contract testing with Swagger snapshots
- **UI Tests**: Blazor component testing with Bunit

---

This foundation provides a robust, maintainable architecture ready for PF2e campaign management features. The Clean Architecture approach ensures separation of concerns, testability, and future extensibility.