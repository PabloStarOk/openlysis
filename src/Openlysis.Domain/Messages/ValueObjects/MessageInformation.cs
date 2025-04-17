using Openlysis.Domain.Common.Entities;
using Openlysis.Domain.Messages.Enums;

namespace Openlysis.Domain.Messages.ValueObjects;

/// <summary>
/// Represents information about a generic message.
/// </summary>
public record MessageInformation
{
    /// <summary>
    /// Gets the type of the message.
    /// </summary>
    public MessageType Type { get; }

    /// <summary>
    /// Gets the sender of the message.
    /// </summary>
    public string Sender { get; }

    /// <summary>
    /// Gets the subject of the message. This can be null if no subject is provided.
    /// </summary>
    public string? Subject { get; }

    /// <summary>
    /// Gets the content of the message.
    /// </summary>
    public string Content { get; }

    /// <summary>
    /// Gets the hash set representing the message sender and content as a whole.
    /// </summary>
    public ContentHashSet MessageHashSet { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="MessageInformation"/> class with the specified parameters.
    /// </summary>
    /// <param name="type">The type of the message.</param>
    /// <param name="sender">The sender of the message.</param>
    /// <param name="subject">The subject of the message. Can be null if no subject is provided.</param>
    /// <param name="content">The content of the message.</param>
    /// <param name="messageHashSet">The hash set representing the message sender and content as a whole.</param>
    public MessageInformation(
        MessageType type,
        string sender,
        string? subject,
        string content,
        ContentHashSet messageHashSet)
    {
        Type = type;
        Sender = sender;
        Subject = subject;
        Content = content;
        MessageHashSet = messageHashSet;
    }

    // For EF Core.
#pragma warning disable CS8618
#pragma warning disable S1144
    /// <summary>
    /// Initializes a new instance of the <see cref="MessageInformation"/> class for EF Core.
    /// </summary>
    /// <remarks>
    /// This constructor is required by EF Core and should not be used directly in application code.
    /// </remarks>
    protected MessageInformation()
    {
    }
#pragma warning restore S1144
#pragma warning restore CS8618
}