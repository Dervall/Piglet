Framework "4.0"

properties {
  $root_dir = Split-Path $psake.build_script_file
  $build_dir = "$root_dir/build/"
  $package_dir = "$root_dir/build/package/"
  
  $nunit_dir = "$root_dir/Source\packages\NUnit.2.5.10.11092\tools"
  $env:Path += ";$nunit_dir"
}

FormatTaskName {
   param($taskName)
   # TeamCity-ReportBuildProgress "Executing Task: $taskName"
}

task default -depends Test

task Clean { 
	Remove-Item "$build_dir*" -recurse -force -ErrorAction SilentlyContinue	
}

task Compile -depends Clean { 
	exec { msbuild ./Piglet.sln /p:Configuration=Release /verbosity:minimal /p:outdir=$build_dir }
}

task Test {
	##create_directory "$build_dir\results"
    exec { & packages\NUnit.Runners.2.6.0.12051\tools\nunit-console $build_dir/Piglet.Tests.dll /nologo /nodots }
}

task test2 {
	create_directory "$build_dir\results"
    exec { & $tools_dir\nunit\nunit-console-x86.exe $build_dir/$config/UnitTests/AutoMapper.UnitTests.dll /nologo /nodots /xml=$result_dir\AutoMapper.xml }
    exec { & $tools_dir\nunit\nunit-console-x86.exe $build_dir/$config/UnitTests.Silverlight/AutoMapper.UnitTests.Silverlight.dll /nologo /nodots /xml=$result_dir\AutoMapper.Silverlight.xml }
}

