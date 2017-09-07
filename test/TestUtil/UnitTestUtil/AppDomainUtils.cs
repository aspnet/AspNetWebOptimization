// Copyright (c) Microsoft Corporation, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.IO;
using System.Reflection;
using System.Web.Compilation;
using System.Web.Hosting;

namespace Microsoft.Web.UnitTest {
    public static class AppDomainUtils {
        // Allow a test to modify static fields in an independent appdomain so that
        // other tests will not be affected.
        public static void RunInSeparateAppDomain(Action action) {
            RunInSeparateAppDomain(new AppDomainSetup(), action);
        }

        public static void RunInSeparateAppDomain(AppDomainSetup setup, Action action) {
            var dir = Path.GetDirectoryName(typeof(AppDomainUtils).Assembly.CodeBase).Replace("file:\\", "");
            setup.PrivateBinPath = dir;
            setup.ApplicationBase = dir;
            var appdomain = AppDomain.CreateDomain(new Guid().ToString(), null, setup);
            AppDomainHelper helper = appdomain.CreateInstanceAndUnwrap(typeof(AppDomainUtils).Assembly.FullName, typeof(AppDomainHelper).FullName) as AppDomainHelper;
            helper.Run(action);
        }

        public class AppDomainHelper : MarshalByRefObject {
            public void Run(Action action) {
                action();
            }
        }

        public static void SetPreAppStartStage() {
            var stage = typeof(BuildManager).GetProperty("PreStartInitStage", BindingFlags.Static | BindingFlags.NonPublic);
            var value = ((FieldInfo)typeof(BuildManager).Assembly.GetType("System.Web.Compilation.PreStartInitStage").GetMember("DuringPreStartInit")[0]).GetValue(null);
            stage.SetValue(null, value, new object[] { });
            SetAppData();
            var env = new HostingEnvironment();
        }

        public static void SetAppData() {
            var appdomain = AppDomain.CurrentDomain;
            // Set some dummy values to make the appdomain seem more like a ASP.NET hosted one
            appdomain.SetData(".appDomain", "*");
            appdomain.SetData(".appId", "appId");
            appdomain.SetData(".appPath", "appPath");
            appdomain.SetData(".appVPath", "/WebSite1");
            appdomain.SetData(".domainId", "1");
        }
    }
}
