param ($InstallPath, $ToolsPath, $Package, $Project)

$TargetsFile = 'SplashGenerator.targets'
$TargetsPath = $ToolsPath | Join-Path -ChildPath $TargetsFile

$ProjectFullName = $Project.FullName

Add-Type -AssemblyName 'Microsoft.Build, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a'

$MSBProject = [Microsoft.Build.Evaluation.ProjectCollection]::GlobalProjectCollection.GetLoadedProjects($ProjectFullName) | Select-Object -First 1

$ProjectDirectory = $MSBProject.DirectoryPath
$ProjectUri = New-Object -TypeName Uri -ArgumentList $ProjectFullName
$TargetUri = New-Object -TypeName Uri -ArgumentList "$TargetsPath"

$RelativePath = $ProjectUri.MakeRelativeUri($TargetUri) -replace '/','\'

$ExistingImports = $MSBProject.Xml.Imports |
    Where-Object { $_.Project -like "*\$TargetsFile" }
if ($ExistingImports) {
    $ExistingImports | 
        ForEach-Object { $MSBProject.Xml.RemoveChild($_) | Out-Null }
}

$Import = $MSBProject.Xml.AddImport($RelativePath)
$Import.Condition = "Exists('" + $RelativePath + "')"

$Project.Save()

$Item = $MSBProject.GetItems('SplashScreen') | Select-Object -First 1
if (!$Item)
{
    Get-ChildItem -Path $ToolsPath -Filter 'Splash.*' | 
        Where-Object { -not (($ProjectDirectory | Join-Path -ChildPath $_.Name) | Test-Path ) } | 
        Copy-Item -Destination $ProjectDirectory

    $ItemPath = $ProjectDirectory | Join-Path -ChildPath 'Splash.png'
    $ProjectItem = $Project.ProjectItems.AddFromFile($ItemPath)
    $ProjectItem.Properties.Item('ItemType').Value = 'SplashScreen'
    
    $ItemPath = $ProjectDirectory | Join-Path -ChildPath 'Splash.xaml'
    $ProjectItem = $Project.ProjectItems.AddFromFile($ItemPath)
}