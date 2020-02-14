@echo off
del "%~dp0rg.redux\log*.log" 1>nul 2>nul
cmd /k " cd /d %~dp0rg.redux & call rg.exe "