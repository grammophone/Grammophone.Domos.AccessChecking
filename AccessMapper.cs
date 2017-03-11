using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Grammophone.Configuration;
using Grammophone.Domos.AccessChecking.Configuration;

namespace Grammophone.Domos.AccessChecking
{
	/// <summary>
	/// Loads and maps the rights implied by roles and disposition types.
	/// </summary>
	[Serializable]
	public class AccessMapper
	{
		#region Private fields

		private Dictionary<string, AccessRight> rolesAccessRightsByCodeName;

		private Dictionary<string, AccessRight> dispositionTypeAccessRightsByCodeName;

		#endregion

		#region Cosntruction

		/// <summary>
		/// Create.
		/// </summary>
		/// <param name="permissionsSetupProvider">
		/// The provider for a <see cref="PermissionsSetup"/> instance.
		/// </param>
		public AccessMapper(IPermissionsSetupProvider permissionsSetupProvider)
		{
			if (permissionsSetupProvider == null) throw new ArgumentNullException(nameof(permissionsSetupProvider));

			var permissionsSetup = permissionsSetupProvider.Load();

			rolesAccessRightsByCodeName = new Dictionary<string, AccessRight>();
			dispositionTypeAccessRightsByCodeName = new Dictionary<string, AccessRight>();

			foreach (var assignment in permissionsSetup.PermissionAssignments)
			{
				CombineToAccessRightsMap(rolesAccessRightsByCodeName, assignment.RoleCodeNames, assignment);

				CombineToAccessRightsMap(dispositionTypeAccessRightsByCodeName, assignment.DispositionTypeCodeNames, assignment);
			}
		}

		#endregion

		#region Public properties

		/// <summary>
		/// Access rights implied by roles, indexed by their <see cref="Domain.Role.CodeName"/>.
		/// </summary>
		public IReadOnlyDictionary<string, AccessRight> RolesAccessRightsByCodeName => rolesAccessRightsByCodeName;

		/// <summary>
		/// Access rights implied by disposition types, indexed by their <see cref="Domain.DispositionType.CodeName"/>.
		/// </summary>
		public IReadOnlyDictionary<string, AccessRight> DispositionTypesAccessRightsByCodeName => dispositionTypeAccessRightsByCodeName;

		#endregion

		#region Private methods

		private static void CombineToAccessRightsMap(
			IDictionary<string, AccessRight> map, 
			IEnumerable<string> codeNames, 
			PermissionAssignment assignment)
		{
			if (map == null) throw new ArgumentNullException(nameof(map));
			if (codeNames == null) throw new ArgumentNullException(nameof(codeNames));
			if (assignment == null) throw new ArgumentNullException(nameof(assignment));

			var permission = assignment.Permission;

			foreach (string codeName in codeNames)
			{
				AccessRight accessRight;

				if (!map.TryGetValue(codeName, out accessRight))
				{
					accessRight = new AccessRight();

					map[codeName] = accessRight;
				}

				foreach (var entityAccess in permission.EntityAccesses)
				{
					accessRight.CombineEntityAccess(entityAccess);
				}

				foreach (var managerAccess in permission.ManagerAccesses)
				{
					accessRight.CombineManagerAccess(managerAccess.ClassName);
				}

				foreach (var statePathAccess in permission.StatePathAccesses)
				{
					accessRight.CombineStatePathAccess(statePathAccess.StatePathCodeName);
				}
			}
		}

		#endregion
	}
}
