using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Terrascape.Debugging;

#nullable enable

namespace Terrascape.Registry
{
	[SuppressMessage("ReSharper", "VirtualMemberNeverOverridden.Global")]
	[SuppressMessage("ReSharper", "UnusedMember.Global")]
	[SuppressMessage("ReSharper", "MemberCanBeProtected.Global")]
	public abstract class Registry<T> where T : class, IIdentifiable
	{
		private readonly Dictionary<Identifier, T> registry = new Dictionary<Identifier, T>();
		private readonly string type_name;

		protected Registry()
		{
			this.type_name = typeof(T).Name;
		}

		#region IsRegistered
		protected bool ActualIsRegistered(in Identifier p_identifier)
		{
			if (p_identifier == null || string.IsNullOrEmpty(p_identifier.ToString()))
				return false;
			
			return this.registry.ContainsKey(p_identifier);
		}
		
		protected bool ActualIsRegistered(in T p_object)
		{
			if (p_object == null || string.IsNullOrEmpty(p_object.name))
				return false;
			
			return this.registry.ContainsKey(p_object.name);
		}

		public virtual bool IsRegistered(in Identifier p_identifier)
		{
			return ActualIsRegistered(p_identifier);
		}
		
		public virtual bool IsRegistered(in T p_object)
		{
			return ActualIsRegistered(p_object);
		}
		#endregion
		
		#region Register
		protected void ActualRegister(in T p_object)
		{
			if (ActualIsRegistered(p_object))
				throw new ApplicationException($"Cannot register {this.type_name} '{p_object.name}' to the {this.type_name} registry because a {this.type_name} is already registered under this identifier"); // TODO(LOGIX): DuplicateRegistryEntryException, A-or-an
			
			this.registry.Add(p_object.name, p_object);
			Debug.LogDebug($"Registered {this.type_name} '{p_object.name}' to the {this.type_name} registry", DebuggingLevel.Verbose);
		}
		
		public virtual void Register(in T p_object)
		{
			ActualRegister(p_object);
		}
		#endregion
		
		#region Get
		protected T ActualGet(in Identifier p_identifier)
		{
			if (IsRegistered(p_identifier))
				return this.registry[p_identifier];
			
			throw new ApplicationException($"Cannot find {this.type_name} '{p_identifier}' in the {this.type_name} registry"); // TODO(LOGIX): Custom exception type
		}

		protected T? ActualGetOrNull(in Identifier p_identifier)
		{
			return IsRegistered(p_identifier) ? this.registry[p_identifier] : null;
		}

		public T Get(in Identifier p_identifier)
		{
			return ActualGet(p_identifier);
		}

		public T? GetOrNull(in Identifier p_identifier)
		{
			return ActualGetOrNull(p_identifier);
		}
		#endregion
	}
}