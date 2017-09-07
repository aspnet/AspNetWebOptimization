// Copyright (c) Microsoft Corporation, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace System.Web.Optimization.Test {
    // Used to bypass bundling, just return this bundle response directly
    public class CacheHitBundle : Bundle {
        public BundleResponse BundleResponse { get; set; }

        public CacheHitBundle(string path, BundleResponse response)
            : base(path) {
            BundleResponse = response;
        }

        public override BundleResponse CacheLookup(BundleContext context) {
            return BundleResponse;
        }
    }
}
