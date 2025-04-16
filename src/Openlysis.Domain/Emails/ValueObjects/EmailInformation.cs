using Openlysis.Domain.Common.Entities;
using Openlysis.Domain.Common.Enums;
using Openlysis.Domain.Common.ValueObjects;

namespace Openlysis.Domain.Emails.ValueObjects;

/// <summary>
/// Represents information about an email message.
/// </summary>
/// <param name="Sender">The email address of the sender.</param>
/// <param name="Subject">The subject of the email.</param>
/// <param name="Content">The content or body of the email.</param>
/// <param name="MessageHashSet">A set of content hashes representing the sender, subject, and content of the email as a whole.</param>
public sealed record EmailInformation(
    string Sender,
    string Subject,
    string Content,
    ContentHashSet MessageHashSet)
    : MessageInformation(
        MessageType.Email,
        Sender,
        Content,
        MessageHashSet);