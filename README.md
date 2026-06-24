# Grammophone.Domos.AccessChecking

<<<<<<< HEAD
`Grammophone.Domos.AccessChecking` implements the permission resolver used by Domos logic sessions and workflow managers.

It reads a `PermissionsSetup`, combines permissions assigned to roles and disposition types, and answers questions such as whether a user may read an entity, obtain a manager or execute a workflow path.

## Main Features

- `AccessResolver<U>` evaluates entity, manager and state-path access for users derived from `User`.
- `AccessMapper` loads and indexes permission setup data.
- `IPermissionsSetupProvider` abstracts the source of the permission setup.
- `XamlPermissionsSetupProvider` loads the default XAML permission format.
- Configuration objects model permissions, assignments, entity access, manager access and state-path access.
=======
`Grammophone.Domos.AccessChecking` implements access checking for the Domos integrated session system.

The logic layer uses access resolvers and access-checking exceptions to enforce entity access, manager access and workflow path execution rights.
>>>>>>> refs/remotes/origin/master

## Documentation

- [Overview](documentation/overview.md)
<<<<<<< HEAD
- [Permissions setup](documentation/permissions-setup.md)
- [Music domain authorization example](documentation/music-authorization.md)
=======
- [Access layers](documentation/access-layers.md)
>>>>>>> refs/remotes/origin/master
