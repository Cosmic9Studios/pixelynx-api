param (
   [Parameter(Position=0, mandatory=$true)]
   [string]$Name
)

dotnet ef --startup-project ../Pixelynx.Api/ migrations add $Name