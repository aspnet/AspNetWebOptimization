// Copyright (c) Microsoft Corporation, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace System.Web.Optimization.Test {

    [TestClass]
    public class CssMinifyTest {
        [TestMethod]
        public void CssMinifyDoesNotMinifyInInstrumentationModeTest() {
            string css = "body\r\n{ }";
            CssMinify cssmin = new CssMinify();
            BundleContext context = new BundleContext();
            context.EnableInstrumentation = true;
            BundleResponse response = new BundleResponse(css, null);
            cssmin.Process(context, response);
            Assert.AreEqual(css, response.Content);
        }

        [TestMethod]
        public void CssMinifyRemovesWhitespaceTest() {
            string css = "body\r\n{color : blue }";
            CssMinify cssmin = new CssMinify();
            BundleContext context = new BundleContext();
            BundleResponse response = new BundleResponse(css, null);
            cssmin.Process(context, response);
            Assert.AreEqual("body{color:blue}", response.Content);
        }

        [TestMethod]
        public void CssMinifyRemovesImportantCommentsTest() {
            string css = "/*!I am important */";
            CssMinify cssmin = new CssMinify();
            BundleContext context = new BundleContext();
            BundleResponse response = new BundleResponse(css, null);
            cssmin.Process(context, response);
            Assert.AreEqual("", response.Content);
        }

        [TestMethod]
        public void CssMinifyContentTypeTest() {
            // Just to have a test failure if we change this header
            Assert.AreEqual("text/css", CssMinify.CssContentType);
        }
    }
}
