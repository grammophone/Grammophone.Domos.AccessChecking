using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Grammophone.Domos.Domain;
using Grammophone.Domos.Domain.Workflow;

namespace Grammophone.Domos.AccessChecking.Configuration
{
	/// <summary>
	/// Assignment of a permission with all its contents to roles and disposition types.
	/// </summary>
	[Serializable]
	public class PermissionAssignment
	{
		#region Private fields

		private References roleReferences;

		private References dispositionTypeRerferences;

		#endregion

		#region Public properties

		/// <summary>
		/// The permission to assign.
		/// </summary>
		public Permission Permission { get; set; }

		/// <summary>
		/// The collection of references to <see cref="Role"/>s
		/// associated to this permission.
		/// </summary>
		public References RoleReferences
		{
			get
			{
				return roleReferences ?? (roleReferences = new References());
			}
			set
			{
				if (value == null) throw new ArgumentNullException(nameof(value));

				roleReferences = value;
			}
		}

		/// <summary>
		/// The collection of references to <see cref="DispositionType"/>s
		/// associated to this permission.
		/// </summary>
		public References DispositionReferences
		{
			get
			{
				return dispositionTypeRerferences ?? (dispositionTypeRerferences = new References());
			}
			set
			{
				if (value == null) throw new ArgumentNullException(nameof(value));

				dispositionTypeRerferences = value;
			}
		}

		#endregion
	}
}
