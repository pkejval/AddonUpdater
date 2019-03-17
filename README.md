Windows [![Build Status](https://dev.azure.com/kejval/AddonUpdater/_apis/build/status/pkejval.AddonUpdater?branchName=master&jobName=Job&configuration=windows)](https://dev.azure.com/kejval/AddonUpdater/_build/latest?definitionId=1&branchName=master)</br>
Linux [![Build Status](https://dev.azure.com/kejval/AddonUpdater/_apis/build/status/pkejval.AddonUpdater?branchName=master&jobName=Job&configuration=linux)](https://dev.azure.com/kejval/AddonUpdater/_build/latest?definitionId=1&branchName=master)</br>
MAC [![Build Status](https://dev.azure.com/kejval/AddonUpdater/_apis/build/status/pkejval.AddonUpdater?branchName=master&jobName=Job&configuration=mac)](https://dev.azure.com/kejval/AddonUpdater/_build/latest?definitionId=1&branchName=master)</br>

# AddonUpdater
Simple addon updater for World of Warcraft. Do you hate to have Twitch/TukUI clients running in background to update your addons? I do! That's why I created this project - just update my addons and exit! 

Supports most popular addon sites:
* wow.curseforge.com
* www.wowace.com
* wowinterface.com
* tukui.org

## Installation
Download release.zip, extract and setup "config.txt" file.

## Configuration
Open "config.txt" in your favourite text editor and specify path to your World of Warcraft folder.
Go to your favourite addon website and copy URL of addons which should be installed/updated.

Example "config.txt":
```
# Set path to your World of Warcraft installation
WOW_PATH=C:\Program Files (x86)\Battle.NET\World of Warcraft

# Set list of addons URLs - delimited by new line (ENTER)
https://wow.curseforge.com/projects/plater-nameplates
https://wowinterface.com/downloads/info8814-DeadlyBossMods.html
https://www.tukui.org/download.php?ui=elvui
https://www.tukui.org/addons.php?id=38
```

## Startup
You can run console application directly from executable in "interactive mode". Progress will be outputted to console window. Updater stops when finished and shows summary about installed/updated/error numbers and waits for user interaction.
If you want to run application from script (Task Scheduler), just run it with **--script** parameter.

## Limitations
* You can update addons in only one World of Warcraft installation at once. If you need update more, you need copy AddonUpdater to separate folder and set its own config.txt.
* .NET CORE applications can't hide console window even if runnig from script. Expect "black window" blinking when running.
* Application should be multi-platform compatible but for now it's tested and released only for **Windows**.

## Contribution
If you want to contribute either by finding and reporting Issues or just with new idea. Do it please! Pull requests are welcome too!
