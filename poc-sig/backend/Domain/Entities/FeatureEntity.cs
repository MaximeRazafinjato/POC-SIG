using System;
using NetTopologySuite.Geometries;

namespace PocSig.Domain.Entities;

public class FeatureEntity
{
    public int Id { get; set; }
    public int LayerId { get; set; }
    public string? PropertiesJson { get; set; }
    public Geometry Geometry { get; set; } = null!;
    public DateTime ValidFromUtc { get; set; }
    public DateTime? ValidToUtc { get; set; }

    public Layer Layer { get; set; } = null!;
}