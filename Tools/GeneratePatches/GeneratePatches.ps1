param ($dcsroot = "e:\dcs")
Push-Location $dcsroot
$gitstatus = & git status --porcelain
Pop-Location
foreach ($line in $gitstatus) {
    $filepath = $line.Substring(3)
    switch ($line.Substring(0,3)) {
        " M " {
            $source = Join-Path $dcsroot $filepath
            $output = Join-Path ..\Patches\DCS\002_005_005_41371 "$($filepath).gpatch"
            $revert_output = Join-Path ..\Patches\DCS\002_005_005_41371 "$($filepath).grevert"
            $output_directory = Split-Path -Path $output
            if( -Not (Test-Path -Path $output_directory ) ) {
                New-Item $output_directory -ItemType Directory
            }
            Push-Location e:\dcs
            $original = & git cat-file -p master:$filepath | Out-String
            Pop-Location
            echo $output
            echo $original | ..\GoogleDiff\bin\x64\Release\GoogleDiff.exe $source > $output
            echo $revert_output
            echo $original | ..\GoogleDiff\bin\x64\Release\GoogleDiff.exe $source --reverse > $revert_output 
        }
    }
}
