# EmoKill

Emotet process killing tool for Windows OS.  
EmoKill is based on [EmoCheck of JPCERT/CC](https://github.com/JPCERTCC/EmoCheck), thanks for it.

## How to use

1. Download EmoKill.zip from the Releases page.
2. Unpack the downloaded zip-file on your host.
3. Install EmoKill as a windows service (Easiest way is to use `EmoKillConsole.exe`).

## Download

Please download from the [Releases](https://github.com/ZiMADE/EmoKill/releases) page.

Latest hash:  

> EmoKill.exe  
>   MD5   : 9865385CFFDA2F6956DBEEA406E6DD12  
>   SHA1  : CA9494B8A98F302CD88575DFBB1421EBE6149594  
>   SHA256: 600CFF67CC6C67ED55E77CE560740459A77345412C38845B20853A4E410F4AE1  

> EmoKillConsole.exe  
>   MD5   : BF31F7D4BB6B5FEDD10EC9B6230E2A0B  
>   SHA1  : 30A236F79EA3135626CFB4D422A7494B9AA31B8D  
>   SHA256: 18B7483498B4F561B90A55FB74C073B42FB08B12E40B44183C084E2A6ADD4125  

> EmoKillTest.exe  
>   MD5   : E508926C7A59E6D21641F45410E0A123  
>   SHA1  : 337576963EE7868819A50A9B22BAAEFC8D085A98  
>   SHA256: E3D979E0D1911736DB2634FF229557388DA251FCAD6C77F64C1037191617B7D3  

## How EmoKill works

The package of EmoKill consists of 3 program files: 
#### EmoKill.exe
- This is the main part of EmoKill, it runs as a windows service and contains the whole logic to detect and kill processes of Emotet.  
#### EmoKillConsole.exe
- EmoKillConsole has a small menu that helps even inexperienced users to start.
#### EmoKillTest.exe
- To see EmoKill in action without having an Emotet infection, this exe will be helpfull.

When EmoKill starts, there will be made a check of all running processes on your machine. After that every new started process will also be checked by EmoKill. Each process which match the pattern of Emotet based on the logic from [EmoCheck v0.0.2 of JPCERT/CC](https://github.com/JPCERTCC/EmoCheck) will be killed as soon as possible. The time between detection and successful killing such a process is normaly less than 1 second.

Detection and killing of Emotet processes are logged to a logfile `%windir%\Temp\EmoKill.log` and also to the application eventlog.

After a reboot of your machine the installed EmoKill service will start again automaticly.

## Installation

1. Start `EmoKillConsole.exe` (run as amdinistrator is required)
2. Select menuitem 1 (Install and start EmoKill as Service)

Selecting the installation of the EmoKill as a windows service will do follwoing: 
1. program files are copied to the program files folder on your systemdrive
2. after that the service will be installed with installutils.exe of the .NET-Framework
3. and than the EmoKill service will be startet

That's all, EmoKill should be ready to detect and kill processes of Emotet.

## Testing

You may use `EmoKillTest.exe` to check and see how is EmoKill working on your machine. 

#### Scenario 1: 
1. Stop EmoKill service if the service is running (i.e. menuitem 4 of `EmoKillConsole.exe`)
2. Start `EmoKillTest.exe` one or more times
3. Start EmoKill service (i.e. menuitem 3 of `EmoKillConsole.exe`)
4. All startet instances of `EmoKillTest.exe` should be killed in a few seconds

#### Scenario 2: 
1. Start the EmoKill service if the service is not running (i.e. menuitem 3 of `EmoKillConsole.exe`)
2. Start `EmoKillTest.exe` one or more times
3. All startet instances of `EmoKillTest.exe` should be killed in a few seconds

After Testing have a look to the application eventlog and/or to the logfile (i.e. menuitem 6 of `EmoKillConsole.exe`).

## Why EmoKill needs Administrator Privileges?

EmoKill can only work correctly if the program runs with administrator privileges. This is necessary because  EmoKill will be informed by the operating system about the creation of new windows processes. Without Administrator Privileges this is not possible! 

## Can you trust EmoKill? YES!!!

Why should you trust EmoKill?
- EmoKill does only watch for windows processes and kills processes which match the pattern of Emotet as soon as possible!
- EmoKill is open source!
  - many developers can check the code of EmoKill
  - it's also possible for developers to decompile all components of EmoKill
- EmoKill runs only on the machine where it is installed!
- EmoKill needs no network connection!
- EmoKill needs no connection to the internet!
- EmoKill contains no ads, no malware, ..., and no third party components!

## How EmoKill detects Emotet

(v1.0.7348.16624)  

Detection of Emotet is a port of the C++ code from [EmoCheck v0.0.2 of JPCERT/CC](https://github.com/JPCERTCC/EmoCheck), thanks for it.

## Screenshot

(v1.0.7348.16624)  
<div align="left"><img src="./img/EmoKillConsole.png"></div>
<div align="left"><img src="./img/EmoKillTest.png"></div>

## Releases

- (Feb. 13, 2020) v1.0.7348.16624
  - Initial release

### Tested environments

- Windows 10 1909 64bit German/English Edition
- Windows 10 1903 64bit German/English Edition
- Windows 10 1607 Enterprise 206 LTSB 64bit German/English Edition
- Windows 7 SP1 32bit German/English Edition
- Windows 7 SP1 64bit German/English Edition

### Build

- Windows 10 1909 64bit German Edition
- Microsoft Visual Studio Community 2017
