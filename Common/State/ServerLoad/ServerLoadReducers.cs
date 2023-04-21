﻿using Fluxor;

namespace Common.State.ServerLoad;

public static class ServerLoadReducers
{
    [ReducerMethod]
    public static ServerLoadState ReduceServerLoadUpdate(ServerLoadState _, ServerLoadAction action) => new(action.ServerLoad);
}
