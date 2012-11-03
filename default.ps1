Framework "4.0"

properties {
  $root_dir = Split-Path $psake.build_script_file
  $build_dir = "$root_dir\build\"
  $package_dir = "$root_dir/build/package/"
  $testresult_dir = "$root_dir/build/test-results"
  
  $debug = $false
}

FormatTaskName {
   param($taskName)
   Write-Host "Execution $taskName"
}

task default -depends Test

task Clean { 
	Remove-Item "$build_dir*" -recurse -force -ErrorAction SilentlyContinue	
	New-Item $testresult_dir -type directory -force
	New-Item $package_dir -type directory -force
}

task Compile -depends Clean { 
	exec { msbuild ./Piglet.sln /p:Configuration=Release /verbosity:minimal /p:outdir=$build_dir }
}

task Test -depends Compile {
	exec { & packages\NUnit.Runners.2.6.0.12051\tools\nunit-console $build_dir/Piglet.Tests.dll /nologo /nodots /xml=$testresult_dir\Piglet.xml }
}

task PrepareForNuget -depends Clean {
	$gitVersion = getNugetVersionFromTag($debug)
	Assert ($gitVersion -ne $null) "Can only generate packages from pure tags. Set $debug to $true to be able to test the creation of packages"
	
	generateNuspec($gitVersion)
	generateSharedAssemblyInfo($gitVersion)
}

task PackageNuget -depends PrepareForNuget, Test {
	exec { & NuGet.exe pack .\build\Piglet.nuspec -OutputDirectory $package_dir }
} -PostAction { resetChangedFiles }

function resetChangedFiles() {
	exec { & git checkout ./SharedAssemblyInfo.cs }
}

# http://youtrack.jetbrains.com/issue/TW-20310
# added patch version to the script above since we would like to tag the full version e.g. v1.2.3
function getNugetVersionFromTag($allowSequenceTags) {

	$git_version = (git describe --tags --long | Select-String -pattern '(?<major>[0-9]+)\.(?<minor>[0-9]+)\.(?<patch>[0-9]+)-(?<seq>[0-9]+)-(?<hash>[a-z0-9]+)').Matches[0].Groups

	$git_describe = $git_version[0].Value
	Write-Host "##teamcity[setParameter name='git.describe' value='$git_describe']"
	ForEach ($prop_name in @('major', 'minor', 'patch', 'seq', 'hash'))
	{
	  $prop_value = $git_version[$prop_name]
	  Write-Host "##teamcity[setParameter name='git.$prop_name' value='$prop_value']"
	}
	
	If (!$allowSequenceTags -and $git_version['hash']) {
		return $null
	}
	
	$version = [string]::Join('.', @(
		$git_version['major'],
		$git_version['minor'],
		$git_version['patch']
	))
	  
	Write-Host "##teamcity[setParameter name='env.Version' value='$version']"
	Write-Host "##teamcity[buildNumber '$version']"
	
	return $version
}

function generateSharedAssemblyInfo($version) {
	"using System.Reflection;
[assembly: AssemblyProduct(""Piglet"")]
[assembly: AssemblyCopyright(""Copyright © 2012"")]
[assembly: AssemblyVersion(""$version"")]
[assembly: AssemblyFileVersion(""$version"")]" | out-file ./SharedAssemblyInfo.cs  
}

function generateNuspec($version)
{
    "<?xml version=""1.0""?>
<package xmlns=""http://schemas.microsoft.com/packaging/2010/07/nuspec.xsd"">
  <metadata>
    <id>Piglet</id>
    <version>$version</version>
    <authors>Per Dervall</authors>
    <owners>Per Dervall</owners>
    <licenseUrl>https://github.com/Dervall/Piglet/blob/master/LICENSE.txt</licenseUrl>
    <projectUrl>https://github.com/Dervall/Piglet</projectUrl>
    <iconUrl>https://raw.github.com/Dervall/Piglet/master/logo32.png</iconUrl>
    <requireLicenseAcceptance>false</requireLicenseAcceptance>
    <summary>Parser and lexer generator that does not require a pre-build step and configurable using fluent configuration</summary>
    <description>Piglet is a lightweight library for lexing and parsing text, in the spirit of those big parser and lexer genererators such as bison, antlr and flex focusing on ease of use and integration.</description>
    <tags>parser generator parser lexer fluent</tags>
  </metadata>
  <files>
    <file src=""${build_dir}Piglet.dll"" target=""lib\net40"" />
    <file src=""${build_dir}Piglet.pdb"" target=""lib\net40"" />
    <file src=""${build_dir}Piglet.xml"" target=""lib\net40"" />
  </files>
</package>" | out-file $build_dir\Piglet.nuspec
}