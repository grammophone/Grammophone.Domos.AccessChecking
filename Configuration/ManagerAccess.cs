using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Grammophone.Domos.AccessChecking.Configuration
{
	/// <summary>
	/// An abstraction for a session API access.
	/// </summary>
	[Serializable]
	public class ManagerAccess
	{
		#region Primitive properties

		/// <summary>
		/// The .NET type of a session manager serving the permission.
		/// </summary>
		[Required]
		public Type ManagerType { get; set; }

		#endregion
	}
}
