// Copyright (c) Microsoft Corporation, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.IO;
using System.Web.Optimization;
using System.Web.TestUtil;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace System.Web.Optimization.Test {

    [TestClass]
    public class ItemRegistryTest {
        [TestMethod]
        public void IncludeAllowsMixedFilesAndDirectoriesTest() {
            ItemRegistry reg = new ItemRegistry();
            string[] files = { "~/foo", "~/dir/*.js", "~/jquery-{version}.js" };
            reg.Include(files);
            Assert.AreEqual(files.Length, reg.Count);
            Assert.AreEqual("~/foo", reg[0].VirtualPath);
            BundleDirectoryItem reg1 = reg[1] as BundleDirectoryItem;
            Assert.IsNotNull(reg1);
            Assert.IsTrue(reg1.VirtualPath.EndsWith("~/dir/"));
            Assert.AreEqual("*.js", reg1.SearchPattern);
            Assert.AreEqual(PatternType.Suffix, reg1.PatternType);
            BundleDirectoryItem reg2 = reg[2] as BundleDirectoryItem;
            Assert.IsNotNull(reg2);
            Assert.IsTrue(reg2.VirtualPath.EndsWith("~/"));
            Assert.AreEqual("jquery-{version}.js", reg2.SearchPattern);
            Assert.AreEqual(PatternType.Version, reg2.PatternType);
        }
    }
}
