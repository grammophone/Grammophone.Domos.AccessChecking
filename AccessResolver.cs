using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Grammophone.Caching;
using Grammophone.Configuration;
using Grammophone.Domos.AccessChecking.Configuration;
using Grammophone.Domos.Domain;
using Grammophone.Domos.Domain.Workflow;
using Grammophone.GenericContentModel;

namespace Grammophone.Domos.AccessChecking
{
	/// <summary>
	/// Class to aid resolution of access rights of combinations of
	/// roles and disposition types.
	/// </summary>
	/// <typeparam name="U">The type of the user, derived from <see cref="User"/>.</typeparam>
	public class AccessResolver<U>
		where U : User
	{
		#region Constants

		/// <summary>
		/// Size of <see cref="rolesAccessRightsCache"/>.
		/// </summary>
		private const int RolesAccessRightsCacheSize = 128;

		/// <summary>
		/// Size of <see cref="dispositionTypesAccessRightsCache"/>.
		/// </summary>
		private const int DispositionTypesAccessRightsCache = 512;

		#endregion

		#region Private fields

		/// <summary>
		/// Thread-safe lazy access to a singleton <see cref="AccessMapper"/>.
		/// Ensures that exceptions occured during singleton creation
		/// will be rethrown during each access.
		/// </summary>
		private Lazy<AccessMapper> lazyAccessMapper;

		/// <summary>
		/// Caches the access rights of combinations of roles.
		/// The key of the cache is a set of role code names.
		/// </summary>
		private MRUCache<EquatableReadOnlyBag<string>, AccessRight> rolesAccessRightsCache;

		/// <summary>
		/// Caches the access rights of combinations of disposition types.
		/// The key of the cache is a set of disposition type code names.
		/// </summary>
		private MRUCache<EquatableReadOnlyBag<string>, AccessRight> dispositionTypesAccessRightsCache;

		/// <summary>
		/// An access right where everything is denied.
		/// </summary>
		private static AccessRight nullAccessRight = new AccessRight();

		#endregion

		#region Construction

		/// <summary>
		/// Create.
		/// </summary>
		/// <param name="permissionsSetupProvider">
		/// The provider for a <see cref="PermissionsSetup"/> instance.
		/// </param>
		public AccessResolver(IPermissionsSetupProvider permissionsSetupProvider)
		{
			if (permissionsSetupProvider == null) throw new ArgumentNullException(nameof(permissionsSetupProvider));

			lazyAccessMapper = new Lazy<AccessMapper>(() => new AccessMapper(permissionsSetupProvider), true);

			rolesAccessRightsCache = 
				new MRUCache<EquatableReadOnlyBag<string>, AccessRight>(
					CombineAccessRightOfRoles, 
					RolesAccessRightsCacheSize);

			dispositionTypesAccessRightsCache =
				new MRUCache<EquatableReadOnlyBag<string>, AccessRight>(
					CombineAccessRightOfDispositionTypes, 
					DispositionTypesAccessRightsCache);
		}

		#endregion

		#region Public methods

		#region Basic access rights combination

		/// <summary>
		/// Get the combined access right of a set of roles.
		/// </summary>
		/// <param name="roles">The set of roles.</param>
		/// <returns>Returns the combined access right.</returns>
		public AccessRight GetAccessRightOfRoles(IEnumerable<Role> roles)
		{
			if (roles == null) throw new ArgumentNullException(nameof(roles));

			var roleCodeNames = new EquatableReadOnlyBag<string>(roles.Select(r => r.CodeName));

			return rolesAccessRightsCache.Get(roleCodeNames);
		}

		/// <summary>
		/// Get the combined access right of a set of disposition types.
		/// </summary>
		/// <param name="dispositionTypes">The set of disposition types.</param>
		/// <returns>Returns the combined access right.</returns>
		public AccessRight GetAccessRightOfDispositionTypes(IEnumerable<DispositionType> dispositionTypes)
		{
			if (dispositionTypes == null) throw new ArgumentNullException(nameof(dispositionTypes));

			var dispositionTypeCodeNames = new EquatableReadOnlyBag<string>(dispositionTypes.Select(dt => dt.CodeName));

			return dispositionTypesAccessRightsCache.Get(dispositionTypeCodeNames);
		}

		/// <summary>
		/// Get the access rights of a disposition type.
		/// </summary>
		/// <param name="dispositionType">The disposition type.</param>
		/// <returns>Returns the access right.</returns>
		public AccessRight GetAccessRightOfDispositionType(DispositionType dispositionType)
		{
			if (dispositionType == null) throw new ArgumentNullException(nameof(dispositionType));

			AccessRight accessRight;

			if (this.lazyAccessMapper.Value.DispositionTypesAccessRightsByCodeName.TryGetValue(dispositionType.CodeName, out accessRight))
			{
				return accessRight;
			}
			else
			{
				return nullAccessRight;
			}
		}

		#endregion

		#region Entity access checking

		/// <summary>
		/// Determine whether a user can read an entity.
		/// For proper performance, ensure that <see cref="User.Roles"/>, 
		/// <see cref="User.Dispositions"/> and their <see cref="Disposition.Type"/> are prefetched.
		/// </summary>
		public bool CanUserReadEntity(U user, object entity)
		{
			if (user == null) throw new ArgumentNullException(nameof(user));
			if (entity == null) throw new ArgumentNullException(nameof(entity));

			var rolesAccessRight = GetAccessRightOfRoles(user.Roles);

			var rolesEntityRight = rolesAccessRight.GetEntityRight(entity);

			if (rolesEntityRight.CanRead) return true;

			if (rolesEntityRight.CanReadOwn)
			{
				var userTrackingEntity = entity as IUserTrackingEntity;

				if (userTrackingEntity != null)
				{
					if (userTrackingEntity.OwningUserID == user.ID) return true;
				}

				var userGroupTrackingEntity = entity as IUserGroupTrackingEntity<U>;

				if (userGroupTrackingEntity != null)
				{
					if (userGroupTrackingEntity.OwningUsers.Contains(user)) return true;
				}
			}

			var segregatedEntity = entity as ISegregatedEntity;

			if (segregatedEntity != null)
			{
				var dispositionsAccessRight = GetDispositionsAccessRight(user, segregatedEntity);

				var dispositionsEntityRight = dispositionsAccessRight.GetEntityRight(entity);

				if (dispositionsEntityRight.CanRead) return true;
			}

			return false;
		}

		/// <summary>
		/// Determine whether a user can write an entity.
		/// For proper performance, ensure that <see cref="User.Roles"/>, 
		/// <see cref="User.Dispositions"/> and their <see cref="Disposition.Type"/> are prefetched.
		/// </summary>
		public bool CanUserWriteEntity(U user, object entity)
		{
			var rolesAccessRight = GetAccessRightOfRoles(user.Roles);

			var rolesEntityRight = rolesAccessRight.GetEntityRight(entity);

			if (rolesEntityRight.CanWrite) return true;

			if (rolesEntityRight.CanWriteOwn)
			{
				var userTrackingEntity = entity as IUserTrackingEntity;

				if (userTrackingEntity != null)
				{
					if (userTrackingEntity.OwningUserID == user.ID) return true;
				}

				var userGroupTrackingEntity = entity as IUserGroupTrackingEntity<U>;

				if (userGroupTrackingEntity != null)
				{
					if (userGroupTrackingEntity.OwningUsers.Contains(user)) return true;
				}
			}

			var segregatedEntity = entity as ISegregatedEntity;

			if (segregatedEntity != null)
			{
				var dispositionsAccessRight = GetDispositionsAccessRight(user, segregatedEntity);

				var dispositionsEntityRight = dispositionsAccessRight.GetEntityRight(entity);

				if (dispositionsEntityRight.CanWrite) return true;
			}

			return false;
		}

		/// <summary>
		/// Determine whether a user can delete an entity.
		/// For proper performance, ensure that <see cref="User.Roles"/>, 
		/// <see cref="User.Dispositions"/> and their <see cref="Disposition.Type"/> are prefetched.
		/// </summary>
		public bool CanUserDeleteEntity(U user, object entity)
		{
			var rolesAccessRight = GetAccessRightOfRoles(user.Roles);

			var rolesEntityRight = rolesAccessRight.GetEntityRight(entity);

			if (rolesEntityRight.CanDelete) return true;

			if (rolesEntityRight.CanDeleteOwn)
			{
				var userTrackingEntity = entity as IUserTrackingEntity;

				if (userTrackingEntity != null)
				{
					if (userTrackingEntity.OwningUserID == user.ID) return true;
				}
			}

			var segregatedEntity = entity as ISegregatedEntity;

			if (segregatedEntity != null)
			{
				var dispositionsΑccessRight = GetDispositionsAccessRight(user, segregatedEntity);

				var dispositionsEntityRight = dispositionsΑccessRight.GetEntityRight(entity);

				if (dispositionsEntityRight.CanDelete) return true;
			}

			return false;
		}

		/// <summary>
		/// Determine whether a user can create an entity.
		/// For proper performance, ensure that <see cref="User.Roles"/>, 
		/// <see cref="User.Dispositions"/> and their <see cref="Disposition.Type"/> are prefetched.
		/// </summary>
		public bool CanUserCreateEntity(U user, object entity)
		{
			var rolesAccessRight = GetAccessRightOfRoles(user.Roles);

			var rolesEntityRight = rolesAccessRight.GetEntityRight(entity);

			if (rolesEntityRight.CanCreate) return true;

			if (rolesEntityRight.CanCreateOwn)
			{
				var userTrackingEntity = entity as IUserTrackingEntity;

				if (userTrackingEntity != null)
				{
					if (userTrackingEntity.OwningUserID == user.ID) return true;
				}

				var userGroupTrackingEntity = entity as IUserGroupTrackingEntity<U>;

				if (userGroupTrackingEntity != null)
				{
					if (userGroupTrackingEntity.OwningUsers.Contains(user)) return true;
				}
			}

			var segregatedEntity = entity as ISegregatedEntity;

			if (segregatedEntity != null)
			{
				var dispositionsAccessRight = GetDispositionsAccessRight(user, segregatedEntity);

				var dispositionsEntityRight = dispositionsAccessRight.GetEntityRight(entity);

				if (dispositionsEntityRight.CanCreate) return true;
			}

			return false;
		}

		#endregion

		#region Manager access checking

		/// <summary>
		/// Determine whether a manager is supported via the user's roles alone or optionally
		/// via her dispositions against a segregated entity.
		/// For proper performance, ensure that <see cref="User.Roles"/>,
		/// <see cref="User.Dispositions"/> and their <see cref="Disposition.Type"/>
		/// are prefetched.
		/// </summary>
		/// <param name="user">The user.</param>
		/// <param name="managerType">The .NET class type of the manager.</param>
		/// <param name="segregatedEntity">The optional segregated entity to check user dispositions against.</param>
		public bool CanUserAccessManager(U user, Type managerType, ISegregatedEntity segregatedEntity = null)
		{
			if (user == null) throw new ArgumentNullException(nameof(user));
			if (managerType == null) throw new ArgumentNullException(nameof(managerType));

			var rolesAccessRight = GetAccessRightOfRoles(user.Roles);

			// If roles alone yield access right to the manager, return true.
			if (rolesAccessRight.SupportsManager(managerType)) return true;

			if (segregatedEntity != null)
			{
				// Determine whether a disposition yeilds access right to the manager.
				foreach (var disposition in user.Dispositions)
				{
					if (disposition.SegregationID == segregatedEntity.SegregationID)
					{
						AccessRight dispositionAccessRight;

						if (lazyAccessMapper.Value.DispositionTypesAccessRightsByCodeName.TryGetValue(
							disposition.Type.CodeName,
							out dispositionAccessRight))
						{
							if (dispositionAccessRight.SupportsManager(managerType)) return true;
						}
					}
				}
			}

			return false;
		}

		/// <summary>
		/// Determine whether a manager is supported via the user's roles alone or optionally
		/// via her dispositions against a segregation.
		/// For proper performance, ensure that <see cref="User.Roles"/>,
		/// <see cref="User.Dispositions"/> and their <see cref="Disposition.Type"/>
		/// are prefetched.
		/// </summary>
		/// <param name="user">The user.</param>
		/// <param name="managerType">The .NET class type of the manager.</param>
		/// <param name="segregationID">The ID of the segregation to check user dispositions against.</param>
		public bool CanUserAccessManager(U user, Type managerType, long segregationID)
		{
			if (user == null) throw new ArgumentNullException(nameof(user));
			if (managerType == null) throw new ArgumentNullException(nameof(managerType));

			var rolesAccessRight = GetAccessRightOfRoles(user.Roles);

			// If roles alone yield access right to the manager, return true.
			if (rolesAccessRight.SupportsManager(managerType)) return true;

			// Determine whether a disposition yeilds access right to the manager.
			foreach (var disposition in user.Dispositions)
			{
				if (disposition.SegregationID == segregationID)
				{
					AccessRight dispositionAccessRight;

					if (lazyAccessMapper.Value.DispositionTypesAccessRightsByCodeName.TryGetValue(
						disposition.Type.CodeName,
						out dispositionAccessRight))
					{
						if (dispositionAccessRight.SupportsManager(managerType)) return true;
					}
				}
			}

			return false;
		}

		/// <summary>
		/// Determines whether a manager is supported as implied from a
		/// user's roles and a disposition she owns as current context.
		/// For proper performance, ensure that <see cref="User.Roles"/>,
		/// <see cref="User.Dispositions"/> and their <see cref="Disposition.Type"/>
		/// are prefetched.
		/// </summary>
		/// <param name="user">The user.</param>
		/// <param name="managerType">The .NET lass type of the manager.</param>
		/// <param name="currentDisposition">The current disposition.</param>
		public bool CanUserAccessManagerByDisposition(U user, Type managerType, Disposition currentDisposition)
		{
			if (currentDisposition == null) throw new ArgumentNullException(nameof(currentDisposition));

			long currentDispositionID = currentDisposition.ID;

			return CanUserAccessManagerByDisposition(user, managerType, currentDispositionID);
		}

		/// <summary>
		/// Determines whether a manager is supported as implied from a
		/// user's roles and a disposition she owns as current context.
		/// For proper performance, ensure that <see cref="User.Roles"/>,
		/// <see cref="User.Dispositions"/> and their <see cref="Disposition.Type"/>
		/// are prefetched.
		/// </summary>
		/// <param name="user">The user.</param>
		/// <param name="managerType">The .NET class type of the manager.</param>
		/// <param name="currentDispositionID">The ID of the current disposition.</param>
		public bool CanUserAccessManagerByDisposition(U user, Type managerType, long currentDispositionID)
		{
			if (user == null) throw new ArgumentNullException(nameof(user));
			if (managerType == null) throw new ArgumentNullException(nameof(managerType));

			var rolesAccessRight = GetAccessRightOfRoles(user.Roles);

			if (rolesAccessRight.SupportsManager(managerType)) return true;

			foreach (var disposition in user.Dispositions)
			{
				if (disposition.ID == currentDispositionID)
				{
					AccessRight dispositionAccessRight;

					if (lazyAccessMapper.Value.DispositionTypesAccessRightsByCodeName.TryGetValue(
						disposition.Type.CodeName,
						out dispositionAccessRight))
					{
						if (dispositionAccessRight.SupportsManager(managerType)) return true;
					}
				}
			}

			return false;
		}

		#endregion

		#region Permissions checking

		/// <summary>
		/// Determine whether a user has a permission via the user's roles alone or optionally
		/// via her dispositions against a segregated entity.
		/// For proper performance, ensure that <see cref="User.Roles"/>,
		/// <see cref="User.Dispositions"/> and their <see cref="Disposition.Type"/>
		/// are prefetched.
		/// </summary>
		/// <param name="user">The user.</param>
		/// <param name="permissionCodeName">
		/// The <see cref="Permission.CodeName"/> of the <see cref="Permission"/>.
		/// </param>
		/// <param name="segregatedEntity">The optional segregated entity to check user dispositions against.</param>
		public bool UserHasPermission(U user, string permissionCodeName, ISegregatedEntity segregatedEntity = null)
		{
			if (user == null) throw new ArgumentNullException(nameof(user));
			if (permissionCodeName == null) throw new ArgumentNullException(nameof(permissionCodeName));

			var rolesAccessRight = GetAccessRightOfRoles(user.Roles);

			// If roles alone yield access right to the permission, return true.
			if (rolesAccessRight.HasPermission(permissionCodeName)) return true;

			if (segregatedEntity != null)
			{
				// Determine whether a disposition yeilds access right to the manager.
				foreach (var disposition in user.Dispositions)
				{
					if (disposition.SegregationID == segregatedEntity.SegregationID)
					{
						AccessRight dispositionAccessRight;

						if (lazyAccessMapper.Value.DispositionTypesAccessRightsByCodeName.TryGetValue(
							disposition.Type.CodeName,
							out dispositionAccessRight))
						{
							if (dispositionAccessRight.HasPermission(permissionCodeName)) return true;
						}
					}
				}
			}

			return false;
		}

		/// <summary>
		/// Determine whether a user has a permission via the user's roles alone or optionally
		/// via her dispositions against a segregation.
		/// For proper performance, ensure that <see cref="User.Roles"/>,
		/// <see cref="User.Dispositions"/> and their <see cref="Disposition.Type"/>
		/// are prefetched.
		/// </summary>
		/// <param name="user">The user.</param>
		/// <param name="permissionCodeName">
		/// The <see cref="Permission.CodeName"/> of the <see cref="Permission"/>.
		/// </param>
		/// <param name="segregationID">The ID of the segregation to check user dispositions against.</param>
		public bool UserHasPermission(U user, string permissionCodeName, long segregationID)
		{
			if (user == null) throw new ArgumentNullException(nameof(user));
			if (permissionCodeName == null) throw new ArgumentNullException(nameof(permissionCodeName));

			var rolesAccessRight = GetAccessRightOfRoles(user.Roles);

			// If roles alone yield access right to the manager, return true.
			if (rolesAccessRight.HasPermission(permissionCodeName)) return true;

			// Determine whether a disposition yeilds access right to the manager.
			foreach (var disposition in user.Dispositions)
			{
				if (disposition.SegregationID == segregationID)
				{
					AccessRight dispositionAccessRight;

					if (lazyAccessMapper.Value.DispositionTypesAccessRightsByCodeName.TryGetValue(
						disposition.Type.CodeName,
						out dispositionAccessRight))
					{
						if (dispositionAccessRight.HasPermission(permissionCodeName)) return true;
					}
				}
			}

			return false;
		}

		/// <summary>
		/// Determines whether a user has a permission as implied from a
		/// user's roles and a disposition she owns as current context.
		/// For proper performance, ensure that <see cref="User.Roles"/>,
		/// <see cref="User.Dispositions"/> and their <see cref="Disposition.Type"/>
		/// are prefetched.
		/// </summary>
		/// <param name="user">The user.</param>
		/// <param name="permissionCodeName">
		/// The <see cref="Permission.CodeName"/> of the <see cref="Permission"/>.
		/// </param>
		/// <param name="currentDisposition">The current disposition.</param>
		public bool UserHasPermissionByDisposition(U user, string permissionCodeName, Disposition currentDisposition)
		{
			if (currentDisposition == null) throw new ArgumentNullException(nameof(currentDisposition));

			long currentDispositionID = currentDisposition.ID;

			return UserHasPermissionByDisposition(user, permissionCodeName, currentDispositionID);
		}

		/// <summary>
		/// Determines whether a user has a permission as implied from a
		/// user's roles and a disposition she owns as current context.
		/// For proper performance, ensure that <see cref="User.Roles"/>,
		/// <see cref="User.Dispositions"/> and their <see cref="Disposition.Type"/>
		/// are prefetched.
		/// </summary>
		/// <param name="user">The user.</param>
		/// <param name="permissionCodeName">
		/// The <see cref="Permission.CodeName"/> of the <see cref="Permission"/>.
		/// </param>
		/// <param name="currentDispositionID">The ID of the current disposition.</param>
		public bool UserHasPermissionByDisposition(U user, string permissionCodeName, long currentDispositionID)
		{
			if (user == null) throw new ArgumentNullException(nameof(user));
			if (permissionCodeName == null) throw new ArgumentNullException(nameof(permissionCodeName));

			var rolesAccessRight = GetAccessRightOfRoles(user.Roles);

			if (rolesAccessRight.HasPermission(permissionCodeName)) return true;

			foreach (var disposition in user.Dispositions)
			{
				if (disposition.ID == currentDispositionID)
				{
					AccessRight dispositionAccessRight;

					if (lazyAccessMapper.Value.DispositionTypesAccessRightsByCodeName.TryGetValue(
						disposition.Type.CodeName,
						out dispositionAccessRight))
					{
						if (dispositionAccessRight.HasPermission(permissionCodeName)) return true;
					}
				}
			}

			return false;
		}

		#endregion

		#region State path access checking

		/// <summary>
		/// Determine whether a user can execute a <see cref="StatePath"/>
		/// over a stateful instance.
		/// </summary>
		/// <typeparam name="ST">The type of state transitions, derived from <see cref="StateTransition{U}"/>.</typeparam>
		/// <param name="user">The user.</param>
		/// <param name="stateful">The stateful instance.</param>
		/// <param name="statePath">The state path to execute.</param>
		public bool CanExecuteStatePath<ST>(U user, IStateful<U, ST> stateful, StatePath statePath)
			where ST : StateTransition<U>
		{
			if (user == null) throw new ArgumentNullException(nameof(user));
			if (stateful == null) throw new ArgumentNullException(nameof(stateful));
			if (statePath == null) throw new ArgumentNullException(nameof(statePath));

			if (!CanUserReadEntity(user, stateful) || !CanUserWriteEntity(user, stateful))
				return false;

			var rolesAccessRight = GetAccessRightOfRoles(user.Roles);

			var rolesStatefulRight = rolesAccessRight.GetEntityRight(stateful);

			if (rolesAccessRight.SupportsStatePath(statePath))
				return true;

			var segregatedStateful = stateful as ISegregatedEntity;

			if (segregatedStateful != null)
			{
				AccessRight dispositionsAccessRight = GetDispositionsAccessRight(user, segregatedStateful);

				if (dispositionsAccessRight.SupportsStatePath(statePath))
				{
					var dispositionsEntityRight = dispositionsAccessRight.GetEntityRight(stateful);

					if (dispositionsEntityRight.CanWrite) return true;
				}
			}

			return false;
		}

		#endregion

		#endregion

		#region Private methods

		/// <summary>
		/// Called during cache miss in <see cref="rolesAccessRightsCache"/>.
		/// </summary>
		/// <param name="roleCodeNames">The set of role code names as the missed key.</param>
		/// <returns>Returns the combined access right.</returns>
		private AccessRight CombineAccessRightOfRoles(EquatableReadOnlyBag<string> roleCodeNames)
		{
			if (roleCodeNames == null) throw new ArgumentNullException(nameof(roleCodeNames));

			var accessMapper = lazyAccessMapper.Value;

			return CombineAccessRights(accessMapper.RolesAccessRightsByCodeName, roleCodeNames);
		}

		/// <summary>
		/// Called during cache miss in <see cref="dispositionTypesAccessRightsCache"/>.
		/// </summary>
		/// <param name="dispositionTypeCodeNames">The set of disposition type code names as the missed key.</param>
		/// <returns>Returns the combined access right.</returns>
		private AccessRight CombineAccessRightOfDispositionTypes(EquatableReadOnlyBag<string> dispositionTypeCodeNames)
		{
			if (dispositionTypeCodeNames == null) throw new ArgumentNullException(nameof(dispositionTypeCodeNames));

			var accessMapper = lazyAccessMapper.Value;

			return CombineAccessRights(accessMapper.DispositionTypesAccessRightsByCodeName, dispositionTypeCodeNames);
		}

		/// <summary>
		/// Combine selected access rights found in an access rights dictionary.
		/// </summary>
		/// <param name="accessRightsMapByCodeName">An access rights dictionary.</param>
		/// <param name="codeNames">The keys used to fetch items from the dictionary.</param>
		/// <returns>Returns the combined access right.</returns>
		private static AccessRight CombineAccessRights(
			IReadOnlyDictionary<string, AccessRight> accessRightsMapByCodeName, 
			IEnumerable<string> codeNames)
		{
			var combinedAccessRight = new AccessRight();

			foreach (string codeName in codeNames)
			{
				AccessRight accessRight;

				if (accessRightsMapByCodeName.TryGetValue(codeName, out accessRight))
				{
					combinedAccessRight.Combine(accessRight);
				}
			}

			return combinedAccessRight;
		}

		/// <summary>
		/// Get the access right that stems from the <see cref="User.Dispositions"/> of
		/// a <see cref="User"/> over a segregated entity.
		/// </summary>
		/// <param name="user">The user.</param>
		/// <param name="segregatedEntity">The segregated entity.</param>
		/// <returns>Returns the combined <see cref="AccessRight"/>.</returns>
		private AccessRight GetDispositionsAccessRight(User user, ISegregatedEntity segregatedEntity) 
		{
			var relativeDispositionTypes = new List<DispositionType>(user.Dispositions.Count);

			foreach (var disposition in user.Dispositions)
			{
				if (segregatedEntity.SegregationID != disposition.SegregationID) continue;

				relativeDispositionTypes.Add(disposition.Type);
			}

			var dispositionTypesAccessRight = GetAccessRightOfDispositionTypes(relativeDispositionTypes);

			return dispositionTypesAccessRight;
		}

		#endregion
	}
}
