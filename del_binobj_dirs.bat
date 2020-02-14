@echo off
cd /d %~dp0

"%cd%\lib\csi\csi-vs2017.cmd" "scripts\del_binobj_dirs.csx"

pause