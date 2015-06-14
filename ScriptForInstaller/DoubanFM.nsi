﻿; Script generated by the HM NIS Edit Script Wizard.

SetCompressor /FINAL /SOLID lzma
!include "FileFunc.nsh"
!insertmacro GetParameters

; HM NIS Edit Wizard helper defines
!define PRODUCT_NAME "豆瓣电台"
!define PRODUCT_PUBLISHER "K.F.Storm"
!define PRODUCT_WEB_SITE "http://www.kfstorm.com/doubanfm"
!define PRODUCT_DIR_REGKEY "Software\Microsoft\Windows\CurrentVersion\App Paths\DoubanFM.exe"
!define PRODUCT_UNINST_KEY "Software\Microsoft\Windows\CurrentVersion\Uninstall\${PRODUCT_NAME}"
!define PRODUCT_UNINST_ROOT_KEY "HKLM"

; MUI 1.67 compatible ------
!include "MUI.nsh"

; MUI Settings
!define MUI_ABORTWARNING
!define MUI_ICON "DoubanFM_Icon_Install.ico"
!define MUI_UNICON "DoubanFM_Icon_Uninstall.ico"
!define MUI_WELCOMEFINISHPAGE_BITMAP "DoubanFM_Install_Welcome.bmp"
!define MUI_WELCOMEPAGE_TITLE "欢迎使用${PRODUCT_NAME}"
!define MUI_WELCOMEPAGE_TEXT "　　打开豆瓣电台客户端，聆听动人的音乐，无需打开网页就能收听豆瓣电台。清新的界面，平滑的动画，快捷的操作，体验如此美妙。\r\n\r\n　　$_CLICK"

; Welcome page
!insertmacro MUI_PAGE_WELCOME
; License page
;!insertmacro MUI_PAGE_LICENSE "..\README.txt"
; Directory page
!insertmacro MUI_PAGE_DIRECTORY
; Instfiles page
!insertmacro MUI_PAGE_INSTFILES
; Finish page
!define MUI_FINISHPAGE_RUN "$INSTDIR\DoubanFM.exe"
!insertmacro MUI_PAGE_FINISH

; Uninstaller pages
!insertmacro MUI_UNPAGE_INSTFILES

; Language files
!insertmacro MUI_LANGUAGE "SimpChinese"

; MUI end ------

Name "${PRODUCT_NAME}"
InstallDir "$PROGRAMFILES64\K.F.Storm\豆瓣电台"
InstallDirRegKey HKLM "${PRODUCT_DIR_REGKEY}" ""
ShowInstDetails nevershow
ShowUnInstDetails nevershow
BrandingText "　K.F.Storm"
RequestExecutionLevel admin
Section "MainSection" SEC01
  SetOutPath "$INSTDIR"
  SetOverwrite on
  File /r "bin\*.*"
  CreateDirectory "$SMPROGRAMS\豆瓣电台"
  CreateShortCut "$SMPROGRAMS\豆瓣电台\豆瓣电台.lnk" "$INSTDIR\DoubanFM.exe"
  CreateShortCut "$DESKTOP\豆瓣电台.lnk" "$INSTDIR\DoubanFM.exe"
SectionEnd

Section -AdditionalIcons
  CreateShortCut "$SMPROGRAMS\豆瓣电台\卸载豆瓣电台.lnk" "$INSTDIR\uninst.exe"
SectionEnd

Section -Post
  WriteUninstaller "$INSTDIR\uninst.exe"
  WriteRegStr HKLM "${PRODUCT_DIR_REGKEY}" "" "$INSTDIR\DoubanFM.exe"
  WriteRegStr ${PRODUCT_UNINST_ROOT_KEY} "${PRODUCT_UNINST_KEY}" "DisplayName" "$(^Name)"
  WriteRegStr ${PRODUCT_UNINST_ROOT_KEY} "${PRODUCT_UNINST_KEY}" "UninstallString" "$INSTDIR\uninst.exe"
  WriteRegStr ${PRODUCT_UNINST_ROOT_KEY} "${PRODUCT_UNINST_KEY}" "DisplayIcon" "$INSTDIR\DoubanFM.exe"
  WriteRegStr ${PRODUCT_UNINST_ROOT_KEY} "${PRODUCT_UNINST_KEY}" "DisplayVersion" "${PRODUCT_VERSION}"
  WriteRegStr ${PRODUCT_UNINST_ROOT_KEY} "${PRODUCT_UNINST_KEY}" "URLInfoAbout" "${PRODUCT_WEB_SITE}"
  WriteRegStr ${PRODUCT_UNINST_ROOT_KEY} "${PRODUCT_UNINST_KEY}" "Publisher" "${PRODUCT_PUBLISHER}"
SectionEnd

Function ExitDoubanFMWarnning
Check1:
  FindProcDLL::FindProc "DoubanFM.exe"  
  Pop $R0
  IntCmp $R0 1 +1 Check1End
  MessageBox MB_ICONEXCLAMATION|MB_OKCANCEL "安装程序检测到 ${PRODUCT_NAME} 正在运行，请退出程序后继续。" IDOK +2
  Abort
  Goto Check1
Check1End:
FunctionEnd

Function un.ExitDoubanFMWarnning
Check2:
  FindProcDLL::FindProc "DoubanFM.exe"  
  Pop $R0
  IntCmp $R0 1 +1 Check2End
  MessageBox MB_ICONEXCLAMATION|MB_OKCANCEL "安装程序检测到 ${PRODUCT_NAME} 正在运行，请退出程序后继续。" IDOK +2
  Abort
  Goto Check2
Check2End:
FunctionEnd

Function .onInit
  ReadRegDWORD $0 HKLM "SOFTWARE\Microsoft\NET Framework Setup\NDP\v4\Client" Install 
  IntCmp $0 1 +2
  MessageBox MB_ICONEXCLAMATION|MB_OK "您尚未安装 .NET Framework 4.0，$(^Name)可能无法正常使用。"
  IfSilent +1 +2
	FindProcDLL::WaitProcEnd "DoubanFM.exe" -1
  Call ExitDoubanFMWarnning
FunctionEnd

Function .onInstSuccess
  IfSilent +1 +2
	Exec "$INSTDIR\DoubanFM.exe"
FunctionEnd

Function un.onUninstSuccess
  HideWindow
  MessageBox MB_ICONINFORMATION|MB_OK "$(^Name) 已成功地从你的计算机移除。"
FunctionEnd

Function un.onInit
  MessageBox MB_ICONQUESTION|MB_YESNO|MB_DEFBUTTON2 "你确实要完全移除 $(^Name) ，其及所有的组件？" IDYES +2
  Abort
  Call un.ExitDoubanFMWarnning
  Var /GLOBAL NoDeleteUserData
  StrCpy $NoDeleteUserData 1
  MessageBox MB_ICONQUESTION|MB_YESNO|MB_DEFBUTTON2 "是否删除用户数据？" IDNO +2
  StrCpy $NoDeleteUserData 0
FunctionEnd

Section Uninstall
  IntCmp $NoDeleteUserData 1 DeleteExe
  ${Locate} "$APPDATA\K.F.Storm\豆瓣电台" "/L=F" "un.DeleteFile"
  RMDir /r "$APPDATA\K.F.Storm\豆瓣电台"
DeleteExe:
  RMDir /r "$INSTDIR"

  Delete "$DESKTOP\豆瓣电台.lnk"
  RMDir /r "$SMPROGRAMS\豆瓣电台"
  
  DeleteRegKey ${PRODUCT_UNINST_ROOT_KEY} "${PRODUCT_UNINST_KEY}"
  DeleteRegKey HKLM "${PRODUCT_DIR_REGKEY}"
  SetAutoClose true
SectionEnd

Function un.DeleteFile
	Delete "$R9"
	Push $0
FunctionEnd
