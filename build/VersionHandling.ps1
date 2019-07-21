function XmlPeek(
    [Parameter(Mandatory=$true)] [string] $FilePath,
    [Parameter(Mandatory=$true)] [string] $XPath,
    [HashTable] $NamespaceUrisByPrefix
) {
    $document = [xml](Get-Content $FilePath)
    $namespaceManager = [System.Xml.XmlNamespaceManager]::new($document.NameTable)

    if ($null -ne $NamespaceUrisByPrefix) {
        foreach ($prefix in $NamespaceUrisByPrefix.Keys) {
            $namespaceManager.AddNamespace($prefix, $NamespaceUrisByPrefix[$prefix]);
        }
    }

    return $document.SelectSingleNode($XPath, $namespaceManager).Value
}

function XmlPoke(
    [Parameter(Mandatory=$true)] [string] $FilePath,
    [Parameter(Mandatory=$true)] [string] $XPath,
    [Parameter(Mandatory=$true)] [string] $Value,
    [HashTable] $NamespaceUrisByPrefix
) {
    $document = [System.Xml.XmlDocument]::new()
    $document.PreserveWhitespace = $true
    $document.Load((Resolve-Path $FilePath))

    $namespaceManager = [System.Xml.XmlNamespaceManager]::new($document.NameTable)

    if ($null -ne $NamespaceUrisByPrefix) {
        foreach ($prefix in $NamespaceUrisByPrefix.Keys) {
            $namespaceManager.AddNamespace($prefix, $NamespaceUrisByPrefix[$prefix]);
        }
    }

    $document.SelectSingleNode($XPath, $namespaceManager).Value = $Value
    $document.Save((Resolve-Path $FilePath))
}

function SemanticVersionToFileVersion([Parameter(Mandatory=$true)] [string] $SemanticVersion) {
    $endOfPrefix = $SemanticVersion.IndexOfAny(@('-', '+'))

    $prefix = if ($endOfPrefix -eq -1) { $SemanticVersion } else { $SemanticVersion.Substring(0, $endOfPrefix) }

    return [version]$prefix
}

function Set-VsixVersionFromCsprojVersion(
    [Parameter(Mandatory=$true)] [string] $CsprojPath,
    [Parameter(Mandatory=$true)] [string] $VsixManifestPath
) {
    $versionFromCsproj = XmlPeek `
        $CsprojPath `
        '/Project/PropertyGroup/Version/text()'

    $fileVersion = SemanticVersionToFileVersion $versionFromCsproj

    XmlPoke `
        $VsixManifestPath `
        '/vsx:PackageManifest/vsx:Metadata/vsx:Identity/@Version' `
        -Value $fileVersion `
        -NamespaceUrisByPrefix @{ vsx = 'http://schemas.microsoft.com/developer/vsx-schema/2011' }
}
