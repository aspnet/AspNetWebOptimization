// Copyright (c) Microsoft Corporation, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Configuration;
using System.Configuration.Internal;
using System.Diagnostics;
using System.Reflection;
using System.Web;
using System.Web.Security;
using System.Web.Configuration;

namespace Microsoft.Web.UnitTest {
    public static class ConfigHelper {
        private static readonly Dictionary<string, ConfigurationSection> _configSections =
            new Dictionary<string, ConfigurationSection>();
        private static readonly MockConfigSystem _mockConfigSystem = new MockConfigSystem();
        private static IInternalConfigSystem _originalConfigSystem;

        public static void OverrideSection(string sectionName, ConfigurationSection section) {
            ResetConfigCache();

            _configSections[sectionName] = section;

            if (_originalConfigSystem == null) {
                // Ensure ConfigurationManager is initialized by reading something from config
                object o = ConfigurationManager.ConnectionStrings;

                // Replace config implementation with mock
                _originalConfigSystem =
                    (IInternalConfigSystem)CommonUtil.GetFieldValue(typeof(ConfigurationManager), "s_configSystem");
                Debug.Assert(_originalConfigSystem != null);

                CommonUtil.SetFieldValue(typeof(ConfigurationManager), "s_configSystem", _mockConfigSystem);
            }
        }

        private static void ResetConfigCache() {
            // Clear cached config data in RuntimeConfig.s_clientRuntimeConfig
            Type runtimeConfigType = Type.GetType("System.Web.Configuration.RuntimeConfig, " +
                typeof(HttpContext).Assembly.FullName);
            CommonUtil.SetFieldValue(runtimeConfigType, "s_clientRuntimeConfig", null);

            // FormsAuth and MachineKeySection cache config settings as well so we reset them here
            CommonUtil.SetFieldValue(typeof(FormsAuthentication), "_Initialized", false);
            CommonUtil.SetFieldValue(typeof(Membership), "s_Initialized", false);
            CommonUtil.SetFieldValue(typeof(MachineKeySection), "s_config", null);
        }

        public static void Revert() {
            if (_originalConfigSystem != null) {
                _configSections.Clear();

                // Revert ConfigurationManager to original state
                CommonUtil.SetFieldValue(typeof(ConfigurationManager), "s_configSystem", _originalConfigSystem);

                ResetConfigCache();

                _originalConfigSystem = null;
            }
        }

        private class MockConfigSystem : IInternalConfigSystem {
            #region IInternalConfigSystem Members
            object IInternalConfigSystem.GetSection(string configKey) {
                if (_configSections.ContainsKey(configKey)) {
                    return _configSections[configKey];
                }
                else {
                    return _originalConfigSystem.GetSection(configKey);
                }
            }

            void IInternalConfigSystem.RefreshConfig(string sectionName) {
                throw new NotImplementedException();
            }

            bool IInternalConfigSystem.SupportsUserConfig {
                get {
                    throw new NotImplementedException();
                }
            }

            #endregion
        }
    }
}
