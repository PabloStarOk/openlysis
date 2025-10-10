namespace Openlysis.Analyzers.HybridAnalysis.Core.Models.Enums;

/// <summary>
/// Represents the environments available for sandbox analyses.
/// </summary>
public enum SandboxEnvironment
{
    /// <summary>
    /// MacOS Catalina 64-bit environment.
    /// </summary>
    MacCatalinaX64 = 400,

    /// <summary>
    /// Linux Ubuntu 64-bit environment.
    /// </summary>
    LinuxUbuntuX64 = 310,

    /// <summary>
    /// Android static analysis environment. Doesn't work with HTML documents (Websites).
    /// </summary>
    AndroidStaticAnalysis = 200,

    /// <summary>
    /// Windows 10 64-bit environment.
    /// </summary>
    Windows10X64 = 160,

    /// <summary>
    /// Windows 11 64-bit environment.
    /// </summary>
    Windows11X64 = 140,

    /// <summary>
    /// Windows 7 64-bit environment.
    /// </summary>
    Windows7X64 = 120,

    /// <summary>
    /// Windows 7 32-bit environment with HWP support.
    /// </summary>
    Windows7X32HwpSupport = 110,

    /// <summary>
    /// Windows 7 32-bit environment.
    /// </summary>
    Windows7X32 = 100,
}