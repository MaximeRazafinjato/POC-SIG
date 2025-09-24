using System;
using System.Collections.Generic;

namespace PocSig.Domain.Entities;

public class Layer
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int Srid { get; set; } = 4326;
    public string GeometryType { get; set; } = string.Empty;
    public DateTime CreatedUtc { get; set; }
    public DateTime UpdatedUtc { get; set; }
    public string? MetadataJson { get; set; }

    public ICollection<FeatureEntity> Features { get; set; } = new List<FeatureEntity>();
}