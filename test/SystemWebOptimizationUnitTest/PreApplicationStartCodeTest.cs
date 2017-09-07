// Copyright (c) Microsoft Corporation, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Web.UnitTest;

namespace System.Web.Optimization.Test {
    [TestClass]
    public class PreApplicationStartCodeTest {

        [TestMethod]
        public void StartCanRunTwice() {
            AppDomainUtils.RunInSeparateAppDomain(() => {
                AppDomainUtils.SetPreAppStartStage();
                PreApplicationStartCode.Start();
                // Call a second time to ensure multiple calls do not cause issues
                PreApplicationStartCode.Start();
            });
        }


        [TestMethod]
        public void TestPreAppStartClass() {
            PreAppStartTestHelper.TestPreAppStartClass(typeof(PreApplicationStartCode));
        }
    }
}
