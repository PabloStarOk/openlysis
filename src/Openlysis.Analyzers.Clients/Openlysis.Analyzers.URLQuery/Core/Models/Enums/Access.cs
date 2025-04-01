namespace Openlysis.Analyzers.URLQuery.Core.Models.Enums;

/// <summary>
/// Specifies the access level for an analysis.
/// </summary>
internal enum Access
{
    /// <summary>
    /// The resource is publicly accessible.
    /// </summary>
    Public,

    /// <summary>
    /// The resource is accessible with restrictions.
    /// </summary>
    Restricted,

    /// <summary>
    /// The resource is privately accessible.
    /// </summary>
    Private,
}