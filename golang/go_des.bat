@echo off
if exist go_des.8   del go_des.8
if exist go_des.exe del go_des.exe
8g go_des.go
if exist go_des.8   8l -o go_des.exe go_des.8
if exist go_des.exe go_des.exe
pause