# LostMyMisoSoup
Stable &amp; Effective Deobfuscator for MindLated , i don't even know how i've pulled it off

It can also support slight mods of <a href="https://github.com/Sato-Isolated/MindLated/">MindLated</a> (see [Presets](#presets))

## Credits
- <a href="https://github.com/Sato-Isolated/Sato-Isolated">Sato-Isolated</a> - <a href="https://github.com/Sato-Isolated/MindLated/">MindLated</a>
- 0xd4d (now wtfsck) - <a href="https://github.com/0xd4d/dnlib">dnlib</a>
# Supported Protections
Note: ***All Supported Protections work perfectly alone***

Protection Name | Is supported | Stability Status
------------- | :---: | ---- |
AntiDe4Dot | Yes | Perfect
AntiDebug | Yes | Perfect
AntiDump | Yes | Perfect
AntiTamper | Yes | Perfect
Calli | Yes | Perfect
InvalidMD | Yes | Perfect
Jump Control Flow | Yes | Perfect
Jump Control Flow Watermark | Yes | Perfect
Online String Encryption | Yes | Perfect
Proxy Constants | Yes | Perfect
Proxy Strings | Yes | Perfect
String Encryption | Yes | Perfect
StackUnfConfusion | Yes | Perfect
| | |
Int Confusion | Yes | Near Perfect, can crash when bundled with Arithmatic
Local2Field v1 | Yes | Near Perfect, can crash when bundled with Int Confusion
Local2Field v2 | Yes | Near Perfect, can crash when bundled with Int Confusion
| | |
Arithmatic | Yes | Poor, can easily crash
| | |
Control Flow | No | Use <a href="https://github.com/miso-xyz/CCFlow">CCFlow</a> (requires some manual work afterwards)
Renamer | No | Original names cannot be recovered
Proxy Methods | No | Doesn't seem to be finished
Resource Protections | No | Theres no code other than a checkbox
<a name="presets"></a>
## Presets
Presets allows you to customise what `LostMyMisoSoup` is searching for.</br>
This can be used to make `LostMyMisoSoup` support slight modifications of <a href="https://github.com/Sato-Isolated/MindLated/">MindLated</a>.
<details>
            <summary>Default Preset</summary>
            <hr>
            <pre>// this file can be edited if the target application uses a modified version of MindLated

[AntiTamper]
AntiTamper_Throw	= System,BadImageFormatException
[AntiDump]
AntiDump_Marshal	= System.Runtime.InteropServices,Marshal,GetHINSTANCE
[AntiDebug]
AntiDebug_OSPlatform	= System,OperatingSystem,get_Platform
AntiDebug_EnvExit	= System,Environment,Exit
AntiDebug_EnvVar	= System,Environment,GetEnvironmentVariable
AntiDebug_EnvOSVer	= System,Environment,get_OSVersion
AntiDebug_DebugHooked	= System.Diagnostics,Debugger,get_IsAttached
AntiDebug_DebugLog	= System.Diagnostics,Debugger,IsLogging
[JumpCFlow]
JCF_Watermark		= MindLated.jpg
[IntConfusion & Arithmatic]
IntConf_Default		= 1.5707963267949
Math_Class		= System,Math
Math_Truncate		= System,Math,Truncate
Math_Abs		= System,Math,Abs
Math_Cos		= System,Math,Cos
Math_Sin		= System,Math,Sin
Math_Log		= System,Math,Log
Math_Log10		= System,Math,Log10
Math_Floor		= System,Math,Floor
Math_Round		= System,Math,Round
Math_Tan		= System,Math,Tan
Math_Tanh		= System,Math,Tanh
Math_Sqrt		= System,Math,Sqrt
Math_Ceiling		= System,Math,Ceiling
[Proxy]
Proxy_CommonName	= ProxyMeth
[Strings]
EncString_ResourceName	= MindLated.zero
EncString_PasswordHash	= p7K95451qB88sZ7J
EncString_KeyAlgorithm	= System.Security.Cryptography,Rfc2898DeriveBytes
EncString_AESAlgorithm	= System.Security.Cryptography,RijndaelManaged
EncString_SymmetricAlg 	= System.Security.Cryptography,SymmetricAlgorithm
// Start of Base64 (you will have to replace the padding by '#')
EncString_Salt		= MkdNMjNqMzAxdDYwWjk2VA##
EncString_VI		= SXpUZGhHNlM4dXdnMTQxUw##
// End of Base64</pre>
<hr>
         </details>
         
## General Configuration File (config.txt)
The config file allows you to:
 - Choose which protection to clean
 - Dump all Randomised Seeds from Int Confusion present in a protected application
 - Set the preset filepath

<details>
           <summary>Default Configuration</summary>
           <pre>// Unsupported Protections:
// ProxyMethods 		- MindLated's ProxyMethod isn't finished.
// CFlow			- Use "https://github.com/miso-xyz/CCFlow/" to fix it, manual work will however be required after.
// Renamer			- Can be detected, cannot be recovered.
// ResourceEncryption		- MindLated has no code for it, it only has a checkbox which doesn't do anything.

[General]
ForceDefault		= 0
PresetFile		= preset_file.txt
PresetDevMode		= 0
ExportRNGSeeds		= 0
[Antis]
AntiTamper		= 1
AntiDump		= 1
AntiDebug		= 1
AntiDe4Dot		= 1
[CFlow]
Watermark		= 1
JumpCFlow		= 1
[Proxy]
ProxyConstants		= 1
ProxyStrings		= 1
[Math]
IntConfusion		= 1
Arithmectic		= 1
[Encryption]
StringsEncryption	= 1
OnlineStringDecryption	= 1
ResourceEncryption	= 1
[Misc]
StackUnfConfusion	= 1
Calli			= 1
InvalidMD		= 1
Local2Field		= 1
[Cleanup]
InvalidCalls		= 1
UselessJumps		= 1
UselessNOPs		= 1
UnusedLocals		= 1
UnusedVariables		= 1</pre>
         </details>
