using MediatR;
using PathfinderCampaignManager.Application.Common.Models;
using PathfinderCampaignManager.Domain.Entities;
using PathfinderCampaignManager.Domain.Interfaces;

namespace PathfinderCampaignManager.Application.Campaigns.Commands;

public record JoinCampaignCommand(
    Guid JoinToken,
    Guid UserId,
    string Alias
) : IRequest<Result<CampaignMemberDto>>;

public class JoinCampaignCommandHandler : IRequestHandler<JoinCampaignCommand, Result<CampaignMemberDto>>
{
    private readonly IUnitOfWork _unitOfWork;

    public JoinCampaignCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<CampaignMemberDto>> Handle(JoinCampaignCommand request, CancellationToken cancellationToken)
    {
        try
        {
            // Find campaign by join token
            var campaigns = await _unitOfWork.Repository<Campaign>()
                .FindAsync(c => c.JoinToken == request.JoinToken, cancellationToken);
            
            var campaign = campaigns.FirstOrDefault();
            if (campaign == null)
            {
                return Result.Failure<CampaignMemberDto>("Invalid or expired join link");
            }

            // Check if campaign is active
            if (!campaign.IsActive)
            {
                return Result.Failure<CampaignMemberDto>("This campaign is no longer active");
            }

            // Check if user can join
            if (!campaign.CanUserJoin(request.UserId))
            {
                return Result.Failure<CampaignMemberDto>("You are already a member of this campaign");
            }

            // Validate alias
            if (string.IsNullOrWhiteSpace(request.Alias))
            {
                return Result.Failure<CampaignMemberDto>("Alias is required");
            }

            if (request.Alias.Length > 50)
            {
                return Result.Failure<CampaignMemberDto>("Alias must be 50 characters or less");
            }

            // Check if alias is already taken in this campaign
            var existingMembers = campaign.Members;
            if (existingMembers.Any(m => m.Alias.Equals(request.Alias, StringComparison.OrdinalIgnoreCase)))
            {
                return Result.Failure<CampaignMemberDto>("This alias is already taken in this campaign");
            }

            // Add user to campaign
            campaign.AddMember(request.UserId, request.Alias, CampaignRole.Player);
            campaign.UpdateActivity();

            _unitOfWork.Repository<Campaign>().Update(campaign);
            await _unitOfWork.CommitAsync(cancellationToken);

            // Return member info
            var newMember = campaign.Members.First(m => m.UserId == request.UserId);
            var memberDto = new CampaignMemberDto
            {
                Id = newMember.Id,
                CampaignId = campaign.Id,
                CampaignName = campaign.Name,
                UserId = request.UserId,
                Alias = request.Alias,
                Role = CampaignRole.Player,
                JoinedAt = newMember.JoinedAt
            };

            return Result.Success(memberDto);
        }
        catch (Exception ex)
        {
            return Result.Failure<CampaignMemberDto>($"Failed to join campaign: {ex.Message}");
        }
    }
}

public class CampaignMemberDto
{
    public Guid Id { get; set; }
    public Guid CampaignId { get; set; }
    public string CampaignName { get; set; } = string.Empty;
    public Guid UserId { get; set; }
    public string Alias { get; set; } = string.Empty;
    public CampaignRole Role { get; set; }
    public DateTime JoinedAt { get; set; }
}