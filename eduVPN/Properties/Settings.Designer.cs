﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace eduVPN.Properties {
    
    
    [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Microsoft.VisualStudio.Editors.SettingsDesigner.SettingsSingleFileGenerator", "15.1.0.0")]
    public sealed partial class Settings : global::System.Configuration.ApplicationSettingsBase {
        
        private static Settings defaultInstance = ((Settings)(global::System.Configuration.ApplicationSettingsBase.Synchronized(new Settings())));
        
        public static Settings Default {
            get {
                return defaultInstance;
            }
        }
        
        [global::System.Configuration.ApplicationScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.SpecialSettingAttribute(global::System.Configuration.SpecialSetting.WebServiceUrl)]
        [global::System.Configuration.DefaultSettingValueAttribute("https://static.eduvpn.nl/disco/institute_access.json")]
        public string InstituteAccessDiscovery {
            get {
                return ((string)(this["InstituteAccessDiscovery"]));
            }
        }
        
        [global::System.Configuration.ApplicationScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("E5On0JTtyUVZmcWd+I/FXRm32nSq8R2ioyW7dcu/U88=")]
        public string InstituteAccessDiscoveryPubKey {
            get {
                return ((string)(this["InstituteAccessDiscoveryPubKey"]));
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        public global::eduVPN.JSON.Response InstituteAccessDiscoveryCache {
            get {
                return ((global::eduVPN.JSON.Response)(this["InstituteAccessDiscoveryCache"]));
            }
            set {
                this["InstituteAccessDiscoveryCache"] = value;
            }
        }
        
        [global::System.Configuration.ApplicationScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.SpecialSettingAttribute(global::System.Configuration.SpecialSetting.WebServiceUrl)]
        [global::System.Configuration.DefaultSettingValueAttribute("https://static.eduvpn.nl/disco/secure_internet.json")]
        public string SecureInternetDiscovery {
            get {
                return ((string)(this["SecureInternetDiscovery"]));
            }
        }
        
        [global::System.Configuration.ApplicationScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("E5On0JTtyUVZmcWd+I/FXRm32nSq8R2ioyW7dcu/U88=")]
        public string SecureInternetDiscoveryPubKey {
            get {
                return ((string)(this["SecureInternetDiscoveryPubKey"]));
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        public global::eduVPN.JSON.Response SecureInternetDiscoveryCache {
            get {
                return ((global::eduVPN.JSON.Response)(this["SecureInternetDiscoveryCache"]));
            }
            set {
                this["SecureInternetDiscoveryCache"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("00000000-0000-0000-0000-000000000000")]
        public global::System.Guid OpenVPNInterfaceID {
            get {
                return ((global::System.Guid)(this["OpenVPNInterfaceID"]));
            }
            set {
                this["OpenVPNInterfaceID"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("0")]
        public int SettingsVersion {
            get {
                return ((int)(this["SettingsVersion"]));
            }
            set {
                this["SettingsVersion"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("<SerializableStringDictionary/>")]
        public global::eduVPN.SerializableStringDictionary AccessTokens {
            get {
                return ((global::eduVPN.SerializableStringDictionary)(this["AccessTokens"]));
            }
            set {
                this["AccessTokens"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("<InstanceSettingsDictionary/>")]
        public global::eduVPN.Models.InstanceSettingsDictionary InstanceSettings {
            get {
                return ((global::eduVPN.Models.InstanceSettingsDictionary)(this["InstanceSettings"]));
            }
            set {
                this["InstanceSettings"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("<VPNConfigurationSettingsList/>")]
        public global::eduVPN.Models.VPNConfigurationSettingsList InstituteAccessConfigHistory {
            get {
                return ((global::eduVPN.Models.VPNConfigurationSettingsList)(this["InstituteAccessConfigHistory"]));
            }
            set {
                this["InstituteAccessConfigHistory"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("<VPNConfigurationSettingsList/>")]
        public global::eduVPN.Models.VPNConfigurationSettingsList SecureInternetConfigHistory {
            get {
                return ((global::eduVPN.Models.VPNConfigurationSettingsList)(this["SecureInternetConfigHistory"]));
            }
            set {
                this["SecureInternetConfigHistory"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("False")]
        public bool OpenVPNForceTCP {
            get {
                return ((bool)(this["OpenVPNForceTCP"]));
            }
            set {
                this["OpenVPNForceTCP"] = value;
            }
        }
        
        [global::System.Configuration.ApplicationScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("OpenVPNServiceInteractive$eduVPN\\service")]
        public string OpenVPNInteractiveServiceNamedPipe {
            get {
                return ((string)(this["OpenVPNInteractiveServiceNamedPipe"]));
            }
        }
    }
}
