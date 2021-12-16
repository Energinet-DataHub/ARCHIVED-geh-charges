# Run integration tests locally

As a developer, you can run all unit tests and integration tests locally.

Integration tests require access to Azure and some local resources and setup.

## Installing prerequisites

### Install NVM for Windows

* Download latest version of [nvm-setup.zip](https://github.com/coreybutler/nvm-windows/releases)
* Install as [documented here](https://github.com/coreybutler/nvm-windows)

### Install Node and NPM using NVM

If you have any version of Node installed, you should uninstall it before you proceed.

Run Command Prompt as Administrator and run the following:

```Prompt
nvm install lts
nvm use 14.18.1
```

Afterwards you can check your installation and versions

```Prompt
nvm -v
node -v
npm -v
```

### Install Azurite and Azure Functions Core Tools

```Prompt
npm install -g azurite@3.14.0
npm install -g azure-functions-core-tools@3.0.3568
```

### Set up local settings

You will need to make a local copy of all `local.settings.json.sample` and `integrationtest.local.settings.json.sample`
files and fill in your own settings.
