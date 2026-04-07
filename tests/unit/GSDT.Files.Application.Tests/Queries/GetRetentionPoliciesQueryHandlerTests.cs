using GSDT.Files.Application.DTOs;
using GSDT.Files.Application.Queries.GetRetentionPolicies;
using GSDT.SharedKernel.Application.Data;
using NSubstitute;

namespace GSDT.Files.Application.Tests.Queries;

/// <summary>
/// Unit tests for GetRetentionPoliciesQueryHandler.
/// Validates: returns policies, active filter, ordering by document type and name.
/// </summary>
public sealed class GetRetentionPoliciesQueryHandlerTests
{
    private readonly IReadDbConnection _db;
    private readonly GetRetentionPoliciesQueryHandler _handler;

    private static readonly Guid TenantId = Guid.NewGuid();

    public GetRetentionPoliciesQueryHandlerTests()
    {
        _db = Substitute.For<IReadDbConnection>();
        _handler = new GetRetentionPoliciesQueryHandler(_db);
    }

    // --- Success path ---

    [Fact]
    public async Task Handle_WithPolicies_ReturnsList()
    {
        var policies = new List<RetentionPolicyDto>
        {
            new(Guid.NewGuid(), "Standard Invoice", "Invoice", 365, 90, 2555, true, TenantId),
            new(Guid.NewGuid(), "Short Email", "Email", 90, null, 180, true, TenantId)
        };

        _db.QueryAsync<RetentionPolicyDto>(
                Arg.Any<string>(),
                Arg.Any<object>(),
                Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<IEnumerable<RetentionPolicyDto>>(policies));

        var query = new GetRetentionPoliciesQuery(TenantId);

        var result = await _handler.Handle(query, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(2);
    }

    [Fact]
    public async Task Handle_NoPolicies_ReturnsEmpty()
    {
        var emptyList = new List<RetentionPolicyDto>();

        _db.QueryAsync<RetentionPolicyDto>(
                Arg.Any<string>(),
                Arg.Any<object>(),
                Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<IEnumerable<RetentionPolicyDto>>(emptyList));

        var query = new GetRetentionPoliciesQuery(TenantId);

        var result = await _handler.Handle(query, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_WithoutActiveFilter_ReturnsAll()
    {
        var policies = new List<RetentionPolicyDto>
        {
            new(Guid.NewGuid(), "Active Policy", "Doc", 365, null, null, true, TenantId),
            new(Guid.NewGuid(), "Inactive Policy", "Doc", 365, null, null, false, TenantId)
        };

        _db.QueryAsync<RetentionPolicyDto>(
                Arg.Any<string>(),
                Arg.Any<object>(),
                Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<IEnumerable<RetentionPolicyDto>>(policies));

        var query = new GetRetentionPoliciesQuery(TenantId);

        var result = await _handler.Handle(query, CancellationToken.None);

        result.Value.Should().HaveCount(2);
        result.Value.Should().Contain(p => p.IsActive);
        result.Value.Should().Contain(p => !p.IsActive);
    }

    [Fact]
    public async Task Handle_WithActiveFilterTrue_ReturnsActiveOnly()
    {
        var policies = new List<RetentionPolicyDto>
        {
            new(Guid.NewGuid(), "Active1", "Invoice", 365, null, null, true, TenantId),
            new(Guid.NewGuid(), "Active2", "Email", 180, null, null, true, TenantId)
        };

        _db.QueryAsync<RetentionPolicyDto>(
                Arg.Any<string>(),
                Arg.Any<object>(),
                Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<IEnumerable<RetentionPolicyDto>>(policies));

        var query = new GetRetentionPoliciesQuery(TenantId, IsActive: true);

        var result = await _handler.Handle(query, CancellationToken.None);

        result.Value.Should().AllSatisfy(p => p.IsActive.Should().BeTrue());
    }

    [Fact]
    public async Task Handle_WithActiveFilterFalse_ReturnsInactiveOnly()
    {
        var policies = new List<RetentionPolicyDto>
        {
            new(Guid.NewGuid(), "Inactive1", "Document", 365, null, null, false, TenantId)
        };

        _db.QueryAsync<RetentionPolicyDto>(
                Arg.Any<string>(),
                Arg.Any<object>(),
                Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<IEnumerable<RetentionPolicyDto>>(policies));

        var query = new GetRetentionPoliciesQuery(TenantId, IsActive: false);

        var result = await _handler.Handle(query, CancellationToken.None);

        result.Value.Should().AllSatisfy(p => p.IsActive.Should().BeFalse());
    }

    [Fact]
    public async Task Handle_OrderedByDocumentTypeThenName()
    {
        var policies = new List<RetentionPolicyDto>
        {
            new(Guid.NewGuid(), "Zebra Policy", "Email", 365, null, null, true, TenantId),
            new(Guid.NewGuid(), "Alpha Policy", "Invoice", 365, null, null, true, TenantId),
            new(Guid.NewGuid(), "Beta Policy", "Invoice", 365, null, null, true, TenantId),
            new(Guid.NewGuid(), "Charlie Policy", "Email", 365, null, null, true, TenantId)
        };

        _db.QueryAsync<RetentionPolicyDto>(
                Arg.Any<string>(),
                Arg.Any<object>(),
                Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<IEnumerable<RetentionPolicyDto>>(policies));

        var query = new GetRetentionPoliciesQuery(TenantId);

        var result = await _handler.Handle(query, CancellationToken.None);

        var groupedByType = result.Value
            .GroupBy(p => p.DocumentType)
            .SelectMany(g => g)
            .ToList();

        groupedByType.Should().HaveCount(4);
    }

    [Fact]
    public async Task Handle_ComplexRetentionScenarios()
    {
        var policies = new List<RetentionPolicyDto>
        {
            new(Guid.NewGuid(), "Invoice - 7 Years", "Invoice", 2555, 365, 3650, true, TenantId),
            new(Guid.NewGuid(), "Email - 1 Year", "Email", 365, null, null, true, TenantId),
            new(Guid.NewGuid(), "Temp Files - 30 Days", "Temp", 30, null, null, true, TenantId)
        };

        _db.QueryAsync<RetentionPolicyDto>(
                Arg.Any<string>(),
                Arg.Any<object>(),
                Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<IEnumerable<RetentionPolicyDto>>(policies));

        var query = new GetRetentionPoliciesQuery(TenantId);

        var result = await _handler.Handle(query, CancellationToken.None);

        result.Value.Should().HaveCount(3);
        result.Value.Should().Contain(p => p.ArchiveAfterDays.HasValue);
        result.Value.Should().Contain(p => !p.ArchiveAfterDays.HasValue);
    }

    [Fact]
    public async Task Handle_PassesCorrectParameters()
    {
        var emptyList = new List<RetentionPolicyDto>();

        _db.QueryAsync<RetentionPolicyDto>(
                Arg.Any<string>(),
                Arg.Any<object>(),
                Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<IEnumerable<RetentionPolicyDto>>(emptyList));

        var query = new GetRetentionPoliciesQuery(TenantId, IsActive: true);

        await _handler.Handle(query, CancellationToken.None);

        await _db.Received(1).QueryAsync<RetentionPolicyDto>(
            Arg.Any<string>(),
            Arg.Any<object>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_CancellationTokenPropagated()
    {
        var cts = new CancellationTokenSource();
        var emptyList = new List<RetentionPolicyDto>();

        _db.QueryAsync<RetentionPolicyDto>(
                Arg.Any<string>(),
                Arg.Any<object>(),
                Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<IEnumerable<RetentionPolicyDto>>(emptyList));

        var query = new GetRetentionPoliciesQuery(TenantId);

        await _handler.Handle(query, cts.Token);

        await _db.Received(1).QueryAsync<RetentionPolicyDto>(
            Arg.Any<string>(),
            Arg.Any<object>(),
            cts.Token);
    }

    [Fact]
    public async Task Handle_DatabaseException_Propagates()
    {
        _db.QueryAsync<RetentionPolicyDto>(
                Arg.Any<string>(),
                Arg.Any<object>(),
                Arg.Any<CancellationToken>())
            .Returns(Task.FromException<IEnumerable<RetentionPolicyDto>>(
                new InvalidOperationException("DB connection error")));

        var query = new GetRetentionPoliciesQuery(TenantId);

        var act = async () => await _handler.Handle(query, CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("*DB connection error*");
    }
}
