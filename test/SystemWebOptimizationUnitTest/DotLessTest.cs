// Copyright (c) Microsoft Corporation, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.VisualStudio.TestTools.UnitTesting;
using SystemWebOptimizationUnitTest.Util;

namespace System.Web.Optimization.Test {

    [TestClass]
    public class DotLessTest {
        [TestMethod]
        public void LessTransformTest() {
            //Setup the vpp to contain the files/directories
            TestVirtualPathProvider vpp = new TestVirtualPathProvider();
            var directory = new TestVirtualPathProvider.TestVirtualDirectory("/");
            directory.DirectoryFiles.Add(new TestVirtualPathProvider.TestVirtualFile("/test.less", @"@color: #111; h2 { color: @color; }"));
            vpp.AddDirectory(directory);

            // Setup the bundle
            Bundle bundle = new Bundle("~/bundles/test", new LessTransform(), new CssMinify());
            bundle.Items.VirtualPathProvider = vpp;
            bundle.Include("~/*.less");

            // Verify the bundle repsonse
            BundleContext context = BundleTest.SetupContext(bundle, vpp);
            BundleResponse response = bundle.GetBundleResponse(context);
            Assert.AreEqual(@"h2{color:#111}", response.Content);
        }

        public class LessTransform : IBundleTransform {
            public void Process(BundleContext context, BundleResponse response) {
                response.Content = dotless.Core.Less.Parse(response.Content);
                response.ContentType = "text/css"; 
            }
        }
    }
}
