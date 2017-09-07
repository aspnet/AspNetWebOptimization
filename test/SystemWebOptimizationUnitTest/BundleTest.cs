// Copyright (c) Microsoft Corporation, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.Web.Hosting;
using System.Web.TestUtil;
using SystemWebOptimizationUnitTest.Util;

namespace System.Web.Optimization.Test {

    [TestClass]
    public class BundleTest {
        [TestMethod]
        public void BundlePublicArgumentNullChecks() {
            Bundle bundle = new Bundle("~/whatever");
            ExceptionHelper.ExpectArgumentNullException(() => bundle.EnumerateFiles(null), "context");
            ExceptionHelper.ExpectArgumentNullException(() => bundle.CacheLookup(null), "context");
            ExceptionHelper.ExpectArgumentNullException(() => bundle.UpdateCache(null, null), "context");
            ExceptionHelper.ExpectArgumentNullException(() => bundle.UpdateCache(new BundleContext(), null), "response");
            ExceptionHelper.ExpectArgumentNullException(() => bundle.GenerateBundleResponse(null), "context");
            ExceptionHelper.ExpectArgumentNullException(() => bundle.GetCacheKey(null), "context");
            ExceptionHelper.ExpectArgumentNullException(() => bundle.ApplyTransforms(null, "", null), "context");
        }

        
        [TestMethod]
        public void BundleConstructorsDefaultToNoTransformTest() {
            Bundle bundle = new Bundle("~/whatever");
            Assert.AreEqual(0, bundle.Transforms.Count);
        }

        /// <summary>
        /// Setups a test bundle that turns off existence checks and does not map paths
        /// </summary>
        /// <param name="virtualPath"></param>
        /// <returns></returns>
        private Bundle SetupBundle(string virtualPath) {
            Bundle bundle = new Bundle(virtualPath);
            return bundle;
        }

        [TestMethod]
        public void BundleMapPathDoesNotBlowUpTest() {
            Bundle bundle = SetupBundle("~/nope");
            string[] files = {"~/foo", "~/bar", "~/gobblygook"};
            bundle.Include(files);
            Assert.AreEqual(files.Length, bundle.Items.Count);
            for (int i = 0; i < files.Length; i++) {
                Assert.AreEqual(files[i], bundle.Items[i].VirtualPath);
            }
        }

        [TestMethod]
        public void NoWildcardsInMiddleIncludeTest() {
            ExceptionHelper.ExpectArgumentException(delegate { new Bundle("~/test").Include("~/a*b"); }, String.Format(PatternHelperTest.InvalidPatternError, "a*b", "virtualPath"));
        }

        [TestMethod]
        public void NoExtraLeadingWildcardsIncludeTest() {
            ExceptionHelper.ExpectArgumentException(delegate { new Bundle("~/test").Include("~/**a"); }, String.Format(PatternHelperTest.InvalidPatternError, "**a", "virtualPath"));
        }

        [TestMethod]
        public void NoExtraTrailingWildcardsIncludeTest() {
            ExceptionHelper.ExpectArgumentException(delegate { new Bundle("~/test").Include("~/**a"); }, String.Format(PatternHelperTest.InvalidPatternError, "**a", "virtualPath"));
        }

        [TestMethod]
        public void NoWildcardsAllowedInDirectoryIncludeTest() {
            ExceptionHelper.ExpectArgumentException(delegate { new Bundle("~/test").Include("~/a*/foo.js"); }, String.Format(PatternHelperTest.InvalidPatternError, "~/a*/foo.js", "virtualPath"));
        }

        [TestMethod]
        public void ChainTransformsTest() {
            Bundle bundle = new Bundle("~/ignored", new AppendTransform("H"), new AppendTransform("a"), new AppendTransform("o"));
            BundleContext context = new BundleContext();
            List<BundleFile> files = new List<BundleFile>();
            BundleResponse response = bundle.ApplyTransforms(context, "", files);
            Assert.AreEqual("Hao", response.Content);
        }

        [TestMethod]
        public void IncludeWildCardSearchPatternThrowsTest() {
            Bundle b = new Bundle("~/ignored");
            string error = "Pure wildcard search patterns '*' and '*.*' are not supported.\r\nParameter name: searchPattern";
            ExceptionHelper.ExpectArgumentException(
                delegate {
                    b.IncludeDirectory("~/foo", "*");
                }, error);
            ExceptionHelper.ExpectArgumentException(
                delegate {
                    b.IncludeDirectory("~/foo", "*.*");
                }, error);
        }

        [TestMethod]
        public void AddDirectoryBlocksStarTest() {
            Assert.IsTrue(ExceptionUtil.IsPureWildcardSearchPattern("*"));
            Assert.IsTrue(ExceptionUtil.IsPureWildcardSearchPattern("   *"));
            Assert.IsTrue(ExceptionUtil.IsPureWildcardSearchPattern("*   "));
            Assert.IsTrue(ExceptionUtil.IsPureWildcardSearchPattern("   *    "));
        }

        [TestMethod]
        public void AddDirectoryDoesNotBlockOtherWildcardsTest() {
            Assert.IsFalse(ExceptionUtil.IsPureWildcardSearchPattern("web*"));
            Assert.IsFalse(ExceptionUtil.IsPureWildcardSearchPattern("*.config"));
            Assert.IsFalse(ExceptionUtil.IsPureWildcardSearchPattern("*.*.*"));
            Assert.IsFalse(ExceptionUtil.IsPureWildcardSearchPattern("*.*h"));
        }

        [TestMethod]
        public void AddDirectoryBlocksStarDotStarTest() {
            Assert.IsTrue(ExceptionUtil.IsPureWildcardSearchPattern("*.*"));
            Assert.IsTrue(ExceptionUtil.IsPureWildcardSearchPattern("   *.*"));
            Assert.IsTrue(ExceptionUtil.IsPureWildcardSearchPattern("*.*   "));
            Assert.IsTrue(ExceptionUtil.IsPureWildcardSearchPattern("   *.*    "));
        }

        [TestMethod]
        public void CustomVPPWithOneFileNoTransformTest() {
            //Setup the vpp to contain the files/directories
            TestVirtualPathProvider vpp = new TestVirtualPathProvider();
            var file = new TestVirtualPathProvider.TestVirtualFile("/dir/file", "test");
            vpp.AddFile(file);

            // Setup the bundle
            Bundle bundle = new Bundle("~/bundles/test");
            bundle.Items.VirtualPathProvider = vpp;
            bundle.Include("~/dir/file");

            // Verify the bundle repsonse
            BundleContext context = SetupContext(bundle, vpp);
            BundleResponse response = bundle.GetBundleResponse(context);
            Assert.AreEqual("test\r\n", response.Content);
        }

        [TestMethod]
        public void BundleCustomVPPIncludeDirectoryNestedAllJsTest() {
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
            Bundle bundle = new Bundle("~/bundles/test");
            bundle.ConcatenationToken = " ";
            bundle.Items.VirtualPathProvider = vpp;
            bundle.IncludeDirectory("~/dir/", "*.js", searchSubdirectories: true);

            // Verify the bundle repsonse
            BundleContext context = SetupContext(bundle, vpp);
            BundleResponse response = bundle.GetBundleResponse(context);
            Assert.AreEqual(@"1 2 a b aa bb c d ", response.Content);
        }

        [TestMethod]
        public void ScriptBundleCustomVPPIncludeAllJsTest() {
            //Setup the vpp to contain the files/directories
            TestVirtualPathProvider vpp = new TestVirtualPathProvider();
            var directory = new TestVirtualPathProvider.TestVirtualDirectory("/dir/");
            directory.DirectoryFiles.Add(new TestVirtualPathProvider.TestVirtualFile("/dir/1.js", "alert('1')"));
            directory.DirectoryFiles.Add(new TestVirtualPathProvider.TestVirtualFile("/dir/2.js", "alert('2')"));
            vpp.AddDirectory(directory);

            // Setup the bundle
            ScriptBundle bundle = new ScriptBundle("~/bundles/test");
            bundle.Items.VirtualPathProvider = vpp;
            bundle.Include("~/dir/*.js");

            // Verify the bundle repsonse
            BundleContext context = SetupContext(bundle, vpp);
            BundleResponse response = bundle.GetBundleResponse(context);
            Assert.AreEqual(@"alert(""1"");alert(""2"")", response.Content);
        }

        [TestMethod]
        public void IncludeWithVersionInDirectoryThrowsTest() {
            //Setup the vpp to contain the files/directories
            TestVirtualPathProvider vpp = new TestVirtualPathProvider();
            var directory = new TestVirtualPathProvider.TestVirtualDirectory("/jquery1.0.0-pre/");
            directory.DirectoryFiles.Add(new TestVirtualPathProvider.TestVirtualFile("/jquery1.0.0-pre/1.js", "alert('1')"));
            directory.DirectoryFiles.Add(new TestVirtualPathProvider.TestVirtualFile("/jquery1.0.0-pre/2.js", "alert('2')"));
            vpp.AddDirectory(directory);

            // Setup the bundle
            ScriptBundle bundle = new ScriptBundle("~/bundles/test");
            bundle.Items.VirtualPathProvider = vpp;
            string error = "Directory does not exist.\r\nParameter name: directoryVirtualPath";
            ExceptionHelper.ExpectArgumentException(
                delegate {
                    bundle.Include("~/jquery{version}/*.js");
                }, error);

        }


        [TestMethod]
        public void StyleBundleCustomVPPIncludeVersionSelectsTest() {
            //Setup the vpp to contain the files/directories
            TestVirtualPathProvider vpp = new TestVirtualPathProvider();
            var directory = new TestVirtualPathProvider.TestVirtualDirectory("/dir/");
            directory.DirectoryFiles.Add(new TestVirtualPathProvider.TestVirtualFile("/dir/style1.0.css", "correct"));
            directory.DirectoryFiles.Add(new TestVirtualPathProvider.TestVirtualFile("/dir/style.css", "wrong"));
            vpp.AddDirectory(directory);

            // Setup the bundle
            ScriptBundle bundle = new ScriptBundle("~/bundles/test");
            bundle.Items.VirtualPathProvider = vpp;
            bundle.Include("~/dir/style{version}.css");

            // Verify the bundle repsonse
            BundleContext context = SetupContext(bundle, vpp);
            BundleResponse response = bundle.GetBundleResponse(context);
            Assert.AreEqual(@"correct", response.Content);
        }

        [TestMethod]
        public void MinJsPreferredWithOptimizationsEnabledTest() {
            //Setup the vpp to contain the files/directories
            TestVirtualPathProvider vpp = new TestVirtualPathProvider();
            var directory = new TestVirtualPathProvider.TestVirtualDirectory("/");
            var jqueryFile = new TestVirtualPathProvider.TestVirtualFile("/jquery.js", "jquery");
            var jqueryMinFile = new TestVirtualPathProvider.TestVirtualFile("/jquery.min.js", "jquery.min");
            directory.DirectoryFiles.Add(jqueryFile);
            directory.DirectoryFiles.Add(jqueryMinFile);
            vpp.AddDirectory(directory);
            vpp.AddFile(jqueryFile);
            vpp.AddFile(jqueryMinFile);

            // Setup the bundle
            ScriptBundle bundle = new ScriptBundle("~/bundles/test");
            bundle.Items.VirtualPathProvider = vpp;
            bundle.Include("~/jquery.js");

            // Verify the bundle repsonse
            BundleContext context = BundleTest.SetupContext(bundle, vpp);
            context.EnableOptimizations = true;
            BundleResponse response = bundle.GetBundleResponse(context);
            Assert.AreEqual(@"jquery.min", response.Content);
        }

        [TestMethod]
        public void MinJsNotPreferredWithOptimizationsDisabledTest() {
            //Setup the vpp to contain the files/directories
            TestVirtualPathProvider vpp = new TestVirtualPathProvider();
            var directory = new TestVirtualPathProvider.TestVirtualDirectory("/");
            var jqueryFile = new TestVirtualPathProvider.TestVirtualFile("/jquery.js", "jquery");
            var jqueryMinFile = new TestVirtualPathProvider.TestVirtualFile("/jquery.min.js", "jquery.min");
            directory.DirectoryFiles.Add(jqueryFile);
            directory.DirectoryFiles.Add(jqueryMinFile);
            vpp.AddDirectory(directory);
            vpp.AddFile(jqueryFile);
            vpp.AddFile(jqueryMinFile);

            // Setup the bundle
            ScriptBundle bundle = new ScriptBundle("~/bundles/test");
            bundle.Items.VirtualPathProvider = vpp;
            bundle.Include("~/jquery.js");

            // Verify the bundle repsonse
            BundleContext context = BundleTest.SetupContext(bundle, vpp);
            context.EnableOptimizations = false;
            BundleResponse response = bundle.GetBundleResponse(context);
            Assert.AreEqual(@"jquery", response.Content);
        }

        [TestMethod]
        public void DebugPreferredWithOptimizationsDisabledTest() {
            //Setup the vpp to contain the files/directories
            TestVirtualPathProvider vpp = new TestVirtualPathProvider();
            var directory = new TestVirtualPathProvider.TestVirtualDirectory("/");
            var files = new TestVirtualPathProvider.TestVirtualFile[] {
                new TestVirtualPathProvider.TestVirtualFile("/jquery.js", "jquery"),
                new TestVirtualPathProvider.TestVirtualFile("/jquery.debug.js", "jquery.debug")
            };
            foreach(var file in files) {
                directory.DirectoryFiles.Add(file);
                vpp.AddFile(file);
            }
            vpp.AddDirectory(directory);

            // Setup the bundle
            ScriptBundle bundle = new ScriptBundle("~/bundles/test");
            bundle.Items.VirtualPathProvider = vpp;
            bundle.Include("~/jquery.js");

            // Verify the bundle repsonse
            BundleContext context = BundleTest.SetupContext(bundle, vpp);
            context.EnableOptimizations = false;
            BundleResponse response = bundle.GetBundleResponse(context);
            Assert.AreEqual(@"jquery.debug", response.Content);
        }

        class FixedResponseBundle : Bundle {
            public static readonly string Content = "fixed";
            public static readonly string ContentType = "txt/rigged";

            public override BundleResponse CacheLookup(BundleContext context) {
                return new BundleResponse(Content, new List<BundleFile>()) { ContentType = ContentType };
            }

            public FixedResponseBundle(string path) : base(path) { }
        }

        [TestMethod]
        public void CacheExtensibilityTest() {
            var bundle = new FixedResponseBundle("~/whatever");
            BundleContext context = SetupContext(bundle, vpp: null);
            BundleResponse response = bundle.GetBundleResponse(context);
            Assert.AreEqual(FixedResponseBundle.Content, response.Content);
            Assert.AreEqual(FixedResponseBundle.ContentType, response.ContentType);
        }

        internal static BundleContext SetupContext(Bundle bundle, VirtualPathProvider vpp) {
            BundleContext context = new BundleContext();
            BundleCollection bundles = new BundleCollection();
            bundles.Add(bundle);
            context.BundleCollection = bundles;
            context.BundleVirtualPath = bundle.Path;
            context.VirtualPathProvider = vpp;
            return context;
        }
    }
}
