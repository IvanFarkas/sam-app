@echo off

for /R %%f in (bin,obj) do (
	if exist "%%f" rmdir "%%f" /s /q
)
