# QWest
QWest is a platform for sharing once's travel experiences

## External programs needed
### node.js
node.js 13 needs to be installed, use can use this command to check your version
```
node -v
```
### .NET
you need the dotnet framework installed, including asp&#46;net framework
## Setup
### git
Start by git cloning the project
```
git cloning https://github.com/cramt/QWest.git
```
### npm
npm packages needs to be installed, the node project is QWest&#46;Web so you must open a console there and
```
npm i
```
### nuget
nuget packages can be installed with
```
dotnet restore
```
however this for some reason doesnt work sometimes
## Execution
For executing you need to run the QWest.Api project, this automatically starts the QWest&#46;Web project that serves the webpackes and proxies anything on the /api/ to QWest.Api

QWest.Api runs on port 9000

QWest&#46;Web runs on port 8080

### Development
## Git pull hook
A git pull hook is a bash program that will be executed when you git pull, this one automatically installs possible new npm package
```sh
#!/usr/bin/env bash
# MIT © Sindre Sorhus - sindresorhus.com

# git hook to run a command after `git pull` if a specified file was changed
# Run `chmod +x post-merge` to make it executable then put it into `.git/hooks/`.

changed_files="$(git diff-tree -r --name-only --no-commit-id ORIG_HEAD HEAD)"

check_run() {
	echo "$changed_files" | grep --quiet "$1" && eval "$2"
}

# Example usage
# In this example it's used to run `npm install` if package.json changed
cd QWest.Web
check_run package.json "npm install"
```
put this in .git/hooks/post-merge.

(this also works on windows, since git uses Git Bash which comes preinstalled with all windows git installations)
