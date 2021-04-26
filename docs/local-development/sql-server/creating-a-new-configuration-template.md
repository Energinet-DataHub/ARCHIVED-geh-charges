# Creating a new SQL server configuration template

This guide will describe how to add a new configuration template for silently installing a SQL-server instance.

## Concept

To make sure that all developers share the same settings when developing locally, we want to add a configuration file to use in the installation so that the individual develop does not have to set the settings during the installation.

Another approach is to use command line parameters, but for convenience, it is easier for the developer to use the installation configuration file.

## Building a new configuration template

First, download the installation file for the SQL server you want to add configuration for.

Then, run the installation, specifying all the settings you want along the way.

Stop before running the step "Ready to install". In this window the path to the configuration used to run your chosen installation will be shown. Copy that file. It will serve as the template.

Close the installer without proceeding.

### Making sure all options are displayed

The amount of windows shown in the installer may vary depending on the version downloaded.

If you want to make sure all information is displayed, then use the following command:

```Prompt
<name of file> /ACTION=INSTALL /UIMODE=Normal 
```

Note that the installer above might just be downloading the actual setup files and then call them afterwards. The above command is for the setup files and not the application downloading it.

## Changing the template

We now have to change the template copied in the previous step slightly.

The following lines must be added to the configuration:

```Ini
IACCEPTSQLSERVERLICENSETERMS="True"
```

A silent installation will fail without it.

When we need to change to following parameter:

```Ini
QUIET="True"
```

This will make sure the installer is actually running silently.

Finally, we need to comment out or remove the following line:

```Ini
UIMODE="Normal"
```

Otherwise dialogues will still be displayed to the user.

## Placing the template

Lastly, add the template to this folder, named appropriately, so that others can enjoy your work.

## Running the installation with the template

To actually run the installation using your new template, use the following command:

```Prompt
.setup.exe /ConfigurationFile=<YourConfigurationFile.ini name>
```
