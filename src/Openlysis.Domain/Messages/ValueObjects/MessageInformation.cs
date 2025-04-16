using Openlysis.Domain.Common.Entities;
using Openlysis.Domain.Messages.Enums;

namespace Openlysis.Domain.Messages.ValueObjects;

/// <summary>
/// Represents information about a generic message.
/// </summary>
/// <param name="Type">The type of the message.</param>
/// <param name="Sender">The sender of the message.</param>
/// <param name="Content">The content of the message.</param>
/// <param name="MessageHashSet">The hash set representing the message sender and content as a whole.</param>
public record MessageInformation(
    MessageType Type,
    string Sender,
    string? Subject,
    string Content,
    ContentHashSet MessageHashSet);