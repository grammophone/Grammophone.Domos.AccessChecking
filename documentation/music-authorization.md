# Music Domain Authorization Example

This page applies access-checking concepts to the fictional music domain used in the Domos documentation.

## Domain Shape

The user type is `MusicUser`. A `RecordLabel` derives from `Segregation<MusicUser>`. `Album` and `Track` implement `ISegregatedEntity<RecordLabel>` and `IOwnedEntity<MusicUser>`.

`RecordLabelAdministrator` derives from `RecordLabelDisposition`, which derives from `Disposition` and implements `IDisposition<MusicUser, RecordLabel>`.

## Rights Granted By Role

A system role named `CatalogReader` can be assigned a permission with `CanRead="True"` for `Album`, `Artist` and `Track`. This grants read access across the catalog.

## Rights Granted By Disposition

The `RecordLabelAdministrator` disposition type can be assigned a permission with write rights over `Album`, `Artist` and `Track`. The rights apply only to entities in the record label named by the disposition's segregation ID.

## Own Rights

If `Track` implements `IOwnedEntity<MusicUser>`, a contributor disposition can be granted `CanWriteOwn="True"`. The user can then edit their own tracks in the relevant label, but not tracks owned by other users.

## Workflow Rights

The same `RecordLabelAdministrator` disposition type can grant state-path access to `ApproveForRelease`. A user can then approve albums only inside labels for which they have the administrator disposition.
