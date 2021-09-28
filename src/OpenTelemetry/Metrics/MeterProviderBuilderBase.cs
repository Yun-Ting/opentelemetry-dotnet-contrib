// <copyright file="MeterProviderBuilderBase.cs" company="OpenTelemetry Authors">
// Copyright The OpenTelemetry Authors
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// </copyright>

using System;
using System.Collections.Generic;
using System.Diagnostics.Metrics;
using OpenTelemetry.Resources;

namespace OpenTelemetry.Metrics
{
    /// <summary>
    /// Build MeterProvider with Resource, Readers, and Instrumentation.
    /// </summary>
    public abstract class MeterProviderBuilderBase : MeterProviderBuilder
    {
        private readonly List<InstrumentationFactory> instrumentationFactories = new List<InstrumentationFactory>();
        private readonly List<string> meterSources = new List<string>();
        private readonly List<Func<Instrument, MetricStreamConfiguration>> viewConfigs = new List<Func<Instrument, MetricStreamConfiguration>>();
        private ResourceBuilder resourceBuilder = ResourceBuilder.CreateDefault();

        protected MeterProviderBuilderBase()
        {
        }

        internal List<MetricReader> MetricReaders { get; } = new List<MetricReader>();

        /// <inheritdoc />
        public override MeterProviderBuilder AddInstrumentation<TInstrumentation>(Func<TInstrumentation> instrumentationFactory)
        {
            if (instrumentationFactory == null)
            {
                throw new ArgumentNullException(nameof(instrumentationFactory));
            }

            this.instrumentationFactories.Add(
                new InstrumentationFactory(
                    typeof(TInstrumentation).Name,
                    "semver:" + typeof(TInstrumentation).Assembly.GetName().Version,
                    instrumentationFactory));

            return this;
        }

        /// <inheritdoc />
        public override MeterProviderBuilder AddSource(params string[] names)
        {
            if (names == null)
            {
                throw new ArgumentNullException(nameof(names));
            }

            foreach (var name in names)
            {
                if (string.IsNullOrWhiteSpace(name))
                {
                    throw new ArgumentException($"{nameof(names)} contains null or whitespace string.");
                }

                this.meterSources.Add(name);
            }

            return this;
        }

        internal MeterProviderBuilder AddReader(MetricReader reader)
        {
            if (this.MetricReaders.Count >= 1)
            {
                throw new InvalidOperationException("Only one Metricreader is allowed.");
            }

            this.MetricReaders.Add(reader);
            return this;
        }

        internal MeterProviderBuilder AddView(string instrumentName, string name)
        {
            // TODO: Actually implement view.
            return this;
        }

        internal MeterProviderBuilder AddView(string instrumentName, MetricStreamConfiguration aggregationConfig)
        {
            // TODO: Actually implement view.
            return this;
        }

        internal MeterProviderBuilder AddView(Func<Instrument, MetricStreamConfiguration> viewConfig)
        {
            // TODO: Actually implement view.
            return this;
        }

        internal MeterProviderBuilder SetResourceBuilder(ResourceBuilder resourceBuilder)
        {
            this.resourceBuilder = resourceBuilder ?? throw new ArgumentNullException(nameof(resourceBuilder));
            return this;
        }

        /// <summary>
        /// Run the configured actions to initialize the <see cref="MeterProvider"/>.
        /// </summary>
        /// <returns><see cref="MeterProvider"/>.</returns>
        protected MeterProvider Build()
        {
            return new MeterProviderSdk(
                this.resourceBuilder.Build(),
                this.meterSources,
                this.instrumentationFactories,
                this.viewConfigs,
                this.MetricReaders.ToArray());
        }

        internal readonly struct InstrumentationFactory
        {
            public readonly string Name;
            public readonly string Version;
            public readonly Func<object> Factory;

            internal InstrumentationFactory(string name, string version, Func<object> factory)
            {
                this.Name = name;
                this.Version = version;
                this.Factory = factory;
            }
        }
    }
}