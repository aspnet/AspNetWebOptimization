// Copyright (c) Microsoft Corporation, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Web.TestUtil;
using SystemWebOptimizationUnitTest.Util;

namespace System.Web.Optimization.Test {

    [TestClass]
    public class DynamicFolderBundleTest {
        [TestMethod]
        public void DynamicFolderBundleSearchPatternBlocksStarTest() {
            ExceptionHelper.ExpectArgumentException(
                delegate { new DynamicFolderBundle("duh", "*"); },
                "Pure wildcard search patterns '*' and '*.*' are not supported.\r\nParameter name: value");
        }

        [TestMethod]
        public void DynamicFolderBundleSearchPatternBlocksStarDotStarTest() {
            ExceptionHelper.ExpectArgumentException(
                delegate { new DynamicFolderBundle("duh", "*.*"); },
                "Pure wildcard search patterns '*' and '*.*' are not supported.\r\nParameter name: value");
        }

        [TestMethod]
        public void DynamicFolderBundleDoNotSupportSettingCdnPathTest() {
            ExceptionHelper.ExpectException<NotSupportedException>(delegate { new DynamicFolderBundle("duh", "yo").CdnPath = "die"; });
        }

        [TestMethod]
        public void DynamicFolderBundleConstructorsDefaultToNoTransformTest() {
            DynamicFolderBundle bundle = new DynamicFolderBundle("yo", "*.yo");
            Assert.IsTrue(bundle.Transforms.Count == 0);
            DynamicFolderBundle bundle2 = new DynamicFolderBundle("yo", "*.yo", true);
            Assert.IsTrue(bundle2.Transforms.Count == 0);
        }

        [TestMethod]
        public void DynamicBundleWithCustomVPPNestedAllJsTest() {
            //Setup the vpp to contain the files/directories
            TestVirtualPathProvider vpp = new TestVirtualPathProvider();
            var directory = new TestVirtualPathProvider.TestVirtualDirectory("/dir/");
            directory.DirectoryFiles.Add(new TestVirtualPathProvider.TestVirtualFile("/dir/1.js", "1"));
            directory.DirectoryFiles.Add(new TestVirtualPathProvider.TestVirtualFile("/dir/2.js", "2"));
            vpp.AddDirectory(directory);
            var sub1 = new TestVirtualPathProvider.TestVirtualDirectory("/dir/sub1/");
            sub1.DirectoryFiles.Add(new TestVirtualPathProvider.TestVirtualFile("/dir/sub1/a.js", "a"));
            sub1.DirectoryFiles.Add(new TestVirtualPathProvider.TestVirtualFile("/dir/sub1/b.js", "b"));
            vpp.AddDirectory(sub1);
            directory.SubDirectories.Add(sub1);
            var sub2 = new TestVirtualPathProvider.TestVirtualDirectory("/dir/sub2/");
            sub2.DirectoryFiles.Add(new TestVirtualPathProvider.TestVirtualFile("/dir/sub2/c.js", "c"));
            sub2.DirectoryFiles.Add(new TestVirtualPathProvider.TestVirtualFile("/dir/sub2/d.js", "d"));
            vpp.AddDirectory(sub2);
            directory.SubDirectories.Add(sub2);
            var subSub1 = new TestVirtualPathProvider.TestVirtualDirectory("/dir/sub1/sub/");
            subSub1.DirectoryFiles.Add(new TestVirtualPathProvider.TestVirtualFile("/dir/sub1/sub/aa.js", "aa"));
            subSub1.DirectoryFiles.Add(new TestVirtualPathProvider.TestVirtualFile("/dir/sub1/sub/bb.js", "bb"));
            vpp.AddDirectory(subSub1);
            sub1.SubDirectories.Add(subSub1);

            // Setup the bundle
            Bundle bundle = new DynamicFolderBundle("js", "*.js", searchSubdirectories: true);
            bundle.ConcatenationToken = " ";
            bundle.Items.VirtualPathProvider = vpp;

            // Verify the bundle repsonse
            BundleContext context = BundleTest.SetupContext(bundle, vpp);
            context.BundleVirtualPath = "~/dir/js";
            Assert.AreEqual(@"1 2 a b aa bb c d ", bundle.GetBundleResponse(context).Content);

            context.BundleVirtualPath = "~/dir/sub1/js";
            Assert.AreEqual(@"a b aa bb ", bundle.GetBundleResponse(context).Content);

            context.BundleVirtualPath = "~/dir/sub2/js";
            Assert.AreEqual(@"c d ", bundle.GetBundleResponse(context).Content);
        }

        [TestMethod]
        public void DynamicBundleWithCustomVPPSearchSubDirOffTest() {
            //Setup the vpp to contain the files/directories
            TestVirtualPathProvider vpp = new TestVirtualPathProvider();
            var directory = new TestVirtualPathProvider.TestVirtualDirectory("/dir/");
            directory.DirectoryFiles.Add(new TestVirtualPathProvider.TestVirtualFile("/dir/1.js", "1"));
            directory.DirectoryFiles.Add(new TestVirtualPathProvider.TestVirtualFile("/dir/2.js", "2"));
            vpp.AddDirectory(directory);
            var sub1 = new TestVirtualPathProvider.TestVirtualDirectory("/dir/sub1/");
            sub1.DirectoryFiles.Add(new TestVirtualPathProvider.TestVirtualFile("/dir/sub1/a.js", "a"));
            sub1.DirectoryFiles.Add(new TestVirtualPathProvider.TestVirtualFile("/dir/sub1/b.js", "b"));
            vpp.AddDirectory(sub1);
            directory.SubDirectories.Add(sub1);
            var sub2 = new TestVirtualPathProvider.TestVirtualDirectory("/dir/sub2/");
            sub2.DirectoryFiles.Add(new TestVirtualPathProvider.TestVirtualFile("/dir/sub2/c.js", "c"));
            sub2.DirectoryFiles.Add(new TestVirtualPathProvider.TestVirtualFile("/dir/sub2/d.js", "d"));
            vpp.AddDirectory(sub2);
            directory.SubDirectories.Add(sub2);
            var subSub1 = new TestVirtualPathProvider.TestVirtualDirectory("/dir/sub1/sub/");
            subSub1.DirectoryFiles.Add(new TestVirtualPathProvider.TestVirtualFile("/dir/sub1/sub/aa.js", "aa"));
            subSub1.DirectoryFiles.Add(new TestVirtualPathProvider.TestVirtualFile("/dir/sub1/sub/bb.js", "bb"));
            vpp.AddDirectory(subSub1);
            sub1.SubDirectories.Add(subSub1);

            // Setup the bundle
            Bundle bundle = new DynamicFolderBundle("js", "*.js");
            bundle.ConcatenationToken = " ";
            bundle.Items.VirtualPathProvider = vpp;

            // Verify the bundle repsonse
            BundleContext context = BundleTest.SetupContext(bundle, vpp);
            context.BundleVirtualPath = "~/dir/js";
            Assert.AreEqual(@"1 2 ", bundle.GetBundleResponse(context).Content);

            context.BundleVirtualPath = "~/dir/sub1/js";
            Assert.AreEqual(@"a b ", bundle.GetBundleResponse(context).Content);

            context.BundleVirtualPath = "~/dir/sub2/js";
            Assert.AreEqual(@"c d ", bundle.GetBundleResponse(context).Content);
        }

    }
}
