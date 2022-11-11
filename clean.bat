echo ==^> Cleaning up obj and bin files

@FOR /F "delims=" %%i IN ('dir bin /s/b/ad') DO del /F/S/Q "%%i"
@FOR /F "delims=" %%i IN ('dir obj /s/b/ad') DO del /F/S/Q "%%i"
@FOR /F "delims=" %%i IN ('dir AppPackages /s/b/ad') DO del /F/S/Q "%%i"
