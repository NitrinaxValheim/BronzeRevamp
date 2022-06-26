@echo off

REM do not change lines after this

REM get config name
REM $(Configuration) = %1
set config_name=%1
rem echo %config_name%

REM set project dir
REM $(ProjectDir) = %2
set project_dir=%2
rem echo %project_dir%

REM set project name
REM $(ProjectName) = %3
set project_name=%3
rem echo %project_name%

REM author name
set creator_name=NitrinaxValheim-
rem echo %creator_name%

REM publish.log
set publish_log=%project_dir%publish.log
rem echo %publish_log%

REM set plugin file name
set plugin_file_name=%project_name%.dll
rem echo %plugin_file_name%

REM set manifest file name
set plugin_file_manifest=%project_dir%manifest.json
rem echo %plugin_file_manifest%

REM set readme file name
set plugin_file_readme=%project_dir%README.md
rem echo %plugin_file_readme%

REM set changelog file name
set plugin_file_changelog=%project_dir%Docs\CHANGELOG.md
rem echo %plugin_file_changelog%

REM set warning file name
set plugin_file_warning=%project_dir%Docs\ADD_DOCS_BEFORE_PACK.txt
rem echo %plugin_file_warning%

ADD_DOCS_BEFORE_PACK.txt

REM set icon file name
set plugin_file_icon=E:\Developer\Projects\Games\Valheim\DistSource\%project_name%\thunderstore\icon.png
rem echo %plugin_file_icon%

REM set source path of compiled mod .dll
set source_path=%project_dir%bin\%config_name%\net472
rem echo %source_path%

REM set target bepinex plugin path
set bepinex_plugin_path=D:\Games\Steam\steamapps\common\Valheim\BepInEx\plugins
rem echo %bepinex_plugin_path%

REM set target plugin dir path in bepinex plugin folder
set plugin_dir_path=%bepinex_plugin_path%\%creator_name%%project_name%
rem echo %plugin_dir_path%

REM set target deploy sub dir for plugin
set plugin_dir_subdir=%plugin_dir_path%\plugins
rem echo %plugin_dir_subdir%

REM set target deploy sub dir for plugin
set plugin_dir_docsdir=%plugin_dir_path%\Docs
rem echo %plugin_dir_docsdir%

REM remove old publish.log
if exist %publish_log% (
    del %publish_log%
)

REM set timestamp
set HR=%time:~0,2%
if "%HR:~0,1%" == " " SET HR=0%hr:~1,1%
set TI=%HR%:%time:~3,2%:%time:~6,2%
set TS=%date:~-4%-%date:~-7,2%-%date:~-10,2% %TI%

REM check for compiled source file
if exist "%source_path%\%plugin_file_name%" (
    
    echo %TS% source file found %source_path%\%plugin_file_name% >>publish.log

    REM check for plugin dir path
    if not exist "%plugin_dir_path%" (

        echo %TS% target path not found %plugin_dir_path% >>publish.log

        REM makedir plugin path
        echo %TS% creating target path %plugin_dir_path% >>publish.log
        mkdir "%plugin_dir_path%"

        REM makedir plugin path sub dir
        echo %TS% creating target path %plugin_dir_subdir% >>publish.log
        mkdir "%plugin_dir_subdir%"

        REM makedir plugin path docs dir
        echo %TS% creating target path %plugin_dir_docsdir% >>publish.log
        mkdir "%plugin_dir_docsdir%"        

    ) else (

        echo %TS% target path found %plugin_dir_path% >>publish.log

        REM check for old plugin_file
        if exist "%plugin_dir_path%\%plugin_file_name%" (

            REM del old plugin_file
            echo %TS% deleting %plugin_dir_path%\%plugin_file_name% >>publish.log
            del "%plugin_dir_path%\%plugin_file_name%"

        )

    )

    REM copy manifest file to plugin_path
    if exist "%plugin_file_manifest%" (
        echo %TS% copying %plugin_file_manifest% to %plugin_dir_path% >>publish.log
        xcopy /Y "%plugin_file_manifest%" "%plugin_dir_path%"
    )
    
    REM copy readme file to plugin_path
    if exist "%plugin_file_readme%" (
        echo %TS% copying %plugin_file_readme% to %plugin_dir_path% >>publish.log
        xcopy /Y "%plugin_file_readme%" "%plugin_dir_path%"
    )
    
    REM copy changelog file to plugin_path
    if exist "%plugin_file_changelog%" (
        echo %TS% copying %plugin_file_changelog% to %plugin_dir_docsdir% >>publish.log
        xcopy /Y "%plugin_file_changelog%" "%plugin_dir_docsdir%"
    )
    
    REM copy icon file to plugin_path
    if exist "%plugin_file_icon%" (
        echo %TS% copying %plugin_file_icon% to %plugin_dir_path% >>publish.log
        xcopy /Y "%plugin_file_icon%" "%plugin_dir_path%"
    )
    
    REM copy warning file to plugin_path
    if exist "%plugin_file_warning%" (
        echo %TS% copying %plugin_file_warning% to %plugin_dir_path% >>publish.log
        xcopy /Y "%plugin_file_warning%" "%plugin_dir_path%"
    )
    
    REM copy new plugin file to plugin_path
    echo %TS% copying %source_path%\%plugin_file_name% to %plugin_dir_path% >>publish.log
    xcopy /Y "%source_path%\%plugin_file_name%" "%plugin_dir_subdir%"

    REM start game
    if exist "%plugin_dir_subdir%\%plugin_file_name%" (
        echo %TS% starting game >>publish.log
        start steam://rungameid/892970
    )

) else (

    echo %TS% source file not exists %source_path%\%plugin_file_name% >>publish.log

)