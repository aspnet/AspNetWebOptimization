// Copyright (c) Microsoft Corporation, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Web.TestUtil.Hosting;

namespace System.Web.Optimization.Test {
    [TestClass]
    public class CustomOutputCacheTest : HostedTestBase {

        [TestMethod]
        public void CustomOutputCacheProviderDoesNotBlowUpTest() {
            var result = ProcessRequest("Test.aspx");
            result.AssertLogMatchesBaseline();
        }

    }
}
