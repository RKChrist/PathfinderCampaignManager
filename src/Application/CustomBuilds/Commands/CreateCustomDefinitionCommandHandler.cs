using MediatR;
using PathfinderCampaignManager.Domain.Entities;
using PathfinderCampaignManager.Domain.Interfaces;
using PathfinderCampaignManager.Domain.Enums;

namespace PathfinderCampaignManager.Application.CustomBuilds.Commands;

public class CreateCustomDefinitionCommandHandler : IRequestHandler<CreateCustomDefinitionCommand, CreateCustomDefinitionResponse>
{
    private readonly IRepository<CustomDefinition> _repository;
    private readonly IUnitOfWork _unitOfWork;

    public CreateCustomDefinitionCommandHandler(
        IRepository<CustomDefinition> repository,
        IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task<CreateCustomDefinitionResponse> Handle(CreateCustomDefinitionCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var customDefinition = CustomDefinition.Create(
                request.OwnerUserId,
                request.Type,
                request.Name,
                request.Description,
                request.JsonData,
                request.Category);

            await _repository.AddAsync(customDefinition, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return new CreateCustomDefinitionResponse(customDefinition.Id, true);
        }
        catch (Exception ex)
        {
            return new CreateCustomDefinitionResponse(Guid.Empty, false, ex.Message);
        }
    }
}