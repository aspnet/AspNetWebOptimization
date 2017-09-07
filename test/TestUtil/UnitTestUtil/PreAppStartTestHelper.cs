// Copyright (c) Microsoft Corporation, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.ComponentModel;
using System.Reflection;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.Web.UnitTest {
    public static class PreAppStartTestHelper {
        public static void TestPreAppStartClass(Type preAppStartType) {
            Assert.IsTrue(preAppStartType.IsSealed && preAppStartType.IsAbstract, "The type '{0}' should be static.", preAppStartType.FullName);
            
            Assert.IsTrue(preAppStartType.IsPublic, "The type '{0}' should be public.", preAppStartType.FullName);

            Assert.AreEqual("PreApplicationStartCode", preAppStartType.Name, "The type '{0}' has the wrong name.", preAppStartType.FullName);

            object[] attrs = preAppStartType.GetCustomAttributes(typeof(EditorBrowsableAttribute), true);
            Assert.AreEqual(1, attrs.Length, "The type '{0}' should have [EditorBrowsable(EditorBrowsableState.Never)] applied to it.", preAppStartType.FullName);
            EditorBrowsableAttribute editorAttr = (EditorBrowsableAttribute)attrs[0];
            Assert.AreEqual(EditorBrowsableState.Never, editorAttr.State, "The type '{0}' should have [EditorBrowsable(EditorBrowsableState.Never)] applied to it.", preAppStartType.FullName);

            MemberInfo[] publicMembers = preAppStartType.GetMembers(BindingFlags.Public | BindingFlags.Static);
            Assert.AreEqual(1, publicMembers.Length, "The type '{0}' should have only one public member.", preAppStartType.FullName);
            Assert.AreEqual(MemberTypes.Method, publicMembers[0].MemberType, "The only public member on type '{0}' should be a method called Start().", preAppStartType.FullName);
            Assert.AreEqual("Start", publicMembers[0].Name, "The only public member on type '{0}' should be a method called Start().", preAppStartType.FullName);
        }
    }
}
