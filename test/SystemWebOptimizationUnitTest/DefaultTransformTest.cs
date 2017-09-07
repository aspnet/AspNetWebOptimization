// Copyright (c) Microsoft Corporation, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.IO;
using System.Web.Hosting;

namespace System.Web.Optimization.Test {

    [TestClass]
    public class NoTransformTest {
        [TestMethod]
        public void NoContentTypeSetDoesNothingTest() {
            DefaultTransform no = new DefaultTransform();
            BundleContext context = new BundleContext();
            BundleResponse response = new BundleResponse(null, null);
            no.Process(context, response);
            Assert.AreEqual(null, response.Content);
            Assert.IsNull(response.ContentType);
        }

        [TestMethod]
        public void NoTransformContentTypeOverridesTest() {
            DefaultTransform no = new DefaultTransform("me");
            BundleContext context = new BundleContext();
            BundleResponse response = new BundleResponse(null, null);
            response.ContentType = "whatever";
            no.Process(context, response);
            Assert.AreEqual(null, response.Content);
            Assert.AreEqual("me", response.ContentType);
        }

        public class MyVirtualFile : VirtualFile {
            public MyVirtualFile(string path) : base(path) {
            }

            public override Stream Open() {
                throw new NotImplementedException();
            }
        }


        [TestMethod]
        public void InferJsContentTypeTest() {
            DefaultTransform no = new DefaultTransform();
            BundleContext context = new BundleContext();
            List<BundleFile> files = new List<BundleFile>();
            files.Add(new BundleFile("~/foo.js", new MyVirtualFile("foo.js")));
            BundleResponse response = new BundleResponse(null, files);
            no.Process(context, response);
            Assert.AreEqual(null, response.Content);
            Assert.AreEqual(JsMinify.JsContentType, response.ContentType);
        }

        [TestMethod]
        public void InferCssContentTypeTest() {
            DefaultTransform no = new DefaultTransform();
            BundleContext context = new BundleContext();
            List<BundleFile> files = new List<BundleFile>();
            files.Add(new BundleFile("~/foo.js", new MyVirtualFile("foo.css")));
            BundleResponse response = new BundleResponse(null, files);
            no.Process(context, response);
            Assert.AreEqual(null, response.Content);
            Assert.AreEqual(CssMinify.CssContentType, response.ContentType);
        }

        [TestMethod]
        public void DoesntInferCssContentTypeWhenAlreadySetTest() {
            DefaultTransform no = new DefaultTransform();
            BundleContext context = new BundleContext();
            List<BundleFile> files = new List<BundleFile>();
            files.Add(new BundleFile("~/foo.js", new MyVirtualFile("foo.css")));
            BundleResponse response = new BundleResponse(null, files);
            response.ContentType = "whatever";
            no.Process(context, response);
            Assert.AreEqual(null, response.Content);
            Assert.AreEqual("whatever", response.ContentType);
        }

        [TestMethod]
        public void DoesntInferJsContentTypeWhenAlreadySetTest() {
            DefaultTransform no = new DefaultTransform();
            BundleContext context = new BundleContext();
            List<BundleFile> files = new List<BundleFile>();
            files.Add(new BundleFile("~/foo.js", new MyVirtualFile("foo.js")));
            BundleResponse response = new BundleResponse(null, files);
            response.ContentType = "whatever";
            no.Process(context, response);
            Assert.AreEqual(null, response.Content);
            Assert.AreEqual("whatever", response.ContentType);
        }

        [TestMethod]
        public void EmptyTransformsListUsesNoTransformTest() {
            Bundle b = new Bundle("~/foo");
            BundleContext context = new BundleContext();
            List<BundleFile> files = new List<BundleFile>();
            files.Add(new BundleFile("~/foo.js", new MyVirtualFile("foo.js")));
            BundleResponse response = b.ApplyTransforms(context, null, files);
            Assert.AreEqual(null, response.Content);
            Assert.AreEqual(JsMinify.JsContentType, response.ContentType);
        }
    }
}
