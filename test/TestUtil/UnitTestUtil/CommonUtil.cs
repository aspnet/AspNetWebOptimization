// Copyright (c) Microsoft Corporation, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Reflection;
using System.Security;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Security.Permissions;
using System.Diagnostics;
using System.IO;

namespace Microsoft.Web.UnitTest {
    public static class CommonUtil {
        public const BindingFlags AllFlags =
            BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic;
        private const string _systemWebAssemblyName =
            ", System.Web, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a";
        private const string _systemDesignAssemblyName =
            ", System.Design, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a";

        public static PermissionSet MinimalPermissionSet {
            get {
                PermissionSet pset = new PermissionSet(PermissionState.None);
                return pset;
            }
        }

        public static T GetCustomAttribute<T>(this ICustomAttributeProvider attributeProvider)
            where T : Attribute {

            T[] attributes = GetCustomAttributes<T>(attributeProvider);
            if (attributes.Length == 0) {
                return null;
            }

            Debug.Assert(attributes.Length == 1);

            return attributes[0];
        }

        public static T[] GetCustomAttributes<T>(this ICustomAttributeProvider attributeProvider)
            where T : Attribute {

            return (T[])attributeProvider.GetCustomAttributes(typeof(T), false);
        }

        public static object GetFieldValue(object obj, string name) {
            return GetOrSetFieldOrPropertyValue(obj, obj.GetType(), name, null, null, null, GetOrSet.Get, FieldOrProperty.Field);
        }

        public static object GetFieldValue<TStatic>(string name) {
            return GetFieldValue(typeof(TStatic), name);
        }

        public static object GetFieldValue(Type type, string name) {
            return GetOrSetFieldOrPropertyValue(null, type, name, null, null, null, GetOrSet.Get, FieldOrProperty.Field);
        }

        public static object GetPropertyValue(object obj, string name) {
            return GetPropertyValue(obj, name, null);
        }

        public static object GetPropertyValue(object obj, string name, object[] index) {
            return GetPropertyValue(obj, name, index, null);
        }

        public static object GetPropertyValue(object obj, string name, object[] index, Type[] types) {
            return GetOrSetFieldOrPropertyValue(obj, obj.GetType(), name, types, index, null, GetOrSet.Get, FieldOrProperty.Property);
        }

        public static object GetPropertyValue(Type type, string name) {
            return GetPropertyValue(type, name, null);
        }

        public static object GetPropertyValue(Type type, string name, object[] index) {
            return GetPropertyValue(type, name, index, null);
        }

        public static object GetPropertyValue(Type type, string name, object[] index, Type[] types) {
            return GetOrSetFieldOrPropertyValue(null, type, name, types, index, null, GetOrSet.Get, FieldOrProperty.Property);
        }

        private static object GetOrSetFieldOrPropertyValue(object obj, Type objType, string name, Type[] types, object[] index,
                                                           object value, GetOrSet getOrSet, FieldOrProperty fieldOrProperty) {
           if (types == null) {
               types = new Type[0];
           }
            
            try {
                for (Type type = objType; type != null; type = type.BaseType) {
                    if (getOrSet == GetOrSet.Get) {
                        if (fieldOrProperty == FieldOrProperty.Field) {
                            FieldInfo fieldInfo = type.GetField(name, AllFlags);
                            if (fieldInfo != null) {
                                return fieldInfo.GetValue(obj);
                            }
                        }
                        else {
                            PropertyInfo propertyInfo = type.GetProperty(name, AllFlags, null, null, types, null);
                            if (propertyInfo != null) {
                                return propertyInfo.GetValue(obj, index);
                            }
                        }
                    }
                    else {
                        if (fieldOrProperty == FieldOrProperty.Field) {
                            FieldInfo fieldInfo = type.GetField(name, AllFlags);
                            if (fieldInfo != null) {
                                fieldInfo.SetValue(obj, value);
                                return null;
                            }
                        }
                        else {
                            PropertyInfo propertyInfo = type.GetProperty(name, AllFlags, null, null, types, null);
                            if (propertyInfo != null) {
                                propertyInfo.SetValue(obj, value, index);
                                return null;
                            }
                        }
                    }
                }
            }
            catch (TargetInvocationException e) {
                ThrowInnerException(e);
            }
            throw new ArgumentException("Cannot find " + fieldOrProperty.ToString().ToLower() + ": " + name, "name");
        }

        // Gets the Type object that represents the specified name.  Automatically appends
        // the assembly name to known namespaces.
        public static Type GetType(string name) {
            if (name.StartsWith("System.Web.UI.Design", StringComparison.InvariantCulture) ||
                name.StartsWith("System.Design", StringComparison.InvariantCulture)) {
                name += _systemDesignAssemblyName;
            }
            else if (name.StartsWith("System.Web")) {
                name += _systemWebAssemblyName;
            }
            return Type.GetType(name, true);
        }

        public static object InvokeMethod(object obj, string name, params object[] parameters) {
            return InvokeMethod(obj, name, null, parameters);
        }

        public static object InvokeMethod(object obj, string name, Type[] types, params object[] parameters) {
            return InvokeMethod(obj, obj.GetType(), name, AllFlags, types, parameters);
        }

        public static object InvokeMethod(Type type, string name, params object[] parameters) {
            return InvokeMethod(type, name, null, parameters);
        }

        public static object InvokeMethod(Type type, string name, Type[] types, params object[] parameters) {
            if (type == null) {
                throw new ArgumentNullException("type");
            }
            return InvokeMethod(null, type, name, AllFlags, types, parameters);
        }

        private static object InvokeMethod(object obj, Type objType, string name, BindingFlags bindingAttr,
                                           Type[] types, params object[] parameters) {
            try {
                for (Type type = objType; type != null; type = type.BaseType) {
                    MethodInfo methodInfo;
                    if (types == null) {
                        methodInfo = type.GetMethod(name, bindingAttr);
                    }
                    else {
                        methodInfo = type.GetMethod(name, bindingAttr, null, types, null);
                    }
                    if (methodInfo != null) {
                        return methodInfo.Invoke(obj, parameters);
                    }
                }
            }
            catch (TargetInvocationException e) {
                ThrowInnerException(e);
            }
            throw new ArgumentException("Cannot find method " + name, "name");
        }

        public static void SetFieldValue(object obj, string name, object value) {
            GetOrSetFieldOrPropertyValue(obj, obj.GetType(), name, null, null, value, GetOrSet.Set, FieldOrProperty.Field);
        }

        public static void SetFieldValue<TStatic>(string name, object value) {
            SetFieldValue(typeof(TStatic), name, value);
        }

        public static void SetFieldValue(Type type, string name, object value) {
            GetOrSetFieldOrPropertyValue(null, type, name, null, null, value, GetOrSet.Set, FieldOrProperty.Field);
        }

        public static void SetPropertyValue(object obj, string name, object value) {
            SetPropertyValue(obj, name, value, null);
        }

        public static void SetPropertyValue(object obj, string name, object value, object[] index) {
            SetPropertyValue(obj, name, value, index, null);
        }
        
        public static void SetPropertyValue(object obj, string name, object value, object[] index, Type[] types) {
            GetOrSetFieldOrPropertyValue(obj, obj.GetType(), name, types, index, value, GetOrSet.Set, FieldOrProperty.Property);
        }

        public static void SetPropertyValue<TStatic>(string name, object value) {
            SetPropertyValue(typeof(TStatic), name, value);
        }

        public static void SetPropertyValue(Type type, string name, object value) {
            SetPropertyValue(type, name, value, null);
        }
        
        public static void SetPropertyValue(Type type, string name, object value, object[] index) {
            SetPropertyValue(type, name, value, index, null);
        }

        public static void SetPropertyValue(Type type, string name, object value, object[] index, Type[] types) {
            GetOrSetFieldOrPropertyValue(null, type, name, types, index, value, GetOrSet.Set, FieldOrProperty.Property);
        }

        private static void ThrowInnerException(Exception e) {
            // Set the private _remoteStackTraceString on the inner exception so the stack trace is from
            // the original location it was thrown.
            // Hack taken from: http://dotnetjunkies.com/WebLog/chris.taylor/archive/2004/03/03/8353.aspx
            SetFieldValue(e.InnerException, "_remoteStackTraceString", e.InnerException.StackTrace);
            throw e.InnerException;
        }

        private enum GetOrSet {
            Get, Set
        }

        private enum FieldOrProperty {
            Field, Property
        }
    }
}
