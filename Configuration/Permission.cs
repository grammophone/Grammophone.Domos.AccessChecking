using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Grammophone.Domos.AccessChecking.Configuration
{
	/// <summary>
	/// An abstraction of allowed behavior.
	/// </summary>
	[Serializable]
	public class Permission
	{
		#region Private fields

		private EntityAccesses entityAccesses;

		private ManagerAccesses managerAccesses;

		private StatePathAccesses statePathAccesses;

		#endregion

		#region Primitive properties

		/// <summary>
		/// A code name of the permission.
		/// </summary>
		[Required]
		public string CodeName { get; set; }

		#endregion

		#region Relations

		/// <summary>
		/// These are the entity accesses associated to the permission.
		/// </summary>
		public EntityAccesses EntityAccesses
		{
			get
			{
				return entityAccesses ?? (entityAccesses = new EntityAccesses());
			}
			set
			{
				if (value == null) throw new ArgumentNullException(nameof(value));

				entityAccesses = value;
			}
		}

		/// <summary>
		/// These are the entity accesses associated to the permission.
		/// </summary>
		public ManagerAccesses ManagerAccesses
		{
			get
			{
				return managerAccesses ?? (managerAccesses = new ManagerAccesses());
			}
			set
			{
				if (value == null) throw new ArgumentNullException(nameof(value));

				managerAccesses = value;
			}
		}

		/// <summary>
		/// These are the state path accesses associated to the permission.
		/// </summary>
		public StatePathAccesses StatePathAccesses
		{
			get
			{
				return statePathAccesses ?? (statePathAccesses = new StatePathAccesses());
			}
			set
			{
				if (value == null) throw new ArgumentNullException(nameof(value));

				statePathAccesses = value;
			}
		}

		#endregion
	}
}
