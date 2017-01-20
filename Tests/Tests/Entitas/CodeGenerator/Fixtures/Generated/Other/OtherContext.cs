﻿using Entitas;
using Entitas.Api;

public sealed partial class OtherContext : Context<OtherEntity> {

    public OtherContext(int totalComponents, int startCreationIndex, ContextInfo contextInfo)
        : base(totalComponents, startCreationIndex, contextInfo) {
    }
}