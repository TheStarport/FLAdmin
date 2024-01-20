// This file is used by Code Analysis to maintain SuppressMessage
// attributes that are applied to this project.
// Project-level suppressions either have no target or are given
// a specific target and scoped to a namespace, type, member, etc.

using System.Diagnostics.CodeAnalysis;

[assembly: SuppressMessage("Style", "IDE0060:Remove unused parameter", Justification = "Cannot remove from states management, cannot use discard character twice")]
[assembly: SuppressMessage("Style", "IDE0290:Use primary constructor", Justification = "<Pending>", Scope = "member", Target = "~M:Common.Messaging.RpcMessage.#ctor(System.Collections.Generic.IDictionary{System.String,System.Object})")]
[assembly: SuppressMessage("Style", "IDE0290:Use primary constructor", Justification = "<Pending>", Scope = "member", Target = "~M:Common.State.ModalInfo.ModalInfoAction.#ctor(System.String,System.Boolean)")]
[assembly: SuppressMessage("Style", "IDE0290:Use primary constructor", Justification = "<Pending>", Scope = "member", Target = "~M:Common.State.ServerEvents.ConsoleMessageAction.#ctor(System.String)")]
[assembly: SuppressMessage("Style", "IDE0290:Use primary constructor", Justification = "<Pending>", Scope = "member", Target = "~M:Common.State.ServerStats.Load.#ctor(System.UInt32)")]
[assembly: SuppressMessage("Style", "IDE0290:Use primary constructor", Justification = "<Pending>", Scope = "member", Target = "~M:Common.State.ServerStats.Usage.#ctor(System.UInt32)")]
