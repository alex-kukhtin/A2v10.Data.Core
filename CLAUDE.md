# A2v10.Data.Core

## Deferred

- **Tenant data source selection for every method.** Open; all decisions deferred.
  Likely place: `ITenantManager` (scoped in the platform, already gets `source` per connection),
  resolved once in `GetConnection*` before both `ConnectionString` and `SetTenantId`.
  Open: member name, default implementation (implementations exist outside this package:
  `WebTenantManager`, `WebApplicationHost`), whether `IDbContext.ConnectionString` resolves too.
  Not `IDataConfiguration`: singleton, `MetadataCache` (singleton) depends on it.
- **`SqlDbContext.ResolveSource`** (`{{key}}` from `ExpandoObject` parameters, `LoadModel*` and
  `LoadStream*` only) serves a different purpose — it is not the tenant mechanism. Do not add call
  sites to other methods.
