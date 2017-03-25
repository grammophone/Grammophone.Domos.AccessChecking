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

		#endregion
	}
}
