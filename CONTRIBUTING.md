# Contributing

## Language policy

English is the required language for all public and user-facing content in FavoriteItems. This
includes documentation, release notes, manifest metadata, source comments, configuration
descriptions, in-game notifications, warnings, errors, and log messages.

Before preparing a release, review every changed string and run the Thunderstore packaging script.
The script checks for Portuguese phrases that appeared in earlier builds and rejects the package if
one is found in the source or compiled DLL.
