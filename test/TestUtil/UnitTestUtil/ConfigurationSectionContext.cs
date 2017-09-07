// Copyright (c) Microsoft Corporation, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace Microsoft.Web.UnitTest {
    using System;
    using System.Configuration;
    using System.Web.Configuration;

    // Base class for temporarily changing configuration settings
    public abstract class ConfigurationSectionContext : IDisposable {

        protected ConfigurationSectionContext(string sectionName, ConfigurationSection section)
        {
            ConfigHelper.OverrideSection(sectionName, section);
        }

        public void Dispose()
        {
            Dispose(true);
        }

        protected virtual void Dispose(bool disposing) {

            if (disposing) {
                ConfigHelper.Revert();
            }
        }
    }
}
