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
		/// <summary>
		/// The collection of permissions along with their assignments.
		/// </summary>
		public PermissionAssignments PermissionAssignments { get; set; }
	}
}
