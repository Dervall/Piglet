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

task default -depends Compile

task Clean { 
	Remove-Item "$build_dir*" -recurse -force -ErrorAction SilentlyContinue	
}

task Compile -depends Clean { 
	exec { msbuild ./Piglet.sln /p:Configuration=Release /verbosity:minimal /p:outdir=$build_dir }
}
