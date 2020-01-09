param (
	[Parameter(Mandatory=$true)] [string] $buildNumber,
	[Parameter(Mandatory=$true)] [string] $solutionDirectory
)

$buildNumberRegex = "(.+)_([1-9][0-9]*).([0-9]*).([0-9]{3,5}).([0-9]{1,2})"
$validBuildNumber = $buildNumber -match $buildNumberRegex

if ($validBuildNumber -eq $false)
{
	Write-Error "Build number passed in must be in the following format: (BuildDefinitionName)_(ProjectVersion).(date:yy)(DayOfYear)(rev:.r)"
	return
}

$buildNumberSplit = $buildNumber.Split('_')
$buildRevisionNumber = $buildNumberSplit[1] -replace ".DRAFT", ""
$versionToApply = "$buildRevisionNumber"

$assemblyValues = @{
	"Company" = "UK Hydrographic Office";
	"Copyright" = "Copyright © UK Hydrographic Office 2020";
	"Description" = "UKHO.Logging.EventHubLogProvider";
	"Product" = "UKHO.Logging.EventHubLogProvider";
	"AssemblyVersion" = $versionToApply;
	"FileVersion" = $versionToApply;
    "Version" = $versionToApply;
}

function UpdateOrAddAttribute($xmlContent, $assemblyKey, $newValue, $namespace)
{
	$propertyGroup = $xmlContent.Project.PropertyGroup
	if($propertyGroup -is [array])
	{
		$propertyGroup=$propertyGroup[0]
	}

	$propertyGroupNode = $propertyGroup.$assemblyKey

	if ($propertyGroupNode -ne $null)
	{
		Write-Host "Assembly key $assemblyKey has been located in source file - updating"
		$propertyGroup.$assemblyKey = $newValue
		return $xmlContent
	}

	Write-Host "Assembly key $assemblyKey could not be located in source file - appending"

	$newChild = $xmlContent.CreateElement($assemblyKey, $namespace)
	$newChild.InnerText = $newValue
	$propertyGroup.AppendChild($newChild)

	return $propertyGroupNode
}

(Get-ChildItem -Path $solutionDirectory -File -Filter "*.csproj" -Recurse) | ForEach-Object {
	$file = $_

	Write-Host "Updating assembly file at path: $file"
	[xml]$xmlContent = (Get-Content $file.FullName)

	$assemblyValues.Keys | ForEach-Object {
		$key = $_

		UpdateOrAddAttribute $xmlContent $key $assemblyValues[$key] $xmlContent.DocumentElement.NamespaceURI
	}

	$xmlContent.Save($file.FullName)
}