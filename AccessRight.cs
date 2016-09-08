using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Grammophone.Domos.Domain;

namespace Grammophone.Domos.AccessChecking
{
	/// <summary>
	/// Provides the entity access rights that are assigned to 
	/// a disposition type, a role or a user.
	/// It is an aggregation of <see cref="Domain.EntityAccess"/> entries.
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
		private HashSet<string> managerClassNames = new HashSet<string>();

		/// <summary>
		/// Map of entity rights by entity class names.
		/// </summary>
		private Dictionary<string, EntityRight> entityRights = new Dictionary<string, EntityRight>();

		#endregion

		#region Public properties

		/// <summary>
		/// The set of managers as a dictionary having the manager name as key.
		/// </summary>
		public IReadOnlyCollection<string> ManagerClassNames => managerClassNames;

		/// <summary>
		/// The set of access checks as a dictionary having the entity name as key.
		/// </summary>
		public IReadOnlyDictionary<string, EntityRight> EntityRights => entityRights;

		#endregion

		#region Public methods

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

		#endregion

		#region Internal methods

		internal void CombineManagerAccess(string managerClassName)
		{
			if (managerClassName == null) throw new ArgumentNullException(nameof(managerClassName));

			managerClassNames.Add(managerClassName);
		}

		internal void CombineEntityAccess(EntityAccess entityAccess)
		{
			if (entityAccess == null) throw new ArgumentNullException(nameof(entityAccess));

			EntityRight entityRight;

			if (!entityRights.TryGetValue(entityAccess.EntityName, out entityRight))
			{
				entityRight = new AccessChecking.EntityRight(entityAccess);

				entityRights[entityAccess.EntityName] = entityRight;
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

		internal void Combine(AccessRight otherAccessRight)
		{
			if (otherAccessRight == null) throw new ArgumentNullException(nameof(otherAccessRight));

			foreach (string managerClassName in otherAccessRight.ManagerClassNames)
			{
				CombineManagerAccess(managerClassName);
			}

			foreach (var entityRightEntry in otherAccessRight.EntityRights)
			{
				CombineEntityRight(entityRightEntry.Key, entityRightEntry.Value);
			}
		}

		#endregion

		#region Private methods

		/// <summary>
		/// Takes care of possible proxy classes containing '_' in their name.
		/// </summary>
		private static string GetEntityTypeName(object entity)
		{
			if (entity == null) throw new ArgumentNullException(nameof(entity));

			Type type = entity.GetType();

			if (type.Name.Contains('_') && type.BaseType != null)
				return type.BaseType.FullName;
			else
				return type.FullName;
		}

		#endregion
	}
}
