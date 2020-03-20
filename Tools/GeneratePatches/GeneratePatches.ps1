Push-Location e:\dcs
$gitstatus = & git status --porcelain
Pop-Location
foreach ($line in $gitstatus) {
    $filepath = $line.Substring(3)
    switch ($line.Substring(0,3)) {
        " M " {
            $source = Join-Path e:\dcs $filepath
            echo $source
            $output = Join-Path ..\Patches\DCS\002_005_005_41371 "$($filepath).gpatch"
            echo $output
            $output_directory = Split-Path -Path $output
            echo $output_directory
            if( -Not (Test-Path -Path $output_directory ) ) {
                New-Item $output_directory -ItemType Directory
            }
            Push-Location e:\dcs
            $original = & git cat-file -p master:$filepath | Out-String
            Pop-Location
            echo $original | ..\GoogleDiff\bin\x64\Release\GoogleDiff.exe $source > $output 
        }
    }
}
