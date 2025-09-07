# Pathfinder 2e Campaign Manager - System Architecture

## Overview
A comprehensive web-based campaign management system for Pathfinder 2e RPG sessions, built with Clean Architecture principles using .NET 9, Blazor Server, and SignalR for real-time collaboration.

## Architecture

### Clean Architecture Layers

```
┌─────────────────────────────────────────────┐
│                Presentation                 │
│  ┌─────────────┐ ┌─────────────────────────┐ │
│  │   Server    │ │         Client          │ │
│  │ Controllers │ │  Blazor Components      │ │
│  │ SignalR     │ │  Pages & Services       │ │
│  │   Hubs      │ │                         │ │
│  └─────────────┘ └─────────────────────────┘ │
└─────────────────────────────────────────────┘
┌─────────────────────────────────────────────┐
│               Application                   │
│  Services, Commands, Queries, DTOs          │
└─────────────────────────────────────────────┘
┌─────────────────────────────────────────────┐
│                 Domain                      │
│  Entities, Value Objects, Interfaces       │
│  Business Logic, Domain Services            │
└─────────────────────────────────────────────┘
┌─────────────────────────────────────────────┐
│              Infrastructure                 │
│  Data Access, External Services             │
│  Persistence, Repositories                  │
└─────────────────────────────────────────────┘
```

### Technology Stack

- **.NET 9**: Modern .NET platform with latest performance improvements
- **Blazor Server**: Server-side rendering with real-time UI updates
- **SignalR**: Real-time communication for collaborative features
- **Entity Framework Core**: ORM with In-Memory database for development
- **xUnit + FluentAssertions**: Comprehensive testing framework
- **Clean Architecture**: Separation of concerns with dependency inversion

## Core Systems

### 1. Character Management System
**Location**: `src/Domain/Entities/Pathfinder/PfCharacter.cs`

- **Character Creation Wizard**: Multi-step character building process
- **Ability Score Management**: Point-buy system with racial bonuses
- **Skill System**: Comprehensive skill tracking with proficiency ranks
- **Equipment Management**: Bulk tracking and encumbrance calculations
- **Level Progression**: Automatic feat slots and ability increases

**Key Features**:
- Pathfinder 2e rule compliance
- Real-time validation
- Multi-step wizard interface
- Character sheet auto-calculations

### 2. Archetype System
**Location**: `src/Infrastructure/Data/ArchetypeRepository.cs`

- **Multiclass Dedications**: Cross-class feat access
- **General Archetypes**: Specialized character builds
- **Class Archetypes**: Class-specific modifications
- **Prerequisite Validation**: Automatic requirement checking
- **Spellcasting Progression**: Multiclass spellcaster calculations

**Supported Archetypes**:
- Barbarian, Fighter, Wizard multiclass
- Extensible system for additional archetypes

### 3. Campaign Management System
**Location**: `src/Presentation/Client/Pages/Campaigns/`

- **Campaign Creation**: GM tools for campaign setup
- **Session Management**: Session planning and tracking
- **Player Invitation**: Secure join codes
- **Variant Rules**: Free Archetype, Gradual Boosts, etc.
- **Real-time Collaboration**: Multi-user campaign editing

### 4. Real-time Combat System
**Location**: `src/Presentation/Server/Hubs/CombatHub.cs`

- **Initiative Tracking**: Automatic turn order management
- **Health Management**: Real-time HP tracking
- **Combat Actions**: Action economy tracking
- **Multi-user Sync**: All players see updates instantly
- **Combat Status**: Conditions and effects tracking

### 5. Rules Navigation System
**Location**: `src/Presentation/Client/Pages/Rules/`

- **Category-based Organization**: Actions, Feats, Spells, etc.
- **Advanced Search**: Full-text search with filtering
- **Rule Cards**: Detailed rule displays
- **Cross-references**: Linked rule dependencies
- **Responsive Design**: Mobile-friendly rule lookup

### 6. Hover Card System
**Location**: `src/Presentation/Client/Components/HoverCards/`

- **Spell Cards**: Detailed spell information on hover
- **Feat Cards**: Prerequisite and effect display
- **Smart Positioning**: Automatic card placement
- **Context Awareness**: Character-specific information
- **Performance Optimized**: Lazy loading and caching

### 7. Validation System
**Location**: `src/Infrastructure/Validation/ValidationService.cs`

- **Character Validation**: Rule compliance checking
- **Data Integrity**: Consistency verification
- **Real-time Feedback**: Immediate validation results
- **Severity Levels**: Errors, Warnings, Suggestions
- **Actionable Feedback**: Fix suggestions for issues

## Data Model

### Character Entities
```csharp
PfCharacter
├── AbilityScores (Str, Dex, Con, Int, Wis, Cha)
├── Skills (List<PfCharacterSkill>)
├── Equipment (List<string>)
├── Feats (List<PfFeat>)
└── ClassProgression (PfClassProgression)
```

### Archetype System
```csharp
PfArchetype
├── Prerequisites (List<PfPrerequisite>)
├── Feats (List<PfArchetypeFeat>)
├── SpellcastingProgression
└── Type (Multiclass, General, Class)
```

### Validation Framework
```csharp
ValidationReport
├── Issues (List<ValidationIssue>)
├── Warnings (List<ValidationWarning>)
├── Suggestions (List<ValidationSuggestion>)
└── IsValid (bool)
```

## API Design

### REST Endpoints
- `GET /api/characters` - Character listing
- `POST /api/characters` - Character creation
- `PUT /api/characters/{id}` - Character updates
- `GET /api/campaigns` - Campaign management
- `POST /api/validation/*` - Validation services

### SignalR Hubs
- `CombatHub` - Real-time combat tracking
- `CampaignHub` - Campaign-wide notifications

## Testing Strategy

### Test Coverage
- **Unit Tests**: 95%+ coverage on domain logic
- **Integration Tests**: API endpoint validation
- **Component Tests**: UI interaction testing
- **End-to-End Tests**: Full user workflow validation

### Test Organization
```
tests/
├── Unit/
│   ├── Domain/           # Business logic tests
│   └── Application/      # Service tests
├── Integration/
│   └── Infrastructure/   # Repository & API tests
└── E2E/                  # Full system tests
```

## Security Considerations

### Authentication & Authorization
- JWT token-based authentication
- Role-based access control (GM/Player)
- Session management
- CORS configuration

### Data Protection
- Input validation at all layers
- SQL injection prevention
- XSS protection
- CSRF tokens

## Performance Optimizations

### Client-Side
- Lazy loading of components
- Efficient re-rendering with Blazor
- Caching of frequently accessed data
- Optimized SignalR connections

### Server-Side
- Async/await throughout
- Connection pooling
- Memory-efficient data structures
- Background service processing

## Deployment Architecture

### Development
- In-Memory database
- Local SignalR hub
- Development CORS policies

### Production Considerations
- SQL Server database
- Redis SignalR backplane
- Load balancing support
- Health checks

## Extension Points

### Adding New Character Classes
1. Implement `IPfClass` interface
2. Add class progression data
3. Register in dependency injection
4. Create class-specific components

### Adding New Archetypes
1. Create archetype data in `ArchetypeRepository`
2. Define prerequisites and feats
3. Add spellcasting progression if needed
4. Update validation rules

### Custom Validation Rules
1. Implement custom validation methods
2. Add to `ValidationService`
3. Create corresponding UI feedback
4. Add unit tests for new rules

## Development Guidelines

### Code Standards
- Follow Clean Architecture principles
- Use dependency injection
- Implement comprehensive logging
- Maintain high test coverage
- Follow C# coding conventions in CLAUDE.md

### Component Architecture
- Separate logic in `.razor.cs` files
- Use proper error boundaries
- Implement loading states
- Provide accessibility features

## Monitoring & Diagnostics

### Logging
- Structured logging with Serilog
- Performance metrics
- Error tracking
- User activity logging

### Health Checks
- Database connectivity
- External service availability
- Memory usage monitoring
- Response time tracking

This architecture provides a solid foundation for a comprehensive Pathfinder 2e campaign management system with room for future expansion and enhancement.