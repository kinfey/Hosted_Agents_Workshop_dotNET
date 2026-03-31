$ErrorActionPreference = 'Stop'
$mdFiles = Get-ChildItem -Recurse -File -Filter *.md
$broken = @()
$totalLinks = 0

foreach ($f in $mdFiles) {
  $content = Get-Content -Raw -LiteralPath $f.FullName
  $matches = [regex]::Matches($content, '\[[^\]]+\]\(([^)]+)\)')
  foreach ($m in $matches) {
    $target = $m.Groups[1].Value.Trim()
    if ([string]::IsNullOrWhiteSpace($target)) { continue }
    $totalLinks++
    if ($target -match '^(https?://|mailto:|javascript:|#)') { continue }
    $targetPath = $target.Split('#')[0].Split('?')[0].Trim()
    if ([string]::IsNullOrWhiteSpace($targetPath)) { continue }
    $resolved = Join-Path $f.DirectoryName $targetPath
    if (-not (Test-Path -LiteralPath $resolved)) {
      $broken += [PSCustomObject]@{ Source = $f.FullName; Target = $target }
    }
  }
}

$oldRefs = git grep -n '2-core-guided.md' -- '*.md'

Write-Output ("MD files scanned: {0}" -f $mdFiles.Count)
Write-Output ("Markdown links found: {0}" -f $totalLinks)
Write-Output ("Broken local links: {0}" -f $broken.Count)
if ($oldRefs) {
  Write-Output 'Legacy filename refs found:'
  Write-Output $oldRefs
} else {
  Write-Output 'No legacy 2-core-guided.md refs found.'
}
if ($broken.Count -gt 0) {
  $broken | ForEach-Object { Write-Output ("BROKEN: {0} -> {1}" -f $_.Source, $_.Target) }
  exit 1
}
