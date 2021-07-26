// Copyright 2021 Ammo Goettsch
// 
// Helios is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// Helios is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with this program.  If not, see <http://www.gnu.org/licenses/>.
// 

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using GadrocsWorkshop.Helios.Controls.Capabilities;

namespace GadrocsWorkshop.Helios
{
    internal class ProfileInstances : IDisposable
    {
        private readonly Dictionary<Type, object> _instances = new Dictionary<Type, object>();

        public void Attach(HeliosObject client)
        {
            foreach (Type interfaceType in GetSupportedInterfaces(client))
            {
                Type instanceType = InstanceType(interfaceType);
                if (!_instances.TryGetValue(instanceType, out object instance))
                {
                    instance = Activator.CreateInstance(instanceType);
                    _instances[instanceType] = instance;
                }
                AttachToProfileInstance(interfaceType).Invoke(client, new[] { instance });
            }
        }

        public void Detach(HeliosObject client)
        {
            foreach (Type interfaceType in GetSupportedInterfaces(client))
            {
                Type instanceType = InstanceType(interfaceType);
                if (_instances.TryGetValue(instanceType, out object instance))
                {
                    DetachFromProfileInstance(interfaceType).Invoke(client, new[] {instance});
                }
            }
        }

        private static IEnumerable<Type> GetSupportedInterfaces(HeliosObject client)
        {
            return client.GetType().GetInterfaces()
                .Where(x =>
                    x.IsGenericType &&
                    x.GetGenericTypeDefinition() == typeof(IControlWithProfileInstance<>) &&
                    x.GenericTypeArguments.Length == 1);
        }

        private static Type InstanceType(Type interfaceType) => interfaceType.GenericTypeArguments[0];

        private static MethodInfo AttachToProfileInstance(Type interfaceType)
        {
            string methodName = nameof(IControlWithProfileInstance<object>.AttachToProfileInstance);
            Type instanceType = InstanceType(interfaceType);
            MethodInfo methodInfo = interfaceType.GetMethod(methodName, new[] { instanceType });
            if (methodInfo == null)
            {
                throw new Exception(
                    $"missing method AttachToProfileInstance in class that implements interface IControlWithProfileInstance<{instanceType.Name}>");
            }
            return methodInfo;
        }

        private static MethodInfo DetachFromProfileInstance(Type interfaceType)
        {
            string methodName = nameof(IControlWithProfileInstance<object>.DetachFromProfileInstance);
            Type instanceType = InstanceType(interfaceType);
            MethodInfo methodInfo = interfaceType.GetMethod(methodName, new[] {instanceType});
            if (methodInfo == null)
            {
                throw new Exception(
                    $"missing method DetachFromProfileInstance in class that implements interface IControlWithProfileInstance<{instanceType.Name}>");
            }

            return methodInfo;
        }

        #region IDisposable

        public void Dispose()
        {
            foreach (IDisposable instance in _instances.Values.OfType<IDisposable>())
            {
                instance.Dispose();
            }

            _instances.Clear();
        }

        #endregion
    }
}
