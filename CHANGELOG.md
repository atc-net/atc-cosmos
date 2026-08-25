# Changelog

## [2.0.0](https://github.com/atc-net/atc-cosmos/compare/v1.1.46...v2.0.0) (2026-08-25)


### ⚠ BREAKING CHANGES

* **reader:** throw on non-success status codes in FindAsync
* **deps:** bump NuGet packages across solution
* upgrade Microsoft.Azure.Cosmos to 3.61.0 and remove preview build

### Features

* upgrade Microsoft.Azure.Cosmos to 3.61.0 and remove preview build ([0d7f06e](https://github.com/atc-net/atc-cosmos/commit/0d7f06ea23bf95de9367775601fe5bf1692f979d))


### Bug Fixes

* **reader:** throw on non-success status codes in FindAsync ([eafa9a7](https://github.com/atc-net/atc-cosmos/commit/eafa9a746be64c4812202bea7c30f5142627b197))
* **sample:** fix build errors and modernize sample projects ([794684b](https://github.com/atc-net/atc-cosmos/commit/794684b8c7569bd605cc9cddc8a507895ddf75f4))
* **sample:** fix partition key, switch to Scalar, run against local lib ([9d9e5a2](https://github.com/atc-net/atc-cosmos/commit/9d9e5a2e6e21bccde9ecb9e4deb45c4882ef5861))
* **serialization:** deserialize non-MemoryStream Cosmos responses ([9364dc0](https://github.com/atc-net/atc-cosmos/commit/9364dc0d019ec53c1422edf7898e9c41f364f939))


### Performance Improvements

* **reader:** read FindAsync as stream to avoid throwing on 404 ([f7b547a](https://github.com/atc-net/atc-cosmos/commit/f7b547abee69d7c71de62a39372ebc6a14e0d330))


### Miscellaneous Chores

* **deps:** bump NuGet packages across solution ([71c281d](https://github.com/atc-net/atc-cosmos/commit/71c281d129b7b618b63167a9e34b274e5c965345))

## Changelog
