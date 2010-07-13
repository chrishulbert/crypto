@echo off
rem You may want to update the paths below depending on your version of VS
set path=%path%;C:\Program Files\Microsoft Visual Studio 8\VC\bin
set path=%path%;C:\Program Files\Microsoft Visual Studio 8\Common7\IDE
set lib=%lib%;C:\Program Files\Microsoft Visual Studio 8\VC\lib
set include=%include%;C:\Program Files\Microsoft Visual Studio 8\VC\include

rem The /Tp option compiles it as C++ to get the advantage of C99 features
cl /Tp c_rsa.c
del c_rsa.obj
c_rsa.exe
pause
