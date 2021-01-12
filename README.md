# Sleet.Search

An example of external search for Sleet static feeds.

Update the static feed to point to this service and pass the static search resource url.
When called by NuGet this azure function will dynamically filter the static search results
based on the query and prerelease filters.

## Setting an external search provider in Sleet 4.0.0

Set *externalsearch* to the url for your instance of Sleet.Search along with the url of the feed's static search resource.

For the example feed: `https://nuget.blob.core.windows.net/packages/index.json`
the search resource is: `https://nuget.blob.core.windows.net/packages/search/query`

Sleet.Search uses the route `search/source/{source}/query`
If the search is hosted on `https://example.org/` 
then url to add to sleet is: `https://example.org/search/source/https%3A%2F%2Fnuget.blob.core.windows.net%2Fpackages%2Fsearch%2Fquery/query`

### Sleet command

```
feed-settings --set "externalsearch:https://example.org/search/source/https%3A%2F%2Fnuget.blob.core.windows.net%2Fpackages%2Fsearch%2Fquery/query"
```

NuGet client does not support query parameters on search urls which is why the parameter here must be encoded and passed in the url itself.

## Customizing your own search

Instead of passing the static search resource in the url you can modify your own instance of Sleet.Search and hardcode it to your feed to avoid these steps.

## Further help and examples

* [How to add search to static nuget feed](https://til.cazzulino.com/dotnet/nuget/use-dotnet-vs-to-get-developer-prompt-in-terminal) - Daniel Cazzulino
