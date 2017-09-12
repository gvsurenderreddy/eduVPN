#
#   eduVPN - End-user friendly VPN
#
#   Copyright: 2017, The Commons Conservancy eduVPN Programme
#   SPDX-License-Identifier: GPL-3.0+
#

PRODUCT_NAME=eduVPN Client
OUTPUT_DIR=bin
SETUP_DIR=$(OUTPUT_DIR)/Setup

# Default testing configuration and platform
CFG=Debug
!IF "$(PROCESSOR_ARCHITECTURE)" == "AMD64"
PLAT=x64
!ELSE
PLAT=x86
!ENDIF

# Utility default flags
!IF "$(PROCESSOR_ARCHITECTURE)" == "AMD64"
REG_FLAGS=/f /reg:64
REG_FLAGS32=/f /reg:32
!ELSE
REG_FLAGS=/f
!ENDIF
DEVENV_FLAGS=/NoLogo
CSCRIPT_FLAGS=//Nologo

######################################################################
# Default target
######################################################################

All :: \
	Setup


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
	"$(OUTPUT_DIR)\$(CFG)\$(PLAT)\eduVPN.Client.exe"
	reg.exe add "HKCR\org.eduvpn.app"                    /v "URL Protocol" /t REG_SZ /d ""                                                                     $(REG_FLAGS) > NUL
	reg.exe add "HKCR\org.eduvpn.app\DefaultIcon"        /ve               /t REG_SZ /d "$(MAKEDIR)\$(OUTPUT_DIR)\$(CFG)\$(PLAT)\eduVPN.Client.exe,1"          $(REG_FLAGS) > NUL
	reg.exe add "HKCR\org.eduvpn.app\shell\open\command" /ve               /t REG_SZ /d "\"$(MAKEDIR)\$(OUTPUT_DIR)\$(CFG)\$(PLAT)\eduVPN.Client.exe\" \"%1\"" $(REG_FLAGS) > NUL

UnregisterSettings ::
	-reg.exe delete "HKCR\org.eduvpn.app" $(REG_FLAGS) > NUL

RegisterShortcuts :: \
	"$(PROGRAMDATA)\Microsoft\Windows\Start Menu\Programs\$(PRODUCT_NAME).lnk"

UnregisterShortcuts ::
	-if exist "$(PROGRAMDATA)\Microsoft\Windows\Start Menu\Programs\$(PRODUCT_NAME).lnk" rd /s /q "$(PROGRAMDATA)\Microsoft\Windows\Start Menu\Programs\$(PRODUCT_NAME).lnk"


######################################################################
# Setup
######################################################################

Setup :: \
	SetupBuild

SetupBuild ::
	devenv.com "eduVPN.sln" /Build "Release|x86" $(DEVENV_FLAGS)
	devenv.com "eduVPN.sln" /Build "Release|x64" $(DEVENV_FLAGS)


######################################################################
# Shortcut creation
######################################################################

"$(PROGRAMDATA)\Microsoft\Windows\Start Menu\Programs\$(PRODUCT_NAME).lnk" : "$(OUTPUT_DIR)\$(CFG)\$(PLAT)\eduVPN.Client.exe"
	cscript.exe "bin\MkLnk.wsf" //Nologo $@ "$(MAKEDIR)\$(OUTPUT_DIR)\$(CFG)\$(PLAT)\eduVPN.Client.exe"


######################################################################
# Configuration and platform specific rules
######################################################################

CFG=Debug
PLAT=x86
!INCLUDE "Makefile-Build.mak"
PLAT=x64
!INCLUDE "Makefile-Build.mak"

CFG=Release
PLAT=x86
!INCLUDE "Makefile-Build.mak"
PLAT=x64
!INCLUDE "Makefile-Build.mak"
