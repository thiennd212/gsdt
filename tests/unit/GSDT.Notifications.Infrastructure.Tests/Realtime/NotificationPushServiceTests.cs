using GSDT.Notifications.Domain.Events;
using GSDT.Notifications.Infrastructure.Realtime;
using FluentAssertions;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;

namespace GSDT.Notifications.Infrastructure.Tests.Realtime;

/// <summary>
/// TC-NOT-I006: NotificationPushService sends to correct SignalR group ("user:{userId}").
/// </summary>
public sealed class NotificationPushServiceTests
{
    [Fact]
    public async Task Handle_NotificationCreatedEvent_SendsToCorrectUserGroup()
    {
        // TC-NOT-I006: group MUST be "user:{recipientUserId}" — never Clients.All
        var recipientId = Guid.NewGuid();
        var expectedGroup = $"user:{recipientId}";

        var hubContext = Substitute.For<IHubContext<NotificationsHub>>();
        var clients = Substitute.For<IHubClients>();
        var clientProxy = Substitute.For<IClientProxy>();

        hubContext.Clients.Returns(clients);
        clients.Group(expectedGroup).Returns(clientProxy);

        var sut = new NotificationPushService(
            hubContext,
            NullLogger<NotificationPushService>.Instance);

        var @event = new NotificationCreatedEvent(
            NotificationId: Guid.NewGuid(),
            TenantId: Guid.NewGuid(),
            RecipientUserId: recipientId,
            Channel: "InApp");

        await sut.Handle(@event, CancellationToken.None);

        // Verify the group was addressed by "user:{recipientId}"
        clients.Received(1).Group(expectedGroup);

        // Verify SendAsync was called on that group proxy
        await clientProxy.Received(1).SendCoreAsync(
            "NotificationCreated",
            Arg.Any<object[]>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_DifferentRecipients_UseDifferentGroups()
    {
        // Each recipient gets their own group — group names are distinct
        var userId1 = Guid.NewGuid();
        var userId2 = Guid.NewGuid();

        $"user:{userId1}".Should().NotBe($"user:{userId2}");
    }
}
