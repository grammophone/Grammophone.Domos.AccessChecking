using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Grammophone.Domos.AccessChecking.Configuration
{
	/// <summary>
	/// Root of configuration of permissions.
	/// </summary>
	[Serializable]
	public class PermissionsSetup
	{
		#region Private fields

		private PermissionAssignments permissionAssignments;

		private References defaultRolesForAnonymous;

		private References defaultRolesForAuthenticated;

		#endregion

		#region Public properties

		/// <summary>
		/// The collection of permissions along with their assignments.
		/// </summary>
		public PermissionAssignments PermissionAssignments
		{
			get
			{
				return permissionAssignments ?? (permissionAssignments = new PermissionAssignments());
			}
			set
			{
				if (value == null) throw new ArgumentNullException(nameof(value));

				permissionAssignments = value;
			}
		}

		/// <summary>
		/// Roles that the anonymous user has implicitly.
		/// </summary>
		/// <remarks>
		/// As these roles are attached implicitly, they need to exist in the database.
		/// </remarks>
		public References DefaultRolesForAnonymous
		{
			get
			{
				return defaultRolesForAnonymous ?? (defaultRolesForAnonymous = new References());
			}
			set
			{
				if (value == null) throw new ArgumentNullException(nameof(value));

				defaultRolesForAnonymous = value;
			}
		}

		/// <summary>
		/// Roles that the authenticated user has implicitly.
		/// </summary>
		/// <remarks>
		/// As these roles are attached implicitly, they need to exist in the database.
		/// </remarks>
		public References DefaultRolesForAuthenticated
		{
			get
			{
				return defaultRolesForAuthenticated ?? (defaultRolesForAuthenticated = new References());
			}
			set
			{
				if (value == null) throw new ArgumentNullException(nameof(value));

				defaultRolesForAuthenticated = value;
			}
		}

		#endregion
	}
}
