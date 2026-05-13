<# Get_AICodeReview.ps1
.SYNOPSIS
  Fetches the current PR diff and asks Ollama for refactoring advice.
  Prints the suggestion to stdout.
#>

param()

function Invoke-OllamaApi {
    [CmdletBinding()]
    param (
        [Parameter(Mandatory)][string] $Model,
        [Parameter(Mandatory)][string] $Prompt,
        [string] $HostName = 'http://localhost:11434'
    )
    $body = @{
        model  = $Model
        prompt = $Prompt
        stream = $false
    } | ConvertTo-Json

    try {
        # *** FIX: Changed URI to the correct Ollama /api/generate endpoint ***
        $response = Invoke-RestMethod -Method POST `
          -Uri "$HostName/api/generate" `
          -Body $body -ContentType 'application/json'
        return $response.response # Ollama's /api/generate endpoint returns 'response' field for the generated text
    }
    catch {
        Write-Error "Ollama API call failed: $_"
        return $null
    }
}

# Main
git fetch origin $Env:GITHUB_BASE_REF
$diff = git diff "origin/$($Env:GITHUB_BASE_REF)" $Env:GITHUB_SHA

# --- DEBUGGING: Print the diff content to the console ---
#Write-Host "--- PR Diff for Debugging ---"
#Write-Host $diff
#Write-Host "---------------------------"

if ([string]::IsNullOrWhiteSpace($diff)) {
    Write-Output ''
    exit 0
}

$prompt = @"
You are a senior software engineer performing a pull request code review.

Below is a Git diff.

1. Identify only the **most critical issues** (bugs, missing logic, bad practices).
2. Ignore minor style or formatting issues.
3. For each issue, respond as a JSON object with:
   - **file**: filename
   - **line**: line number
   - **issue**: short but clear description
   - **impact**: High, Medium, or Low
   - **suggestion**: precise developer action

Respond with a **JSON array only**.
Only give me critical issues.

--- Git dif---
$diff
"@


$suggestion = Invoke-OllamaApi -Model $Env:OLLAMA_MODEL -Prompt $prompt -HostName $Env:OLLAMA_HOSTNAME

# Print only the suggestion (will be captured by the workflow)
if (-not [string]::IsNullOrWhiteSpace($suggestion)) {
    Write-Output $suggestion.Trim()
}