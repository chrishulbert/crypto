@echo off

rem Compile
ghc -o rsa rsa.hs

rem Run it
rsa.exe

rem Clean up and wait
del rsa.hi
del rsa.o
pause