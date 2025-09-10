using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using PathfinderCampaignManager.Presentation.Shared.Models;
using PathfinderCampaignManager.Presentation.Server.Controllers;
using System.Collections.Concurrent;

namespace PathfinderCampaignManager.Presentation.Server.Hubs;

[AllowAnonymous]
public class CombatHub : Hub
{
    private static readonly ConcurrentDictionary<string, CombatSession> CombatSessions = new();

    public CombatHub()
    {
    }

    public async Task Ping()
    {
        try
        {
            Console.WriteLine("Ping method called");
            await Clients.Caller.SendAsync("Pong", "Connection is working");
            Console.WriteLine("Pong sent successfully");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Ping failed: {ex}");
            throw;
        }
    }

    public async Task JoinCombat(string combatId, string? campaignId = null)
    {
        try
        {
            Console.WriteLine($"JoinCombat called with combatId: {combatId}, campaignId: {campaignId}");
            Console.WriteLine($"Connection ID: {Context.ConnectionId}");
            
            await Groups.AddToGroupAsync(Context.ConnectionId, $"Combat-{combatId}");
            Console.WriteLine($"Added to group: Combat-{combatId}");
            
            // Send current combat state to the joining user
            // Check CombatController's shared storage first - prioritize campaignId if provided
            Guid lookupKey;
            if (!string.IsNullOrEmpty(campaignId) && Guid.TryParse(campaignId, out var campaignGuid))
            {
                lookupKey = campaignGuid;
                Console.WriteLine($"Using campaignId for lookup: {lookupKey}");
            }
            else if (Guid.TryParse(combatId, out var combatGuid))
            {
                lookupKey = combatGuid;
                Console.WriteLine($"Using combatId for lookup: {lookupKey}");
            }
            else
            {
                Console.WriteLine($"Invalid combat/campaign ID provided");
                return;
            }
            
            if (CombatController._combatSessions.TryGetValue(lookupKey, out var controllerSession))
            {
                Console.WriteLine($"Found combat session in CombatController storage: {controllerSession.Name}");
                try
                {
                    // Convert CombatController's CombatSession to Shared.Models.CombatSession
                    var sharedSession = new PathfinderCampaignManager.Presentation.Shared.Models.CombatSession
                    {
                        Id = controllerSession.Id,
                        Name = controllerSession.Name,
                        IsActive = controllerSession.IsActive,
                        Round = controllerSession.Round,
                        CurrentTurn = controllerSession.CurrentTurn,
                        Participants = controllerSession.Participants.Select(MapToSharedParticipant).ToList()
                    };
                    
                    await Clients.Caller.SendAsync("CombatStateUpdated", sharedSession);
                    Console.WriteLine("CombatStateUpdated sent successfully from CombatController storage");
                }
                catch (Exception sessionEx)
                {
                    Console.WriteLine($"Failed to send CombatStateUpdated: {sessionEx}");
                    throw;
                }
            }
            // Fallback to local CombatHub storage
            else if (CombatSessions.TryGetValue(combatId, out var session))
            {
                Console.WriteLine($"Sending existing combat state for session: {session.Name}");
                try
                {
                    await Clients.Caller.SendAsync("CombatStateUpdated", session);
                    Console.WriteLine("CombatStateUpdated sent successfully from CombatHub storage");
                }
                catch (Exception sessionEx)
                {
                    Console.WriteLine($"Failed to send CombatStateUpdated: {sessionEx}");
                    throw;
                }
            }
            else
            {
                Console.WriteLine($"No existing combat session found for ID: {combatId}");
            }
            
            Console.WriteLine("JoinCombat completed successfully");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"JoinCombat failed: {ex}");
            await Clients.Caller.SendAsync("Error", $"Failed to join combat: {ex.Message}");
            throw;
        }
    }

    public async Task LeaveCombat(string combatId)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"Combat-{combatId}");
    }

    public async Task UpdateInitiative(string combatId, string participantId, int initiative)
    {
        if (CombatSessions.TryGetValue(combatId, out var session))
        {
            var participant = session.Participants.FirstOrDefault(p => p.Id.ToString() == participantId);
            if (participant != null)
            {
                participant.Initiative = initiative;
                session.Participants = session.Participants.OrderByDescending(p => p.Initiative).ToList();
                
                await Clients.Group($"Combat-{combatId}").SendAsync("InitiativeUpdated", participantId, initiative);
                await Clients.Group($"Combat-{combatId}").SendAsync("CombatStateUpdated", session);
            }
        }
    }

    public async Task UpdateHitPoints(string combatId, string participantId, int currentHp, int maxHp)
    {
        if (CombatSessions.TryGetValue(combatId, out var session))
        {
            var participant = session.Participants.FirstOrDefault(p => p.Id.ToString() == participantId);
            if (participant != null)
            {
                participant.CurrentHitPoints = currentHp;
                participant.HitPoints = maxHp;
                
                await Clients.Group($"Combat-{combatId}").SendAsync("HitPointsUpdated", participantId, currentHp, maxHp);
                await Clients.Group($"Combat-{combatId}").SendAsync("CombatStateUpdated", session);
            }
        }
    }

    public async Task UpdateArmorClass(string combatId, string participantId, int armorClass)
    {
        if (CombatSessions.TryGetValue(combatId, out var session))
        {
            var participant = session.Participants.FirstOrDefault(p => p.Id.ToString() == participantId);
            if (participant != null)
            {
                participant.ArmorClass = armorClass;
                
                await Clients.Group($"Combat-{combatId}").SendAsync("ArmorClassUpdated", participantId, armorClass);
                await Clients.Group($"Combat-{combatId}").SendAsync("CombatStateUpdated", session);
            }
        }
    }

    public async Task UpdateSave(string combatId, string participantId, string saveType, int value)
    {
        if (CombatSessions.TryGetValue(combatId, out var session))
        {
            var participant = session.Participants.FirstOrDefault(p => p.Id.ToString() == participantId);
            if (participant != null)
            {
                switch (saveType.ToLower())
                {
                    case "fortitude":
                        participant.Fortitude = value;
                        break;
                    case "reflex":
                        participant.Reflex = value;
                        break;
                    case "will":
                        participant.Will = value;
                        break;
                }
                
                await Clients.Group($"Combat-{combatId}").SendAsync("SaveUpdated", participantId, saveType, value);
                await Clients.Group($"Combat-{combatId}").SendAsync("CombatStateUpdated", session);
            }
        }
    }

    public async Task AddParticipant(string combatId, CombatParticipant participant)
    {
        if (!CombatSessions.ContainsKey(combatId))
        {
            CombatSessions[combatId] = new CombatSession
            {
                Id = Guid.Parse(combatId),
                Name = $"Combat Session {combatId}",
                Participants = new List<CombatParticipant>()
            };
        }

        if (CombatSessions.TryGetValue(combatId, out var session))
        {
            session.Participants.Add(participant);
            session.Participants = session.Participants.OrderByDescending(p => p.Initiative).ToList();
            
            await Clients.Group($"Combat-{combatId}").SendAsync("ParticipantAdded", participant);
            await Clients.Group($"Combat-{combatId}").SendAsync("CombatStateUpdated", session);
        }
    }

    public async Task RemoveParticipant(string combatId, string participantId)
    {
        if (CombatSessions.TryGetValue(combatId, out var session))
        {
            var participant = session.Participants.FirstOrDefault(p => p.Id.ToString() == participantId);
            if (participant != null)
            {
                session.Participants.Remove(participant);
                
                await Clients.Group($"Combat-{combatId}").SendAsync("ParticipantRemoved", participantId);
                await Clients.Group($"Combat-{combatId}").SendAsync("CombatStateUpdated", session);
            }
        }
    }

    public async Task StartCombat(string combatId)
    {
        if (CombatSessions.TryGetValue(combatId, out var session))
        {
            session.IsActive = true;
            session.CurrentTurn = 0;
            session.Round = 1;
            
            await Clients.Group($"Combat-{combatId}").SendAsync("CombatStarted", session);
            await Clients.Group($"Combat-{combatId}").SendAsync("CombatStateUpdated", session);
        }
    }

    public async Task EndCombat(string combatId)
    {
        if (CombatSessions.TryGetValue(combatId, out var session))
        {
            session.IsActive = false;
            
            await Clients.Group($"Combat-{combatId}").SendAsync("CombatEnded", session);
            await Clients.Group($"Combat-{combatId}").SendAsync("CombatStateUpdated", session);
        }
    }

    public async Task NextTurn(string combatId)
    {
        if (CombatSessions.TryGetValue(combatId, out var session) && session.IsActive)
        {
            session.CurrentTurn++;
            if (session.CurrentTurn >= session.Participants.Count)
            {
                session.CurrentTurn = 0;
                session.Round++;
            }
            
            await Clients.Group($"Combat-{combatId}").SendAsync("TurnChanged", session.CurrentTurn, session.Round);
            await Clients.Group($"Combat-{combatId}").SendAsync("CombatStateUpdated", session);
        }
    }

    public async Task UpdateParticipantConditions(string combatId, string participantId, List<string> conditions)
    {
        if (CombatSessions.TryGetValue(combatId, out var session))
        {
            var participant = session.Participants.FirstOrDefault(p => p.Id.ToString() == participantId);
            if (participant != null)
            {
                participant.Conditions = conditions;
                
                await Clients.Group($"Combat-{combatId}").SendAsync("ConditionsUpdated", participantId, conditions);
                await Clients.Group($"Combat-{combatId}").SendAsync("CombatStateUpdated", session);
            }
        }
    }

    public async Task RollInitiative(string combatId, string participantId)
    {
        if (CombatSessions.TryGetValue(combatId, out var session))
        {
            var participant = session.Participants.FirstOrDefault(p => p.Id.ToString() == participantId);
            if (participant != null)
            {
                // Roll d20 + participant's initiative modifier
                var random = new Random();
                var roll = random.Next(1, 21);
                var modifier = participant.Initiative; // Assuming this was the modifier, not the result
                var result = roll + modifier;
                
                participant.Initiative = result;
                session.Participants = session.Participants.OrderByDescending(p => p.Initiative).ToList();
                
                await Clients.Group($"Combat-{combatId}").SendAsync("InitiativeRolled", participantId, roll, modifier, result);
                await Clients.Group($"Combat-{combatId}").SendAsync("CombatStateUpdated", session);
            }
        }
    }

    public async Task AddTemporaryEffect(string combatId, string participantId, object effect)
    {
        if (CombatSessions.TryGetValue(combatId, out var session))
        {
            var participant = session.Participants.FirstOrDefault(p => p.Id.ToString() == participantId);
            if (participant != null)
            {
                await Clients.Group($"Combat-{combatId}").SendAsync("TemporaryEffectAdded", participantId, effect);
                await Clients.Group($"Combat-{combatId}").SendAsync("CombatStateUpdated", session);
            }
        }
    }

    public async Task RemoveTemporaryEffect(string combatId, string participantId, string effectId)
    {
        if (CombatSessions.TryGetValue(combatId, out var session))
        {
            var participant = session.Participants.FirstOrDefault(p => p.Id.ToString() == participantId);
            if (participant != null)
            {
                await Clients.Group($"Combat-{combatId}").SendAsync("TemporaryEffectRemoved", participantId, effectId);
                await Clients.Group($"Combat-{combatId}").SendAsync("CombatStateUpdated", session);
            }
        }
    }

    public async Task UpdateParticipantPosition(string combatId, string participantId, int x, int y)
    {
        await Clients.Group($"Combat-{combatId}").SendAsync("ParticipantPositionUpdated", participantId, x, y);
    }

    public async Task SendCombatMessage(string combatId, string message, string messageType = "action")
    {
        try
        {
            var userName = Context.User?.Identity?.Name ?? "Unknown";
            
            await Clients.Group($"Combat-{combatId}").SendAsync("CombatMessageReceived", new
            {
                Message = message,
                Type = messageType, // "action", "damage", "heal", "system", "chat"
                Sender = userName,
                Timestamp = DateTime.UtcNow
            });
        }
        catch (Exception ex)
        {
            await Clients.Caller.SendAsync("Error", $"Failed to send combat message: {ex.Message}");
            throw;
        }
    }

    public async Task RequestReroll(string combatId, string rollType, string reason)
    {
        try
        {
            var userName = Context.User?.Identity?.Name ?? "Unknown";
            
            await Clients.Group($"Combat-{combatId}").SendAsync("RerollRequested", new
            {
                RollType = rollType,
                Reason = reason,
                RequestedBy = userName,
                Timestamp = DateTime.UtcNow
            });
        }
        catch (Exception ex)
        {
            await Clients.Caller.SendAsync("Error", $"Failed to request reroll: {ex.Message}");
            throw;
        }
    }

    public async Task ShareDiceRoll(string combatId, object rollResult)
    {
        try
        {
            var userName = Context.User?.Identity?.Name ?? "Unknown";
            
            await Clients.Group($"Combat-{combatId}").SendAsync("DiceRollShared", new
            {
                Roll = rollResult,
                RolledBy = userName,
                Timestamp = DateTime.UtcNow
            });
        }
        catch (Exception ex)
        {
            await Clients.Caller.SendAsync("Error", $"Failed to share dice roll: {ex.Message}");
            throw;
        }
    }

    public async Task UpdateCombatMap(string combatId, object mapData)
    {
        if (CombatSessions.TryGetValue(combatId, out var session))
        {
            await Clients.Group($"Combat-{combatId}").SendAsync("CombatMapUpdated", mapData);
        }
    }

    public async Task PauseCombat(string combatId)
    {
        if (CombatSessions.TryGetValue(combatId, out var session))
        {
            session.IsPaused = true;
            
            await Clients.Group($"Combat-{combatId}").SendAsync("CombatPaused", session);
            await Clients.Group($"Combat-{combatId}").SendAsync("CombatStateUpdated", session);
        }
    }

    public async Task ResumeCombat(string combatId)
    {
        if (CombatSessions.TryGetValue(combatId, out var session))
        {
            session.IsPaused = false;
            
            await Clients.Group($"Combat-{combatId}").SendAsync("CombatResumed", session);
            await Clients.Group($"Combat-{combatId}").SendAsync("CombatStateUpdated", session);
        }
    }

    private PathfinderCampaignManager.Presentation.Shared.Models.CombatParticipant MapToSharedParticipant(PathfinderCampaignManager.Domain.Entities.Combat.CombatParticipant participant)
    {
        return new PathfinderCampaignManager.Presentation.Shared.Models.CombatParticipant
        {
            Id = participant.Id,
            Name = participant.Name,
            Type = participant.Type.ToString(),
            Initiative = participant.Initiative,
            InitiativeModifier = participant.InitiativeModifier,
            HitPoints = participant.HitPoints,
            CurrentHitPoints = participant.CurrentHitPoints,
            ArmorClass = participant.ArmorClass,
            FortitudeSave = participant.FortitudeSave,
            ReflexSave = participant.ReflexSave,
            WillSave = participant.WillSave,
            IsPlayerCharacter = participant.IsPlayerCharacter,
            IsHidden = participant.IsHidden,
            CharacterId = participant.CharacterId?.ToString(),
            PlayerId = participant.PlayerId?.ToString(),
            Notes = participant.Notes,
            Conditions = participant.Conditions?.ToList() ?? new List<string>(),
            Level = participant.Level,
            Class = participant.Class,
            Ancestry = participant.Ancestry,
            CreatureType = participant.Class ?? "Unknown",
            PassivePerception = 10 + participant.Level // Simple calculation
        };
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        // Clean up any group memberships
        await base.OnDisconnectedAsync(exception);
    }
}