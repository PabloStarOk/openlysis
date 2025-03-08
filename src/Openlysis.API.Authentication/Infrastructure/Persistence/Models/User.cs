using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

using Microsoft.AspNetCore.Identity;

namespace Openlysis.API.Authentication.Infrastructure.Persistence.Models;

/// <summary>
/// Represents a user of the application.
/// </summary>
public class User : IdentityUser
{
    /// <summary>
    /// Gets or sets the hash of the API Key of the user.
    /// </summary>
    [Column(TypeName = "VARCHAR(64)")]
    [Length(64, 64)]
    public string? ApiKeyHash { get; set; }
}