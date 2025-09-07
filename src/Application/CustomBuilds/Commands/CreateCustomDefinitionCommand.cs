using MediatR;
using PathfinderCampaignManager.Domain.Enums;

namespace PathfinderCampaignManager.Application.CustomBuilds.Commands;

public sealed record CreateCustomDefinitionCommand(
    Guid OwnerUserId,
    CustomDefinitionType Type,
    string Name,
    string Description,
    string JsonData,
    string Category = "") : IRequest<CreateCustomDefinitionResponse>;

public sealed record CreateCustomDefinitionResponse(
    Guid Id,
    bool IsSuccess,
    string? ErrorMessage = null);