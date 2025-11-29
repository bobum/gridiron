using Riok.Mapperly.Abstractions;
using EngineTeam = Gridiron.Engine.Domain.Team;
using EnginePlayer = Gridiron.Engine.Domain.Player;
using EngineGame = Gridiron.Engine.Domain.Game;
using EngineCoach = Gridiron.Engine.Domain.Coach;
using EngineTrainer = Gridiron.Engine.Domain.Trainer;
using EngineScout = Gridiron.Engine.Domain.Scout;
using EngineDepthChart = Gridiron.Engine.Domain.DepthChart;
using EngineInjury = Gridiron.Engine.Domain.Injury;
using EnginePositions = Gridiron.Engine.Domain.Positions;
using DomainTeam = Team;
using DomainPlayer = DomainObjects.Player;
using DomainCoach = DomainObjects.Coach;
using DomainTrainer = DomainObjects.Trainer;
using DomainScout = DomainObjects.Scout;
using DomainDepthChart = DomainObjects.DepthChart;
using DomainInjury = DomainObjects.Injury;
using DomainPositions = DomainObjects.Positions;

namespace GameManagement.Mapping;

/// <summary>
/// Mapperly-generated mapper for converting between EF entities (DomainObjects)
/// and engine domain objects (Gridiron.Engine.Domain).
/// </summary>
[Mapper]
public partial class GridironMapper
{
    // ========================================
    // TEAM MAPPINGS
    // ========================================

    /// <summary>
    /// Maps an EF Team entity to an engine Team for simulation.
    /// Ignores database-specific properties (Id, DivisionId, soft delete fields).
    /// </summary>
    [MapperIgnoreSource(nameof(DomainTeam.Id))]
    [MapperIgnoreSource(nameof(DomainTeam.DivisionId))]
    [MapperIgnoreSource(nameof(DomainTeam.IsDeleted))]
    [MapperIgnoreSource(nameof(DomainTeam.DeletedAt))]
    [MapperIgnoreSource(nameof(DomainTeam.DeletedBy))]
    [MapperIgnoreSource(nameof(DomainTeam.DeletionReason))]
    public partial EngineTeam ToEngineTeam(DomainTeam entity);

    /// <summary>
    /// Maps an engine Team back to an EF Team entity.
    /// Note: Id and other EF-specific properties must be set separately.
    /// </summary>
    [MapperIgnoreTarget(nameof(DomainTeam.Id))]
    [MapperIgnoreTarget(nameof(DomainTeam.DivisionId))]
    [MapperIgnoreTarget(nameof(DomainTeam.IsDeleted))]
    [MapperIgnoreTarget(nameof(DomainTeam.DeletedAt))]
    [MapperIgnoreTarget(nameof(DomainTeam.DeletedBy))]
    [MapperIgnoreTarget(nameof(DomainTeam.DeletionReason))]
    public partial DomainTeam ToEntityTeam(EngineTeam engineTeam);

    /// <summary>
    /// Updates an existing EF Team entity from an engine Team.
    /// Preserves Id and EF-specific properties.
    /// </summary>
    [MapperIgnoreTarget(nameof(DomainTeam.Id))]
    [MapperIgnoreTarget(nameof(DomainTeam.DivisionId))]
    [MapperIgnoreTarget(nameof(DomainTeam.IsDeleted))]
    [MapperIgnoreTarget(nameof(DomainTeam.DeletedAt))]
    [MapperIgnoreTarget(nameof(DomainTeam.DeletedBy))]
    [MapperIgnoreTarget(nameof(DomainTeam.DeletionReason))]
    public partial void UpdateTeamEntity(EngineTeam source, DomainTeam target);

    // ========================================
    // PLAYER MAPPINGS
    // ========================================

    /// <summary>
    /// Maps an EF Player entity to an engine Player for simulation.
    /// </summary>
    [MapperIgnoreSource(nameof(DomainPlayer.Id))]
    [MapperIgnoreSource(nameof(DomainPlayer.TeamId))]
    [MapperIgnoreSource(nameof(DomainPlayer.IsDeleted))]
    [MapperIgnoreSource(nameof(DomainPlayer.DeletedAt))]
    [MapperIgnoreSource(nameof(DomainPlayer.DeletedBy))]
    [MapperIgnoreSource(nameof(DomainPlayer.DeletionReason))]
    public partial EnginePlayer ToEnginePlayer(DomainPlayer entity);

    /// <summary>
    /// Maps an engine Player back to an EF Player entity.
    /// </summary>
    [MapperIgnoreTarget(nameof(DomainPlayer.Id))]
    [MapperIgnoreTarget(nameof(DomainPlayer.TeamId))]
    [MapperIgnoreTarget(nameof(DomainPlayer.IsDeleted))]
    [MapperIgnoreTarget(nameof(DomainPlayer.DeletedAt))]
    [MapperIgnoreTarget(nameof(DomainPlayer.DeletedBy))]
    [MapperIgnoreTarget(nameof(DomainPlayer.DeletionReason))]
    public partial DomainPlayer ToEntityPlayer(EnginePlayer enginePlayer);

    /// <summary>
    /// Updates an existing EF Player entity from an engine Player.
    /// </summary>
    [MapperIgnoreTarget(nameof(DomainPlayer.Id))]
    [MapperIgnoreTarget(nameof(DomainPlayer.TeamId))]
    [MapperIgnoreTarget(nameof(DomainPlayer.IsDeleted))]
    [MapperIgnoreTarget(nameof(DomainPlayer.DeletedAt))]
    [MapperIgnoreTarget(nameof(DomainPlayer.DeletedBy))]
    [MapperIgnoreTarget(nameof(DomainPlayer.DeletionReason))]
    public partial void UpdatePlayerEntity(EnginePlayer source, DomainPlayer target);

    // ========================================
    // SUPPORTING TYPE MAPPINGS
    // ========================================

    [MapperIgnoreSource(nameof(DomainCoach.IsDeleted))]
    [MapperIgnoreSource(nameof(DomainCoach.DeletedAt))]
    [MapperIgnoreSource(nameof(DomainCoach.DeletedBy))]
    [MapperIgnoreSource(nameof(DomainCoach.DeletionReason))]
    public partial EngineCoach ToEngineCoach(DomainCoach entity);

    [MapperIgnoreTarget(nameof(DomainCoach.IsDeleted))]
    [MapperIgnoreTarget(nameof(DomainCoach.DeletedAt))]
    [MapperIgnoreTarget(nameof(DomainCoach.DeletedBy))]
    [MapperIgnoreTarget(nameof(DomainCoach.DeletionReason))]
    public partial DomainCoach ToEntityCoach(EngineCoach engineCoach);

    [MapperIgnoreSource(nameof(DomainTrainer.IsDeleted))]
    [MapperIgnoreSource(nameof(DomainTrainer.DeletedAt))]
    [MapperIgnoreSource(nameof(DomainTrainer.DeletedBy))]
    [MapperIgnoreSource(nameof(DomainTrainer.DeletionReason))]
    public partial EngineTrainer ToEngineTrainer(DomainTrainer entity);

    [MapperIgnoreTarget(nameof(DomainTrainer.IsDeleted))]
    [MapperIgnoreTarget(nameof(DomainTrainer.DeletedAt))]
    [MapperIgnoreTarget(nameof(DomainTrainer.DeletedBy))]
    [MapperIgnoreTarget(nameof(DomainTrainer.DeletionReason))]
    public partial DomainTrainer ToEntityTrainer(EngineTrainer engineTrainer);

    [MapperIgnoreSource(nameof(DomainScout.IsDeleted))]
    [MapperIgnoreSource(nameof(DomainScout.DeletedAt))]
    [MapperIgnoreSource(nameof(DomainScout.DeletedBy))]
    [MapperIgnoreSource(nameof(DomainScout.DeletionReason))]
    public partial EngineScout ToEngineScout(DomainScout entity);

    [MapperIgnoreTarget(nameof(DomainScout.IsDeleted))]
    [MapperIgnoreTarget(nameof(DomainScout.DeletedAt))]
    [MapperIgnoreTarget(nameof(DomainScout.DeletedBy))]
    [MapperIgnoreTarget(nameof(DomainScout.DeletionReason))]
    public partial DomainScout ToEntityScout(EngineScout engineScout);

    public partial EngineDepthChart ToEngineDepthChart(DomainDepthChart entity);
    public partial DomainDepthChart ToEntityDepthChart(EngineDepthChart engineDepthChart);

    public partial EngineInjury ToEngineInjury(DomainInjury entity);
    public partial DomainInjury ToEntityInjury(EngineInjury engineInjury);

    // ========================================
    // ENUM MAPPINGS
    // ========================================

    public partial EnginePositions ToEnginePosition(DomainPositions position);
    public partial DomainPositions ToEntityPosition(EnginePositions position);

    // ========================================
    // COLLECTION HELPERS
    // ========================================

    public List<EnginePlayer> ToEnginePlayers(List<DomainPlayer> entities)
        => entities.Select(ToEnginePlayer).ToList();

    public List<DomainPlayer> ToEntityPlayers(List<EnginePlayer> enginePlayers)
        => enginePlayers.Select(ToEntityPlayer).ToList();
}
