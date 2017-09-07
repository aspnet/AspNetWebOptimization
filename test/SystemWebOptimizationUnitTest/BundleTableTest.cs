// Copyright (c) Microsoft Corporation, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace System.Web.Optimization.Test {

    [TestClass]
    public class BundleTableTest {
        [TestMethod]
        public void EnableOptimizationsSetterTest() {
            BundleTable.EnableOptimizations = true;
            Assert.IsTrue(BundleTable.EnableOptimizations);
            BundleTable.EnableOptimizations = false;
            Assert.IsFalse(BundleTable.EnableOptimizations);
        }
    }
}
