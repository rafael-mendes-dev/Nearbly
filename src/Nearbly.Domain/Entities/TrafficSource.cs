using System.Text.Json.Serialization;

namespace Nearbly.Domain.Entities;

[JsonConverter(typeof(JsonStringEnumConverter<TrafficSource>))]
public enum TrafficSource
{
    Nfc,
    QrCode,
    Direct,
    Unknown
}
