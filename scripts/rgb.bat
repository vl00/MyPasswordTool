@echo off & title rgb

cd /d %~dp0rgb
del "log*.log" 1>nul 2>nul
del "rg.info.json" 1>nul 2>nul
call rgb.exe
copy /y "rg.info.json" "..\rg.redux\rg.info.json"
copy /y "rg.info.json" "..\rg.map.mvvm\rg.info.json"

pause