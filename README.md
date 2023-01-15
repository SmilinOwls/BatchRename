<h1 align="center">BATCH RENAME 👋</h1>
<p>
  <img alt="Version" src="https://img.shields.io/badge/version-v1.0.0-blue.svg?cacheSeconds=2592000" />
  <a href="https://github.com/SmilinOwls/BatchRename#readme" target="_blank">
    <img alt="Documentation" src="https://img.shields.io/badge/documentation-yes-brightgreen.svg" />
  </a>
  <a href="https://github.com/SmilinOwls/BatchRename/blob/master/LICENSE" target="_blank">
    <img alt="License: MIT" src="https://img.shields.io/badge/license-MIT-green" />
  </a>
  <a href="https://visualstudio.microsoft.com/downloads/">
    <img alth="IDE: Visual Studio Code 2022" src = "https://img.shields.io/badge/IDE-VS%20Code%202022-ff69b4"/>  
  </a>
  <a href="https://dotnet.microsoft.com/en-us/download/dotnet/6.0">
    <img alt=".NET version" src="https://img.shields.io/badge/.NET-6.0-red" />
  </a>
  <img alt="Git Followers" src ="https://img.shields.io/github/followers/SmilinOwls?style=social"/>
</p>

> Project Windows's 01 - Detailed Instruction To Problem: [Batch Rename](https://tdquang7.notion.site/Project-batch-rename-2022-9dc9eb9c9d674dbdb4a988a3794d1335) 
 
> *See Solution at [Github Project Here](https://github.com/SmilinOwls/BatchRename)*

### 🏠 [Homepage](https://github.com/SmilinOwls/BatchRename#readme)

## ✨ Objectives
   ### Main Target 
   - Create an application for renaming multiple files.
   ### Techniques
   - Design patterns: `Singleton`, `Factory`, `Abstract factory`, `prototype`
   - Plugin architecture
   - Delegate & event
   
## Prerequisites

- Visual Studio Code 2022 or latest versions
- Build, Compile & Run on platform `.NET 6.0`

## Install

- Open Terminal (MacOS) or CMD Shell (Windows), then go on command line described below:
```sh
# Change your direction path to Desktop/ 
cd Desktop/
# Fork the project, clone this fork in a repo called BatchRename
git clone https://github.com/SmilinOwls/BatchRename/ BatchRename
# Navigate to the newly cloned repo
cd BatchRename/
```

## Usage

- Open file `.exe` in *Release* folder to run all functions to check wether all requirements are completely done
```sh
cd BatchRename/Release
open BatchRename.exe
```
## Adjustment

- If you want to extend the rules of renaming, open `BatchRename.sln` and add a new `WPF class library` project. Remember to copy TWO following blocks into `*.csproj`:
  - Reference to the interface: 
    ```
    <ItemGroup>
      <ProjectReference Include="..\Core\Core.csproj" />
    </ItemGroup>
    ```
  - Create a post-build event in order to automatically copy file `*.dll` of each of renaming rules generated from the *Build Process* into a folder named `Rules` which contains all `*.dll` needed for renaming```, so you do not have to manually copy file `*.dll` one by one into target path. Just by declaring the code bellow will help you a lot:
    ```
      <Target Name="PostBuild" AfterTargets="PostBuildEvent">
        <Exec Command="xcopy /y $(ProjectDir)$(OutDir)*.dll $(SolutionDir)\BatchRename\Rules\" />
      </Target>
    ```
  
## Core requirements

1. Dynamically load all renaming rules from external DLL files
2. Select all files and folders you want to rename
3. Create a set of rules for renaming the files. 
    1. Each rule can be added from a menu 
    2. Each rule's parameters can be edited 
4. Apply the set of rules in numerical order to each file, make them have a new name
5. Save this set of rules into presets for quickly loading later if you need to reuse

## Renaming rules

1. Change the extension to another extension (no conversion, force renaming extension)
2. Add counter to the end of the file
   - Could specify the start value, steps, number of digits (Could have padding like 01, 02, 03...10....99)
3. Remove all space from the beginning and the ending of the filename
4. Replace certain characters into one character like replacing "-" ad "_" into space " "
   - Could be the other way like replace all space " " into dot "."
5. Adding a prefix to all the files
6. Adding a suffix to all the files
7. Convert all characters to lowercase, remove all spaces
8. Convert filename to PascalCase

## Improvements

1. Drag & Drop a file to add to the list
2. Storing parameters for renaming using XML file / JSON / excel / database
3. Adding recursively: just specify a folder only, the application will automatically scan and add all the files inside
4. Handling duplication
5. Last time state: When exiting the application, auto-save and load the 
    1. The current size of the screen
    2. Current position of the screen
    3. Last chosen preset
6. Autosave & load the current working condition to prevent sudden power loss
    1. The current file list
    2. The current set of renaming rules, together with the parameters
7. Using regular expressions
8. Checking exceptions when editing rules: like characters that cannot be in the file name, the maximum length of the filename cannot exceed 255 characters
9. Save and load your work into a project.
10. Let the user see the preview of the result
11. Create a copy of all the files and move them to a selected folder rather than perform the renaming on the original file
12. Filter + Search the file or folder 
13. Strong Responsive UI to ensure the WPF UI has not broke down
