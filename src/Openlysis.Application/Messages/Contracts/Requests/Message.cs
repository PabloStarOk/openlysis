using Openlysis.Domain.Messages.Enums;

namespace Openlysis.Application.Messages.Contracts.Requests;

/// <summary>
/// Represents a message with details about its type, sender, subject, and content.
/// </summary>
/// <param name="Type">The type of the message.</param>
/// <param name="Sender">The sender of the message.</param>
/// <param name="Subject">The subject of the message (optional).</param>
/// <param name="Content">The content of the message.</param>
public record Message(
    MessageType Type,
    string Sender,
    string? Subject,
    string Content);