// Copyright (c) Microsoft Corporation, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Web.TestUtil;

namespace System.Web.Optimization.Test {

    [TestClass]
    public class IgnoreListTest {
        [TestMethod]
        public void IgnoreAllTest() {
            IgnoreList list = new IgnoreList();
            list.Ignore("*");
            BundleContext context = new BundleContext();

            Assert.IsTrue(list.ShouldIgnore(context, "whatever"));
            Assert.IsTrue(list.ShouldIgnore(context, "adlfkjadf"));
        }

        [TestMethod]
        public void OptimizationEnabledIgnoreModesTest() {
            IgnoreList list = new IgnoreList();
            BundleContext context = new BundleContext();
            context.EnableOptimizations = true;
            list.Ignore("unoptimized", OptimizationMode.WhenDisabled);
            list.Ignore("optimized", OptimizationMode.WhenEnabled);
            list.Ignore("always", OptimizationMode.Always);

            Assert.IsFalse(list.ShouldIgnore(context, "unoptimized"));
            Assert.IsTrue(list.ShouldIgnore(context, "optimized"));
            Assert.IsTrue(list.ShouldIgnore(context, "always"));
        }

        [TestMethod]
        public void IgnoreUsingRegexPatternTest() {
            IgnoreList list = new IgnoreList();
            list.Ignore("jquery-{version}.js");
            BundleContext context = new BundleContext();

            Assert.IsTrue(list.ShouldIgnore(context, "jquery-1.0.js"));
            Assert.IsTrue(list.ShouldIgnore(context, "jquery-10.0.2.js"));
            Assert.IsTrue(list.ShouldIgnore(context, "JQueRY-10.0.2.js"));
            Assert.IsFalse(list.ShouldIgnore(context, "jquery-1.js"));
            Assert.IsFalse(list.ShouldIgnore(context, "jquery-1.0"));
            Assert.IsFalse(list.ShouldIgnore(context, "jquery.js"));
        }

        [TestMethod]
        public void IgnoreValidatesIgnoreModeTest() {
            ExceptionHelper.ExpectArgumentException(delegate { new IgnoreList().Ignore("doh", (OptimizationMode)4); }, "Invalid OptimizationMode value, valid values are: Always, WhenEnabled, WhenDisabled.\r\nParameter name: mode");
        }

        [TestMethod]
        public void IgnoreValidatesPatternTest() {
            ExceptionHelper.ExpectArgumentExceptionNullOrEmpty(delegate { new IgnoreList().Ignore(null, (OptimizationMode)4); }, "pattern");
        }

        [TestMethod]
        public void OptimizationNotEnabledIgnoreModesTest() {
            IgnoreList list = new IgnoreList();
            BundleContext context = new BundleContext();
            context.EnableOptimizations = false;
            list.Ignore("unoptimized", OptimizationMode.WhenDisabled);
            list.Ignore("optimized", OptimizationMode.WhenEnabled);
            list.Ignore("always", OptimizationMode.Always);

            Assert.IsTrue(list.ShouldIgnore(context, "unoptimized"));
            Assert.IsFalse(list.ShouldIgnore(context, "optimized"));
            Assert.IsTrue(list.ShouldIgnore(context, "always"));
        }

        [TestMethod]
        public void IgnoreExactWhenUnoptimizedDoesNotAffectOtherModesTest() {
            IgnoreList list = new IgnoreList();
            BundleContext context = new BundleContext();
            context.EnableOptimizations = true;
            list.Ignore("*", OptimizationMode.WhenDisabled);
            Assert.IsFalse(list.ShouldIgnore(context, "whatever"));
        }

        [TestMethod]
        public void ClearTest() {
            BundleContext context = new BundleContext();
            IgnoreList list = new IgnoreList();
            list.Ignore("*");
            list.Ignore("*.ignore");
            list.Ignore("_*");
            list.Ignore("ignore.me");

            Assert.IsTrue(list.ShouldIgnore(context, ".ignore"));
            Assert.IsTrue(list.ShouldIgnore(context, "blah.IGNore"));
            Assert.IsTrue(list.ShouldIgnore(context, "_whatever"));
            Assert.IsTrue(list.ShouldIgnore(context, "IGNORE.me"));

            list.Clear();
            Assert.IsFalse(list.ShouldIgnore(context, ".ignore"));
            Assert.IsFalse(list.ShouldIgnore(context, "blah.IGNore"));
            Assert.IsFalse(list.ShouldIgnore(context, "_whatever"));
            Assert.IsFalse(list.ShouldIgnore(context, "IGNORE.me"));
        }

        [TestMethod]
        public void NoWildcardsInMiddleTest() {
            ExceptionHelper.ExpectArgumentException(delegate { new IgnoreList().Ignore("a*b"); }, String.Format(PatternHelperTest.InvalidPatternError, "a*b", "item"));
        }

        [TestMethod]
        public void NoExtraLeadingWildcardsTest() {
            ExceptionHelper.ExpectArgumentException(delegate { new IgnoreList().Ignore("**a"); }, String.Format(PatternHelperTest.InvalidPatternError, "**a", "item"));
        }

        [TestMethod]
        public void NoExtraTrailingWildcardsTest() {
            ExceptionHelper.ExpectArgumentException(delegate { new IgnoreList().Ignore("a**"); }, String.Format(PatternHelperTest.InvalidPatternError, "a**", "item"));
        }

        [TestMethod]
        public void AlwaysIgnoreNullEmptyTest() {
            BundleContext context = new BundleContext();
            IgnoreList list = new IgnoreList();
            Assert.IsTrue(list.ShouldIgnore(context, null));
            Assert.IsTrue(list.ShouldIgnore(context, ""));
        }

        [TestMethod]
        public void IgnoreNoneTest() {
            BundleContext context = new BundleContext();
            IgnoreList list = new IgnoreList();

            Assert.IsFalse(list.ShouldIgnore(context, "whatever"));
            Assert.IsFalse(list.ShouldIgnore(context, "adlfkjadf"));
        }

        [TestMethod]
        public void IgnoreExactWithDifferentCasingTest() {
            BundleContext context = new BundleContext();
            IgnoreList list = new IgnoreList();
            list.Ignore("IGnore.foo");

            Assert.IsFalse(list.ShouldIgnore(context, "whatever"));
            Assert.IsFalse(list.ShouldIgnore(context, "adlfkjadf"));
            Assert.IsFalse(list.ShouldIgnore(context, "FooIGnore.Bar"));
            Assert.IsTrue(list.ShouldIgnore(context, "IGNORE.FOO"));
            Assert.IsTrue(list.ShouldIgnore(context, "ignore.FOO"));
            Assert.IsTrue(list.ShouldIgnore(context, "ignore.foo"));
        }

        [TestMethod]
        public void IgnorePrefixWithNoDotTest() {
            BundleContext context = new BundleContext();
            IgnoreList list = new IgnoreList();
            list.Ignore("*ignore");

            Assert.IsFalse(list.ShouldIgnore(context, "ignore.foo"));
            Assert.IsTrue(list.ShouldIgnore(context, "whateverignore"));
            Assert.IsTrue(list.ShouldIgnore(context, "whatever.ignore"));
            Assert.IsTrue(list.ShouldIgnore(context, "whatever.ignore.ignore.ignore"));
            Assert.IsTrue(list.ShouldIgnore(context, "whatever.no.way.ignore"));
        }

        [TestMethod]
        public void IgnoreSuffixWithDifferentCasingTest() {
            BundleContext context = new BundleContext();
            IgnoreList list = new IgnoreList();
            list.Ignore("IGnore.*");

            Assert.IsFalse(list.ShouldIgnore(context, "signore"));
            Assert.IsFalse(list.ShouldIgnore(context, "adlfkjadf"));
            Assert.IsTrue(list.ShouldIgnore(context, "IGNORE.FOO"));
            Assert.IsTrue(list.ShouldIgnore(context, "ignore.FOO"));
            Assert.IsTrue(list.ShouldIgnore(context, "ignore.foo"));
            Assert.IsTrue(list.ShouldIgnore(context, "ignore.foo.bar"));
            Assert.IsTrue(list.ShouldIgnore(context, "iGNore."));
        }

        [TestMethod]
        public void IgnorePrefixWithDifferentCasingTest() {
            BundleContext context = new BundleContext();
            IgnoreList list = new IgnoreList();
            list.Ignore("*.ignore");

            Assert.IsFalse(list.ShouldIgnore(context, ".aignore"));
            Assert.IsFalse(list.ShouldIgnore(context, "aignoredlfkjadf"));
            Assert.IsFalse(list.ShouldIgnore(context, "ignore"));
            Assert.IsTrue(list.ShouldIgnore(context, "foo.vsfoo.ignore"));
            Assert.IsTrue(list.ShouldIgnore(context, "foo.IGNore"));
            Assert.IsTrue(list.ShouldIgnore(context, ".ignore"));
            Assert.IsTrue(list.ShouldIgnore(context, "blah.ignore"));
        }

        [TestMethod]
        public void SomeOfEachAllTogetherTest() {
            BundleContext context = new BundleContext();
            IgnoreList list = new IgnoreList();
            list.Ignore("*.ignore");
            list.Ignore("_*");
            list.Ignore("ignore.me");

            Assert.IsFalse(list.ShouldIgnore(context, "hao.kung"));
            Assert.IsTrue(list.ShouldIgnore(context, ".ignore"));
            Assert.IsTrue(list.ShouldIgnore(context, "blah.IGNore"));
            Assert.IsTrue(list.ShouldIgnore(context, "_whatever"));
            Assert.IsTrue(list.ShouldIgnore(context, "IGNORE.me"));
        }

    }
}
