using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Grammophone.Domos.Domain;

namespace Grammophone.Domos.AccessChecking.Configuration
{
	/// <summary>
	/// Assignment of a permission with all its contents to roles and disposition types.
	/// </summary>
	[Serializable]
	public class PermissionAssignment
	{
		#region Private fields

		private ICollection<string> roleCodeNames;

		private ICollection<string> dispositionTypeCodeNames;

		#endregion

		#region Public properties

		/// <summary>
		/// The permission to assign.
		/// </summary>
		public Permission Permission { get; set; }

		/// <summary>
		/// The collection of <see cref="Role.CodeName"/>s.
		/// </summary>
		public ICollection<string> RoleCodeNames
		{
			get
			{
				return roleCodeNames ?? (roleCodeNames = new HashSet<string>());
			}
			set
			{
				if (value == null) throw new ArgumentNullException(nameof(value));

				roleCodeNames = value;
			}
		}

		/// <summary>
		/// The collection of <see cref="DispositionType.CodeName"/>s.
		/// </summary>
		public ICollection<string> DispositionTypeCodeNames
		{
			get
			{
				return dispositionTypeCodeNames ?? (dispositionTypeCodeNames = new HashSet<string>());
			}
			set
			{
				if (value == null) throw new ArgumentNullException(nameof(value));

				dispositionTypeCodeNames = value;
			}
		}

		#endregion
	}
}
