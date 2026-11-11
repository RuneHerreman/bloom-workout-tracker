using Bloom.Application.Contracts.Ports;

namespace UnitTests.Application.Mocks;

/// <summary>Records what it was asked to issue; the token is a readable stand-in.</summary>
public sealed class StubTokenIssuer : ITokenIssuer
{
    public List<SessionTicket> Issued { get; } = [];

    public string Issue(SessionTicket ticket)
    {
        Issued.Add(ticket);
        return $"token:{ticket.UserId.Value}:{ticket.Stamp}:{ticket.Persistent}";
    }
}
