﻿#region BSD license
// Copyright 2010 Chris Dibbs. All rights reserved.
//
// Redistribution and use in source and binary forms, with or without modification, are
// permitted provided that the following conditions are met:
//
//   1. Redistributions of source code must retain the above copyright notice, this list of
//      conditions and the following disclaimer.
//
//   2. Redistributions in binary form must reproduce the above copyright notice, this list
//      of conditions and the following disclaimer in the documentation and/or other materials
//      provided with the distribution.
//
// THIS SOFTWARE IS PROVIDED BY Chris Dibbs ``AS IS'' AND ANY EXPRESS OR IMPLIED
// WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND
// FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL Chris Dibbs OR
// CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR
// CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR
// SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON
// ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING
// NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF
// ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
//
// The views and conclusions contained in the software and documentation are those of the
// authors and should not be interpreted as representing official policies, either expressed
// or implied, of Chris Dibbs.
#endregion
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Configuration;

namespace SharePointPrimitives.SettingsProvider.Stsadm {
    public static class ReflectionExtensions {
        public static IEnumerable<AttributeT> GetCustomAttributes<AttributeT>(this ICustomAttributeProvider provider, bool inherit) {
            return provider.GetCustomAttributes(typeof(AttributeT),inherit).OfType<AttributeT>();
        }

        public static AttributeT GetCustomAttribute<AttributeT>(this ICustomAttributeProvider provider, bool inherit) {
            return provider.GetCustomAttributes(typeof(AttributeT), inherit).OfType<AttributeT>().FirstOrDefault();
        }

        public static bool HasCustomAttribute<AttributeT>(this ICustomAttributeProvider provider, bool inherit) {
            return provider.GetCustomAttributes(typeof(AttributeT), inherit).OfType<AttributeT>().Any();

        }

        public static string DefaultValue(this PropertyInfo info) {
            string value = null;

            DefaultSettingValueAttribute defaultValue = info.GetCustomAttribute<DefaultSettingValueAttribute>(true);
            if (defaultValue != null)
                value = defaultValue.Value;
            return value;
        }

        public static bool IsConnectionString(this PropertyInfo setting) {
            SpecialSettingAttribute special = setting.GetCustomAttribute<SpecialSettingAttribute>(true);
            return special != null && special.SpecialSetting == SpecialSetting.ConnectionString;
        }
    }
}
