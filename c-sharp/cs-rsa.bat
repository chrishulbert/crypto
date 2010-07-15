@echo off
set path=%path%;C:\WINDOWS\Microsoft.NET\Framework\v2.0.50727

del cs-rsa.exe > nul
csc cs-rsa.cs /debug
cs-rsa.exe
del cs-rsa.pdb
pause
