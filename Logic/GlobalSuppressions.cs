// This file is used by Code Analysis to maintain SuppressMessage
// attributes that are applied to this project.
// Project-level suppressions either have no target or are given
// a specific target and scoped to a namespace, type, member, etc.

using System.Diagnostics.CodeAnalysis;

[assembly: SuppressMessage("Performance", "CA1848:Use the LoggerMessage delegates", Justification = "<Pending>")]
[assembly: SuppressMessage("Style", "IDE0305:Simplify collection initialization", Justification = "Suggestion doesn't work.")]
[assembly: SuppressMessage("Style", "IDE0290:Use primary constructor", Justification = "<Pending>", Scope = "member", Target = "~M:Logic.Auth.JwtAuthStateProvider.#ctor(Common.Auth.IJwtProvider,Common.Auth.IPersistentRoleProvider)")]
[assembly: SuppressMessage("Style", "IDE0290:Use primary constructor", Justification = "<Pending>", Scope = "member", Target = "~M:Logic.Auth.JwtProvider.#ctor(Common.Auth.IKeyProvider)")]
[assembly: SuppressMessage("Style", "IDE0290:Use primary constructor", Justification = "<Pending>", Scope = "member", Target = "~M:Logic.Auth.KeyProvider.#ctor(Microsoft.Extensions.Logging.ILogger{Logic.Auth.KeyProvider})")]
[assembly: SuppressMessage("Style", "IDE0290:Use primary constructor", Justification = "<Pending>", Scope = "member", Target = "~M:Logic.Freelancer.FreelancerDataProvider.#ctor(Microsoft.Extensions.Logging.ILogger{Logic.Freelancer.FreelancerDataProvider},Common.Configuration.FLAdminConfiguration)")]
[assembly: SuppressMessage("Style", "IDE0290:Use primary constructor", Justification = "<Pending>", Scope = "member", Target = "~M:Logic.Jobs.JobManager.#ctor(Quartz.ISchedulerFactory)")]
[assembly: SuppressMessage("Style", "IDE0290:Use primary constructor", Justification = "<Pending>", Scope = "member", Target = "~M:Logic.Jobs.RunShellJob.#ctor(System.String)")]
[assembly: SuppressMessage("Style", "IDE0290:Use primary constructor", Justification = "<Pending>", Scope = "member", Target = "~M:Logic.Managers.StatsManager.#ctor(Fluxor.IDispatcher)")]
[assembly: SuppressMessage("Style", "IDE0290:Use primary constructor", Justification = "<Pending>", Scope = "member", Target = "~M:Logic.Messaging.ExchangeSubscriber.#ctor(Common.Messaging.IChannelProvider,Common.Messaging.IMessageSubscriber,Microsoft.Extensions.Logging.ILogger{Logic.Messaging.ExchangeSubscriber})")]
[assembly: SuppressMessage("Style", "IDE0290:Use primary constructor", Justification = "<Pending>", Scope = "member", Target = "~M:Logic.Messaging.MessagePublisher.#ctor(Microsoft.Extensions.Logging.ILogger{Logic.Messaging.MessagePublisher},Common.Messaging.IChannelProvider)")]
[assembly: SuppressMessage("Style", "IDE0290:Use primary constructor", Justification = "<Pending>", Scope = "member", Target = "~M:Logic.Messaging.MessageSubscriber.#ctor(Common.Messaging.IChannelProvider,Microsoft.Extensions.Logging.ILogger{Logic.Messaging.MessageSubscriber})")]
