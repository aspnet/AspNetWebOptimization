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

        [TestMethod]
        public void DontRebaseDataUris()
        {
            var cssWithDatUri = "background: url(data:image/gif;base64,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);";
            var convertedCss = CssRewriteUrlTransform.ConvertUrlsToAbsolute("/Content/themes/base", cssWithDatUri);
            Assert.AreEqual(cssWithDatUri,convertedCss);
        }
    }
}
