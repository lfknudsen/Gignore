param ($o = $(throw "Output directory parameter ('-o') is required."))

$subDirs = Get-ChildItem -Path $(Get-Location) -Directory

foreach ($dir in $subDirs) {
    $proj = Get-ChildItem -Path $dir -File -Filter "*.fsproj"
    if ($proj.Exists) {
        $selection = Select-String $proj -Pattern "<OutputType>Exe</OutputType>"
        if ($selection -ne $null) {
            dotnet publish $dir -o $o
        }
    }
}