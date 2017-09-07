// Copyright (c) Microsoft Corporation, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace System.Web.Optimization.Test {

    [TestClass]
    public class BundleResolverTest {
        public TestContext TestContext { get; set; }

        [TestMethod]
        public void EnsureNonVirtualPathsDoNotThrowTest() {
            BundleCollection col = new BundleCollection();
            BundleResolver resolver = new BundleResolver(col);
            Assert.IsFalse(resolver.IsBundleVirtualPath("missingTilde"));
            Assert.IsNull(resolver.GetBundleContents("missingTilde"));
            Assert.IsNull(resolver.GetBundleUrl("missingTilde"));
        }

        [TestMethod]
        public void EnsureNullVirtualPathsDoNotThrowTest() {
            BundleCollection col = new BundleCollection();
            BundleResolver resolver = new BundleResolver(col);
            Assert.IsFalse(resolver.IsBundleVirtualPath(null));
            Assert.IsNull(resolver.GetBundleContents(null));
            Assert.IsNull(resolver.GetBundleUrl(null));
        }

        [TestMethod]
        public void EnsureEmptyVirtualPathsDoNotThrowTest() {
            BundleCollection col = new BundleCollection();
            BundleResolver resolver = new BundleResolver(col);
            Assert.IsFalse(resolver.IsBundleVirtualPath(String.Empty));
            Assert.IsNull(resolver.GetBundleContents(String.Empty));
            Assert.IsNull(resolver.GetBundleUrl(String.Empty));
        }

        [TestMethod]
        public void NonBundleValidUrlTest() {
            BundleCollection col = new BundleCollection();
            col.Add(new Bundle("~/js"));
            BundleResolver resolver = new BundleResolver(col);
            Assert.IsFalse(resolver.IsBundleVirtualPath("~/nope"));
            Assert.IsNull(resolver.GetBundleContents("~/nope"));
            Assert.IsNull(resolver.GetBundleUrl("~/nope"));
        }

        [TestMethod]
        public void ValidBundleUrlTest() {
            BundleCollection col = new BundleCollection();
            col.Add(new Bundle("~/js"));
            BundleResolver resolver = new BundleResolver(col);
            Assert.IsTrue(resolver.IsBundleVirtualPath("~/js"));
        }

        [TestMethod]
        public void DynamicBundleGetBundleContentsTest() {
            BundleCollection bundles = new BundleCollection();
            bundles.Add(new DynamicFolderBundle("js", "*.js", new JsMinify()));
            BundleTable.VirtualPathProvider = new FileVirtualPathProvider(TestContext.DeploymentDirectory);
            BundleResolver resolver = new BundleResolver(bundles, new Moq.Mock<HttpContextBase>().Object);
            string output = "";
            foreach (var s in resolver.GetBundleContents("~/scripts/js")) {
                output += s + "|";
            }
            Assert.AreEqual("~/scripts/first.js|~/scripts/second.js|", output);
        }

        [TestMethod]
        public void ScriptBundleGetBundleContextTest() {
            try {
                BundleCollection bundles = new BundleCollection();
                bundles.Add(new ScriptBundle("~/js").Include("~/scripts/first.js", "~/scripts/second.js"));
                BundleTable.VirtualPathProvider = new FileVirtualPathProvider(TestContext.DeploymentDirectory);
                BundleResolver resolver = new BundleResolver(bundles, new Moq.Mock<HttpContextBase>().Object);
                string output = "";
                foreach (var s in resolver.GetBundleContents("~/js")) {
                    output += s + "|";
                }
                Assert.AreEqual("~/scripts/first.js|~/scripts/second.js|", output);
            }
            finally {
                BundleTable.VirtualPathProvider = null;
            }
        }

    }
}
