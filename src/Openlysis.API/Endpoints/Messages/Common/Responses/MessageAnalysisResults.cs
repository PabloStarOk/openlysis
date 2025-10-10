using Openlysis.API.Endpoints.EmailAddresses.GetReputation;
using Openlysis.API.Endpoints.Files.Common.Responses;
using Openlysis.API.Endpoints.Phones.GetReputation;
using Openlysis.API.Endpoints.URLs.Common;

namespace Openlysis.API.Endpoints.Messages.Common.Responses;

/// <summary>
/// Analysis and reputation results of the detected data in a message.
/// </summary>
/// <param name="FileMultiAnalyses">A collection of <see cref="FileMultiAnalysisDto"/>.</param>
/// <param name="UrlMultiAnalyses">A collection of <see cref="UrlMultiAnalysisDto"/>.</param>
/// <param name="EmailAddressMultiReputations">A collection of <see cref="EmailAddressMultiReputationDto"/>.</param>
/// <param name="PhoneNumberMultiReputations">A collection of <see cref="PhoneMultiReputationDto"/>.</param>
public record MessageAnalysisResults(
    IEnumerable<FileMultiAnalysisDto> FileMultiAnalyses,
    IEnumerable<UrlMultiAnalysisDto> UrlMultiAnalyses,
    IEnumerable<EmailAddressMultiReputationDto> EmailAddressMultiReputations,
    IEnumerable<PhoneMultiReputationDto> PhoneNumberMultiReputations);