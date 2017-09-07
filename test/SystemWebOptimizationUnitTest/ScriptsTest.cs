// Copyright (c) Microsoft Corporation, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System.Collections;
using System.Collections.Generic;
using System.Web.Hosting;
using System.Web.TestUtil;
using SystemWebOptimizationUnitTest.Util;

namespace System.Web.Optimization.Test {

    [TestClass]
    public class ScriptsTest {
        private AssetManager SetupAssetManager() {
            return SetupAssetManager(baseUrl:null, resolvePath: ResolvePath);
        }

        private AssetManager SetupAssetManager(string baseUrl) {
            return SetupAssetManager(baseUrl, ResolvePath);
        }

        private static HttpContextBase SetupContext(string baseUrl) {
            Mock<HttpContextBase> mockContext = new Mock<HttpContextBase>();
            var itemsCollection = new Hashtable();
            mockContext.Setup(context => context.Items).Returns(itemsCollection);
            if (baseUrl != null) {
                mockContext.Setup(context => context.Request.AppRelativeCurrentExecutionFilePath).Returns(baseUrl);
                mockContext.Setup(context => context.Response.ApplyAppPathModifier(It.IsAny<string>())).Returns((string s) => baseUrl + "/" + s);
            }
            Scripts.Context = mockContext.Object;
            return mockContext.Object;
        }

        private static AssetManager SetupAssetManager(string baseUrl, Func<string, string, string> resolvePath) {
            HttpContextBase context = SetupContext(baseUrl);
            AssetManager manager = AssetManager.GetInstance(context);
            manager.ResolveUrlMethod = resolvePath;
            return manager;
        }

        [TestMethod]
        public void Render_WillLeaveHttpUrlsAlone() {
            // Arrange
            AssetManager manager = SetupAssetManager();

            // Act
            // Assert
            Assert.AreEqual(@"<script src=""Http://foo.com/bar.js""></script>
", Scripts.Render("Http://foo.com/bar.js").ToHtmlString());
        }

        [TestMethod]
        public void Render_WillLeaveHttpsUrlsAlone() {
            // Arrange
            AssetManager manager = SetupAssetManager();

            // Act
            // Assert
            Assert.AreEqual(@"<script src=""Https://foo.com/bar.js""></script>
", Scripts.Render("Https://foo.com/bar.js").ToHtmlString());
        }

        [TestMethod]
        public void Render_WillLeaveFTPUrlsAlone() {
            // Arrange
            AssetManager manager = SetupAssetManager();

            // Act
            // Assert
            Assert.AreEqual(@"<script src=""ftp://foo.com/bar.js""></script>
", Scripts.Render("ftp://foo.com/bar.js").ToHtmlString());
        }

        [TestMethod]
        public void Render_WillResolveAppRelativeUrls() {
            // Arrange
            AssetManager manager = SetupAssetManager("/foo/bar/");

            // Act
            // Assert
            Assert.AreEqual(@"<script src=""/foo/bar/baz.js""></script>
", Scripts.Render("~/baz.js").ToHtmlString());
        }

        [TestMethod]
        public void Render_WillResolveRelativeUrlsBasedOnExecutingFile() {
            // Arrange
            AssetManager manager = SetupAssetManager("/foo/bar/");

            // Act
            // Assert
            Assert.AreEqual(@"<script src=""/foo/bar/../baz.js""></script>
", Scripts.Render("../baz.js").ToHtmlString());
        }

        [TestMethod]
        public void Render_WillResolvePathsAndNotDuplicate() {
            // Arrange
            AssetManager manager = SetupAssetManager(null, ((_, __) => "/foo/baz.js"));

            // Act
            // Assert
            Assert.AreEqual(@"<script src=""/foo/baz.js""></script>
", Scripts.Render("~/baz.js", "../baz.js").ToHtmlString());
        }

        [TestMethod]
        public void Render_WillResolvePathsAndDedupeWithBundle() {
            // Arrange
            AssetManager manager = SetupAssetManager(null, ((_, __) => "/foo/bundleFile1"));
            manager.OptimizationEnabled = true;
            manager.Resolver = new TestBundleResolver();

            // Act
            // Assert
            Assert.AreEqual(@"<script src=""~/Bundle?version""></script>
", Scripts.Render("~/baz.js", "~/Bundle").ToHtmlString());
        }

        [TestMethod]
        public void ScriptsRenderInDebugWorksWithBaseAppAndBundle() {
            // Arrange
            AssetManager manager = SetupAssetManager("/");
            manager.OptimizationEnabled = false;
            BundleCollection bundles = new BundleCollection();
            BundleResponse response = new BundleResponse("fake", new List<BundleFile>() { new BundleFile("/scripts/1.js", new FileVirtualPathProvider.FileInfoVirtualFile("/scripts/1.js", null)) });
            bundles.Add(new CacheHitBundle("~/bundle", response));
            manager.Bundles = bundles;
            manager.Resolver = new BundleResolver(bundles) { Context = manager.Context };

            // Act
            // Assert
            string expectedTags = @"<script src=""/scripts/1.js""></script>
";
            Assert.AreEqual(expectedTags, Scripts.Render("~/bundle").ToHtmlString());
        }

        [TestMethod]
        public void Render_WillRenderInOrderOfAdd() {
            // Arrange
            AssetManager manager = SetupAssetManager("/foo/bar/");

            // Act
            // Assert
            string expectedTags = @"<script src=""/foo/bar/3.js""></script>
<script src=""/foo/bar/1.js""></script>
<script src=""/foo/bar/2.js""></script>
";
            Assert.AreEqual(expectedTags, Scripts.Render("~/3.js", "~/1.js", "~/2.js").ToHtmlString());
        }

        [TestMethod]
        public void Render_WillRenderNothingWhenNoScriptsRegistered() {
            // Arrange
            AssetManager manager = SetupAssetManager("/foo/bar/");

            // Act

            // Assert
            Assert.AreEqual("", Scripts.Render().ToHtmlString());
        }
        [TestMethod]
        public void Render_WillRenderWithEncoding() {
            // Arrange
            AssetManager manager = SetupAssetManager("/foo/bar/");

            // Act
            // Assert
            string expectedTags = @"<script src=""/foo/bar/1%201.js""></script>
<script src=""/foo/bar/2%202.js""></script>
<script src=""/foo/bar/3%203.js""></script>
";
            Assert.AreEqual(expectedTags, Scripts.Render("~/1 1.js", "~/2 2.js", "~/3 3.js").ToHtmlString());
        }

        [TestMethod]
        public void UrlWillResolveNonBundleVirtualPaths() {
            SetupAssetManager("/foo/bar/");
            Assert.AreEqual("/foo/bar/notExist", Scripts.Url("~/notExist").ToHtmlString());
        }

        [TestMethod]
        public void UrlWillNotModifyValidURIs() {
            Assert.AreEqual("http://foo/url", Scripts.Url("http://foo/url").ToHtmlString());
            Assert.AreEqual("https://foo/url", Scripts.Url("https://foo/url").ToHtmlString());
            Assert.AreEqual("ftp://foo/url", Scripts.Url("ftp://foo/url").ToHtmlString());
        }

        [TestMethod]
        public void RenderWithNullThrowsTest() {
            SetupAssetManager();
            ExceptionHelper.ExpectArgumentNullException(delegate { Scripts.Render(null); }, "paths");
        }

        [TestMethod]
        public void RenderWithNestedNullThrowsTest() {
            SetupAssetManager();
            ExceptionHelper.ExpectArgumentExceptionNullOrEmpty(delegate { Scripts.Render("notnull", null); }, "paths");
        }

        [TestMethod]
        public void RenderWithNestedEmptyThrowsTest() {
            SetupAssetManager();
            ExceptionHelper.ExpectArgumentExceptionNullOrEmpty(delegate { Scripts.Render("notnull", ""); }, "paths");
        }

        [TestMethod]
        public void RenderWithCdnFallback() {
            BundleCollection col = SetupBundleCollection(optimizationEnabled: true);
            ScriptBundle bundle = new ScriptBundle("~/bundle1", "http://cdnPath1");
            bundle.CdnFallbackExpression = "cdnFallback";
            col.Add(bundle);
            col.UseCdn = true;
            string expectedTags = @"<script src=""http://cdnPath1""></script>
<script>(cdnFallback)||document.write('<script src=""bundle1""><\/script>');</script>
";
            Assert.AreEqual(expectedTags, Scripts.Render("~/bundle1").ToHtmlString());
        }

        [TestMethod]
        public void RenderWithOptimizationsWillUseCdnFromBundleIfSpecifiedTest() {
            BundleCollection col = SetupBundleCollection(optimizationEnabled: true);
            col.Add(new ScriptBundle("~/bundle1", "http://cdnPath1"));
            col.Add(new ScriptBundle("~/bundle2"));
            col.Add(new ScriptBundle("~/bundle3", "http://cdnPath3"));
            col.UseCdn = true;

            string expectedTags = @"<script src=""http://cdnPath1""></script>
<script src=""bundle2?v=""></script>
<script src=""http://cdnPath3""></script>
";
            Assert.AreEqual(expectedTags, Scripts.Render("~/bundle1", "~/bundle2", "~/bundle3").ToHtmlString());
        }

        [TestMethod]
        public void RenderWithOptimizationsWillIgnoreCdnPathFromBundleIfNotSpecifiedTest() {
            BundleCollection col = SetupBundleCollection(optimizationEnabled: true);
            col.Add(new ScriptBundle("~/bundle1", "http://cdnPath1"));
            col.Add(new ScriptBundle("~/bundle2"));
            col.Add(new ScriptBundle("~/bundle3", "http://cdnPath3"));
            col.UseCdn = false;

            string expectedTags = @"<script src=""bundle1?v=""></script>
<script src=""bundle2?v=""></script>
<script src=""bundle3?v=""></script>
";
            Assert.AreEqual(expectedTags, Scripts.Render("~/bundle1", "~/bundle2", "~/bundle3").ToHtmlString());
        }

        private BundleCollection SetupBundleCollection(bool optimizationEnabled) {
            AssetManager manager = SetupAssetManager();
            BundleCollection col = new BundleCollection();
            manager.Bundles = col;
            BundleResolver resolver = new BundleResolver(col, manager.Context);
            manager.Resolver = resolver;
            manager.OptimizationEnabled = optimizationEnabled;
            return col;
        }

        [TestMethod]
        public void RenderWithNoopCustomFormat() {
            // Arrange
            AssetManager manager = SetupAssetManager();

            string[] paths = { "https://foo.com/bar", "http://foo.com/bar", "foo.js" };
            string expected = String.Join(Environment.NewLine, paths) + Environment.NewLine;

            // Act
            // Assert
            Assert.AreEqual(expected, Scripts.RenderFormat("{0}", paths).ToHtmlString());
        }

        private static string ResolvePath(string basePath, string virtualPath) {
            return basePath + virtualPath.TrimStart('~', '/');
        }

    }
}
