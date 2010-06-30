@echo off
if exist go_aes.8 del go_aes.8
if exist go_aes.exe del go_aes.exe
8g go_aes.go
if exist go_aes.8 8l -o go_aes.exe go_aes.8
if exist go_aes.exe go_aes.exe
pause