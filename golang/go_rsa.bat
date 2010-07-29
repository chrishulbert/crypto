@echo off
if exist go_rsa.8 del go_rsa.8
if exist go_rsa.exe del go_rsa.exe
8g go_rsa.go
if exist go_rsa.8 8l -o go_rsa.exe go_rsa.8
if exist go_rsa.exe go_rsa.exe
pause