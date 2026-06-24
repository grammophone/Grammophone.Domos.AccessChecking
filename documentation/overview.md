<<<<<<< HEAD
# Access Checking Overview

`Grammophone.Domos.AccessChecking` is the policy evaluation layer for Domos.

It does not know the concrete application domain. It understands Domos security concepts: users, roles, disposition types, dispositions, segregations, owned entities, managers and workflow paths.

The main runtime type is:

```csharp
public class AccessResolver<U>
	where U : User
```

`AccessResolver<U>` is used by `LogicSession`, `Manager` and `WorkflowManager` to decide whether an operation is allowed.

## Three Access Planes

The resolver works over three planes:

- Entity access for create, read, write and delete operations.
- Manager access for obtaining logic managers from a session.
- State-path access for workflow transitions.

Each plane is configured through permissions. A permission can grant any combination of entity, manager and state-path rights.

## Roles And Dispositions

Permissions are assigned to roles and disposition types.

Roles are system-wide. If a `MusicUser` has the `CatalogOperator` role, the associated permissions apply everywhere.

Dispositions are segregation-scoped. If a `MusicUser` has a `RecordLabelAdministrator` disposition for `RecordLabel` 42, the associated permissions apply only when the target entity or manager operation is scoped to record label 42.

## Owned And Segregated Entities

Entity rights include full rights and own-right variants. Own rights apply when an entity implements `IOwnedEntity<TUser>` and `IsOwnedBy(user)` returns true.

For entities implementing `ISegregatedEntity`, the resolver checks disposition rights for the entity's segregation. For entities implementing `IMultiSegregatedEntity`, it checks each segregation ID exposed by the entity.

## Caching

`AccessResolver<U>` caches combined rights for sets of role code names and disposition type code names. This keeps repeated checks cheap after the permissions setup has been loaded.

Applications should still prefetch `User.Roles`, `User.Dispositions` and disposition types when performing many access checks in a batch.
=======
# Overview

`Grammophone.Domos.AccessChecking` provides access-checking services used by the Domos logic layer.

The library works with the domain security ontology: roles, dispositions, ownership, segregation, managers and workflow state paths. Logic sessions use access resolvers to decide whether the active or acting user may perform an operation.

Concrete applications are expected to supply or configure access rules suitable for their business model.
>>>>>>> refs/remotes/origin/master
