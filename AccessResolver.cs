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
		private const int RolesAccessRightsCacheSize = 4096;

		/// <summary>
		/// Size of <see cref="dispositionTypesAccessRightsCache"/>.
		/// </summary>
		private const int DispositionTypesAccessRightsCacheSize = 4096;

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
					DispositionTypesAccessRightsCacheSize);
		}

		#endregion

		#region Public methods

		#region Basic access rights combination

		/// <summary>
		/// Get the access right derived from the user's roles,
		/// including those speified in <see cref="User.Roles"/> proeprty of the <see cref="User"/>
		/// and those implied by <see cref="PermissionsSetup.DefaultRolesForAuthenticated"/>
		/// or <see cref="PermissionsSetup.DefaultRolesForAnonymous"/>.
		/// </summary>
		/// <param name="user">The user whose set of specified and implied roles to check.</param>
		/// <returns>Returns the combined access right.</returns>
		public AccessRight GetRolesAccessRight(U user)
		{
			if (user == null) throw new ArgumentNullException(nameof(user));

			// Combine default roles and user roles.

			IReadOnlyList<string> defaultRoleCodeNames;

			if (user.IsAnonymous)
			{
				defaultRoleCodeNames = lazyAccessMapper.Value.DefaultRolesForAnonymous;
			}
			else
			{
				defaultRoleCodeNames = lazyAccessMapper.Value.DefaultRolesForAuthenticated;
			}

			string[] roleCodeNames = new string[user.Roles.Count + defaultRoleCodeNames.Count];

			int i = 0;

			// Add default roles.
			for (i = 0; i < defaultRoleCodeNames.Count; i++)
			{
				roleCodeNames[i] = defaultRoleCodeNames[i];
			}

			// Add user roles.
			foreach (var role in user.Roles)
			{
				roleCodeNames[i++] = role.CodeName;
			}

			return rolesAccessRightsCache.Get(new EquatableReadOnlyBag<string>(roleCodeNames));
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

			if (this.lazyAccessMapper.Value.DispositionTypesAccessRightsByCodeName.TryGetValue(
				dispositionType.CodeName, 
				out AccessRight accessRight))
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

			var rolesAccessRight = GetRolesAccessRight(user);

			var rolesEntityRight = rolesAccessRight.GetEntityRight(entity);

			if (rolesEntityRight.CanRead) return true;

			if (rolesEntityRight.CanReadOwn)
			{
				if (entity is IOwnedEntity<U> ownedEntity)
				{
					if (ownedEntity.IsOwnedBy(user)) return true;
				}
			}

			if (entity is ISegregatedEntity segregatedEntity)
			{
				var dispositionsAccessRight = GetDispositionsAccessRight(user, segregatedEntity);

				var dispositionsEntityRight = dispositionsAccessRight.GetEntityRight(entity);

				if (dispositionsEntityRight.CanRead) return true;

				if (dispositionsEntityRight.CanReadOwn && !rolesEntityRight.CanReadOwn)
				{
					if (entity is IOwnedEntity<U> ownedEntity)
					{
						if (ownedEntity.IsOwnedBy(user)) return true;
					}
				}
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
			var rolesAccessRight = GetRolesAccessRight(user);

			var rolesEntityRight = rolesAccessRight.GetEntityRight(entity);

			if (rolesEntityRight.CanWrite) return true;

			if (rolesEntityRight.CanWriteOwn)
			{
				if (entity is IOwnedEntity<U> ownedEntity)
				{
					if (ownedEntity.IsOwnedBy(user)) return true;
				}
			}

			if (entity is ISegregatedEntity segregatedEntity)
			{
				var dispositionsAccessRight = GetDispositionsAccessRight(user, segregatedEntity);

				var dispositionsEntityRight = dispositionsAccessRight.GetEntityRight(entity);

				if (dispositionsEntityRight.CanWrite) return true;

				if (dispositionsEntityRight.CanWriteOwn && !rolesEntityRight.CanWriteOwn)
				{
					if (entity is IOwnedEntity<U> ownedEntity)
					{
						if (ownedEntity.IsOwnedBy(user)) return true;
					}
				}
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
			var rolesAccessRight = GetRolesAccessRight(user);

			var rolesEntityRight = rolesAccessRight.GetEntityRight(entity);

			if (rolesEntityRight.CanDelete) return true;

			if (rolesEntityRight.CanDeleteOwn)
			{
				if (entity is IOwnedEntity<U> ownedEntity)
				{
					if (ownedEntity.IsOwnedBy(user)) return true;
				}
			}

			if (entity is ISegregatedEntity segregatedEntity)
			{
				var dispositionsΑccessRight = GetDispositionsAccessRight(user, segregatedEntity);

				var dispositionsEntityRight = dispositionsΑccessRight.GetEntityRight(entity);

				if (dispositionsEntityRight.CanDelete) return true;

				if (dispositionsEntityRight.CanDeleteOwn && !rolesEntityRight.CanDeleteOwn)
				{
					if (entity is IOwnedEntity<U> ownedEntity)
					{
						if (ownedEntity.IsOwnedBy(user)) return true;
					}
				}
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
			var rolesAccessRight = GetRolesAccessRight(user);

			var rolesEntityRight = rolesAccessRight.GetEntityRight(entity);

			if (rolesEntityRight.CanCreate) return true;

			if (rolesEntityRight.CanCreateOwn)
			{
				if (entity is IOwnedEntity<U> ownedEntity)
				{
					if (ownedEntity.IsOwnedBy(user)) return true;
				}
			}

			if (entity is ISegregatedEntity segregatedEntity)
			{
				var dispositionsAccessRight = GetDispositionsAccessRight(user, segregatedEntity);

				var dispositionsEntityRight = dispositionsAccessRight.GetEntityRight(entity);

				if (dispositionsEntityRight.CanCreate) return true;

				if (dispositionsEntityRight.CanCreateOwn && !rolesEntityRight.CanCreateOwn)
				{
					if (entity is IOwnedEntity<U> ownedEntity)
					{
						if (ownedEntity.IsOwnedBy(user)) return true;
					}
				}
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

			var rolesAccessRight = GetRolesAccessRight(user);

			// If roles alone yield access right to the manager, return true.
			if (rolesAccessRight.SupportsManager(managerType)) return true;

			if (segregatedEntity != null)
			{
				// Determine whether a disposition yields access right to the manager.
				var dispositionsAccessRight = GetDispositionsAccessRight(user, segregatedEntity);

				return dispositionsAccessRight.SupportsManager(managerType);
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

			var rolesAccessRight = GetRolesAccessRight(user);

			// If roles alone yield access right to the manager, return true.
			if (rolesAccessRight.SupportsManager(managerType)) return true;

			// Determine whether a disposition yields access right to the manager.
			var dispositionsAccessRight = GetDispositionsAccessRight(user, segregationID);

			return dispositionsAccessRight.SupportsManager(managerType);
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

			var rolesAccessRight = GetRolesAccessRight(user);

			if (rolesAccessRight.SupportsManager(managerType)) return true;

			foreach (var disposition in user.Dispositions)
			{
				if (disposition.ID == currentDispositionID)
				{
					if (lazyAccessMapper.Value.DispositionTypesAccessRightsByCodeName.TryGetValue(
						disposition.Type.CodeName,
						out AccessRight dispositionAccessRight))
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

			var rolesAccessRight = GetRolesAccessRight(user);

			// If roles alone yield access right to the permission, return true.
			if (rolesAccessRight.HasPermission(permissionCodeName)) return true;

			if (segregatedEntity != null)
			{
				// Determine whether a disposition yields access right to the manager.
				var dispositionsAccessRight = GetDispositionsAccessRight(user, segregatedEntity);

				return dispositionsAccessRight.HasPermission(permissionCodeName);
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

			var rolesAccessRight = GetRolesAccessRight(user);

			// If roles alone yield access right to the manager, return true.
			if (rolesAccessRight.HasPermission(permissionCodeName)) return true;

			// Determine whether a disposition yields access right to the manager.
			var dispositionsAccessRight = GetDispositionsAccessRight(user, segregationID);

			return dispositionsAccessRight.HasPermission(permissionCodeName);
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

			var rolesAccessRight = GetRolesAccessRight(user);

			if (rolesAccessRight.HasPermission(permissionCodeName)) return true;

			foreach (var disposition in user.Dispositions)
			{
				if (disposition.ID == currentDispositionID)
				{
					if (lazyAccessMapper.Value.DispositionTypesAccessRightsByCodeName.TryGetValue(
						disposition.Type.CodeName,
						out AccessRight dispositionAccessRight))
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

			var rolesAccessRight = GetRolesAccessRight(user);

			var rolesStatefulRight = rolesAccessRight.GetEntityRight(stateful);

			if (rolesAccessRight.SupportsStatePath(statePath))
				return true;

			if (stateful is ISegregatedEntity segregatedStateful)
			{
				AccessRight dispositionsAccessRight = GetDispositionsAccessRight(user, segregatedStateful);

				return dispositionsAccessRight.SupportsStatePath(statePath);
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

				if (accessRightsMapByCodeName.TryGetValue(codeName, out AccessRight accessRight))
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
			=> GetDispositionsAccessRight(user, segregatedEntity.SegregationID);

		/// <summary>
		/// Get the access right that stems from the <see cref="User.Dispositions"/> of
		/// a <see cref="User"/> over a segregation..
		/// </summary>
		/// <param name="user">The user.</param>
		/// <param name="segregationID">The ID of the segregation.</param>
		/// <returns>Returns the combined <see cref="AccessRight"/>.</returns>
		private AccessRight GetDispositionsAccessRight(User user, long segregationID)
		{
			var dispositionsForSegregation = user.GetDispositionsBySegregationID(segregationID);

			var activeDispositionTypes = from d in dispositionsForSegregation
																	 where d.Status != DispositionStatus.Revoked
																	 select d.Type;

			return GetAccessRightOfDispositionTypes(activeDispositionTypes);
		}

		#endregion
	}
}
