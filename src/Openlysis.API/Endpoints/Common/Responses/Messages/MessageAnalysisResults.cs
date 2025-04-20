using Openlysis.API.Endpoints.EmailAddresses.GetReputation;
using Openlysis.API.Endpoints.Files.Common.Responses;
using Openlysis.API.Endpoints.Phones.GetReputation;
using Openlysis.API.Endpoints.URLs.Common;

namespace Openlysis.API.Endpoints.Common.Responses.Messages;

/// <summary>
/// Analysis and reputation results of the detected data in a message.
/// </summary>
/// <param name="FileMultiAnalyses">A collection of file multi-analysis results.</param>
/// <param name="UrlMultiAnalyses">A collection of URL multi-analysis results.</param>
/// <param name="EmailAddressesReputations">A collection of email address reputation results.</param>
/// <param name="PhoneNumbersReputations">A collection of phone number reputation results.</param>
public record MessageAnalysisResults(
    IEnumerable<FileMultiAnalysisDto> FileMultiAnalyses,
    IEnumerable<UrlMultiAnalysisDto> UrlMultiAnalyses,
    IEnumerable<EmailAddressMultiReputationDto> EmailAddressesReputations,
    IEnumerable<PhoneMultiReputationDto> PhoneNumbersReputations);