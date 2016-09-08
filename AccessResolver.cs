using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Grammophone.Caching;
using Grammophone.Domos.Domain;
using Grammophone.GenericContentModel;

namespace Grammophone.Domos.AccessChecking
{
	/// <summary>
	/// Static class to aid resolution of access rights of combinations of 
	/// roles and disposition types.
	/// </summary>
	public static class AccessResolver
	{
		#region Private fields

		/// <summary>
		/// Thread-safe lazy access to a singleton <see cref="AccessMapper"/>.
		/// Ensures that exceptions occured during singleton creation
		/// will be rethrown during each access.
		/// </summary>
		private static Lazy<AccessMapper> lazyAccessMapper;

		/// <summary>
		/// Caches the access rights of combinations of roles.
		/// The key of the cache is a set of role code names.
		/// </summary>
		private static MRUCache<EquatableReadOnlyBag<string>, AccessRight> rolesAccessRightsCache;

		/// <summary>
		/// Caches the access rights of combinations of disposition types.
		/// The key of the cache is a set of disposition type code names.
		/// </summary>
		private static MRUCache<EquatableReadOnlyBag<string>, AccessRight> dispositionTypesAccessRightsCache;

		#endregion

		#region Construction

		/// <summary>
		/// Static initialization.
		/// </summary>
		static AccessResolver()
		{
			lazyAccessMapper = new Lazy<AccessMapper>(() => new AccessMapper(), true);

			rolesAccessRightsCache = 
				new MRUCache<EquatableReadOnlyBag<string>, AccessRight>(CombineAccessRightOfRoles, 128);

			dispositionTypesAccessRightsCache =
				new MRUCache<EquatableReadOnlyBag<string>, AccessRight>(CombineAccessRightOfDispositionTypes, 128);
		}

		#endregion

		#region Public methods

		/// <summary>
		/// Get the combined access right of a set of roles.
		/// </summary>
		/// <param name="roles">The set of roles.</param>
		/// <returns>Returns the combined access right.</returns>
		public static AccessRight GetAccessRightOfRoles(IEnumerable<Role> roles)
		{
			if (roles == null) throw new ArgumentNullException(nameof(roles));

			var roleCodeNames = new EquatableReadOnlyBag<string>(roles.Select(r => r.CodeName));

			return rolesAccessRightsCache.Get(roleCodeNames);
		}

		/// <summary>
		/// Get the combined access right of a set of disposition types.
		/// </summary>
		/// <param name="dispositionTypes">The set of disposition types.</param>
		/// <returns>Returns the combined access right.</returns>
		public static AccessRight GetAccessRightOfDispositionTypes(IEnumerable<DispositionType> dispositionTypes)
		{
			if (dispositionTypes == null) throw new ArgumentNullException(nameof(dispositionTypes));

			var dispositionTypeCodeNames = new EquatableReadOnlyBag<string>(dispositionTypes.Select(dt => dt.CodeName));

			return dispositionTypesAccessRightsCache.Get(dispositionTypeCodeNames);
		}

		#endregion

		#region Private methods

		/// <summary>
		/// Called during cache miss in <see cref="rolesAccessRightsCache"/>.
		/// </summary>
		/// <param name="roleCodeNames">The set of role code names as the missed key.</param>
		/// <returns>Returns the combined access right.</returns>
		private static AccessRight CombineAccessRightOfRoles(EquatableReadOnlyBag<string> roleCodeNames)
		{
			if (roleCodeNames == null) throw new ArgumentNullException(nameof(roleCodeNames));

			var accessMapper = lazyAccessMapper.Value;

			return CombineAccessRights(accessMapper.RolesAccessRightsByCodeName, roleCodeNames);
		}

		/// <summary>
		/// Called during cache miss in <see cref="dispositionTypesAccessRightsCache"/>.
		/// </summary>
		/// <param name="dispositionTypeCodeNames">The set of disposition type code names as the missed key.</param>
		/// <returns>Returns the combined access right.</returns>
		private static AccessRight CombineAccessRightOfDispositionTypes(EquatableReadOnlyBag<string> dispositionTypeCodeNames)
		{
			if (dispositionTypeCodeNames == null) throw new ArgumentNullException(nameof(dispositionTypeCodeNames));

			var accessMapper = lazyAccessMapper.Value;

			return CombineAccessRights(accessMapper.DispositionTypesAccessRightsByCodeName, dispositionTypeCodeNames);
		}

		private static AccessRight CombineAccessRights(
			IReadOnlyDictionary<string, AccessRight> accessRightsMapByCodeName, 
			IEnumerable<string> codeNames)
		{
			var combinedAccessRight = new AccessRight();

			foreach (string codeName in codeNames)
			{
				AccessRight accessRight;

				if (accessRightsMapByCodeName.TryGetValue(codeName, out accessRight))
				{
					combinedAccessRight.Combine(accessRight);
				}
			}

			return combinedAccessRight;
		}

		#endregion
	}
}
