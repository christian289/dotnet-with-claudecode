# Source Link Integration

Connect embedded PDB to source code repositories for debugging.

---

## GitHub

```xml
<ItemGroup>
  <PackageReference Include="Microsoft.SourceLink.GitHub" Version="8.0.*" PrivateAssets="All"/>
</ItemGroup>

<PropertyGroup>
  <DebugType>embedded</DebugType>
  <EmbedUntrackedSources>true</EmbedUntrackedSources>
  <PublishRepositoryUrl>true</PublishRepositoryUrl>
</PropertyGroup>
```

---

## Azure DevOps

```xml
<ItemGroup>
  <PackageReference Include="Microsoft.SourceLink.AzureRepos.Git" Version="8.0.*" PrivateAssets="All"/>
</ItemGroup>

<PropertyGroup>
  <DebugType>embedded</DebugType>
  <EmbedUntrackedSources>true</EmbedUntrackedSources>
  <PublishRepositoryUrl>true</PublishRepositoryUrl>
</PropertyGroup>
```

---

## GitLab

```xml
<ItemGroup>
  <PackageReference Include="Microsoft.SourceLink.GitLab" Version="8.0.*" PrivateAssets="All"/>
</ItemGroup>

<PropertyGroup>
  <DebugType>embedded</DebugType>
  <EmbedUntrackedSources>true</EmbedUntrackedSources>
  <PublishRepositoryUrl>true</PublishRepositoryUrl>
</PropertyGroup>
```

---

## Troubleshooting

| Issue | Solution |
|-------|----------|
| Source Link not working | Verify `<PublishRepositoryUrl>true</PublishRepositoryUrl>` |
| Sources not embedded | Add `<EmbedUntrackedSources>true</EmbedUntrackedSources>` |

---

## Resources

- [Source Link Documentation](https://learn.microsoft.com/en-us/dotnet/standard/library-guidance/sourcelink)
