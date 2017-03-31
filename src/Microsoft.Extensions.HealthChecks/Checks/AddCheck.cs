// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Extensions.HealthChecks
{
    public static partial class HealthCheckBuilderExtensions
    {
        // Lambda versions of AddCheck for Func/Func<Task>/Func<ValueTask>

        public static HealthCheckBuilder AddCheck(this HealthCheckBuilder builder, string name, Func<IHealthCheckResult> check)
        {
            Guard.ArgumentNotNull(nameof(builder), builder);

            builder.AddCheck(name, HealthCheck.FromCheck(check, builder.DefaultCacheDuration));
            return builder;
        }

        public static HealthCheckBuilder AddCheck(this HealthCheckBuilder builder, string name, Func<CancellationToken, IHealthCheckResult> check)
        {
            Guard.ArgumentNotNull(nameof(builder), builder);

            builder.AddCheck(name, HealthCheck.FromCheck(check, builder.DefaultCacheDuration));
            return builder;
        }

        public static HealthCheckBuilder AddCheck(this HealthCheckBuilder builder, string name, Func<IHealthCheckResult> check, TimeSpan cacheDuration)
        {
            Guard.ArgumentNotNull(nameof(builder), builder);

            builder.AddCheck(name, HealthCheck.FromCheck(check, cacheDuration));
            return builder;
        }

        public static HealthCheckBuilder AddCheck(this HealthCheckBuilder builder, string name, Func<CancellationToken, IHealthCheckResult> check, TimeSpan cacheDuration)
        {
            Guard.ArgumentNotNull(nameof(builder), builder);

            builder.AddCheck(name, HealthCheck.FromCheck(check, cacheDuration));
            return builder;
        }

        public static HealthCheckBuilder AddCheck(this HealthCheckBuilder builder, string name, Func<Task<IHealthCheckResult>> check)
        {
            Guard.ArgumentNotNull(nameof(builder), builder);

            builder.AddCheck(name, HealthCheck.FromTaskCheck(check, builder.DefaultCacheDuration));
            return builder;
        }

        public static HealthCheckBuilder AddCheck(this HealthCheckBuilder builder, string name, Func<CancellationToken, Task<IHealthCheckResult>> check)
        {
            Guard.ArgumentNotNull(nameof(builder), builder);

            builder.AddCheck(name, HealthCheck.FromTaskCheck(check, builder.DefaultCacheDuration));
            return builder;
        }

        public static HealthCheckBuilder AddCheck(this HealthCheckBuilder builder, string name, Func<Task<IHealthCheckResult>> check, TimeSpan cacheDuration)
        {
            Guard.ArgumentNotNull(nameof(builder), builder);

            builder.AddCheck(name, HealthCheck.FromTaskCheck(check, cacheDuration));
            return builder;
        }

        public static HealthCheckBuilder AddCheck(this HealthCheckBuilder builder, string name, Func<CancellationToken, Task<IHealthCheckResult>> check, TimeSpan cacheDuration)
        {
            Guard.ArgumentNotNull(nameof(builder), builder);

            builder.AddCheck(name, HealthCheck.FromTaskCheck(check, cacheDuration));
            return builder;
        }

        public static HealthCheckBuilder AddValueTaskCheck(this HealthCheckBuilder builder, string name, Func<ValueTask<IHealthCheckResult>> check)
        {
            Guard.ArgumentNotNull(nameof(builder), builder);

            builder.AddCheck(name, HealthCheck.FromValueTaskCheck(check, builder.DefaultCacheDuration));
            return builder;
        }

        public static HealthCheckBuilder AddValueTaskCheck(this HealthCheckBuilder builder, string name, Func<CancellationToken, ValueTask<IHealthCheckResult>> check)
        {
            Guard.ArgumentNotNull(nameof(builder), builder);

            builder.AddCheck(name, HealthCheck.FromValueTaskCheck(check, builder.DefaultCacheDuration));
            return builder;
        }

        public static HealthCheckBuilder AddValueTaskCheck(this HealthCheckBuilder builder, string name, Func<ValueTask<IHealthCheckResult>> check, TimeSpan cacheDuration)
        {
            Guard.ArgumentNotNull(nameof(builder), builder);

            builder.AddCheck(name, HealthCheck.FromValueTaskCheck(check, cacheDuration));
            return builder;
        }

        public static HealthCheckBuilder AddValueTaskCheck(this HealthCheckBuilder builder, string name, Func<CancellationToken, ValueTask<IHealthCheckResult>> check, TimeSpan cacheDuration)
        {
            Guard.ArgumentNotNull(nameof(builder), builder);

            builder.AddCheck(name, HealthCheck.FromValueTaskCheck(check, cacheDuration));
            return builder;
        }

        public static HealthCheckBuilder AddRetryCheck(this HealthCheckBuilder builder,
                                                       string name, Func<ValueTask<IHealthCheckResult>> check,
                                                       int threshold = 5,
                                                       TimeSpan? delay = null,
                                                       CheckStatus partiallyStatus = CheckStatus.Warning)
        {
            Guard.ArgumentNotNull(nameof(builder), builder);
            if (delay == null)
            {
                delay = TimeSpan.FromSeconds(0);
            }

            builder.AddCheck(name, async () =>
            {
                var result = new CompositeHealthCheckResult(partiallyStatus);
                for (var i = 0; i < threshold; i++)
                {
                    try
                    {
                        var checkResult = await check();
                        result.Add($"Run {i}", checkResult);
                    }
                    catch (Exception ex)
                    {
                        var data = new Dictionary<string, object>
                        {
                            { "Details", ex }
                        };
                        var healthCheckResult = HealthCheckResult.Unhealthy(ex.Message, data);
                        result.Add($"Run {i} failed", healthCheckResult);
                    }
                    await Task.Delay(delay.Value);
                }
                return result;
            });

            return builder;
        }

        public static HealthCheckBuilder AddRetryCheck(this HealthCheckBuilder builder, string groupName,
                                                       Action<HealthCheckBuilder> innerChecks,
                                                       int threshold = 5,
                                                       TimeSpan? delay = null,
                                                       CheckStatus partiallyStatus = CheckStatus.Warning)
        {
            var innerBuilder = new HealthCheckBuilder();
            innerChecks(innerBuilder);

            builder.AddCheck($"Group {groupName}", async () =>
            {
                var result = new CompositeHealthCheckResult(partiallyStatus);

                foreach (var check in innerBuilder.Checks)
                {
                    var val = await check.Value.CheckAsync();
                    result.Add(check.Key, val);
                }

                return result;
            });

            return builder;
        }
    }
}
