using Nearbly.Domain.Entities;

namespace Nearbly.Api.Endpoints;

internal static class TrafficSourceParser
{
    public static TrafficSource Parse(string? value) => value?.Trim().ToLowerInvariant() switch
    {
        "nfc" => TrafficSource.Nfc,
        "qr" or "qr_code" or "qrcode" => TrafficSource.QrCode,
        "direct" or null or "" => TrafficSource.Direct,
        _ => TrafficSource.Unknown
    };
}
