# RGB.net Plugin Manager
This project is designed to allow users to uninstall / install RGB.net device providers with a simple UI. It can be customized via JSON config files, and all data is fetched from the JSON feed of your choice.

# Setup
## Package index
To use this plugin manager you need a package index URL. You provide it in the PluginManager.Settings.json file, along with the name of the main EXE of your app.

You can either use my package manager URL, located at `https://rgbsync.com/api/pluginmanager/index.json`, or you can create your own. My package index uses all features and can be used as an example.

The package manager itself has 4 main variables. `Packages` is an array of package objects to be included in the main index. `AdditionalPackageURLs` is an array of additional URLs containing packages to include in the main index. This allows you to embed other packages in a central index for modularity. `Version` is unused at the moment, and should be set to `1.0` for now. If any breaking schema changes are made in the future, this variable will be used to prevent errors caused by incompatible schema versions. `MarketplaceName` is the name you wish to be displayed in the window title and top of the UI. 

## Packages
Packages are the plugins the user can choose to install. They have several variables which allow different configurations. `Name` is the display name of the package. `Image` should be set to a resolvable URL of a .png image which you would like to be displayed along with the name in the package manager window. `Description` is the package description, and can be used to describe compatibility or other info a user should know. `Warning` is a boolean, which if set to true causes an additional confirmation message with a warning. This should be used in a plugin modifies any SMBus registers. If `Warning` is set to true, a `WarningText` must be supplied. This is the text to be displayed in the confirmation dialog.

`TotalFiles` is an integer that should be equal to the number of files total in the package. All 4 file object arrays behave the same, the only difference is where the files are placed. `DPFiles` are placed in a directory named `DeviceProvider`, `x86Files` are placed in a directory named `x86`, `x64Files` will be placed in a directory named `x64Files`, and `RootFiles` are placed in the root directory. Any file arrays that aren't necessary can simply be set to `null`. File arrays are filled with file objects.

## File objects
File objects are used to describe the actual files that make up the package. The array they are in determines the folder they will be placed in. File objects have 2 variables. The first one of these is `LocalFile`, which should be set to the final filename of the file. `RemoteURL` should be set to a resolvable URL containing the binary data for the device provider. 
