// Copyright (c) Microsoft Corporation, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace System.Web.Optimization.Test {

    [TestClass]
    public class JsMinifyTest {
        [TestMethod]
        public void JsMinifyDoesNotMinifyInInstrumentationModeTest() {
            string js = "foo = bar;\r\nfoo = yes;";
            JsMinify jsmin = new JsMinify();
            BundleContext context = new BundleContext();
            context.EnableInstrumentation = true;
            BundleResponse response = new BundleResponse(js, null);
            response.Content = js;
            jsmin.Process(context, response);
            Assert.AreEqual(js, response.Content);
        }

        [TestMethod]
        public void JsMinifyRemovesCommentsNewLinesAndSpacesTest() {
            string js = "//I am a comment\r\nfoo = bar;\r\nfoo = yes;";
            JsMinify jsmin = new JsMinify();
            BundleContext context = new BundleContext();
            BundleResponse response = new BundleResponse(js, null);
            response.Content = js;
            jsmin.Process(context, response);
            Assert.AreEqual("foo=bar;foo=yes", response.Content);
        }

        [TestMethod]
        public void JsMinifyRemovesImportantCommentsTest() {
            string js = "/*!I am important */";
            JsMinify jsmin = new JsMinify();
            BundleContext context = new BundleContext();
            BundleResponse response = new BundleResponse(js, null);
            response.Content = js;
            jsmin.Process(context, response);
            Assert.AreEqual("", response.Content);
        }

        [TestMethod]
        public void JsMinifyContentTypeTest() {
            // Just to have a test failure if we change this header
            Assert.AreEqual("text/javascript", JsMinify.JsContentType);
        }

        [TestMethod]
        public void JsMinifyDoesNotRenameEvalMethods() {
            // Based of WebFormsUIValidation.js eval usage
            string js = @"function ValidatorOnLoad() {
    var i, val;
    for (i = 0; i < 10; i++) {
        val = i;
        eval(""val.evaluationfunction = "" + val.evaluationfunction + "";"");
    }
}";
            string minifiedJs = "function ValidatorOnLoad(){for(var val,i=0;i<10;i++)val=i,eval(\"val.evaluationfunction = \"+val.evaluationfunction+\";\")}";
            JsMinify jsmin = new JsMinify();
            BundleContext context = new BundleContext();
            BundleResponse response = new BundleResponse(js, null);
            response.Content = js;
            jsmin.Process(context, response);
            Assert.AreEqual(minifiedJs, response.Content);
        }
    }
}
