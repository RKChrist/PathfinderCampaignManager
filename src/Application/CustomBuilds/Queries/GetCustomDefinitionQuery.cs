using MediatR;
using PathfinderCampaignManager.Application.CustomBuilds.Models;

namespace PathfinderCampaignManager.Application.CustomBuilds.Queries;

public sealed record GetCustomDefinitionQuery(Guid Id, Guid RequestingUserId) : IRequest<CustomDefinitionDto?>;

public sealed record GetCustomDefinitionsQuery(
    Guid UserId,
    string? SearchTerm = null,
    string? Type = null,
    bool IncludePublic = false) : IRequest<List<CustomDefinitionDto>>;