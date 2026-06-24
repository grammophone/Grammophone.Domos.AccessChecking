# Permissions Setup

The default permissions setup format is XAML. It is loaded through `XamlPermissionsSetupProvider`, which implements `IPermissionsSetupProvider`.

The root object is `PermissionsSetup`. It contains `PermissionAssignments`, default anonymous roles and default authenticated roles.

## Permission Assignment Shape

Each `PermissionAssignment` has:

- A `Permission` object.
- A collection of role references.
- A collection of disposition type references.

Each `Permission` can contain:

- `EntityAccesses` for entity rights.
- `ManagerAccesses` for logic-manager rights.
- `StatePathAccesses` for workflow path execution rights.

## Entity Access

`EntityAccess` points to an entity type and grants any of these rights:

- `CanCreate`
- `CanCreateOwn`
- `CanRead`
- `CanReadOwn`
- `CanWrite`
- `CanWriteOwn`
- `CanDelete`
- `CanDeleteOwn`

Own-rights apply only when the entity implements `IOwnedEntity<TUser>`.

## Manager Access

`ManagerAccess` points to a manager type. Logic sessions should expose managers through `GetManager` or `TryGetManager`, because those methods enforce this permission plane.

## State Path Access

`StatePathAccess` points to a workflow path by `StatePath.CodeName`. `WorkflowManager` checks this right before executing a path.

## Minimal XAML Example

```xml
<PermissionsSetup xmlns="clr-namespace:Grammophone.Domos.AccessChecking.Configuration;assembly=Grammophone.Domos.AccessChecking"
				  xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
				  xmlns:music="clr-namespace:Music.Domain;assembly=Music.Domain"
				  xmlns:logic="clr-namespace:Music.Logic;assembly=Music.Logic">
	<PermissionsSetup.PermissionAssignments>
		<PermissionAssignment>
			<PermissionAssignment.Permission>
				<Permission CodeName="ManageRecordLabelCatalog">
					<Permission.EntityAccesses>
						<EntityAccess EntityType="{x:Type music:Album}" CanRead="True" CanWrite="True" CanCreate="True" />
						<EntityAccess EntityType="{x:Type music:Track}" CanRead="True" CanWrite="True" CanCreate="True" />
					</Permission.EntityAccesses>
					<Permission.ManagerAccesses>
						<ManagerAccess ManagerType="{x:Type logic:RecordLabelCatalogManager}" />
					</Permission.ManagerAccesses>
					<Permission.StatePathAccesses>
						<StatePathAccess StatePathCodeName="ApproveForRelease" />
					</Permission.StatePathAccesses>
				</Permission>
			</PermissionAssignment.Permission>
			<PermissionAssignment.DispositionTypes>
				<Reference CodeName="RecordLabelAdministrator" />
			</PermissionAssignment.DispositionTypes>
		</PermissionAssignment>
	</PermissionsSetup.PermissionAssignments>
</PermissionsSetup>
```

This grants the permission to users having a `RecordLabelAdministrator` disposition. The permission is then evaluated against the relevant `RecordLabel` segregation.
