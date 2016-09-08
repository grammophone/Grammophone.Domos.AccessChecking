using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Grammophone.Domos.Domain;

namespace Grammophone.Domos.AccessChecking
{
	/// <summary>
	/// Represents an abstract access check for a type of entity.
	/// </summary>
	public class EntityRight
	{
		#region Construction

		/// <summary>
		/// Create having all fields set to false.
		/// </summary>
		public EntityRight()
		{
		}

		/// <summary>
		/// Create based on an <see cref="EntityAccess"/>.
		/// </summary>
		public EntityRight(EntityAccess entityAccess)
		{
			if (entityAccess == null) throw new ArgumentNullException(nameof(entityAccess));

			this.CanCreate = entityAccess.CanCreate;
			this.CanRead = entityAccess.CanRead;
			this.CanReadOwn = entityAccess.CanReadOwn;
			this.CanWrite = entityAccess.CanWrite;
			this.CanWriteOwn = entityAccess.CanWriteOwn;
			this.CanDelete = entityAccess.CanDelete;
			this.CanDeleteOwn = entityAccess.CanDeleteOwn;
		}

		#endregion

		#region Public properties

		/// <summary>
		/// If true, the user can create entities of the requested type.
		/// </summary>
		public bool CanCreate { get; set; }

		/// <summary>
		/// If true, the user can read entities of the requested type.
		/// </summary>
		public bool CanRead { get; private set; }

		/// <summary>
		/// If true, the user can read entities created by her of the requested 
		/// type. This applies to entities derived from <see cref="IUserTrackingEntity{U}"/>.
		/// </summary>
		public bool CanReadOwn { get; private set; }

		/// <summary>
		/// If true, the user can read entities of the requested type.
		/// </summary>
		public bool CanWrite { get; private set; }

		/// <summary>
		/// If true, the user can read entities created by her of the requested 
		/// type. This applies to entities derived from <see cref="IUserTrackingEntity{U}"/>.
		/// </summary>
		public bool CanWriteOwn { get; private set; }

		/// <summary>
		/// If true, the user can delete entities of the requested type.
		/// </summary>
		public bool CanDelete { get; private set; }

		/// <summary>
		/// If true, the user can delete entities created by her of the requested 
		/// type. This applies to entities implementing <see cref="IUserTrackingEntity{U}"/>.
		/// </summary>
		public bool CanDeleteOwn { get; private set; }

		#endregion

		#region Internal methods

		/// <summary>
		/// Modify the instance by combining the rights of a given <see cref="EntityAccess"/>.
		/// </summary>
		internal EntityRight Combine(EntityAccess entityAccess)
		{
			if (entityAccess == null) throw new ArgumentNullException(nameof(entityAccess));

			this.CanCreate = this.CanCreate || entityAccess.CanCreate;
			this.CanRead = this.CanRead || entityAccess.CanRead;
			this.CanReadOwn = this.CanReadOwn || entityAccess.CanReadOwn;
			this.CanWrite = this.CanWrite || entityAccess.CanWrite;
			this.CanWriteOwn = this.CanWriteOwn || entityAccess.CanWriteOwn;
			this.CanDelete = this.CanDelete || entityAccess.CanDelete;
			this.CanDeleteOwn = this.CanDeleteOwn || entityAccess.CanDeleteOwn;

			return this;
		}

		/// <summary>
		/// Combines another entity right to this one.
		/// </summary>
		internal EntityRight Combine(EntityRight otherEntityRight)
		{
			if (otherEntityRight == null) throw new ArgumentNullException(nameof(otherEntityRight));

			this.CanCreate = this.CanCreate || otherEntityRight.CanCreate;
			this.CanRead = this.CanRead || otherEntityRight.CanRead;
			this.CanReadOwn = this.CanReadOwn || otherEntityRight.CanReadOwn;
			this.CanWrite = this.CanWrite || otherEntityRight.CanWrite;
			this.CanWriteOwn = this.CanWriteOwn || otherEntityRight.CanWriteOwn;
			this.CanDelete = this.CanDelete || otherEntityRight.CanDelete;
			this.CanDeleteOwn = this.CanDeleteOwn || otherEntityRight.CanDeleteOwn;

			return this;
		}

		#endregion

		#region Operators

		#endregion
	}
}
