using Grammophone.Domos.Domain;
using System;
using System.Collections.Generic;
using System.ComponentModel;
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
		#region Private fields

		/// <summary>
		/// Backing field for <see cref="EntityType"/>.
		/// </summary>
		private Type entityType;

		#endregion

		#region Primitive properties

		/// <summary>
		/// The entity type for which access is defined.
		/// </summary>
		[Required]
		public Type EntityType
		{
			get
			{
				return entityType;
			}
			set
			{
				if (value == null) throw new ArgumentNullException(nameof(value));

				entityType = value;

				this.EntityTypeName = AccessRight.GetTypeName(value);
			}
		}

		/// <summary>
		/// The full name of the <see cref="EntityType"/> for which access is defined.
		/// </summary>
		public string EntityTypeName { get; private set; }

		/// <summary>
		/// If true, the user can create entities of the requested type.
		/// </summary>
		[DefaultValue(false)]
		public bool CanCreate { get; set; }

		/// <summary>
		/// If true, the user can create entities owned by by her of the requested 
		/// type. This applies to entities implementing <see cref="IOwnedEntity{U}"/>.
		/// </summary>
		[DefaultValue(false)]
		public bool CanCreateOwn { get; set; }

		/// <summary>
		/// If true, the user can read entities of the requested type.
		/// </summary>
		[DefaultValue(false)]
		public bool CanRead { get; set; }

		/// <summary>
		/// If true, the user can read entities created by her of the requested 
		/// type. This applies to entities implementing <see cref="IOwnedEntity{U}"/>.
		/// </summary>
		[DefaultValue(false)]
		public bool CanReadOwn { get; set; }

		/// <summary>
		/// If true, the user can read entities of the requested type.
		/// </summary>
		[DefaultValue(false)]
		public bool CanWrite { get; set; }

		/// <summary>
		/// If true, the user can read entities created by her of the requested 
		/// type. This applies to entities implementing <see cref="IOwnedEntity{U}"/>.
		/// </summary>
		[DefaultValue(false)]
		public bool CanWriteOwn { get; set; }

		/// <summary>
		/// If true, the user can delete entities of the requested type.
		/// </summary>
		[DefaultValue(false)]
		public bool CanDelete { get; set; }

		/// <summary>
		/// If true, the user can delete entities created by her of the requested 
		/// type. This applies to entities implementing <see cref="IOwnedEntity{U}"/>.
		/// </summary>
		[DefaultValue(false)]
		public bool CanDeleteOwn { get; set; }

		#endregion
	}
}
