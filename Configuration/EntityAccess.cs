using Grammophone.Domos.Domain;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Grammophone.Domos.AccessChecking.Configuration
{
	/// <summary>
	/// Defines access to en entity type by a user or a user's disposition.
	/// </summary>
	[Serializable]
	public class EntityAccess 
	{
		#region Primitive properties

		/// <summary>
		/// The full name of the entity class for which access is defined.
		/// </summary>
		[Required]
		public string EntityName { get; set; }

		/// <summary>
		/// If true, the user can create entities of the requested type.
		/// </summary>
		public bool CanCreate { get; set; }

		/// <summary>
		/// If true, the user can read entities of the requested type.
		/// </summary>
		public bool CanRead { get; set; }

		/// <summary>
		/// If true, the user can read entities created by her of the requested 
		/// type. This applies to entities implementing <see cref="IUserTrackingEntity{U}"/>
		/// or <see cref="IUserGroupTrackingEntity{U}"/>.
		/// </summary>
		public bool CanReadOwn { get; set; }

		/// <summary>
		/// If true, the user can read entities of the requested type.
		/// </summary>
		public bool CanWrite { get; set; }

		/// <summary>
		/// If true, the user can read entities created by her of the requested 
		/// type. This applies to entities implementing <see cref="IUserTrackingEntity{U}"/>
		/// or <see cref="IUserGroupTrackingEntity{U}"/>.
		/// </summary>
		public bool CanWriteOwn { get; set; }

		/// <summary>
		/// If true, the user can delete entities of the requested type.
		/// </summary>
		public bool CanDelete { get; set; }

		/// <summary>
		/// If true, the user can delete entities created by her of the requested 
		/// type. This applies to entities implementing <see cref="IUserTrackingEntity{U}"/>
		/// or <see cref="IUserGroupTrackingEntity{U}"/>.
		/// </summary>
		public bool CanDeleteOwn { get; set; }

		#endregion
	}
}
