#
#   eduVPN - End-user friendly VPN
#
#   Copyright: 2017, The Commons Conservancy eduVPN Programme
#   SPDX-License-Identifier: GPL-3.0+
#

PRODUCT_NAME=eduVPN
OUTPUT_DIR=bin

!IF "$(PROCESSOR_ARCHITECTURE)" == "AMD64"
PLAT=x64
REG_FLAGS=/f /reg:64
REG_FLAGS32=/f /reg:32
!ELSE
PLAT=x86
REG_FLAGS=/f
!ENDIF

All ::

Clean ::
	-devenv.com "eduVPN.sln" /clean "Release|x86"
	-devenv.com "eduVPN.sln" /clean "Debug|x86"
	-devenv.com "eduVPN.sln" /clean "Release|x64"
	-devenv.com "eduVPN.sln" /clean "Debug|x64"
	-devenv.com "eduVPN.sln" /clean "Release|AnyCPU"
	-devenv.com "eduVPN.sln" /clean "Debug|AnyCPU"


######################################################################
# Default target
######################################################################

All ::


######################################################################
# Registration
######################################################################

Register :: \
	RegisterSettings \
	RegisterShortcuts

Unregister :: \
	UnregisterShortcuts \
	UnregisterSettings

RegisterSettings :: \
	"$(OUTPUT_DIR)\Debug\$(PLAT)\eduVPN.Client.exe"
	reg.exe add "HKCR\org.eduvpn.app"                    /v "URL Protocol" /t REG_SZ /d ""                                                                    $(REG_FLAGS) > NUL
	reg.exe add "HKCR\org.eduvpn.app\DefaultIcon"        /ve               /t REG_SZ /d "$(MAKEDIR)\$(OUTPUT_DIR)\Debug\$(PLAT)\eduVPN.Client.exe,1"          $(REG_FLAGS) > NUL
	reg.exe add "HKCR\org.eduvpn.app\shell\open\command" /ve               /t REG_SZ /d "\"$(MAKEDIR)\$(OUTPUT_DIR)\Debug\$(PLAT)\eduVPN.Client.exe\" \"%1\"" $(REG_FLAGS) > NUL

UnregisterSettings ::
	-reg.exe delete "HKCR\org.eduvpn.app" $(REG_FLAGS) > NUL

RegisterShortcuts :: \
	"$(PROGRAMDATA)\Microsoft\Windows\Start Menu\Programs\$(PRODUCT_NAME) Client.lnk"

UnregisterShortcuts ::
	-if exist "$(PROGRAMDATA)\Microsoft\Windows\Start Menu\Programs\$(PRODUCT_NAME)" rd /s /q "$(PROGRAMDATA)\Microsoft\Windows\Start Menu\Programs\$(PRODUCT_NAME)"


######################################################################
# Shortcut creation
######################################################################

"$(PROGRAMDATA)\Microsoft\Windows\Start Menu\Programs\$(PRODUCT_NAME) Client.lnk" : "$(OUTPUT_DIR)\Debug\$(PLAT)\eduVPN.Client.exe"
	cscript.exe "bin\MkLnk.wsf" //Nologo $@ "$(MAKEDIR)\$(OUTPUT_DIR)\Debug\$(PLAT)\eduVPN.Client.exe"


######################################################################
# Building
######################################################################

"$(OUTPUT_DIR)\Debug\x86\eduVPN.Client.exe" ::
	devenv.com "eduVPN.sln" /build "Debug|x86"

"$(OUTPUT_DIR)\Debug\x64\eduVPN.Client.exe" ::
	devenv.com "eduVPN.sln" /build "Debug|x64"

"$(OUTPUT_DIR)\Release\x86\eduVPN.Client.exe" ::
	devenv.com "eduVPN.sln" /build "Release|x86"

"$(OUTPUT_DIR)\Release\x64\eduVPN.Client.exe" ::
	devenv.com "eduVPN.sln" /build "Release|x64"
