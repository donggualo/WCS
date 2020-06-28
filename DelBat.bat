@echo off

set srcDir="本地LOG路径"

set daysAgo=7

echo Delete the Log 7 days ago...

forfiles /p %srcDir% /s /d -%daysAgo% /c "cmd /c del /f /s /q @path && rd /s /q @path"

echo Over.