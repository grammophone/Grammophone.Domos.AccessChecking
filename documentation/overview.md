# Overview

`Grammophone.Domos.AccessChecking` provides access-checking services used by the Domos logic layer.

The library works with the domain security ontology: roles, dispositions, ownership, segregation, managers and workflow state paths. Logic sessions use access resolvers to decide whether the active or acting user may perform an operation.

Concrete applications are expected to supply or configure access rules suitable for their business model.
