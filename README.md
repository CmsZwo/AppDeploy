# AppDeploy
Simple console based tool for deploying Applications

```
Written in C# with .netcore 2.1
```

## Main menu
```
Choose project:

[esc] to quit


1. Project 1
2. Project 2
```

## Project menu
```
Choose batch to process:

[esc] to quit
[a] apply current state
[r] reset state

1. live
2. dev
```

AppDeploy helps you uploading your project to servers. 

## Currently supported protocols
- FTP

## Filestructure
AppDeploy looks for `.deployment` files in two ways. 

### Traverses up the working directory

Finding the first available `.deployment` file. This file should hold common 
platform specific configuration for exclude and also shared credentials to 
servers which is not project specific.

### Traverses down the working directory

Finding all available `.deployment` files in subfolders. These files should 
contain project specific configuration.

If you select a project the common and the project specific `.deployment` file 
will be dynamicalle merged. 

> Having the common `.deployment` is not required. You can just only use a project specific `.deployment` file.

### Example
```
/my-projects

  - project-1
    - ... project files
  - .deployment

  - project-2
    - ... project files
  - .deployment

  - ... more projects

  - .deployment
```


## Configuration
Configuration is stored in `.deployment` files. Files are text formatted. 
`[Tabulator] characters are impotant.`
Keep in mind: passwords are stored in `clear text` for now. 
That could change in future.

### exclude
Sets filters for files or folders to be excluded. Wildcards are allowed here.

```
exclude: webproject
	_notes
	.git, .*

	bin/*.pdb, bin/*.xml, bin/*.config

	*.csproj, *.cs, *.user
	Connected Services/, Properties/, obj/
```

### pick
Sets files or folders (recursively) to be included. Ommits exclude filters. 
No wildcards allowed here.

```
pick: my
	file
	directory/
```

# FTP Deployment
Running a deployment to ftp will only upload files 
that were modified or created since last time.

## Configuration

### ftp
Sets information for connecting to your ftp server.

```
ftp: label-for-your-server
	host: ftp.example.com
	port: 21
	user: user
	password: password
```

### target
Defines a single batch which can be executed.

```
target: project
	ftp: server1, server2
	exclude: webproject, crap
	pick: my
	directory: user/
	ok: https://my-domain/ready
```

- ftp: You can specify multiple ftp configurations. Files will be uploaded to all servers.
- exclude: You can specify multiple exclude filters.
- pick: You can specify multiple pick lists.
- directory: You can specify a sub directory on you ftp.
- ok: After each server this url will be called. AppDeploy waits until Url return HTTP 200. 

ok is useful if you deploy a load balanced scenario.

### batch
Definies a batch of multiple targets to be executed on after another.

```
batch: dev
	target: project
	target: project-test
```

## Special Commands

### Apply current state
Will write `.lastrun` file to remote root folder. Information will state that all files are up to date. Running a profile will not upload any files until next change.

### Reset state
Will delete `.lastrun` file from remote root folder. Running profile will cause full upload.
