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

		private References roles;

		private References dispositionTypes;

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
		public References Roles
		{
			get
			{
				return roles ?? (roles = new References());
			}
			set
			{
				if (value == null) throw new ArgumentNullException(nameof(value));

				roles = value;
			}
		}

		/// <summary>
		/// The collection of references to <see cref="DispositionType"/>s
		/// associated to this permission.
		/// </summary>
		public References DispositionTypes
		{
			get
			{
				return dispositionTypes ?? (dispositionTypes = new References());
			}
			set
			{
				if (value == null) throw new ArgumentNullException(nameof(value));

				dispositionTypes = value;
			}
		}

		#endregion
	}
}
