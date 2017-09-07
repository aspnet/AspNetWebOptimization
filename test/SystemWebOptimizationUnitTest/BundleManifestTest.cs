// Copyright (c) Microsoft Corporation, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;
using System.Text;
using System.Web.TestUtil;

namespace System.Web.Optimization.Test {
    [TestClass]
    public class BundleConfigTests {
        [TestMethod]
        public void BundleConfigThrowsIfBundlesHasUnknownElement() {
            ExceptionHelper.ExpectException<InvalidOperationException>(() => BundleManifest.ReadBundleManifest(ToStream(@"<?xml version=""1.0"" ?><bundles version=""1.0""><Blah /></bundles>")));
        }

        [TestMethod]
        public void BundleConfigThrowsWithUnknownAttribute() {
            ExceptionHelper.ExpectException<InvalidOperationException>(() => BundleManifest.ReadBundleManifest(ToStream(@"<?xml version=""1.0"" ?><bundles version=""1.0"" unknown=""oops""></bundles>")));
        }

        [TestMethod]
        public void BundleConfigThrowsWithPascalCasing() {
            ExceptionHelper.ExpectException<InvalidOperationException>(() => BundleManifest.ReadBundleManifest(ToStream(@"<?xml version=""1.0"" ?><bundles><StyleBundle path=""~/bundle/css""><include path=""jQuery.css""></include></StyleBundle></bundles>")));
        }

        [TestMethod]
        public void BundleConfigThrowsWithIncludeWithNoPath() {
            ExceptionHelper.ExpectException<InvalidOperationException>(() => BundleManifest.ReadBundleManifest(ToStream(@"<?xml version=""1.0"" ?><bundles><styleBundle path=""~/my-bundle-path""><include /></styleBundle></bundles>")));
        }

        [TestMethod]
        public void BundleConfigThrowsWithScriptBundleWithNoPath() {
            ExceptionHelper.ExpectException<InvalidOperationException>(() => BundleManifest.ReadBundleManifest(ToStream(@"<?xml version=""1.0"" ?><bundles><styleBundle><include path=""jQuery.css""/></styleBundle></bundles>")));
        }

        [TestMethod]
        public void BundleConfigDoesNotThrowIfVersionNotSpecified() {
            BundleManifest.ReadBundleManifest(ToStream(@"<?xml version=""1.0"" ?><bundles></bundles>"));
        }

        [TestMethod]
        public void BundleConfigAllowsCommentsTags() {
            BundleManifest.ReadBundleManifest(ToStream(@"<?xml version=""1.0"" ?><bundles><!--comment --><styleBundle path=""~/my-bundle-path""><include path=""~/Content/jQuery.css""/><!--include path=""~/Content/jQuery.ui.css""/--></styleBundle></bundles>"));
        }

        [TestMethod]
        public void BundleConfigAllowsSelfClosingIncludeTags() {
            BundleManifest.ReadBundleManifest(ToStream(@"<?xml version=""1.0"" ?><bundles><styleBundle path=""~/my-bundle-path""><include path=""~/Content/jQuery.css""/><include path=""~/Content/jQuery.ui.css""/></styleBundle></bundles>"));
        }

        [TestMethod]
        public void BundleConfigReadsBundleDataCorrectly() {
            // Arrange
            var xml = ToStream(@"<?xml version=""1.0"" ?><bundles version=""1.0""><styleBundle path=""~/my-bundle-path""><include path=""~/Content/jQuery.css""></include><include path=""~/Content/jQuery.ui.css""></include></styleBundle></bundles>");

            // Act
            var result = BundleManifest.ReadBundleManifest(xml);

            // Assert
            Assert.AreEqual(1, result.StyleBundles.Count);
            Assert.AreEqual("~/my-bundle-path", result.StyleBundles[0].Path);
            Assert.AreEqual(2, result.StyleBundles[0].Includes.Count);
            Assert.AreEqual("~/Content/jQuery.css", result.StyleBundles[0].Includes[0]);
            Assert.AreEqual("~/Content/jQuery.ui.css", result.StyleBundles[0].Includes[1]);
        }

        [TestMethod]
        public void BundleConfigReadsBundleConfigWithOnlyStyleBundles() {
            // Arrange
            var xml = ToStream(
@"<?xml version=""1.0"" ?>
    <bundles version=""1.0"">
        <styleBundle path=""~/my-bundle-path"" cdnPath=""http://cdn.com/bundle.css"">
            <include path=""~/Content/jQuery.ui.css""></include>
        </styleBundle>
        <styleBundle path=""~/content/css"">
            <include path=""~/scripts/master.css""></include>
            <include path=""~/scripts/page.css""></include>
        </styleBundle>
    </bundles>");

            // Act
            var result = BundleManifest.ReadBundleManifest(xml);

            // Assert
            Assert.AreEqual(0, result.ScriptBundles.Count);
            Assert.AreEqual(2, result.StyleBundles.Count);
            Assert.AreEqual("~/my-bundle-path", result.StyleBundles[0].Path);
            Assert.AreEqual("http://cdn.com/bundle.css", result.StyleBundles[0].CdnPath);
            Assert.AreEqual(1, result.StyleBundles[0].Includes.Count);
            Assert.AreEqual("~/Content/jQuery.ui.css", result.StyleBundles[0].Includes[0]);

            Assert.AreEqual("~/content/css", result.StyleBundles[1].Path);
            Assert.AreEqual(2, result.StyleBundles[1].Includes.Count);
            Assert.AreEqual("~/scripts/master.css", result.StyleBundles[1].Includes[0]);
            Assert.AreEqual("~/scripts/page.css", result.StyleBundles[1].Includes[1]);
        }

        [TestMethod]
        public void BundleConfigReadsBundleConfigWithOnlyScriptBundles() {
            // Arrange
            var xml = ToStream(
@"<?xml version=""1.0"" ?>
    <bundles version=""1.0"">
        <scriptBundle path=""~/my-bundle-path"" cdnPath=""http://cdn.com/bundle.js"" cdnFallbackExpression=""!window.jquery"">
            <include path=""~/Scripts/jQuery.js""></include>
        </scriptBundle>
        <scriptBundle path=""~/Scripts/js"">
            <include path=""~/scripts/first.js""></include>
            <include path=""~/scripts/second.js""></include>
        </scriptBundle>
    </bundles>");

            // Act
            var result = BundleManifest.ReadBundleManifest(xml);

            // Assert
            Assert.AreEqual(0, result.StyleBundles.Count);
            Assert.AreEqual(2, result.ScriptBundles.Count);
            Assert.AreEqual("~/my-bundle-path", result.ScriptBundles[0].Path);
            Assert.AreEqual("http://cdn.com/bundle.js", result.ScriptBundles[0].CdnPath);
            Assert.AreEqual("!window.jquery", result.ScriptBundles[0].CdnFallbackExpression);
            Assert.AreEqual(1, result.ScriptBundles[0].Includes.Count);
            Assert.AreEqual("~/Scripts/jQuery.js", result.ScriptBundles[0].Includes[0]);

            Assert.AreEqual("~/Scripts/js", result.ScriptBundles[1].Path);
            Assert.AreEqual(2, result.ScriptBundles[1].Includes.Count);
            Assert.AreEqual("~/scripts/first.js", result.ScriptBundles[1].Includes[0]);
            Assert.AreEqual("~/scripts/second.js", result.ScriptBundles[1].Includes[1]);
        }

        [TestMethod]
        public void BundleConfigReadsBundleConfigWithMixedBundles() {
            // Arrange
            var xml = ToStream(
@"<?xml version=""1.0"" ?>
    <bundles version=""1.0"">
        <scriptBundle path=""~/my-bundle-path"" cdnPath=""http://cdn.com/bundle.js"" cdnFallbackExpression=""!window.jquery"">
            <include path=""~/Scripts/jQuery.js""></include>
        </scriptBundle>
        <scriptBundle path=""~/Scripts/js"">
            <include path=""~/scripts/first.js""></include>
            <include path=""~/scripts/second.js""></include>
        </scriptBundle>
        <styleBundle path=""~/my-bundle-path"" cdnPath=""http://cdn.com/bundle.css"">
            <include path=""~/Content/jQuery.ui.css""></include>
        </styleBundle>
        <styleBundle path=""~/content/css"">
            <include path=""~/scripts/master.css""></include>
            <include path=""~/scripts/page.css""></include>
        </styleBundle>
    </bundles>");

            // Act
            var result = BundleManifest.ReadBundleManifest(xml);

            // Assert
            Assert.AreEqual(2, result.ScriptBundles.Count);
            Assert.AreEqual("~/my-bundle-path", result.ScriptBundles[0].Path);
            Assert.AreEqual("http://cdn.com/bundle.js", result.ScriptBundles[0].CdnPath);
            Assert.AreEqual("!window.jquery", result.ScriptBundles[0].CdnFallbackExpression);
            Assert.AreEqual(1, result.ScriptBundles[0].Includes.Count);
            Assert.AreEqual("~/Scripts/jQuery.js", result.ScriptBundles[0].Includes[0]);

            Assert.AreEqual("~/Scripts/js", result.ScriptBundles[1].Path);
            Assert.AreEqual(2, result.ScriptBundles[1].Includes.Count);
            Assert.AreEqual("~/scripts/first.js", result.ScriptBundles[1].Includes[0]);
            Assert.AreEqual("~/scripts/second.js", result.ScriptBundles[1].Includes[1]);

            Assert.AreEqual(2, result.StyleBundles.Count);
            Assert.AreEqual("~/my-bundle-path", result.StyleBundles[0].Path);
            Assert.AreEqual("http://cdn.com/bundle.css", result.StyleBundles[0].CdnPath);
            Assert.AreEqual(1, result.StyleBundles[0].Includes.Count);
            Assert.AreEqual("~/Content/jQuery.ui.css", result.StyleBundles[0].Includes[0]);

            Assert.AreEqual("~/content/css", result.StyleBundles[1].Path);
            Assert.AreEqual(2, result.StyleBundles[1].Includes.Count);
            Assert.AreEqual("~/scripts/master.css", result.StyleBundles[1].Includes[0]);
            Assert.AreEqual("~/scripts/page.css", result.StyleBundles[1].Includes[1]);

        }

        private static Stream ToStream(string input) {
            var bytes = Encoding.UTF8.GetBytes(input);
            return new MemoryStream(bytes);
        }
    }
}
