param($installPath, $toolsPath, $package, $project)

Write-Host "Starting"

$vsSolution = Get-Interface $dte.Solution ([EnvDTE80.Solution2])
Write-Host $vsSolution

$solutionItemsNode = $vsSolution.Projects | where-object { $_.ProjectName -eq "newFolder" } | select -first 1

if (!$solutionItemsNode) {
	$solutionItemsNode = $vsSolution.AddSolutionFolder("newFolder")
} 
Write-Host "newFolder now there"

$projectItems = Get-Interface $solutionItemsNode.ProjectItems ([EnvDTE.ProjectItems])

Write-Host "projectItems"
Write-Host $projectItems

$projectItemsNode = $solutionItemsNode.ProjectItems | where-object { $_.Name -eq "loggingSettings.config" } | select -first 1

Write-Host "projectItemsNode"
Write-Host $projectItemsNode

$logSettingsNewPath = Join-Path $toolsPath '..\..\..\loggingSettings.config'
$logSettingsConfigPath = Join-Path $toolsPath '..\config\loggingSettings.config' 
	
Write-Host "Copy from"
Write-Host $logSettingsConfigPath
Write-Host "Copy to"
Write-Host $logSettingsNewPath

#copy the config to the sln dir, we need to be able to get to it during the custom target build step
cp $logSettingsConfigPath $logSettingsNewPath 
	
if(!$projectItemsNode){ 	
	Write-Host "adding to sln/newFolder"
	$projectItemsNode = $projectItems.AddFromFile($logSettingsNewPath)	
	Write-Host "added to sln/newFolder"
}