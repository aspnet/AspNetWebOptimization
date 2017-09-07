// Copyright (c) Microsoft Corporation, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace System.Web.Optimization.Test {

    [TestClass]
    public class CssRewriteUrlTransformTest {
        public TestContext TestContext { get; set; }

        [TestMethod]
        public void RebaseDotDotRelativeUrlTest() {
            Assert.AreEqual("/images/bg.jpg", CssRewriteUrlTransform.RebaseUrlToAbsolute("/images", "../images/bg.jpg"));
            Assert.AreEqual("/images/bg.jpg", CssRewriteUrlTransform.RebaseUrlToAbsolute("/images/", "../images/bg.jpg"));
            // content/themes/foo.css => bundles/themes.css
            Assert.AreEqual("/content/images/bg.jpg", CssRewriteUrlTransform.RebaseUrlToAbsolute("/content/themes/", "../images/bg.jpg"));
            Assert.AreEqual("/content/images/bg.jpg", CssRewriteUrlTransform.RebaseUrlToAbsolute("/content/themes", "../images/bg.jpg"));
        }

        [TestMethod]
        public void RebaseAbsoluteUrlIsNoopTest() {
            // i.e. moving css from /content/foo.css => /bundles/css
            Assert.AreEqual("/content/images/bg.jpg", CssRewriteUrlTransform.RebaseUrlToAbsolute("/content", "/content/images/bg.jpg"));
            Assert.AreEqual("/content/images/bg.jpg", CssRewriteUrlTransform.RebaseUrlToAbsolute("/content/", "/content/images/bg.jpg"));
        }

        [TestMethod]
        public void RebaseArgumentCheckingTests() {
            // i.e. moving css from /content/foo.css => /bundles/css
            Assert.AreEqual("foo", CssRewriteUrlTransform.RebaseUrlToAbsolute("  ", "foo"));
            Assert.AreEqual("foo", CssRewriteUrlTransform.RebaseUrlToAbsolute("", "foo"));
            Assert.AreEqual("foo", CssRewriteUrlTransform.RebaseUrlToAbsolute(null, "foo"));
            Assert.AreEqual(" ", CssRewriteUrlTransform.RebaseUrlToAbsolute("/content", " "));
            Assert.AreEqual("", CssRewriteUrlTransform.RebaseUrlToAbsolute("/content", ""));
            Assert.AreEqual(null, CssRewriteUrlTransform.RebaseUrlToAbsolute("/content", null));
        }

        [TestMethod]
        public void RebaseBasicRelativeUrlTest() {
            // i.e. moving css from /content/foo.css => /bundles/css
            Assert.AreEqual("/content/images/bg.jpg", CssRewriteUrlTransform.RebaseUrlToAbsolute("/content", "images/bg.jpg"));
        }

        [TestMethod]
        public void RebaseUrlNoQuotesTest() {
            var css = new CssRewriteUrlTransform();
            // i.e. moving css from /content/foo.css => /bundles/css
            Assert.AreEqual("url(/content/images/foo.png)", CssRewriteUrlTransform.ConvertUrlsToAbsolute("/content", "url(images/foo.png)"));
        }

        [TestMethod]
        public void RebaseUrlQuotesTest() {
            var css = new CssRewriteUrlTransform();
            // i.e. moving css from /content/foo.css => /bundles/css
            Assert.AreEqual("url(/content/images/foo.png)", CssRewriteUrlTransform.ConvertUrlsToAbsolute("/content", "url(\"images/foo.png\")"));
        }

        [TestMethod]
        public void RebaseUrlSingleQuotesTest() {
            var css = new CssRewriteUrlTransform();
            // i.e. moving css from /content/foo.css => /bundles/css
            Assert.AreEqual("url(/content/images/foo.png)", CssRewriteUrlTransform.ConvertUrlsToAbsolute("/content", "url('images/foo.png')"));
        }

        [TestMethod]
        public void RebaseTemplateContentSiteCssTest() {
            var css = new CssRewriteUrlTransform();
            string siteCss = @"
.main-content {
    background: url(""../Images/accent.png"") no-repeat;
}

.featured + .main-content {
    background: url(""../Images/heroAccent.png"") no-repeat;
}";
            string expectedCss = @"
.main-content {
    background: url(/Images/accent.png) no-repeat;
}

.featured + .main-content {
    background: url(/Images/heroAccent.png) no-repeat;
}";
            Assert.AreEqual(expectedCss, CssRewriteUrlTransform.ConvertUrlsToAbsolute("/content", siteCss));
        }

        [TestMethod]
        public void RebaseJqueryBaseThemeTest() {
            var css = new CssRewriteUrlTransform();
            Assert.AreEqual("url(/content/themes/base/images/ui-bg_flat_75_ffffff_40x100.png)", CssRewriteUrlTransform.ConvertUrlsToAbsolute("/content/themes/base", "url(images/ui-bg_flat_75_ffffff_40x100.png)"));
        }

        [TestMethod]
        public void OptimizerCssUrlRewriteInTwoDirectoriesTest() {
            OptimizationSettings config = new OptimizationSettings() {
                ApplicationPath = TestContext.DeploymentDirectory,
                BundleSetupMethod = (bundles) => {
                    bundles.Add(new StyleBundle("~/bundles/css")
                        .Include("~/Styles/image.css", new CssRewriteUrlTransform())
                        .Include("~/Styles/nested/image2.css", new CssRewriteUrlTransform()));
                }
            };

            BundleResponse response = Optimizer.BuildBundle("~/bundles/css", config);
            Assert.IsNotNull(response);
            Assert.AreEqual(".image{background:url(/Styles/images/ui.png)}.image2{background:url(/Styles/nested/images/ui2.png)}", response.Content);
            Assert.AreEqual(CssMinify.CssContentType, response.ContentType);
        }

        [TestMethod]
        public void OptimizerCssUrlRewriteInTwoDirectoriesAndUpperOneTest() {
            OptimizationSettings config = new OptimizationSettings() {
                ApplicationPath = TestContext.DeploymentDirectory,
                BundleSetupMethod = (bundles) => {
                    bundles.Add(new StyleBundle("~/bundles/css")
                        .Include("~/Styles/image.css", new CssRewriteUrlTransform(), new UppercaseTransform())
                        .Include("~/Styles/nested/image2.css", new CssRewriteUrlTransform()));
                }
            };

            BundleResponse response = Optimizer.BuildBundle("~/bundles/css", config);
            Assert.IsNotNull(response);
            // Note: Minification of the bundle lower cases the url
            Assert.AreEqual(".IMAGE{BACKGROUND:url(/STYLES/IMAGES/UI.PNG)}.image2{background:url(/Styles/nested/images/ui2.png)}", response.Content);
            Assert.AreEqual(CssMinify.CssContentType, response.ContentType);
        }

        [TestMethod]
        public void OptimizerCssUrlRewriteWithWildcardTest() {
            OptimizationSettings config = new OptimizationSettings() {
                ApplicationPath = TestContext.DeploymentDirectory,
                BundleSetupMethod = (bundles) => {
                    bundles.Add(new StyleBundle("~/bundles/css")
                        .Include("~/Styles/image.css", new CssRewriteUrlTransform(), new UppercaseTransform())
                        .Include("~/Styles/nested/*.css", new CssRewriteUrlTransform()));
                }
            };

            BundleResponse response = Optimizer.BuildBundle("~/bundles/css", config);
            Assert.IsNotNull(response);
            // Note: Minification of the bundle lower cases the url
            Assert.AreEqual(".IMAGE{BACKGROUND:url(/STYLES/IMAGES/UI.PNG)}.image2{background:url(/Styles/nested/images/ui2.png)}", response.Content);
            Assert.AreEqual(CssMinify.CssContentType, response.ContentType);
        }

    }
}
