# Zip Authenticode

Do you already sign .msi, .exe, .dll, .ps1 and .cab files with Authenticode and just wish there was a simple way to make it work for .zip files? Look no further! Taking inspiration from the [zipsign](https://github.com/falk-werner/zipsign) project, we have adapted Authenticode to the zip file format in a way that leverages the existing Windows APIs for signing and validation.

## Self-signed Certificate

Start by creating a simple self-signed certificate for Authenticode code signing:

```powershell
$params = @{
    Subject = 'CN=ZipAuthenticode'
    Type = 'CodeSigning'
    CertStoreLocation = 'cert:\CurrentUser\My'
    HashAlgorithm = 'sha256'
}
$cert = New-SelfSignedCertificate @params
```

Find the certificate in the current user store, export it to a file without the private key, and import it into the system trusted root CAs (requires an elevated shell):

```powershell
$cert = @(Get-ChildItem cert:\CurrentUser\My -CodeSigning | Where-Object { $_.Subject -eq "CN=ZipAuthenticode" })[0]
$cert | Export-Certificate -FilePath "~\Documents\ZipAuthenticode.crt"
Import-Certificate -FilePath "~\Documents\ZipAuthenticode.crt" -CertStoreLocation "cert:\LocalMachine\Root"
```

Keep a one-liner command to obtain the correct certificate for code signing, as you will likely need it more than once. This command filters for code signing certificates in the current user certificate store that use the "ZipAuthenticode" subject name. You can also use the thumbprint to uniquely identify the certificate easily.

```powershell
PS C:\> $cert = @(Get-ChildItem cert:\CurrentUser\My -CodeSigning | Where-Object { $_.Subject -eq "CN=ZipAuthenticode" })[0]
PS C:\> $cert

   PSParentPath: Microsoft.PowerShell.Security\Certificate::CurrentUser\My

Thumbprint                                Subject              EnhancedKeyUsageList
----------                                -------              --------------------
6256DFDA7528DF20730950A4D9DC0727CE7EA404  CN=ZipAuthenticode   Code Signing
```

## PowerShell Module

Build the PowerShell module and import it:

```powershell
git clone https://github.com/Devolutions/devolutions-authenticode
cd devolutions-authenticode/PowerShell
.\build.ps1
Import-Module .\Devolutions.Authenticode
```

The `Get-ZipAuthenticodeSignature` and `Set-ZipAuthenticodeSignature` PowerShell cmdlets should now be available.

```powershell
Get-Command *ZipAuthenticode*

CommandType     Name                                               Version    Source
-----------     ----                                               -------    ------
Cmdlet          Get-ZipAuthenticodeSignature                       1.0.0.0    Devolutions.Authenticode.PowerShell
Cmdlet          Set-ZipAuthenticodeSignature                       1.0.0.0    Devolutions.Authenticode.PowerShell
```

## Signing a zip file

Copy a zip file into the current directory (you can use "data\test-unsigned.zip") and rename it to "test.zip". Get the SHA256 file hash of the original file and keep it for later:

```powershell
(Get-FileHash .\test-unsigned.zip -Algorithm SHA256) | ForEach-Object { ($_.Algorithm + ':' + $_.Hash).ToLower() }
sha256:4667433dd582f5955e7f6355cbb2a39c5e95cbccc894c1ffaa4286f1acfed0b7
```

Fetch the code signing certificate object:

```powershell
$cert = @(Get-ChildItem cert:\CurrentUser\My -CodeSigning | Where-Object { $_.Subject -eq "CN=ZipAuthenticode" })[0]
```

And then call `Set-ZipAuthenticodeSignature` on the zip file using the certificate:

```powershell
Set-ZipAuthenticodeSignature -Certificate $cert -TimestampServer 'http://timestamp.digicert.com' -FilePath .\test.zip

SignerCertificate      : [Subject]
                           CN=ZipAuthenticode

                         [Issuer]
                           CN=ZipAuthenticode

                         [Serial Number]
                           1CEDD95663204A804AEA488546F1641F

                         [Not Before]
                           2022-03-12 3:10:04 PM

                         [Not After]
                           2023-03-12 4:30:04 PM

                         [Thumbprint]
                           6256DFDA7528DF20730950A4D9DC0727CE7EA404

TimeStamperCertificate : [Subject]
                           CN=DigiCert Timestamp 2021, O="DigiCert, Inc.", C=US

                         [Issuer]
                           CN=DigiCert SHA2 Assured ID Timestamping CA, OU=www.digicert.com, O=DigiCert Inc, C=US

                         [Serial Number]
                           0D424AE0BE3A88FF604021CE1400F0DD

                         [Not Before]
                           2020-12-31 7:00:00 PM

                         [Not After]
                           2031-01-05 7:00:00 PM

                         [Thumbprint]
                           E1D782A8E191BEEF6BCA1691B5AAB494A6249BF3

Status                 : Valid
StatusMessage          : Signature verified.
Path                   : test.zip.sig.ps1
SignatureType          : Authenticode
IsOSBinary             : False
```

Congratulations, you have just signed your first zip file using Authenticode!

## Validating Zip File Signature

Call `Get-ZipAuthenticodeSignature` to object the Authenticode signature on the signed zip file:

```powershell
Get-ZipAuthenticodeSignature .\test.zip

SignerCertificate      : [Subject]
                           CN=ZipAuthenticode

                         [Issuer]
                           CN=ZipAuthenticode

                         [Serial Number]
                           1CEDD95663204A804AEA488546F1641F

                         [Not Before]
                           2022-03-12 3:10:04 PM

                         [Not After]
                           2023-03-12 4:30:04 PM

                         [Thumbprint]
                           6256DFDA7528DF20730950A4D9DC0727CE7EA404

TimeStamperCertificate : [Subject]
                           CN=DigiCert Timestamp 2021, O="DigiCert, Inc.", C=US

                         [Issuer]
                           CN=DigiCert SHA2 Assured ID Timestamping CA, OU=www.digicert.com, O=DigiCert Inc, C=US

                         [Serial Number]
                           0D424AE0BE3A88FF604021CE1400F0DD

                         [Not Before]
                           2020-12-31 7:00:00 PM

                         [Not After]
                           2031-01-05 7:00:00 PM

                         [Thumbprint]
                           E1D782A8E191BEEF6BCA1691B5AAB494A6249BF3

Status                 : Valid
StatusMessage          : Signature verified.
Path                   : .sig.ps1
SignatureType          : Authenticode
IsOSBinary             : False
```

## Zip Signature Format

You may have noticed that the reported file path in the signature object is "test.zip.sig.ps1" instead "test.zip". This file is left over from the `Set-ZipAuthenticodeSignature` operation, so let's open it to see what it contains:

```
sha256:4667433dd582f5955e7f6355cbb2a39c5e95cbccc894c1ffaa4286f1acfed0b7
# SIG # Begin signature block
# MIIR2AYJKoZIhvcNAQcCoIIRyTCCEcUCAQExCzAJBgUrDgMCGgUAMGkGCisGAQQB
# gjcCAQSgWzBZMDQGCisGAQQBgjcCAR4wJgIDAQAABBAfzDtgWUsITrck0sYpfvNR
# AgEAAgEAAgEAAgEAAgEAMCEwCQYFKw4DAhoFAAQURbYOY7I38yZeBoKx0kC3Iqp9
# vR6ggg0/MIIDBDCCAeygAwIBAgIQHO3ZVmMgSoBK6kiFRvFkHzANBgkqhkiG9w0B
# AQsFADAaMRgwFgYDVQQDDA9aaXBBdXRoZW50aWNvZGUwHhcNMjIwMzEyMjAxMDA0
# WhcNMjMwMzEyMjAzMDA0WjAaMRgwFgYDVQQDDA9aaXBBdXRoZW50aWNvZGUwggEi
# MA0GCSqGSIb3DQEBAQUAA4IBDwAwggEKAoIBAQC5eftpY1WCaq0lZ4nYd43646x/
# rng40z/RrNFfxrAXtI1nLve5QQ75Das65xanvMjnehlXX2SweU1yy1X5tLvIeHb5
# KED4DI03q64Cn7QqtLYFQLFmv868ZIpSB2/URPDgSYn1i7s3yoxxpCjSCZgbaR1n
# HrwBTyDaQWbP5kLSwDo5sw4iehvXBXUmOnbknTa7N/iOy4s5bN/bJH0rtiEXQAWt
# /EvXO4cff4za4/mCBTCbK9ZjzNDlf5t9njd9J/myalYGnSjq04QqTfeUyuZ1RqFY
# zJWhKev/vUhUsBFtOexvz4UBYFU7WwDz4uNUiq24C09nQLhEs4OEGQ1IactZAgMB
# AAGjRjBEMA4GA1UdDwEB/wQEAwIHgDATBgNVHSUEDDAKBggrBgEFBQcDAzAdBgNV
# HQ4EFgQUCm9taG963syCKfZi5gRNYH/BBrIwDQYJKoZIhvcNAQELBQADggEBAHNC
# e+un62MBUuR81qo+2QUvDZLa0n2LV2HX8Co7ZhFIGR9b/dUTmRfsdvh2IkUhj6B8
# 9VObrntG0+DenZtuRpG10qM8uQMRJTY9OF2HoxPD5mK+NBOXT3oGyJFkv1hdypTR
# c2eOfgy7ea+bNC7MrWYEonpX0z0SMXNXezYcP2LaQdMHn7P5oGnGbE0CquxYH778
# i99Bd+EjZkkJrkPUSlh3TPZt1QCYhBGhS55csDiRGy1YkkmsiRDCowMZn355dEce
# viGuqxYdStXHIxykN9vKcMsc26FLdPACsZKJxlAyHfAoO1wh6eQY3au6d25GUok3
# fFvpExHPQvjBPKmGuxkwggT+MIID5qADAgECAhANQkrgvjqI/2BAIc4UAPDdMA0G
# CSqGSIb3DQEBCwUAMHIxCzAJBgNVBAYTAlVTMRUwEwYDVQQKEwxEaWdpQ2VydCBJ
# bmMxGTAXBgNVBAsTEHd3dy5kaWdpY2VydC5jb20xMTAvBgNVBAMTKERpZ2lDZXJ0
# IFNIQTIgQXNzdXJlZCBJRCBUaW1lc3RhbXBpbmcgQ0EwHhcNMjEwMTAxMDAwMDAw
# WhcNMzEwMTA2MDAwMDAwWjBIMQswCQYDVQQGEwJVUzEXMBUGA1UEChMORGlnaUNl
# cnQsIEluYy4xIDAeBgNVBAMTF0RpZ2lDZXJ0IFRpbWVzdGFtcCAyMDIxMIIBIjAN
# BgkqhkiG9w0BAQEFAAOCAQ8AMIIBCgKCAQEAwuZhhGfFivUNCKRFymNrUdc6EUK9
# CnV1TZS0DFC1JhD+HchvkWsMlucaXEjvROW/m2HNFZFiWrj/ZwucY/02aoH6Kfjd
# K3CF3gIY83htvH35x20JPb5qdofpir34hF0edsnkxnZ2OlPR0dNaNo/Go+EvGzq3
# YdZz7E5tM4p8XUUtS7FQ5kE6N1aG3JMjjfdQJehk5t3Tjy9XtYcg6w6OLNUj2vRN
# eEbjA4MxKUpcDDGKSoyIxfcwWvkUrxVfbENJCf0mI1P2jWPoGqtbsR0wwptpgrTb
# /FZUvB+hh6u+elsKIC9LCcmVp42y+tZji06lchzun3oBc/gZ1v4NSYS9AQIDAQAB
# o4IBuDCCAbQwDgYDVR0PAQH/BAQDAgeAMAwGA1UdEwEB/wQCMAAwFgYDVR0lAQH/
# BAwwCgYIKwYBBQUHAwgwQQYDVR0gBDowODA2BglghkgBhv1sBwEwKTAnBggrBgEF
# BQcCARYbaHR0cDovL3d3dy5kaWdpY2VydC5jb20vQ1BTMB8GA1UdIwQYMBaAFPS2
# 4SAd/imu0uRhpbKiJbLIFzVuMB0GA1UdDgQWBBQ2RIaOpLqwZr68KC0dRDbd42p6
# vDBxBgNVHR8EajBoMDKgMKAuhixodHRwOi8vY3JsMy5kaWdpY2VydC5jb20vc2hh
# Mi1hc3N1cmVkLXRzLmNybDAyoDCgLoYsaHR0cDovL2NybDQuZGlnaWNlcnQuY29t
# L3NoYTItYXNzdXJlZC10cy5jcmwwgYUGCCsGAQUFBwEBBHkwdzAkBggrBgEFBQcw
# AYYYaHR0cDovL29jc3AuZGlnaWNlcnQuY29tME8GCCsGAQUFBzAChkNodHRwOi8v
# Y2FjZXJ0cy5kaWdpY2VydC5jb20vRGlnaUNlcnRTSEEyQXNzdXJlZElEVGltZXN0
# YW1waW5nQ0EuY3J0MA0GCSqGSIb3DQEBCwUAA4IBAQBIHNy16ZojvOca5yAOjmdG
# /UJyUXQKI0ejq5LSJcRwWb4UoOUngaVNFBUZB3nw0QTDhtk7vf5EAmZN7WmkD/a4
# cM9i6PVRSnh5Nnont/PnUp+Tp+1DnnvntN1BIon7h6JGA0789P63ZHdjXyNSaYOC
# +hpT7ZDMjaEXcw3082U5cEvznNZ6e9oMvD0y0BvL9WH8dQgAdryBDvjA4VzPxBFy
# 5xtkSdgimnUVQvUtMjiB2vRgorq0Uvtc4GEkJU+y38kpqHNDUdq9Y9YfW5v3LhtP
# Ex33Sg1xfpe39D+E68Hjo0mh+s6nv1bPull2YYlffqe0jmd4+TaY4cso2luHpoov
# MIIFMTCCBBmgAwIBAgIQCqEl1tYyG35B5AXaNpfCFTANBgkqhkiG9w0BAQsFADBl
# MQswCQYDVQQGEwJVUzEVMBMGA1UEChMMRGlnaUNlcnQgSW5jMRkwFwYDVQQLExB3
# d3cuZGlnaWNlcnQuY29tMSQwIgYDVQQDExtEaWdpQ2VydCBBc3N1cmVkIElEIFJv
# b3QgQ0EwHhcNMTYwMTA3MTIwMDAwWhcNMzEwMTA3MTIwMDAwWjByMQswCQYDVQQG
# EwJVUzEVMBMGA1UEChMMRGlnaUNlcnQgSW5jMRkwFwYDVQQLExB3d3cuZGlnaWNl
# cnQuY29tMTEwLwYDVQQDEyhEaWdpQ2VydCBTSEEyIEFzc3VyZWQgSUQgVGltZXN0
# YW1waW5nIENBMIIBIjANBgkqhkiG9w0BAQEFAAOCAQ8AMIIBCgKCAQEAvdAy7kvN
# j3/dqbqCmcU5VChXtiNKxA4HRTNREH3Q+X1NaH7ntqD0jbOI5Je/YyGQmL8TvFfT
# w+F+CNZqFAA49y4eO+7MpvYyWf5fZT/gm+vjRkcGGlV+Cyd+wKL1oODeIj8O/36V
# +/OjuiI+GKwR5PCZA207hXwJ0+5dyJoLVOOoCXFr4M8iEA91z3FyTgqt30A6XLdR
# 4aF5FMZNJCMwXbzsPGBqrC8HzP3w6kfZiFBe/WZuVmEnKYmEUeaC50ZQ/ZQqLKfk
# dT66mA+Ef58xFNat1fJky3seBdCEGXIX8RcG7z3N1k3vBkL9olMqT4UdxB08r8/a
# rBD13ays6Vb/kwIDAQABo4IBzjCCAcowHQYDVR0OBBYEFPS24SAd/imu0uRhpbKi
# JbLIFzVuMB8GA1UdIwQYMBaAFEXroq/0ksuCMS1Ri6enIZ3zbcgPMBIGA1UdEwEB
# /wQIMAYBAf8CAQAwDgYDVR0PAQH/BAQDAgGGMBMGA1UdJQQMMAoGCCsGAQUFBwMI
# MHkGCCsGAQUFBwEBBG0wazAkBggrBgEFBQcwAYYYaHR0cDovL29jc3AuZGlnaWNl
# cnQuY29tMEMGCCsGAQUFBzAChjdodHRwOi8vY2FjZXJ0cy5kaWdpY2VydC5jb20v
# RGlnaUNlcnRBc3N1cmVkSURSb290Q0EuY3J0MIGBBgNVHR8EejB4MDqgOKA2hjRo
# dHRwOi8vY3JsNC5kaWdpY2VydC5jb20vRGlnaUNlcnRBc3N1cmVkSURSb290Q0Eu
# Y3JsMDqgOKA2hjRodHRwOi8vY3JsMy5kaWdpY2VydC5jb20vRGlnaUNlcnRBc3N1
# cmVkSURSb290Q0EuY3JsMFAGA1UdIARJMEcwOAYKYIZIAYb9bAACBDAqMCgGCCsG
# AQUFBwIBFhxodHRwczovL3d3dy5kaWdpY2VydC5jb20vQ1BTMAsGCWCGSAGG/WwH
# ATANBgkqhkiG9w0BAQsFAAOCAQEAcZUS6VGHVmnN793afKpjerN4zwY3QITvS4S/
# ys8DAv3Fp8MOIEIsr3fzKx8MIVoqtwU0HWqumfgnoma/Capg33akOpMP+LLR2HwZ
# YuhegiUexLoceywh4tZbLBQ1QwRostt1AuByx5jWPGTlH0gQGF+JOGFNYkYkh2OM
# kVIsrymJ5Xgf1gsUpYDXEkdws3XVk4WTfraSZ/tTYYmo9WuWwPRYaQ18yAGxuSh1
# t5ljhSKMYcp5lH5Z/IwP42+1ASa2bKXuh1Eh5Fhgm7oMLSttosR+u8QlK0cCCHxJ
# rhO24XxCQijGGFbPQTS2Zl22dHv1VjMiLyI2skuiSpXY9aaOUjGCBAMwggP/AgEB
# MC4wGjEYMBYGA1UEAwwPWmlwQXV0aGVudGljb2RlAhAc7dlWYyBKgErqSIVG8WQf
# MAkGBSsOAwIaBQCgeDAYBgorBgEEAYI3AgEMMQowCKACgAChAoAAMBkGCSqGSIb3
# DQEJAzEMBgorBgEEAYI3AgEEMBwGCisGAQQBgjcCAQsxDjAMBgorBgEEAYI3AgEV
# MCMGCSqGSIb3DQEJBDEWBBQd12oYFtYD2RrO2YbEOurpoGz8KjANBgkqhkiG9w0B
# AQEFAASCAQCUevV4BNm6XpeNBdDi84OyQP/qz4guQJfeEoTfwLbrvNtu7wqghCtM
# UlXyN9+3AJBuyPgzmXWXX2cuSme1AOQZLuvf8x405/37h5B7lhUHN+pCwcSjLB+T
# IfDq6m6P6uynRltbDzUTRBAboCnZwzvpcFaLtX1ve/sCB8HeDx6rRobw37nhGr5y
# LR89tG/gcGrQ/gAwe8DitRQ/e2f+lMEn/LUMlzzt7Tx09zBcTNkcMHZXGrwFHwJE
# e9UBHTUMGOCAslAFIqOT1WznRYMpHmuvLhC6zF4xTCC8nNcF6LxVIW9a57MbJKRJ
# UJr+Ugxnf7Dg/rP2ccX/Wxc72qy4lOj6oYICMDCCAiwGCSqGSIb3DQEJBjGCAh0w
# ggIZAgEBMIGGMHIxCzAJBgNVBAYTAlVTMRUwEwYDVQQKEwxEaWdpQ2VydCBJbmMx
# GTAXBgNVBAsTEHd3dy5kaWdpY2VydC5jb20xMTAvBgNVBAMTKERpZ2lDZXJ0IFNI
# QTIgQXNzdXJlZCBJRCBUaW1lc3RhbXBpbmcgQ0ECEA1CSuC+Ooj/YEAhzhQA8N0w
# DQYJYIZIAWUDBAIBBQCgaTAYBgkqhkiG9w0BCQMxCwYJKoZIhvcNAQcBMBwGCSqG
# SIb3DQEJBTEPFw0yMjAzMTUxNzI1MDNaMC8GCSqGSIb3DQEJBDEiBCCpGJlS66sl
# ngoou7SKFpsLJTTcWPEJmzWTiEz1M1GRvjANBgkqhkiG9w0BAQEFAASCAQA07rIo
# adJh+rrIMXwy3D3RspJupxlCoGImxzXQGwBNcN1uPoUjwmnBi++i3dZnMafLdetC
# WfxCOOy8+xRvEy0ossv4rvU//xDe0O0LCrFK6tjuhhuZezKvz8YI+eOB9LrDFv1l
# G9l7h7Eh3HdeCiBOkG1OreAtcXeDb2fdg+t3URfEacAoJKdTC9w69wFU2c/h6DeZ
# fB8nVjlJk/WdwEZp2FYi3huEv+s4ZiMC7iGzlilL3eRtQwpSmlSNPVh5p3QeUxmz
# XR+TwLYu43RZTovFPnZ8i/8q9N4J3QmWDw4xknpiDBPqWJOQ+VI+nqLKwaCphOk7
# hzTR1bUjsM+zHDA1
# SIG # End signature block
```

Since Authenticode doesn't support the zip file format natively, we use a file hash of the zip file as if it had no comment appended at the end of it. We convert this hash to the [OCI digest string](https://github.com/opencontainers/image-spec/blob/v1.0.1/descriptor.md#digests) string format and create a one-line .sig.ps1 file with it. This file is then signed like a PowerShell script, except it doesn't contain executable code. The digest string and the signature block are then reformatted to be embedded as a single-line comment inside the zip file format, like this:

```
ZipAuthenticode=sha256:4667433dd582f5955e7f6355cbb2a39c5e95cbccc894c1ffaa4286f1acfed0b7,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
```

To validate the signature, we extract lines beginning with "ZipAuthenticode" from the zip file comment field, and reconstruct original script formatting. We then compute the zip file digest excluding the comment field itself, compare it with the digest embedded in the signature, and validate the signature file as a PowerShell script. If the digest strings match and the signature on the script is valid, then the zip file is correctly signed.
