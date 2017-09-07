// Copyright (c) Microsoft Corporation, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System.Collections;
using System.Collections.Generic;
using System.Web.Hosting;
using System.Web.TestUtil;

namespace System.Web.Optimization.Test {

    [TestClass]
    public class StylesTest {

        private AssetManager SetupAssetManager() {
            return SetupAssetManager(baseUrl:null, resolvePath: ResolvePath);
        }

        private AssetManager SetupAssetManager(string baseUrl) {
            return SetupAssetManager(baseUrl, ResolvePath);
        }

        private AssetManager SetupAssetManager(string baseUrl, Func<string, string, string> resolvePath) {
            Mock<HttpContextBase> mockContext = new Mock<HttpContextBase>();
            var itemsCollection = new Hashtable();
            mockContext.Setup(context => context.Items).Returns(itemsCollection);
            if (baseUrl != null) {
                mockContext.Setup(context => context.Request.AppRelativeCurrentExecutionFilePath).Returns(baseUrl);
            }
            Styles.Context = mockContext.Object;
            AssetManager manager = AssetManager.GetInstance(mockContext.Object);
            manager.ResolveUrlMethod = resolvePath;
            return manager;
        }


        [TestMethod]
        public void Render_WillResolvePathsAndDedupeWithBundle() {
            // Arrange
            AssetManager manager = SetupAssetManager(null, ((_, __) => "/foo/bundleFile1"));
            manager.OptimizationEnabled = true;
            manager.Resolver = new TestBundleResolver();

            // Act
            // Assert
            Assert.AreEqual(@"<link href=""~/Bundle?version"" rel=""stylesheet""/>
", Styles.Render("~/baz.css", "~/Bundle").ToHtmlString());
        }

        [TestMethod]
        public void StylesRenderInDebugWorksWithBaseAppAndBundle() {
            // Arrange
            AssetManager manager = SetupAssetManager("/");
            manager.OptimizationEnabled = false;

            BundleCollection bundles = new BundleCollection();
            BundleResponse response = new BundleResponse("fake", new List<BundleFile>() { new BundleFile("~/Content/1.css", new FileVirtualPathProvider.FileInfoVirtualFile("/Content/1.css", null)) });
            bundles.Add(new CacheHitBundle("~/bundle", response));
            manager.Bundles = bundles;
            manager.Resolver = new BundleResolver(bundles) { Context = manager.Context };

            // Act
            // Assert
            string expectedTags = @"<link href=""/Content/1.css"" rel=""stylesheet""/>
";
            Assert.AreEqual(expectedTags, Styles.Render("~/bundle").ToHtmlString());
        }

        [TestMethod]
        public void Render_WillUseQualifiedUrlsWithoutResolving() {
            // Arrange
            AssetManager manager = SetupAssetManager(null, ((_, __) => { throw new Exception("This should not be called."); }));

            // Act
            string expected = "";
            string[] urls = { "http://foo.com/bar.js", "Https://foo.com/bar.js", "ftp://foo.com/bar.js" };
            foreach (string url in urls) {
                expected += String.Format(@"<link href=""{0}"" rel=""stylesheet""/>
", url);
            }

            // Assert
            Assert.AreEqual(expected, Styles.Render(urls).ToHtmlString());
        }

        [TestMethod]
        public void Render_ResolvesPaths() {
            // Arrange
            AssetManager manager = SetupAssetManager("/foo/bar/");

            // Act
            // Assert
            Assert.AreEqual(@"<link href=""/foo/bar/baz.css"" rel=""stylesheet""/>
", Styles.Render("~/baz.css", "/foo/bar/baz.css").ToHtmlString());
        }

        [TestMethod]
        public void Render_WillResolvePathsAndNotDuplicate() {
            // Arrange
            AssetManager manager = SetupAssetManager("/foo/bar/", ((_, __) => "/foo/baz.css"));

            // Act
            // Assert
            Assert.AreEqual(@"<link href=""/foo/baz.css"" rel=""stylesheet""/>
", Styles.Render("~/baz.css", "../baz.css").ToHtmlString());
        }

        [TestMethod]
        public void RenderWillRenderInOrderOfAdd() {
            // Arrange
            AssetManager manager = SetupAssetManager("/foo/bar/");

            // Act
            // Assert
            string expectedTags = @"<link href=""/foo/bar/2.css"" rel=""stylesheet""/>
<link href=""/foo/bar/1.css"" rel=""stylesheet""/>
<link href=""/foo/bar/3.css"" rel=""stylesheet""/>
";

            Assert.AreEqual(expectedTags, Styles.Render("~/2.css", "~/1.css", "~/3.css").ToHtmlString());
        }

        [TestMethod]
        public void RenderWillRenderNothingWhenNoScriptsRegistered() {
            // Arrange
            AssetManager manager = SetupAssetManager("/foo/bar/");

            // Act
            // Assert
            Assert.AreEqual("", Styles.Render().ToHtmlString());
        }

        [TestMethod]
        public void RenderWillRenderWithEncoding() {
            // Arrange
            AssetManager manager = SetupAssetManager("/foo/bar/");

            // Act
            // Assert
            string expectedTags = @"<link href=""/foo/bar/1%201.css"" rel=""stylesheet""/>
<link href=""/foo/bar/2%202.css"" rel=""stylesheet""/>
<link href=""/foo/bar/3%203.css"" rel=""stylesheet""/>
";
            Assert.AreEqual(expectedTags, Styles.Render("~/1 1.css", "~/2 2.css", "~/3 3.css").ToHtmlString());
        }

        [TestMethod]
        public void UrlWillResolveNonBundleVirtualPaths() {
            SetupAssetManager("/foo/bar/");
            Assert.AreEqual("/foo/bar/notExist", Styles.Url("~/notExist").ToHtmlString());
        }

        [TestMethod]
        public void UrlWillNotModifyValidURIs() {
            Assert.AreEqual("http://foo/url", Styles.Url("http://foo/url").ToHtmlString());
            Assert.AreEqual("https://foo/url", Styles.Url("https://foo/url").ToHtmlString());
            Assert.AreEqual("ftp://foo/url", Styles.Url("ftp://foo/url").ToHtmlString());
        }

        [TestMethod]
        public void RenderWithNullThrowsTest() {
            SetupAssetManager();
            ExceptionHelper.ExpectArgumentNullException(delegate { Styles.Render(null); }, "paths");
        }

        [TestMethod]
        public void RenderWithNestedNullThrowsTest() {
            SetupAssetManager();
            ExceptionHelper.ExpectArgumentExceptionNullOrEmpty(delegate { Styles.Render("notnull", null); }, "paths");
        }

        [TestMethod]
        public void RenderWithNestedEmptyThrowsTest() {
            SetupAssetManager();
            ExceptionHelper.ExpectArgumentExceptionNullOrEmpty(delegate { Styles.Render("notnull", ""); }, "paths");
        }

        [TestMethod]
        public void RenderWithOptimizationsWillUseCdnFromBundleIfSpecifiedTest() {
            AssetManager manager = SetupAssetManager();
            BundleCollection col = new BundleCollection();
            manager.Bundles = col;
            BundleResolver resolver = new BundleResolver(col, manager.Context);
            manager.Resolver = resolver;
            manager.OptimizationEnabled = true;
            col.Add(new ScriptBundle("~/bundle1", "http://cdnPath1"));
            col.Add(new ScriptBundle("~/bundle2"));
            col.Add(new ScriptBundle("~/bundle3", "http://cdnPath3"));
            col.UseCdn = true;

            string expectedTags = @"<link href=""http://cdnPath1"" rel=""stylesheet""/>
<link href=""bundle2?v="" rel=""stylesheet""/>
<link href=""http://cdnPath3"" rel=""stylesheet""/>
";
            Assert.AreEqual(expectedTags, Styles.Render("~/bundle1", "~/bundle2", "~/bundle3").ToHtmlString());
        }

        [TestMethod]
        public void RenderWithOptimizationsWillIgnoreCdnPathFromBundleIfNotSpecifiedTest() {
            AssetManager manager = SetupAssetManager();
            BundleCollection col = new BundleCollection();
            manager.Bundles = col;
            BundleResolver resolver = new BundleResolver(col, manager.Context);
            manager.Resolver = resolver;
            manager.OptimizationEnabled = true;
            col.Add(new ScriptBundle("~/bundle1", "http://cdnPath1"));
            col.Add(new ScriptBundle("~/bundle2"));
            col.Add(new ScriptBundle("~/bundle3", "http://cdnPath3"));
            col.UseCdn = false;

            string expectedTags = @"<link href=""bundle1?v="" rel=""stylesheet""/>
<link href=""bundle2?v="" rel=""stylesheet""/>
<link href=""bundle3?v="" rel=""stylesheet""/>
";
            Assert.AreEqual(expectedTags, Styles.Render("~/bundle1", "~/bundle2", "~/bundle3").ToHtmlString());
        }

        [TestMethod]
        public void Styles_RenderWithNoopCustomFormat() {
            // Arrange
            AssetManager manager = SetupAssetManager();

            string[] paths = { "https://foo.com/bar", "http://foo.com/bar", "foo.css" };
            string expected = String.Join(Environment.NewLine, paths) + Environment.NewLine;

            // Act
            // Assert
            Assert.AreEqual(expected, Styles.RenderFormat("{0}", paths).ToHtmlString());
        }

        private static string ResolvePath(string basePath, string virtualPath) {
            return basePath + virtualPath.TrimStart('~', '/');
        }

    }
}
