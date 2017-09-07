// Copyright (c) Microsoft Corporation, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Web.Hosting;

namespace SystemWebOptimizationUnitTest.Util {
    public class TestVirtualPathProvider : VirtualPathProvider {

        private string NormalizeVirtualPath(string virtualPath, bool isDirectory = false) {
            if (!virtualPath.StartsWith("~")) {
                virtualPath = "~" + virtualPath;
            }
            virtualPath = virtualPath.Replace('\\', '/');
            // Normalize directories to always have an ending "/"
            if (isDirectory && !virtualPath.EndsWith("/")) {
                return virtualPath + "/";
            }
            return virtualPath;
        }

        // Files on disk (virtualPath -> file)
        private Dictionary<string, VirtualFile> _fileMap = new Dictionary<string, VirtualFile>();
        private Dictionary<string, VirtualFile> FileMap {
            get { return _fileMap; }
        }

        public void AddFile(VirtualFile file) {
            FileMap[NormalizeVirtualPath(file.VirtualPath)] = file;
        }

        private Dictionary<string, VirtualDirectory> _directoryMap = new Dictionary<string, VirtualDirectory>();
        private Dictionary<string, VirtualDirectory> DirectoryMap {
            get { return _directoryMap; }
        }

        public void AddDirectory(VirtualDirectory dir) {
            DirectoryMap[NormalizeVirtualPath(dir.VirtualPath, isDirectory: true)] = dir;
        }

        public override bool FileExists(string virtualPath) {
            return FileMap.ContainsKey(NormalizeVirtualPath(virtualPath));
        }

        public override bool DirectoryExists(string virtualDir) {
            return DirectoryMap.ContainsKey(NormalizeVirtualPath(virtualDir, isDirectory: true));
        }

        public override VirtualFile GetFile(string virtualPath) {
            return FileMap[NormalizeVirtualPath(virtualPath)];
        }

        public override VirtualDirectory GetDirectory(string virtualDir) {
            return DirectoryMap[NormalizeVirtualPath(virtualDir, isDirectory: true)];
        }

        internal class TestVirtualFile : VirtualFile {
            public TestVirtualFile(string virtualPath, string contents)
                : base(virtualPath) {
                Contents = contents;
            }

            public string Contents { get; set; }

            public override Stream Open() {
                return new MemoryStream(UTF8Encoding.Default.GetBytes(Contents));
            }
        }

        internal class TestVirtualDirectory : VirtualDirectory {
            public TestVirtualDirectory(string virtualPath)
                : base(virtualPath) {
            }

            public List<VirtualFile> _directoryFiles = new List<VirtualFile>();
            public List<VirtualFile> DirectoryFiles {
                get {
                    return _directoryFiles;
                }
            }

            public List<VirtualDirectory> _subDirs = new List<VirtualDirectory>();
            public List<VirtualDirectory> SubDirectories {
                get {
                    return _subDirs;
                }
            }

            public override IEnumerable Files {
                get {
                    return DirectoryFiles;
                }
            }

            public override IEnumerable Children {
                get { throw new NotImplementedException(); }
            }

            public override IEnumerable Directories {
                get { 
                    return SubDirectories;
                }
            }
        }
    }
}
