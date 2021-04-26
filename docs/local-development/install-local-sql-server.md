# Installing a local SQL-server

As a developer, you can install a local SQL Server instance to use in your development.

If you install the SQL Server manually you will be able to pick and choice from a number of settings, meaning that each developer might use the SQL server with very different settings.

This guide will enable you to install a SQL Server instance silently with the settings shared between developers working on the Green Energy Hub.

Installing a SQL Server instance will not add any databases. This will need to be done afterwards.

## Installing SQL Server 2019 Developer Edition

### Download the installer

Download the newest installer for the SQL Server 2019.

The download page can be found [here](https://www.microsoft.com/en-us/sql-server/sql-server-downloads)

Click the "Download now" button under the developer edition.

### Using the installer to download the setup files

* Run the downloaded file
* Accept that the file may cause changes to your PC
* Click the custom button
* Click install (without changing any options)
* Close the window for installing

### Prepare configuration

* Copy the file `ConfigurationFile.ini.SQL2019Developer.samples` (from the `sql-server` sub-folder next to this) to `C:\SQL2019\Developer_ENU`
* Rename the file to `ConfigurationFile.ini`
* Find the line containing the setting for `SQLSYSADMINACCOUNTS`
* Edit the line to use your windows account on the device you are installing it on. An example can be seen below:

```Prompt
; Windows account(s) to provision as SQL Server system administrators. 

SQLSYSADMINACCOUNTS="ENERGINET\xmdje-admin"
```

If you are unsure of which account you are using you can run the following in a prompt:

```Prompt
whoami
```

* Save the file

### Installing the SQL server

* Start a prompt
* Navigate to the `C:\SQL2019\Developer_ENU` folder
* Run the following command

```Prompt
SETUP.EXE /ConfigurationFile=ConfigurationFile.ini
```

After a bit of time your SQL server instance should now be up and running.

### Installing SQL Server Management Studio

To view and work without SQL instance from outside your code, you can install the SQL Server Management Studio.

It can be found [here](https://aka.ms/ssmsfullsetup).

## Installation other versions of the SQL Server

Each version of the SQL server may vary and may possible need a new configuration template to be created.

A guide for creating a new template can be found [here](./sql-server/creating-a-new-configuration-template.md)

## Adding your first database

After installation of the SQL server instance it will be completely empty.

You can create a new empty database by using the following script in the SQL Server Management Studio (where master data is replaced by the name of the database you need)

```SQL
CREATE DATABASE [MasterData]
```

## I have already installed SQL Server

If you already installed the SQL Server and wishes to use this method instead, follow these steps:

* In Add/Remove Programs, uninstall your current SQL Server
* Restart Windows
* Follow the installation process described above
