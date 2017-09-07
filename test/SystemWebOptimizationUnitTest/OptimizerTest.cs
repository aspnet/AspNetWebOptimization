// Copyright (c) Microsoft Corporation, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;
using System.Web.TestUtil;

namespace System.Web.Optimization.Test {
    [TestClass]
    public class OptimizerTests {
        public TestContext TestContext { get; set; }

        [TestMethod]
        [DeploymentItem("bundle.config")]
        public void OptimizerScriptBundleUsingBundleConfig() {
            OptimizationSettings config = new OptimizationSettings() { 
                ApplicationPath = TestContext.DeploymentDirectory, 
                BundleManifestPath = "bundle.config" };
            BundleResponse response = Optimizer.BuildBundle("~/bundles/js", config);
            Assert.IsNotNull(response);
            Assert.AreEqual("alert(\"first\");alert(\"second\")", response.Content);
            Assert.AreEqual(JsMinify.JsContentType, response.ContentType);
        }

        [TestMethod]
        [DeploymentItem("bundle.config")]
        public void OptimizerStyleBundleUsingBundleConfig() {
            OptimizationSettings config = new OptimizationSettings() {
                ApplicationPath = TestContext.DeploymentDirectory,
                BundleManifestPath = "bundle.config"
            };
            BundleResponse response = Optimizer.BuildBundle("~/bundles/css", config);
            Assert.IsNotNull(response);
            Assert.AreEqual("Css2{color:blue}Css3{color:blue}Css1{color:blue}", response.Content);
            Assert.AreEqual(CssMinify.CssContentType, response.ContentType);
        }

        [TestMethod]
        [DeploymentItem("bundle.config")]
        public void OptimizerReturnsNullWhenNoBundleRegistered() {
            OptimizationSettings config = new OptimizationSettings() {
                ApplicationPath = TestContext.DeploymentDirectory,
                BundleManifestPath = "bundle.config"
            };
            BundleResponse response = Optimizer.BuildBundle("~/bundles/none", config);
            Assert.IsNull(response);
        }

        [TestMethod]
        public void OptimizerWithWildcardMissUsingSetupMethodOnly() {
            OptimizationSettings config = new OptimizationSettings() {
                ApplicationPath = TestContext.DeploymentDirectory,
                BundleSetupMethod = (bundles) => {
                    bundles.Add(new ScriptBundle("~/bundles/js").Include("~/scripts/none-*"));
                }
            };

            BundleResponse response = Optimizer.BuildBundle("~/bundles/js", config);
            Assert.IsNotNull(response);
            Assert.AreEqual("", response.Content);
            Assert.AreEqual(JsMinify.JsContentType, response.ContentType);
        }

        [TestMethod]
        public void OptimizerUsingSetupMethodOnly() {
            OptimizationSettings config = new OptimizationSettings() {
                ApplicationPath = TestContext.DeploymentDirectory,
                BundleSetupMethod = (bundles) => {
                    bundles.Add(new ScriptBundle("~/bundles/js").Include("~/scripts/first.js"));
                    bundles.Add(new StyleBundle("~/bundles/css").Include("~/styles/1.css"));
                }
            };

            BundleResponse response = Optimizer.BuildBundle("~/bundles/js", config);
            Assert.IsNotNull(response);
            Assert.AreEqual("alert(\"first\")", response.Content);
            Assert.AreEqual(JsMinify.JsContentType, response.ContentType);

            response = Optimizer.BuildBundle("~/bundles/css", config);
            Assert.IsNotNull(response);
            Assert.AreEqual("Css1{color:blue}", response.Content);
            Assert.AreEqual(CssMinify.CssContentType, response.ContentType);
        }

        [TestMethod]
        public void OptimizerUppercaseItemTransformTest() {
            OptimizationSettings config = new OptimizationSettings() {
                ApplicationPath = TestContext.DeploymentDirectory,
                BundleSetupMethod = (bundles) => {
                    bundles.Add(new ScriptBundle("~/bundles/upper").Include("~/scripts/first.js", new UppercaseTransform()));
                }
            };

            BundleResponse response = Optimizer.BuildBundle("~/bundles/upper", config);
            Assert.IsNotNull(response);
            Assert.AreEqual("ALERT(\"FIRST\")", response.Content);
            Assert.AreEqual(JsMinify.JsContentType, response.ContentType);
        }

        [TestMethod]
        public void OptimizerBundleItemsAddWithBadPathThrowsFileNotFoundExceptionTest() {
            OptimizationSettings config = new OptimizationSettings() {
                ApplicationPath = TestContext.DeploymentDirectory,
                BundleSetupMethod = (bundles) => {
                    var bundle = new ScriptBundle("~/bundles/bogus");
                    var bundleItem = new BundleItem("~/totallybogus");
                    bundle.Items.Add(bundleItem);
                    bundles.Add(bundle);
                }
            };

            ExceptionHelper.ExpectException<FileNotFoundException>(() => {
                BundleResponse response = Optimizer.BuildBundle("~/bundles/bogus", config);
            });
        }

        [TestMethod]
        public void OptimizerUppercaseOnlyFirstItemTransformTest() {
            OptimizationSettings config = new OptimizationSettings() {
                ApplicationPath = TestContext.DeploymentDirectory,
                BundleSetupMethod = (bundles) => {
                    var bundle = new ScriptBundle("~/bundles/upper");
                    var bundleItem = new BundleItem("~/scripts/first.js");
                    bundleItem.Transforms.Add(new UppercaseTransform());
                    bundle.Items.Add(bundleItem);
                    bundleItem = new BundleItem("~/scripts/second.js");
                    bundle.Items.Add(bundleItem);
                    bundles.Add(bundle);
                }
            };

            BundleResponse response = Optimizer.BuildBundle("~/bundles/upper", config);
            Assert.IsNotNull(response);
            Assert.AreEqual("ALERT(\"FIRST\");alert(\"second\")", response.Content);
            Assert.AreEqual(JsMinify.JsContentType, response.ContentType);
        }

        [TestMethod]
        public void OptimizerDynamicBundleTest() {
            OptimizationSettings config = new OptimizationSettings() {
                ApplicationPath = TestContext.DeploymentDirectory,
                BundleSetupMethod = (bundles) => {
                    bundles.Add(new DynamicFolderBundle("js", "*.js", new JsMinify()));
                }
            };

            BundleResponse response = Optimizer.BuildBundle("~/scripts/js", config);
            Assert.IsNotNull(response);
            Assert.AreEqual("alert(\"first\");alert(\"second\")", response.Content);
            Assert.AreEqual(JsMinify.JsContentType, response.ContentType);
        }

        [TestMethod]
        [DeploymentItem("bundle.config")]
        public void OptimizerSetupMethodOverridesConfig() {
            // Override config bundles to be empty
            OptimizationSettings config = new OptimizationSettings() {
                ApplicationPath = TestContext.DeploymentDirectory,
                BundleManifestPath = "bundle.config",
                BundleSetupMethod = (bundles) => {
                    bundles.Add(new ScriptBundle("~/bundles/js"));
                    bundles.Add(new StyleBundle("~/bundles/css"));
                }
            };

            BundleResponse response = Optimizer.BuildBundle("~/bundles/js", config);
            Assert.IsNotNull(response);
            Assert.AreEqual("", response.Content);
            Assert.AreEqual(JsMinify.JsContentType, response.ContentType);

            response = Optimizer.BuildBundle("~/bundles/css", config);
            Assert.IsNotNull(response);
            Assert.AreEqual("", response.Content);
            Assert.AreEqual(CssMinify.CssContentType, response.ContentType);
        }

        [TestMethod]
        public void OptimizerUsingSetupMethodWithMissingFileThrows() {
            BundleCollection bundles = new BundleCollection();
            bundles.Add(new ScriptBundle("~/zzzz").Include("~/totallyfake.notexist"));
            OptimizationSettings config = new OptimizationSettings() {
                ApplicationPath = TestContext.DeploymentDirectory,
                BundleTable = bundles
            };
            ExceptionHelper.ExpectException<FileNotFoundException>(() => { Optimizer.BuildBundle("~/zzzz", config); });
        }

        [TestMethod]
        public void OptimizerCssInTwoDirectoriesTest() {
            OptimizationSettings config = new OptimizationSettings() {
                ApplicationPath = TestContext.DeploymentDirectory,
                BundleSetupMethod = (bundles) => {
                    bundles.Add(new StyleBundle("~/bundles/css").Include("~/Styles/image.css", "~/Styles/nested/image2.css"));
                }
            };

            BundleResponse response = Optimizer.BuildBundle("~/bundles/css", config);
            Assert.IsNotNull(response);
            Assert.AreEqual(".image{background:url(images/ui.png)}.image2{background:url(images/ui2.png)}", response.Content);
            Assert.AreEqual(CssMinify.CssContentType, response.ContentType);
        }

        [TestMethod]
        public void BuildBundleRequiresBundlePathTest() {
            OptimizationSettings settings = new OptimizationSettings() {
                ApplicationPath = TestContext.DeploymentDirectory,
                BundleManifestPath = "bundle.config"
            };
            ExceptionHelper.ExpectArgumentExceptionNullOrEmpty(() => { Optimizer.BuildBundle("", settings); }, "bundlePath");
        }

        [TestMethod]
        public void BuildBundleRequiresApplicationPathTest() {
            OptimizationSettings settings = new OptimizationSettings() {
                BundleManifestPath = "bundle.config"
            };
            ExceptionHelper.ExpectArgumentExceptionNullOrEmpty(() => { Optimizer.BuildBundle("~/bundles/js", settings); }, "settings.ApplicationPath");
        }

        [TestMethod]
        public void OptimizerDynamicFolderBundleBadDirectory() {
            BundleCollection bundles = new BundleCollection();
            bundles.Add(new DynamicFolderBundle("js", "*.js", new JsMinify()));
            OptimizationSettings config = new OptimizationSettings() {
                ApplicationPath = TestContext.DeploymentDirectory,
                BundleTable = bundles
            };

            ExceptionHelper.ExpectException<InvalidOperationException>(() => { Optimizer.BuildBundle("~/foo/js", config); }, "Directory does not exist.");
        }

        private bool VerifyBundleResponse(BundleResponse expected, BundleResponse actual) {
            Assert.AreEqual(expected.Content, actual.Content);
            Assert.AreEqual(expected.ContentType, actual.ContentType);
            return true;
        }
    }
}
