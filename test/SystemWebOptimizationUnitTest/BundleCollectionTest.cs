// Copyright (c) Microsoft Corporation, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Web.Optimization;
using System.Web.TestUtil;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace System.Web.Optimization.Test {

    [TestClass]
    public class BundleCollectionTest {
        [TestMethod]
        public void BundleApplicationRelativeValidationTest() {
            string badPath = "foo";
            string virtualPathErr = "The URL 'foo' is not valid. Only application relative URLs (~/url) are allowed.\r\nParameter name: {0}";
            ExceptionHelper.ExpectArgumentException(
                delegate {
                    new Bundle(badPath);
                }, String.Format(virtualPathErr, "virtualPath"));

            Bundle b = new Bundle("~/hi");
            ExceptionHelper.ExpectArgumentException(
                delegate {
                    b.IncludeDirectory(badPath, "*.js", true);
                }, String.Format(virtualPathErr, "directoryVirtualPath"));
            ExceptionHelper.ExpectArgumentException(
                delegate {
                    b.Include(badPath);
                }, String.Format(virtualPathErr, "virtualPath"));
        }

        [TestMethod]
        public void ResolveBundleUrlMustBeAppRelative() {
            ExceptionHelper.ExpectArgumentException(
                delegate {
                    BundleTable.Bundles.ResolveBundleUrl("foo");
                }, "The URL 'foo' is not valid. Only application relative URLs (~/url) are allowed.\r\nParameter name: bundleVirtualPath");
        }

        [TestMethod]
        public void GetBundleForUrlMustBeAppRelative() {
            ExceptionHelper.ExpectArgumentException(
                delegate {
                    BundleTable.Bundles.GetBundleFor("foo");
                }, "The URL 'foo' is not valid. Only application relative URLs (~/url) are allowed.\r\nParameter name: bundleVirtualPath");
        }

        [TestMethod]
        public void DynamicFolderBundle_InvalidPathsTest() {
            string error = "The Path for DynamicFolderBundles cannot start with a '/' or '~' character and it cannot contain a '?' character.\r\nParameter name: pathSuffix";
            ExceptionHelper.ExpectArgumentException(
                delegate {
                    new DynamicFolderBundle("js?", "*.js");
                }, error);
            ExceptionHelper.ExpectArgumentException(
                delegate {
                    new DynamicFolderBundle("/js", "*.js");
                }, error);
            ExceptionHelper.ExpectArgumentException(
                delegate {
                    new DynamicFolderBundle("~js", "*.js");
                }, error);
        }

        [TestMethod]
        public void BasicAddRemoveClearTest() {
            BundleCollection col = new BundleCollection();

            DynamicFolderBundle db1 = new DynamicFolderBundle("foo", "*.js");
            col.Add(db1);
            Assert.AreEqual(1, col.Count);

            Bundle sb1 = new Bundle("~/static");
            col.Add(sb1);
            Assert.AreEqual(2, col.Count);

            col.Remove(db1);
            Assert.AreEqual(1, col.Count);

            col.Clear();
            Assert.AreEqual(0, col.Count);
        }

        [TestMethod]
        public void NullOrEmptyThrowsTests() {
            string pathErr = "The value assigned to property 'Path' cannot be null or empty.\r\nParameter name: Path";

            ExceptionHelper.ExpectArgumentException(
                delegate {
                    new DynamicFolderBundle(null, "*.js");
                }, pathErr);

            ExceptionHelper.ExpectArgumentException(
                delegate {
                    new DynamicFolderBundle("", "*.js");
                }, pathErr);

            ExceptionHelper.ExpectArgumentException(
                delegate {
                    new Bundle(null);
                }, pathErr);

            ExceptionHelper.ExpectArgumentException(
                delegate {
                    new Bundle("");
                }, pathErr);
        }

        [TestMethod]
        public void DynamicAndStaticPathDontConflictTest() {
            BundleCollection col = new BundleCollection();
            DynamicFolderBundle db1 = new DynamicFolderBundle("foo", "*.js");
            col.Add(db1);
            Assert.AreEqual(1, col.Count);
            Bundle sb1 = new Bundle("~/foo");
            col.Add(sb1);
            Assert.AreEqual(2, col.Count);
        }

        [TestMethod]
        public void SameBundlePathReplacesTests() {
            BundleCollection col = new BundleCollection();
            DynamicFolderBundle db1 = new DynamicFolderBundle("foo", "*.js", JsMinify.Instance);
            col.Add(db1);
            Assert.AreEqual(1, col.Count);
            DynamicFolderBundle db2 = new DynamicFolderBundle("foo", "*.css", CssMinify.Instance);
            col.Add(db2);
            Assert.AreEqual(1, col.Count);
            Assert.AreEqual(CssMinify.Instance, col.GetRegisteredBundles()[0].Transforms[0]);
        }

        [TestMethod]
        public void SameBundlePathWithDifferentCaseReplacesTests() {
            BundleCollection col = new BundleCollection();
            DynamicFolderBundle db1 = new DynamicFolderBundle("hAo", "*.js", JsMinify.Instance);
            col.Add(db1);
            Assert.AreEqual(1, col.Count);
            DynamicFolderBundle db2 = new DynamicFolderBundle("hao", "*.css", CssMinify.Instance);
            col.Add(db2);
            Assert.AreEqual(1, col.Count);
            Assert.AreEqual(CssMinify.Instance, col.GetRegisteredBundles()[0].Transforms[0]);
        }

        [TestMethod]
        public void AddDefaultIgnorePatternsTest() {
            IgnoreList list = new IgnoreList();
            BundleCollection.AddDefaultIgnorePatterns(list);
            ValidateDefaultIgnoreList(list, optimizationsEnabled: true);
            ValidateDefaultIgnoreList(list, optimizationsEnabled: false);
        }

        [TestMethod]
        public void AddDefaultFileExtensionReplacementsTest() {
            FileExtensionReplacementList list = new FileExtensionReplacementList();
            BundleCollection.AddDefaultFileExtensionReplacements(list);
            ValidateDefaultFileExtensionReplacementList(list);
        }

        [TestMethod]
        public void AddDefaultIgnorePatternsThrowsWithNullTest() {
            ExceptionHelper.ExpectArgumentNullException(delegate { BundleCollection.AddDefaultIgnorePatterns(null); }, "ignoreList");
        }

        [TestMethod]
        public void AddDefaultFileExtensionReplacementsThrowsWithNullTest() {
            ExceptionHelper.ExpectArgumentNullException(delegate { BundleCollection.AddDefaultFileExtensionReplacements(null); }, "list");
        }

        [TestMethod]
        public void AddDefaultFileOrderingsThrowsWithNullTest() {
            ExceptionHelper.ExpectArgumentNullException(delegate { BundleCollection.AddDefaultFileOrderings(null); }, "list");
        }

        [TestMethod]
        public void AddDefaultFileOrderings() {
            IList<BundleFileSetOrdering> list = new List<BundleFileSetOrdering>();
            BundleCollection.AddDefaultFileOrderings(list);
            ValidateDefaultOrderings(list);
        }

        [TestMethod]
        public void DefaultBehaviorsForBundleCollectionWhenOptimizationEnabledTest() {
            BundleCollection col = new BundleCollection();
            Assert.AreEqual(0, col.Count);

            // Make sure ignore list is on by default only for wildcards
            ValidateDefaultIgnoreList(col.DirectoryFilter, optimizationsEnabled: true);

            // Make sure Default file extension replacement is on
            ValidateDefaultFileExtensionReplacementList(col.FileExtensionReplacementList);

            // Make sure we have the correct default orderings
            ValidateDefaultOrderings(col.FileSetOrderList);
        }


        [TestMethod]
        public void DefaultBehaviorsForBundleCollectionWhenOptimizationNotEnabledTest() {
            BundleCollection col = new BundleCollection();
            Assert.AreEqual(0, col.Count);

            // Make sure ignore list is on by default
            ValidateDefaultIgnoreList(col.DirectoryFilter, optimizationsEnabled: false);

            // Make sure Default file extension replacement is on
            ValidateDefaultFileExtensionReplacementList(col.FileExtensionReplacementList);

            // Make sure we have the correct default orderings
            ValidateDefaultOrderings(col.FileSetOrderList);
        }

        private static void ValidateDefaultIgnoreList(IgnoreList list, bool optimizationsEnabled) {
            BundleContext context = new BundleContext() { EnableOptimizations = optimizationsEnabled };

            Assert.IsTrue(list.ShouldIgnore(context, "jquery.1.4.1-vsdoc.js"));
            Assert.IsTrue(list.ShouldIgnore(context, "test-vsdoc.js"));
            Assert.IsTrue(list.ShouldIgnore(context, "test.intellisense.js"));
            Assert.IsTrue(list.ShouldIgnore(context, "jquery.2.0.0.min.map"));
            if (optimizationsEnabled) {
                Assert.IsTrue(list.ShouldIgnore(context, "MicrosoftAjax.debug.js"));
                Assert.IsFalse(list.ShouldIgnore(context, "jquery.min.css"));
                Assert.IsFalse(list.ShouldIgnore(context, "jquery.min.js"));
            }
            else {
                Assert.IsFalse(list.ShouldIgnore(context, "MicrosoftAjax.debug.js"));
                Assert.IsTrue(list.ShouldIgnore(context, "jquery.min.css"));
                Assert.IsTrue(list.ShouldIgnore(context, "jquery.min.js"));
            }
        }

        private static void ValidateDefaultFileExtensionReplacementList(FileExtensionReplacementList list) {
            Assert.IsTrue(list.Count == 2);
            Assert.IsTrue(list[0].Extension == "min");
            Assert.IsTrue(list[0].Mode == OptimizationMode.WhenEnabled);
            Assert.IsTrue(list[1].Extension == "debug");
            Assert.IsTrue(list[1].Mode == OptimizationMode.WhenDisabled);
        }

        private static void ValidateDefaultOrderings(IList<BundleFileSetOrdering> list) {
            Assert.IsTrue(list.Count == 7);
            Assert.AreEqual("css", list[0].Name);
            Assert.AreEqual("jquery", list[1].Name);
            Assert.AreEqual("modernizr", list[2].Name);
            Assert.AreEqual("dojo", list[3].Name);
            Assert.AreEqual("moo", list[4].Name);
            Assert.AreEqual("prototype", list[5].Name);
            Assert.AreEqual("ext", list[6].Name);
        }

        [TestMethod]
        public void ResetAllTest() {
            BundleCollection col = new BundleCollection();
            col.ResetAll();
            Assert.IsFalse(col.DirectoryFilter.ShouldIgnore(new BundleContext(), "jquery.1.4.1-vsdoc.js"));
            Assert.IsTrue(col.FileExtensionReplacementList.Count == 0);
            Assert.IsTrue(col.FileSetOrderList.Count == 0);
            Assert.AreEqual(0, col.Count);
        }

        [TestMethod]
        public void UseCdnWhenSpecifiedTest() {
            BundleCollection col = new BundleCollection();
            col.UseCdn = true;
            Bundle b = new Bundle("~/bundles/a");
            string cdnPath = "cdnPath";
            b.CdnPath = cdnPath;
            col.Add(b);
            Assert.AreEqual(cdnPath, col.ResolveBundleUrl(b.Path));
        }
    }
}
