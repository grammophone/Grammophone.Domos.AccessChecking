using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Grammophone.Domos.Domain;
using Grammophone.Domos.AccessChecking.Configuration;

namespace Grammophone.Domos.AccessChecking
{
	/// <summary>
	/// Provides the entity access rights that are assigned to 
	/// a disposition type, a role or a user.
	/// It is an aggregation of <see cref="EntityAccess"/> entries.
	/// </summary>
	public class AccessRight
	{
		#region Private fields

		/// <summary>
		/// Holds a 'deny all' response.
		/// </summary>
		private static EntityRight nullEntityRight = new EntityRight();

		/// <summary>
		/// Collection of manager class names.
		/// </summary>
		private HashSet<Type> managerTypes = new HashSet<Type>();

		/// <summary>
		/// Collection of state path code names.
		/// </summary>
		private HashSet<string> statePathCodeNames = new HashSet<string>();

		/// <summary>
		/// Map of entity rights by entity class names.
		/// </summary>
		private Dictionary<string, EntityRight> entityRights = new Dictionary<string, EntityRight>();

		#endregion

		#region Public properties

		/// <summary>
		/// The set of managers, defined by their class names.
		/// </summary>
		public IReadOnlyCollection<Type> ManagerTypes => managerTypes;

		/// <summary>
		/// The set of state paths, defined by their code names.
		/// </summary>
		public IReadOnlyCollection<string> StatePathCodeNames => statePathCodeNames;

		/// <summary>
		/// The set of access checks as a dictionary having the entity name as key.
		/// </summary>
		public IReadOnlyDictionary<string, EntityRight> EntityRights => entityRights;

		#endregion

		#region Public methods

		/// <summary>
		/// Determines whether a manager class is supported by the present access right.
		/// </summary>
		/// <param name="managerType">The type of the manager class.</param>
		public bool SupportsManager(Type managerType)
		{
			if (managerType == null) throw new ArgumentNullException(nameof(managerType));

			return managerTypes.Contains(managerType);
		}

		/// <summary>
		/// Determins whether a state path is supported by the present access right.
		/// </summary>
		/// <param name="statePathCodeName">
		/// The <see cref="Domain.Workflow.StatePath.CodeName"/> 
		/// of the <see cref="Domain.Workflow.StatePath"/>.
		/// </param>
		public bool SupportsStatePath(string statePathCodeName)
		{
			if (statePathCodeName == null) throw new ArgumentNullException(nameof(statePathCodeName));

			return statePathCodeNames.Contains(statePathCodeName);
		}

		/// <summary>
		/// Determins whether a state path is supported by the present access right.
		/// </summary>
		/// <param name="statePath">The state path.</param>
		public bool SupportsStatePath(Domain.Workflow.StatePath statePath)
		{
			if (statePath == null) throw new ArgumentNullException(nameof(statePath));

			return SupportsStatePath(statePath.CodeName);
		}

		/// <summary>
		/// Get the access rights to an entity based on the present information.
		/// </summary>
		/// <param name="entity">The entity to check.</param>
		/// <returns>Returns the access check response.</returns>
		public EntityRight GetEntityRight(object entity)
		{
			if (entity == null) throw new ArgumentNullException(nameof(entity));

			string entityTypeName = GetEntityTypeName(entity);

			EntityRight existingEntityRight;

			if (this.EntityRights.TryGetValue(entityTypeName, out existingEntityRight))
			{
				return existingEntityRight;
			}
			else
			{
				return nullEntityRight;
			}
		}

		/// <summary>
		/// Get the name of the type of an entity.
		/// Takes care of possible proxy classes containing '_' in their name.
		/// </summary>
		public static string GetEntityTypeName(object entity)
		{
			if (entity == null) throw new ArgumentNullException(nameof(entity));

			return GetTypeName(entity.GetType());
		}

		/// <summary>
		/// Get the name of an entity type.
		/// Takes care of possible proxy classes containing '_' in their name.
		/// </summary>
		public static string GetTypeName(Type type)
		{
			if (type == null) throw new ArgumentNullException(nameof(type));

			if (type.Name.Contains('_') && type.BaseType != null)
				return type.BaseType.FullName;
			else
				return type.FullName;
		}

		#endregion

		#region Internal methods

		internal void CombineManagerAccess(Type managerType)
		{
			if (managerType == null) throw new ArgumentNullException(nameof(managerType));

			managerTypes.Add(managerType);
		}

		internal void CombineEntityAccess(EntityAccess entityAccess)
		{
			if (entityAccess == null) throw new ArgumentNullException(nameof(entityAccess));

			EntityRight entityRight;

			if (!entityRights.TryGetValue(entityAccess.EntityTypeName, out entityRight))
			{
				entityRight = new AccessChecking.EntityRight(entityAccess);

				entityRights[entityAccess.EntityTypeName] = entityRight;
			}
			else
			{
				entityRight.Combine(entityAccess);
			}
		}

		internal void CombineEntityRight(string entityClassName, EntityRight entityRight)
		{
			if (entityClassName == null) throw new ArgumentNullException(nameof(entityClassName));
			if (entityRight == null) throw new ArgumentNullException(nameof(entityRight));

			EntityRight existingEntityRight;

			if (!entityRights.TryGetValue(entityClassName, out existingEntityRight))
			{
				existingEntityRight = new EntityRight();

				entityRights[entityClassName] = existingEntityRight;
			}

			existingEntityRight.Combine(entityRight);
		}

		internal void CombineStatePathAccess(string statePathCodeName)
		{
			if (statePathCodeName == null) throw new ArgumentNullException(nameof(statePathCodeName));

			statePathCodeNames.Add(statePathCodeName);
		}

		internal void Combine(AccessRight otherAccessRight)
		{
			if (otherAccessRight == null) throw new ArgumentNullException(nameof(otherAccessRight));

			foreach (Type managerType in otherAccessRight.ManagerTypes)
			{
				CombineManagerAccess(managerType);
			}

			foreach (string statePathCodeName in otherAccessRight.StatePathCodeNames)
			{
				CombineStatePathAccess(statePathCodeName);
			}

			foreach (var entityRightEntry in otherAccessRight.EntityRights)
			{
				CombineEntityRight(entityRightEntry.Key, entityRightEntry.Value);
			}
		}

		#endregion
	}
}
