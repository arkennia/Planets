
# Planets
A project in Godot 4 to mess with procedural planets.
# Compiling from Source
Due to the nature of the game, Godot should be compiled from source. This is because Godot by default does not use double precision.
## Getting the Godot Source
The source can be found [here](https://github.com/godotengine/godot). 
Clone the repo, and checkout branch 4.7 (the current version).
This information is summarized from the build instructions that can be [found on Godot's website](https://docs.godotengine.org/en/stable/engine_details/development/compiling/introduction_to_the_buildsystem.html).
### Git Commands
The following commands will avoid dealing with merge conflicts:
1. `git checkout -b 4.7`
2. `git fetch origin 4.7`
3. `git reset --hard origin/4.7`
### Other Requirements
Download and install Python
Download and install MSVC build tools
Download and install .NET
### Compile Commands
To compile, you need to install `scons` using python's pip.

#### Prerequisites
Run the following commands inside the source directory from the terminal to install prerequisites:
```
python misc/scripts/install_d3d12_sdk_windows.py
python misc/scripts/install_accesskit.py
python misc/scripts/install_winrt.py
python misc/scripts/install_angle.py
```
#### Custom Build Options
Create `custom.py` inside the source directory and place the following inside it:
```
production =  "yes"
precision =  "double"
module_mono_enabled =  "yes"
```
#### Build Command
You can edit the platform and `-j6` options to suit your hardware and operating system.
`scons platform=windows target=editor profile=custom.py -j6`
After that finishes, run:
` .\bin\godot.windows.editor.double.x86_64.mono.console.exe --headless --generate-mono-glue modules/mono/glue`
You need to create a NugetSource directory for this step. I created it at `C:\Users\<user>\NugetSource`
You can add it to dotnet with the command 
`dotnet nuget add source C:\Users\<user>\NugetSource --name NugetSource       `
Then run:
`python ./modules/mono/build_scripts/build_assemblies.py --godot-output-dir ./bin --push-nupkgs-local C:\Users\jmanl\NugetSource --precision=double`
#### Complete
The Godot binaries should now be in the bin folder and ready for use.

# Getting the Game Source
Clone this repo.
Hard link common into `planets/client/scripts` and `planets/server/scripts`. 
The project should now run.





