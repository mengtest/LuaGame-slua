@echo off

:: lua paths define
set LUAJIT_PATH=luajit-2.1.0-beta3
set STANDARD_LUA_PATH=lua-5.1.5

:: deciding whether to use luajit or not
:: set USE_STANDARD_LUA=%1%
set USE_LUA_PATH=%LUAJIT_PATH%
:: if "%USE_STANDARD_LUA%"=="YES" (set USE_LUA_PATH=%STANDARD_LUA_PATH%)

:: get visual studio tools path

set VS_TOOL_VER=vs140
set VCVARS="C:\Program Files (x86)\Microsoft Visual Studio 14.0\VC\bin\"
goto build


:build
set ENV32="%VCVARS%vcvars32.bat"
set ENV64="%VCVARS%amd64\vcvars64.bat"

copy /Y slua.c "%USE_LUA_PATH%\src\"
copy /Y luasocket-mini\*.* "%USE_LUA_PATH%\src\"
copy /Y pbc\pbc.h "%USE_LUA_PATH%\src\"
copy /Y pbc\binding\lua\pbc-lua.c "%USE_LUA_PATH%\src\"
copy /Y pbc\src\*.* "%USE_LUA_PATH%\src\"

call "%ENV64%"
echo Swtich to x64 build env(%VS_TOOL_VER%)
cd %USE_LUA_PATH%\src
call msvcbuild.bat gc64
copy /Y lua51.dll ..\..\..\Assets\Plugins\x64\slua.dll
copy /Y lua51.dll ..\..\..\jit\win\x64\lua51.dll
copy /Y luajit.exe ..\..\..\jit\win\x64\luajit.exe
cd ..\..

goto :eof

:missing
echo Can't find Visual Studio, compilation fails!

goto :eof
