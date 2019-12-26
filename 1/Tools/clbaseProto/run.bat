@echo off

::协议文件路径, 最后不要跟“\”符号
set SOURCE_FOLDER=protos

::C#编译器路径
set CS_COMPILER_PATH=Tools\protobuf-net\ProtoGen\protogen.exe
::C#文件生成路径, 最后不要跟“\”符号
set CS_TARGET_PATH=..\..\Assets\Scripts\Framework\ProtoMsg

::Java编译器路径
set JAVA_COMPILER_PATH=Tools\protoc-2.6.1-win32\protoc.exe
::Java文件生成路径, 最后不要跟“\”符号
set JAVA_TARGET_PATH=out\java

::删除之前创建的文件
::del %CS_TARGET_PATH%\*.* /f /s /q
::del %JAVA_TARGET_PATH%\*.* /f /s /q

@echo.
@echo.
@echo.

::遍历所有文件
for /f "delims=" %%i in ('dir /b "%SOURCE_FOLDER%\*.proto"') do (
    
    ::生成 C# 代码
    echo %CS_COMPILER_PATH% -i:%SOURCE_FOLDER%\%%i -o:%CS_TARGET_PATH%\%%~ni.cs 
    %CS_COMPILER_PATH% -i:%SOURCE_FOLDER%\%%i -o:%CS_TARGET_PATH%\%%~ni.cs 
    
    ::生成 Java 代码
    echo %JAVA_COMPILER_PATH% --java_out=%JAVA_TARGET_PATH% %SOURCE_FOLDER%\%%i
    %JAVA_COMPILER_PATH% --java_out=%JAVA_TARGET_PATH% %SOURCE_FOLDER%\%%i
    
)
@echo.
@echo.
@echo.
echo ===========================================================
echo =======The protobuffer protocal has generator done!!!!!====
echo ===========================================================
pause 0