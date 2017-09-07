// Copyright (c) Microsoft Corporation, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.IO;
using System.Web.Optimization;

namespace System.Web.Optimization.Test {
    public class UppercaseTransform : IItemTransform {
        public string Process(string itemVirtualPath, string input) {
            return input.ToUpperInvariant();
        }
    }

    public class AppendTransform : IBundleTransform {
        private string _txt;
        public AppendTransform(string txt) {
            _txt = txt;
        }

        public void Process(BundleContext context, BundleResponse response) {
            response.Content += _txt;
        }
    }
}
