# Access Layers

Domos access checking is applied at several layers.

Entity access determines whether users can read, create, update or delete entities. Ownership and segregation can refine these decisions.

Manager access determines whether users can obtain or use logic managers and therefore reach groups of business operations.

Workflow access determines whether users can execute a `StatePath` for a particular stateful object or segregation.

The logic layer raises access-denied exceptions when these checks fail.
