@echo off
del "%~dp0rg.map.mvvm\log*.log" 1>nul 2>nul
cmd /k " cd /d %~dp0rg.map.mvvm & call rg.exe "