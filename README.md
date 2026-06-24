# Grammophone.Domos.AccessChecking

`Grammophone.Domos.AccessChecking` implements the permission resolver used by Domos logic sessions and workflow managers.

It reads a `PermissionsSetup`, combines permissions assigned to roles and disposition types, and answers questions such as whether a user may read an entity, obtain a manager or execute a workflow path.

## Main Features

- `AccessResolver<U>` evaluates entity, manager and state-path access for users derived from `User`.
- `AccessMapper` loads and indexes permission setup data.
- `IPermissionsSetupProvider` abstracts the source of the permission setup.
- `XamlPermissionsSetupProvider` loads the default XAML permission format.
- Configuration objects model permissions, assignments, entity access, manager access and state-path access.

## Documentation

- [Overview](documentation/overview.md)
- [Permissions setup](documentation/permissions-setup.md)
- [Music domain authorization example](documentation/music-authorization.md)
