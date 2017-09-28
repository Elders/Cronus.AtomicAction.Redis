@echo off

%FAKE% %NYX% "target=clean" -st
%FAKE% %NYX% "target=RestoreNugetPackages" -st

IF NOT [%1]==[] (set RELEASE_NUGETKEY="%1")

SET SUMMARY="Cronus.AtomicAction.Redis"
SET DESCRIPTION="Cronus.AtomicAction.Redis"

%FAKE% %NYX% appName=Elders.Cronus.AtomicAction.Redis appSummary=%SUMMARY% appDescription=%DESCRIPTION% nugetPackageName=Cronus.AtomicAction.Redis nugetkey=%RELEASE_NUGETKEY%
