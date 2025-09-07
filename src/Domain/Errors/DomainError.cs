namespace PathfinderCampaignManager.Domain.Errors;

public record DomainError(string Code, string Message, string? Details = null)
{
    public static readonly DomainError None = new(string.Empty, string.Empty);

    public static DomainError From(Exception exception)
        => new("DOMAIN.EXCEPTION", exception.Message, exception.StackTrace);

    public static implicit operator string(DomainError error) => error.Code;
}

// Session Errors
public static class SessionErrors
{
    public static readonly DomainError NotFound = new("SESSION.NOT_FOUND", "Session was not found");
    public static readonly DomainError AlreadyExists = new("SESSION.ALREADY_EXISTS", "Session with this code already exists");
    public static readonly DomainError UserAlreadyMember = new("SESSION.USER_ALREADY_MEMBER", "User is already a member of this session");
    public static readonly DomainError CannotRemoveDM = new("SESSION.CANNOT_REMOVE_DM", "Cannot remove the DM from the session");
    public static readonly DomainError Inactive = new("SESSION.INACTIVE", "Session is not active");
    public static readonly DomainError InvalidCode = new("SESSION.INVALID_CODE", "Invalid session code format");
    public static readonly DomainError AccessDenied = new("SESSION.ACCESS_DENIED", "Access to session denied");
}

// Character Errors
public static class CharacterErrors
{
    public static readonly DomainError NotFound = new("CHARACTER.NOT_FOUND", "Character was not found");
    public static readonly DomainError AlreadyAssigned = new("CHARACTER.ALREADY_ASSIGNED", "Character is already assigned to a session");
    public static readonly DomainError NotOwner = new("CHARACTER.NOT_OWNER", "User is not the owner of this character");
    public static readonly DomainError InvalidLevel = new("CHARACTER.INVALID_LEVEL", "Level must be between 1 and 20");
    public static readonly DomainError AccessDenied = new("CHARACTER.ACCESS_DENIED", "Access to character denied");
    public static readonly DomainError ValidationFailed = new("CHARACTER.VALIDATION_FAILED", "Character data validation failed");
}

// Encounter Errors
public static class EncounterErrors
{
    public static readonly DomainError NotFound = new("ENCOUNTER.NOT_FOUND", "Encounter was not found");
    public static readonly DomainError AlreadyActive = new("ENCOUNTER.ALREADY_ACTIVE", "Encounter is already active");
    public static readonly DomainError NotActive = new("ENCOUNTER.NOT_ACTIVE", "Encounter is not active");
    public static readonly DomainError Completed = new("ENCOUNTER.COMPLETED", "Encounter is already completed");
    public static readonly DomainError NoCombatants = new("ENCOUNTER.NO_COMBATANTS", "Encounter has no combatants");
    public static readonly DomainError CombatantNotFound = new("ENCOUNTER.COMBATANT_NOT_FOUND", "Combatant was not found");
    public static readonly DomainError AccessDenied = new("ENCOUNTER.ACCESS_DENIED", "Access to encounter denied");
}

// User Errors
public static class UserErrors
{
    public static readonly DomainError NotFound = new("USER.NOT_FOUND", "User was not found");
    public static readonly DomainError EmailAlreadyExists = new("USER.EMAIL_ALREADY_EXISTS", "Email is already in use");
    public static readonly DomainError UsernameAlreadyExists = new("USER.USERNAME_ALREADY_EXISTS", "Username is already taken");
    public static readonly DomainError Inactive = new("USER.INACTIVE", "User account is inactive");
    public static readonly DomainError InsufficientPermissions = new("USER.INSUFFICIENT_PERMISSIONS", "User has insufficient permissions");
    public static readonly DomainError InvalidCredentials = new("USER.INVALID_CREDENTIALS", "Invalid credentials provided");
}

// Rules Errors
public static class RulesErrors
{
    public static readonly DomainError VersionNotFound = new("RULES.VERSION_NOT_FOUND", "Rules version was not found");
    public static readonly DomainError VersionAlreadyExists = new("RULES.VERSION_ALREADY_EXISTS", "Rules version already exists");
    public static readonly DomainError CannotModifyPublished = new("RULES.CANNOT_MODIFY_PUBLISHED", "Cannot modify published rules version");
    public static readonly DomainError ClassNotFound = new("RULES.CLASS_NOT_FOUND", "Class definition was not found");
    public static readonly DomainError ClassAlreadyExists = new("RULES.CLASS_ALREADY_EXISTS", "Class already exists in this version");
    public static readonly DomainError NoPublishedVersion = new("RULES.NO_PUBLISHED_VERSION", "No published rules version available");
    public static readonly DomainError ValidationFailed = new("RULES.VALIDATION_FAILED", "Rules validation failed");
}

// General Errors
public static class GeneralErrors
{
    public static readonly DomainError ValidationFailed = new("VALIDATION.FAILED", "Validation failed");
    public static readonly DomainError NotFound = new("GENERAL.NOT_FOUND", "Resource was not found");
    public static readonly DomainError AccessDenied = new("GENERAL.ACCESS_DENIED", "Access denied");
    public static readonly DomainError ConcurrencyConflict = new("GENERAL.CONCURRENCY_CONFLICT", "The resource was modified by another user");
    public static readonly DomainError InvalidOperation = new("GENERAL.INVALID_OPERATION", "Invalid operation");
    public static readonly DomainError InternalError = new("GENERAL.INTERNAL_ERROR", "An internal error occurred");
}

// Authorization Errors
public static class AuthorizationErrors
{
    public static readonly DomainError Unauthorized = new("AUTHORIZATION.UNAUTHORIZED", "User is not authenticated");
    public static readonly DomainError Forbidden = new("AUTHORIZATION.FORBIDDEN", "User does not have permission to perform this action");
    public static readonly DomainError InvalidToken = new("AUTHORIZATION.INVALID_TOKEN", "Authentication token is invalid");
    public static readonly DomainError TokenExpired = new("AUTHORIZATION.TOKEN_EXPIRED", "Authentication token has expired");
    public static readonly DomainError InsufficientRole = new("AUTHORIZATION.INSUFFICIENT_ROLE", "User role is insufficient for this action");
}