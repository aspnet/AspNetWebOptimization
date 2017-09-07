// Copyright (c) Microsoft Corporation, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;

namespace System.Web.Optimization.Test {
    // Test resolver that treats all virtual paths that end with bundle as bundles, and returns 3 fixed files
    public class TestBundleResolver : IBundleResolver {
        public bool IsBundleVirtualPath(string virtualPath) {
            return virtualPath.EndsWith("Bundle");
        }
        public IEnumerable<string> GetBundleContents(string virtualPath) {
            return new string[] { "~/bundleFile1", "~/bundleFile2", "~/bundleFile3" };
        }
        public string GetBundleUrl(string virtualPath) {
            return virtualPath + "?version";
        }
    }
}
