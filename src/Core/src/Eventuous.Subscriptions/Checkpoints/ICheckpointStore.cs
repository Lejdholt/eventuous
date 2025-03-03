// Copyright (C) Eventuous HQ OÜ. All rights reserved
// Licensed under the Apache License, Version 2.0.

namespace Eventuous.Subscriptions.Checkpoints;

[PublicAPI]
public interface ICheckpointStore {
    ValueTask<Checkpoint> GetLastCheckpoint(string checkpointId, CancellationToken cancellationToken);

    ValueTask<Checkpoint> StoreCheckpoint(Checkpoint checkpoint, bool force, CancellationToken cancellationToken);
}