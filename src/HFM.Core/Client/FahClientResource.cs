﻿namespace HFM.Core.Client;

[System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
public record FahClientResource : ClientResource
{
    public int SlotId { get; init; }

    public FahClientSlotDescription? SlotDescription { get; init; }
}